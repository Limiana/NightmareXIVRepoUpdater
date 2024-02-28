using Newtonsoft.Json;
using Octokit;
using Octokit.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightmareXIVRepoUpdater;
internal class Updater
{
    List<RepoMetadata> Meta = [
        new("DynamicBridge", null, "main"),
        new("HuntTrainAssistant"),
        new("Lifestream", null, "main"),
        new("DSREyeLocator"),
        new("AbyssosToolbox"),
        new("WExtras"),
        new("TextAdvance"),
        new("AntiAfkKick", "AntiAfkKick-Dalamud"),
        new("SelectString"),
        new("Prioritizer"),
        new("UnloadErrorFuckOff"),
        new("Imagenation"),
        new("ObjectExplorer"),
        new("Instancinator"),
        new("XIVInstantMessenger", "Messenger"),
        ];

    public Updater(string[] args)
    {
        var secretsProvider = new SecretsProvider();
        var roCredentials = new InMemoryCredentialStore(new(secretsProvider.ReadOnlyKey, AuthenticationType.Bearer));
        var nightmareXivCredentials = new InMemoryCredentialStore(new(secretsProvider.NightmareXIVWriteKey, AuthenticationType.Bearer));
        var github = new GitHubClient(new ProductHeaderValue("NightmareXIVRepoUpdater"), roCredentials);

        List<PluginManifest> pluginmaster = [];

        foreach(var data in Meta)
        {
            var releases = github.Repository.Release.GetAll(data.Owner, data.RepoName).Result;
            var cnt = releases.Sum(x => x.Assets.FindSuitableAssets().Sum(a => a.DownloadCount));
            Console.WriteLine($"{data.RepoName}, downloads={cnt}");
            var mainVersions = releases.Where(x => !x.Draft && !x.Prerelease && x.Assets.FindSuitableAsset() != null).OrderByDescending(x => x.CreatedAt);
            var testingVersions = releases.Where(x => !x.Draft && x.Prerelease && x.Assets.FindSuitableAsset() != null).OrderByDescending(x => x.CreatedAt);
            PluginManifest? baseManifest = null;
            if (mainVersions.Any())
            {
                var ver = mainVersions.First();
                var asset = ver.Assets.FindSuitableAsset();
                if (asset != null)
                {
                    Console.WriteLine($"  Latest release: {ver.TagName} at {asset.BrowserDownloadUrl}");
                    Console.WriteLine($"  Changelog: \n{ver.Body}");
                    var response = github.Connection.GetRawStream(new(asset.BrowserDownloadUrl), new Dictionary<string, string>()).Result;
                    var manifest = Utils.GetMainfest(response.Body);
                    baseManifest ??= manifest;
                    baseManifest.DownloadLinkInstall = asset.BrowserDownloadUrl;
                    baseManifest.DownloadLinkUpdate = asset.BrowserDownloadUrl;
                    baseManifest.LastUpdate = asset.CreatedAt.ToUnixTimeSeconds();
                    baseManifest.Changelog = ver.Body;
                }
            }
            if (testingVersions.Any())
            {
                var ver = testingVersions.First();
                var asset = ver.Assets.FindSuitableAsset();
                if (asset != null)
                {
                    Console.WriteLine($"  Testing release: {ver.TagName} at {asset.BrowserDownloadUrl}");
                    Console.WriteLine($"  Changelog: \n{ver.Body}");
                    var response = github.Connection.GetRawStream(new(asset.BrowserDownloadUrl), new Dictionary<string, string>()).Result;
                    var manifest = Utils.GetMainfest(response.Body);
                    baseManifest ??= manifest;
                    baseManifest.DownloadLinkTesting = asset.BrowserDownloadUrl;
                    baseManifest.TestingAssemblyVersion = manifest.AssemblyVersion;
                    if(asset.CreatedAt.ToUnixTimeSeconds() > baseManifest.LastUpdate) baseManifest.LastUpdate = asset.CreatedAt.ToUnixTimeSeconds();
                    baseManifest.Changelog ??= ver.Body;
                }
            }
            baseManifest.IsTestingExclusive = !mainVersions.Any();
            baseManifest.DownloadCount = cnt;
            baseManifest.AcceptsFeedback = false;
            var content = github.Repository.Content.GetAllContents(data.Owner, data.RepoName).Result;
            foreach (var item in content)
            {
                if(item.Type == ContentType.Dir && item.Name.Equals("images", StringComparison.OrdinalIgnoreCase))
                {
                    var imageDir = github.Repository.Content.GetAllContents(data.Owner, data.RepoLink, item.Path).Result;
                    foreach(var image in imageDir)
                    {
                        if(image.Name.Equals("icon.png", StringComparison.OrdinalIgnoreCase))
                        {
                            baseManifest.IconUrl = image.DownloadUrl;
                            Console.WriteLine($"Found icon: {image.DownloadUrl}");
                        }
                        else if(image.Name.StartsWith("image", StringComparison.OrdinalIgnoreCase) && image.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        {
                            baseManifest.ImageUrls ??= [];
                            baseManifest.ImageUrls.Add(image.DownloadUrl);
                            Console.WriteLine($"Found image: {image.DownloadUrl}");
                        }
                    }
                }
            }

            pluginmaster.Add(baseManifest);
        }

        File.WriteAllText("pluginmaster.json", JsonConvert.SerializeObject(pluginmaster, Formatting.Indented));

        var rateLimit = github.RateLimit.GetRateLimits().Result;
        Console.WriteLine($"Rate limit remains: {rateLimit.Resources.Core.Remaining}");
    }
}
