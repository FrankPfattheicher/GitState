using System.Diagnostics;
// ReSharper disable MemberCanBePrivate.Global

namespace GitState.ViewModels
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class RepoVm
    {
        private readonly RepoState _repoState;

        public RepoVm(RepoState repoState)
        {
            _repoState = repoState;
        }

        public bool Selected => _repoState.Selected;
        public string Name => _repoState.Name;
        public string Branch => _repoState.Branch;        

        public string State
        {
            get
            {
                if (_repoState.IsUnknown) return "?";

                if (_repoState.UntrackedCount > 0) return "!";

                if (_repoState.BehindBy > 0) return _repoState.BehindBy.ToString();

                return _repoState.ModifiedCount == 0
                    ? "✓"
                    : _repoState.ModifiedCount.ToString();
            }
        }
        public string StateText =>
            Color == "orange" || Color == "yellow" || Color == "deepskyblue"
                ? "black" 
                : "white";

        public string Color
        {
            get
            {
                if (_repoState.IsUnknown) return "gray";
                if (_repoState.IsClean)
                {
                    if(_repoState.AheadBy > 0) return "deepskyblue";
                    if(_repoState.BehindBy > 0) return "yellow";
                    return "green";
                }
                if (_repoState.AddedCount > 0) return "deepskyblue";
                if (_repoState.ModifiedCount > 0)
                {
                    if(_repoState.BehindBy > 0) return "orange";
                    return "tomato";
                }
                if (_repoState.UntrackedCount > 0) return "darkmagenta";
                return "green";
            }
        }

        public string ShortText => _repoState.IsUnknown ? "<unknown>" : _repoState.ShortText;
    }
}