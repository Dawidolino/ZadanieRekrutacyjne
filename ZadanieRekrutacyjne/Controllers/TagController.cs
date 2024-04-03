using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;
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
        private readonly ILogger<TagController> _logger;
                 
        public TagController(HttpClient httpClient, TagContext tagContext, ITagApiConfiguration tagApiConfiguration, ILogger<TagController> logger)
        {
            _httpClient = httpClient;
            _tagContext = tagContext;
            _tagApiConfiguration = tagApiConfiguration;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTags(int totalTags, int currentPage = 1   )
        {
            var allTags = new List<Tag>();
            //var totalFetchedCount = 0;

            //Loop to jump to the next page starting from page 1
            while (allTags.Count < totalTags)
            {
                var fetchedTags = await GetTagsFromApi(currentPage);
                //totalFetchedCount += fetchedTags.Count;

                allTags.AddRange(fetchedTags);

                // Check if we've reached the desired totalTags
                if (allTags.Count >= totalTags)
                {
                    break;
                }

                currentPage++;
            }
            var totalCount = allTags.Sum(tag => tag.Count);

            await SaveTagsToDatabase(allTags);

            return Ok(allTags);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetTags(int id)
        {
            if (_tagContext == null)
            {
                return NotFound();
            }
            var tags = await _tagContext.Tags.FindAsync(id);
            if (tags == null) { return NotFound(); }
            return Ok(tags);

        }
        private async Task<List<Tag>> GetTagsFromApi(int currentPage)
        {
            var apiKey = _tagApiConfiguration.ApiKey;
            var baseUrl = "https://api.stackexchange.com/2.3/tags?";


            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
            //search by popularity, sort by descending count (most searched first), filter by name and count only
            var url = baseUrl + $"site=stackoverflow&sort=popular&order=desc&page={currentPage}&pagesize=100&key={apiKey}&filter=!4-C9.H1YNh.sprLqs";  
                
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentEncoding.Contains("gzip") ){           //json is compressed into gzip and needs to be decompressed first
                using (Stream stream = await response.Content.ReadAsStreamAsync())
                using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                using (StreamReader reader = new StreamReader(gZipStream))
                {
                    var responseBody = await reader.ReadToEndAsync();
                    var tagApiResponse = JsonConvert.DeserializeObject<TagApiResponse>(responseBody);
                    return tagApiResponse.Items;
                }

            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tagApiResponse = JsonConvert.DeserializeObject<TagApiResponse>(responseContent);
                return tagApiResponse.Items; // Return deserialized tags list for non-Gzip
            }

            //previous attempt of deserializing json

            //if (response.IsSuccessStatusCode)
            //    {
            //    var responseContent = await response.Content.ReadAsStringAsync();
            //   // Console.WriteLine("Response from API: " + responseContent);
            //   // responseContent = responseContent.Trim('\u001F');

            //    //_logger.LogInformation("Response content: {content}", responseContent);
            //        var tagApiResponse = JsonConvert.DeserializeObject<TagApiResponse>(responseContent); // Deserialize to TagApiResponse
            //        var tags = tagApiResponse.Items.Select(tagInfo => new Tag { Name = tagInfo.Name }).ToList();
            //        return tags;
            //}
            //    else
            //    {
            //        throw new Exception($"Failed to get tags from API: {response.StatusCode}");
            //    }

        }
        private async Task SaveTagsToDatabase(List<Tag> tags)
        {
     
            foreach (var tag in tags)
            {
                if (!_tagContext.Tags.Any(t => t.Name == tag.Name)) // Check for existing tag
                {
                    //tag.Percentage = (double)tag.Count / totalCount * 100;
                    _tagContext.Tags.Add(tag);
                }
            }
            // calculate percentages for all tags
            var allTags = await _tagContext.Tags.ToListAsync();
            var totalCount = allTags.Sum(tag => tag.Count);

            foreach (var tag in allTags)
            {
                tag.Percentage = (double)tag.Count / totalCount * 100;
            }
            await _tagContext.SaveChangesAsync(); // Save all changes to database
        }
    }
}
