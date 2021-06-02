using DIGNDB.App.SmitteStop.DAL.Context;
using DIGNDB.App.SmitteStop.Domain.Db;

namespace DIGNDB.App.SmitteStop.DAL.Repositories
{
    public class LoginInformationRepository : GenericRepository<LoginInformation>, ILoginInformationRepository
    {
        public LoginInformationRepository(DigNDB_SmittestopContext context) : base(context) { }
        public bool CreateEntry(LoginInformation entry)
        {
            bool added = true;
            var allRecords = Get(filter: x => x.Timestamp.Date == entry.Timestamp.Date);
            foreach (var record in allRecords)
            {
                Delete(record);
                added = false;
            }
            Insert(entry);
            Save();
            return added;
        }
    }
}
