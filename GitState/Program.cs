using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Chromely.CefGlue;
using Chromely.Core;
using Chromely.Core.Host;
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

        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
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
            var startUrl = host.BaseUrl;

            var config = ChromelyConfiguration
                .Create()
                .WithLoadingCefBinariesIfNotFound(true)
                .WithSilentCefBinariesLoading(true)
                // ReSharper disable once RedundantArgumentDefaultValue
                .WithHostMode(WindowState.Normal)
                .WithDefaultSubprocess()
                .WithHostTitle(options.Title)
                .WithHostIconFile("GitState.ico")
                .WithAppArgs(args)
                .WithHostBounds(250, 800)
                .RegisterCustomerUrlScheme("http", "localhost")
                .WithStartUrl(startUrl);

            using (var window = ChromelyWindow.Create(config))
            {
                var exitCode = window.Run(args);
                if (exitCode != 0)
                {
                    Console.WriteLine("Failed to start chromely frontend: code " + exitCode);
                }
            }
            
            Console.WriteLine("Demo done.");
        }
    }
}
