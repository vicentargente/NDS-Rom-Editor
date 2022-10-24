using System;
using NDSRom.Calculation;
using System.IO;
using System.Text;

namespace NDSRom.DataStructures.Header
{
    //http://dsibrew.org/wiki/DSi_cartridge_header
    class Header
    {
        private const ushort WHOLE_HEADER_SIZE = 0x180;
        private const ushort HEADER_SIZE = 0x15E;
        private const byte GAME_TITLE_SIZE = 0xC;
        private const byte GAME_CODE_SIZE = 0x4;
        private const byte RESERVED_SIZE = 0x38;
        private const byte NINTENDO_LOGO_SIZE = 0x9C;
        private const byte DEBUGER_RESERVED_SIZE = 0x20;

        private byte[] rawData;

        private string convertedGameTitle;

        public string GameTitle
        {
            get
            {
                return this.convertedGameTitle;
            }
            set
            {
                convertedGameTitle = value;
                byte[] title = Encoding.GetEncoding("shift_jis").GetBytes(value);
                byte i;
                for (i = 0; i < title.Length && title[i] != 0; i++)
                {
                    this.rawData[i] = title[i];
                }
                while (i < GAME_TITLE_SIZE)
                {
                    this.rawData[i++] = 0;
                }
            }
        }
        public byte[] GameCode
        {
            get
            {
                return new byte[] { rawData[0xC], rawData[0xD], rawData[0xE], rawData[0xF] };
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (value.Length != GAME_CODE_SIZE) throw new ArgumentException();
                rawData[0xC] = value[0];
                rawData[0xD] = value[1];
                rawData[0xE] = value[2];
                rawData[0xF] = value[3];
            }
        }
        public ushort MakerCode
        {
            get
            {
                return NumberHelper.LBytesToUInt16(rawData[0x10], rawData[0x11]);
            }
            set
            {
                NumberHelper.WriteLUint16(value, this.rawData, 0x10);
            }
        }
        public byte UnitCode
        {
            get { return rawData[0x12]; }
            set { rawData[0x12] = value; }
        } //Consola (0x00=NDS, 0x02=NDS+DSi, 0x03=DSi)
        public byte EncryptionSeed
        {
            get { return rawData[0x13]; }
            set { rawData[0x13] = value; }
        }
        public byte DeviceCapacity
        {
            get { return rawData[0x14]; }
            set { rawData[0x14] = value; }
        } //Tamaño del archivo/cartucho

