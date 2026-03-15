using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Core.Application.Pipelines.Caching;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ICacheableRequest
{
    private readonly CacheSettings _cacheSettings;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(IDistributedCache distributedCache, IConfiguration configuration, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheSettings = configuration.GetSection("CacheSettings").Get<CacheSettings>() ?? throw new InvalidOperationException("CacheSettings section is missing in the configuration.");
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.BypassCache) { return await next(); }

        TResponse response;

        byte[]? cachedResponse = await _distributedCache.GetAsync(request.CacheKey, cancellationToken);

        if (cachedResponse != null)
        {
            response = JsonSerializer.Deserialize<TResponse>(Encoding.Default.GetString(cachedResponse))!;

            _logger.LogInformation($"Fetched from Cache -> {request.CacheKey}");
        }
        else
        {
            response = await GetResponseAndAddToCache(request, next, cancellationToken);
        }

        return response;
    }

    private async Task<TResponse> GetResponseAndAddToCache(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response = await next();

        int cacheDuration = request.SlidingExpirationTime != 0 ? request.SlidingExpirationTime : _cacheSettings.SlidingExpirationTime;

        DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(cacheDuration) };

        byte[] serializedData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));

        await _distributedCache.SetAsync(request.CacheKey, serializedData, cacheOptions, cancellationToken);

        _logger.LogInformation($"Added to Cache -> {request.CacheKey}");

        return response;
    }
}