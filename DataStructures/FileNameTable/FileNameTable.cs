using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NDSRom.DataStructures.FileNameTable
{
    class FileNameTable
    {
        class MainTable
        {
            public uint Offset { get; set; }
            public ushort FirstFileId { get; set; }
            public ushort ParentDirectoryId { get; set; }
            private List<Tuple<ushort, string>> SubDirectories;
            public List<Tuple<ushort, string>> SubFiles { get; }
            public MainTable(uint Offset, ushort FirstFileId, ushort ParentDirectoryId)
            {
                this.Offset = Offset;
                this.FirstFileId = FirstFileId;
                this.ParentDirectoryId = ParentDirectoryId;
                this.SubDirectories = new List<Tuple<ushort, string>>();
                this.SubFiles = new List<Tuple<ushort, string>>();
            }

            public void AddSubDirectory(ushort id, string name)
            {
                this.SubDirectories.Add(new Tuple<ushort, string>(id, name));
            }

            public Tuple<ushort, string> GetSubDirectory(int i)
            {
                return this.SubDirectories.ElementAt(i);
            }

            public void AddSubFile(ushort id, string name)
            {
                this.SubFiles.Add(new Tuple<ushort, string>(id, name));
            }
        }

        private const uint FILE_NAME_TABLE_OFFSET = 0X40;
        private const ushort ROOT_DIRECTORY_ID = 0xF000;

        private FNTDirectory Root;

        public FileNameTable(BinaryReader rom)
        {
            rom.BaseStream.Position = FILE_NAME_TABLE_OFFSET;
            uint tableOffset = rom.ReadUInt32();
            uint tableLength = rom.ReadUInt32();

            rom.BaseStream.Position = tableOffset;
            uint firstFolderOffset = rom.ReadUInt32(); //Fin de las mainTable || Inicio de las subTable

            uint mainTableAmount = firstFolderOffset >> 3;

            MainTable[] mainTables = new MainTable[mainTableAmount];

            rom.BaseStream.Position = tableOffset; //Nos situamos en la primera MainTable
            for (uint i = 0; i < mainTableAmount; i++)
            {
                MainTable currentMain = new MainTable(rom.ReadUInt32(), rom.ReadUInt16(), rom.ReadUInt16());
                uint nextTable = (uint)rom.BaseStream.Position; //Guardamos donde tenemos que volver antes de la siguiente iteracion

                ushort firstFileId = currentMain.FirstFileId;

                rom.BaseStream.Position = tableOffset + currentMain.Offset;
                for (byte nameLength = rom.ReadByte(); nameLength != 0; nameLength = rom.ReadByte())
                {
                    if (nameLength < 0x80)
                    { //Fichero
                        string fileName = new string(Encoding.GetEncoding("shift_jis").GetChars(rom.ReadBytes(nameLength)));
                        currentMain.AddSubFile(firstFileId++, fileName);
                    }
                    else
                    { //Carpeta
                        nameLength -= 0x80;
                        string directoryName = new string(Encoding.GetEncoding("shift_jis").GetChars(rom.ReadBytes(nameLength)));
                        currentMain.AddSubDirectory(rom.ReadUInt16(), directoryName);
                    }
                }
                mainTables[i] = currentMain;
                rom.BaseStream.Position = nextTable;
            }

            this.Root = new FNTDirectory(ROOT_DIRECTORY_ID);
            FNTDirectory[] dirs = new FNTDirectory[mainTableAmount];
            dirs[0] = this.Root;
            int[] exploredSubDirectories = new int[mainTableAmount];

            for (ushort i = 1; i < dirs.Length; i++)
            {
                ushort parentIndex = (ushort)(mainTables[i].ParentDirectoryId & 0xFFF);
                Tuple<ushort, string> currentData = mainTables[parentIndex].GetSubDirectory(exploredSubDirectories[parentIndex]++);
                dirs[i] = new FNTDirectory(currentData.Item1);
                dirs[parentIndex].AddChild(currentData.Item2, dirs[i]);
                foreach (Tuple<ushort, string> subFile in mainTables[i].SubFiles)
                {
                    dirs[i].AddChild(subFile.Item2, new FNTFile(subFile.Item1));
                }
            }
        }

        public ushort GetFileId(string path)
        {//#NotFinished -> Comprobar que la id no tiene la forma Fxxx?
            string[] pathParts = path.Split('/');
            Node currentNode = this.Root;
            for (byte i = pathParts[0].Equals("root") ? (byte)1 : (byte)0; i < pathParts.Length; i++)
            {
                currentNode = ((FNTDirectory)currentNode).GetChild(pathParts[i]);
            }
            return currentNode.Id;
        }
    }
}
