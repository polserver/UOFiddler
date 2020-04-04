namespace UoFiddler.Classes
{
    internal static class TagHelper
    {
        public static string StripInitialV(string versionString)
        {
            return (versionString[0] == 'v')
                ? versionString.Substring(1)
                : versionString;
        }
    }
}
