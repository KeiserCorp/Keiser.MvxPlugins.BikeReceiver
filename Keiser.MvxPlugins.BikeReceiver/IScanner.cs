namespace Keiser.MvxPlugins.BikeReceiver
{
    public interface IScanner
    {
        bool IsConnected { get; }
        bool IsScanning { get; }
        void StartScan(IScanCallback scanCallback);
        void StopScan();
    }
}
