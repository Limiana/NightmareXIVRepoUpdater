using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightmareXIVRepoUpdater;
internal class RepoMetadata
{
    public virtual string RepoName { get; }
    public virtual string RepoLink { get; }
    public virtual string JsonMetaLink { get; }

    public RepoMetadata(string repoName, string? projectName = null, string mainBranchName = "master")
    {
        this.RepoName = repoName;
        projectName ??= repoName;
        this.RepoLink = $"https://github.com/NightmareXIV/{repoName}";
        this.JsonMetaLink = $"https://github.com/NightmareXIV/{repoName}/raw/{mainBranchName}/{projectName}/{projectName}.json";
    }
}
