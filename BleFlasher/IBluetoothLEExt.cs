
#if __ANDROID__
using Android;
using Android.Bluetooth;
using AndroidX.Core.App;
#endif

namespace BleFlasher
{
    public static class BluetoothLEExt
    {
        static public bool RequestPermissions()
        {
            try
            {
#if __ANDROID__

                string[] permlist = {
                    Manifest.Permission.AccessFineLocation,
                    Manifest.Permission.Bluetooth,
                    Manifest.Permission.BluetoothAdmin,
                    Manifest.Permission.Internet,
                    Manifest.Permission.ReadExternalStorage,
                    Manifest.Permission.AccessNetworkState };

                ActivityCompat.RequestPermissions(Platform.CurrentActivity, permlist, 1);
#endif

#if WINDOWS

#endif
            }
            catch (Exception ex)
            {

            }

            return true;
        }

        static public bool RequestEnable()
        {

#if __ANDROID__
            BluetoothManager bluetoothManager = (BluetoothManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.BluetoothService);
            bluetoothManager.Adapter.Enable();
            return bluetoothManager.Adapter.IsEnabled;
#endif
            return true;
        }
    }
}
