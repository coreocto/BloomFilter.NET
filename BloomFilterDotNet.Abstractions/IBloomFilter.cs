namespace BloomFilterDotNet.Abstractions
{
    using System.Collections;
    using System.Collections.Generic;
    public interface IBloomFilter<T>
    {
        void Add(byte[] bytes);
        void Add(T element);
        void AddAll(ICollection<T> c);
        void Clear();
        bool Contains(byte[] bytes);
        bool Contains(T element);
        bool ContainsAll(ICollection<T> c);
        int Count { get; }
        bool Equals(object obj);
        double GetExpectedFalsePositiveProbability();
        bool GetBit(int bit);
        BitArray BitSet { get; }
        double GetBitsPerElement();
        double ExpectedBitsPerElement { get; }
        int ExpectedNumberOfElements { get; }
        double GetFalsePositiveProbability();
        double GetFalsePositiveProbability(double numberOfElements);
        int GetHashCode();
        int K { get; }
        void SetBit(int bit, bool value);
        int Size { get; }
    }
}