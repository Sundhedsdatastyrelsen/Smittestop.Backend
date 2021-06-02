using DIGNDB.App.SmitteStop.Domain.Dto;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.Core.Contracts
{
    public interface IAppleService
    {
        AppleQueryBitsDto BuildQueryBitsDto(string deviceToken);
        AppleUpdateBitsDto BuildUpdateBitsDto(string deviceToken);
        Task<AppleResponseDto> ExecuteQueryBitsRequest(string deviceToken);
        Task<AppleResponseDto> ExecuteUpdateBitsRequest(string deviceToken);
    }
}
