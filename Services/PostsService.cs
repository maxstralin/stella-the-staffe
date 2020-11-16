using StellaTheStaffe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using System.Threading;
using System.Collections.Concurrent;

namespace StellaTheStaffe.Services
{
    public class PostsService
    {
        private readonly PostsContext postsContext;
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private SortedDictionary<DateTime, IEnumerable<InstagramData>?> DateGroups = new SortedDictionary<DateTime, IEnumerable<InstagramData>?>();
        private IEnumerable<int>? Weeks { get; set; }

        public PostsService(PostsContext postsContext)
        {
            this.postsContext = postsContext;
            InstagramFetchService.NewData += async (_, newData) =>
            {
                await UpdateNewDataAsync(newData);
            };
        }



        private async Task UpdateNewDataAsync(IEnumerable<InstagramData> data)
        {
            await semaphore.WaitAsync();
            try
            {
                var grouped = data.GroupBy(a => a.Timestamp.Date, a => a, (a, b) => new { Date = a, Data = b });
                foreach (var group in grouped)
                {
                    if (!DateGroups.ContainsKey(group.Date) || DateGroups[group.Date] == null)
                    {
                        DateGroups[group.Date] = group.Data;
                    }
                    else
                    {
                        var entriesList = DateGroups[group.Date]!.ToList();
                        //Let's just double check IDs don't exist already
                        entriesList.AddRange(group.Data.Where(a => entriesList.Any(b => b.Id == a.Id)));
                        DateGroups[group.Date] = entriesList;
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        //private async Task<List<InstagramData>> GetDataAsync()
        //{
        //    return InstagramData ??= await postsContext.Data.AsQueryable().ToListAsync();
        //}

        /// <summary>
        /// Get entries for a specific date
        /// </summary>
        /// <returns>An unordered list of entries for that date</returns>
        public async Task<IEnumerable<InstagramData>> GetByDatesAsync(DateTime minDate, DateTime? maxDate = null)
        {
            await semaphore.WaitAsync();
            var results = Enumerable.Empty<InstagramData>();
            try
            {
                maxDate ??= minDate;
                maxDate = maxDate.Value.Date;
                var allDays = Enumerable.Range(0, 1 + maxDate.Value.Subtract(minDate.Date).Days).Select(offset => minDate.AddDays(offset).Date).ToList();

                foreach (var day in allDays)
                {
                    //Because I'm lazy and MongoDb driver doesn't support to use a.Timestamp.ToString("yyyy-MM-dd"), or a.Timestamp.Date, then we check if the timestamp is between start of the day and end of day
                    var startOfDay = day.Date;
                    var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
                    if (!DateGroups.ContainsKey(day.Date) || DateGroups[day.Date] == null)
                    {
                        var entries = await postsContext.Data.AsQueryable().Where(a => a.Timestamp >= startOfDay && a.Timestamp <= endOfDay).ToListAsync();
                        if (entries.Count > 0)
                        {
                            DateGroups[day.Date] = entries;
                            results = results.Concat(entries);
                        }
                    }
                    else results = results.Concat(DateGroups[day.Date]!);
                }
                return results;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<IEnumerable<DateTime>> GetDatesAsync()
        {
            await semaphore.WaitAsync();
            try
            {
                if (DateGroups.Count == 0)
                {
                    var timestamps = await postsContext.Data.AsQueryable().Select(a => a.Timestamp).ToListAsync();
                    var dates = timestamps.Select(a => a.Date).Distinct();
                    DateGroups = new SortedDictionary<DateTime, IEnumerable<InstagramData>?>(dates.ToDictionary(a => a, a => (IEnumerable<InstagramData>?)null));
                }

                return DateGroups.Keys;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<IEnumerable<int>> GetWeeksAsync()
        {
            if (Weeks != null) return Weeks;
            var dates = await GetDatesAsync();
            var latest = dates.OrderByDescending(a => a).First();
            var weeksOld = latest.GetWeeksFromBirth();

            var res = Enumerable.Empty<int>();

            for (int i = weeksOld; i >= 1; i--)
            {
                var (startDate, endDate) = DateHelpers.GetDatesInWeek(i);

                if (dates.Any(a => a >= startDate && a <= endDate)) res = res.Append(i);
            }

            return res;

        }

    }
}
