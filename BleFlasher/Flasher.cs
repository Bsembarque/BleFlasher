using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE;

namespace BleFlasher
{
    internal class Flasher
    {
        private Plugin.BLE.Abstractions.Contracts.IDevice device;
        private Plugin.BLE.Abstractions.Contracts.ICharacteristic cmd_charact;

        const string SERVICE_GUID = "";
        const string COMMAND_GUID = "";
  

        enum COMMAND_LIST
        {
           COMMAND_ERASE,
           COMMAND_WRITE,
           COMMAND_READ,
           COMMAND_START,
           COMMMAND_RESET,
        };

        enum COMMAND_STATUS
        {
            COMMAND_REQUEST,
            COMMAND_ACK,
            COMMAND_ERR,
        };

        class Frame

        {
            public const int HEADER_SIZE = 10;
            public const int MAX_PAYLOAD = 128;
            public COMMAND_LIST command;
            public COMMAND_STATUS status;
            public uint adresse;
            public uint size;
            public byte[] datas = new byte[128];

            public Frame(COMMAND_LIST command, uint adresse, uint size)
            {
                this.command = command;
                this.status = COMMAND_STATUS.COMMAND_REQUEST;
                this.adresse = adresse;
                this.size = size;
                this.datas = null;
            }

            public Frame(COMMAND_LIST command, uint adresse, byte[] datas)
            {
                this.command = command;
                this.status = COMMAND_STATUS.COMMAND_REQUEST;
                this.adresse = adresse;
                this.size = (uint)datas.Length;
                this.datas = datas;
            }

            public Frame(byte[] frame)
            {
                this.command = (COMMAND_LIST)(frame[0]&0x0F);
                this.status = (COMMAND_STATUS)(frame[0]>>4 & 0x0F);
                this.adresse = (uint)((frame[1] << 24) + (frame[2] << 16) + (frame[3] << 8) + frame[4]);
                this.size = (uint)((frame[5] << 24) + (frame[6] << 16) + (frame[7] << 8) + frame[8]); ;
                frame.Skip(HEADER_SIZE).ToArray().CopyTo(this.datas,0);
            }


            public static explicit operator byte[](Frame b) {
                byte[] bytes = new byte[HEADER_SIZE+MAX_PAYLOAD];
                bytes[0] = ((byte)b.command);
                bytes[1] = ((byte)(b.adresse >> 24));
                bytes[2] = ((byte)(b.adresse >> 16));
                bytes[3] = ((byte)(b.adresse >> 8));
                bytes[4] = ((byte)b.adresse);
                bytes[5] = ((byte)(b.adresse >> 24));
                bytes[6] = ((byte)(b.size >> 16));
                bytes[7] = ((byte)(b.size >> 8));
                bytes[8] = ((byte)b.size);
                if(b.datas != null)
                    b.datas.CopyTo(bytes, HEADER_SIZE);

                return bytes;
            }
        }

        Frame current_request;
        Queue<Frame> received_frames;

        Flasher(Plugin.BLE.Abstractions.Contracts.IDevice device)
        {
            this.device = device;
            this.received_frames = new Queue<Frame>();
        }

        async Task<bool> connect()
        {
            await Plugin.BLE.CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(device);

            var service = await device.GetServiceAsync(Guid.Parse(SERVICE_GUID));
            if(service != null)
            {
                cmd_charact = await service.GetCharacteristicAsync(Guid.Parse(COMMAND_GUID));
                cmd_charact.ValueUpdated += Cmd_charact_ValueUpdated;
                await cmd_charact.StartUpdatesAsync();

            }
            return cmd_charact != null;
        }

        async void disconnect()
        {
            await Plugin.BLE.CrossBluetoothLE.Current.Adapter.DisconnectDeviceAsync(device);
        }

        private void Cmd_charact_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
        {
            received_frames.Append(new Frame(e.Characteristic.Value));
        }

        async Task<bool> waitForAck(int timeout)
        {
            while (timeout > 0)
            {
                var frame = received_frames.Dequeue();
                if (frame.adresse == current_request.adresse && frame.command == current_request.command)
                {
                    return frame.status == COMMAND_STATUS.COMMAND_ACK;
                }
                await Task.Delay(100);
                timeout -= 100;
            }
            return false;
        }

        async Task<byte[]> waitForData(int timeout)
        {
            
            while (timeout >0)
            {
                var frame = received_frames.Dequeue();
                if (frame.adresse == current_request.adresse && frame.command == current_request.command)
                {
                    if(frame.status == COMMAND_STATUS.COMMAND_ACK)
                    {
                        return frame.datas;
                    }
                    else
                    {
                        return null;
                    }
                }
                await Task.Delay(100);
                timeout -= 100;
            }
            return null;
        }

        async Task<bool> erase(uint base_adresse, uint size)
        {
            /* COMMAND ERASE */
            current_request = new Frame(COMMAND_LIST.COMMAND_ERASE, base_adresse, size);

            await cmd_charact.WriteAsync(((byte[])current_request));

            return await waitForAck(2000);
        }

        async Task<bool> write(uint base_adresse, byte[] data)
        {
            var chunks = data.Chunk(128);
            foreach (var chunk in chunks) // for each chunk
            {

                current_request = new Frame(COMMAND_LIST.COMMAND_WRITE, base_adresse, chunk);

                await cmd_charact.WriteAsync(((byte[])current_request));

                if(await waitForAck(200) == false)
                {
                    return false;
                }
                base_adresse += (uint)chunk.Length;
            }
            return true;

        }

        async Task<byte[]> read(uint base_adresse, uint size)
        {
            List<byte> data = new List<byte>();

            /* COMMAND read */
            for (int i = 0; i < size; i++)
            {
                if(i- size > Frame.MAX_PAYLOAD)
                {
                    current_request = new Frame(COMMAND_LIST.COMMAND_READ, base_adresse, Frame.MAX_PAYLOAD);
                }
                else
                {
                    current_request = new Frame(COMMAND_LIST.COMMAND_READ, base_adresse, (uint)(i - size));
                }

                await cmd_charact.WriteAsync(((byte[])current_request));

                // WAIT FOR DATA
                var incomming_data= await waitForData(200);
                if(incomming_data != null)
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
            current_request.adresse = 0;
            current_request.size = 0;
            current_request.command = COMMAND_LIST.COMMMAND_RESET;

            await cmd_charact.WriteAsync(((byte[])current_request));

            return await waitForAck(2000);
        }
        async Task<bool> start(uint addresse = uint.MaxValue)
        {
            /* COMMAND start */
            current_request.adresse = addresse;
            current_request.size = 0;
            current_request.command = COMMAND_LIST.COMMAND_START;

            await cmd_charact.WriteAsync(((byte[])current_request));

            return await waitForAck(2000);
        }
    }
}
