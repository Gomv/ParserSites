
using Avalonia;
using Avalonia.Controls;
using ParserSites.Classes;
using ParserSites.Model;
using ParserSites.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace ParserSites.Views;

public partial class MainView : UserControl
{
    private string Site { get; set; } = "";

    private List<Tag<object>> lstParserLine { get; set; } = new List<Tag<object>>();

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
        int positionTag = 0;
        int finalTag = text.LastIndexOf("</html");
        text = JsonSerializer.Deserialize<string>(text);

        while (positionTag != finalTag)
        {
            var openTag = text.IndexOf("<", positionTag);
            var space = text.IndexOf(" ", openTag);

            positionTag = openTag;

            if (space - openTag == 1) //Ловить условия логических операций
            {
                continue;
            }

            string nameTag = text[(positionTag + 1)..space];

            var closeTag = text.IndexOf(">", positionTag);

            lstParserLine.Add(new()
                {
                    Name = nameTag,
                }
            );

            var lstAtr = text[(positionTag + nameTag.Count() + 2)..closeTag].Split(" ");

            for (var i = 0; i < lstAtr.Count(); i++)
            {
                string word = lstAtr[i];

                while (word.Contains("=") && word.Split('=').Count() == 2)
                {
                    if (word.Contains("="))
                    {
                        if (word.Split("=").Count() >= 2)
                        {
                            word = string.Join("=", word.Split("=").Where(s => !string.IsNullOrEmpty(s)));
                            if (word.Split("=")[1].Count('\"') != 2)
                            {
                                i++;
                                word += " " + lstAtr[i];
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            i++;
                            word += lstAtr[i];
                        }
                    }
                    else
                    {
                        i++;
                        word += lstAtr[i];
                    }
                }

                var lst = word.TrimEnd('/').Split("=");

                lstParserLine.Last().Atribute.Add
                (
                    new Atribute()
                    {
                        Name = lst[0],
                        Value = lst[1]
                    }
                );
            }

            if (text.IndexOf("/>", positionTag) - closeTag == 1)
            {
                positionTag = closeTag + 1;
                continue;
            }

            var closeNode = text.IndexOf($"</{nameTag}", positionTag);

            string elm = text[(closeTag + 1)..closeNode];

            lstParserLine.Last().Value = elm;

            positionTag = closeTag + 1;
        }

        return new();
    }

}