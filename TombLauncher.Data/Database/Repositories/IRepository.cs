﻿using System.Linq.Expressions;

namespace TombLauncher.Data.Database.Repositories;

public interface IRepository<T> where T : class
{
    IQueryable<T> GetAll();

    IQueryable<T> Get(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        string includeProperties = "");

    T GetEntityById(int id);
    void Insert(T entity);
    bool Delete(int id);
    bool Update(T entity);
    Task Commit();
    bool Upsert(T entity);
}