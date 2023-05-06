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
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class FontTextForm : Form
    {
        private readonly int _type;
        private readonly int _font;

        public FontTextForm(int mType, int mFont)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            TopMost = true;
            _type = mType;
            _font = mFont;
            pictureBox1.BackColor = Color.White;
            Text = _type == 1
                ? $"Unicode Font:{mFont}"
                : $"ASCII Font:{mFont}";
        }

        private void OnTextChange(object sender, EventArgs e)
        {
            pictureBox1.Image = _type == 1
                ? UnicodeFonts.WriteText(_font, textBox1.Text)
                : AsciiText.DrawText(_font, textBox1.Text);
        }
    }
}
