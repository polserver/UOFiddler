namespace UoFiddler
{
    partial class LoadProfile
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
            this.comboBoxLoad = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.textBoxCreate = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboBoxBasedOn = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxLoad
            // 
            this.comboBoxLoad.FormattingEnabled = true;
            this.comboBoxLoad.Location = new System.Drawing.Point(21, 31);
            this.comboBoxLoad.Name = "comboBoxLoad";
            this.comboBoxLoad.Size = new System.Drawing.Size(121, 21);
            this.comboBoxLoad.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(165, 29);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(85, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Load Profile";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnClickLoad);
            // 
            // textBoxCreate
            // 
            this.textBoxCreate.Location = new System.Drawing.Point(9, 19);
            this.textBoxCreate.Name = "textBoxCreate";
            this.textBoxCreate.Size = new System.Drawing.Size(121, 20);
            this.textBoxCreate.TabIndex = 2;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(153, 17);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(85, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Create Profile";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OnClickCreate);
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(253, 57);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Load";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.comboBoxBasedOn);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.textBoxCreate);
            this.groupBox2.Location = new System.Drawing.Point(12, 75);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(253, 83);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Create";
            // 
            // comboBoxBasedOn
            // 
            this.comboBoxBasedOn.FormattingEnabled = true;
            this.comboBoxBasedOn.Location = new System.Drawing.Point(66, 46);
            this.comboBoxBasedOn.Name = "comboBoxBasedOn";
            this.comboBoxBasedOn.Size = new System.Drawing.Size(121, 21);
            this.comboBoxBasedOn.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Based On";
            // 
            // LoadProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(277, 164);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBoxLoad);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "LoadProfile";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Choose Profile";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxLoad;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBoxCreate;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxBasedOn;
    }
}