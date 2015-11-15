using System;

namespace Mrh.DataStructures.Buffer
{
    public class RingBuffer<T>
    {

        private readonly long _size;

        private readonly long _modNumber;

        private readonly T[] _buffer;

        public RingBuffer(int size)
        {
            if ((size & (size - 1)) != 0)
            {
                throw new Exception("Size must be a power of 2");
            }
            _size = size;
            _modNumber = size - 1;
            _buffer = new T[size];
        }

        public T Get(long position)
        {
            return _buffer[position & _modNumber];
        }

        public void Set(long position, T valaue)
        {
            _buffer[position & _modNumber] = valaue;
        }

        public long Size
        {
            get { return _size; }
        }
    }
}