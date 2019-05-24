using System.Diagnostics;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedMember.Global

namespace GitState.ViewModels
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class RepoVm
    {
        private readonly RepoState _repoState;

        public bool Selected => _repoState.Selected;
        public string Name => _repoState.Name;
        public string Branch => _repoState.Branch;

        public bool IsUpdating => _repoState.IsUpdating;
        
        public string StateText { get; private set; }
        public string TextColor { get; private set; }
        public string StateColor { get; private set; }
        public string ShortDescription { get; private set; }

        public RepoVm(RepoState repoState)
        {
            _repoState = repoState;
            ShortDescription = _repoState.IsUnknown ? "<unknown>" : _repoState.ShortText;

            if (_repoState.IsUnknown)
            {
                StateText = "?";
                StateColor = "gray";
                TextColor = "white";
                return;
            }

            if (_repoState.IsFailed)
            {
                StateText = "?";
                StateColor = "black";
                TextColor = "gray";
                return;
            }

            // What could be happened ?
            if (_repoState.IsClean)
            {
                if (_repoState.AheadBy > 0)
                {
                    StateText = $"+{_repoState.AheadBy.ToString()}";
                    StateColor = "blue";
                    TextColor = "white";
                    return;
                }

                if (_repoState.BehindBy > 0)
                {
                    StateText = $"-{_repoState.BehindBy.ToString()}";
                    StateColor = "yellow";
                    TextColor = "black";
                    return;
                }
            }

            if (_repoState.ModifiedCount > 0)
            {
                if (_repoState.BehindBy > 0)
                {
                    // During you work - thi is NOT fine   
                    StateText = "❌";
                    StateColor = "tomato";
                    TextColor = "yellow";
                    return;
                }

                StateText = $"~{_repoState.ModifiedCount.ToString()}";
                StateColor = "orange";
                TextColor = "white";
                return;
            }

            if (_repoState.UntrackedCount > 0)
            {
                StateText = $"+{_repoState.UntrackedCount.ToString()}";
                StateColor = "slateblue";
                TextColor = "white";
                return;
            }

            // everything fine
            StateText = "✓";
            StateColor = "darkolivegreen";
            TextColor = "white";
        }
    }
}