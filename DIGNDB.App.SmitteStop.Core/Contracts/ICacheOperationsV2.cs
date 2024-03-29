﻿using DIGNDB.App.SmitteStop.Domain;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.Core.Contracts
{
    public interface ICacheOperationsV2
    {
        Task<byte[]> GetCacheValue(ZipFileInfo zipFileInfo, string zipFilesFolder, bool forceRefresh = false);
    }
}
