using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sprouty.Api.Services;

namespace Sprouty.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IFileStorageService _storage;

    public UploadController(IFileStorageService storage) => _storage = storage;

    [HttpPost]
    [RequestSizeLimit(30_000_000)] // 30 MB
    public async Task<ActionResult<object>> Upload(IFormFile file, [FromQuery] string subfolder = "media")
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        try
        {
            var url = await _storage.SaveAsync(file, subfolder);
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var type = ext is ".mp4" or ".webm" or ".mov" ? "video" : "image";
            return Ok(new { url, mediaType = type });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
