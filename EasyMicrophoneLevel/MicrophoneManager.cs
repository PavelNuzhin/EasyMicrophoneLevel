using NAudio.CoreAudioApi;
using System.Runtime.InteropServices;

namespace MicLevel
{

    internal class MicrophoneManager
    {
        public string DeviceName => Device.DeviceFriendlyName;
        public float Volume
        {
            get => Device.GetVolume();
            set => Device.SetVolume(value);
        }

        private MMDevice? _device = null;
        private string _prefferedDeviceName;
        private DeviceWatcher _deviceWatcher;
        private MMDeviceEnumerator _deviceEnumerator;
        private string? _prefferedDeviceId = null;
        private string _currentDeviceId;

        private MMDevice Device
        {
            get
            {
                CheckValidDevice();

                if (_device!.State != DeviceState.Active)
                {
                    SetDefaultDevice();
                }

                return _device;
            }
        }

        public MicrophoneManager(string deviceName)
        {
            _deviceEnumerator = new MMDeviceEnumerator();
            _prefferedDeviceName = deviceName;

            var device = GetCaptureDevice(deviceName);

            if (device is null)
            {
                SetDefaultDevice();
            }
            else
            {
                SetDeviceInternal(device, true);
            }

            _deviceWatcher = new DeviceWatcher();

            _deviceWatcher.DeviceStateChanged += OnDeviceStateChanged;

            _deviceEnumerator.RegisterEndpointNotificationCallback(_deviceWatcher);
        }

        ~MicrophoneManager()
        {
            _deviceEnumerator.UnregisterEndpointNotificationCallback(_deviceWatcher);
        }

        public void SetDevice(string deviceName)
        {
            var device = GetCaptureDevice(deviceName);
            SetDeviceInternal(device, true);
        }

        public static string[] GetCaptureDeviceNames()
        {
            var enumerator = new MMDeviceEnumerator();

            return enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).Select(x => x.DeviceFriendlyName).ToArray();
        }

        public static string GetDefaultCaptureDeviceName()
        {
            return new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications).DeviceFriendlyName;
        }

        private MMDevice GetDefaultCaptureDevice()
        {
            var enumerator = _deviceEnumerator;
            var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);

            if (defaultDevice is null)
            {
                return enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).First();
            }

            return defaultDevice;
        }

        // Dirty hack to fix problems if device is set from another thread
        private void CheckValidDevice()
        {
            try
            {
                var deviceId = _device!.ID;
                return;
            }
            catch (System.InvalidCastException)
            {
                SetCurrentDevice();
            }
        }

        private void SetCurrentDevice()
        {
            try
            {
                var device = _deviceEnumerator.GetDevice(_currentDeviceId);
                SetDeviceInternal(device);
            }
            catch (COMException)
            {
                SetDefaultDevice();
            }
        }

        private void SetDefaultDevice()
        {
            var device = GetDefaultCaptureDevice();
            SetDeviceInternal(device);
        }

        private void SetDeviceInternal(MMDevice device, bool byUser = false)
        {
            _device = device;

            if (byUser)
            {
                _prefferedDeviceName = device.DeviceFriendlyName;
                _prefferedDeviceId = device.ID;
            }

            _currentDeviceId = _device.ID;
        }

        private void OnDeviceStateChanged(string id, DeviceState state)
        {
            if (IsPrefferedDevice(id) && state == DeviceState.Active)
            {
                var device = GetCaptureDevice(_prefferedDeviceName);

                if (device is not null)
                {
                    SetDeviceInternal(device, true);
                    return;
                }
            }

            if (_currentDeviceId == id)
            {
                if (state != DeviceState.Active)
                {
                    SetDefaultDevice();
                }
            }
        }

        private bool IsPrefferedDevice(string id) => _prefferedDeviceId is null || _prefferedDeviceId == id;

        private MMDevice GetCaptureDevice(string deviceName)
        {
            return _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).FirstOrDefault(x => x.DeviceFriendlyName == deviceName);
        }
    }
}
