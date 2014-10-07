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

namespace FiddlerControls
{
    public partial class FontOffset : Form
    {
        private int m_font;
        private int m_char;
        public FontOffset(int font, int ch)
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            CharLabel.Text = String.Format("Character: '{0}' 0x{1:X}", (char)ch, ch);
            offsetx.Value = UnicodeFonts.Fonts[font].Chars[ch].XOffset;
            offsety.Value = UnicodeFonts.Fonts[font].Chars[ch].YOffset;
            m_font = font;
            m_char = ch;
        }

        private void OnClickOK(object sender, EventArgs e)
        {
            UnicodeFonts.Fonts[m_font].Chars[m_char].XOffset = (sbyte)offsetx.Value;
            UnicodeFonts.Fonts[m_font].Chars[m_char].YOffset = (sbyte)offsety.Value;
            Fonts.RefreshOnCharChange();
            Close();
        }

        private void OnClickCancel(object sender, EventArgs e)
        {
            Close();
        }
    }
}
