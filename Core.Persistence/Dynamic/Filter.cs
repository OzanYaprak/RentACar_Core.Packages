namespace Core.Persistence.Dynamic;

// Filter sınıfı, dinamik sorgularda filtreleme kriterlerini temsil eder.
public class Filter
{
    // Varsayılan kurucu metot. Field ve Operator özelliklerini boş string olarak başlatır.
    public Filter()
    {
        Field = string.Empty;
        Operator = string.Empty;
    }

    // Parametreli kurucu metot. Field ve Operator özelliklerini verilen değerlerle başlatır.
    // Operator parametresi, filtreleme işleminin türünü belirtir (ör. "eq", "contains", "gt" gibi karşılaştırma operatörleri).
    public Filter(string field, string @operator) //operator bir C# anahtar kelimesi olduğu için doğrudan kullanılamaz. @ ile kullanıldı // Buradaki @operator, C#'a bunun bir anahtar kelime değil, bir tanımlayıcı (identifier) olarak kullanılacağını belirtir.
    {
        Field = field;
        Operator = @operator;
    }

    // Filtrelenecek alanın (property/kolon) adı.
    public string Field { get; set; }

    // Filtreleme işlemi için kullanılacak değer (opsiyonel, null olabilir).
    public string? Value { get; set; }

    // Filtreleme işleminin türü (ör. "eq", "contains", "gt" gibi karşılaştırma operatörleri).
    public string Operator { get; set; }

    // Birden fazla filtre arasında mantıksal ilişkiyi belirten operatör (ör. "and", "or") (opsiyonel, null olabilir).
    public string? Logic { get; set; }

    // Alt filtreleri tutan koleksiyon. Çoklu ve iç içe filtreleme için kullanılır.
    public IEnumerable<Filter>? Filters { get; set; } = null;
}
