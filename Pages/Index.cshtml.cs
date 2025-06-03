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
            Username = User.Identity?.Name;
            Logger.Log($"{DateTime.Now.ToString("yyyy-mm-dd-h-m-s")} Подключен пользователь: {Username}");
        }
        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger, IOptions<ApiSettings> apiSettings)
        {

            _httpClientFactory = httpClientFactory;
            
            _apiBaseUrl = apiSettings.Value.BaseUrl;
            Logger.Log($"{DateTime.Now.ToString("yyyy-mm-dd-h-m-s")} Определен url сервера: {_apiBaseUrl}");
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


            string name = User.Identity?.Name;
            var request = new
            {
                Kurs,
                Urovni,
                FormyObucheniya,
                Goda,
                Semestry,
                name
            };

            var client = _httpClientFactory.CreateClient("WithWindowsAuth");


            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(request, options);
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Logger.Log($"{DateTime.Now.ToString("yyyy-mm-dd-h-m-s")} Сформировано тело запроса json: {json}");
            try
            {
                Logger.Log($"{DateTime.Now.ToString("yyyy-mm-dd-h-m-s")} Отпарвляю запрос к API: {_apiBaseUrl}/Excel/download");
                var response = await client.PostAsync($"{_apiBaseUrl}/Excel/download", content);
                
                if (response.IsSuccessStatusCode)
                {
                    Logger.Log($"{DateTime.Now.ToString("yyyy-mm-dd-h-m-s")} Получен ответ: {response.StatusCode}");
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                }
                else
                {
                    Logger.Log($"{DateTime.Now.ToString("yyyy-mm-dd-h-m-s")} Получен ответ: {response.StatusCode}");
                    ErrorMessage = $"{DateTime.Now.ToString("yyyy-mm-dd-h-m-s")} Ошибка: {response.StatusCode}";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"{DateTime.Now.ToString("yyyy-mm-dd-h-m-s")} Ошибка отправки: {ex}");
                ErrorMessage = "Ошибка при подключении к API.";
                return Page();
            }
        }
    }
}
