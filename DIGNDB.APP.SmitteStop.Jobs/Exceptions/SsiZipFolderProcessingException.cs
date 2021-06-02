using System;

namespace DIGNDB.APP.SmitteStop.Jobs.Exceptions
{
    public class SsiZipFolderProcessingException : Exception
    {
        public SsiZipFolderProcessingException(string message) : base(message) { }
    }
}
