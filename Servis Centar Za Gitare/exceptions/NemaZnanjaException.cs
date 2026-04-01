using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Servis_Centar_Za_Gitare.exceptions
{
    public class NemaZnanjaException : Exception
    {
        public NemaZnanjaException()
        {
        }

        public NemaZnanjaException(string? message) : base(message)
        {
        }

        public NemaZnanjaException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NemaZnanjaException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
