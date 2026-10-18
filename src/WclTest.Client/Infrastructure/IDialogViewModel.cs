namespace WclTest.Client.Infrastructure;

public interface IDialogViewModel
{
    string Title { get; }

    Action<bool>? CloseRequested { get; set; }
}
