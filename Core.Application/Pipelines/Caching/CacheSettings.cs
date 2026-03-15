namespace Core.Application.Pipelines.Caching;

/// <summary> 
/// Bu sınıf, önbellek ayarlarını merkezi bir şekilde yönetmek için kullanılabilir. 
/// Örneğin, uygulamanın farklı bölümlerinde farklı önbellek stratejileri uygulanabilir ve bu ayarlar bu sınıfta tanımlanarak kolayca yönetilebilir. 
/// Ayrıca, bu sınıfın özellikleri, uygulamanın yapılandırma dosyalarından veya ortam değişkenlerinden okunarak dinamik olarak ayarlanabilir hale getirilebilir.
/// </summary>
public class CacheSettings
{
    /// <summary>
    /// Verinin önbellekte kalma süresini tanımlar. Bu süre dolduğunda, önbellekteki veri geçersiz hale gelir ve temizlenir.
    /// </summary>
    public int SlidingExpirationTime { get; set; }
}

