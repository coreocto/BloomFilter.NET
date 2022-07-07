namespace BloomFilterDotNet
{
    using System;
    using System.Collections;

    public static class BitArrayFunctions
    {
        public static int GetBitArrayHashCode(IEnumerable input)
        {
            var hash = 7;
            var i = 0;
            foreach (var b in input)
            {
                hash = 61 * hash + (((bool)b) ? 1 : 0);
                i++;
            }
            return hash;
        }

        public static int BitArrayHashCode(BitArray input)
        {
            var hash = 7;
            hash = 61 * hash + (input.IsSynchronized ? 1 : 0);
            hash = 61 * hash + (input.Count);
            hash = 61 * hash + (input.Length);
            hash = 61 * hash + (input.IsReadOnly ? 1 : 0);
            hash = 61 * hash + GetBitArrayHashCode(input);

            return hash;
        }

        public static bool BitArrayEquals(BitArray first, BitArray second)
        {
            // Short-circuit if the arrays are not equal in size
            if (first.Length != second.Length)
                return false;

            // Convert the arrays to int[]s
            var firstInts = new int[(int)Math.Ceiling((decimal)first.Count / 32)];
            first.CopyTo(firstInts, 0);
            var secondInts = new int[(int)Math.Ceiling((decimal)second.Count / 32)];
            second.CopyTo(secondInts, 0);

            // Look for differences
            var areDifferent = false;
            for (var i = 0; i < firstInts.Length && !areDifferent; i++)
                areDifferent = firstInts[i] != secondInts[i];

            return !areDifferent;
        }
    }
}
