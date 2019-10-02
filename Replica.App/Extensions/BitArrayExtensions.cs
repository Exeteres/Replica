using System;
using System.Collections;

namespace Replica.App.Extensions
{
    public static class BitArrayExtensions
    {
        public static int ToInt(this BitArray bitArray)
        {

            if (bitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];

        }
    }
}