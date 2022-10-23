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

        private byte[] RawData;

        private string ConvertedGameTitle;

        public string GameTitle
        {
            get
            {
                return this.ConvertedGameTitle;
            }
            set
            {
                ConvertedGameTitle = value;
                byte[] title = Encoding.GetEncoding("shift_jis").GetBytes(value);
                byte i;
                for (i = 0; i < title.Length && title[i] != 0; i++)
                {
                    this.RawData[i] = title[i];
                }
                while (i < GAME_TITLE_SIZE)
                {
                    this.RawData[i++] = 0;
                }
            }
        }
        public byte[] GameCode
        {
            get
            {
                return new byte[] { RawData[0xC], RawData[0xD], RawData[0xE], RawData[0xF] };
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (value.Length != GAME_CODE_SIZE) throw new ArgumentException();
                RawData[0xC] = value[0];
                RawData[0xD] = value[1];
                RawData[0xE] = value[2];
                RawData[0xF] = value[3];
            }
        }
        public ushort MakerCode
        {
            get
            {
                return NumberHelper.LBytesToUInt16(RawData[0x10], RawData[0x11]);
            }
            set
            {
                NumberHelper.WriteLUint16(value, this.RawData, 0x10);
            }
        }
        public byte UnitCode
        {
            get { return RawData[0x12]; }
            set { RawData[0x12] = value; }
        } //Consola (0x00=NDS, 0x02=NDS+DSi, 0x03=DSi)
        public byte EncryptionSeed
        {
            get { return RawData[0x13]; }
            set { RawData[0x13] = value; }
        }
        public byte DeviceCapacity
        {
            get { return RawData[0x14]; }
            set { RawData[0x14] = value; }
        } //Tamaño del archivo/cartucho

        //8 bytes blank space
        public byte RomVersion
        {
            get { return RawData[0x1E]; }
            set { RawData[0x1E] = value; }
        }
        public byte InternalFlags
        {
            get { return RawData[0x1F]; }
            set { RawData[0x1F] = value; }
        }
        public uint ARM9RomOffset
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x20], RawData[0x21], RawData[0x22], RawData[0x23]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x20);
            }
        }
        public uint ARM9EntryAddress
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x24], RawData[0x25], RawData[0x26], RawData[0x27]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x24);
            }
        }
        public uint ARM9RamAddress
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x28], RawData[0x29], RawData[0x2A], RawData[0x2B]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x28);
            }
        }
        public uint ARM9CodeSize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x2C], RawData[0x2D], RawData[0x2E], RawData[0x2F]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x2C);
            }
        }
        public uint ARM7RomOffset
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x30], RawData[0x31], RawData[0x32], RawData[0x33]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x30);
            }
        }
        public uint ARM7EntryAddress
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x34], RawData[0x35], RawData[0x36], RawData[0x37]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x34);
            }
        }
        public uint ARM7RamAddress
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x38], RawData[0x39], RawData[0x3A], RawData[0x3B]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x38);
            }
        }
        public uint ARM7CodeSize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x3C], RawData[0x3D], RawData[0x3E], RawData[0x3F]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x3C);
            }
        }
        public uint FileNameTableOffset
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x40], RawData[0x41], RawData[0x42], RawData[0x43]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x40);
            }
        }
        public uint FileNameTableSize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x44], RawData[0x45], RawData[0x46], RawData[0x47]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x44);
            }
        }
        public uint FileAllocationTableOffset
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x48], RawData[0x49], RawData[0x4A], RawData[0x4B]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x48);
            }
        }
        public uint FileAllocationTableSize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x4C], RawData[0x4D], RawData[0x4E], RawData[0x4F]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x4C);
            }
        }
        public uint ARM9OverlayOffset
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x50], RawData[0x51], RawData[0x52], RawData[0x53]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x50);
            }
        }
        public uint ARM9OverlaySize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x54], RawData[0x55], RawData[0x56], RawData[0x57]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x54);
            }
        }
        public uint ARM7OverlayOffset
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x58], RawData[0x59], RawData[0x5A], RawData[0x5B]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x58);
            }
        }
        public uint ARM7OverlaySize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x5C], RawData[0x5D], RawData[0x5E], RawData[0x5F]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x5C);
            }
        }
        public uint NormalCardControlRegisterSettings
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x60], RawData[0x61], RawData[0x62], RawData[0x63]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x60);
            }
        }
        public uint SecureCardControlRegisterSettings
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x64], RawData[0x65], RawData[0x66], RawData[0x67]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x64);
            }
        }
        public uint IconBannerOffset
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x68], RawData[0x69], RawData[0x6A], RawData[0x6B]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x68);
            }
        }
        public ushort SecureAreaCRC { get; } //#NotImplemented
        public ushort SecureTransferTimeout { get; set; }
        public uint ARM9Autoload
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x70], RawData[0x71], RawData[0x72], RawData[0x73]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x70);
            }
        }
        public uint ARM7Autoload
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x74], RawData[0x75], RawData[0x76], RawData[0x77]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x74);
            }
        }
        public ulong SecureDisable { get; set; } //#CouldBeBetter->Dependiendo del uso que se le vaya a dar podría ser mejor, seguramente se quede así
        public uint NtrRegionRomSize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x80], RawData[0x81], RawData[0x82], RawData[0x83]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x80);
            }
        }
        public uint HeaderSize
        {
            get
            {
                return NumberHelper.LBytesToUInt32(RawData[0x84], RawData[0x85], RawData[0x86], RawData[0x87]);
            }
            set
            {
                NumberHelper.WriteLUint32(value, this.RawData, 0x84);
            }
        }
        public byte[] Reserved
        {
            get
            {
                byte[] res = new byte[RESERVED_SIZE];
                Array.Copy(this.RawData, 0x88, res, 0, RESERVED_SIZE);
                return res;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (value.Length != RESERVED_SIZE) throw new ArgumentException();
                Array.Copy(value, 0, this.RawData, 0x88, RESERVED_SIZE);
            }
        } //#CouldBeImproved
        public byte[] NintendoLogo
        {
            get
            {
                byte[] res = new byte[NINTENDO_LOGO_SIZE];
                Array.Copy(this.RawData, 0xC0, res, 0, NINTENDO_LOGO_SIZE);
                return res;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (value.Length != NINTENDO_LOGO_SIZE) throw new ArgumentException();
                Array.Copy(value, 0, this.RawData, 0xC0, NINTENDO_LOGO_SIZE);
                NumberHelper.WriteLUint16(CRC16.Calculate(this.RawData, 0xC0, NINTENDO_LOGO_SIZE), this.RawData, 0x15C);
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
                Array.Copy(this.RawData, 0x160, res, 0, DEBUGER_RESERVED_SIZE);
                return res;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (value.Length != DEBUGER_RESERVED_SIZE) throw new ArgumentException();
                Array.Copy(value, 0, this.RawData, 0x160, DEBUGER_RESERVED_SIZE);
            }
        } //#CouldBeImproved

        public Header(BinaryReader rom)
        {
            rom.BaseStream.Position = 0;

            this.RawData = new byte[WHOLE_HEADER_SIZE];
            rom.Read(this.RawData, 0, WHOLE_HEADER_SIZE);

            byte[] gameTitleBytes = new byte[12];
            gameTitleBytes[0] = this.RawData[0];
            gameTitleBytes[1] = this.RawData[1];
            gameTitleBytes[2] = this.RawData[2];
            gameTitleBytes[3] = this.RawData[3];
            gameTitleBytes[4] = this.RawData[4];
            gameTitleBytes[5] = this.RawData[5];
            gameTitleBytes[6] = this.RawData[6];
            gameTitleBytes[7] = this.RawData[7];
            gameTitleBytes[8] = this.RawData[8];
            gameTitleBytes[9] = this.RawData[9];
            gameTitleBytes[10] = this.RawData[10];
            gameTitleBytes[11] = this.RawData[11];
            this.ConvertedGameTitle = new string(Encoding.GetEncoding("shift_jis").GetChars(gameTitleBytes));
        }

        public byte[] GetBytes()
        {
            CalculateHeaderCRC16();
            return this.RawData;
        }

        private ushort CalculateHeaderCRC16()
        {
            ushort crc = CRC16.Calculate(this.RawData, 0, HEADER_SIZE);
            NumberHelper.WriteLUint16(crc, this.RawData, 0x15E);
            return crc;
        }
    }
}