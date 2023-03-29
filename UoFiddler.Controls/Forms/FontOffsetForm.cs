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
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class FontOffsetForm : Form
    {
        private readonly int _font;
        private readonly int _char;

        public FontOffsetForm(int font, int ch)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            CharLabel.Text = $"Character: '{(char)ch}' 0x{ch:X}";
            offsetx.Value = UnicodeFonts.Fonts[font].Chars[ch].XOffset;
            offsety.Value = UnicodeFonts.Fonts[font].Chars[ch].YOffset;
            _font = font;
            _char = ch;
        }

        private void OnClickOK(object sender, EventArgs e)
        {
            UnicodeFonts.Fonts[_font].Chars[_char].XOffset = (sbyte)offsetx.Value;
            UnicodeFonts.Fonts[_font].Chars[_char].YOffset = (sbyte)offsety.Value;

            Close();
        }

        private void OnClickCancel(object sender, EventArgs e)
        {
            Close();
        }
    }
}
