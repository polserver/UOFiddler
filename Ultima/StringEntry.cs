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

        private string _text;

        public int Number { get; }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value ?? string.Empty;
            }
        }

        public CliLocFlag Flag { get; set; }

        public StringEntry(int number, string text, byte flag)
        {
            Number = number;
            _text = text;
            Flag = (CliLocFlag)flag;
        }

        public StringEntry(int number, string text, CliLocFlag flag)
        {
            Number = number;
            _text = text;
            Flag = flag;
        }

        // Razor
        private static readonly Regex _regEx = new Regex(
            @"~(\d+)[_\w]+~",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        private string _fmtTxt;
        private static readonly object[] _args = { "", "", "", "", "", "", "", "", "", "", "" };

        public string Format(params object[] args)
        {
            if (_fmtTxt == null)
            {
                _fmtTxt = _regEx.Replace(_text, "{$1}");
            }

            for (int i = 0; i < args.Length && i < 10; i++)
            {
                _args[i + 1] = args[i];
            }

            return string.Format(_fmtTxt, _args);
        }

// TODO: unused?
//        public string SplitFormat(string argString)
//        {
//            if (_fmtTxt == null)
//            {
//                _fmtTxt = _regEx.Replace(_text, "{$1}");
//            }
//
//            string[] args = argString.Split('\t'); // adds an extra on to the args array
//
//            for (int i = 0; i < args.Length && i < 10; i++)
//            {
//                _args[i + 1] = args[i];
//            }
//
//            return string.Format(_fmtTxt, _args);
//            /*
//            {
//                StringBuilder sb = new StringBuilder();
//                sb.Append( _fmtTxt );
//                for(int i=0;i<args.Length;i++)
//                {
//                    sb.Append( "|" );
//                    sb.Append( args[i] == null ? "-null-" : args[i] );
//                }
//                throw new Exception( sb.ToString() );
//            }*/
//        }
    }
}