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

        private uint Offset;
        private TableEntry[] Data;

        public uint FilesAmount { get; }
        public FileAllocationTable(BinaryReader rom)
        {
            rom.BaseStream.Position = FILE_ALLOCATION_TABLE_OFFSET;
            this.Offset = rom.ReadUInt32();

            //rom.BaseStream.Position = FILE_ALLOCATION_TABLE_LENGTH;
            this.FilesAmount = rom.ReadUInt32() >> 3;

            this.Data = new TableEntry[this.FilesAmount];
            rom.BaseStream.Position = this.Offset;

            for (uint i = 0; i < this.FilesAmount; i++)
            {
                this.Data[i] = new TableEntry(rom.ReadUInt32(), rom.ReadUInt32());
            }
        }

        private FileAllocationTable(FileAllocationTable Original)
        {
            this.Offset = Original.Offset;
            this.FilesAmount = Original.FilesAmount;
            this.Data = new TableEntry[this.FilesAmount];
            for (uint i = 0; i < this.FilesAmount; i++)
            {
                this.Data[i] = new TableEntry(Original.Data[i].Begining, Original.Data[i].End);
            }
        }

        public uint GetStartOffsetFromFileId(ushort FileId)
        {
            return this.Data[FileId].Begining;
        }

        public uint GetEndOffsetFromFileId(ushort FileId)
        {
            return this.Data[FileId].End;
        }

        public uint GetSizeFromFileId(ushort FileId)
        {
            return this.Data[FileId].Size;
        }

        public void ResizeFile(ushort FileId, uint NewFileSize)
        {
            uint currentSize = this.Data[FileId].Size;
            if (NewFileSize == currentSize) return;
            int sizeDifference = (int)NewFileSize - (int)currentSize;
            sizeDifference = NumberHelper.NextValidAddress(sizeDifference);
            for (int i = 0; i < this.Data.Length; i++)
            {
                if (this.Data[i].Begining >= this.Data[FileId].End)
                {
                    this.Data[i].Begining = (uint)((int)this.Data[i].Begining + sizeDifference);
                    this.Data[i].End = (uint)((int)this.Data[i].End + sizeDifference);
                }
            }
            this.Data[FileId].End = this.Data[FileId].Begining + NewFileSize;
        }

        public byte[] GetBytes()
        {
            byte[] res = new byte[this.Data.Length << 3];
            for (uint i_byte = 0, i_data = 0; i_data < this.Data.Length; i_data++, i_byte += 8)
            {
                res[i_byte] = (byte)(this.Data[i_data].Begining & 0xFF);
                res[i_byte + 1] = (byte)((this.Data[i_data].Begining >> 8) & 0xFF);
                res[i_byte + 2] = (byte)((this.Data[i_data].Begining >> 16) & 0xFF);
                res[i_byte + 3] = (byte)((this.Data[i_data].Begining >> 24) & 0xFF);
                res[i_byte + 4] = (byte)(this.Data[i_data].End & 0xFF);
                res[i_byte + 5] = (byte)((this.Data[i_data].End >> 8) & 0xFF);
                res[i_byte + 6] = (byte)((this.Data[i_data].End >> 16) & 0xFF);
                res[i_byte + 7] = (byte)((this.Data[i_data].End >> 24) & 0xFF);
            }
            return res;
        }

        public FileAllocationTable Clone()
        {
            return new FileAllocationTable(this);
        }
    }
}