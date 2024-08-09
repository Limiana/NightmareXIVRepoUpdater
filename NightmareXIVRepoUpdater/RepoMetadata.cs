using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightmareXIVRepoUpdater;
internal class RepoMetadata
{
    public virtual string Owner => "NightmareXIV";
    public virtual string RepoName { get; }
    public virtual string ProjectName { get; }
    public virtual string ProjectFolderName { get; }
    public virtual string RepoLink { get; }
    public virtual string JsonMetaLink { get; }

    public RepoMetadata(string repoName, string? projectName = null, string mainBranchName = "master", string? projectFolderName = null)
    {
        this.RepoName = repoName;
        this.ProjectName = projectName ?? repoName;
        this.ProjectFolderName = projectFolderName ?? ProjectName;
        this.RepoLink = $"https://github.com/NightmareXIV/{repoName}";
        this.JsonMetaLink = $"https://github.com/NightmareXIV/{repoName}/raw/{mainBranchName}/{ProjectFolderName}/{ProjectName}.json";
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public virtual async Task PostBuildAction(PluginManifest manifest) {  }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
