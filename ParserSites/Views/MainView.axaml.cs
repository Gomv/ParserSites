
using Avalonia;
using Avalonia.Controls;
using ParserSites.Classes;
using ParserSites.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace ParserSites.Views;

public partial class MainView : UserControl
{
    private string Site { get; set; } = "";

    public MainView()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string text = this.FindControl<TextBox>("SiteAddress").Text;

        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        this.FindControl<NativeWebView>("ParsersSites").Source = new Uri(text);

        this.Opacity = .25;
        this.IsEnabled = false;
    }

    private void SiteAddress_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter)
        {
            Button_Click(sender, e);
        }
    }

    private async void ParsersSites_NavigationCompleted(object? sender, WebViewNavigationCompletedEventArgs e)
    {
        if (e.IsSuccess)
        {
            string? html = await((NativeWebView)sender).InvokeScript("document.documentElement.outerHTML");

            if (html != null)
            {
                Site = html;
                (this.DataContext as MainViewModel).GetSite = AnalysisText(Site);
            }
        }

        this.Opacity = 1;
        this.IsEnabled = true;
    }

    private List<string> AnalysisText(string text)
    {
        // 1. Удаляем содержимое <script> и <style>
        string cleaned = Regex.Replace(text, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @"<style[^>]*>[\s\S]*?</style>", "", RegexOptions.IgnoreCase);

        // 2. Заменяем блочные теги на переносы строк (чтобы текст не слипался)
        cleaned = Regex.Replace(cleaned, @"<(br|p|div|h1|h2|h3|li|tr)[^>]*>", "\n", RegexOptions.IgnoreCase);

        // 3. Удаляем все оставшиеся HTML-теги
        cleaned = Regex.Replace(cleaned, @"<[^>]+>", "");

        // 4. Декодируем HTML-сущности (&nbsp;, &amp;, &quot; и т.д.)
        cleaned = WebUtility.HtmlDecode(cleaned);

        // 5. Разбиваем на строки, убираем пустые и лишние пробелы
        var lines = cleaned.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(line => line.Trim())
                           .Where(line => !string.IsNullOrWhiteSpace(line))
                           .ToList();

        return lines;
    }
}