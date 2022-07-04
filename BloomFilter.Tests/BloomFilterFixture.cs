using System.Text;

namespace Org.Coreocto.Dev.Tests
{

    [TestFixture]
    public class BloomFilterFixture
    {
        private Random _r;

        [SetUp]
        public void Setup()
        {
            _r = new Random();
        }

        [Test]
        public void Test_Constructor_CNK()
        {
            for (var i = 0; i < 10000; i++)
            {
                var c = (double)_r.Next(20) + 1;
                var n = _r.Next(10000) + 1;
                var k = _r.Next(20) + 1;
                var bf = new BloomFilter<string>(c, n, k);
                Assert.That(bf.K, Is.EqualTo(k));
                Assert.That(c, Is.EqualTo(bf.ExpectedBitsPerElement));
                Assert.That(n, Is.EqualTo(bf.ExpectedNumberOfElements));
                Assert.That(c * n, Is.EqualTo(bf.Size));
            }
        }

        [Test]
        public void Test_CreateHash_String()
        {
            var val = $"{Guid.NewGuid()}";
            var result1 = BloomFilter<string>.CreateHash(val);
            var result2 = BloomFilter<string>.CreateHash(val);
            Assert.AreEqual(result2, result1);
            var result3 = BloomFilter<string>.CreateHash($"{Guid.NewGuid()}");
            Assert.AreNotEqual(result3, result2);

            var result4 = BloomFilter<string>.CreateHash(Encoding.UTF8.GetBytes(val));
            Assert.AreEqual(result4, result1);
        }

