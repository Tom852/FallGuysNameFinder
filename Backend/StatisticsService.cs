using Common;
using System.Linq;

namespace Backend
{
    public class StatisticsService
    {
        public void Account(History history)
        {
            var stats = DataStorageStuff.GetStats();
            stats.Account(history.GetWithoutSameElementsInRow());
            // todo: temp to see if list is good
            stats.AddAllPoosiibleNamesToSeeIFSomeAreEmpty();
            stats.Sort();
            DataStorageStuff.SaveStats(stats);
        }
    }
}