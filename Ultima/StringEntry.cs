using System;
using System.Text.RegularExpressions;

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
        private string _mText;

        public int Number { get; }
        public string Text
        {
            get => _mText;
            set
            {
                if (value == null)
                    _mText = "";
                else
                    _mText = value;
            }
        }
        public CliLocFlag Flag { get; set; }

        public StringEntry(int number, string text, byte flag)
        {
            Number = number;
            _mText = text;
            Flag = (CliLocFlag)flag;
        }

        public StringEntry(int number, string text, CliLocFlag flag)
        {
            Number = number;
            _mText = text;
            Flag = flag;
        }

		// Razor
		private static readonly Regex _mRegEx = new Regex(@"~(\d+)[_\w]+~", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
		private string _mFmtTxt;
		private static readonly object[] _mArgs = new object[] { "", "", "", "", "", "", "", "", "", "", "" };

		public string Format(params object[] args)
		{
			if (_mFmtTxt == null)
				_mFmtTxt = _mRegEx.Replace(_mText, @"{$1}");
			for (int i = 0; i < args.Length && i < 10; i++)
				_mArgs[i + 1] = args[i];
			return string.Format(_mFmtTxt, _mArgs);
		}

		public string SplitFormat(string argstr)
		{
			if (_mFmtTxt == null)
				_mFmtTxt = _mRegEx.Replace(_mText, @"{$1}");
			string[] args = argstr.Split('\t');// adds an extra on to the args array
			for (int i = 0; i < args.Length && i < 10; i++)
				_mArgs[i + 1] = args[i];
			return string.Format(_mFmtTxt, _mArgs);
			/*
			{
				StringBuilder sb = new StringBuilder();
				sb.Append( m_FmtTxt );
				for(int i=0;i<args.Length;i++)
				{
					sb.Append( "|" );
					sb.Append( args[i] == null ? "-null-" : args[i] );
				}
				throw new Exception( sb.ToString() );
			}*/
		}
    }
}