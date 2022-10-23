using System;
using System.IO;
using System.Collections.Generic;
using NDSRom.DataStructures.FileNameTable;
using NDSRom.DataStructures.FileAllocationTable;
using NDSRom.DataStructures.Header;
using NDSRom.Tools;

namespace NDSRom
{
    public class Rom
    {
        private string romPath;

        private Header header;
        private FileNameTable fnt;
        private FileAllocationTable fat;

        //Variables para exportar la rom
        private FileAllocationTable modifiedRomFat;
        private Dictionary<ushort, ImportedFile> importedFiles; //Key -> FileId; Value -> FileComputerPath
        private FileCopier fileCopier;
        //---//

        public byte[] GameCode
        {
            get
            {
                return this.header.GameCode;
            }
        }
        public Rom(string path)
        {
            this.romPath = path;
            BinaryReader rom = new BinaryReader(File.OpenRead(path));

            this.header = new Header(rom);
            this.fnt = new FileNameTable(rom);
            this.fat = new FileAllocationTable(rom);

            rom.Close();

            this.modifiedRomFat = this.fat.Clone();
            this.importedFiles = new Dictionary<ushort, ImportedFile>();
            this.fileCopier = FileCopier.Create();
        }

        public void GetFile(string romInternalFilePath, string computerPath)
        {
            ushort fileId = this.fnt.GetFileId(romInternalFilePath);
            FileStream rom = new FileStream(this.romPath, FileMode.Open, FileAccess.Read);
            FileStream exportedFile = new FileStream(computerPath, FileMode.Create, FileAccess.Write);
            this.fileCopier.Copy(rom, this.fat.GetStartOffsetFromFileId(fileId), this.fat.GetSizeFromFileId(fileId), exportedFile);

            exportedFile.Close();
            rom.Close();
        }

        public byte[] GetFile(string romInternalFilePath)
        {
            ushort fileId = this.fnt.GetFileId(romInternalFilePath);
            BinaryReader rom = new BinaryReader(File.OpenRead(this.romPath));

            rom.BaseStream.Position = this.fat.GetStartOffsetFromFileId(fileId);
            uint fileLength = this.fat.GetSizeFromFileId(fileId);

            byte[] res = new byte[fileLength];
            rom.Read(res, 0, res.Length);

            rom.Close();

            return res;
        }

        public void ImportFile(string romInternalFilePath, string fileNewPath)
        {
            this.ImportFile(this.fnt.GetFileId(romInternalFilePath), fileNewPath);
        }
        public void ImportFile(ushort fileId, string fileNewPath)
        {
            this.importedFiles[fileId] = new ExternalImportedFile(fileNewPath, this.fileCopier);
        }
        public void ImportFile(string romInternalFilePath, byte[] newFile)
        {
            this.ImportFile(this.fnt.GetFileId(romInternalFilePath), newFile);
        }
        public void ImportFile(ushort fileId, byte[] newFile)
        {
            this.importedFiles[fileId] = new ByteArrayImportedFile(newFile);
        }

        //Indica si el usuario ha reemplazado ese archivo, de manera que podrá revertir o no el reemplazo
        public bool FileHasBeenReplaced(string romInternalFilePath)
        {
            return this.FileHasBeenReplaced(this.fnt.GetFileId(romInternalFilePath));
        }

        public bool FileHasBeenReplaced(ushort fileId)
        {
            return this.importedFiles.ContainsKey(fileId);
        }

        //Revierte la importación de un archivo, haciendo que quede el original de la rom
        public void RevertImportFile(string romInternalFilePath)
        {
            this.RevertImportFile(this.fnt.GetFileId(romInternalFilePath));
        }

        public void RevertImportFile(ushort fileId)
        {
            this.importedFiles.Remove(fileId);
        }

        public void SaveRom(string newRomPath)
        {
            byte[] structureAux;

            //Preparamos la nueva Fat para la rom
            foreach (KeyValuePair<ushort, ImportedFile> importedFile in this.importedFiles)
            {
                this.modifiedRomFat.ResizeFile(importedFile.Key, importedFile.Value.Size);
            }

            //BinaryWriter newRom = new BinaryWriter(File.OpenWrite(newRomPath));
            BinaryReader rom = new BinaryReader(File.OpenRead(this.romPath));

            FileStream newRom = new FileStream(newRomPath, FileMode.Create, FileAccess.Write);

            //Cabecera se escribe al final

            //Escribe hasta el final de la cabecera 0-0x4000 (provisional)
            this.fileCopier.Copy(rom.BaseStream, 0x180, 0x3E80, newRom, 0x180);

            //Empieza con el codigo
            this.fileCopier.Copy(rom.BaseStream, this.header.ARM9RomOffset, this.header.ARM9CodeSize, newRom, this.header.ARM9RomOffset);
            this.fileCopier.Copy(rom.BaseStream, this.header.ARM9OverlayOffset, this.header.ARM9OverlaySize, newRom, this.header.ARM9OverlayOffset);
            this.fileCopier.Copy(rom.BaseStream, this.header.ARM7RomOffset, this.header.ARM7CodeSize, newRom, this.header.ARM7RomOffset);
            this.fileCopier.Copy(rom.BaseStream, this.header.ARM7OverlayOffset, this.header.ARM7OverlaySize, newRom, this.header.ARM7OverlayOffset);
            //Acaba con el codigo

            //Copia la FAT
            structureAux = this.modifiedRomFat.GetBytes();
            newRom.Position = this.header.FileAllocationTableOffset;
            newRom.Write(structureAux, 0, structureAux.Length);

            //Copia la FNT
            this.fileCopier.Copy(rom.BaseStream, this.header.FileNameTableOffset, this.header.FileNameTableSize, newRom, this.header.FileNameTableOffset);

            //Copia el banner
            this.fileCopier.Copy(rom.BaseStream, this.header.IconBannerOffset, 0x20 + 0x20 + 0x200 + 6 * 0x100, newRom, this.header.IconBannerOffset);

            //Copia los archivos
            ImportedFile auxImportedFile;
            for (ushort fileId = 0; fileId < this.fat.FilesAmount; fileId++)
            {
                if (this.importedFiles.TryGetValue(fileId, out auxImportedFile))
                {
                    auxImportedFile.WriteFileAt(newRom, this.modifiedRomFat.GetStartOffsetFromFileId(fileId));
                }
                else
                {
                    this.fileCopier.Copy(rom.BaseStream, this.fat.GetStartOffsetFromFileId(fileId), this.fat.GetSizeFromFileId(fileId), newRom, this.modifiedRomFat.GetStartOffsetFromFileId(fileId));
                }
            }

            //Cambiamos el tamaño de la rom en la cabecera
            this.header.NtrRegionRomSize = (uint)newRom.Length;

            //Copiamos cabecera
            structureAux = this.header.GetBytes();
            newRom.Position = 0;
            newRom.Write(structureAux, 0, structureAux.Length);

            newRom.Close();
            rom.Close();

            //Console.WriteLine("Copia escrita");
        }
    }
}