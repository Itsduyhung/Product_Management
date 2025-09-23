using Microsoft.EntityFrameworkCore;
using Products_Management.API;

namespace Products_Management.API
{
    public interface IEntityRepository
    {
        Task<List<Entity>> GetAllAsync();
        Task<Entity?> GetByIdAsync(int id);
        Task<Entity> AddAsync(Entity entity);
        Task<Entity?> UpdateAsync(Entity entity);
        Task<bool> DeleteAsync(int id);
    }

    public class EntityRepository : IEntityRepository
    {
        private readonly ApplicationDbContext _context;

        public EntityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entity>> GetAllAsync() =>
            await _context.Entities.ToListAsync();

        public async Task<Entity?> GetByIdAsync(int id) =>
            await _context.Entities.FindAsync(id);

        public async Task<Entity> AddAsync(Entity entity)
        {
            _context.Entities.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Entity?> UpdateAsync(Entity entity)
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