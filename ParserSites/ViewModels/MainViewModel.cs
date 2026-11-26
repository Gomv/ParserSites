using CommunityToolkit.Mvvm.ComponentModel;
using ParserSites.Model;
using System.Collections.Generic;

namespace ParserSites.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to Avalonia!";

    [ObservableProperty]
    public partial List<Tag<object>> GetSite { get; set; } = new List<Tag<object>>();
}
