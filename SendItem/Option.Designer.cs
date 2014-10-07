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

namespace FiddlerPlugin
{
    partial class Option
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
            this.groupBox1.Location = new System.Drawing.Point(15, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 105);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Send Item";
            // 
            // SendOnClick
            // 
            this.SendOnClick.AutoSize = true;
            this.SendOnClick.Location = new System.Drawing.Point(37, 80);
            this.SendOnClick.Name = "SendOnClick";
            this.SendOnClick.Size = new System.Drawing.Size(126, 17);
            this.SendOnClick.TabIndex = 18;
            this.SendOnClick.Text = "Send on DoubleClick";
            this.toolTip1.SetToolTip(this.SendOnClick, "Overrides DoubleClick");
            this.SendOnClick.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(15, 56);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(28, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "Args";
            this.toolTip1.SetToolTip(this.label8, "{1} = Selected item ObjType");
            // 
            // argstext
            // 
            this.argstext.Location = new System.Drawing.Point(85, 53);
            this.argstext.Name = "argstext";
            this.argstext.Size = new System.Drawing.Size(100, 20);
            this.argstext.TabIndex = 16;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 30);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(28, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Cmd";
            this.toolTip1.SetToolTip(this.label7, "Definies the cmd to send Client for selected Item");
            // 
            // cmdtext
            // 
            this.cmdtext.Location = new System.Drawing.Point(85, 27);
            this.cmdtext.Name = "cmdtext";
            this.cmdtext.Size = new System.Drawing.Size(100, 20);
            this.cmdtext.TabIndex = 14;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(77, 124);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnClickSave);
            // 
            // Option
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(231, 155);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Name = "Option";
            this.Text = "SendItem Options";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox argstext;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox cmdtext;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox SendOnClick;
    }
}