using System;
using System.Runtime.Serialization;

namespace Ultima
{
    public class WaveFormatException : Exception
    {
        public WaveFormatException()
        {
        }

        public WaveFormatException(string message) : base(message)
        {
        }

        public WaveFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WaveFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}