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
using System.IO;
using Ultima.Uop;

namespace Ultima.Maps
{
    public enum MapOutputFormat
    {
        Mul,
        Uop
    }

    /// <summary>
    /// Somewhere to put a facet's land blocks, in index order, without the producer caring whether
    /// the result is a map{N}.mul or a map{N}LegacyMUL.uop.
    /// </summary>
    public interface IMapBlockSink : IDisposable
    {
        /// <summary>Where the finished file will be, or is.</summary>
        string OutputPath { get; }

        /// <summary>Appends one 196-byte block: a 4-byte header followed by 64 three-byte tiles.</summary>
        void WriteBlock(ReadOnlySpan<byte> block);

        /// <summary>Finishes the file and moves it into place. Without this the output is discarded.</summary>
        void Complete();
    }

    public static class MapBlockSink
    {
        public const int MapBlockSize = 196;

        /// <summary>
        /// Opens a sink for a facet. The file is built beside its destination under a temporary name
        /// and only moved into place by <see cref="IMapBlockSink.Complete"/>, so an interrupted run
        /// leaves whatever was there before untouched.
        /// </summary>
        public static IMapBlockSink Create(string outputDirectory, int fileIndex, MapOutputFormat format, long blockCount)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new ArgumentException("No output directory was given.", nameof(outputDirectory));
            }

            Directory.CreateDirectory(outputDirectory);

            string name = format == MapOutputFormat.Uop
                ? $"map{fileIndex}LegacyMUL.uop"
                : $"map{fileIndex}.mul";

            string path = Path.GetFullPath(Path.Combine(outputDirectory, name));

            return format == MapOutputFormat.Uop
                ? new UopMapBlockSink(path, fileIndex, blockCount)
                : (IMapBlockSink)new MulMapBlockSink(path, blockCount);
        }

        internal static FileStream CreateTemporary(string path, out string temporaryPath)
        {
            temporaryPath = path + ".tmp-" + Guid.NewGuid().ToString("N");

            return new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1 << 20);
        }

        internal static void TryDelete(string path)
        {
            if (path == null || !File.Exists(path))
            {
                return;
            }

            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    internal sealed class MulMapBlockSink : IMapBlockSink
    {
        private readonly long _blockCount;
        private readonly FileStream _stream;

        private string _temporaryPath;
        private long _blocksWritten;
        private bool _completed;

        public MulMapBlockSink(string path, long blockCount)
        {
            OutputPath = path;
            _blockCount = blockCount;
            _stream = MapBlockSink.CreateTemporary(path, out _temporaryPath);
        }

        public string OutputPath { get; }

        public void WriteBlock(ReadOnlySpan<byte> block)
        {
            if (block.Length != MapBlockSink.MapBlockSize)
            {
                throw new ArgumentException($"A land block is {MapBlockSink.MapBlockSize} bytes.", nameof(block));
            }

            _stream.Write(block);
            ++_blocksWritten;
        }

        public void Complete()
        {
            if (_completed)
            {
                return;
            }

            if (_blocksWritten != _blockCount)
            {
                throw new InvalidOperationException(
                    $"{_blocksWritten:N0} blocks were written but the facet holds {_blockCount:N0}.");
            }

            _stream.Flush();
            _stream.Dispose();

            File.Move(_temporaryPath, OutputPath, true);

            _temporaryPath = null;
            _completed = true;
        }

        public void Dispose()
        {
            _stream.Dispose();
            MapBlockSink.TryDelete(_temporaryPath);
        }
    }

    internal sealed class UopMapBlockSink : IMapBlockSink
    {
        private readonly FileStream _stream;
        private readonly MapUopWriter _writer;

        private string _temporaryPath;
        private bool _completed;

        public UopMapBlockSink(string path, int fileIndex, long blockCount)
        {
            OutputPath = path;
            _stream = MapBlockSink.CreateTemporary(path, out _temporaryPath);
            _writer = new MapUopWriter(_stream, fileIndex, blockCount, MapTrailingBlock.Empty, true);
        }

        public string OutputPath { get; }

        public void WriteBlock(ReadOnlySpan<byte> block)
        {
            _writer.WriteBlock(block);
        }

        public void Complete()
        {
            if (_completed)
            {
                return;
            }

            _writer.Complete();
            _stream.Flush();
            _stream.Dispose();

            File.Move(_temporaryPath, OutputPath, true);

            _temporaryPath = null;
            _completed = true;
        }

        public void Dispose()
        {
            _writer.Dispose();
            _stream.Dispose();
            MapBlockSink.TryDelete(_temporaryPath);
        }
    }
}