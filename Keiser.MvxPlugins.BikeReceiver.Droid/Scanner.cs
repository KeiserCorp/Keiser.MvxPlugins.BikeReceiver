namespace Keiser.MvxPlugins.BikeReceiver.Droid
{
    using Keiser.MvxPlugins.BikeReceiver;

    public class Scanner : Java.Lang.Object, IScanner
    {
        public bool IsConnected
        {
            get { return DeviceManager.IsConnected; }
        }

        protected volatile bool _isScanning;
        public bool IsScanning
        {
            get { return _isScanning; }
            protected set { _isScanning = value; }
        }

        protected DeviceManager DeviceManager = new DeviceManager();

        public Scanner()
        {
            DeviceManager.FindDevice();
        }

        public void StartScan(IScanCallback scanCallback)
        {
            if (IsConnected)
            {
                IsScanning = true;
                while (IsScanning)
                {
                    scanCallback.ScanCallback(DeviceManager.Read());
                }
            }
        }

        public void StopScan()
        {
            IsScanning = false;
        }
    }
}