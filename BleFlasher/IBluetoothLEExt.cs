using Plugin.BLE.Abstractions.Contracts;

#if __ANDROID__
using Android;
using Android.Bluetooth;
using AndroidX.Core.App;
#endif

namespace BleFlasher
{
    public static class IBluetoothLEExt
    {
        static public bool RequestPermissions(this IBluetoothLE ible)
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

        static public bool RequestEnable(this IBluetoothLE ible)
        {

#if __ANDROID__
            BluetoothManager bluetoothManager = (BluetoothManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.BluetoothService);
            bluetoothManager.Adapter.Enable();
#endif
            return ible.IsOn;
        }
    }
}
