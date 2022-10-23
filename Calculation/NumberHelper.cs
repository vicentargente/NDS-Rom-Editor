using System;

namespace NDSRom.Calculation
{
    public class NumberHelper
    {
        #region UInt32
        public static uint LByteArrayToUInt32(byte[] NumberBytes)
        {
            if (NumberBytes == null) throw new ArgumentNullException();
            if (NumberBytes.Length != 4) throw new ArgumentException();
            return (uint)(NumberBytes[0] | (NumberBytes[1] << 8) | (NumberBytes[2] << 16) | (NumberBytes[3] << 24));
        }

        public static uint LBytesToUInt32(byte First, byte Second, byte Third, byte Fourth)
        {
            return (uint)(First | (Second << 8) | (Third << 16) | (Fourth << 24));
        }

        public static byte[] UInt32ToLByteArray(uint Number)
        {
            return new byte[] { (byte)(Number & 0xFF), (byte)((Number >> 8) & 0xFF), (byte)((Number >> 16) & 0xFF), (byte)(Number >> 24) };
        }

        public static void WriteLUint32(uint Number, byte[] Data, uint Position)
        {
            Data[Position] = (byte)(Number & 0xFF);
            Data[Position + 1] = (byte)((Number >> 8) & 0xFF);
            Data[Position + 2] = (byte)((Number >> 16) & 0xFF);
            Data[Position + 3] = (byte)(Number >> 24);
        }
        public static uint ReadLUint32(byte[] Data, uint Position)
        {
            return (uint)(Data[Position] | (Data[Position + 1] << 8) | (Data[Position + 2] << 16) | (Data[Position + 3] << 24));
        }
        #endregion

        #region UInt16
        public static ushort LByteArrayToUInt16(byte[] NumberBytes)
        {
            if (NumberBytes == null) throw new ArgumentNullException();
            if (NumberBytes.Length != 2) throw new ArgumentException();
            return (ushort)(NumberBytes[0] | (NumberBytes[1] << 8));
        }

        public static ushort LBytesToUInt16(byte First, byte Second)
        {
            return (ushort)(First | (Second << 8));
        }

        public static byte[] UInt16ToLByteArray(ushort Number)
        {
            return new byte[] { (byte)(Number & 0xFF), (byte)(Number >> 8) };
        }

        public static void WriteLUint16(ushort Number, byte[] Data, uint Position)
        {
            Data[Position] = (byte)(Number & 0xFF);
            Data[Position + 1] = (byte)(Number >> 8);
        }

        public static ushort ReadLUint16(byte[] Data, uint Position)
        {
            return (ushort)(Data[Position] | (Data[Position + 1] << 8));
        }
        #endregion

        #region Multiple
        public static int NextValidAddress(int Number)
        {
            int nAux = Number;
            if (Number < 0) { nAux = -nAux; /*nAux -= 0x10;*/ }
            byte lastHexDigit = (byte)(nAux & 0xF);
            if (lastHexDigit == 0) return Number;
            nAux += 0x10 - lastHexDigit;
            if (Number < 0) nAux = -nAux;
            return nAux;
        }
        #endregion
    }
}
