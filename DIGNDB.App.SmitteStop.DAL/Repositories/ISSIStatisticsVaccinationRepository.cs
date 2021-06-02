using DIGNDB.App.SmitteStop.Domain.Db;
using System;
using System.Threading.Tasks;


namespace DIGNDB.App.SmitteStop.DAL.Repositories
{
    public interface ISSIStatisticsVaccinationRepository
    {
        public Task<SSIStatisticsVaccination> GetEntryByDateAsync(DateTime date);
        SSIStatisticsVaccination GetEntryByDate(DateTime date);
        void CreateEntry(SSIStatisticsVaccination entry);
        void RemoveEntriesOlderThan(DateTime date);
        void Delete(SSIStatisticsVaccination existingSsiStatistics);
        Task<SSIStatisticsVaccination> GetNewestEntryAsync();
    }
}
