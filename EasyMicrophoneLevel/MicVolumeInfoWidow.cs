using System.Windows;
using System.Windows.Controls;

public class MicVolumeInfoWidow : Window
{
    private bool _hidden = false;
    private CancellationTokenSource _cancelClosing = new CancellationTokenSource();

    public bool InfoHidden => _hidden;
    public MicVolumeInfoWidow()
    {
        WindowStyle = WindowStyle.None;
        WindowStartupLocation= WindowStartupLocation.CenterScreen;
        AllowsTransparency = true;
        Background = System.Windows.Media.Brushes.Transparent;
        SizeToContent = SizeToContent.WidthAndHeight;
        ShowInTaskbar = false;
        Topmost = true;
    }

    protected override void OnClosed(EventArgs e)
    {
        _hidden = true;
        base.OnClosed(e);
    }

    internal void ShowForSeconds(int seconds)
    {
        _cancelClosing.Cancel();
        _cancelClosing = new CancellationTokenSource();
        Show();
        Task.Run(() => WaitForSecondsAndClose(seconds, _cancelClosing.Token), _cancelClosing.Token);

    }

    private void WaitForSecondsAndClose(int seconds, CancellationToken token)
    {
        Thread.Sleep(TimeSpan.FromSeconds(seconds));
        if(token.IsCancellationRequested) return;
        Dispatcher.Invoke(Close);
    }

    internal void ShowVolumeInfo(string deviceName, int volume)
    {
        Content = new TextBlock() { Text = $"{deviceName}: {volume}", FontSize = 20, Foreground = System.Windows.Media.Brushes.Red, HorizontalAlignment = System.Windows.HorizontalAlignment.Center };
        ShowForSeconds(3);
    }
}
