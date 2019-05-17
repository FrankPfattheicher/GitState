using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace GitState
{
    internal static class Updater
    {

        public static List<RepositoryState> GetRepositories(IEnumerable<string> baseFolders)
        {
            var repoStates = GetRepoStates(baseFolders);
            return repoStates;
        }

        private static List<RepositoryState> GetRepoStates(IEnumerable<string> baseFolders)
        {
            var repoStates = new List<RepositoryState>();
            
            foreach (var baseFolder in baseFolders)
            {
                foreach (var directory in Directory.EnumerateDirectories(baseFolder))
                {
                    try
                    {
                        var repo = new LibGit2Sharp.Repository(directory);
                        var info = repo.Info;
                        var state = new RepositoryState(repo, Path.GetFileName(directory));
                        repoStates.Add(state);
                    }
                    catch (RepositoryNotFoundException)
                    {
                        // not a repo
                    }
                    
                }
            }

            return repoStates;
        }


    }
}
