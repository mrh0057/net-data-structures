using System.Text;
using Mrh.DataStructures.Buffer;
using NUnit.Framework;

namespace Mrh.TaskOrganizer.DataStructures.Test.Buffer
{
    [TestFixture]
    public class RingBufferMemoryMappedTest
    {
        private string _testFile = @"memoryMappedTest.mrhmmf";
        private string _speedTestFile = @"memoryMappedSpeedTest.mrhmmf";

        [Test]
        public void WriteTests()
        {
            if (System.IO.File.Exists(_testFile))
            {
                System.IO.File.Delete(_testFile);
            }
            using (var buffer = new RingBufferMemoryMap(10000L, _testFile, "bufferTest"))
            {
                var testArray = Encoding.UTF8.GetBytes("Cat went up the hill.");
                for (var i = 0; i < 10000; i++)
                {
                    buffer.Insert(testArray, i);
                }

                Assert.AreEqual(testArray, buffer.Get(9995));
                Assert.IsNull(buffer.Get(0));
                Assert.AreEqual(9999, buffer.LastNumber);
                Assert.IsNull(buffer.Get(9756));
            }
            using (var buffer = new RingBufferMemoryMap(10000L, _testFile, "bufferTest"))
            {
                Assert.AreEqual(9999, buffer.LastNumber);
            }
        }

        [Test]
        public void WriteNoWrapTest()
        {
            if (System.IO.File.Exists(_testFile))
            {
                System.IO.File.Delete(_testFile);
            }
            using (var buffer = new RingBufferMemoryMap(10000L, _testFile, "bufferTest"))
            {
                var testArray = Encoding.UTF8.GetBytes("Cat went up the hill.");
                for (var i = 0; i < 25; i++)
                {
                    buffer.Insert(testArray, i);
                }
                Assert.AreEqual(24, buffer.LastNumber);
            }
            using (var buffer = new RingBufferMemoryMap(10000L, _testFile, "bufferTest"))
            {
                Assert.AreEqual(24, buffer.LastNumber);
            }
        }

        [Test]
        [Ignore]
        public void WriteSpeedTest()
        {
            using (var buffer = new RingBufferMemoryMap(1000000L, _speedTestFile, "bufferTest"))
            {
                var testArray = Encoding.UTF8.GetBytes("Cat went up the hill.");
                // 10 Million
                for (var i = 0; i < 10000000; i++)
                {
                    buffer.Insert(testArray, i);
                }
            }
        }
    }
}