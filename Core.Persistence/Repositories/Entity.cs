namespace Core.Persistence.Repositories;

// Entity<TId> sınıfı, temel varlık (entity) özelliklerini ve kimlik bilgisini tutar.
// TId: Varlığın kimlik (Id) tipini belirtir (ör. int, Guid, string).
public class Entity<TId> : IEntityTimeStamps
{
    // Varsayılan kurucu metot. Id özelliğini tipin varsayılan değeriyle başlatır.
    public Entity()
    {
        Id = default!;
    }

    // Parametreli kurucu metot. Id özelliğini verilen id ile başlatır.
    public Entity(TId id)
    {
        Id = id;
    }

    // Varlığın kimlik (Id) bilgisi.
    public TId Id { get; set; }

    // Varlığın oluşturulma tarihi.
    public DateTime CreatedDate { get; set; }

    // Varlığın güncellenme tarihi (opsiyonel, null olabilir).
    public DateTime? UpdatedDate { get; set; }

    // Varlığın silinme tarihi (opsiyonel, null olabilir).
    public DateTime? DeletedDate { get; set; }
}
