using System;
using System.Diagnostics;
using Mrh.DataStructures.Buffer;
using NUnit.Framework;

namespace Mrh.TaskOrganizer.DataStructures.Test.Buffer
{
    [TestFixture]
    public class RingBufferTest
    {

        [Test]
        public void AddTest()
        {
            var buffer = new RingBuffer<int>(0x100000);
            var iterations = 1000000000;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            for (var i = 0; i < iterations; i++)
            {
                buffer.Set(i, i);
            }
            Console.WriteLine("Message per ms {0}", iterations / stopWatch.ElapsedMilliseconds);
        }
    }
}