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
using System.Drawing;
using System.Windows.Forms;
using Ultima;

namespace FiddlerControls
{
    public partial class FontText : Form
    {
        private int type;
        private int font;
        public FontText(int m_type, int m_font)
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            this.TopMost = true;
            type = m_type;
            font = m_font;
            pictureBox1.BackColor = Color.White;
            if (type == 1)
                this.Text = String.Format("Unicode Font:{0}", m_font);
            else
                this.Text = String.Format("ASCII Font:{0}", m_font);
        }

        private void OnTextChange(object sender, EventArgs e)
        {
            if (type == 1) //Unicode
                pictureBox1.Image = UnicodeFonts.WriteText(font, textBox1.Text);
            else
                pictureBox1.Image = ASCIIText.DrawText(font, textBox1.Text);
        }
    }
}
