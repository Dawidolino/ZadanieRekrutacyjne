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
        public async Task<IActionResult> GetTags()
        {
            var tags =await GetTagsFromApi();           
            SaveTagsToDatabase(tags);

            return Ok(tags);
        }

        private async Task<List<Tag>> GetTagsFromApi(int offSet =0)
        {
            var apiKey = _tagApiConfiguration.ApiKey;
            var baseUrl = "https://api.stackexchange.com/2.3/tags?";


            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var url = baseUrl + $"pagesize=100&order=desc&sort=popular&site=stackoverflow&key={apiKey}&from={offSet}";
            var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                  _logger.LogInformation("Response content: {content}", responseContent);


                try
                {
                    // Simplified deserialization for initial inspection
                    var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                    var items = (List<object>)responseData["items"];

                    // Check for expected structure and handle potential null values
                    var tags = new List<Tag>();
                    foreach (var item in items)
                    {
                        var tagDict = item as Dictionary<string, object>;
                        if (tagDict != null)
                        {
                            var name = tagDict["name"] as string;
                            var count = Convert.ToInt32(tagDict["count"]);

                            // Handle optional "collectives" property
                            List<object> collectives = null;
                            if (tagDict.ContainsKey("collectives"))
                            {
                                collectives = tagDict["collectives"] as List<object>;
                            }

                            tags.Add(new Tag
                            {
                                Name = name,
                                Count = count,
                                Collectives = collectives // May be null
                            });
                        }
                    }

                    return tags;
                }
                catch (JsonException ex)
                {
                    _logger.LogError("Error deserializing JSON response: {message}", ex.Message);
                    throw; // Re-throw the exception for handling by caller
                }
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
