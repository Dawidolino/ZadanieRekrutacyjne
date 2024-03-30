using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http;
using ZadanieRekrutacyjne.Model;

namespace ZadanieRekrutacyjne.Services
{   
    public class TagDownloader
    {
        private readonly HttpClient _httpClient;
        private readonly TagContext _tagContext;
        private readonly ITagApiConfiguration _tagApiConfiguration;
        private readonly ILogger<TagDownloader> _logger;

        public TagDownloader(HttpClient httpClient, TagContext tagContext, ITagApiConfiguration tagApiConfiguration, ILogger<TagDownloader> logger)
        {
            _httpClient = httpClient;
            _tagContext = tagContext;
            _tagApiConfiguration = tagApiConfiguration;
            _logger = logger;
        }
        public async Task DownloadTagsFromStackOverflow()
        {
            var apiKey = _tagApiConfiguration.ApiKey;
            var url = $"https://api.stackexchange.com/2.2/tags?pagesize=1&order=desc&sort=popular&site=stackoverflow&key={apiKey}";
            _logger.LogInformation("Downloading tags from {url}", url);

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Response content: {content}", content);

                var tagsResponse = JsonConvert.DeserializeObject<TagsResponse>(content);

                if (tagsResponse != null && tagsResponse.Tags != null)
                {
                    var totalTagsCount = tagsResponse.Tags.Sum(t => t.Count);

                    foreach (var tag in tagsResponse.Tags)
                    {
                        _logger.LogInformation("Processing tag: {Name}", tag.Name);

                        var existingTag = await _tagContext.Tags.FirstOrDefaultAsync(t => t.Name == tag.Name);

                        if (existingTag != null)
                        {
                            existingTag.Count = tag.Count;
                            existingTag.Percentage = (double)tag.Count / totalTagsCount;
                        }
                        else
                        {
                            tag.Percentage = (double)tag.Count / totalTagsCount;
                            await _tagContext.Tags.AddAsync(tag);
                        }
                    }

                    await _tagContext.SaveChangesAsync();
                }
            }
            else
            {
                _logger.LogError("Failed to download tags. Status code: {StatusCode}", response.StatusCode);
                throw new Exception($"Failed to download tags. Status code: {response.StatusCode}");
            }
        }
    }
    public class TagsResponse
    {
        public Tag[]? Tags { get; set; }
    }

}

