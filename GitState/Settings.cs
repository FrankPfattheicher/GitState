using System.Collections.Generic;

namespace GitState
{
    internal class Settings
    {
        public int UpdateIntervalSec { get; set; } = 5 * 60;
        public int FontSize { get; set; } = 11;
        public List<string> RepositoryFolders { get; set; } = new List<string> { @"C:\ICT Baden" };
    }
}

