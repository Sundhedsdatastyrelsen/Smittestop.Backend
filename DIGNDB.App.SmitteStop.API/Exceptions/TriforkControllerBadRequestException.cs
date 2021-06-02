using System;

namespace DIGNDB.APP.SmitteStop.Jobs.Exceptions
{
    public class TriforkControllerBadRequestException : Exception
    {
        public TriforkControllerBadRequestException(string message) : base(message) { }
    }
}
