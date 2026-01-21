namespace Core.Persistence.Repositories;

// IQuery<TEntity> arayüzü, generic olarak sorgulama işlemleri için kullanılır.
public interface IQuery<TEntity>
{
    // Query metodu, TEntity tipinde varlıklar için sorgu oluşturmak amacıyla kullanılır.
    // IQueryable<TEntity> döndürerek, LINQ sorgularının zincirleme şekilde yazılmasına ve veritabanına gönderilmeden önce sorgunun şekillendirilmesine olanak tanır.
    IQueryable<TEntity> Query();
}
