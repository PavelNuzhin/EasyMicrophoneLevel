using mrousavy;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MicLevel
{

    internal class MicrophoneManager
    {
        private MMDevice? _device = null;
        private string _prefferedDeviceName;
        private DeviceWatcher _deviceWatcher;
        private MMDeviceEnumerator _deviceEnumerator;
        private string? _prefferedDeviceId = null;
        private string _currentDeviceId;

        public string DeviceName => Device.DeviceFriendlyName;
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

        // Hack to solve problems if device is set from another thread
        private void CheckValidDevice()
        {
            try
            {
                var deviceId = _device!.ID;
                return;
            }
            catch (System.InvalidCastException)
            {
            }

            try
            {
                var device = _deviceEnumerator.GetDevice(_currentDeviceId);
                SetDeviceInternal(device);
            }
            catch(COMException ex)
            {
                SetDefaultDevice();
            }
        }

        private void SetDefaultDevice()
        {
            var device = GetDefaultCaptureDevice();
            SetDeviceInternal(device);
        }

        public void SetDevice(string deviceName)
        {
            var device = GetCaptureDevice(deviceName);
            SetDeviceInternal(device, true);
        }

        private void SetDeviceInternal(string deviceName, bool byUser = false)
        {
            var device = GetCaptureDevice(deviceName);
            SetDeviceInternal(device, byUser);
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

        public float Volume
        {
            get => Device.GetVolume();
            set => Device.SetVolume(value);
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
                if(state != DeviceState.Active)
                {
                    SetDefaultDevice();
                }
            }
        }

        private bool IsPrefferedDevice(string id) => _prefferedDeviceId is null || _prefferedDeviceId == id;

        ~MicrophoneManager()
        {
            _deviceEnumerator.UnregisterEndpointNotificationCallback(_deviceWatcher);
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

        public MMDevice GetDefaultCaptureDevice()
        {
            var enumerator = _deviceEnumerator;
            var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);

            if (defaultDevice is null)
            {
                return enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).First();
            }

            return defaultDevice;
        }

        private MMDevice GetCaptureDevice(string deviceName)
        {
            return _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).FirstOrDefault(x => x.DeviceFriendlyName == deviceName);
        }
    }

    public static class MMDeviceExtensions
    {
        public static void SetVolume(this MMDevice device, float volume)
        {
            if (volume < 0)
            {
                volume = 0;
            }
            if (volume > 1)
            {
                volume = 1;
            }
            device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
        }

        public static float GetVolume(this MMDevice device)
        {
            return device.AudioEndpointVolume.MasterVolumeLevelScalar;
        }
    }
}
