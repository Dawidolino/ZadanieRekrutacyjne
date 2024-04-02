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
        private readonly ILogger<TagDownloader> _logger;
                 
        public TagController(HttpClient httpClient, TagContext tagContext, ITagApiConfiguration tagApiConfiguration, ILogger<TagDownloader> logger)
        {
            _httpClient = httpClient;
            _tagContext = tagContext;
            _tagApiConfiguration = tagApiConfiguration;
            _logger = logger;
        }

        [HttpGet()]
        public async Task<IActionResult> GetTags(int limit)
        {
            var totalTags = 1000;
            var offset = 0;
            var allTags = new List<Tag>();

            while (allTags.Count < totalTags)
            {
                var fetchedTags = await GetTagsFromApi(limit, offset); // Get tags with current offset
                allTags.AddRange(fetchedTags); // Add fetched tags to the main list
                offset += limit; // Increment offset for the next iteration
            }
            //var tags =await GetTagsFromApi(limit);
            await SaveTagsToDatabase(allTags);

            return Ok(allTags);
        }

        private async Task<List<Tag>> GetTagsFromApi(int limit, int offset)
        {
            var apiKey = _tagApiConfiguration.ApiKey;
            var baseUrl = "https://api.stackexchange.com/2.3/tags?";


            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var url = baseUrl + $"site=stackoverflow&pagesize={limit}&offset={offset}&key={apiKey}";
                
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentEncoding.Contains("gzip") ){                                   //json is compressed into gzip and needs to be decompressed first
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
                    _tagContext.Tags.Add(tag);
                }
            }

            await _tagContext.SaveChangesAsync(); // Save all changes to database
        }
    }
}
