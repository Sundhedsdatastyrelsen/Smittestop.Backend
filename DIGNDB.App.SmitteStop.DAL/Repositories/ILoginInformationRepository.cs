using DIGNDB.App.SmitteStop.Domain.Db;

namespace DIGNDB.App.SmitteStop.DAL.Repositories
{
    public interface ILoginInformationRepository
    {
        public bool CreateEntry(LoginInformation entry);
    }
}
