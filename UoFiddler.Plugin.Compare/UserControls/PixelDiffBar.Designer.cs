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

namespace UoFiddler.Plugin.Compare.UserControls
{
    partial class PixelDiffBar
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                DisposeDiff();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            toolTip = new System.Windows.Forms.ToolTip(components);
            flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            checkBoxPixelDiff = new System.Windows.Forms.CheckBox();
            labelTolerance = new System.Windows.Forms.Label();
            numericTolerance = new System.Windows.Forms.NumericUpDown();
            labelFade = new System.Windows.Forms.Label();
            numericFade = new System.Windows.Forms.NumericUpDown();
            labelResult = new System.Windows.Forms.Label();
            flowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericTolerance).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericFade).BeginInit();
            SuspendLayout();
            //
            // flowLayoutPanel
            //
            flowLayoutPanel.Controls.Add(checkBoxPixelDiff);
            flowLayoutPanel.Controls.Add(labelTolerance);
            flowLayoutPanel.Controls.Add(numericTolerance);
            flowLayoutPanel.Controls.Add(labelFade);
            flowLayoutPanel.Controls.Add(numericFade);
            flowLayoutPanel.Controls.Add(labelResult);
            flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            flowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.Size = new System.Drawing.Size(400, 29);
            flowLayoutPanel.TabIndex = 0;
            flowLayoutPanel.WrapContents = false;
            //
            // checkBoxPixelDiff
            //
            checkBoxPixelDiff.AutoSize = true;
            checkBoxPixelDiff.Margin = new System.Windows.Forms.Padding(3, 6, 6, 3);
            checkBoxPixelDiff.Name = "checkBoxPixelDiff";
            checkBoxPixelDiff.Size = new System.Drawing.Size(73, 19);
            checkBoxPixelDiff.TabIndex = 0;
            checkBoxPixelDiff.Text = "Pixel diff";
            toolTip.SetToolTip(checkBoxPixelDiff, "Show which pixels differ between the two previews: unchanged pixels fade, changed ones keep their colors inside a magenta outline. The images are compared from their top-left corners.");
            checkBoxPixelDiff.CheckedChanged += OnEnabledChanged;
            //
            // labelTolerance
            //
            labelTolerance.AutoSize = true;
            labelTolerance.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
            labelTolerance.Name = "labelTolerance";
            labelTolerance.Size = new System.Drawing.Size(57, 15);
            labelTolerance.TabIndex = 1;
            labelTolerance.Text = "Tolerance";
            toolTip.SetToolTip(labelTolerance, "Ignore color differences up to this much per channel (0-255)");
            //
            // numericTolerance
            //
            numericTolerance.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            numericTolerance.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericTolerance.Name = "numericTolerance";
            numericTolerance.Size = new System.Drawing.Size(50, 23);
            numericTolerance.TabIndex = 2;
            toolTip.SetToolTip(numericTolerance, "Ignore color differences up to this much per channel (0-255)");
            numericTolerance.ValueChanged += OnToleranceChanged;
            //
            // labelFade
            //
            labelFade.AutoSize = true;
            labelFade.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
            labelFade.Name = "labelFade";
            labelFade.Size = new System.Drawing.Size(62, 15);
            labelFade.TabIndex = 3;
            labelFade.Text = "Fade %";
            toolTip.SetToolTip(labelFade, "How much the unchanged pixels fade into the background, so the changed ones stand out (0: no fading, only the outline)");
            //
            // numericFade
            //
            numericFade.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            numericFade.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            numericFade.Name = "numericFade";
            numericFade.Size = new System.Drawing.Size(50, 23);
            numericFade.TabIndex = 4;
            numericFade.Value = new decimal(new int[] { 70, 0, 0, 0 });
            toolTip.SetToolTip(numericFade, "How much the unchanged pixels fade into the background, so the changed ones stand out (0: no fading, only the outline)");
            numericFade.ValueChanged += OnFadeChanged;
            //
            // labelResult
            //
            labelResult.AutoSize = true;
            labelResult.Margin = new System.Windows.Forms.Padding(6, 7, 3, 0);
            labelResult.Name = "labelResult";
            labelResult.Size = new System.Drawing.Size(0, 15);
            labelResult.TabIndex = 5;
            toolTip.SetToolTip(labelResult, "How many pixels differ between the two previews");
            //
            // PixelDiffBar
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(flowLayoutPanel);
            Margin = new System.Windows.Forms.Padding(0);
            Name = "PixelDiffBar";
            Size = new System.Drawing.Size(400, 29);
            flowLayoutPanel.ResumeLayout(false);
            flowLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericTolerance).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericFade).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private System.Windows.Forms.CheckBox checkBoxPixelDiff;
        private System.Windows.Forms.Label labelTolerance;
        private System.Windows.Forms.NumericUpDown numericTolerance;
        private System.Windows.Forms.Label labelFade;
        private System.Windows.Forms.NumericUpDown numericFade;
        private System.Windows.Forms.Label labelResult;
    }
}