        //8 bytes blank space
        public byte RomVersion
        {
            get { return rawData[0x1E]; }
            set { rawData[0x1E] = value; }
        }
        public byte InternalFlags
        {
            get { return rawData[0x1F]; }
            set { rawData[0x1F] = value; }
        }
        public uint ARM9RomOffset
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x20], rawData[0x21], rawData[0x22], rawData[0x23]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x20);
            }
        }
        public uint ARM9EntryAddress
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x24], rawData[0x25], rawData[0x26], rawData[0x27]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x24);
            }
        }
        public uint ARM9RamAddress
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x28], rawData[0x29], rawData[0x2A], rawData[0x2B]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x28);
            }
        }
        public uint ARM9CodeSize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x2C], rawData[0x2D], rawData[0x2E], rawData[0x2F]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x2C);
            }
        }
        public uint ARM7RomOffset
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x30], rawData[0x31], rawData[0x32], rawData[0x33]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x30);
            }
        }
        public uint ARM7EntryAddress
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x34], rawData[0x35], rawData[0x36], rawData[0x37]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x34);
            }
        }
        public uint ARM7RamAddress
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x38], rawData[0x39], rawData[0x3A], rawData[0x3B]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x38);
            }
        }
        public uint ARM7CodeSize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x3C], rawData[0x3D], rawData[0x3E], rawData[0x3F]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x3C);
            }
        }
        public uint FileNameTableOffset
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x40], rawData[0x41], rawData[0x42], rawData[0x43]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x40);
            }
        }
        public uint FileNameTableSize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x44], rawData[0x45], rawData[0x46], rawData[0x47]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x44);
            }
        }
        public uint FileAllocationTableOffset
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x48], rawData[0x49], rawData[0x4A], rawData[0x4B]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x48);
            }
        }
        public uint FileAllocationTableSize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x4C], rawData[0x4D], rawData[0x4E], rawData[0x4F]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x4C);
            }
        }
        public uint ARM9OverlayOffset
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x50], rawData[0x51], rawData[0x52], rawData[0x53]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x50);
            }
        }
        public uint ARM9OverlaySize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x54], rawData[0x55], rawData[0x56], rawData[0x57]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x54);
            }
        }
        public uint ARM7OverlayOffset
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x58], rawData[0x59], rawData[0x5A], rawData[0x5B]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x58);
            }
        }
        public uint ARM7OverlaySize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x5C], rawData[0x5D], rawData[0x5E], rawData[0x5F]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x5C);
            }
        }
        public uint NormalCardControlRegisterSettings
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x60], rawData[0x61], rawData[0x62], rawData[0x63]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x60);
            }
        }
        public uint SecureCardControlRegisterSettings
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x64], rawData[0x65], rawData[0x66], rawData[0x67]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x64);
            }
        }
        public uint IconBannerOffset
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x68], rawData[0x69], rawData[0x6A], rawData[0x6B]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x68);
            }
        }
        public ushort SecureAreaCRC { get; } //#NotImplemented
        public ushort SecureTransferTimeout { get; set; }
        public uint ARM9Autoload
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x70], rawData[0x71], rawData[0x72], rawData[0x73]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x70);
            }
        }
        public uint ARM7Autoload
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x74], rawData[0x75], rawData[0x76], rawData[0x77]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x74);
            }
        }
        public ulong SecureDisable { get; set; } //#CouldBeBetter->Dependiendo del uso que se le vaya a dar podría ser mejor, seguramente se quede así
        public uint NtrRegionRomSize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x80], rawData[0x81], rawData[0x82], rawData[0x83]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x80);
            }
        }
        public uint HeaderSize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(rawData[0x84], rawData[0x85], rawData[0x86], rawData[0x87]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.rawData, 0x84);
            }
        }
        public byte[] Reserved
        {
            get
            {
                byte[] res = new byte[RESERVED_SIZE];
                Array.Copy(this.rawData, 0x88, res, 0, RESERVED_SIZE);
                return res;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (value.Length != RESERVED_SIZE) throw new ArgumentException();
                Array.Copy(value, 0, this.rawData, 0x88, RESERVED_SIZE);
            }
        } //#CouldBeImproved
        public byte[] NintendoLogo
        {
            get
            {
                byte[] res = new byte[NINTENDO_LOGO_SIZE];
                Array.Copy(this.rawData, 0xC0, res, 0, NINTENDO_LOGO_SIZE);
                return res;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (value.Length != NINTENDO_LOGO_SIZE) throw new ArgumentException();
                Array.Copy(value, 0, this.rawData, 0xC0, NINTENDO_LOGO_SIZE);
                NumberHelper.WriteLUint16(CRC16.Calculate(this.rawData, 0xC0, NINTENDO_LOGO_SIZE), this.rawData, 0x15C);
            }
        } //#CouldBeImproved
        public ushort NintendoLogoCRC { get; }
        public ushort HeaderCRC
        {
            get
            {
                return CalculateHeaderCRC16();
            }
        }
        public byte[] DebuggerReserved
        {
            get
            {
                byte[] res = new byte[DEBUGER_RESERVED_SIZE];
                Array.Copy(this.rawData, 0x160, res, 0, DEBUGER_RESERVED_SIZE);
                return res;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (value.Length != DEBUGER_RESERVED_SIZE) throw new ArgumentException();
                Array.Copy(value, 0, this.rawData, 0x160, DEBUGER_RESERVED_SIZE);
            }
        } //#CouldBeImproved

        public Header(Stream rom)
        {
            long positionAux = rom.Position;

            rom.Position = 0;

            this.rawData = new byte[WHOLE_HEADER_SIZE];
            rom.Read(this.rawData, 0, WHOLE_HEADER_SIZE);

            byte[] gameTitleBytes = new byte[12];
            gameTitleBytes[0] = this.rawData[0];
            gameTitleBytes[1] = this.rawData[1];
            gameTitleBytes[2] = this.rawData[2];
            gameTitleBytes[3] = this.rawData[3];
            gameTitleBytes[4] = this.rawData[4];
            gameTitleBytes[5] = this.rawData[5];
            gameTitleBytes[6] = this.rawData[6];
            gameTitleBytes[7] = this.rawData[7];
            gameTitleBytes[8] = this.rawData[8];
            gameTitleBytes[9] = this.rawData[9];
            gameTitleBytes[10] = this.rawData[10];
            gameTitleBytes[11] = this.rawData[11];
            this.convertedGameTitle = new string(Encoding.GetEncoding("shift_jis").GetChars(gameTitleBytes));

            rom.Position = positionAux;
        }

        public byte[] GetBytes()
        {
            CalculateHeaderCRC16();
            return this.rawData;
        }

        private ushort CalculateHeaderCRC16()
        {
            ushort crc = CRC16.Calculate(this.rawData, 0, HEADER_SIZE);
            NumberHelper.WriteLUint16(crc, this.rawData, 0x15E);
            return crc;
        }
    }
}