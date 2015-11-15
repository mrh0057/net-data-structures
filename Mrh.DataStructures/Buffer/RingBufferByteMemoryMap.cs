using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace Mrh.DataStructures.Buffer
{
    /// <summary>
    ///     Creates a ring buffer that's backed by a memory mapped file.
    ///     File format is [number(long)][previous(long)][size(int)][msg(bytes)][-1next number(long)]
    /// </summary>
    public sealed class RingBufferByteMemoryMap : RingBufferByteBase, IDisposable
    {
        private const long _numberOffset = 0;
        private const long _previousOffset = 8 + _numberOffset;
        private const long _sizeOffset = 8 + _previousOffset;
        private const long _msgOffset = 4 + _sizeOffset;
        private readonly MemoryMappedFile _file;
        private readonly bool _isCreated;
        private readonly long _size;
        private readonly MemoryMappedViewAccessor _view;

        private long _currentPosition;
        private long _nextFreeSlot;

        /// <summary>
        ///     Creates a new memory mapped ring buffer.
        /// </summary>
        /// <param name="size">The size of the ring buffer to create.</param>
        /// <param name="location">The location to store the ring buffer at.</param>
        /// <param name="name">The name of the ring buffer.</param>
        public RingBufferByteMemoryMap(long size, string location, string name)
        {
            _size = size;
            _isCreated = !System.IO.File.Exists(location);
            _file = MemoryMappedFile.CreateFromFile(location, FileMode.OpenOrCreate, name, size,
                MemoryMappedFileAccess.ReadWrite);
            _view = _file.CreateViewAccessor(0, size);
            if (_isCreated)
            {
                // If created write -1 to the front of the ring buffer so it will find position 0 as the end.
                _view.Write(0, -1L);
            }
            SeekToEnd();
        }

        public long LastNumber
        {
            get { return _view.ReadInt64(_currentPosition); }
        }

        public void Dispose()
        {
            try
            {
                _view.Dispose();
            }
            finally
            {
                _file.Dispose();
            }
        }

        public override void Insert(byte[] msg, long number)
        {
            var size = msg.Length;
            var totalLength = size + _msgOffset;
            // Make sure to include the write ahead.
            if (totalLength + 8 > _size - _nextFreeSlot)
            {
                _nextFreeSlot = 0;
            }
            _view.Write(_nextFreeSlot + _numberOffset, number);
            _view.Write(_nextFreeSlot + _previousOffset, _currentPosition);
            _view.Write(_nextFreeSlot + _sizeOffset, size);
            _view.WriteArray(_nextFreeSlot + _msgOffset, msg, 0, size);
            _view.Write(_nextFreeSlot + totalLength, -1L);

            Interlocked.Exchange(ref _currentPosition, _nextFreeSlot);
            Interlocked.Add(ref _nextFreeSlot, totalLength);

            Fire(msg, number);
        }

        /// <summary>
        ///     Used to get a message from the current position.
        /// </summary>
        /// <param name="msgId">The id of the message to get.</param>
        /// <returns></returns>
        public override byte[] Get(long msgId)
        {
            var pos = _currentPosition;
            var wrapped = false;
            while (true)
            {
                var number = _view.ReadInt64(pos + _numberOffset);
                if (number == msgId)
                {
                    var size = _view.ReadInt32(pos + _sizeOffset);
                    var buffer = new byte[size];
                    _view.ReadArray(pos + _msgOffset, buffer, 0, size);
                    return buffer;
                }
                pos = _view.ReadInt64(pos + _previousOffset);
                if (pos == 0)
                {
                    if (wrapped)
                    {
                        return null;
                    }
                    wrapped = true;
                }
            }
        }

        /// <summary>
        ///     Finds the end of the ring buffer if opening from the file system.
        /// </summary>
        private void SeekToEnd()
        {
            var last = 0L;
            var pos = 0L;
            while (true)
            {
                var number = _view.ReadInt64(pos + _numberOffset);
                if (number == -1)
                {
                    Interlocked.Exchange(ref _currentPosition, last);
                    return;
                }
                var size = _view.ReadInt32(pos + _sizeOffset);
                last = pos;
                pos = pos + size + _msgOffset;
            }
        }
    }
}