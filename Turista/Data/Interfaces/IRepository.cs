using Turista.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public interface IRepository<Entity> : IDisposable where Entity : Turista.Data.Entity
{
    IQueryable<Entity> Table { get; }

    bool IsDeleted(object id);
    bool Any(Expression<Func<Entity, bool>> filter);
    Task<bool> AnyAsync(Expression<Func<Entity, bool>> filter);
    IEnumerable<Entity> Get(string languageCode);

    IEnumerable<Entity> Get(Expression<Func<Entity, bool>> filter, string languageCode);
    Entity Find(object id);
    Task<Entity> FindAsync(object id);
    Guid Add(Entity entity);

    //Guid Add(IRepository<Entity> masterRepository, );

    Task<Guid> AddAsync(Entity entity, CancellationToken cancellationToken = default);
    void AddRange(IEnumerable<Entity> entities);
    Task AddRangeAsync(IEnumerable<Entity> entities, CancellationToken cancellationToken = default);

    Task<List<Entity>> ToListAsync();

    void Update(Entity entity);
    Task UpdateAsync(Entity entity);
    void UpdateRange(IEnumerable<Entity> entities);
    //void Remove(Entity entity);
    //Task RemoveAsync(Entity entity);

    void Restore(object id);

    bool Remove(Entity entity);
    Task RestoreAsync(object id);

    bool RemoveSoft(object id);
    Task<bool> RemoveSoftAsync(object id);
    bool RemoveHard(object id);
    Task<bool> RemoveHardAsync(object id);
    Task<bool> RemoveSoftRange(IEnumerable<Entity> entities);
    Task<bool> RemoveHardRange(IEnumerable<Entity> entities);

    string GetEntityName();

}