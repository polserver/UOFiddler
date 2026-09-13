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

using System.Windows.Forms;
using Ultima.Uop;
using UoFiddler.Controls.Forms;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// Turns the save format option into the one container a save is about to write.
    /// </summary>
    public static class SaveFormatResolver
    {
        /// <summary>
        /// The container to write without asking anything. Use this where a prompt would be wrong -
        /// a batch, or a form that already has its own format control.
        /// </summary>
        public static ContainerFormat Resolve(FileType type, int mapIndex = 0)
        {
            switch (Options.SaveFormat)
            {
                case ClientFileSaveFormat.Mul:
                    return ContainerFormat.Mul;

                case ClientFileSaveFormat.Uop:
                    return ContainerFormat.Uop;

                default:
                    // Ask falls back to the source format: a batch must not stop on a dialog.
                    return ClientFileSaver.ClientUsesUop(type, mapIndex)
                        ? ContainerFormat.Uop
                        : ContainerFormat.Mul;
            }
        }

        /// <summary>
        /// The container to write, prompting when the option says to ask. Returns false when the user
        /// cancelled, in which case nothing should be written.
        /// </summary>
        public static bool TryResolve(IWin32Window owner, FileType type, string outputDirectory,
            out ContainerFormat format, int mapIndex = 0)
        {
            format = Resolve(type, mapIndex);

            if (Options.SaveFormat != ClientFileSaveFormat.Ask)
            {
                return true;
            }

            using (var dialog = new SaveFormatDialog(type, outputDirectory, format, mapIndex))
            {
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                {
                    return false;
                }

                format = dialog.SelectedFormat;
            }

            return true;
        }
    }
}