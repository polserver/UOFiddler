/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace Ultima.Uop
{
    /// <summary>
    /// Which container a save writes. The same two choices the map sinks already offer, for the file
    /// types that are packed whole rather than streamed block by block.
    /// </summary>
    public enum ContainerFormat
    {
        Mul,
        Uop
    }

    /// <summary>What a save wrote, and anything the user should know about it.</summary>
    public sealed class ClientFileSaveResult
    {
        internal ClientFileSaveResult(string directory, IReadOnlyList<string> filesWritten,
            IReadOnlyList<string> warnings)
        {
            Directory = directory;
            FilesWritten = filesWritten;
            Warnings = warnings;
        }

        /// <summary>Folder the files landed in.</summary>
        public string Directory { get; }

        /// <summary>File names, without their directory.</summary>
        public IReadOnlyList<string> FilesWritten { get; }

        /// <summary>Losses worth telling the user about. Empty on a clean save.</summary>
        public IReadOnlyList<string> Warnings { get; }
    }

    /// <summary>
    /// Saves one of the packable file types as either a mul/idx pair or a uop, from whatever the
    /// in-memory model holds.
    /// </summary>
    /// <remarks>
    /// <see cref="LegacyMulFileConverter"/> packs file to file, so a uop save writes the mul into a
    /// temporary folder with the domain's own save method, packs that, and throws the temporary copy
    /// away. Nothing in the domain classes has to learn about uop.
    /// </remarks>
    public static class ClientFileSaver
    {
        /// <summary>
        /// Whether the loaded client keeps this type in a uop. The choice is per file - a client can be
        /// uop art and mul gumps - so it is asked per type rather than once for the whole client.
        /// </summary>
        public static bool ClientUsesUop(FileType type, int mapIndex = 0)
        {
            return UopFileNames.ClientUopPath(type, mapIndex) != null;
        }

        /// <summary>
        /// What the user should be asked to confirm before this save runs. Empty when there is nothing
        /// to weigh up. Conditions that make a save impossible are not listed here - those throw from
        /// <see cref="Save"/>.
        /// </summary>
        public static IReadOnlyList<string> Preflight(FileType type, ContainerFormat format, int mapIndex = 0)
        {
            if (type != FileType.MultiCollection || format != ContainerFormat.Uop)
            {
                return Array.Empty<string>();
            }

            if (UopFileNames.ClientUopPath(FileType.MultiCollection) == null)
            {
                return Array.Empty<string>();
            }

            // The multi list is read from multi.idx and multi.mul, and on a modern client those describe
            // a reduced collection next to MultiCollection.uop: the live 7.0.114.4 client ships 800
            // multis of 62 177 tiles in the mul against the uop's 872 of 188 349. Packing the list
            // therefore writes a smaller collection than the client shipped, and the file itself will
            // not show it.
            int multisInModel = 0;
            int multisInClientUop = 0;
            long tilesInModel = 0;
            long tilesInClientUop = 0;

            for (int index = 0; index < Multis.MaximumMultiIndex; ++index)
            {
                MultiComponentList fromMul = Multis.GetComponents(index);

                if (fromMul != MultiComponentList.Empty)
                {
                    ++multisInModel;
                    tilesInModel += fromMul.SortedTiles.Length;
                }

                MultiComponentList fromUop = Multis.GetUopComponents(index);

                if (fromUop != MultiComponentList.Empty)
                {
                    ++multisInClientUop;
                    tilesInClientUop += fromUop.SortedTiles.Length;
                }
            }

            if (multisInClientUop <= multisInModel && tilesInClientUop <= tilesInModel)
            {
                return Array.Empty<string>();
            }

            return new[]
            {
                $"This client's MultiCollection.uop describes {multisInClientUop:N0} multis of "
                + $"{tilesInClientUop:N0} tiles. Its multi.mul, which is what the Multis tab shows and "
                + $"what a save writes, describes {multisInModel:N0} of {tilesInModel:N0}. Saving as "
                + "MultiCollection.uop writes what the tab holds and nothing else, so the difference "
                + "would be lost. Save as multi.mul instead, or unpack the client's MultiCollection.uop "
                + "with the UOP Packer and point the profile at the result first."
            };
        }

        /// <summary>
        /// Writes <paramref name="type"/> into <paramref name="outputDirectory"/> in the requested
        /// container. <paramref name="writeMul"/> is the domain's existing save method, which is handed
        /// the directory to write its mul and idx into.
        /// </summary>
        public static ClientFileSaveResult Save(FileType type, string outputDirectory, ContainerFormat format,
            Action<string> writeMul, int mapIndex = 0, IProgress<int> progress = null)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new ArgumentException("No output directory was given.", nameof(outputDirectory));
            }

            if (writeMul == null)
            {
                throw new ArgumentNullException(nameof(writeMul));
            }

            Directory.CreateDirectory(outputDirectory);

            var (mulName, idxName, uopName) = UopFileNames.For(type, mapIndex);

            if (format == ContainerFormat.Mul)
            {
                writeMul(outputDirectory);

                string[] written = idxName == null
                    ? new[] { mulName }
                    : new[] { mulName, idxName };

                return new ClientFileSaveResult(outputDirectory, written, Array.Empty<string>());
            }

            string temporaryDirectory = Path.Combine(Path.GetTempPath(),
                "UoFiddler-save-" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(temporaryDirectory);

            try
            {
                string temporaryMul = Path.Combine(temporaryDirectory, mulName);
                string temporaryIdx = idxName == null ? null : Path.Combine(temporaryDirectory, idxName);
                var warnings = new List<string>();

                string housingBin = string.Empty;
                string componentsFile = string.Empty;

                if (type == FileType.MultiCollection)
                {
                    // Unpacks the client's own file first, for the two things the in-memory multi model
                    // cannot supply. writeMul then overwrites the mul and idx it left behind.
                    (housingBin, componentsFile) = PrepareMultiCollection(temporaryDirectory, temporaryMul,
                        temporaryIdx, warnings);
                }

                writeMul(temporaryDirectory);

                if (!File.Exists(temporaryMul))
                {
                    throw new FileNotFoundException(
                        $"The save wrote no {mulName}, so there is nothing to pack into {uopName}.", temporaryMul);
                }

                string outputUop = Path.Combine(outputDirectory, uopName);

                LegacyMulFileConverter.ToUop(temporaryMul, temporaryIdx, outputUop, type, mapIndex,
                    UopFileNames.DefaultCompression(type), housingBin, progress, componentsFile);

                return new ClientFileSaveResult(outputDirectory, new[] { uopName }, warnings);
            }
            finally
            {
                TryDeleteDirectory(temporaryDirectory);
            }
        }

        /// <summary>
        /// Produces housing.bin and the component id sidecar in <paramref name="temporaryDirectory"/> by
        /// unpacking the loaded client's MultiCollection.uop.
        /// </summary>
        /// <remarks>
        /// MultiCollection.uop carries two things multi.mul does not: build/multicollection/housing.bin,
        /// the custom housing piece catalog, and a component id per tile that marks its interactive role -
        /// a boat without its tiller man cannot be steered and a house door stops being a door. The reader
        /// drops the component ids (see Multis.LoadUop) and multi.mul has nowhere to keep either, so the
        /// only honest source for both is the client's own file.
        /// </remarks>
        private static (string HousingBin, string ComponentsFile) PrepareMultiCollection(string temporaryDirectory,
            string temporaryMul, string temporaryIdx, ICollection<string> warnings)
        {
            string clientUop = UopFileNames.ClientUopPath(FileType.MultiCollection);

            if (clientUop == null)
            {
                throw new InvalidOperationException(
                    "Multis can only be saved as MultiCollection.uop when the loaded client has one to take "
                    + "build/multicollection/housing.bin and the tile component ids from. This client has no "
                    + "MultiCollection.uop, so save multis as multi.mul instead, or use the UOP Packer with a "
                    + "housing.bin of your own.");
            }

            if (UopFileNames.ClientMulPath(FileType.MultiCollection) == null)
            {
                throw new InvalidOperationException(
                    "This client has no multi.mul, so the multi list was never loaded and packing it would "
                    + "write an empty MultiCollection.uop. Unpack the client's MultiCollection.uop with the "
                    + "UOP Packer first, point the profile at the result, then save.");
            }

            string housingBin = Path.Combine(temporaryDirectory, "housing.bin");
            string componentsFile = MultiComponentSidecar.GetDefaultPath(temporaryMul);

            new LegacyMulFileConverter().FromUop(clientUop, temporaryMul, temporaryIdx,
                FileType.MultiCollection, 0, housingBin, null, componentsFile);

            if (!File.Exists(housingBin))
            {
                throw new InvalidOperationException(
                    $"{Path.GetFileName(clientUop)} contains no build/multicollection/housing.bin, which the "
                    + "client needs to place customisable house pieces. Packing without it would produce a "
                    + "file the client cannot use.");
            }

            MultiComponentSidecar.Status sidecar = MultiComponentSidecar.Probe(temporaryMul, componentsFile);

            if (sidecar.IsEmpty)
            {
                warnings.Add(
                    $"{Path.GetFileName(clientUop)} carries no tile component ids, so every tile is written "
                    + "with none. Boats lose their tiller man, hatch and planks, and customisable houses lose "
                    + "their doors.");
            }

            return (housingBin, componentsFile);
        }

        private static void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (IOException)
            {
                // A leftover temporary folder is untidy, not a failure; the original result stands.
            }
            catch (UnauthorizedAccessException)
            {
                // As above.
            }
        }
    }
}