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
        var dbCredentials = new InMemoryCredentialStore(new(Program.Updater.SecretsProvider.DynamicBridgeStandaloneWriteKey, AuthenticationType.Bearer));
        var dbClient = new GitHubClient(new ProductHeaderValue("NightmareXIVRepoUpdater"), dbCredentials);

        var manifestClone = JsonConvert.DeserializeObject<PluginManifest>(JsonConvert.SerializeObject(manifest));
        manifestClone.RepoUrl = "https://github.com/Limiana/DynamicBridgeStandalone";

        await Updater.WritePluginmaster(dbClient, [manifestClone], "Limiana", "DynamicBridgeStandalone", "pluginmaster.json");
    }
}
