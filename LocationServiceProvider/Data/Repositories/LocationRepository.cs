using LocationServiceProvider.Data.Contexts;
using LocationServiceProvider.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LocationServiceProvider.Data.Repositories
{
    public interface ILocationRepository
    {
        Task<bool> CreateAsync(LocationEntity entity);
        Task<bool> ExistsAsync(Expression<Func<LocationEntity, bool>> expression);
        Task<IEnumerable<LocationEntity>> GetAllAsync(Expression<Func<LocationEntity, object>>? sortBy = null, params Expression<Func<LocationEntity, object>>[] includes);
        Task<LocationEntity?> GetAsync(Expression<Func<LocationEntity, bool>> findBy, params Expression<Func<LocationEntity, object>>[] includes);
        Task<bool> UpdateAsync(LocationEntity entity);
        Task<bool> DeleteAsync(Expression<Func<LocationEntity, bool>> expression);
    }

    public class LocationRepository(DataContext context) : ILocationRepository
    {
        protected readonly DataContext _context = context;

        public async Task<bool> CreateAsync(LocationEntity entity)
        {
            try
            {
                _context.Locations.Add(entity);
                await _context.SaveChangesAsync();
                return true; 
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> ExistsAsync(Expression<Func<LocationEntity, bool>> expression)
        {
            try
            {
                var exists = await _context.Locations.AnyAsync(expression);
                return exists;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR: {ex.Message}");
            }
            return false;
        }

        public async Task<IEnumerable<LocationEntity>> GetAllAsync(Expression<Func<LocationEntity, object>>? sortBy = null, params Expression<Func<LocationEntity, object>>[] includes)
        {
            try
            {
                IQueryable<LocationEntity> query = _context.Locations;

                if (includes.Length != 0)
                    foreach (var include in includes)
                        query = query.Include(include);

                if (sortBy != null)
                    query = query.OrderBy(sortBy);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR: {ex.Message}");
            }
            return [];
        }

        public async Task<LocationEntity?> GetAsync(Expression<Func<LocationEntity, bool>> findBy, params Expression<Func<LocationEntity, object>>[] includes)
        {
            try
            {
                IQueryable<LocationEntity> query = _context.Locations;

                if (includes.Length != 0)
                    foreach (var include in includes)
                        query = query.Include(include);

                var entity = await query.FirstOrDefaultAsync(findBy);
                return entity;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> UpdateAsync(LocationEntity entity)
        {
            try
            {
                _context.Locations.Update(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> DeleteAsync(Expression<Func<LocationEntity, bool>> expression)
        {
            try
            {
                var entity = await _context.Locations.FirstOrDefaultAsync(expression);
                if (entity == null)
                    return false; 

                _context.Locations.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR: {ex.Message}");
            }
            return false;
        }
    }
}
