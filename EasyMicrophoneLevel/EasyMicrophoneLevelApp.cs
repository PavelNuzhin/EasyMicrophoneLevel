using MicLevel;
using NAudio.CoreAudioApi;
using System.IO;
using System.Reflection;
using WindowsHookEx;

public class EasyMicrophoneLevelApp
{
    private IKeyboardMouseEvents? _globalHook = default;
    private bool _altIsDown;
    private readonly MicrophoneManager _micManager;
    private MicVolumeInfoWidow _infoWindow = new();
    private NotifyIcon? _notifyIcon = default;
    private ToolStripDropDown _devicesList = new();
    private const string _captureDeviceParameterName = "CaptureDevice";

    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            var app = new EasyMicrophoneLevelApp();
            Application.Run();
        }
        catch (Exception ex)
        {
            var appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var errorFile = Path.Combine(appDirectory, "Error.txt");
            File.WriteAllText(errorFile, ex.ToString());
        }
    }

    public EasyMicrophoneLevelApp()
    {
        var device = GetCapturingDeviceFromConfig();
        _micManager = new MicrophoneManager(device);

        SetNotifyIcon();
        SubscribeToHotKeys();
    }

    private MMDevice GetCapturingDeviceFromConfig()
    {
        if (!AppConfiguration.Contains(_captureDeviceParameterName))
        {
            return MicrophoneManager.GetDefaultCaptureDevice();
        }

        var deviceName = AppConfiguration.Get(_captureDeviceParameterName);

        try
        {
            return MicrophoneManager.GetCaptureDevice(deviceName);
        }
        catch
        {
            return MicrophoneManager.GetDefaultCaptureDevice();
        }
    }

    private void SetNotifyIcon()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = EasyMicrophoneLevel.Properties.Resources.Icon,
            Visible = true,
            ContextMenuStrip = new ContextMenuStrip()
        };

        _devicesList = new ToolStripDropDown();
        var dropDownButton = new ToolStripMenuItem()
        {
            Text = "Devices",
            DisplayStyle = ToolStripItemDisplayStyle.Text,
            DropDown = _devicesList,
            AllowDrop = true
        };

        _notifyIcon.ContextMenuStrip.Items.Add(dropDownButton);
        _notifyIcon.ContextMenuStrip.Items.Add("Close", null, (sender, args) =>
        {
            Close();
        });


        _notifyIcon.ContextMenuStrip.Opening += (sender, args) =>
        {
            FillDeviceList();
        };
    }

    private void FillDeviceList()
    {
        _devicesList.Items.Clear();
        foreach (var device in MicrophoneManager.GetCaptureDevices())
        {
            _devicesList.Items.Add(
                device.DeviceFriendlyName,
                null,
                (sender, args) =>
                {
                    _micManager.Device = device;
                    SetCaptureDevice(device.DeviceFriendlyName);
                });
        }

        SetCaptureDevice(_micManager.Device.DeviceFriendlyName);
    }

    private void SetCaptureDevice(string deviceName)
    {
        if(_captureDeviceParameterName != deviceName)
        {
            AppConfiguration.Set(_captureDeviceParameterName, deviceName);
        }
        
        foreach (ToolStripItem item in _devicesList.Items)
        {
            if(item.Text == deviceName)
            {
                item.ForeColor = item.Text == deviceName ? Color.Red : Color.Black;
            }
        }
    }

    private void ShowDeviceInfo()
    {
        if (_infoWindow.InfoHidden)
        {
            _infoWindow = new MicVolumeInfoWidow();
        }
        _infoWindow.ShowVolumeInfo(_micManager.Device.DeviceFriendlyName, (int)(_micManager.Volume * 100));
    }

    protected void Close()
    {
        _notifyIcon?.Dispose();
        Unsubscribe();

        Application.Exit();
    }

    public void SubscribeToHotKeys()
    {
        _globalHook = Hook.GlobalEvents();

        _globalHook.MouseWheelExt += GlobalHookMouseWheelExt;
        _globalHook.KeyDown += GlobalHookKeyDown;
        _globalHook.KeyUp += GlobalHookKeyUp;
    }

    private void GlobalHookMouseWheelExt(object? sender, MouseEventExtArgs e)
    {
        if (!_altIsDown)
        {
            return;
        }

        if (!IsMouseOverTaskBar(e.Location))
        {
            return;
        }

        var currentVolume = _micManager.Volume;
        currentVolume = e.Delta > 0 ? currentVolume + .02f : currentVolume - .02f;

        _micManager.Volume = currentVolume;

        ShowDeviceInfo();
    }

    private bool IsMouseOverTaskBar(System.Drawing.Point location)
    {
        var rectangle = GetTaskBarsRectangle();
        return rectangle.Contains(location);
    }
    public Rectangle GetTaskBarsRectangle()
    {
        var handle = User32.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_Traywnd", "");
        var rectangle = User32.GetClientRect(handle);
        return rectangle;
    }

    private void GlobalHookKeyUp(object sender, WindowsHookEx.KeyEventArgs e)
    {
        if (IsAlt(e.KeyCode))
        {
            return;
        }

        _altIsDown = false;
    }

    private void GlobalHookKeyDown(object sender, WindowsHookEx.KeyEventArgs e)
    {
        if (IsAlt(e.KeyCode))
        {
            return;
        }

        _altIsDown = true;
    }

    private bool IsAlt(WindowsHookEx.Keys e)
    {
        return e != WindowsHookEx.Keys.Alt && e != WindowsHookEx.Keys.LMenu;
    }

    public void Unsubscribe()
    {
        _globalHook!.MouseWheelExt -= GlobalHookMouseWheelExt;
        _globalHook!.KeyDown -= GlobalHookKeyDown;
        _globalHook!.KeyUp -= GlobalHookKeyUp;

        _globalHook!.Dispose();
    }
}
