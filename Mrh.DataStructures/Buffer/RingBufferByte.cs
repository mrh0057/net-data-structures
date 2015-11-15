using System;
using System.Threading;

namespace Mrh.DataStructures.Buffer
{
    /// <summary>
    ///     A ring buffer designed to hold serialized messages of bytes.  Uses copy for now since speed is not a concern at the
    ///     moment.  Can only have a single writer but it is safe to have multiple readers.
    ///     Designed to hold a serious of messages in order.
    ///     Format for a message is [size(long)][previous(long)][number(long)][msg].
    ///     Would like to have it memory mapped at some point to allow for faster recovery and to make it possible to have the
    ///     part written to task would be another part of the app receiving messages.
    ///     To create a memory mapped file you uses the MemoryMappedFile.CreateFromFile() the you can open the file with differ
    ///     methods like CreateViewAccessor.
    /// </summary>
    public sealed class RingBufferByte : RingBufferByteBase
    {
        private const long _headerOffest = 8 + 8 + 8;
        private const long _sizeOffset = 0;
        private const int _previousOffest = 8;
        private const int _numberOffset = 8 + _previousOffest;
        private readonly byte[] _buffer;
        private readonly long _size;
        private long _currentPosition;
        private long _nextFreeSlot;

        /// <summary>
        ///     Creates a new ring buffer.
        /// </summary>
        /// <param name="size">The number of bytes for the ring buffer to hold.</param>
        public RingBufferByte(long size)
        {
            _size = size;
            _nextFreeSlot = 0;
            _currentPosition = -1;
            _buffer = new byte[_size];
        }

        /// <summary>
        ///     Used to insert a message in to the ring buffer.  Can only be called from a single thread and the numbers must be in
        ///     order.
        /// </summary>
        /// <param name="msg">The message to insert.</param>
        /// <param name="number">The number for the message.</param>
        public override void Insert(byte[] msg, long number)
        {
            var size = (long) msg.Length;
            var totalLength = size + _headerOffest;
            if (totalLength > _size - _nextFreeSlot)
            {
                _nextFreeSlot = 0;
            }

            Array.Copy(BitConverter.GetBytes(size), 0, _buffer, _nextFreeSlot + _sizeOffset, 4);
            Array.Copy(BitConverter.GetBytes(_currentPosition), 0, _buffer, _nextFreeSlot + _previousOffest, 8);
            Array.Copy(BitConverter.GetBytes(number), 0, _buffer, _nextFreeSlot + _numberOffset, 8);
            Array.Copy(msg, 0, _buffer, _nextFreeSlot + _headerOffest, size);

            Interlocked.Exchange(ref _currentPosition, _nextFreeSlot);
            Interlocked.Add(ref _nextFreeSlot, totalLength);

            Fire(msg, number);
        }

        /// <summary>
        ///     Used to get a message with the specified id from the ring buffer.
        /// </summary>
        /// <param name="msgId">The id of the message to get.</param>
        /// <returns>The array of bytes for the message. If it wraps returns null.</returns>
        public override byte[] Get(long msgId)
        {
            var pos = _currentPosition;
            var wraped = false;
            while (true)
            {
                var number = ToInt64(_buffer, pos + _numberOffset);
                if (number == msgId)
                {
                    var size = ToInt64(_buffer, pos);
                    var value = new byte[ToInt64(_buffer, pos)];
                    Array.Copy(_buffer, pos + _headerOffest, value, 0, size);
                    return value;
                }
                pos = ToInt64(_buffer, pos + _previousOffest);
                if (pos == 0)
                {
                    if (wraped)
                    {
                        return null;
                    }
                    wraped = true;
                }
            }
        }

        /// <summary>
        ///     Taken from bit converter to allow long indexes.  Not sure why they haven't fixed this yet.  This
        /// code is licensed under the MIT License and the Copyright is owned by Microsoft.
        /// </summary>
        /// <param name="value">The array to read it from.</param>
        /// <param name="startIndex">The starting index.</param>
        /// <returns>The value of the long.</returns>
        private static unsafe long ToInt64(byte[] value, long startIndex)
        {
            fixed (byte* pbyte = &value[startIndex])
            {
                if (startIndex%8 == 0)
                {
                    // data is aligned
                    return *((long*) pbyte);
                }
                if (BitConverter.IsLittleEndian)
                {
                    var i1 = (*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24);
                    var i2 = (*(pbyte + 4)) | (*(pbyte + 5) << 8) | (*(pbyte + 6) << 16) | (*(pbyte + 7) << 24);
                    return (uint) i1 | ((long) i2 << 32);
                }
                else
                {
                    var i1 = (*pbyte << 24) | (*(pbyte + 1) << 16) | (*(pbyte + 2) << 8) | (*(pbyte + 3));
                    var i2 = (*(pbyte + 4) << 24) | (*(pbyte + 5) << 16) | (*(pbyte + 6) << 8) | (*(pbyte + 7));
                    return (uint) i2 | ((long) i1 << 32);
                }
            }
        }
    }
}