using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Core.Persistence.Repositories;

public interface IRepository<TEntity, TEntityId> : IQuery<TEntity> where TEntity : Entity<TEntityId>
{
    // Bu metod, belirli bir koşulu sağlayan (predicate) bir TEntity nesnesini asenkron olarak veritabanından getirir.
    // predicate: Sorguda kullanılacak filtre koşulunu temsil eden bir ifade (ör. x => x.Id == 5).
    // include: İlişkili varlıkların sorguya dahil edilmesini sağlayan, isteğe bağlı bir fonksiyon (ör. navigation property'leri eager load etmek için).
    // withDeleted: Silinmiş kayıtların da sorguya dahil edilip edilmeyeceğini belirten bir bayrak (varsayılan: false).
    // enableTracking: Entity Framework'ün nesneleri izleyip izlemeyeceğini belirten bir bayrak (performans ve değişiklik takibi için).
    // cancellationToken: Asenkron işlemin iptal edilmesini sağlayan belirteç (varsayılan: CancellationToken.None).
    TEntity? Get(
        Expression<Func<TEntity, bool>> predicate, // Sorgu filtresi
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, // İlişkili varlıkları dahil etme fonksiyonu
        bool withDeleted = false, // Silinmiş kayıtları dahil et
        bool enableTracking = true, // Takip (tracking) açık mı
        CancellationToken cancellationToken = default // İptal belirteci
    );

    // GetList metodu, filtreleme, sıralama, ilişkili verileri dahil etme ve sayfalama işlemlerini bir arada yaparak
    // TEntity tipinde bir varlık listesini sayfalı şekilde döndürür.
    // predicate: Sorguda kullanılacak filtre koşulu (isteğe bağlı).
    // orderBy: Sonuçların sıralanmasını sağlayan fonksiyon (isteğe bağlı).
    // include: İlişkili varlıkların sorguya dahil edilmesini sağlayan fonksiyon (isteğe bağlı).
    // index: Getirilecek sayfanın indeksi (varsayılan: 0).
    // size: Sayfa başına düşen kayıt sayısı (varsayılan: 10).
    // withDeleted: Silinmiş kayıtların da dahil edilip edilmeyeceğini belirten bayrak.
    // enableTracking: Entity Framework'ün nesneleri izleyip izlemeyeceğini belirten bayrak.
    // cancellationToken: Asenkron işlemin iptal edilmesini sağlayan belirteç.
    Paginate<TEntity> GetList(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    // GetListByDynamic metodu, dinamik olarak oluşturulan sorgu kriterleriyle birlikte
    // filtreleme, ilişkili verileri dahil etme ve sayfalama işlemlerini yaparak
    // TEntity tipinde bir varlık listesini sayfalı şekilde döndürür.
    // dynamic: Dinamik sorgu kriterlerini içeren nesne.
    // predicate: Ekstra filtre koşulu (isteğe bağlı).
    // include: İlişkili varlıkların sorguya dahil edilmesini sağlayan fonksiyon (isteğe bağlı).
    // index: Getirilecek sayfanın indeksi (varsayılan: 0).
    // size: Sayfa başına düşen kayıt sayısı (varsayılan: 10).
    // withDeleted: Silinmiş kayıtların da dahil edilip edilmeyeceğini belirten bayrak.
    // enableTracking: Entity Framework'ün nesneleri izleyip izlemeyeceğini belirten bayrak.
    // cancellationToken: Asenkron işlemin iptal edilmesini sağlayan belirteç.
    Paginate<TEntity> GetListByDynamic(
        DynamicQuery dynamic,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );


    // Any metodu, belirli bir koşulu sağlayan en az bir TEntity nesnesi olup olmadığını kontrol eder.
    // predicate: Sorguda kullanılacak filtre koşulu (isteğe bağlı).
    // withDeleted: Silinmiş kayıtların da dahil edilip edilmeyeceğini belirten bayrak.
    // enableTracking: Entity Framework'ün nesneleri izleyip izlemeyeceğini belirten bayrak.
    // cancellationToken: Asenkron işlemin iptal edilmesini sağlayan belirteç.
    bool Any(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    // Add metodu, yeni bir TEntity nesnesini veritabanına ekler ve eklenen nesneyi döndürür.
    TEntity Add(TEntity entity);
    // AddRange metodu, birden fazla TEntity nesnesini topluca veritabanına ekler ve eklenen nesneleri döndürür.
    ICollection<TEntity> AddRange(ICollection<TEntity> entities);
    // Update metodu, mevcut bir TEntity nesnesini günceller ve güncellenen nesneyi döndürür.
    TEntity Update(TEntity entity);
    // UpdateRange metodu, birden fazla TEntity nesnesini topluca günceller ve güncellenen nesneleri döndürür.
    ICollection<TEntity> UpdateRange(ICollection<TEntity> entities);
    // Delete metodu, bir TEntity nesnesini siler. hardDelete true ise kalıcı olarak siler, false ise yumuşak silme (soft delete) uygular.
    TEntity Delete(TEntity entity, bool hardDelete = false);
    // DeleteRange metodu, birden fazla TEntity nesnesini topluca siler. hardDelete true ise kalıcı olarak siler, false ise yumuşak silme (soft delete) uygular.
    ICollection<TEntity> DeleteRange(ICollection<TEntity> entities, bool hardDelete = false);
}

