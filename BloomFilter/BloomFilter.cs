using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BloomFilter.Tests")]
namespace Org.Coreocto.Dev
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public class BloomFilter<T> : IBloomFilter<T>
    {
        public BitArray BitSet { get; internal set; }
        public int Size { get; internal set; }
        public double ExpectedBitsPerElement { get; internal set; }
        public int ExpectedNumberOfElements { get; internal set; }
        public int Count { get; internal set; }
        public int K { get; internal set; }

        private static readonly Encoding charset = Encoding.UTF8;
        private static readonly HashAlgorithm _digestFunction = MD5.Create();

        public BloomFilter(double c, int n, int k)
        {
            ExpectedNumberOfElements = n;
            K = k;
            ExpectedBitsPerElement = c;
            Size = (int)Math.Ceiling(c * n);
            Count = 0;
            BitSet = new BitArray(Size);
        }

        public BloomFilter(int bitSetSize, int expectedNumberOElements) : this(bitSetSize / (double)expectedNumberOElements,
                                                                             expectedNumberOElements,
                                                                             (int)Math.Round((bitSetSize / (double)expectedNumberOElements) * Math.Log(2.0)))
        {
        }

        public BloomFilter(double falsePositiveProbability, int expectedNumberOfElements) : this(Math.Ceiling(-(Math.Log(falsePositiveProbability) / Math.Log(2))) / Math.Log(2), // c = k / ln(2)
                 expectedNumberOfElements,
                 (int)Math.Ceiling(-(Math.Log(falsePositiveProbability) / Math.Log(2)))) // k = ceil(-log_2(false prob.))
        {
        }

        public BloomFilter(int bitSetSize, int expectedNumberOfFilterElements, int actualNumberOfFilterElements, BitArray filterData) : this(bitSetSize, expectedNumberOfFilterElements)
        {
            BitSet = filterData;
            Count = actualNumberOfFilterElements;
        }

        public static int CreateHash(string val, Encoding charset)
        {
            return CreateHash(charset.GetBytes(val));
        }

        public static int CreateHash(string val)
        {
            return CreateHash(val, charset);
        }

        public static int CreateHash(byte[] data)
        {
            return CreateHashes(data, 1)[0];
        }

        public static int[] CreateHashes(byte[] data, int hashes)
        {
            var result = new int[hashes];

            var k = 0;
            var salt = (byte)0;
            while (k < hashes)
            {
                byte[] digest;

                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Write(new byte[] { salt }, 0, 1);
                    memoryStream.Write(data);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    salt++;
                    digest = _digestFunction.ComputeHash(memoryStream.ToArray());
                }

                for (var i = 0; i < digest.Length / 4 && k < hashes; i++)
                {
                    var h = 0;
                    for (var j = (i * 4); j < (i * 4) + 4; j++)
                    {
                        h <<= 8;
                        h |= ((int)digest[j]) & 0xFF;
                    }
                    result[k] = h;
                    k++;
                }
            }
            return result;
        }

        public double GetExpectedFalsePositiveProbability()
        {
            return GetFalsePositiveProbability(ExpectedNumberOfElements);
        }

        public double GetFalsePositiveProbability(double numberOfElements)
        {
            // (1 - e^(-k * n / m)) ^ k
            return Math.Pow((1 - Math.Exp(-K * (double)numberOfElements
                            / (double)Size)), K);

        }

        public double GetFalsePositiveProbability()
        {
            return GetFalsePositiveProbability(Count);
        }

        public void Clear()
        {
            BitSet.SetAll(false);
            Count = 0;
        }

        public void Add(T element)
        {
            Add(charset.GetBytes(element.ToString()));
        }

        public void Add(byte[] bytes)
        {
            var hashes = CreateHashes(bytes, K);
            foreach (var hash in hashes)
                BitSet.Set(Math.Abs(hash % Size), true);
            Count++;
        }

        public void AddAll(ICollection<T> c)
        {
            foreach (var element in c)
                Add(element);
        }

        public bool Contains(T element)
        {
            return Contains(charset.GetBytes(element.ToString()));
        }

        public bool Contains(byte[] bytes)
        {
            int[] hashes = CreateHashes(bytes, K);
            foreach (int hash in hashes)
            {
                if (!BitSet.Get(Math.Abs(hash % Size)))
                {
                    return false;
                }
            }
            return true;
        }

        public bool ContainsAll(ICollection<T> c)
        {
            foreach (var element in c)
                if (!Contains(element))
                    return false;
            return true;
        }

        public bool GetBit(int bit)
        {
            return BitSet.Get(bit);
        }

        public void SetBit(int bit, bool value)
        {
            BitSet.Set(bit, value);
        }

        public double GetBitsPerElement()
        {
            return Size / (double)Count;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            var other = (BloomFilter<T>)obj;

            if (ExpectedNumberOfElements != other.ExpectedNumberOfElements || K != other.K || Size != other.Size)
                return false;

            if (BitSet != other.BitSet && (BitSet == null || !BitArrayFunctions.BitArrayEquals(BitSet, other.BitSet)))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            var hash = 7;
            hash = 61 * hash + Size;
            hash = 61 * hash + ExpectedNumberOfElements;
            hash = 61 * hash + K;
            hash = 61 * hash + BitArrayFunctions.BitArrayHashCode(BitSet);
            return hash;
        }
    }
}
