using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace GitState;

internal class Updater
{
    private readonly Settings _settings;

    public Updater(Settings settings)
    {
        _settings = settings;
    }

    public  List<RepoState> GetRepositories(IEnumerable<string> baseFolders)
    {
        var repoStates = GetRepoStates(baseFolders);
        return repoStates.OrderBy(r => r.Name).ToList();
    }

    private List<RepoState> GetRepoStates(IEnumerable<string> baseFolders)
    {
        var repos = new List<RepoState>();
            
        foreach (var baseFolder in baseFolders)
        {
            if (!Directory.Exists(baseFolder))
            {
                Trace.TraceWarning("Repository base folder does not exist: " + baseFolder);
                continue;
            }
                
            foreach (var directory in Directory.EnumerateDirectories(baseFolder))
            {
                if(string.IsNullOrEmpty(directory)) continue;
                    
                try
                {
                    Trace.TraceInformation("Get repo state of " + directory);
                    var repo = new Repository(directory);
                    //var info = repo.Info;
                    var status = new RepoState(repo, _settings, Path.GetFileName(directory));
                    repos.Add(status);
                }
                catch (RepositoryNotFoundException)
                {
                    // not a repo
                }
                    
            }
        }

        return repos;
    }

}