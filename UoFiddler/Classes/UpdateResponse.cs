using System;

namespace UoFiddler.Classes
{
    public sealed class UpdateResponse
    {
        public bool HasErrors { get; private init; }
        public string ErrorMessage { get; private init; }

        public string Body { get; private init; }
        public string HtmlUrl { get; private init; }
        public bool IsNewVersion { get; private init; }
        public Version NewVersion { get; private init; }

        public static UpdateResponse Error(string errorMessage)
        {
            return new UpdateResponse
            {
                HasErrors = true,
                ErrorMessage = errorMessage
            };
        }

        public static UpdateResponse Ok(string body, string htmlUrl, Version newVersion, bool isNewVersion)
        {
            return new UpdateResponse
            {
                Body = body,
                HtmlUrl = htmlUrl,
                NewVersion = newVersion,
                IsNewVersion = isNewVersion
            };
        }
    }
}
