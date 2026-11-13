using Microsoft.Playwright;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace ParserSites.Classes
{
    public class GetSite
    {
        public static async Task<string> Get(string siteAdress)
        {
            var webView = new NativeWebView();

            string html = "";

            webView.NavigationCompleted += async (sender, args) =>
            {
            };

            // 3. Загружаем страницу
            webView.Source = new Uri(siteAdress);

            return html;
        }
    }
}
