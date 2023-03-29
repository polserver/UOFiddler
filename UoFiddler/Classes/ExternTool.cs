using System.Collections.Generic;

namespace UoFiddler.Classes
{
    public sealed class ExternTool
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public List<string> Args { get; }
        public List<string> ArgsName { get; }

        public ExternTool(string name, string filename)
        {
            Name = name;
            FileName = filename;
            Args = new List<string>();
            ArgsName = new List<string>();
        }

        public string FormatName()
        {
            return $"{Name}: {FileName}";
        }

        public string FormatArg(int i)
        {
            return $"{ArgsName[i]}: {Args[i]}";
        }
    }
}