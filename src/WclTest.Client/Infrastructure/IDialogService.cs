using WclTest.Client.ViewModels;

namespace WclTest.Client.Infrastructure;

public interface IDialogService
{
    Task<bool> ShowDialogAsync<TViewModel>() where TViewModel : ViewModelBase, IDialogViewModel;
}
