namespace BleFlasher
{
    internal partial class Device
    {
        class Frame

        {
            public enum COMMAND_LIST
            {
                NO_COMMAND,
                COMMAND_ERASE,
                COMMAND_WRITE,
                COMMAND_READ,
                COMMAND_START,
                COMMMAND_RESET,
            };

            public enum COMMAND_STATUS
            {
                COMMAND_REQUEST,
                COMMAND_ACK,
                COMMAND_ERR,
            };

            public const int HEADER_SIZE = 10;
            public const int MAX_PAYLOAD = 128;
            public const uint DEFAULT_ADDRESS = uint.MaxValue;


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

            public Frame(COMMAND_LIST command, uint adresse)
            {
                this.command = command;
                this.status = COMMAND_STATUS.COMMAND_REQUEST;
                this.adresse = adresse;
                this.size = 0;
                this.datas = null;
            }


            public Frame(COMMAND_LIST command)
            {
                this.command = command;
                this.status = COMMAND_STATUS.COMMAND_REQUEST;
                this.adresse = DEFAULT_ADDRESS;
                this.size = 0;
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
                this.adresse = (uint)((frame[1] << 0) + (frame[2] << 8) + (frame[3] << 16) + (frame[4] << 24));
                this.size = (uint)((frame[5] << 0) + (frame[6] << 8) + (frame[7] << 16) + (frame[8] << 24));
                frame.Skip(HEADER_SIZE).ToArray().CopyTo(this.datas,0);
            }


            public static explicit operator byte[](Frame b) {
                byte[] bytes = new byte[HEADER_SIZE+MAX_PAYLOAD];
                bytes[0] = ((byte)b.command);
                bytes[1] = ((byte)(b.adresse >> 0));
                bytes[2] = ((byte)(b.adresse >> 8));
                bytes[3] = ((byte)(b.adresse >> 16));
                bytes[4] = ((byte)(b.adresse >> 24));
                bytes[5] = ((byte)(b.size >> 0));
                bytes[6] = ((byte)(b.size >> 8));
                bytes[7] = ((byte)(b.size >> 16));
                bytes[8] = ((byte)(b.size >> 24));
                if (b.datas != null)
                    b.datas.CopyTo(bytes, HEADER_SIZE);

                return bytes;
            }
        }
    }
}
