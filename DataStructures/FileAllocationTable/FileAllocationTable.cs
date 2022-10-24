using NDSRom.Calculation;
using System.IO;

namespace NDSRom.DataStructures.FileAllocationTable
{
    class FileAllocationTable
    {
        class TableEntry
        {
            public uint Begining { get; set; }
            public uint End { get; set; }
            public uint Size
            {
                get
                {
                    return this.End - this.Begining;
                }
            }
            public TableEntry(uint Begining, uint End)
            {
                this.Begining = Begining;
                this.End = End;
            }
        }
        private const uint FILE_ALLOCATION_TABLE_OFFSET = 0X48;
        //private const uint FILE_ALLOCATION_TABLE_LENGTH = 0X4C;

        private uint offset;
        private TableEntry[] data;

        public uint FilesAmount { get; }
        public FileAllocationTable(Stream rom)
        {
            long positionAux = rom.Position;

            BinaryReader romReader = new BinaryReader(rom);

            rom.Position = FILE_ALLOCATION_TABLE_OFFSET;
            this.offset = romReader.ReadUInt32();

            //rom.BaseStream.Position = FILE_ALLOCATION_TABLE_LENGTH;
            this.FilesAmount = romReader.ReadUInt32() >> 3;

            this.data = new TableEntry[this.FilesAmount];
            rom.Position = this.offset;

            for (uint i = 0; i < this.FilesAmount; i++)
            {
                this.data[i] = new TableEntry(romReader.ReadUInt32(), romReader.ReadUInt32());
            }

            rom.Position = positionAux;
        }

        private FileAllocationTable(FileAllocationTable Original)
        {
            this.offset = Original.offset;
            this.FilesAmount = Original.FilesAmount;
            this.data = new TableEntry[this.FilesAmount];
            for (uint i = 0; i < this.FilesAmount; i++)
            {
                this.data[i] = new TableEntry(Original.data[i].Begining, Original.data[i].End);
            }
        }

        public uint GetStartOffsetFromFileId(ushort FileId)
        {
            return this.data[FileId].Begining;
        }

        public uint GetEndOffsetFromFileId(ushort FileId)
        {
            return this.data[FileId].End;
        }

        public uint GetSizeFromFileId(ushort FileId)
        {
            return this.data[FileId].Size;
        }

        public void ResizeFile(ushort FileId, uint NewFileSize)
        {
            uint currentSize = this.data[FileId].Size;
            if (NewFileSize == currentSize) return;
            int sizeDifference = (int)NewFileSize - (int)currentSize;
            sizeDifference = NumberHelper.NextValidAddress(sizeDifference);
            for (int i = 0; i < this.data.Length; i++)
            {
                if (this.data[i].Begining >= this.data[FileId].End)
                {
                    this.data[i].Begining = (uint)((int)this.data[i].Begining + sizeDifference);
                    this.data[i].End = (uint)((int)this.data[i].End + sizeDifference);
                }
            }
            this.data[FileId].End = this.data[FileId].Begining + NewFileSize;
        }

        public byte[] GetBytes()
        {
            byte[] res = new byte[this.data.Length << 3];
            for (uint i_byte = 0, i_data = 0; i_data < this.data.Length; i_data++, i_byte += 8)
            {
                res[i_byte] = (byte)(this.data[i_data].Begining & 0xFF);
                res[i_byte + 1] = (byte)((this.data[i_data].Begining >> 8) & 0xFF);
                res[i_byte + 2] = (byte)((this.data[i_data].Begining >> 16) & 0xFF);
                res[i_byte + 3] = (byte)((this.data[i_data].Begining >> 24) & 0xFF);
                res[i_byte + 4] = (byte)(this.data[i_data].End & 0xFF);
                res[i_byte + 5] = (byte)((this.data[i_data].End >> 8) & 0xFF);
                res[i_byte + 6] = (byte)((this.data[i_data].End >> 16) & 0xFF);
                res[i_byte + 7] = (byte)((this.data[i_data].End >> 24) & 0xFF);
            }
            return res;
        }

        public FileAllocationTable Clone()
        {
            return new FileAllocationTable(this);
        }
    }
}