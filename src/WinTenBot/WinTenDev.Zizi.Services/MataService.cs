using System;
using System.Threading.Tasks;
using EasyCaching.Core;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Utils;

namespace WinTenDev.Zizi.Services
{
    public class MataService
    {
        private string baseKey = "zizi-mata";
        private readonly IEasyCachingProvider _cachingProvider;

        public MataService
        (
            IEasyCachingProvider cachingProvider
        )
        {
            _cachingProvider = cachingProvider;
        }

        public async Task<CacheValue<HitActivity>> GetMataCore(int fromId)
        {
            var key = baseKey + fromId;
            var hitActivity = await _cachingProvider.GetAsync<HitActivity>(key);
            return hitActivity;
        }

        public async Task SaveMataAsync(int fromId, HitActivity hitActivity)
        {
            var key = baseKey + fromId;
            await _cachingProvider.SetAsync(key, hitActivity, TimeUtil.YearSpan(30));
        }
    }
}