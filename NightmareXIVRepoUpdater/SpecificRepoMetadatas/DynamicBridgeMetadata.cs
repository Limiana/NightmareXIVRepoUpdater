using Newtonsoft.Json;
using Octokit.Internal;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightmareXIVRepoUpdater.SpecificRepoMetadatas;
internal class DynamicBridgeMetadata : RepoMetadata
{
    public DynamicBridgeMetadata(string repoName, string? projectName = null, string mainBranchName = "master") : base(repoName, projectName, mainBranchName)
    {
    }

    public override async Task PostBuildAction(PluginManifest manifest)
    {
        var pluginmasterOwner = "Limiana";
        var pluginmasterRepo = "DynamicBridgeStandalone";
        var targetFile = "pluginmaster.json";

        var dbCredentials = new InMemoryCredentialStore(new(Program.Updater.SecretsProvider.DynamicBridgeStandaloneWriteKey, AuthenticationType.Bearer));
        var dbClient = new GitHubClient(new ProductHeaderValue("NightmareXIVRepoUpdater"), dbCredentials);

        var manifestClone = JsonConvert.DeserializeObject<PluginManifest>(JsonConvert.SerializeObject(manifest));
        manifestClone.RepoUrl = "https://github.com/Limiana/DynamicBridgeStandalone";

        try
        {
            // try to get the file (and with the file the last commit sha)
            var existingFile = await dbClient.Repository.Content.GetAllContents(pluginmasterOwner, pluginmasterRepo, targetFile);
            var existingManifest = JsonConvert.DeserializeObject<List<PluginManifest>>(existingFile[0].Content)[0];
            if (!existingManifest.AreChangesSignificant(manifest))
            {
                Console.WriteLine($"Changes insignificant. Will not update DB standalone.");
                return;
            }
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine($"Not found: {ex.Message}");
        }

        await Updater.WritePluginmaster(dbClient, [manifestClone], pluginmasterOwner, pluginmasterRepo, targetFile);
    }
}
