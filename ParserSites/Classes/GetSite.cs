using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ParserSites.Classes
{
    public class GetSite
    {
        public static async Task<string> Get(string siteAdress)
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10); // Установка таймаута для предотвращения зависания
            try
            {
                // Настройка заголовков, чтобы избежать блокировки сайтом
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                // Получаем HTML без использования Playwright и браузеров
                HttpResponseMessage response = await client.GetAsync(siteAdress);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException)
            {
                // Обработка таймаута
                return "Ошибка: превышено время ожидания ответа с сервера.";
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("504"))
            {
                // Обработка ошибки 504 (Gateway Timeout)
                return $"Ошибка: {ex.Message}. Возможно, сайт временно недоступен.";
            }
            catch (Exception ex)
            {
                // Обработка других ошибок
                return $"Ошибка при загрузке страницы: {ex.Message}";
            }
        }
    }
}
