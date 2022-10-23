using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDSRom.Tools
{
    interface ImportedFile
    {
        uint Size { get; }
        void WriteFileAt(BinaryWriter output, uint position);
    }

    class ByteArrayImportedFile : ImportedFile
    {
        private byte[] File;
        public uint Size
        {
            get
            {
                return (uint)this.File.Length;
            }
        }
        public ByteArrayImportedFile(byte[] file)
        {
            this.File = file;
        }
        public void WriteFileAt(BinaryWriter output, uint position)
        {
            long auxPos = output.BaseStream.Position;
            output.BaseStream.Position = position;
            output.Write(this.File);
            output.BaseStream.Position = auxPos;
        }
    }
    class ExternalImportedFile : ImportedFile
    {
        private string FilePath;
        private FileCopier FileCopier;
        public uint Size
        {
            get
            {
                return (uint)new FileInfo(this.FilePath).Length;
            }
        }
        public ExternalImportedFile(string filePath, FileCopier fileCopier)
        {
            this.FilePath = filePath;
            this.FileCopier = fileCopier;
        }
        public void WriteFileAt(BinaryWriter output, uint position)
        {
            BinaryReader input = new BinaryReader(File.OpenRead(this.FilePath));
            this.FileCopier.Copy(input.BaseStream,0,(uint)input.BaseStream.Length, output.BaseStream, position);
            input.Close();
        }
    }
}