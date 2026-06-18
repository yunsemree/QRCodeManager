using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using QRCodeManager.Application.DTOs;
using QRCodeManager.Application.Interfaces;
using QRCodeManager.WPF.Services;

namespace QRCodeManager.WPF.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly IHistoryRepository _historyRepository;
    private readonly INavigationMessenger _navigationMessenger;
    private readonly ILogger<HistoryViewModel> _logger;

    public ObservableCollection<QrHistoryDto> HistoryItems { get; } = new();

    [ObservableProperty]
    private QrHistoryDto? _selectedItem;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public HistoryViewModel(
        IHistoryRepository historyRepository,
        INavigationMessenger navigationMessenger,
        ILogger<HistoryViewModel> logger)
    {
        _historyRepository = historyRepository;
        _navigationMessenger = navigationMessenger;
        _logger = logger;
    }

    public async Task LoadHistoryAsync()
    {
        try
        {
            var items = await _historyRepository.GetAllAsync();
            HistoryItems.Clear();
            foreach (var item in items)
            {
                HistoryItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Geçmiş yüklenemedi");
            StatusMessage = "Geçmiş kayıtları yüklenemedi.";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync() => await LoadHistoryAsync();

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void OpenSelected()
    {
        if (SelectedItem is null)
        {
            return;
        }

        _navigationMessenger.OpenHistoryItem(SelectedItem.JsonData, SelectedItem.QrImagePath);
        StatusMessage = "Kayıt oluşturma sayfasında açıldı.";
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void RegenerateSelected()
    {
        if (SelectedItem is null)
        {
            return;
        }

        _navigationMessenger.RegenerateFromHistory(SelectedItem.JsonData);
        StatusMessage = "Kayıt yeniden oluşturma için yüklendi.";
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedItem is null)
        {
            return;
        }

        var result = MessageBox.Show(
            $"'{SelectedItem.Title}' kaydını silmek istiyor musunuz?",
            "Silme Onayı",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _historyRepository.DeleteAsync(SelectedItem.Id);
            await LoadHistoryAsync();
            StatusMessage = "Kayıt silindi.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıt silinemedi");
            StatusMessage = "Kayıt silinemedi.";
        }
    }

    private bool HasSelection() => SelectedItem is not null;

    partial void OnSelectedItemChanged(QrHistoryDto? value)
    {
        OpenSelectedCommand.NotifyCanExecuteChanged();
        RegenerateSelectedCommand.NotifyCanExecuteChanged();
        DeleteSelectedCommand.NotifyCanExecuteChanged();
    }
}
