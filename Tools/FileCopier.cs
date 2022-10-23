using System.IO;

namespace NDSRom.Tools
{
    public class FileCopier
    {
        //Singleton
        private static FileCopier Instance;
        public static FileCopier GetInstance(uint BufferSize = 0x20000)
        {
            if (Instance == null) Instance = new FileCopier(BufferSize);
            return Instance;
        }
        //

        private byte[] Buffer;

        private FileCopier(uint BufferSize)
        {
            this.Buffer = new byte[BufferSize];
        }

        public void Copy(Stream Input, Stream Output)
        {
            Copy(Input, 0, (uint)Input.Length, Output, 0);
        }

        public void Copy(Stream Input, uint InputBegining, uint InputSize, Stream Output, uint OutputBegining = 0)
        {
            long inputPosAux = Input.Position;
            long outputPosAux = Output.Position;

            Input.Position = InputBegining;
            Output.Position = OutputBegining;

            ushort nIters = (ushort)(InputSize / this.Buffer.Length);
            int excess = (int)(InputSize % this.Buffer.Length);
            for (ushort i = 0; i < nIters; i++)
            {
                Input.Read(this.Buffer, 0, this.Buffer.Length);
                Output.Write(this.Buffer, 0, this.Buffer.Length);
            }
            Input.Read(this.Buffer, 0, excess);
            Output.Write(this.Buffer, 0, excess);

            Input.Position = inputPosAux;
            Output.Position = outputPosAux;
        }
    }
}