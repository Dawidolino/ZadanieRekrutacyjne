using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using ZadanieRekrutacyjne.Model;
using ZadanieRekrutacyjne.Services;

namespace ZadanieRekrutacyjne.Controllers
{
    [Route("api/gettags")]
    [ApiController]
    public class TagController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly TagContext _tagContext;
        private readonly ITagApiConfiguration _tagApiConfiguration;
        private readonly ILogger<TagDownloader> _logger;
       
        //private readonly TagDownloader _tagDownloader;

        //public TagController(TagDownloader tagDownloader)
        //{
        //    _tagDownloader = tagDownloader;
        //}
        //[HttpGet]
        //public async Task<IActionResult> GetTags()
        //{
        //    try
        //    {
        //        await _tagDownloader.DownloadTagsFromStackOverflow();
        //        return Ok("Tags downloaded successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Failed to download tags: {ex.Message}");
        //    }
        //}

        

        public TagController(HttpClient httpClient, TagContext tagContext, ITagApiConfiguration tagApiConfiguration, ILogger<TagDownloader> logger)
        {
            _httpClient = httpClient;
            _tagContext = tagContext;
            _tagApiConfiguration = tagApiConfiguration;
            _logger = logger;
        }

        [HttpGet()]
        public async Task<IActionResult> GetTags(int offset = 0, int limit = 100)
        {
            var tags =await GetTagsFromApi(offset, limit);           
            SaveTagsToDatabase(tags);

            return Ok(tags);
        }

        private async Task<List<Tag>> GetTagsFromApi(int offset, int limit)
        {
            var apiKey = _tagApiConfiguration.ApiKey;
            var baseUrl = "https://api.stackexchange.com/2.3/tags?";


            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var url = baseUrl + $"site=stackoverflow&pagesize={limit}&from={offset}&key={apiKey}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(); // Read response as string
                    _logger.LogInformation("Response content: {content}", responseContent);
                    var tags = JsonConvert.DeserializeObject<List<Tag>>(responseContent); // Deserialize to List<Tag>
                   // Tag c = JsonConvert.DeserializeObject<List<Tag>>(responseContent);
                    return tags;
                }
                else
                {
                    throw new Exception($"Failed to get tags from API: {response.StatusCode}");
                }
            
        }
        private void SaveTagsToDatabase(List<Tag> tags)
        {
            foreach (var tag in tags)
            {
                if (!_tagContext.Tags.Any(t => t.Name == tag.Name)) // Check for existing tag
                {
                    _tagContext.Tags.Add(tag);
                }
            }
            _tagContext.SaveChanges(); // Save all changes to database
        }
    }
}
