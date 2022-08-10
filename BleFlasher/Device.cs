using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE;

namespace BleFlasher
{
    internal partial class Device
    {
        private Plugin.BLE.Abstractions.Contracts.IDevice device;
        private Plugin.BLE.Abstractions.Contracts.ICharacteristic cmd_charact;

        public static Guid SERVICE_GUID = Guid.Parse("42535331-0000-1000-8000-00805F9B34FB");
        public static Guid COMMAND_GUID = Guid.Parse("42534331-0000-1000-8000-00805F9B34FB");


        Frame current_request;
        Queue<Frame> received_frames;

        public Device(Plugin.BLE.Abstractions.Contracts.IDevice device)
        {
            this.device = device;
            this.received_frames = new Queue<Frame>();
        }

        public async Task<bool> connect()
        {
            await Plugin.BLE.CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(device);


            var service = await device.GetServiceAsync(SERVICE_GUID);
            if (service != null)
            {
                cmd_charact = await service.GetCharacteristicAsync(COMMAND_GUID);

                cmd_charact.ValueUpdated -= Cmd_charact_ValueUpdated;
                cmd_charact.ValueUpdated += Cmd_charact_ValueUpdated;
                await cmd_charact.StartUpdatesAsync();

            }
            return cmd_charact != null;
        }

        public async void disconnect()
        {
            await Plugin.BLE.CrossBluetoothLE.Current.Adapter.DisconnectDeviceAsync(device);
        }

        private void Cmd_charact_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
        {
            received_frames.Enqueue(new Frame(e.Characteristic.Value));
        }

        public async Task<bool> waitForAck(int timeout)
        {
            Console.WriteLine("WAIT FOR ACK");
            while (timeout > 0)
            {
                if (received_frames.Count > 0)
                {
                    var frame = received_frames.Dequeue();
                    if (frame.adresse == current_request.adresse && frame.command == current_request.command)
                    {
                        Console.WriteLine("ACK RECEIVED");
                        return frame.status == Frame.COMMAND_STATUS.COMMAND_ACK;
                    }
                }
                await Task.Delay(100);
                timeout -= 100;
            }
            Console.WriteLine("ACK NOT RECEIVED");
            return false;
        }

        public async Task<byte[]> waitForData(int timeout)
        {

            while (timeout > 0)
            {
                if(received_frames.Count> 0) { 
                    var frame = received_frames.Dequeue();
                    if (frame.adresse == current_request.adresse && frame.command == current_request.command)
                    {
                        if (frame.status == Frame.COMMAND_STATUS.COMMAND_ACK)
                        {
                            return frame.datas;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                await Task.Delay(100);
                timeout -= 100;
            }
            return null;
        }

        public async Task<bool> erase(uint base_adresse, uint size)
        {
         

            current_request = new Frame(Frame.COMMAND_LIST.COMMAND_ERASE, base_adresse, size);
            await cmd_charact.WriteAsync(((byte[])current_request));

            if (await waitForAck(10000) == false)
            {
                return false;
            }
            return true;

        }

        public async Task<bool> write(uint base_adresse, byte[] data)
        {
            var chunks = data.Chunk(128);
            foreach (var chunk in chunks) // for each chunk
            {

                current_request = new Frame(Frame.COMMAND_LIST.COMMAND_WRITE, base_adresse, chunk);

                await cmd_charact.WriteAsync(((byte[])current_request));

                if (await waitForAck(2000) == false)
                {
                    return false;
                }
                base_adresse += (uint)chunk.Length;
            }
            return true;

        }

        public async Task<byte[]> read(uint base_adresse, uint size)
        {
            List<byte> data = new List<byte>();

            /* COMMAND read */
            for (int i = 0; i < size; i++)
            {
                if (i - size > Frame.MAX_PAYLOAD)
                {
                    current_request = new Frame(Frame.COMMAND_LIST.COMMAND_READ, base_adresse, Frame.MAX_PAYLOAD);
                }
                else
                {
                    current_request = new Frame(Frame.COMMAND_LIST.COMMAND_READ, base_adresse, (uint)(i - size));
                }

                await cmd_charact.WriteAsync(((byte[])current_request));

                // WAIT FOR DATA
                var incomming_data = await waitForData(200);
                if (incomming_data != null)
                {
                    data.AddRange(incomming_data);
                }
                else
                {
                    return null;
                }
            }


            return data.ToArray();
        }

        async Task<bool> reset()
        {
            /* COMMAND RESET */
            current_request = new Frame(Frame.COMMAND_LIST.COMMMAND_RESET);

            await cmd_charact.WriteAsync(((byte[])current_request));

            return await waitForAck(2000);
        }
        public async Task<bool> start(uint addresse)
        {
            /* COMMAND start */
            current_request = new Frame(Frame.COMMAND_LIST.COMMAND_START, addresse);

            await cmd_charact.WriteAsync(((byte[])current_request));

            return await waitForAck(2000);
        }

        public async Task<bool> start()
        {
            /* COMMAND start */
            current_request = new Frame(Frame.COMMAND_LIST.COMMAND_START);

            await cmd_charact.WriteAsync(((byte[])current_request));

            return await waitForAck(2000);

        }
    }
}