using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WclTest.Client.Models;

namespace WclTest.Client.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private Section? _currentSection;

    public MainViewModel(AuthViewModel auth, UserViewModel user, AboutViewModel about)
    {
        Auth = auth;
        Sections = new Section[]
        {
            new("Профиль", user),
            new("Разработчик", about),
        };
        Sections[0].IsSelected = true;
        _currentSection = Sections[0];
    }

    public AuthViewModel Auth { get; }

    public IReadOnlyList<Section> Sections { get; }

    [RelayCommand]
    private void SelectSection(Section? section)
    {
        if (section is null || ReferenceEquals(section, CurrentSection))
            return;

        if (CurrentSection is not null)
            CurrentSection.IsSelected = false;

        section.IsSelected = true;
        CurrentSection = section;
    }
}
