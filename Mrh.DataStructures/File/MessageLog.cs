using System;
using System.IO;

namespace Mrh.DataStructures.File
{
    /// <summary>
    ///     Used to long a serious of messages.  All code is expected to run in a single thread and writes are consider to be way more common than reads.
    /// 
    ///     It is meant only as a historical log and should be backed by something link a ring buffer if going to constantly read events of off it.
    /// 
    /// Format is:
    /// |4 (bytes) message length | 8 bytes message # | msg bytes of length | 4 bytes message total length |
    /// </summary>
    public class MessageLog : IDisposable
    {
        private readonly string _location;
        private readonly FileStream _stream;
        private readonly byte[] _intBuffer = new byte[4];
        private readonly byte[] _longBuffer = new byte[8];


        /// <summary>
        /// </summary>
        /// <param name="location">The location to store the message log.</param>
        public MessageLog(string location)
        {
            _location = location;
            _stream = new FileStream(location, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _stream.Seek(0, SeekOrigin.End); // Always go to the end.
        }

        /// <summary>
        ///     Used to write out a message to the long.
        /// </summary>
        /// <param name="msg">The message to write out.</param>
        /// <param name="number">The number for the message.</param>
        /// <returns>A task since this message is asynchronous.</returns>
        public void Write(byte[] msg, long number)
        {
            var size = msg.Length;
            var totalLength = (4 + 8 + size + 4);
            _stream.Write(BitConverter.GetBytes(size), 0, 4);
            _stream.Write(BitConverter.GetBytes(number), 0, 8);
            _stream.Write(msg, 0, size);
            _stream.Write(BitConverter.GetBytes(totalLength), 0, 4);
        }

        /// <summary>
        ///     Used to get a message with the specified number.
        /// </summary>
        /// <param name="number">The number of the message to get.</param>
        /// <returns>The byte array of the number.  Returns null if a message with the specified number isn't found.</returns>
        public byte[] GetMessageFromBack(long number)
        {
            while (true)
            {
                // At the beginning so die.
                if (_stream.Position == 0)
                {
                    return null;
                }
                _stream.Seek(-4, SeekOrigin.Current);
                _stream.Read(_intBuffer, 0, 4);
                var size = BitConverter.ToInt32(_intBuffer, 0);
                _stream.Seek(-1 * size, SeekOrigin.Current);
                _stream.Read(_intBuffer, 0, 4);
                _stream.Read(_longBuffer, 0, 8);
                var currentNumber = BitConverter.ToInt64(_longBuffer, 0);
                // Can't find a number that's larger.
                if (currentNumber < number)
                {
                    return null;
                }
                if (number == currentNumber)
                {
                    var msgSize = BitConverter.ToInt32(_intBuffer, 0);
                    var msg = new byte[msgSize];
                    _stream.Read(msg, 0, msgSize);
                    _stream.Seek(-1*(msgSize + 12), SeekOrigin.Current);
                    return msg;
                }
                _stream.Seek(-12, SeekOrigin.Current); // Position at start.
            }
        }

        /// <summary>
        ///     Used to get a message from the front.
        /// </summary>
        /// <param name="number">The number of the message to get.</param>
        /// <returns>the bytes for the message or null.</returns>
        public byte[] GetMessageFromFront(long number)
        {
            while (true)
            {
                if (_stream.Position == _stream.Length)
                {
                    return null;
                }
                _stream.Read(_intBuffer, 0, 4);
                _stream.Read(_longBuffer, 0, 8);
                var payloadSize = BitConverter.ToInt32(_intBuffer, 0);
                var currentNumber = BitConverter.ToInt64(_longBuffer, 0);
                if (currentNumber > number)
                {
                    return null;
                }
                if (number == currentNumber)
                {
                    var msg = new byte[payloadSize];
                    _stream.Read(msg, 0, payloadSize);
                    _stream.Seek(4, SeekOrigin.Current);
                    return msg;
                }
                _stream.Seek(payloadSize + 4, SeekOrigin.Current);
            }
        }

        /// <summary>
        ///     Used to get the next message.
        /// </summary>
        /// <returns>A tuple where the first number is the message number and the second is the payload.</returns>
        public Tuple<long, byte[]> Next()
        {
            _stream.Read(_intBuffer, 0, 4);
            _stream.Read(_longBuffer, 0, 8);
            var payloadSize = BitConverter.ToInt32(_intBuffer, 0);
            var currentNumber = BitConverter.ToInt64(_longBuffer, 0);
            
            var msg = new byte[payloadSize];
            _stream.Read(msg, 0, payloadSize);
            _stream.Seek(4, SeekOrigin.Current);
            return new Tuple<long, byte[]>(currentNumber, msg);
        }

        /// <summary>
        ///     Used to get the number at the current position.
        /// </summary>
        public long CurrentMessageNumber
        {
            get
            {
                if (_stream.Position == 0)
                {
                    return 0;
                }
                // Save the current position.
                var currentPos = _stream.Position;

                _stream.Seek(-4, SeekOrigin.Current);
                _stream.Read(_intBuffer, 0, 4);
                var size = BitConverter.ToInt32(_intBuffer, 0);
                _stream.Seek(-1 * (size + 4), SeekOrigin.Current);
                _stream.Read(_intBuffer, 0, 4);
                _stream.Read(_longBuffer, 0, 8);
                _stream.Position = currentPos;
                return BitConverter.ToInt64(_longBuffer, 0);
            }
        }

        /// <summary>
        ///     Moves the buffer to back to the end after reading a message.  Calls this after getting the messages you want.
        /// </summary>
        public void GoToEnd()
        {
            _stream.Seek(0, SeekOrigin.End);
        }

        public void GoToFront()
        {
            _stream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        ///     Used to close the current message long.
        /// </summary>
        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}