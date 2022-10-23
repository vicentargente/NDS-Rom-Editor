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
        //private const uint MAXIMUM_SMALL_COPY_BUFFER_SIZE = 0x40000; //256kB
        //private const uint MAXIMUM_BIG_COPY_BUFFER_SIZE = 0x800000; //8MB

        private string RomPath;

        private Header Header;
        private FileNameTable Fnt;
        private FileAllocationTable Fat;

        //Variables para exportar la rom
        private FileAllocationTable ModifiedRomFat;
        private Dictionary<ushort, ImportedFile> ImportedFiles; //Key -> FileId; Value -> FileComputerPath
        private FileCopier FileCopier;
        //---//

        public byte[] GameCode
        {
            get
            {
                return this.Header.GameCode;
            }
        }
        public Rom(string ComputerPath)
        {
            this.RomPath = ComputerPath;
            BinaryReader rom = new BinaryReader(File.OpenRead(ComputerPath));

            this.Header = new Header(rom);
            this.Fnt = new FileNameTable(rom);
            this.Fat = new FileAllocationTable(rom);

            rom.Close();

            this.ModifiedRomFat = this.Fat.Clone();
            this.ImportedFiles = new Dictionary<ushort, ImportedFile>();
            this.FileCopier = FileCopier.GetInstance();
        }

        public void GetFile(string RomInternalFilePath, string ComputerPath)
        {
            ushort fileId = this.Fnt.GetFileId(RomInternalFilePath);
            FileStream rom = new FileStream(this.RomPath, FileMode.Open, FileAccess.Read);
            FileStream exportedFile = new FileStream(ComputerPath, FileMode.Create, FileAccess.Write);
            this.FileCopier.Copy(rom, this.Fat.GetStartOffsetFromFileId(fileId), this.Fat.GetSizeFromFileId(fileId), exportedFile);

            exportedFile.Close();
            rom.Close();
        }

        public byte[] GetFile(string RomInternalFilePath)
        {
            ushort fileId = this.Fnt.GetFileId(RomInternalFilePath);
            BinaryReader rom = new BinaryReader(File.OpenRead(this.RomPath));

            rom.BaseStream.Position = this.Fat.GetStartOffsetFromFileId(fileId);
            uint fileLength = this.Fat.GetSizeFromFileId(fileId);

            byte[] res = new byte[fileLength];
            rom.Read(res, 0, res.Length);

            rom.Close();

            return res;
        }

        public void ImportFile(string RomInternalFilePath, string FileNewPath)
        {
            this.ImportFile(this.Fnt.GetFileId(RomInternalFilePath), FileNewPath);
        }
        public void ImportFile(ushort FileId, string FileNewPath)
        {
            this.ImportedFiles[FileId] = new ExternalImportedFile(FileNewPath, this.FileCopier);
        }
        public void ImportFile(string RomInternalFilePath, byte[] NewFile)
        {
            this.ImportFile(this.Fnt.GetFileId(RomInternalFilePath), NewFile);
        }
        public void ImportFile(ushort FileId, byte[] NewFile)
        {
            this.ImportedFiles[FileId] = new ByteArrayImportedFile(NewFile);
        }

        //Indica si el usuario ha reemplazado ese archivo, de manera que podrá revertir o no el reemplazo
        public bool FileHasBeenReplaced(string RomInternalFilePath)
        {
            return this.FileHasBeenReplaced(this.Fnt.GetFileId(RomInternalFilePath));
        }

        public bool FileHasBeenReplaced(ushort FileId)
        {
            return this.ImportedFiles.ContainsKey(FileId);
        }

        //Revierte la importación de un archivo, haciendo que quede el original de la rom
        public void RevertImportFile(string RomInternalFilePath)
        {
            this.RevertImportFile(this.Fnt.GetFileId(RomInternalFilePath));
        }

        public void RevertImportFile(ushort FileId)
        {
            this.ImportedFiles.Remove(FileId);
        }

        public void SaveRom(string NewRomPath)
        {
            //Console.WriteLine("Copia empezada");

            //Preparamos la nueva Fat para la rom
            foreach (KeyValuePair<ushort, ImportedFile> importedFile in this.ImportedFiles)
            {
                this.ModifiedRomFat.ResizeFile(importedFile.Key, importedFile.Value.Size);
            }

            BinaryWriter newRom = new BinaryWriter(File.OpenWrite(NewRomPath));
            BinaryReader rom = new BinaryReader(File.OpenRead(this.RomPath));

            //Cabecera se escribe al final

            //Escribe hasta el final de la cabecera 0-0x4000 (provisional)
            this.FileCopier.Copy(rom.BaseStream, 0x180, 0x3E80, newRom.BaseStream, 0x180);

            //Empieza con el codigo
            this.FileCopier.Copy(rom.BaseStream, this.Header.ARM9RomOffset, this.Header.ARM9CodeSize, newRom.BaseStream, this.Header.ARM9RomOffset);
            this.FileCopier.Copy(rom.BaseStream, this.Header.ARM9OverlayOffset, this.Header.ARM9OverlaySize, newRom.BaseStream, this.Header.ARM9OverlayOffset);
            this.FileCopier.Copy(rom.BaseStream, this.Header.ARM7RomOffset, this.Header.ARM7CodeSize, newRom.BaseStream, this.Header.ARM7RomOffset);
            this.FileCopier.Copy(rom.BaseStream, this.Header.ARM7OverlayOffset, this.Header.ARM7OverlaySize, newRom.BaseStream, this.Header.ARM7OverlayOffset);
            //Acaba con el codigo

            //Copia la FAT
            newRom.BaseStream.Position = this.Header.FileAllocationTableOffset;
            newRom.Write(this.ModifiedRomFat.GetBytes());

            //Copia la FNT
            this.FileCopier.Copy(rom.BaseStream, this.Header.FileNameTableOffset, this.Header.FileNameTableSize, newRom.BaseStream, this.Header.FileNameTableOffset);

            //Copia el banner
            this.FileCopier.Copy(rom.BaseStream, this.Header.IconBannerOffset, 0x20 + 0x20 + 0x200 + 6 * 0x100, newRom.BaseStream, this.Header.IconBannerOffset);

            //Copia los archivos
            ImportedFile auxImportedFile;
            for (ushort fileId = 0; fileId < this.Fat.FilesAmount; fileId++)
            {
                if (this.ImportedFiles.TryGetValue(fileId, out auxImportedFile))
                {
                    auxImportedFile.WriteFileAt(newRom, this.ModifiedRomFat.GetStartOffsetFromFileId(fileId));
                }
                else
                {
                    this.FileCopier.Copy(rom.BaseStream, this.Fat.GetStartOffsetFromFileId(fileId), this.Fat.GetSizeFromFileId(fileId), newRom.BaseStream, this.ModifiedRomFat.GetStartOffsetFromFileId(fileId));
                }
            }

            //Cambiamos el tamaño de la rom en la cabecera
            this.Header.NtrRegionRomSize = (uint)newRom.BaseStream.Length;

            //Copiamos cabecera
            newRom.BaseStream.Position = 0;
            newRom.Write(this.Header.GetBytes());

            newRom.Close();
            rom.Close();

            //Console.WriteLine("Copia escrita");
        }
    }
}