using System;

namespace Ultima
{
    public sealed class StringEntry
    {
        [Flags]
        public enum CliLocFlag
        {
            Original = 0x0,
            Custom = 0x1,
            Modified = 0x2
        }
        private string m_Text;

        public int Number { get; private set; }
        public string Text
        {
            get { return m_Text; }
            set
            {
                if (value == null)
                    m_Text = "";
                else
                    m_Text = value;
            }
        }
        public CliLocFlag Flag { get; set; }

        public StringEntry(int number, string text, byte flag)
        {
            Number = number;
            m_Text = text;
            Flag = (CliLocFlag)flag;
        }

        public StringEntry(int number, string text, CliLocFlag flag)
        {
            Number = number;
            m_Text = text;
            Flag = flag;
        }
    }
}