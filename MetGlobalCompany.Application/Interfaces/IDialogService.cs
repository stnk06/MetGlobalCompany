namespace MetGlobalCompany.Application.Interfaces;

public interface IDialogService
{

    bool? ShowDialog(object viewModel);

    void ShowMessage(string title, string message);
}