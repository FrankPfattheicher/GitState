using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitState.ViewModels
{
    public class RepoVm
    {
        private readonly RepositoryState _repo;

        public RepoVm(RepositoryState repo)
        {
            _repo = repo;
        }

        public string Name => _repo.Name;
        
        public string State => "✓";
        public string Color => "cadetblue";


    }
}
