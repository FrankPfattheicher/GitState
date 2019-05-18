using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace GitState
{
    internal static class Updater
    {

        public static List<RepoState> GetRepositories(IEnumerable<string> baseFolders)
        {
            var repoStates = GetRepoStates(baseFolders);
            return repoStates;
        }

        private static List<RepoState> GetRepoStates(IEnumerable<string> baseFolders)
        {
            var repos = new List<RepoState>();
            
            foreach (var baseFolder in baseFolders)
            {
                foreach (var directory in Directory.EnumerateDirectories(baseFolder))
                {
                    try
                    {
                        var repo = new Repository(directory);
                        var info = repo.Info;
                        var status = new RepoState(repo, Path.GetFileName(directory));
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

        public static int UpdateRepoStates(IEnumerable<RepoState> repos, CancellationToken cancel)
        {
            var updateTasks = repos.Select(r => Task.Run<int>((Func<int>) r.UpdateState, cancel)).ToArray();
            // ReSharper disable once CoVariantArrayConversion
            Task.WaitAll(updateTasks);
            return updateTasks.Sum(t => t.Result);
        }
    }
}
