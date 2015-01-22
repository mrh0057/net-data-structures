using System.Text;
using Mrh.DataStructures.Buffer;
using NUnit.Framework;

namespace Mrh.TaskOrganizer.DataStructures.Test.Buffer
{
    [TestFixture]
    public class RingbufferTest
    {
        [Test]
        public void InsertGetFailureTest()
        {
            var buffer = new RingBuffer(2000);
            byte[] testValue = Encoding.UTF8.GetBytes("The cat went up the hill.");
            for (int i = 0; i < 1000; i++)
            {
                buffer.Insert(testValue, i);
            }
            Assert.IsNull(buffer.Get(0));
        }

        [Test]
        public void InsertGetSuccessTest()
        {
            var buffer = new RingBuffer(2000);
            byte[] testValue = Encoding.UTF8.GetBytes("The cat went up the hill.");
            for (int i = 0; i < 1000; i++)
            {
                buffer.Insert(testValue, i);
                if (i%8 == 0 &&
                    i > 0)
                {
                    byte[] msg = buffer.Get(i - 3);
                    Assert.AreEqual(testValue, msg);
                }
            }
        }

        [Test]
        [Ignore]
        public void InsertSpeedTest()
        {
            var buffer = new RingBuffer(2000000);
            byte[] testValue =
                Encoding.UTF8.GetBytes(
                    "The cat went up the hill. The cat went up the hill.");
            for (int i = 0; i < 500000000; i++)
            {
                buffer.Insert(testValue, i);
            }
        }
    }
}