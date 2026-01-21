namespace Core.Persistence.Paging;

// Paginate<TEntity> sınıfı, sayfalama (pagination) işlemleri için kullanılır.
// Generic olarak herhangi bir TEntity tipiyle çalışabilir.
public class Paginate<TEntity>
{
    // Varsayılan kurucu metot. Items listesini boş bir dizi olarak başlatır.
    public Paginate() { Items = Array.Empty<TEntity>(); }

    // Bir sayfadaki eleman (kayıt) sayısını tutar.
    public int Size { get; set; }

    // Mevcut sayfanın indeksini (0 tabanlı) tutar.
    public int Index { get; set; }

    // Toplam kayıt sayısını tutar.
    public int Count { get; set; }

    // Toplam sayfa sayısını tutar.
    public int Pages { get; set; }

    // Sayfadaki elemanların listesini tutar.
    public IList<TEntity> Items { get; set; }

    // Önceki bir sayfa olup olmadığını belirten özellik.
    // Index > 0 ise önceki sayfa vardır.
    public bool HasPrevios => Index > 0;

    // Sonraki bir sayfa olup olmadığını belirten özellik.
    // (Index + 1) < Pages ise sonraki sayfa vardır.
    public bool HasNext => Index + 1 < Pages;
}
