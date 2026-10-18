using CommunityToolkit.Mvvm.ComponentModel;
using WclTest.Client.ViewModels;

namespace WclTest.Client.Models;

public sealed partial class Section : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    public Section(string title, ViewModelBase content)
    {
        Title = title;
        Content = content;
    }

    public string Title { get; }

    public ViewModelBase Content { get; }
}
