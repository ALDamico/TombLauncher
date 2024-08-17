using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace TombLauncher.Database.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    internal EfRepository(TombLauncherDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<T>();
    }
    private TombLauncherDbContext _dbContext;
    private DbSet<T> _dbSet;

    public IQueryable<T> GetAll()
    {
        return Get();
    }
    
    public IQueryable<T> Get(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "")
    {
        IQueryable<T> query = _dbSet;
        if (filter != null)
        {
            query = query.Where(filter);
        }
        
        foreach (var includeProperty in includeProperties.Split
                     (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            return orderBy(query);
        }

        return query;
    }

    public T GetEntityById(int id)
    {
        return _dbSet.Find(id);
    }

    public void Insert(T entity)
    {
        _dbSet.Add(entity);
    }

    public bool Delete(int id)
    {
        var entityToDelete = _dbSet.Find(id);
        if (entityToDelete == null)
        {
            return false;
        }

        return Delete(entityToDelete);
    }

    public bool Delete(T entity)
    {
        if (_dbContext.Entry(entity).State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }

        _dbSet.Remove(entity);
        return true;
    }

    public bool Update(T entity)
    {
        throw new System.NotImplementedException();
    }

    public void Commit()
    {
        _dbContext.SaveChanges();
    }
}