using System;

namespace DIGNDB.APP.SmitteStop.Jobs.Exceptions
{
    public class SSIZipFileParseException : Exception
    {
        public SSIZipFileParseException(string message, Exception e) : base(message, e) { }
        public SSIZipFileParseException(string message) : base(message) { }
    }
}
