using System;
using IctBaden.Stonehenge3.Core;
using IctBaden.Stonehenge3.ViewModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IctBaden.Framework.IniFile;
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedAutoPropertyAccessor.Local

// ReSharper disable UnusedMember.Global

namespace GitState.ViewModels
{
    public class MainVm : ActiveViewModel, IDisposable
    {
        // ReSharper disable once MemberCanBeMadeStatic.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public List<RepoVm> Repos => Program.Repositories?
            .Select(r => new RepoVm(r))
            .ToList();


        // ReSharper disable once MemberCanBeMadeStatic.Global
        public int FontSize => Program.Settings.FontSize;

        public string StateMessage { get; private set; }
        [DependsOn(nameof(StateMessage))]
        public bool HasStateMessage => !string.IsNullOrEmpty(StateMessage);

        private Timer _updater;
        private Task[] _updateTasks;
        private readonly CancellationTokenSource _cancelUpdates;

        public MainVm(AppSession session) : base(session)
        {
            Trace.TraceInformation($"new MainVm({session.Id})");
            StateMessage = "loading...";
            _cancelUpdates = new CancellationTokenSource();
            _updater = new Timer(Update, this, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(60));
        }

        public void Dispose()
        {
            Trace.TraceInformation($"delete MainVm({Session.Id})");
            _cancelUpdates?.Cancel();
            _updater?.Dispose();
            _updater = null;
        }
        
        private void Update(object state)
        {
            if (_updater == null) return;
            
            if (Program.Repositories == null)
            {
                NotifyPropertyChanged(nameof(FontSize));
                Trace.TraceInformation("Collect repositories...");
                Program.Repositories = Updater.GetRepositories(Program.Settings.RepositoryFolders);
            }

            Trace.TraceInformation($"Start state updates {Session.Id}..");
            var updates = 0;
            // State message
            StateMessage = "";
            if (Program.Repositories.Count == 0)
            {
                if (!File.Exists(Profile.LocalToExeFileName))
                {
                    StateMessage = "No configuration file found<br/>"
                                + Profile.LocalToExeFileName;
                }
                else if (Program.Settings.RepositoryFolders.Count == 0)
                {
                    StateMessage = "No repository folders specified in<br/>"
                                + Profile.LocalToExeFileName;
                }
                else
                {
                    StateMessage = "No repositories found in <br/>"
                                + string.Join("<br/>", Program.Settings.RepositoryFolders);
                }
            }
            NotifyPropertyChanged(nameof(StateMessage));
            
            void Update(RepoState r)
            {
                if (r.IsUpdating) return; 
                if ((DateTime.Now - r.LastUpdate).TotalSeconds <= Program.Settings.UpdateIntervalSec) return;
                
                NotifyPropertyChanged(nameof(Repos));
                r.UpdateState();
                NotifyPropertyChanged(nameof(Repos));
                updates++;
            }
            
            _updateTasks = Program.Repositories
                .Select(r => Task.Run(() => Update(r), _cancelUpdates.Token))
                .ToArray();

            Task.WaitAll(_updateTasks);
            Trace.TraceInformation($"{updates} states updated.");
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