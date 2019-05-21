using System;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.ViewModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IctBaden.Framework.IniFile;

// ReSharper disable UnusedAutoPropertyAccessor.Local

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

        public string StateText { get; set; }

        private readonly Timer _updater;
        private int _updating;
        private Task[] _updateTasks;
        private readonly CancellationTokenSource _cancelUpdates;

        public MainVm(AppSession session) : base(session)
        {
            _cancelUpdates = new CancellationTokenSource();
            _updater = new Timer(Update, this, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60));
        }

        public void Dispose()
        {
            _cancelUpdates?.Cancel();
            _updater?.Dispose();
        }
        
        private void Update(object state)
        {
            if (Program.Repositories == null)
            {
                Program.Repositories = Updater.GetRepositories(Program.Settings.RepositoryFolders);
            }

            if (_updating > 0) return;
            
            _updating = Program.Repositories.Count();

            void Update(RepoState r)
            {
                r.UpdateState();
                NotifyPropertyChanged(nameof(Repos));
                _updating--;
            }
            _updateTasks = Program.Repositories
                .Select(r => Task.Run(() => Update(r), _cancelUpdates.Token))
                .ToArray();

            if (Program.Repositories.Count == 0)
            {
                if (!File.Exists(Profile.LocalToExeFileName))
                {
                    StateText = "No configuration file found<br/>"
                                + Profile.LocalToExeFileName;
                }
                else if (Program.Settings.RepositoryFolders.Count == 0)
                {
                    StateText = "No repository folders specified in<br/>"
                                + Profile.LocalToExeFileName;
                }
                else
                {
                    StateText = "No repositories found in <br/>"
                                + string.Join("<br/>", Program.Settings.RepositoryFolders);
                }
            }
            NotifyPropertyChanged(nameof(StateText));
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