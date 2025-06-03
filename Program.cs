
namespace Uspevaemost_client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            Logger.Log($"{DateTime.Now.ToString("yyyy-mm-dd-h-m-s")} ������: "+ string.Join(",",args));
          
            builder.Services.Configure<ApiSettings>(
     builder.Configuration.GetSection("ApiSettings"));

            builder.Services.AddHttpClient("WithWindowsAuth")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            UseDefaultCredentials = true 
        });

            builder.Services.AddRazorPages();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
            Logger.Log($"{DateTime.Now.ToString("yyyy-mm-dd-h-m-s")} ������ �������");
        }
    }
}
