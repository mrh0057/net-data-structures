using System.Threading;

namespace Mrh.DataStructures
{
    /// <summary>
    ///     Used to generate a sequence.  Currently the implementation is very simple and is designed to be used with the ring
    ///     buffer.
    /// </summary>
    public class SequenceGenerator
    {
        private long _number;

        /// <summary>
        ///     Used to create a new sequence generator.
        /// </summary>
        /// <param name="start"></param>
        public SequenceGenerator(int start)
        {
            _number = start;
        }

        /// <summary>
        ///     Used to get the next number in the sequence.  Safe to call from multiple threads.
        /// </summary>
        /// <returns>The next number in the sequence.</returns>
        public long Next()
        {
            return Interlocked.Increment(ref _number);
        }
    }
}