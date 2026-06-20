using System.Windows;
using System.Windows.Controls;

namespace QRCodeManager.WPF.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => SyncPassword();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.LoginViewModel viewModel)
        {
            viewModel.Password = PasswordBox.Password;
        }
    }

    private void SyncPassword()
    {
        if (DataContext is ViewModels.LoginViewModel viewModel)
        {
            PasswordBox.Password = viewModel.Password;
        }
    }
}
