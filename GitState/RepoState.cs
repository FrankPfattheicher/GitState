using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.IO;
using System.Linq;
using System.Text;
using LibGit2Sharp;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace GitState;

[DebuggerDisplay("{" + nameof(Name) + "}")]
public class RepoState
{
    public bool Selected { get; set; }
    public string Name { get; }
    public string RepoPath => _repo.Info.WorkingDirectory;
    public string Branch => _repo.Head.FriendlyName;

    public bool IsUpdating { get; private set; }
    public DateTime LastUpdate { get; private set; }

    public bool IsUnknown => _status == null;
    public bool IsFailed { get; private set; }
    public bool IsSecurityFailure { get; private set; }

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

    public string ShortText { get; private set; } = string.Empty;
    public string LongText { get; private set; } = string.Empty;

    private readonly Repository _repo;
    private readonly Settings _settings;
    private RepositoryStatus? _status;

    public RepoState(Repository repo, Settings settings, string name)
    {
        _repo = repo;
        _settings = settings;
        Name = name;
        LastUpdate = DateTime.Now - TimeSpan.FromDays(1);
    }

    private static int GetChanges(IEnumerable<object> a, IEnumerable<object> b) => a.Count() != b.Count() ? 1 : 0;

    Credentials CredentialsHandler(string url, string user, SupportedCredentialTypes cred)
    {
        // if ((cred & SupportedCredentialTypes.Ssh) != 0)
        // {
        //     var sshDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
        //     return new SshUserKeyCredentials()
        //     {
        //         Username = _settings.GitUserOrToken,
        //         Passphrase = string.Empty,
        //         PublicKey = Path.Combine(sshDir, "id_rsa.pub"),
        //         PrivateKey = Path.Combine(sshDir, "id_rsa"),
        //     };
        // }

        if ((cred & SupportedCredentialTypes.Default) != 0)
        {
            return new DefaultCredentials();
        }

        if ((cred & SupportedCredentialTypes.UsernamePassword) != 0)
        {
            return new UsernamePasswordCredentials
            {
                Username = _settings.GitUserOrToken,
                Password = _settings.GitPassword
            };
        }

        return new DefaultCredentials();
    }

    private static bool CertificateCheck(Certificate certificate, bool valid, string host)
    {
        return true;
    }

    // ReSharper disable once UnusedMethodReturnValue.Global
    public int UpdateState(Settings settings)
    {
        if (IsUpdating) return 0;

        IsUpdating = true;
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var context = "";
        RepositoryStatus status;
        try
        {
            if (!settings.UseLocalStateOnly)
            {
                context = "Fetch";
                var remote = _repo.Network.Remotes["origin"];
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification).ToList();

                var options = new FetchOptions
                {
                    CredentialsProvider = CredentialsHandler,
                    CertificateCheck = CertificateCheck,
                    TagFetchMode = TagFetchMode.All
                };
                
                Commands.Fetch(_repo, remote.Name, refSpecs, options, null);
            }

            context = "RetrieveStatus";
            status = _repo.RetrieveStatus();
        }
        catch (Exception ex)
        {
            LongText = context + ": " + ex.Message;
            IsFailed = true;
            IsSecurityFailure = ex.Message.Contains("401");
            LastUpdate = DateTime.Now;
            IsUpdating = false;
            Trace.TraceError($"Repo {Name} FAILED TO UPDATE: {LongText}");
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

        var branch = _repo.Branches.FirstOrDefault(b => b.FriendlyName == Branch) ?? _repo.Head;
        var tracking = branch.TrackingDetails;
        AheadBy = tracking.AheadBy ?? 0;
        BehindBy = tracking.BehindBy ?? 0;

        IsFailed = false;
        IsSecurityFailure = false;

        var text = new StringBuilder();
        if(AddedCount > 0) text.AppendLine($"Added: {AddedCount}");
        if(StagedCount > 0) text.AppendLine($"Staged: {StagedCount}");
        if(RemovedCount > 0) text.AppendLine($"Removed: {RemovedCount}");
        if(UntrackedCount > 0) text.AppendLine($"Untracked: {UntrackedCount}");
        if(ModifiedCount > 0) text.AppendLine($"Modified: {ModifiedCount}");
        if(MissingCount > 0) text.AppendLine($"Missing: {MissingCount}");
        if(IgnoredCount > 0) text.AppendLine($"Ignored: {IgnoredCount}");
        
        if(AheadBy > 0) text.AppendLine($"{AheadBy} commits ahead");
        if(BehindBy > 0) text.AppendLine($"{BehindBy} commits behind");
        
        LongText = ShortText = text.ToString();
        
        stopwatch.Stop();
        Trace.TraceInformation($"Repo {Name} updated in {stopwatch.ElapsedMilliseconds / 1000.0}sec");

        LastUpdate = DateTime.Now;
        IsUpdating = false;

        return changes;
    }

}