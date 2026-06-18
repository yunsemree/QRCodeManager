using System.Windows;
using QRCodeManager.WPF.ViewModels;

namespace QRCodeManager.WPF;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.HistoryViewModel.LoadHistoryAsync();
    }
}
