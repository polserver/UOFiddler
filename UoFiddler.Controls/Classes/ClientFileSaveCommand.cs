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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Ultima.Helpers;
using Ultima.Uop;
using UoFiddler.Controls.Forms;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// One save, from the format question through to the dialog that says where the files went.
    /// The five tabs whose file type the client ships in either container all go through here.
    /// </summary>
    public static class ClientFileSaveCommand
    {
        /// <summary>
        /// Resolves the container, writes the files and reports the outcome. Returns false when the
        /// user cancelled or the save failed, in which case the dirty flag is left alone.
        /// </summary>
        /// <param name="owner">Control the dialogs belong to.</param>
        /// <param name="type">What is being saved.</param>
        /// <param name="writeMul">
        /// The domain's own save method. It is handed a directory to write its mul and idx into, which
        /// is not necessarily the output directory - a uop save writes the mul somewhere temporary.
        /// </param>
        /// <param name="dirtyKey">Key in <see cref="Options.ChangedUltimaClass"/> to clear on success.</param>
        /// <param name="createProgress">
        /// Optional progress dialog for the slow types. It is created before the save and disposed after,
        /// and its caption carries the pack percentage once the mul has been written.
        /// </param>
        public static bool Run(Control owner, FileType type, Action<string> writeMul, string dirtyKey = null,
            int mapIndex = 0, Func<ProgressBarDialog> createProgress = null)
        {
            string outputDirectory = Options.OutputPath;

            if (!SaveFormatResolver.TryResolve(owner, type, outputDirectory, out ContainerFormat format, mapIndex))
            {
                return false;
            }

            if (!ConfirmPreflight(owner, type, format, mapIndex))
            {
                return false;
            }

            string uopName = UopFileNames.For(type, mapIndex).Uop;
            ClientFileSaveResult result;

            try
            {
                using (new WaitCursorScope(owner))
                {
                    ProgressBarDialog progressDialog = createProgress?.Invoke();

                    try
                    {
                        IProgress<int> packProgress = progressDialog == null || format != ContainerFormat.Uop
                            ? null
                            : new CaptionProgress(progressDialog, uopName);

                        result = ClientFileSaver.Save(type, outputDirectory, format, writeMul, mapIndex, packProgress);
                    }
                    finally
                    {
                        progressDialog?.Dispose();
                    }
                }
            }
            catch (Exception error)
            {
                ShowError(owner, type, error);
                return false;
            }

            if (!string.IsNullOrEmpty(dirtyKey))
            {
                Options.ChangedUltimaClass[dirtyKey] = false;
            }

            // 尝试通过反射访问 LocalizationService 来获取汉化
            Func<string, string?> getLocalized = null;
            try
            {
                var localizationServiceType = Type.GetType("UoFiddler.Localization.LocalizationService, UoFiddler");
                if (localizationServiceType != null)
                {
                    var getStringMethod = localizationServiceType.GetMethod("GetString", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (getStringMethod != null)
                    {
                        getLocalized = key => (string?)getStringMethod.Invoke(null, new object[] { key });
                    }
                }
            }
            catch { }

            string title = "Saved";
            if (getLocalized != null)
            {
                var localizedTitle = getLocalized("Forms.FileSavedDialog.Title");
                if (localizedTitle != null)
                    title = localizedTitle;
            }

            if (getLocalized != null)
                FileSavedDialog.Show(owner?.FindForm(), outputDirectory, BuildMessage(result), title, getLocalized);
            else
                FileSavedDialog.Show(owner?.FindForm(), outputDirectory, BuildMessage(result));

            return true;
        }

        /// <summary>
        /// Puts anything the save would quietly cost in front of the user before it runs. Answering no
        /// leaves the output folder untouched.
        /// </summary>
        private static bool ConfirmPreflight(Control owner, FileType type, ContainerFormat format, int mapIndex)
        {
            IReadOnlyList<string> concerns;

            try
            {
                concerns = ClientFileSaver.Preflight(type, format, mapIndex);
            }
            catch (Exception error)
            {
                ShowError(owner, type, error);
                return false;
            }

            if (concerns.Count == 0)
            {
                return true;
            }

            var sb = new StringBuilder();

            foreach (string concern in concerns)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine().AppendLine();
                }

                sb.Append(concern);
            }

            sb.AppendLine().AppendLine().Append("Save anyway?");

            return MessageBox.Show(owner?.FindForm(), sb.ToString(), "Save", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private static string BuildMessage(ClientFileSaveResult result)
        {
            var sb = new StringBuilder();

            sb.Append("Saved ").Append(string.Join(", ", result.FilesWritten)).Append('.');

            foreach (string warning in result.Warnings)
            {
                sb.AppendLine().AppendLine().Append(warning);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Shows what actually went wrong rather than a bare framework message, and logs the rest.
        /// </summary>
        private static void ShowError(Control owner, FileType type, Exception error)
        {
            AppLog.For(typeof(ClientFileSaveCommand)).LogError(error, "Saving {Type} failed.", type);

            var sb = new StringBuilder();

            for (Exception current = error; current != null; current = current.InnerException)
            {
                sb.AppendLine(current.Message);

                if (current.InnerException != null)
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine();
            sb.AppendLine(error.GetType().FullName);

            string where = error.StackTrace?
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()?
                .Trim();

            if (!string.IsNullOrEmpty(where))
            {
                sb.AppendLine(where);
            }

            MessageBox.Show(owner?.FindForm(), sb.ToString(), "Save failed", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        /// <summary>
        /// Puts the pack percentage in the progress dialog's caption. The save blocks the UI thread, so
        /// the report has to be applied and repainted where it happens rather than posted back.
        /// </summary>
        private sealed class CaptionProgress : IProgress<int>
        {
            private readonly ProgressBarDialog _dialog;
            private readonly string _fileName;

            private int _lastReported = -1;

            public CaptionProgress(ProgressBarDialog dialog, string fileName)
            {
                _dialog = dialog;
                _fileName = fileName;
            }

            public void Report(int value)
            {
                if (value == _lastReported || _dialog.IsDisposed)
                {
                    return;
                }

                _lastReported = value;

                _dialog.Text = $"Packing {_fileName} - {value}%";
                _dialog.Refresh();
            }
        }
    }
}