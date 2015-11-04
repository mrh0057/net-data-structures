using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mrh.DataStructures.Buffer
{
    /// <summary>
    /// Implementation of a list buffer that is inspired on how aeron works.  The code is thread
    /// safe for multiple readers and writers.
    /// </summary>
    public class ListBuffer
    {

        private volatile readonly byte[] _buffer;
        private long _currentPosition = 0;

        public ListBuffer(long size)
        {
            _buffer = new byte[size];
        }

        public void Rest()
        {
            _currentPosition = 0;
        }
    }
}
