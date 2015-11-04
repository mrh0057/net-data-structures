using System.Text;
using Mrh.DataStructures.File;
using NUnit.Framework;

namespace Mrh.TaskOrganizer.DataStructures.Test.File
{
    [TestFixture]
    public class MessageLogTest
    {
        private const string _testFile = @"messageLog.mrhlg";

        [TestFixtureSetUp]
        public void Setup()
        {
            if (System.IO.File.Exists(_testFile))
            {
                System.IO.File.Delete(_testFile);
            }
        }

        [Test]
        public void WriteTest()
        {
            byte[] testValue = Encoding.UTF8.GetBytes("The cat went up the hill.");
            using (var log = new MessageLog(_testFile))
            {
                for (int i = 0; i < 1000; i++)
                {
                    log.Write(testValue, i);
                }
                Assert.AreEqual(testValue, log.GetMessageFromBack(4));
                Assert.AreEqual(testValue, log.GetMessageFromBack(2));
                Assert.AreEqual(testValue, log.GetMessageFromBack(0));
                Assert.IsNull(log.GetMessageFromBack(-1));
                Assert.IsNull(log.GetMessageFromBack(500));

                // Start testing from the front.
                log.GoToFront();
                Assert.AreEqual(0, log.CurrentMessageNumber);
                Assert.AreEqual(0, log.CurrentMessageNumber);
                var next = log.Next();
                Assert.AreEqual(0, next.Item1);
                next = log.Next();
                Assert.AreEqual(1, next.Item1);
                Assert.AreEqual(testValue, log.GetMessageFromFront(500));
                Assert.AreEqual(testValue, log.GetMessageFromFront(999));
                Assert.IsNull(log.GetMessageFromFront(1000));
            }
        }
    }
}