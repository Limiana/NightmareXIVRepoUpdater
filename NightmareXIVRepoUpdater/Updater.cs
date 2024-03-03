using Newtonsoft.Json;
using NightmareXIVRepoUpdater.SpecificRepoMetadatas;
using Octokit;
using Octokit.Internal;

namespace NightmareXIVRepoUpdater;
internal class Updater
{
    List<RepoMetadata> Meta = [
        new DynamicBridgeMetadata("DynamicBridge", null, "main"),
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
        ];

    public SecretsProvider SecretsProvider;

    public Updater(string[] args)
    {
        SecretsProvider = new();
    }

    public async Task Run()
    {
        var roCredentials = new InMemoryCredentialStore(new(SecretsProvider.ReadOnlyKey, AuthenticationType.Bearer));
        var nightmareXivCredentials = new InMemoryCredentialStore(new(SecretsProvider.NightmareXIVWriteKey, AuthenticationType.Bearer));
        var github = new GitHubClient(new ProductHeaderValue("NightmareXIVRepoUpdater"), roCredentials);
        var nightmareXIVPluginmaster = new GitHubClient(new ProductHeaderValue("NightmareXIVRepoUpdater"), nightmareXivCredentials);

        List<PluginManifest> pluginmaster = [];

        foreach (var data in Meta)
        {
            var releases = await github.Repository.Release.GetAll(data.Owner, data.RepoName);
            var cnt = releases.Sum(x => x.Assets.FindSuitableAssets().Sum(a => a.DownloadCount));
            Console.WriteLine($"{data.RepoName}, downloads={cnt}");
            var mainVersions = releases.Where(x => !x.Draft && !x.Prerelease && x.Assets.FindSuitableAsset() != null).OrderByDescending(x => x.CreatedAt);
            var testingVersions = releases.Where(x => !x.Draft && x.Prerelease && x.Assets.FindSuitableAsset() != null).OrderByDescending(x => x.CreatedAt);
            //Console.WriteLine($"{string.Join("\n", releases.Select(x => x.TagName))}");
            PluginManifest? baseManifest = null;
            if (mainVersions.Any())
            {
                var ver = mainVersions.First();
                var asset = ver.Assets.FindSuitableAsset();
                if (asset != null)
                {
                    Console.WriteLine($"  Latest release: {ver.TagName} at {asset.BrowserDownloadUrl}");
                    Console.WriteLine($"  Changelog: \n{ver.Body}");
                    var response = await github.Connection.GetRawStream(new(asset.BrowserDownloadUrl), new Dictionary<string, string>());
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
                    var response = await github.Connection.GetRawStream(new(asset.BrowserDownloadUrl), new Dictionary<string, string>());
                    var manifest = Utils.GetMainfest(response.Body);
                    baseManifest ??= manifest;
                    baseManifest.DownloadLinkTesting = asset.BrowserDownloadUrl;
                    baseManifest.TestingAssemblyVersion = manifest.AssemblyVersion;
                    if (asset.CreatedAt.ToUnixTimeSeconds() > baseManifest.LastUpdate) baseManifest.LastUpdate = asset.CreatedAt.ToUnixTimeSeconds();
                    if(baseManifest.AssemblyVersion < manifest.AssemblyVersion) baseManifest.Changelog = ver.Body;
                }
            }
            baseManifest.IsTestingExclusive = !mainVersions.Any();
            baseManifest.DownloadCount = cnt;
            baseManifest.AcceptsFeedback = false;
            try
            {
                var content = await github.Repository.Content.GetAllContents(data.Owner, data.RepoName, $"{data.ProjectName}/images");
                foreach (var image in content)
                {
                    if (image.Name.Equals("icon.png", StringComparison.OrdinalIgnoreCase))
                    {
                        baseManifest.IconUrl = image.DownloadUrl;
                        Console.WriteLine($"Found icon: {image.DownloadUrl}");
                    }
                    else if (image.Name.StartsWith("image", StringComparison.OrdinalIgnoreCase) && image.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        baseManifest.ImageUrls ??= [];
                        baseManifest.ImageUrls.Add(image.DownloadUrl);
                        Console.WriteLine($"Found image: {image.DownloadUrl}");
                    }
                }
            }
            catch (NotFoundException ex)
            {
                Console.WriteLine($"Image folder not found {ex}");
            }

            await data.PostBuildAction(baseManifest);

            pluginmaster.Add(baseManifest);
        }

        await WritePluginmaster(nightmareXIVPluginmaster, pluginmaster, "NightmareXIV", "MyDalamudPlugins", "pluginmaster.json");

        var rateLimit = await github.RateLimit.GetRateLimits();
        Console.WriteLine($"Rate limit remains: {rateLimit.Resources.Core.Remaining}");
    }

    public static async Task WritePluginmaster(GitHubClient client, IEnumerable<PluginManifest> pluginmaster, string pluginmasterOwner, string pluginmasterRepo, string targetFile, string? writeFilename = null)
    {
        var pluginmasterJson = JsonConvert.SerializeObject(pluginmaster, Formatting.Indented, new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new PluginManifestContractResolver(),
        });

        if(writeFilename != null) File.WriteAllText(writeFilename, pluginmasterJson);

        try
        {
            // try to get the file (and with the file the last commit sha)
            var existingFile = await client.Repository.Content.GetAllContents(pluginmasterOwner, pluginmasterRepo, targetFile);

            // update the file
            Console.WriteLine($"sha {existingFile[0].Sha}");
            var updateChangeSet = await client.Repository.Content.UpdateFile(pluginmasterOwner, pluginmasterRepo, targetFile,
               new UpdateFileRequest("[API] Pluginmaster autoupdate", pluginmasterJson, existingFile[0].Sha));
            Console.WriteLine($"Created commit {updateChangeSet.Commit.Sha}");
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine(ex.ToString());
            // if file is not found, create it
            var createChangeSet = await client.Repository.Content.CreateFile(pluginmasterOwner, pluginmasterRepo, targetFile, new CreateFileRequest("[API] Pluginmaster autocreate", pluginmasterJson));
        }
    }
}
