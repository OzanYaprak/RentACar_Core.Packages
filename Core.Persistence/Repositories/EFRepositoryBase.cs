using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Core.Persistence.Repositories;

// Bu sınıf, Entity Framework Core ile çalışan genel (generic) bir repository (depo) temel sınıfıdır.
// TEntity: Veritabanındaki tabloyu temsil eden varlık (entity) tipi.
// TEntityId: Bu varlığın birincil anahtar (Id) tipidir (ör. int, Guid).
// TContext: Kullanılan DbContext tipi (veritabanı bağlantısı ve set'leri içerir).
// Bu sınıf hem asenkron repository arayüzünü (IAsyncRepository) hem de senkron repository arayüzünü (IRepository) uygular.
public class EFRepositoryBase<TEntity, TEntityId, TContext> : IAsyncRepository<TEntity, TEntityId>
    where TEntity : Entity<TEntityId>                 // TEntity mutlaka Id, CreatedDate vb. içeren Entity'den türemeli
    where TContext : DbContext                        // TContext mutlaka EF Core DbContext olmalı
{
    // Veritabanı işlemleri için kullanılan DbContext örneği.
    protected readonly TContext _context;

    // Kurucu metot. Dışarıdan bir DbContext alır ve sınıf içinde kullanmak üzere saklar.
    public EFRepositoryBase(TContext context)
    {
        _context = context;
    }

    // Yeni bir varlığı (entity) veritabanına asenkron olarak ekler.
    // Eklenen varlığın oluşturulma tarihini (CreatedDate) şu anki zamana ayarlar.
    public async Task<TEntity> AddAsync(TEntity entity)
    {
        entity.CreatedDate = DateTime.UtcNow;   // Oluşturulan varlığın oluşturulma zamanı atanıyor.
        await _context.AddAsync(entity);        // EF Core'a bu varlığın eklenmesi söyleniyor (henüz veritabanına yazılmadı).
        await _context.SaveChangesAsync();      // Yapılan değişiklikler veritabanına kalıcı olarak yazılıyor.
        return entity;                          // Eklenmiş ve güncel haliyle varlık geri döndürülüyor.
    }

    // Birden fazla varlığı aynı anda (toplu) asenkron olarak veritabanına ekler.
    // Her bir varlık için CreatedDate alanını ayarlar.
    public async Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities)
    {
        // Koleksiyondaki her bir varlığın oluşturulma tarihini ayarla
        foreach (var entity in entities)
        {
            entity.CreatedDate = DateTime.UtcNow;
        }

        await _context.AddRangeAsync(entities); // Tüm varlıklar EF Core'a eklenmesi için bildiriliyor.
        await _context.SaveChangesAsync();      // Değişiklikler veritabanına yazılıyor.
        return entities;                        // Eklenmiş varlıkların listesi geri döndürülüyor.
    }

    // Verilen koşula (predicate) göre veritabanında en az bir kayıt olup olmadığını asenkron olarak kontrol eder.
    // withDeleted: soft delete yapılmış kayıtları da dahil edip etmeyeceğini belirler.
    // enableTracking: EF Core'un değişiklik takibi yapıp yapmayacağını belirler (performansa etki eder).
    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,  // Filtre koşulu (örn: x => x.Id == 5)
        bool withDeleted = false,                           // Silinmiş (DeletedDate dolu) kayıtlar da dahil mi
        bool enableTracking = true,                         // Takip açık mı (değişiklikler izlenecek mi)
        CancellationToken cancellationToken = default)      // İşlemin iptal edilebilmesi için token
    {
        IQueryable<TEntity> query = Query();                // Temel sorguyu TEntity üzerinde başlatıyoruz.

        if (!enableTracking)
            query = query.AsNoTracking();                   // Takip kapalı ise, sorgu sadece okuma amaçlı yapılır.

        if (withDeleted)
            query = query.IgnoreQueryFilters();             // Global sorgu filtrelerini (örn. soft delete filtresi) yok say.

        if (predicate != null)
            query = query.Where(predicate);                 // Eğer filtre verilmişse, sorguya uygula.

        // Sorguyu çalıştır ve koşulu sağlayan en az bir kayıt var mı diye bak.
        return await query.AnyAsync(cancellationToken);
    }

    // Tek bir varlığı asenkron olarak siler.
    // hardDelete = true ise veritabanından tamamen siler.
    // hardDelete = false ise soft delete yapar (DeletedDate alanını doldurur).
    public async Task<TEntity> DeleteAsync(TEntity entity, bool hardDelete = false)
    {
        await SetEntityAsDeletedAsync(entity, hardDelete);  // Varlığa silinmiş muamelesi yap (hard veya soft).
        await _context.SaveChangesAsync();                  // Değişiklikleri veritabanına yaz.
        return entity;                                      // Silinmiş (veya işaretlenmiş) varlığı döndür.
    }

    // Birden fazla varlığı topluca asenkron olarak siler.
    // Tüm varlıklar için hard veya soft delete uygulanır.
    public async Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool hardDelete = false)
    {
        await SetEntityAsDeletedAsync(entities, hardDelete); // Koleksiyondaki her varlığa silme işlemi uygula.
        await _context.SaveChangesAsync();                   // Değişiklikleri veritabanına yaz.
        return entities;                                     // Silinmiş varlıklar geri döndürülür.
    }

    // Verilen koşulu sağlayan tek bir varlığı asenkron olarak getirir.
    // include parametresi ile ilişkili tabloları da sorguya dahil edebiliriz (navigation property).
    // withDeleted ve enableTracking yukarıdaki AnyAsync ile benzer mantıkta çalışır.
    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,                                  // Aranan koşul
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, // İlişkili tabloları eklemek için
        bool withDeleted = false,                                                   // Silinmiş kayıtlar dahil mi
        bool enableTracking = true,                                                 // Takip açık mı
        CancellationToken cancellationToken = default)                              // İptal token
    {
        IQueryable<TEntity> queryable = Query();        // Temel sorgu oluşturulur.

        if (!enableTracking)
            queryable = queryable.AsNoTracking();       // Takip kapatılabilir.

        if (include != null)
            queryable = include(queryable);             // İlişkili tablolar sorguya dahil edilir (Include/ThenInclude).

        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters(); // Soft delete filtreleri yok sayılabilir.

        // Koşulu sağlayan ilk kaydı (veya yoksa null) asenkron olarak getir.
        return await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    // Verilen kriterlere göre sayfalı (paginated) bir listeyi asenkron olarak döndürür.
    // Filtreleme (predicate), sıralama (orderBy), include, soft delete ve tracking seçenekleri içerir.
    public async Task<Paginate<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,                                  // Filtre koşulu
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,             // Sıralama kriteri
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,  // İlişkiler
        int index = 0,                                                                      // Sayfa indeksi (0 tabanlı)
        int size = 10,                                                                      // Sayfa başına kayıt sayısı
        bool withDeleted = false,                                                          // Silinmişler dahil mi
        bool enableTracking = true,                                                        // Takip açık mı
        CancellationToken cancellationToken = default)                                     // İptal token
    {
        IQueryable<TEntity> queryable = Query();   // Temel sorgu

        if (!enableTracking)
            queryable = queryable.AsNoTracking();  // Takip kapatılabilir.

        if (include != null)
            queryable = include(queryable);        // İlişkiler sorguya eklenir.

        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters(); // Soft delete filtreleri yok sayılabilir.

        if (predicate != null)
            queryable = queryable.Where(predicate);     // Filtre uygulanır.

        // Eğer sıralama belirtilmişse önce sıralayıp sonra sayfalama yap.
        if (orderBy != null)
            return await orderBy(queryable).ToPaginateAsync(index, size, cancellationToken);

        // Sıralama yoksa, direkt sayfalama yap.
        return await queryable.ToPaginateAsync(index, size, cancellationToken);
    }

    // Dinamik sorgu (DynamicQuery) kullanarak sayfalı liste döndürür.
    // DynamicQuery, filtre ve sıralama bilgilerini string/tabanlı yapılarla taşır.
    public async Task<Paginate<TEntity>> GetListByDynamicAsync(
        DynamicQuery dynamic,                                                              // Dinamik sorgu tanımı
        Expression<Func<TEntity, bool>>? predicate = null,                                 // Ek filtre
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,  // İlişkiler
        int index = 0,                                                                     // Sayfa indeksi
        int size = 10,                                                                     // Sayfa boyutu
        bool withDeleted = false,                                                         // Silinmişler dahil mi
        bool enableTracking = true,                                                       // Takip açık mı
        CancellationToken cancellationToken = default)                                    // İptal token
    {
        // Önce temel sorgu üzerinde dinamik filtre ve sıralama uygula.
        IQueryable<TEntity> queryable = Query().ToDynamic(dynamic);

        if (!enableTracking)
            queryable = queryable.AsNoTracking();          // Takip kapalı olabilir.

        if (include != null)
            queryable = include(queryable);                // İlişkiler eklenir.

        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters();    // Soft delete filtreleri iptal edilebilir.

        if (predicate != null)
            queryable = queryable.Where(predicate);        // Ek koşul da uygulanır.

        // Sonuçlar sayfalı olarak döndürülür.
        return await queryable.ToPaginateAsync(index, size, cancellationToken);
    }

    // Temel sorguyu başlatmak için kullanılan yardımcı metot.
    // DbContext içindeki ilgili DbSet<TEntity>'i döndürür.
    public IQueryable<TEntity> Query() => _context.Set<TEntity>();

    // Tek bir varlığı asenkron olarak günceller.
    // UpdatedDate alanını şu anki zamana ayarlar.
    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        entity.UpdatedDate = DateTime.UtcNow; // Güncelleme zamanı atanır.
        _context.Update(entity);              // EF Core'a bu varlığın güncellendiği bildirilir.
        await _context.SaveChangesAsync();    // Değişiklikler veritabanına yazılır.
        return entity;                        // Güncellenmiş varlık geri döner.
    }

    // Birden fazla varlığı topluca asenkron olarak günceller.
    public async Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities)
    {
        // Her bir varlığın güncellenme zamanını güncelle
        foreach (var entity in entities)
        {
            entity.UpdatedDate = DateTime.UtcNow;
        }

        _context.UpdateRange(entities);       // EF Core'a tüm bu varlıkların güncellendiği bildirilir.
        await _context.SaveChangesAsync();    // Değişiklikler veritabanına yazılır.
        return entities;                      // Güncellenmiş varlıklar geri döner.
    }


    // Tek bir varlık için silme davranışını uygular.
    // hardDelete = false ise soft delete; true ise gerçekten veritabanından siler.
    protected async Task SetEntityAsDeletedAsync(TEntity entity, bool hardDelete)
    {
        if (!hardDelete)
        {
            // Önce bu varlığın bire bir (one-to-one) ilişkisi var mı kontrol edilir.
            CheckHasEntityHaveOneToOneRelation(entity);
            // Soft delete uygulanır (DeletedDate alanı set edilir ve ilişkili varlıklara da yayılır).
            await SetEntityAsSoftDeleteAsync(entity);
        }
        else
        {
            // Hard delete: EF Core'a bu varlığı tamamen silmesi söylenir.
            _context.Remove(entity);
        }
    }

    // Bir koleksiyon içindeki tüm varlıklara silme davranışını uygular.
    protected async Task SetEntityAsDeletedAsync(IEnumerable<TEntity> entities, bool hardDelete)
    {
        foreach (TEntity entity in entities)
        {
            // Her varlık için tek tek silme işlemi uygulanır.
            await SetEntityAsDeletedAsync(entity, hardDelete);
        }
    }

    // Soft delete yapılmadan önce, varlığın bire bir (one-to-one) ilişkisi olup olmadığını kontrol eder.
    // Bire bir ilişkilerde soft delete, aynı foreign key ile tekrar kayıt açarken sorun çıkarabileceği için uyarı verir.
    protected void CheckHasEntityHaveOneToOneRelation(TEntity entity)
    {
        // Bu ifade, varlıkla ilgili foreign key'leri inceler ve 
        // bire bir ilişki olup olmadığını bulmaya çalışır.
        bool hasEntityHaveOneToOneRelation =
            _context.Entry(entity)                    // Varlığın EF Core üzerindeki entry'si alınır.
            .Metadata.GetForeignKeys()               // Bu varlığa ait tüm foreign key tanımları alınır.
            .All(a =>
                a.DependentToPrincipal?.IsCollection == true ||
                a.PrincipalToDependent?.IsCollection == true ||
                a.DependentToPrincipal?.ForeignKey.DeclaringEntityType.ClrType == entity.GetType()
            ) == false;

        // Eğer bire bir ilişki tespit edilirse, soft delete'e izin verilmez ve hata fırlatılır.
        if (hasEntityHaveOneToOneRelation)
        {
            throw new InvalidOperationException(
                "Entity has one-to-one relationship. Soft delete causes problems if you try to create entry again by same foreign key");
        }
    }

    // Soft delete uygulayan metot.
    // IEntityTimeStamps arayüzünü kullanan varlıklarda DeletedDate alanını doldurur
    // ve ilişkili varlıklara da (cascading soft delete) bu işlemi yayar.
    protected async Task SetEntityAsSoftDeleteAsync(IEntityTimeStamps entity)
    {
        // Eğer zaten DeletedDate doluysa (daha önce silinmişse) tekrar işlem yapma.
        if (entity.DeletedDate.HasValue) return;

        // Silinme tarihini şu anki zamana ayarla.
        entity.DeletedDate = DateTime.UtcNow;

        // Bu varlığın tüm navigasyon (ilişki) özelliklerini al.
        var navigations = _context
            .Entry(entity)
            .Metadata.GetNavigations()
            // Sadece belirli silme davranışına (DeleteBehavior) sahip olan navigasyonlar filtrelenir:
            // DeleteBehavior.ClientCascade veya DeleteBehavior.Cascade olanlar.
            // Ayrıca dependent (bağımlı) olmayan taraf seçilir.
            .Where(x => x is { IsOnDependent: false, ForeignKey.DeleteBehavior: DeleteBehavior.ClientCascade or DeleteBehavior.Cascade })
            .ToList();

        // Her bir navigation (ilişki) için soft delete'i ilişkilere de uygula.
        foreach (INavigation? navigation in navigations)
        {
            if (navigation.TargetEntityType.IsOwned()) continue;   // Owned type ise atla.
            if (navigation.PropertyInfo == null) continue;         // Property bilgisi yoksa atla.

            // İlgili navigation property üzerinden ilişkili veriyi oku.
            object? navValue = navigation.PropertyInfo.GetValue(entity);

            if (navigation.IsCollection)
            {
                // Eğer koleksiyon null ise, EF üzerinden sorgu ile ilişkili veriyi yükle.
                if (navValue == null)
                {
                    IQueryable query = _context.Entry(entity).Collection(navigation.PropertyInfo.Name).Query();
                    navValue = await GetRelationLoaderQuery(query, navigationPropertyType: navigation.PropertyInfo.GetType()).ToListAsync();

                    if (navValue == null) continue;
                }

                // Koleksiyon içindeki her bir ilişkili varlık için soft delete uygula.
                foreach (IEntityTimeStamps navValueItem in (IEnumerable)navValue)
                {
                    await SetEntityAsSoftDeleteAsync(navValueItem);
                }
            }
            else
            {
                // Tekil navigation ise ve değer null ise, sorgu ile yüklemeye çalış.
                if (navValue == null)
                {
                    IQueryable query = _context.Entry(entity).Reference(navigation.PropertyInfo.Name).Query();
                    navValue = await GetRelationLoaderQuery(query, navigationPropertyType: navigation.PropertyInfo.GetType()).FirstOrDefaultAsync();

                    if (navValue == null) continue;
                }

                // Tekil ilişkili varlık için soft delete uygula.
                await SetEntityAsSoftDeleteAsync((IEntityTimeStamps)navValue);
            }
        }

        // Bu entity'nin durumu EF Core tarafında güncellendi olarak işaretlenir.
        _context.Update(entity);
    }

    // İlişkili varlıkları yüklemek için genel bir sorgu oluşturur.
    // Verilen IQueryable ve hedef navigation tipi kullanılarak dinamik bir sorgu oluşturur
    // ve DeletedDate değeri dolu olmayan (yani silinmemiş) kayıtları filtreler.
    protected IQueryable<object> GetRelationLoaderQuery(IQueryable query, Type navigationPropertyType)
    {
        // Sorgunun sağlayıcısının (IQueryProvider) çalışma zamanındaki tipi alınır.
        Type queryProviderType = query.Provider.GetType();

        // IQueryProvider.CreateQuery<T> metodunu yansıma (reflection) ile bulur
        // ve navigationPropertyType tipiyle generic hale getirir.
        MethodInfo createQueryMethod = queryProviderType
                .GetMethods()
                .First(m => m is { Name: nameof(query.Provider.CreateQuery), IsGenericMethod: true })?.MakeGenericMethod(navigationPropertyType) ?? throw new InvalidOperationException("CreateQuery<TElement> method is not found in IQueryProvider.");

        // CreateQuery<T> metodu çağrılarak, verilen expression'dan yeni bir IQueryable<T> üretilir.
        var queryProviderQuery = (IQueryable<object>)createQueryMethod.Invoke(query.Provider, parameters: new object[] { query.Expression })!;

        // DeletedDate'i boş (null) olan, yani soft delete yapılmamış kayıtlar filtrelenerek döndürülür.
        return queryProviderQuery.Where(x => !((IEntityTimeStamps)x).DeletedDate.HasValue);
    }
}