        [Test]
        public void Test_CreateHash_byteArr()
        {
            var val = $"{Guid.NewGuid()}";
            var data = Encoding.UTF8.GetBytes(val);
            var result1 = BloomFilter<byte[]>.CreateHash(data);
            var result2 = BloomFilter<byte[]>.CreateHash(val);
            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void Test_CreateHashes_byteArr()
        {
            var val = $"{Guid.NewGuid()}";
            var data = Encoding.UTF8.GetBytes(val);
            var result1 = BloomFilter<byte[]>.CreateHashes(data, 10);
            var result2 = BloomFilter<byte[]>.CreateHashes(data, 10);
            Assert.AreEqual(result1.Length, 10);
            Assert.AreEqual(result2.Length, 10);
            Assert.True(result1.SequenceEqual(result2));
            var result3 = BloomFilter<byte[]>.CreateHashes(data, 5);
            Assert.AreEqual(result3.Length, 5);
            for (int i = 0; i < result3.Length; i++)
                Assert.AreEqual(result3[i], result1[i]);

        }

        [Test]
        public void Test_Equals()
        {
            var instance1 = new BloomFilter<string>(1000, 100);
            var instance2 = new BloomFilter<string>(1000, 100);

            for (int i = 0; i < 100; i++)
            {
                var val = $"{Guid.NewGuid()}";
                instance1.Add(val);
                instance2.Add(val);
            }
            Assert.AreEqual(instance1, instance2);
            Assert.AreEqual(instance2, instance1);

            instance1.Add("Another entry"); // make instance1 and instance2 different before clearing

            instance1.Clear();
            instance2.Clear();

            Assert.AreEqual(instance1, instance2);
            Assert.AreEqual(instance2, instance1);

            for (int i = 0; i < 100; i++)
            {
                var val = $"{Guid.NewGuid()}";
                instance1.Add(val);
                instance2.Add(val);
            }

            Assert.AreEqual(instance1, instance2);
            Assert.AreEqual(instance2, instance1);
        }

        [Test]
        public void Test_HashCode()
        {

            var instance1 = new BloomFilter<string>(1000, 100);
            var instance2 = new BloomFilter<string>(1000, 100);

            Assert.True(instance1.GetHashCode() == instance2.GetHashCode());

            for (int i = 0; i < 100; i++)
            {
                var val = $"{Guid.NewGuid()}";
                instance1.Add(val);
                instance2.Add(val);
            }

            Assert.True(instance1.GetHashCode() == instance2.GetHashCode());

            instance1.Clear();
            instance2.Clear();

            Assert.True(instance1.GetHashCode() == instance2.GetHashCode());

            instance1 = new BloomFilter<string>(100, 10);
            instance2 = new BloomFilter<string>(100, 9);
            Assert.False(instance1.GetHashCode() == instance2.GetHashCode());

            instance1 = new BloomFilter<string>(100, 10);
            instance2 = new BloomFilter<string>(99, 9);
            Assert.False(instance1.GetHashCode() == instance2.GetHashCode());

            instance1 = new BloomFilter<string>(100, 10);
            instance2 = new BloomFilter<string>(50, 10);
            Assert.False(instance1.GetHashCode() == instance2.GetHashCode());
        }

        [Test]
        public void Test_ExpectedFalsePositiveProbability()
        {
            // These probabilities are taken from the bloom filter probability table at
            // http://pages.cs.wisc.edu/~cao/papers/summary-cache/node8.html

            var instance = new BloomFilter<string>(1000, 100);
            var expResult = 0.00819; // m/n=10, k=7
            var result = instance.GetExpectedFalsePositiveProbability();
            Assert.AreEqual(instance.K, 7);
            Assert.That(expResult, Is.EqualTo(result).Within(0.000009));

            instance = new BloomFilter<string>(100, 10);
            expResult = 0.00819; // m/n=10, k=7
            result = instance.GetExpectedFalsePositiveProbability();
            Assert.AreEqual(instance.K, 7);
            Assert.That(expResult, Is.EqualTo(result).Within(0.000009));

            instance = new BloomFilter<string>(20, 10);
            expResult = 0.393; // m/n=2, k=1
            result = instance.GetExpectedFalsePositiveProbability();
            Assert.AreEqual(1, instance.K);
            Assert.That(expResult, Is.EqualTo(result).Within(0.0005));

            instance = new BloomFilter<string>(110, 10);
            expResult = 0.00509; // m/n=11, k=8
            result = instance.GetExpectedFalsePositiveProbability();
            Assert.AreEqual(8, instance.K);
            Assert.That(expResult, Is.EqualTo(result).Within(0.00001));
        }

        [Test]
        public void Test_Clear()
        {
            var instance = new BloomFilter<string>(1000, 100);
            for (int i = 0; i < instance.Size; i++)
                instance.SetBit(i, true);
            instance.Clear();
            for (int i = 0; i < instance.Size; i++)
                Assert.AreEqual(instance.GetBit(i), false);
        }

        [Test]
        public void Test_Add()
        {
            var instance = new BloomFilter<string>(1000, 100);

            for (int i = 0; i < 100; i++)
            {
                var val = $"{Guid.NewGuid()}";
                instance.Add(val);
                Assert.True(instance.Contains(val));
            }
        }

        [Test]
        public void Test_AddAll()
        {
            var v = new List<string>();
            var instance = new BloomFilter<string>(1000, 100);

            for (int i = 0; i < 100; i++)
                v.Add($"{Guid.NewGuid()}");

            instance.AddAll(v);

            for (int i = 0; i < 100; i++)
                Assert.True(instance.Contains(v[i]));
        }

        [Test]
        public void Test_Contains()
        {
            var instance = new BloomFilter<string>(10000, 10);

            for (int i = 0; i < 10; i++)
            {
                instance.Add(Convert.ToString(i, 2));
                Assert.IsTrue(instance.Contains(Convert.ToString(i, 2)));
            }

            Assert.False(instance.Contains($"{Guid.NewGuid()}"));
        }

        [Test]
        public void Test_Contains_All()
        {
            var v = new List<string>();
            var instance = new BloomFilter<string>(1000, 100);

            for (int i = 0; i < 100; i++)
            {
                v.Add($"{Guid.NewGuid()}");
                instance.Add(v[i]);
            }

            Assert.True(instance.ContainsAll(v));
        }

        [Test]
        public void Test_GetBit()
        {
            var instance = new BloomFilter<string>(1000, 100);
            var r = new Random();

            for (int i = 0; i < 100; i++)
            {
                var b = r.Next(2) == 1;
                instance.SetBit(i, b);
                Assert.AreEqual(instance.GetBit(i), b);
            }
        }

        [Test]
        public void Test_SetBit()
        {
            var instance = new BloomFilter<string>(1000, 100);

            for (int i = 0; i < 100; i++)
            {
                instance.SetBit(i, true);
                Assert.AreEqual(instance.GetBit(i), true);
            }

            for (int i = 0; i < 100; i++)
            {
                instance.SetBit(i, false);
                Assert.AreEqual(instance.GetBit(i), false);
            }
        }

        [Test]
        public void Test_Size()
        {
            for (int i = 100; i < 1000; i++)
            {
                var instance = new BloomFilter<string>(i, 10);
                Assert.AreEqual(instance.Size, i);
            }
        }

        [Test]
        public void Test_FalsePositiveRate1()
        {
            // Numbers are from // http://pages.cs.wisc.edu/~cao/papers/summary-cache/node8.html

            for (int j = 10; j < 21; j++)
            {
                TestContext.Out.Write(j - 9 + "/11");
                var v = new List<byte[]>();
                var instance = new BloomFilter<byte[]>(100 * j, 100);

                for (int i = 0; i < 100; i++)
                {
                    var bytes = new byte[100];
                    _r.NextBytes(bytes);
                    v.Add(bytes);
                }
                instance.AddAll(v);

                var f = 0;
                var tests = 300000;
                for (var i = 0; i < tests; i++)
                {
                    var bytes = new byte[100];
                    _r.NextBytes(bytes);
                    if (instance.Contains(bytes))
                    {
                        if (!v.Contains(bytes))
                        {
                            f++;
                        }
                    }
                }

                var ratio = f / tests;

                TestContext.Out.WriteLine(" - got " + ratio + ", math says " + instance.GetExpectedFalsePositiveProbability());
                Assert.That(instance.GetExpectedFalsePositiveProbability(), Is.EqualTo(ratio).Within(0.01));
            }
        }

        [Test]
        public void Test_GetK()
        {
            // Numbers are from http://pages.cs.wisc.edu/~cao/papers/summary-cache/node8.html
            
            BloomFilter<string> instance;

            instance = new BloomFilter<string>(2, 1);
            Assert.AreEqual(1, instance.K);

            instance = new BloomFilter<string>(3, 1);
            Assert.AreEqual(2, instance.K);

            instance = new BloomFilter<string>(4, 1);
            Assert.AreEqual(3, instance.K);

            instance = new BloomFilter<string>(5, 1);
            Assert.AreEqual(3, instance.K);

            instance = new BloomFilter<string>(6, 1);
            Assert.AreEqual(4, instance.K);

            instance = new BloomFilter<string>(7, 1);
            Assert.AreEqual(5, instance.K);

            instance = new BloomFilter<string>(8, 1);
            Assert.AreEqual(6, instance.K);

            instance = new BloomFilter<string>(9, 1);
            Assert.AreEqual(6, instance.K);

            instance = new BloomFilter<string>(10, 1);
            Assert.AreEqual(7, instance.K);

            instance = new BloomFilter<string>(11, 1);
            Assert.AreEqual(8, instance.K);

            instance = new BloomFilter<string>(12, 1);
            Assert.AreEqual(8, instance.K);
        }

        [Test]
        public void Test_Contains_GenericType()
        {
            var items = 100;
            var instance = new BloomFilter<string>(0.01, items);

            for (var i = 0; i < items; i++)
            {
                var s = $"{Guid.NewGuid()}";
                instance.Add(s);
                Assert.True(instance.Contains(s));
            }
        }

        [Test]
        public void Test_Contains_byteArr()
        {
            var items = 100;
            var instance = new BloomFilter<byte[]>(0.01, items);

            for (int i = 0; i < items; i++)
            {
                var bytes = new byte[500];
                _r.NextBytes(bytes);
                instance.Add(bytes);
                Assert.True(instance.Contains(bytes));
            }
        }

        [Test]
        public void Test_Count()
        {
            var expResult = 100;
            var instance = new BloomFilter<byte[]>(0.01, expResult);
            for (var i = 0; i < expResult; i++)
            {
                var bytes = new byte[100];
                _r.NextBytes(bytes);
                instance.Add(bytes);
            }
            var result = instance.Count;
            Assert.AreEqual(expResult, result);

            var instance2 = new BloomFilter<string>(0.01, expResult);
            for (var i = 0; i < expResult; i++)
            {
                instance2.Add($"{Guid.NewGuid()}");
            }
            result = instance2.Count;
            Assert.AreEqual(expResult, result);
        }
    }
}