using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.ViewModel;
using System.Collections.Generic;
using System.Linq;

namespace GitState.ViewModels
{
    public class MainVm : ActiveViewModel
    {
        public List<RepoVm> RepoStates => Program.Repositories
            .Select(r => new RepoVm(r))
            .ToList();

        public MainVm(AppSession session) : base(session)
        {
            if (Program.Repositories == null)
            {
                Program.Repositories = Updater.GetRepositories(Program.Settings.RepositoryFolders);
            }
        }
        
        
        
    }
}