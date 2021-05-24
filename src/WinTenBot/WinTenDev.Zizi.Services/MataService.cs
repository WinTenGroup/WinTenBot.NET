using System.Threading.Tasks;
using EasyCaching.Core;
using Serilog;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Utils;

namespace WinTenDev.Zizi.Services
{
    /// <summary>
    /// Track user property changes
    /// </summary>
    public class MataService
    {
        private string baseKey = "zizi-mata";
        private readonly IEasyCachingProvider _cachingProvider;

        /// <summary>
        /// Mata constructor
        /// </summary>
        /// <param name="cachingProvider"></param>
        public MataService
        (
            IEasyCachingProvider cachingProvider
        )
        {
            _cachingProvider = cachingProvider;
        }

        private string GetCacheKey(int fromId)
        {
            return $"{baseKey}_{fromId}";
        }


        /// <summary>
        /// Get Mata by userId
        /// </summary>
        /// <param name="fromId"></param>
        /// <returns></returns>
        public async Task<CacheValue<HitActivity>> GetMataCore(int fromId)
        {
            var key = GetCacheKey(fromId);
            var hitActivity = await _cachingProvider.GetAsync<HitActivity>(key);
            return hitActivity;
        }

        /// <summary>
        /// Save Mata by userId
        /// </summary>
        /// <param name="fromId"></param>
        /// <param name="hitActivity"></param>
        public async Task SaveMataAsync(int fromId, HitActivity hitActivity)
        {
            var key = GetCacheKey(fromId);
            var timeSpan = TimeUtil.YearSpan(30);
            Log.Debug("Saving Mata into cache with Key: '{Key}'. Span: {TimeSpan}", key, timeSpan);
            await _cachingProvider.SetAsync(key, hitActivity, timeSpan);
        }
    }
}