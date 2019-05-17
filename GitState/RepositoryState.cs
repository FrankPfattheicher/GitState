using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace GitState
{
    public class RepositoryState
    {
        public string Name { get; private set; }
        public string Path => _repo.Info.WorkingDirectory;

        private LibGit2Sharp.Repository _repo;
        public RepositoryState(Repository repo, string name)
        {
            _repo = repo;
            Name = name;
        }
    }
}
