using DIGNDB.App.SmitteStop.Core.Contracts;
using DIGNDB.App.SmitteStop.Domain.Dto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.API.Services
{
    public class CacheOperations : ICacheOperations
    {
        private const int DefaultCacheMonitorTimeout = 100;
        private readonly ILogger<CacheOperations> _logger;
        private readonly IMemoryCache _memoryCache;
        private static readonly Object _cacheLock = new object();
        private readonly IConfiguration _configuration;
        private readonly IPackageBuilderService _cachePackageBuilder;
        private readonly TimeSpan _previousDayFileCaching;
        private readonly TimeSpan _currentDayFileCaching;

        public CacheOperations(IMemoryCache memoryCache, IConfiguration configuration, IPackageBuilderService cachePackageBuilder, ILogger<CacheOperations> logger)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _configuration = configuration;
            _cachePackageBuilder = cachePackageBuilder;
            _previousDayFileCaching = TimeSpan.Parse(_configuration["AppSettings:PreviousDayFileCaching"]);
            _currentDayFileCaching = TimeSpan.Parse(_configuration["AppSettings:CurrentDayFileCaching"]);
        }

        public async Task<CacheResult> GetCacheValue(DateTime key, bool forceRefresh = false)
        {
            return await GetValueOrDefault(key, forceRefresh);
        }

        private async Task<CacheResult> GetValueOrDefault(DateTime key, bool forceRefresh)
        {
            if (_memoryCache.TryGetValue(key, out CacheResult result))
            {
                return await Task.FromResult(result);
            }

            if (!int.TryParse(_configuration["AppSettings:CacheMonitorTimeout"], out int cacheMonitorTimeout))
            {
                cacheMonitorTimeout = DefaultCacheMonitorTimeout;
            }

            var timeout = TimeSpan.FromMilliseconds(cacheMonitorTimeout);
            var lockTaken = false;

            try
            {
                Monitor.TryEnter(_cacheLock, timeout, ref lockTaken);
                if (lockTaken)
                {
                    if (forceRefresh || !_memoryCache.TryGetValue(key, out result))
                    {
                        result = _cachePackageBuilder.BuildPackage(key);

                        //If FUNC returns a null value, cache it for the defined amount of time
                        if (result == null)
                        {
                            //writes the Null placeholder object to the cache.
                            return default;
                        }

                        var previousDayExpiration = Math.Min(DateTime.UtcNow.Ticks, key.Ticks);
                        var absExpiration = new DateTimeOffset(new DateTime(previousDayExpiration, DateTimeKind.Utc) + _previousDayFileCaching);
                        if (key.Date == DateTime.UtcNow.Date)
                        {
                            DateTime midnight = DateTime.UtcNow.AddDays(1).Date;
                            var currentDayExpiration = Math.Min(midnight.Ticks, (DateTime.UtcNow + _currentDayFileCaching).Ticks);
                            absExpiration = new DateTimeOffset(new DateTime(currentDayExpiration, DateTimeKind.Utc));
                        }

                        _memoryCache.Set(key, result, absExpiration);
                        _logger.LogInformation($"Created package in memory cache with key: {key} Expiration date: {absExpiration}");
                        return result;
                    }

                    return result;
                }
                else
                {
                    throw new SynchronizationLockException();
                }
            }
            finally
            {
                // Ensure that the lock is released.
                if (lockTaken)
                {
                    Monitor.Exit(_cacheLock);
                }
            }
            return result;
        }
    }
}
