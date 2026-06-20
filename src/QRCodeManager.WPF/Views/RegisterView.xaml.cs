using System.Windows;
using System.Windows.Controls;

namespace QRCodeManager.WPF.Views;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => SyncPasswords();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.RegisterViewModel viewModel)
        {
            viewModel.Password = PasswordBox.Password;
        }
    }

    private void OnConfirmPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.RegisterViewModel viewModel)
        {
            viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
        }
    }

    private void SyncPasswords()
    {
        if (DataContext is not ViewModels.RegisterViewModel viewModel)
        {
            return;
        }

        PasswordBox.Password = viewModel.Password;
        ConfirmPasswordBox.Password = viewModel.ConfirmPassword;
    }
}
