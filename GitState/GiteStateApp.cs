using Chromely.Core;
using Chromely.Core.Host;

namespace GitState
{
    public class GiteStateApp : ChromelyApp
    {
        public override void RegisterEvents(IChromelyContainer container)
        {
            base.Configure(container);
            container.RegisterSingleton(typeof(IChromelyWindow), typeof(IChromelyWindow).Name, typeof(IChromelyWindow));
        }

        public override IChromelyWindow CreateWindow()
        {
            return (IChromelyWindow)Container.GetInstance(typeof(IChromelyWindow), typeof(IChromelyWindow).Name);
        }
    }
}