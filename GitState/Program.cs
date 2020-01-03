using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Chromely;
using Chromely.Core;
using IctBaden.Framework.IniFile;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Kestrel;
using IctBaden.Stonehenge3.Resources;
using IctBaden.Stonehenge3.Vue;

namespace GitState
{
    internal static class Program
    {
        public static readonly Settings Settings = new Settings();
        public static List<RepoState> Repositories;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static void HideWindow()
        {
            var hWnd = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(hWnd, 0);
        }

        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            if (ChromelyRuntime.Platform == ChromelyPlatform.Windows)
            {
                HideWindow();
            }
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            
            var path = AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(path);

            var settingsFile = new Profile(Profile.LocalToExeFileName);
            new ProfileClassLoader().LoadClass(Settings, settingsFile);
            
            // Starting stonehenge backend
            var options = new StonehengeHostOptions
            {
                Title = "GitState",
                StartPage = "main",
                ServerPushMode = ServerPushModes.LongPolling,
                PollIntervalMs = 30000
            };
            var provider = StonehengeResourceLoader
                .CreateDefaultLoader(new VueResourceProvider());
            var host = new KestrelHost(provider, options);
            if (!host.Start("localhost", 8880))
            {
                Console.WriteLine("Failed to start stonehenge server");
            }

            // Starting chromely frontend
            Console.WriteLine("Starting chromely frontend");
            var config = DefaultConfiguration.CreateOSDefault(ChromelyRuntime.Platform);
            config.StartUrl = host.BaseUrl;
            config.WindowHeight = Settings.WindowHeight;
            config.WindowWidth = Settings.WindowWidth;
            config.WindowIconFile = "GitState.ico";

            AppBuilder
                .Create()
                .UseApp<GitStateApp>()
                .UseConfiguration<IChromelyConfiguration>(config)
                .Build()
                .Run(args);
            
            Console.ReadLine();
            Console.WriteLine("GitState done.");
        }
    }

    class GitStateApp : BasicChromelyApp
    {
    }
    
}
