using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LZ4ps;

class LZ4
{
    public static byte[] Compress(byte[] input, int inputOffset = 0, int inputLength = 0)
    {
        if (input == null || input.Length == 0)
        {
            return null;
        }
        if (inputLength == 0)
        {
            inputLength = input.Length;
        }
        try
        {
            byte[] array = LZ4Codec.Encode32(input, inputOffset, inputLength);
            byte[] bytes = BitConverter.GetBytes(inputLength);
            byte[] array2 = new byte[array.Length + bytes.Length];
            Buffer.BlockCopy(bytes, 0, array2, 0, bytes.Length);
            Buffer.BlockCopy(array, 0, array2, bytes.Length, array.Length);
            return array2;
        }
        catch
        {
        }
        return null;
    }

    public static byte[] Decompress(byte[] input, int inputOffset = 0, int inputLength = 0)
    {
        if (input == null || input.Length == 0)
        {
            return null;
        }
        if (inputLength == 0)
        {
            inputLength = input.Length;
        }
        try
        {
            byte[] array = new byte[4];
            Buffer.BlockCopy(input, inputOffset, array, 0, array.Length);
            int outputLength = BitConverter.ToInt32(array, 0);
            return LZ4Codec.Decode32(input, inputOffset + array.Length, inputLength - array.Length, outputLength);
        }
        catch
        {
        }
        return null;
    }
}