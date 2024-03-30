using Microsoft.AspNetCore.Mvc;
using ZadanieRekrutacyjne.Services;

namespace ZadanieRekrutacyjne.Controllers
{
    [Route("api/gettags")]
    [ApiController]
    public class TagController : Controller
    {
        private readonly TagDownloader _tagDownloader;

        public TagController(TagDownloader tagDownloader)
        {
            _tagDownloader = tagDownloader;
        }
        [HttpGet]
        public async Task<IActionResult> GetTags()
        {
            try
            {
                await _tagDownloader.DownloadTagsFromStackOverflow();
                return Ok("Tags downloaded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to download tags: {ex.Message}");
            }
        }

    }
}
