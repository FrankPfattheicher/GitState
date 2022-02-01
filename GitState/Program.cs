using System;
using System.Diagnostics;
using System.IO;
using IctBaden.Framework.AppUtils;
using IctBaden.Framework.IniFile;
using IctBaden.Framework.Logging;
using IctBaden.Stonehenge3.App;
using IctBaden.Stonehenge3.Hosting;
using Microsoft.Extensions.Logging;

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


            var ui = new StonehengeUi(logger, options);
            ui.Services.AddService(typeof(Settings), Settings);
            if (!ui.Start())
            {
                ui.Logger.LogCritical("Failed to start application");
                Environment.Exit(1);
            }

            var wnd = new HostWindow(ui.Server.BaseUrl, "GitState", new Point(Settings.WindowWidth, Settings.WindowHeight));
            if(!wnd.Open())
            {
                ui.Logger.LogCritical("Failed to open application window");
                Environment.Exit(1);
            }
            
            Console.WriteLine("GitState done.");
            Environment.Exit(0);
        }
    }
}
