namespace Keiser.MvxPlugins.BikeReceiver.Droid
{
    using Cirrious.CrossCore;
    using Cirrious.CrossCore.Plugins;
    using Keiser.MvxPlugins.BikeReceiver.Droid;
    using Keiser.MvxPlugins.BikeReceiver;

    public class Plugin
        : IMvxPlugin
    {
        public void Load()
        {
            Mvx.RegisterType<IScanner, Scanner>();
        }
    }
}
