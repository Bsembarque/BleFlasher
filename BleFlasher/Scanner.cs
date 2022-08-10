
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Extensions;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BleFlasher
{

    internal class BleManager
    {
        List<Plugin.BLE.Abstractions.Contracts.IDevice> devices;

        public BleManager()
        {

        }
  

        public async void startScanning()
        {
            devices = new List<Plugin.BLE.Abstractions.Contracts.IDevice>();

            Plugin.BLE.CrossBluetoothLE.Current.RequestPermissions();
            Plugin.BLE.CrossBluetoothLE.Current.RequestEnable();

            Plugin.BLE.CrossBluetoothLE.Current.Adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
            var iadapter = Plugin.BLE.CrossBluetoothLE.Current.Adapter;
            iadapter.ScanTimeout = 15000;
            iadapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.Balanced;
            var scanFilterOptions = new ScanFilterOptions();

            Func<IDevice, bool> device_filter = (device) =>
            {
                bool service_found = false;
                foreach (var adv in device.AdvertisementRecords)
                {
                    if (adv.Type == Plugin.BLE.Abstractions.AdvertisementRecordType.UuidsComplete128Bit)
                    {
                        var uuid = Device.SERVICE_GUID.ToByteArray();
                        var swaped_uuid = Device.SERVICE_GUID.ToByteArray();
                        swaped_uuid[0] = uuid[3];
                        swaped_uuid[1] = uuid[2];
                        swaped_uuid[2] = uuid[1];
                        swaped_uuid[3] = uuid[0];

                        swaped_uuid[4] = uuid[5];
                        swaped_uuid[5] = uuid[4];

                        swaped_uuid[6] = uuid[7];
                        swaped_uuid[7] = uuid[6];

                        if (adv.Data.SequenceEqual(swaped_uuid))
                        {

                            service_found = true;
                            break;
                        }

                    }
                }
                return service_found;


            };
            scanFilterOptions.ServiceUuids = new[] { Device.SERVICE_GUID};
            await iadapter.StartScanningForDevicesAsync(allowDuplicatesKey: true, deviceFilter: device_filter);
        }

        private void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {

            Console.WriteLine("Device " + e.Device.Name +" found");
            devices.Add(e.Device);
        }

        public async void stopScanning()
        {
             await Plugin.BLE.CrossBluetoothLE.Current.Adapter.StopScanningForDevicesAsync();

        }

        public bool isScanning()
        {
            return Plugin.BLE.CrossBluetoothLE.Current.Adapter.IsScanning;

        }

        public BleFlasher.Device GetDevice(int idx)
        {
            return new BleFlasher.Device(devices[idx]);
        }
    }
}
