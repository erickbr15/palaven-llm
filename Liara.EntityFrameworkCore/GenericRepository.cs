using Liara.Common.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Liara.EntityFrameworkCore;

public abstract class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly DbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;

    /// <summary>
    ///     Creates an instance of <see cref="GenericRepository"/>
    /// </summary>
    /// <param name="dbContext">
    ///     An instance of <see cref="ExpensesDbContext"/>
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     It is thrown when the <paramref name="dbContext"/> is null
    /// </exception>
    public GenericRepository(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = _dbContext.Set<TEntity>();
    }

    /// <inheritdoc />
    public void Add(TEntity entity)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        _dbSet.Add(entity);
    }

    /// <inheritdoc />
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        return _dbSet.AddAsync(entity, cancellationToken).AsTask();
    }

    /// <inheritdoc />
    public void Delete(TEntity entity)
    {
        if (entity is null)
        {
            return;
        }

        if (_dbContext.Entry(entity)?.State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        _dbSet.Remove(entity);
    }

    /// <inheritdoc />
    public bool Exists(object id)
    {
        var exists = _dbSet.Find(id) != null;
        return exists;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(object id, CancellationToken cancellationToken)
    {
        var exists = await _dbSet.FindAsync(new object[] { id }, cancellationToken: cancellationToken) != null;
        return exists;
    }

    /// <inheritdoc />
    public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy, string includeProperties)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        IQueryable<TEntity> query = _dbSet;

        query = query.Where(filter);

        if (!string.IsNullOrEmpty(includeProperties))
        {
            var propertiesToInclude = includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var includeProperty in propertiesToInclude)
            {
                query = query.Include(includeProperty);
            }
        }

        if (orderBy != null)
        {
            return orderBy(query);
        }

        return query;
    }

    /// <inheritdoc />
    public IQueryable<TEntity> GetAll()
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();
        return query;
    }

    /// <inheritdoc />
    public TEntity? GetById(object id)
    {
        return _dbSet.Find(id);
    }

    /// <inheritdoc />
    public Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken)
    {
        return _dbSet.FindAsync(new object[] { id }, cancellationToken).AsTask();
    }

    /// <inheritdoc />
    public void Update(TEntity entity)
    {
        _dbSet.Attach(entity);
        _dbContext.Entry(entity).State = EntityState.Modified;
    }
}
