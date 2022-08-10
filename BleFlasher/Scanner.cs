
using InTheHand;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BleFlasher
{

    public partial class BleFlasher
    {
        List<InTheHand.Bluetooth.BluetoothDevice> devices;
        InTheHand.Bluetooth.BluetoothLEScan scan;  

        public async void startScanning()
        {
            devices = new List<InTheHand.Bluetooth.BluetoothDevice>();

            BluetoothLEExt.RequestPermissions();
            BluetoothLEExt.RequestEnable();

            InTheHand.Bluetooth.Bluetooth.AdvertisementReceived -= Bluetooth_AdvertisementReceived;
            InTheHand.Bluetooth.Bluetooth.AdvertisementReceived += Bluetooth_AdvertisementReceived;
            var options = new InTheHand.Bluetooth.BluetoothLEScanOptions();
            options.AcceptAllAdvertisements = true;
            options.KeepRepeatedDevices = true;
            options.Filters.Clear();
            scan = await InTheHand.Bluetooth.Bluetooth.RequestLEScanAsync(options);
           }

        private void Bluetooth_AdvertisementReceived(object sender, InTheHand.Bluetooth.BluetoothAdvertisingEvent e)
        {

            foreach (var s in e.Uuids)
            {

                if (s == BleFlasher.SERVICE_GUID)
                {

                    if (!devices.Contains(e.Device)) { 
                        Console.WriteLine("Device " + e.Device.Name + " found");
                        devices.Add(e.Device);
                        break;
                    }
                }
            }
        }

        public  void stopScanning()
        {
             scan.Stop();

        }

        public bool isScanning()
        {
            return scan.Active;

        }

        public List<InTheHand.Bluetooth.BluetoothDevice> GetDevices()
        {
            return devices;
        }
    }
}
