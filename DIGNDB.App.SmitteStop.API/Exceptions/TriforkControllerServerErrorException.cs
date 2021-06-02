using System;

namespace DIGNDB.APP.SmitteStop.API.Exceptions
{
    public class TriforkControllerServerErrorException : Exception
    {
        public TriforkControllerServerErrorException(string message) : base(message) { }
        public TriforkControllerServerErrorException(string message, Exception e) : base(message, e) { }
    }
}