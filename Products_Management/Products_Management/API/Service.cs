namespace Products_Management.API
{
    public interface IEntityService
    {
        Task<List<EntityResponse>> GetAllAsync();
        Task<EntityResponse?> GetByIdAsync(int id);
        Task<EntityResponse> AddAsync(EntityRequest request, string? imgUrl);
        Task<EntityResponse?> UpdateAsync(int id, EntityRequest request, string? imgUrl);
        Task<bool> DeleteAsync(int id);
    }

    public class EntityService : IEntityService
    {
        private readonly IEntityRepository _repository;

        public EntityService(IEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<EntityResponse>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(e => new EntityResponse
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Price = e.Price,
                ImageUrl = e.ImageUrl
            }).ToList();
        }

        public async Task<EntityResponse?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;

            return new EntityResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                ImageUrl = entity.ImageUrl
            };
        }

        public async Task<EntityResponse> AddAsync(EntityRequest request, string? imgUrl)
        {
            var entity = new Entity
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                ImageUrl = imgUrl ?? ""
            };

            var created = await _repository.AddAsync(entity);

            return new EntityResponse
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description,
                Price = created.Price,
                ImageUrl = created.ImageUrl
            };
        }

        public async Task<EntityResponse?> UpdateAsync(int id, EntityRequest request, string? imgUrl)
        {
            var entity = new Entity
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                ImageUrl = imgUrl ?? ""
            };

            var updated = await _repository.UpdateAsync(entity);
            if (updated == null) return null;

            return new EntityResponse
            {
                Id = updated.Id,
                Name = updated.Name,
                Description = updated.Description,
                Price = updated.Price,
                ImageUrl = updated.ImageUrl
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}