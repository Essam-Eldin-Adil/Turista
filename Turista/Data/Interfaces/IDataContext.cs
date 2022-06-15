using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;

public interface IDataContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    EntityEntry Entry(object TEntity);
    int SaveChanges();
    void BeginTransaction();
    void Commit();
    void Rollback();
}