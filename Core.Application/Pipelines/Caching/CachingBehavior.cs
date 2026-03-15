using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;

namespace Core.Application.Pipelines.Caching;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ICacheableRequest
{
    private readonly CacheSettings _cacheSettings;
    private readonly IDistributedCache _distributedCache;
    public CachingBehavior(CacheSettings cacheSettings, IDistributedCache distributedCache)
    {
        _cacheSettings = cacheSettings;
        _distributedCache = distributedCache;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.BypassCache) { return await next(); }

        TResponse response;
        
        byte[]? cachedResponse = await _distributedCache.GetAsync(request.CacheKey, cancellationToken);
        
        if (cachedResponse != null)
        {
            response = JsonSerializer.Deserialize<TResponse>(Encoding.Default.GetString(cachedResponse))!;
        }
        else
        {
            response = await GetResponseAndAddToCache(request,next,cancellationToken);
        }

        return response;
    }

    private async Task<TResponse> GetResponseAndAddToCache(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response = await next();

        TimeSpan cacheDuration = request.SlidingExpirationTime ?? TimeSpan.FromDays(_cacheSettings.SlidingExpirationTime); 
        
        DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions { SlidingExpiration = cacheDuration };

        byte[] serializedData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
       
        await _distributedCache.SetAsync(request.CacheKey, serializedData, cacheOptions, cancellationToken);

        return response;
    }
}