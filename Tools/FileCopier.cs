using System.IO;

namespace NDSRom.Tools
{
    public class FileCopier
    {
        //Default buffer size (128kB)
        private const uint DEFAULT_BUFFER_SIZE = 0x20000;

        //Singleton
        private static FileCopier Instance;
        public static FileCopier Create(uint bufferSize = DEFAULT_BUFFER_SIZE) {
            if (Instance == null) Instance = new FileCopier(bufferSize);
            return Instance;
        }
        public static FileCopier GetInstance()
        {
            return Instance;
        }

        private byte[] buffer;

        private FileCopier(uint bufferSize)
        {
            this.buffer = new byte[bufferSize];
        }

        public void Copy(Stream input, Stream output)
        {
            Copy(input, 0, (uint)input.Length, output, 0);
        }

        public void Copy(Stream input, uint inputBegining, uint inputSize, Stream output, uint outputBegining = 0)
        {
            long inputPosAux = input.Position;
            long outputPosAux = output.Position;

            input.Position = inputBegining;
            output.Position = outputBegining;

            ushort nIters = (ushort)(inputSize / this.buffer.Length);
            int excess = (int)(inputSize % this.buffer.Length);
            for (ushort i = 0; i < nIters; i++)
            {
                input.Read(this.buffer, 0, this.buffer.Length);
                output.Write(this.buffer, 0, this.buffer.Length);
            }
            input.Read(this.buffer, 0, excess);
            output.Write(this.buffer, 0, excess);

            input.Position = inputPosAux;
            output.Position = outputPosAux;
        }
    }
}