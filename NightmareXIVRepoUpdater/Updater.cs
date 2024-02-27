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
        ];

    public Updater(string[] args)
    {
        var secretsProvider = new SecretsProvider();
        var roCredentials = new InMemoryCredentialStore(new(secretsProvider.ReadOnlyKey, AuthenticationType.Basic));
        var nightmareXivCredentials = new InMemoryCredentialStore(new(secretsProvider.NightmareXIVWriteKey, AuthenticationType.Basic));
        var github = new GitHubClient(new ProductHeaderValue("NightmareXIVRepoUpdater"));

        
    }
}
