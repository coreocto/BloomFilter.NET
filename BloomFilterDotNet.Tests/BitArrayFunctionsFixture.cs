using System.Collections;

namespace BloomFilterDotNet.Tests
{
    [TestFixture]
    public class BitArrayFunctionsFixture
    {

        [Test]
        public void Ensure_GetBitArrayHashCode_Returns_HashCode_Correctly()
        {
            var bitArray1 = new BitArray(4);
            var bitArray2 = new BitArray(4);
            var bitArray3 = new BitArray(4);
            var bitArray4 = new BitArray(5);

            bitArray1[0] = true;
            bitArray1[1] = true;
            bitArray1[2] = true;
            bitArray2[0] = true;
            bitArray2[1] = true;
            bitArray2[2] = true;
            bitArray3[0] = false;
            bitArray3[1] = true;
            bitArray3[2] = true;

            var hashCode1 = BitArrayFunctions.GetBitArrayHashCode(bitArray1);
            var hashCode2 = BitArrayFunctions.GetBitArrayHashCode(bitArray2);
            var hashCode3 = BitArrayFunctions.GetBitArrayHashCode(bitArray3);
            var hashCode4 = BitArrayFunctions.GetBitArrayHashCode(bitArray4);

            Assert.AreEqual(hashCode1, hashCode2);
            Assert.AreNotEqual(hashCode1, hashCode3);
            Assert.AreNotEqual(hashCode1, hashCode4);
        }

        [Test]
        public void Ensure_BitArrayEquals_Compare_Bits_Equalness()
        {
            var bitArray1 = new BitArray(1000);
            var bitArray2 = new BitArray(1000);
            var bitArray3 = new BitArray(1000);

            bitArray1[1] = true;
            bitArray2[1] = true;

            Assert.IsTrue(BitArrayFunctions.BitArrayEquals(bitArray1, bitArray2));
            Assert.IsFalse(BitArrayFunctions.BitArrayEquals(bitArray1, bitArray3));
        }
    }
}
