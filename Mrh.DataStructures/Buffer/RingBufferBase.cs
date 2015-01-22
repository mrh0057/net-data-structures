using System;
using System.Collections.Generic;
using System.Threading;

namespace Mrh.DataStructures.Buffer
{
    /// <summary>
    ///     A very basic ring buffer base class.  For speed purposes all derived classes should be sealed to let the runtime make more optimizations.
    /// </summary>
    public abstract class RingBufferBase : IRingBuffer
    {
        private readonly List<Action<byte[], long>> _handlers = new List<Action<byte[], long>>(10);
        private Action<byte[], long>[] _msgHandlers = new Action<byte[], long>[0];

        /// <summary>
        ///     Fires when a message is received.
        /// </summary>
        /// <param name="msg">The message that's received.</param>
        /// <param name="number">The number of the message.</param>
        protected void Fire(byte[] msg, long number)
        {
            // Used for speed reasons.
            for (var i = 0; i < _msgHandlers.Length; i++)
            {
                _msgHandlers[i].Invoke(msg, number);
            }
        }

        public abstract void Insert(byte[] msg, long number);

        public abstract byte[] Get(long msgId);

        /// <summary>
        ///     Used to add a new message handler.  Safe to call from multiple threads.
        /// </summary>
        /// <param name="handler">The handler to add to listen for messages.  Handlers must be light weight and non blocking or you will slow done the ring buffer significantly.</param>
        public void AddMessageHandler(Action<byte[], long> handler)
        {
            _handlers.Add(handler);
            Interlocked.Exchange(ref _msgHandlers, _handlers.ToArray());
        }


    }
}