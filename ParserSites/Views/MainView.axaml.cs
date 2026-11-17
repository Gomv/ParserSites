
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
        bool isAtribute = false;
        int generation = 0;

        string finalTag = "</html"; //что бы не случилось если это финальный тэг цикл вырубится
        string tag = "";

        text = JsonSerializer.Deserialize<string>(text);
        var arrWords = text.Split(" ");

        lstParserLine.Clear();

        ///Наполняем словарь

        for (int i = 0; i < arrWords.Length; i++)
        {
            string word = arrWords[i];

            if (word.Contains("<") && !word.Contains("</"))
            {
                tag = word.Trim('<');

                if (tag.Count() == 0)
                {
                    i++;
                    tag = arrWords[i];
                }

                lstParserLine.Add(new()
                    {
                        Name = tag,
                        Position = i
                    }
                );

                isAtribute = true;
                continue;
            }

            if (lstParserLine.Count() != 0)
            {
                if (word.Contains(">"))
                {
                    isAtribute = false;
                }

                if (isAtribute)
                {
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
                                    word += " " + arrWords[i];
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                i++;
                                word += arrWords[i];
                            }
                        }
                        else
                        {
                            i++;
                            word += arrWords[i];
                        }
                    }

                    /*Атрибут*/

                    var lstAtr = word.Split("=");

                    lstParserLine.Last().Atribute.Add
                    (
                        new Atribute()
                        {
                            Name = lstAtr[0],
                            Value = lstAtr[1]
                        }
                    );
                }
                else
                {
                    /*ТекстВнутри*/
                    lstParserLine.Last().Value += word;
                }
            }
        }

        ///


        return new();
    }
}