using System;
using System.Diagnostics;
using System.IO;
using IctBaden.Framework.AppUtils;
using IctBaden.Framework.IniFile;
using IctBaden.Framework.Logging;
using IctBaden.Stonehenge3.Hosting;
using IctBaden.Stonehenge3.Kestrel;
using IctBaden.Stonehenge3.Resources;
using IctBaden.Stonehenge3.Vue;

namespace GitState
{
    internal static class Program
    {
        private static readonly Settings Settings = new();

        // ReSharper disable once UnusedParameter.Local
        [STAThread]
        private static void Main(string[] args)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            var path = ApplicationInfo.ApplicationDirectory;
            Directory.SetCurrentDirectory(path);

            var settingsFile = new Profile(Path.Combine(path, "GitState.cfg"));
            new ProfileClassLoader().LoadClass(Settings, settingsFile);
            Settings.FileName = settingsFile.FileName; 
            
            // Starting stonehenge backend
            var options = new StonehengeHostOptions
            {
                Title = "GitState",
                StartPage = "main",
                ServerPushMode = ServerPushModes.LongPolling,
                PollIntervalSec = 10,
                HandleWindowResized = true
            };
            var logger = Logger.DefaultFactory.CreateLogger("GitState");
            var vue = new VueResourceProvider(logger);
            var provider = StonehengeResourceLoader.CreateDefaultLoader(logger, vue);
            provider.Services.AddService(typeof(Settings), Settings);
            
            var host = new KestrelHost(provider, options);
            if (!host.Start("localhost", 8880))
            {
                Console.WriteLine("Failed to start stonehenge server");
            }

            // Starting frontend
            Console.WriteLine("Starting frontend");
            var wnd = new HostWindow(host.BaseUrl, "GitState", new Point(Settings.WindowWidth, Settings.WindowHeight));
            if (!wnd.Open())
            {
                Console.WriteLine("GitState failed to open window");
                Environment.Exit(1);
            }
            
            Console.WriteLine("GitState done.");
        }
    }

}
