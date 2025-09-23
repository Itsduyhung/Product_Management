using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;

namespace Products_Management.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntityController : ControllerBase
    {
        private readonly IEntityService _service;
        private readonly Cloudinary _cloudinary;

        public EntityController(IEntityService service, Cloudinary cloudinary)
        {
            _service = service;
            _cloudinary = cloudinary;
        }

        [HttpGet]
        public async Task<ActionResult<List<EntityResponse>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EntityResponse>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<EntityResponse>> Create([FromForm] EntityRequest request)
        {
            string? imgUrl = null;

            if (request.ImageUrl != null)
            {
                using var stream = request.ImageUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.ImageUrl.FileName, stream),
                    Folder = "products" // ✅ Cloudinary folder (tuỳ bạn đặt)
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                imgUrl = uploadResult.SecureUrl?.ToString();
            }

            var result = await _service.AddAsync(request, imgUrl);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EntityResponse>> Update(int id, [FromForm] EntityRequest request)
        {
            string? imgUrl = null;

            if (request.ImageUrl != null)
            {
                using var stream = request.ImageUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.ImageUrl.FileName, stream),
                    Folder = "products"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                imgUrl = uploadResult.SecureUrl?.ToString();
            }

            var result = await _service.UpdateAsync(id, request, imgUrl);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}