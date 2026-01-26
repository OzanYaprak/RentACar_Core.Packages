using Microsoft.EntityFrameworkCore;

namespace Core.Persistence.Paging;

// IQueryable arayüzü için sayfalama (pagination) işlemlerini kolaylaştıran genişletme (extension) metotlarını içerir.
public static class IQueryablePaginateExtensions
{
    #region Async

    // ToPaginateAsync metodu, verilen bir IQueryable sorgusunu asenkron olarak sayfalı (paginated) bir listeye dönüştürür.
    // query: Sorgulanacak veri kümesi.
    // index: Getirilecek sayfanın indeksi (0 tabanlı).
    // size: Sayfa başına düşen kayıt sayısı.
    // cancellationToken: Asenkron işlemin iptal edilmesini sağlayan belirteç.
    public static async Task<Paginate<TEntity>> ToPaginateAsync<TEntity>(
        this IQueryable<TEntity> query,
        int index,
        int size,
        CancellationToken cancellationToken = default)
    {
        // Toplam kayıt sayısını asenkron olarak alır.
        int count = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        // İlgili sayfadaki kayıtları asenkron olarak alır (skip/take ile).
        List<TEntity> items = await query.Skip(index * size).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);

        // Sayfalama bilgilerini ve kayıtları içeren Paginate nesnesi oluşturulur.
        Paginate<TEntity> list = new()
        {
            Index = index, // Mevcut sayfa indeksi
            Count = count, // Toplam kayıt sayısı
            Items = items, // Sayfadaki kayıtlar
            Size = size,   // Sayfa başına kayıt sayısı
            Pages = (int)Math.Ceiling(count / (double)size)// Toplam sayfa sayısı
        };

        // Sonuç olarak Paginate nesnesi döndürülür.
        return list;
    }

    #endregion Async

    #region Sync

    // ToPaginate metodu, verilen bir IQueryable sorgusunu senkron olarak sayfalı (paginated) bir listeye dönüştürür.
    // source: Sorgulanacak veri kümesi.
    // index: Getirilecek sayfanın indeksi (0 tabanlı).
    // size: Sayfa başına düşen kayıt sayısı.
    public static Paginate<T> ToPaginate<T>(this IQueryable<T> source, int index, int size)
    {
        // Toplam kayıt sayısını alır.
        int count = source.Count();

        // İlgili sayfadaki kayıtları alır (skip/take ile).
        var items = source.Skip(index * size).Take(size).ToList();

        // Sayfalama bilgilerini ve kayıtları içeren Paginate nesnesi oluşturulur.
        Paginate<T> list = new()
        {
            Index = index, // Mevcut sayfa indeksi
            Size = size,   // Sayfa başına kayıt sayısı
            Count = count, // Toplam kayıt sayısı
            Items = items, // Sayfadaki kayıtlar
            Pages = (int)Math.Ceiling(count / (double)size) // Toplam sayfa sayısı
        };

        // Sonuç olarak Paginate nesnesi döndürülür.
        return list;
    }

    #endregion Sync
}
