using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace DIGNDB.App.SmitteStop.Core.Contracts
{
    public interface IUploadFileValidationService
    {
        bool Verify(IFormFile file, out string message);
    }
}
