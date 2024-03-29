﻿using System.Collections.Generic;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace GitState
{
    public class Settings
    {
        public string FileName = string.Empty;
        
        /// <summary>
        /// Interval in seconds the repository states are updated.
        /// </summary>
        public int UpdateIntervalSec { get; set; } = 5 * 60;
        /// <summary>
        /// Update repository state only using local available data.
        /// i.e. do not make a fetch from origin.
        /// </summary>
        public bool UseLocalStateOnly { get; set; }
        /// <summary>
        /// UI font size in pixel
        /// </summary>
        public int FontSize { get; set; } = 11;
        /// <summary>
        /// Semicolon separated list of folders to search for repo folders
        /// </summary>
        public List<string> RepositoryFolders { get; set; } = new();
        /// <summary>
        /// GIT user name or access token
        /// </summary>
        public string GitUserOrToken { get; set; } = string.Empty;
        /// <summary>
        /// GIT user's password or empty for usage with token
        /// </summary>
        public string GitPassword { get; set; } = string.Empty;

        public int WindowHeight { get; set; } = 800;
        public int WindowWidth { get; set; } = 250;
    }
}

