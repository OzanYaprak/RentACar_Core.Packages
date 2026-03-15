namespace Core.Application.Pipelines.Caching;

public interface ICacheableRequest
{
    /// <summary>
    /// Önbellekte depolanan verinin benzersiz tanımlayıcısı. Önbelleğe alınacak öğeleri depolamak ve almak için kullanılır.
    /// </summary>
    public string CacheKey { get; }

    /// <summary>
    /// Belirli bir istek için önbelleğin atlanması gerekip gerekmediğini kontrol eder. 
    /// True olarak ayarlandığında, önbellek durumuna bakılmaksızın güncel veriler getirilir.
    /// </summary>
    public bool BypassCache { get; }

    /// <summary>
    /// Kayan pencere stratejisini kullanarak önbellekte kalma süresini tanımlar.
    /// Önbelleğe alınan öğe belirtilen süre kadar geçerli kalır, ancak her erişimde sayaç sıfırlanır.
    /// Sık kullanılan veriler önbellekte kalırken, kullanılmayan veriler temizlenir.
    /// </summary>
    public int SlidingExpirationTime { get; }
}
