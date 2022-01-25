using IctBaden.Framework.IniFile;
using IctBaden.Stonehenge3.Hosting;

namespace GitState;

public class AppCommands : IStonehengeAppCommands
{
    private readonly Settings _settings;

    public AppCommands(Settings settings)
    {
        _settings = settings;
    }
    
    public void WindowResized(int width, int height)
    {
        _settings.WindowWidth = width;
        _settings.WindowHeight = height;

        var profile = new Profile(_settings.FileName);
        profile["Settings"].Set("WindowWidth", width);
        profile["Settings"].Set("WindowHeight", height);
    }
}