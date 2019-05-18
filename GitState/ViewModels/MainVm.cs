using System;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
// ReSharper disable UnusedMember.Global

namespace GitState.ViewModels
{
    public class MainVm : ActiveViewModel, IDisposable
    {
        // ReSharper disable once MemberCanBeMadeStatic.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public List<RepoVm> Repos => Program.Repositories
            .Select(r => new RepoVm(r))
            .ToList();


        public int FontSize => Program.Settings.FontSize;

        private readonly Timer _updater;
        private bool _initialUpdate = true;
        private bool _updating;
        private readonly CancellationTokenSource _cancelUpdates;
        
        public MainVm(AppSession session) : base(session)
        {
            if (Program.Repositories == null)
            {
                Program.Repositories = Updater.GetRepositories(Program.Settings.RepositoryFolders);
            }
            
            _cancelUpdates = new CancellationTokenSource();
            _updater = new Timer(Update, this, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60));
        }

        public void Dispose()
        {
            _cancelUpdates.Cancel();
            _updater?.Dispose();
        }
        
        private void Update(object state)
        {
            if (_updating) return;
            _updating = true;

            if (Updater.UpdateRepoStates(Program.Repositories, _cancelUpdates.Token) > 0 || _initialUpdate)
            {
                _initialUpdate = false;
                NotifyPropertyChanged(nameof(Repos));                
            }
            
            _updating = false;
        }

        [ActionMethod]
        public void SelectRepo(string name)
        {
            foreach (var repo in Program.Repositories)
            {
                repo.Selected = repo.Name == name;
            }
            NotifyPropertyChanged(nameof(Repos));
        }
        
    }
}