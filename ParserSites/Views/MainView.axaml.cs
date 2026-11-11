
using Avalonia;
using Avalonia.Controls;
using ParserSites.Classes;
using ParserSites.ViewModels;
using System.Collections.Generic;
using System.Linq;

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

        Site = GetSite.Get(text).Result;

        (this.DataContext as MainViewModel).GetSite = Site.Split("\n").ToList();
    }
}