using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Uspevaemost_client.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;
        public string? Username { get; private set; }
        private readonly string _apiBaseUrl;
        public void OnGet()
        {
            Username = User.Identity?.Name; // ← тут будет DOMAIN\username
        }
        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        [BindProperty]
        public List<string>? Kurs { get; set; }
        [BindProperty]
        public List<string>? Urovni { get; set; }
        [BindProperty]
        public List<string>? FormyObucheniya { get; set; }
        [BindProperty]
        public List<string>? Goda { get; set; }
        [BindProperty]
        public List<string>? Semestry { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("OnPostAsync вызван");
            _logger.LogInformation("Kurs: {Kurs}", string.Join(", ", Kurs ?? new()));
            _logger.LogInformation("Urovni: {Urovni}", string.Join(", ", Urovni ?? new()));
            _logger.LogInformation("FormyObucheniya: {FormyObucheniya}", string.Join(", ", FormyObucheniya ?? new()));
            _logger.LogInformation("Goda: {Goda}", string.Join(", ", Goda ?? new()));
            _logger.LogInformation("Semestry: {Semestry}", string.Join(", ", Semestry ?? new()));

            System.Diagnostics.Debug.WriteLine(User.Identity?.Name);
            var request = new
            {
                Kurs,
                Urovni,
                FormyObucheniya,
                Goda,
                Semestry
            };

            var client = _httpClientFactory.CreateClient("WithWindowsAuth");


            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(request, options);
            _logger.LogInformation("Сформированный JSON: {Json}", json);
            _logger.LogInformation(User.Identity?.Name);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{_apiBaseUrl}/Excel/download", content);
            
                if (response.IsSuccessStatusCode)
                {
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                }
                else
                {
                    ErrorMessage = $"Ошибка: {response.StatusCode}";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке запроса");
                ErrorMessage = "Ошибка при подключении к API.";
                return Page();
            }
        }
    }
}
