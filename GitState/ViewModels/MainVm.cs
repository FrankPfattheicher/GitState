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
        private readonly Settings _settings;
        private List<RepoState> _repositories;

        // ReSharper disable once MemberCanBeMadeStatic.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public List<RepoVm> Repos => _repositories?
            .Select(r => new RepoVm(r))
            .ToList();


        // ReSharper disable once MemberCanBeMadeStatic.Global
        public int FontSize => _settings.FontSize;

        public string StateMessage { get; private set; }
        [DependsOn(nameof(StateMessage))]
        public bool HasStateMessage => !string.IsNullOrEmpty(StateMessage);
        public bool UpdateRunning { get; private set; }

        
        private Timer _updater;
        private Task[] _updateTasks;
        private CancellationTokenSource _cancelUpdates;

        public MainVm(AppSession session, Settings settings) 
            : base(session)
        {
            _settings = settings;
            Trace.TraceInformation($"new MainVm({session.Id})");
            StateMessage = "loading...";
            StartUpdates();
        }

        private void StartUpdates()
        {
            _cancelUpdates = new CancellationTokenSource();
            _updater = new Timer(_ => Update(), this, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60));
        }

        private void CancelUpdates()
        {
            _cancelUpdates?.Cancel();
            _updater?.Dispose();
            _updater = null;
        }
        
        public void Dispose()
        {
            Trace.TraceInformation($"delete MainVm({Session.Id})");
            CancelUpdates();
        }
        
        private void Update()
        {
            if (_updater == null || UpdateRunning) return;
            
            if (_repositories == null)
            {
                NotifyPropertyChanged(nameof(FontSize));
                StateMessage = "Collect repositories...";
                NotifyPropertyChanged(nameof(StateMessage));
                _repositories = Updater.GetRepositories(_settings.RepositoryFolders);
            }

            Trace.TraceInformation($"Start state updates {Session.Id}..");
            UpdateRunning = true;
            NotifyPropertyChanged(nameof(UpdateRunning));
            
            // State message
            StateMessage = "";
            if (_repositories.Count == 0)
            {
                if (!File.Exists(_settings.FileName))
                {
                    StateMessage = "No configuration file found<br/>"
                                   + _settings.FileName;
                }
                else if (_settings.RepositoryFolders.Count == 0)
                {
                    StateMessage = "No repository folders specified in<br/>"
                                   + Profile.LocalToExeFileName;
                }
                else
                {
                    StateMessage = "No repositories found in <br/>"
                                   + string.Join("<br/>", _settings.RepositoryFolders);
                }
            }
            NotifyPropertyChanged(nameof(StateMessage));

            var updates = 0;
            
            void UpdateRepo(RepoState r)
            {
                if (r.IsUpdating) return; 
                if ((DateTime.Now - r.LastUpdate).TotalSeconds <= _settings.UpdateIntervalSec) return;
                
                NotifyPropertyChanged(nameof(UpdateRunning));
                NotifyPropertyChanged(nameof(Repos));
                r.UpdateState(_settings);
                NotifyPropertyChanged(nameof(Repos));
                updates++;
            }
            
            _updateTasks = _repositories
                .Select(r => Task.Run(() => UpdateRepo(r), _cancelUpdates.Token))
                .ToArray();

            Task.WaitAll(_updateTasks);
            
            UpdateRunning = false;
            NotifyPropertyChanged(nameof(UpdateRunning));
            
            Trace.TraceInformation($"{updates} states updated.");
        }

        [ActionMethod]
        public void SelectRepo(string name)
        {
            foreach (var repo in _repositories)
            {
                repo.Selected = repo.Name == name;
            }
            NotifyPropertyChanged(nameof(Repos));
        }

        [ActionMethod]
        public void RefreshNow()
        {
            if (UpdateRunning) return;
            
            _repositories = null;
            CancelUpdates();
            StartUpdates();
        }
        
    }
}