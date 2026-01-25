// Bu dosyada sayfalama ile ilgili ortak modellerin bulunduğu namespace'i kullanır
using Core.Persistence.Paging;

// Bu sınıfın proje içindeki konumsal adıdır (response modelleri burada toplanır)
namespace Core.Application.Responses;

// Genel (generic) bir cevap modeli: TEntity türünden öğeler içeren liste cevabı
// BasePageableModel'den kalıtım alır, böylece sayfalama ile ilgili özellikleri de içerir
public class GetListResponse<TEntity> : BasePageableModel
{
    // Liste öğelerini saklamak için kullanılan özel (private) backing field
    // Başlangıçta null olabilir; dolayısıyla getter içinde tembel (lazy) oluşturma uygulanır
    private IList<TEntity> _items;

    // Dışarıya açık olarak öğelere erişim sağlayan property
    // Bu property, null ise otomatik olarak boş bir `List<TEntity>` oluşturur
    public IList<TEntity> Items
    {
        // Getter: _items null ise yeni bir List oluşturur ve atar, sonra döndürür
        get => _items ??= new List<TEntity>();
        // Setter: dışarıdan gelen koleksiyonu backing field'a atar
        set => _items = value;
    }
}
