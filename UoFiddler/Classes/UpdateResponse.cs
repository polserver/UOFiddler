using System;

namespace UoFiddler.Classes
{
    public class UpdateResponse
    {
        public bool HasErrors { get; private set; }
        public string ErrorMessage { get; private set; }

        public string Body { get; private set; }
        public string HtmlUrl { get; private set; }
        public bool IsNewVersion { get; private set; }
        public Version NewVersion { get; private set; }

        public static UpdateResponse Error(string errorMessage)
        {
            return new UpdateResponse() { HasErrors = true, ErrorMessage = errorMessage };
        }

        public static UpdateResponse Ok(string body, string htmlUrl, Version newVersion, bool isNewVersion)
        {
            return new UpdateResponse() { Body = body, HtmlUrl = htmlUrl, NewVersion = newVersion, IsNewVersion = isNewVersion };
        }
    }
}
