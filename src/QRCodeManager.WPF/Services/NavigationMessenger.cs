namespace QRCodeManager.WPF.Services;

public interface INavigationMessenger
{
    event Action<string, string?>? NavigateWithPayload;
    void OpenHistoryItem(string json, string? imagePath);
    void RegenerateFromHistory(string json);
}

public class NavigationMessenger : INavigationMessenger
{
    public event Action<string, string?>? NavigateWithPayload;

    public void OpenHistoryItem(string json, string? imagePath)
        => NavigateWithPayload?.Invoke("open", json);

    public void RegenerateFromHistory(string json)
        => NavigateWithPayload?.Invoke("regenerate", json);
}
