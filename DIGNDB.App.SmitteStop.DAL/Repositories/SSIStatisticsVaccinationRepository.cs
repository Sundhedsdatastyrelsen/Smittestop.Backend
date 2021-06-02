using DIGNDB.App.SmitteStop.DAL.Context;
using DIGNDB.App.SmitteStop.Domain.Db;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.DAL.Repositories
{
    public class SSIStatisticsVaccinationRepository : GenericRepository<SSIStatisticsVaccination>, ISSIStatisticsVaccinationRepository
    {
        public SSIStatisticsVaccinationRepository(DigNDB_SmittestopContext context) : base(context)
        {
        }

        public SSIStatisticsVaccination GetEntryByDate(DateTime date)
        {
            return Get(filter: c => c.Date.Date.CompareTo(date.Date) == 0).SingleOrDefault();
        }

        public async Task<SSIStatisticsVaccination> GetEntryByDateAsync(DateTime date)
        {
            return (await GetAsync(filter: c => c.Date.Date == date.Date)).SingleOrDefault();
        }

        public void CreateEntry(SSIStatisticsVaccination entry)
        {
            Insert(entry);
            Save();
        }

        public void RemoveEntriesOlderThan(DateTime date)
        {
            var allRecords = Get(filter: x=> x.Date.Date<= date.Date);
            foreach (var record in allRecords)
            {
                Delete(record);
            }
            Save();
        }

        public async Task<SSIStatisticsVaccination> GetNewestEntryAsync()
        {
            return (await GetAsync(orderBy: x => x.OrderByDescending(x => x.Date))).First();
        }
    }
}
