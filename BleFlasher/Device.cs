using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BleFlasher
{
    internal partial class Device
    {
        private InTheHand.Bluetooth.BluetoothDevice device;
        private InTheHand.Bluetooth.GattCharacteristic cmd_charact;

        public static Guid SERVICE_GUID = Guid.Parse("42535331-0000-1000-8000-00805F9B34FB");
        public static Guid COMMAND_GUID = Guid.Parse("42534331-0000-1000-8000-00805F9B34FB");


        Frame current_request;
        Queue<Frame> received_frames;

        public Device(InTheHand.Bluetooth.BluetoothDevice device)
        {
            this.device = device;
            this.received_frames = new Queue<Frame>();
        }

        public async Task<bool> connect()
        {
            await device.Gatt.ConnectAsync();


            var service = await device.Gatt.GetPrimaryServiceAsync(SERVICE_GUID);
            if (service != null)
            {
                cmd_charact = await service.GetCharacteristicAsync(COMMAND_GUID);

                cmd_charact.CharacteristicValueChanged -= Cmd_charact_CharacteristicValueChanged;
                cmd_charact.CharacteristicValueChanged += Cmd_charact_CharacteristicValueChanged;
                await cmd_charact.StartNotificationsAsync();

            }
            return cmd_charact != null;
        }

        private void Cmd_charact_CharacteristicValueChanged(object sender, InTheHand.Bluetooth.GattCharacteristicValueChangedEventArgs e)
        {
            received_frames.Enqueue(new Frame(e.Value));
        }

        public void disconnect()
        {
            device.Gatt.Disconnect();
        }

        private Task send_Frame(Frame f)
        {
            return cmd_charact.WriteValueWithoutResponseAsync((byte[])f);
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
            await send_Frame(current_request);

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

                await send_Frame(current_request);

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

                await send_Frame(current_request);

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

            await send_Frame(current_request);

            return await waitForAck(2000);
        }
        public async Task<bool> start(uint addresse)
        {
            /* COMMAND start */
            current_request = new Frame(Frame.COMMAND_LIST.COMMAND_START, addresse);

            await send_Frame(current_request);

            return await waitForAck(2000);
        }

        public async Task<bool> start()
        {
            /* COMMAND start */
            current_request = new Frame(Frame.COMMAND_LIST.COMMAND_START);

            await send_Frame(current_request);

            return await waitForAck(2000);

        }

        public async Task<bool> writeBinaryFile(uint start_adress, Stream filestream, bool erasebefore=true)
        {
            var filedata = new byte[filestream.Length];
            filestream.Read(filedata);
            if (erasebefore)
            {
                await erase(start_adress, ((uint)filedata.Length));
            }
            return await write(start_adress, filedata);
        }

        public async void writeHexFile(uint start_adress, Stream filestream, bool erasebefore)
        {
            var filedata = new byte[filestream.Length];
            filestream.Read(filedata);
            await erase(start_adress, ((uint)filedata.Length));
            await write(start_adress, filedata);
        }
    }
}