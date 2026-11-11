using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ParserSites.Classes
{
    public class GetSite
    {
        public static async Task<string> Get(string siteAdress)
        {
            string content = string.Empty;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                try
                {
                    content = await client.GetStringAsync(siteAdress);
                }
                catch
                {
                    return "Ошибка чтения сайта";
                }
            }

            return content;
        }
    }
}
