
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
using System.Threading.Tasks;
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
            string? html = await ((NativeWebView)sender).InvokeScript("document.documentElement.outerHTML");

            if (html != null)
            {
                Site = html;

                (this.DataContext as MainViewModel).GetSite = await Task.Run(() => AnalysisText(Site)); //Сделал заглушкой для получения результата
            }
        }

        this.Opacity = 1;
        this.IsEnabled = true;
    }

    private List<Tag<object>> AnalysisText(string text)
    {
        lstParserLine.Clear();
        int positionTag = 0;
        text = JsonSerializer.Deserialize<string>(text);
        text = WebUtility.HtmlDecode(text);

        int endHtml = text.LastIndexOf("</html", StringComparison.Ordinal);

        while (positionTag != endHtml)
        {
            var openTag = text.IndexOf('<', positionTag);
            if (openTag == -1) break;

            var space = text.IndexOf(' ', openTag);

            if (space == -1) space = text.Length;

            positionTag = openTag;

            if (space - openTag == 1) //Ловить условия логических операций
            {
                positionTag = space + 1;
                continue;
            }

            string nameTag = text[(positionTag + 1)..space].Trim();

            var closeTag = nameTag != "!--"
                ? text.IndexOf('>', positionTag)
                : text.IndexOf("-->", positionTag, StringComparison.Ordinal);
            if (closeTag == -1) break; //тег не закрыт до конца документа

            if (space > closeTag)
            {
                space = closeTag;
                nameTag = text[(positionTag + 1)..space].Trim();
                closeTag = nameTag != "!--"
                    ? text.IndexOf('>', positionTag)
                    : text.IndexOf("-->", positionTag, StringComparison.Ordinal);
            }

            if (nameTag.Contains('/'))
            {
                positionTag = closeTag + 1;
                continue;
            }

            if (space == closeTag)
            {
                positionTag = closeTag + 1;
                continue;
            }

            lstParserLine.Add(new()
            {
                Name = nameTag,
            });

            // Позиция начала тега в тексте — для дерева вложенности и UI
            int newIndex = lstParserLine.Count - 1;
            lstParserLine[newIndex].Position = positionTag;

            int lastIndex = lstParserLine.Count - 1;

            //Проверяем комментарии
            if (nameTag != "!--")
            {
                var lstAtr = text[(positionTag + nameTag.Length + 2)..closeTag].Split(' ');

                //Жёсткий костыль но хочу проверить
                try
                {
                    for (var i = 0; i < lstAtr.Length; i++)
                    {
                        string word = lstAtr[i];

                        while (word.Contains('='))
                        {
                            var parts = word.Split('=');
                            if (parts.Length != 2) break;

                            word = string.Join("=", parts.Where(s => s.Length > 0));
                            if (word.Split('=')[1].Count(c => c == '\"') % 2 != 0)
                            {
                                i++;
                                word += " " + lstAtr[i];
                            }
                            else
                            {
                                break;
                            }
                        }

                        var lst = word.TrimEnd('/').Split('=');

                        lstParserLine[lastIndex].Atribute.Add
                        (
                            new Atribute()
                            {
                                Name = lst[0],
                                Value = lst[1]
                            }
                        );
                    }
                }
                catch
                {
                    if (IsSelfClosing(text, closeTag))
                    {
                        lstParserLine[newIndex].IsSelfClosing = true;
                        lstParserLine[newIndex].EndPosition = closeTag + 1;
                        positionTag = closeTag + 1;
                        continue;
                    }

                    lstParserLine.RemoveAt(lstParserLine.Count - 1);
                    positionTag = closeTag + 1;
                    continue;
                }

                if (IsSelfClosing(text, closeTag))
                {
                    lstParserLine[newIndex].IsSelfClosing = true;
                    lstParserLine[newIndex].EndPosition = closeTag + 1;
                    positionTag = closeTag + 1;
                    continue;
                }

                var closeNode = FindCloseTag(text, nameTag, positionTag);

                // конец элемента: либо после "</имя>", либо (если закрылся сам в себя) после '>'
                lstParserLine[newIndex].EndPosition = closeNode != -1
                    ? closeNode + nameTag.Length + 3
                    : closeTag + 1;

                if (closeNode != -1)
                {
                    string elm = text[(closeTag + 1)..closeNode];

                    lstParserLine[lastIndex].Value = elm;
                }

                positionTag = closeTag + 1;
            }
            else
            {
                string elm = text[positionTag..(closeTag + 3)];

                lstParserLine[lastIndex].Value = elm;
                lstParserLine[lastIndex].EndPosition = closeTag + 3; // "-->"

                positionTag = closeTag + 1;
            }
        }

        FillDepthByPositions(lstParserLine);
        return lstParserLine;
    }

    private void FillDepthByPositions(List<Tag<object>> tags)
    {
        var stack = new Stack<int>();

        for (int i = 0; i < tags.Count; i++)
        {
            while (stack.Count > 0 && tags[stack.Peek()].EndPosition < tags[i].Position)
                stack.Pop();

            tags[i].Depth = stack.Count;

            if (!tags[i].IsSelfClosing)   // ← самозакрывающиеся не оборачивают ничего
                stack.Push(i);
        }
    }

    private int FindCloseTag(string text, string nameTag, int positionOpenTag)
    {
        string closePrefix = "</" + nameTag;

        int depth = 1;                 // открывающий тег уже «открыт» — это мы его вызвали
        int pos = positionOpenTag + 1; // начинаем поиск ПОСЛЕ него

        while (true)
        {
            int lt = text.IndexOf('<', pos);
            if (lt == -1) return -1;

            var rest = text.AsSpan(lt);

            if (rest.StartsWith(closePrefix) && IsNameBoundary(text, lt + closePrefix.Length))
            {
                depth--;
                if (depth == 0) return lt;
                pos = lt + 2;
            }
            else if (rest[0] == '<' && !rest.StartsWith("</") &&
                     rest.Length > 1 + nameTag.Length &&
                     rest.Slice(1).StartsWith(nameTag) &&
                     IsNameBoundary(text, lt + 1 + nameTag.Length))
            {
                depth++;
                pos = lt + 1;
            }
            else
            {
                pos = lt + 1; // любой другой тег или '<' в тексте
            }
        }
    }

    private static bool IsSelfClosing(string text, int closeTag)
    {
        return closeTag > 0 && text[closeTag - 1] == '/';
    }

    private static bool IsNameBoundary(string text, int index)
    {
        if (index >= text.Length) return true;
        char c = text[index];
        return c == '>' || c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '/';
    }

    private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        (this.DataContext as MainViewModel).SelectedTag = ((ListBox)sender).SelectedItem as Tag<object>;
    }
}