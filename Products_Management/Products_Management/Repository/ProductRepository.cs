using Microsoft.EntityFrameworkCore;
using Products_Management.Configuration;
using Products_Management.Model;

namespace Products_Management.Repository
{
    public interface IEntityRepository
    {
        Task<List<ProductEntity>> GetAllAsync();
        Task<ProductEntity?> GetByIdAsync(int id);
        Task<ProductEntity> AddAsync(ProductEntity entity);
        Task<ProductEntity?> UpdateAsync(ProductEntity entity);
        Task<bool> DeleteAsync(int id);
    }

    public class EntityRepository : IEntityRepository
    {
        private readonly ApplicationDbContext _context;

        public EntityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductEntity>> GetAllAsync() =>
            await _context.Entities.ToListAsync();

        public async Task<ProductEntity?> GetByIdAsync(int id) =>
            await _context.Entities.FindAsync(id);

        public async Task<ProductEntity> AddAsync(ProductEntity entity)
        {
            _context.Entities.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ProductEntity?> UpdateAsync(ProductEntity entity)
        {
            var existing = await _context.Entities.FindAsync(entity.Id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Entities.FindAsync(id);
            if (entity == null) return false;

            _context.Entities.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}