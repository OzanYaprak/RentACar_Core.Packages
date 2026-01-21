using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Core.Persistence.Repositories;

public interface IAsyncRepository<TEntity, TEntityId> : IQuery<TEntity> where TEntity : Entity<TEntityId>
{
    // Bu metod, belirli bir koşulu sağlayan (predicate) bir TEntity nesnesini asenkron olarak veritabanından getirir.
    // predicate: Sorguda kullanılacak filtre koşulunu temsil eden bir ifade (ör. x => x.Id == 5).
    // include: İlişkili varlıkların sorguya dahil edilmesini sağlayan, isteğe bağlı bir fonksiyon (ör. navigation property'leri eager load etmek için).
    // withDeleted: Silinmiş kayıtların da sorguya dahil edilip edilmeyeceğini belirten bir bayrak (varsayılan: false).
    // enableTracking: Entity Framework'ün nesneleri izleyip izlemeyeceğini belirten bir bayrak (performans ve değişiklik takibi için).
    // cancellationToken: Asenkron işlemin iptal edilmesini sağlayan belirteç (varsayılan: CancellationToken.None).
    Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate, // Sorgu filtresi
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, // İlişkili varlıkları dahil etme fonksiyonu
        bool withDeleted = false, // Silinmiş kayıtları dahil et
        bool enableTracking = true, // Takip (tracking) açık mı
        CancellationToken cancellationToken = default // İptal belirteci
    );

    // GetListAsync metodu, filtreleme, sıralama, ilişkili verileri dahil etme ve sayfalama işlemlerini asenkron olarak yapar.
    // TEntity tipinde varlıkların sayfalı listesini döndürür.
    // predicate: Sorguda kullanılacak filtre koşulu (isteğe bağlı).
    // orderBy: Sonuçların sıralanmasını sağlayan fonksiyon (isteğe bağlı).
    // include: İlişkili varlıkların sorguya dahil edilmesini sağlayan fonksiyon (isteğe bağlı).
    // index: Getirilecek sayfanın indeksi (varsayılan: 0).
    // size: Sayfa başına düşen kayıt sayısı (varsayılan: 10).
    // withDeleted: Silinmiş kayıtların da dahil edilip edilmeyeceğini belirten bayrak.
    // enableTracking: Entity Framework'ün nesneleri izleyip izlemeyeceğini belirten bayrak.
    // cancellationToken: Asenkron işlemin iptal edilmesini sağlayan belirteç.
    Task<Paginate<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    // GetListByDynamicAsync metodu, dinamik olarak oluşturulan sorgu kriterleriyle birlikte
    // filtreleme, ilişkili verileri dahil etme ve sayfalama işlemlerini asenkron olarak yapar.
    // TEntity tipinde varlıkların sayfalı listesini döndürür.
    // dynamic: Dinamik sorgu kriterlerini içeren nesne.
    // predicate: Ekstra filtre koşulu (isteğe bağlı).
    // include: İlişkili varlıkların sorguya dahil edilmesini sağlayan fonksiyon (isteğe bağlı).
    // index: Getirilecek sayfanın indeksi (varsayılan: 0).
    // size: Sayfa başına düşen kayıt sayısı (varsayılan: 10).
    // withDeleted: Silinmiş kayıtların da dahil edilip edilmeyeceğini belirten bayrak.
    // enableTracking: Entity Framework'ün nesneleri izleyip izlemeyeceğini belirten bayrak.
    // cancellationToken: Asenkron işlemin iptal edilmesini sağlayan belirteç.
    Task<Paginate<TEntity>> GetListByDynamicAsync(
        DynamicQuery dynamic,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );


    // AnyAsync metodu, belirli bir koşulu sağlayan en az bir TEntity nesnesi olup olmadığını asenkron olarak kontrol eder.
    // predicate: Sorguda kullanılacak filtre koşulu (isteğe bağlı).
    // withDeleted: Silinmiş kayıtların da dahil edilip edilmeyeceğini belirten bayrak.
    // enableTracking: Entity Framework'ün nesneleri izleyip izlemeyeceğini belirten bayrak.
    // cancellationToken: Asenkron işlemin iptal edilmesini sağlayan belirteç.
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    // AddAsync metodu, yeni bir TEntity nesnesini asenkron olarak veritabanına ekler ve eklenen nesneyi döndürür.
    Task<TEntity> AddAsync(TEntity entity);
    // AddRangeAsync metodu, birden fazla TEntity nesnesini topluca asenkron olarak veritabanına ekler ve eklenen nesneleri döndürür.
    Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities);
    // UpdateAsync metodu, mevcut bir TEntity nesnesini asenkron olarak günceller ve güncellenen nesneyi döndürür.
    Task<TEntity> UpdateAsync(TEntity entity);
    // UpdateRangeAsync metodu, birden fazla TEntity nesnesini topluca asenkron olarak günceller ve güncellenen nesneleri döndürür.
    Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities);
    // DeleteAsync metodu, bir TEntity nesnesini asenkron olarak siler. hardDelete true ise kalıcı olarak siler, false ise yumuşak silme (soft delete) uygular.
    Task<TEntity> DeleteAsync(TEntity entity, bool hardDelete = false);
    // DeleteRangeAsync metodu, birden fazla TEntity nesnesini topluca asenkron olarak siler. hardDelete true ise kalıcı olarak siler, false ise yumuşak silme (soft delete) uygular.
    Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool hardDelete = false);
}
