using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LibGit2Sharp;
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace GitState
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class RepoState
    {
        public bool Selected { get; set; }
        public string Name { get; }
        public string Path => _repo.Info.WorkingDirectory;
        public string Branch => _repo.Head.FriendlyName;

        private readonly Repository _repo;
        private RepositoryStatus _status;

        public RepoState(Repository repo, string name)
        {
            _repo = repo;
            Name = name;
        }

        private static int GetChanges(IEnumerable<object> a, IEnumerable<object> b) => a.Count() != b.Count() ? 1 : 0;

        public int UpdateState()
        {
            RepositoryStatus status;
            try
            {
                status = _repo.RetrieveStatus(new StatusOptions());
            }
            catch (Exception ex)
            {
                LongText = ex.Message;
                IsFailed = true;
                return 1;
            }

            var changes = (_status == null)
                ? 0
                : GetChanges(status.Added, _status.Added)
                  + GetChanges(status.Staged, _status.Staged)
                  + GetChanges(status.Removed, _status.Removed)
                  + GetChanges(status.Untracked, _status.Untracked)
                  + GetChanges(status.Modified, _status.Modified)
                  + GetChanges(status.Missing, _status.Missing)
                  + GetChanges(status.Ignored, _status.Ignored);

            if (_status == null || changes > 0)
            {
                _status = status;
            }

            AddedCount = _status.Added.Count();
            StagedCount = _status.Staged.Count();
            RemovedCount = _status.Removed.Count();
            UntrackedCount = _status.Untracked.Count();
            ModifiedCount = _status.Modified.Count();
            MissingCount = _status.Missing.Count();

            var branch = _repo.Branches[Branch];
            var tracking = branch.TrackingDetails;
            AheadBy = tracking.AheadBy ?? 0;
            BehindBy = tracking.BehindBy ?? 0;

            LongText =
            ShortText =
                $"+{AddedCount} ~{StagedCount} -{RemovedCount} | +{UntrackedCount} ~{ModifiedCount} -{MissingCount} | i{IgnoredCount}";
            
            return changes;
        }

        public bool IsUnknown => _status == null;
        public bool IsFailed { get; private set; }

        public bool IsClean =>
            AddedCount +
            StagedCount +
            RemovedCount +
            UntrackedCount +
            ModifiedCount +
            MissingCount == 0;

        public int AheadBy { get; private set; }
        public int BehindBy { get; private set; }

        public int AddedCount { get; private set; }
        public int StagedCount { get; private set; }
        public int RemovedCount { get; private set; }
        public int UntrackedCount { get; private set; }
        public int ModifiedCount { get; private set; }
        public int MissingCount { get; private set; }
        public int IgnoredCount { get; private set; }

        public string ShortText { get; private set; }
        public string LongText { get; private set; }
    }
}