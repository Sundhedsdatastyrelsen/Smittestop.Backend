using DIGNDB.App.SmitteStop.Domain.Db;
using DIGNDB.App.SmitteStop.Domain.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.Core.Contracts
{
    public interface IAddTemporaryExposureKeyService
    {
        Task CreateKeysInDatabase(TemporaryExposureKeyBatchDto parameters);

        Task<IList<TemporaryExposureKey>> GetFilteredKeysEntitiesFromDTO(TemporaryExposureKeyBatchDto parameters);

        Task<List<TemporaryExposureKey>> FilterDuplicateKeysAsync(IList<TemporaryExposureKey> incomingKeys);
    }
}
