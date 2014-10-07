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

namespace UoFiddler
{
    partial class AboutBox
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.checkBoxCheckOnStart = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.progresslabel = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.checkBoxFormState = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.richTextBox1.Location = new System.Drawing.Point(13, 25);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(410, 219);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // checkBoxCheckOnStart
            // 
            this.checkBoxCheckOnStart.AutoSize = true;
            this.checkBoxCheckOnStart.Location = new System.Drawing.Point(13, 419);
            this.checkBoxCheckOnStart.Name = "checkBoxCheckOnStart";
            this.checkBoxCheckOnStart.Size = new System.Drawing.Size(99, 17);
            this.checkBoxCheckOnStart.TabIndex = 1;
            this.checkBoxCheckOnStart.Text = "Check On Start";
            this.checkBoxCheckOnStart.UseVisualStyleBackColor = true;
            this.checkBoxCheckOnStart.CheckedChanged += new System.EventHandler(this.OnChangeCheck);
            // 
            // button1
            // 
            this.button1.AutoSize = true;
            this.button1.Location = new System.Drawing.Point(118, 415);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(101, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Check for Update";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnClickUpdate);
            // 
            // progresslabel
            // 
            this.progresslabel.AutoSize = true;
            this.progresslabel.Location = new System.Drawing.Point(225, 420);
            this.progresslabel.Name = "progresslabel";
            this.progresslabel.Size = new System.Drawing.Size(70, 13);
            this.progresslabel.TabIndex = 3;
            this.progresslabel.Text = "Progresslabel";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(12, 9);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(82, 13);
            this.linkLabel1.TabIndex = 4;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Visit HomePage";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnClickLink);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label1.Location = new System.Drawing.Point(13, 251);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Project Author and Admin";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(41, 268);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Turley";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(16, 287);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Special Contributers";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(41, 300);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(195, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "MuadDib, Soulblighter, Nibbio, Andreew";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(19, 325);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Special Thanks";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(41, 338);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(324, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "http://www.polserver.com community for all their feedback and use";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(41, 351);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(229, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "UltimaSDK Devs for the backbone we modified";
            // 
            // checkBoxFormState
            // 
            this.checkBoxFormState.AutoSize = true;
            this.checkBoxFormState.Location = new System.Drawing.Point(13, 396);
            this.checkBoxFormState.Name = "checkBoxFormState";
            this.checkBoxFormState.Size = new System.Drawing.Size(100, 17);
            this.checkBoxFormState.TabIndex = 12;
            this.checkBoxFormState.Text = "Store Formstate";
            this.checkBoxFormState.UseVisualStyleBackColor = true;
            this.checkBoxFormState.CheckedChanged += new System.EventHandler(this.OnChangeFormState);
            // 
            // AboutBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 448);
            this.Controls.Add(this.checkBoxFormState);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.progresslabel);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.checkBoxCheckOnStart);
            this.Controls.Add(this.richTextBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.CheckBox checkBoxCheckOnStart;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label progresslabel;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox checkBoxFormState;



    }
}
