using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure.Storage.Blobs;

namespace Blobpics.Pages;

[BindProperties]
public class IndexModel : PageModel
{
    private IConfiguration _configuration;
    private string _connectionString;
    private const string _containerName = "kissakuvakontti";
    private BlobContainerClient _containerClient;
    private readonly ILogger<IndexModel> _logger;

    [BindProperty]
    public List<string> ImageUrls { get; set; } = new List<string>();

    public IndexModel(IConfiguration configuration, ILogger<IndexModel> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _connectionString = _configuration.GetConnectionString("AzureStorage");

        _containerClient = new BlobContainerClient(_connectionString, _containerName);
        _containerClient.CreateIfNotExistsAsync().Wait();
    }

    public void OnGet()
    {
        var blobItems = _containerClient.GetBlobs();
        foreach (var blobItem in blobItems)
        {
            var url = _containerClient.GetBlobClient(blobItem.Name).Uri.ToString();
            ImageUrls.Add(url);
        }
    }

    public async Task<IActionResult> OnPost(List<IFormFile> photos)
    {
        foreach (var photo in photos)
        {
            var blobClient = _containerClient.GetBlobClient(photo.FileName);
            if (!blobClient.Exists())
            {
                using var stream = new MemoryStream();
                await photo.CopyToAsync(stream);
                stream.Position = 0;
                await blobClient.UploadAsync(stream);
            }
        }
        return RedirectToPage();
    }
}
