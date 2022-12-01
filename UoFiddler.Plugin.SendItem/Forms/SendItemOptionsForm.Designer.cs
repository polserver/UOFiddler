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

namespace UoFiddler.Plugin.SendItem.Forms
{
    partial class SendItemOptionsForm
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SendOnClick = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.argstext = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cmdtext = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.SendOnClick);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.argstext);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.cmdtext);
            this.groupBox1.Location = new System.Drawing.Point(18, 15);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(233, 121);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Send Item";
            // 
            // SendOnClick
            // 
            this.SendOnClick.AutoSize = true;
            this.SendOnClick.Location = new System.Drawing.Point(43, 92);
            this.SendOnClick.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.SendOnClick.Name = "SendOnClick";
            this.SendOnClick.Size = new System.Drawing.Size(136, 19);
            this.SendOnClick.TabIndex = 18;
            this.SendOnClick.Text = "Send on DoubleClick";
            this.toolTip1.SetToolTip(this.SendOnClick, "Overrides DoubleClick");
            this.SendOnClick.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(18, 65);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(31, 15);
            this.label8.TabIndex = 17;
            this.label8.Text = "Args";
            this.toolTip1.SetToolTip(this.label8, "{1} = Selected item ObjType");
            // 
            // argstext
            // 
            this.argstext.Location = new System.Drawing.Point(99, 61);
            this.argstext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.argstext.Name = "argstext";
            this.argstext.Size = new System.Drawing.Size(116, 23);
            this.argstext.TabIndex = 16;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(18, 35);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(33, 15);
            this.label7.TabIndex = 15;
            this.label7.Text = "Cmd";
            this.toolTip1.SetToolTip(this.label7, "Defines the cmd to send Client for selected Item");
            // 
            // cmdtext
            // 
            this.cmdtext.Location = new System.Drawing.Point(99, 31);
            this.cmdtext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cmdtext.Name = "cmdtext";
            this.cmdtext.Size = new System.Drawing.Size(116, 23);
            this.cmdtext.TabIndex = 14;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(90, 143);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 27);
            this.button1.TabIndex = 1;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnClickSave);
            // 
            // SendItemOptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(269, 181);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "SendItemOptionsForm";
            this.Text = "SendItem Options";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox argstext;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox cmdtext;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox SendOnClick;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}