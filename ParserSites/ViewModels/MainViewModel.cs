using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace ParserSites.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to Avalonia!";

    [ObservableProperty]
    public partial List<string> GetSite { get; set; } = new List<string> ();
}
