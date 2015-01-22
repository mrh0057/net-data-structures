using System;

namespace Mrh.DataStructures.Buffer
{
    public interface IRingBuffer
    {
        /// <summary>
        ///     Used to insert a message in to the ring buffer.  Can only be called from a single thread and the numbers must be in order.
        /// </summary>
        /// <param name="msg">The message to insert.</param>
        /// <param name="number">The number for the message.</param>
        void Insert(byte[] msg, long number);

        /// <summary>
        ///     Used to get a message with the specified id from the ring buffer.
        /// </summary>
        /// <param name="msgId">The id of the message to get.</param>
        /// <returns>The array of bytes for the message. If it wraps returns null.</returns>
        byte[] Get(long msgId);

        /// <summary>
        ///     Used to add a new message handler.  Safe to call from multiple threads.
        /// </summary>
        /// <param name="handler">The handler to add to listen for messages.  Handlers must be light weight and non blocking or you will slow done the ring buffer significantly.</param>
        void AddMessageHandler(Action<byte[], long> handler);
    }
}