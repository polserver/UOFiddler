using System;
using System.Threading.Tasks;
using Octokit;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Classes
{
    internal sealed class UpdateChecker
    {
        private readonly IGitHubClient _githubClient;
        private readonly string _repositoryName;
        private readonly string _repositoryOwner;
        private readonly Version _currentVersion;

        public UpdateChecker(string repositoryOwner, string repositoryName, Version currentVersion)
        {
            ThrowIf.Argument.IsNullOrEmptyString(repositoryOwner, nameof(repositoryOwner));
            ThrowIf.Argument.IsNullOrEmptyString(repositoryName, nameof(repositoryName));
            ThrowIf.Argument.IsNull(currentVersion, nameof(currentVersion));

            _githubClient = new GitHubClient(new ProductHeaderValue($"{repositoryName}-update-checker"));

            _repositoryOwner = repositoryOwner;
            _repositoryName = repositoryName;
            _currentVersion = currentVersion;
        }

        public async Task<UpdateResponse> CheckUpdateAsync()
        {
            try
            {
                var rateLimit = await _githubClient.RateLimit.GetRateLimits().ConfigureAwait(false);
                if (rateLimit.Resources.Core.Remaining == 0)
                {
                    return UpdateResponse.Error($"Update API rate limit exceeded. Limit will reset at {rateLimit.Resources.Core.Reset.LocalDateTime}. Please try again later.");
                }

                var latestRelease = await _githubClient.Repository.Release.GetLatest(_repositoryOwner, _repositoryName).ConfigureAwait(false);

                bool isNewVersion = false;
                if (Version.TryParse(StripInitialV(latestRelease.TagName), out Version newVersion))
                {
                    isNewVersion = newVersion > _currentVersion;
                }

                return UpdateResponse.Ok(latestRelease.Body, latestRelease.HtmlUrl, newVersion, isNewVersion);
            }
            catch (NotFoundException ex)
            {
                return UpdateResponse.Error($"Now new\r\nMessage: {ex.Message}");
            }
            catch (AggregateException ae)
            {
                var message = string.Empty;

                foreach (var ex in ae.InnerExceptions)
                {
                    message += $"Exception: {ex.GetType().Name}\r\nMessage: {ex.Message}\r\n";
                }

                return UpdateResponse.Error($"Problem with checking version:\r\n{message}");
            }
        }

        private static string StripInitialV(string versionString)
        {
            return (versionString[0] == 'v') ? versionString[1..] : versionString;
        }
    }
}
