namespace Keiser.MvxPlugins.BikeReceiver.Droid
{
    using Android.App;
    using Android.Content;
    using Android.Hardware.Usb;
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class DeviceManager : BroadcastReceiver
    {
        const string ACTION_USB_PERMISSION = "com.keiser.mvxplugins.bikereceiver.droid.USB_PERMISSION";
        protected Context Context = Android.App.Application.Context;
        protected UsbManager UsbManager;
        protected PendingIntent PermissionIntent;

        protected UsbDevice UsbDevice;
        protected UsbDeviceConnection UsbDeviceConnection;
        protected UsbInterface ControlInterface;
        protected UsbInterface DataInterface;
        protected UsbEndpoint ControlEndpoint;
        protected UsbEndpoint ReadEndpoint;
        protected UsbEndpoint WriteEndpoint;

        protected volatile bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            protected set { _isConnected = value; }
        }

        public DeviceManager()
        {
            UsbManager = (UsbManager)Context.GetSystemService(Context.UsbService);
        }

        public void FindDevice()
        {
            foreach (KeyValuePair<string, UsbDevice> deviceEntry in UsbManager.DeviceList)
            {
                UsbDevice device = deviceEntry.Value;
                if (device.VendorId == 0x0483 && device.ProductId == 0x5740)
                {
#if DEBUG
                    Trace.Info("Usb Receiver Connected");
#endif

                    if (UsbManager.HasPermission(device))
                    {
#if DEBUG
                        Trace.Info("Usb Receiver Permission Already Granted");
#endif
                        OpenDevice(device);
                    }
                    else
                    {
                        PermissionIntent = PendingIntent.GetBroadcast(Context, 0, new Intent(ACTION_USB_PERMISSION), PendingIntentFlags.CancelCurrent);
                        IntentFilter filter = new IntentFilter(ACTION_USB_PERMISSION);
                        Context.RegisterReceiver(this, filter);
                        UsbManager.RequestPermission(device, PermissionIntent);
                    }
                }
            }
        }

        public override void OnReceive(Context context, Intent intent)
        {
            switch (intent.Action)
            {
                case ACTION_USB_PERMISSION:
                    PermissionReceived(intent);
                    break;
            }
        }

        protected void PermissionReceived(Intent intent)
        {
            UsbDevice device = (UsbDevice)intent.GetParcelableExtra(UsbManager.ExtraDevice);
            if (intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false))
            {
#if DEBUG
                Trace.Info("Usb Receiver Permission Granted");
#endif
                OpenDevice(device);
            }
        }

        protected void OpenDevice(UsbDevice device)
        {
            UsbDeviceConnection = UsbManager.OpenDevice(device);
            try
            {
                ControlInterface = device.GetInterface(0);
                if (!UsbDeviceConnection.ClaimInterface(ControlInterface, true))
                {
                    throw new Exception("Could not claim control interface");
                }
                ControlEndpoint = ControlInterface.GetEndpoint(0);

                DataInterface = device.GetInterface(1);
                if (!UsbDeviceConnection.ClaimInterface(DataInterface, true))
                {
                    throw new Exception("Could not claim data interface");
                }
                ReadEndpoint = DataInterface.GetEndpoint(1);
                WriteEndpoint = DataInterface.GetEndpoint(0);
                UsbDevice = device;
                IsConnected = true;
#if DEBUG
                Trace.Info("Usb Receiver Ready");
#endif
            }
            catch (Exception e)
            {
                Trace.Error(e.Message);
            }
        }

        protected byte[] buffer = new byte[4096];
        public byte[] Read()
        {
            try
            {
                int numBytesRead;
                numBytesRead = UsbDeviceConnection.BulkTransfer(ReadEndpoint, buffer, buffer.Length, 1000);

                if (numBytesRead <= 0)
                {
                    numBytesRead = 0;
                }
                byte[] destBuffer = new byte[numBytesRead];
                Array.Copy(buffer, destBuffer, numBytesRead);
                return destBuffer;
            }
            catch (Exception e)
            {
                Trace.Error(e.Message);
                return new byte[0];
            }
        }
    }
}