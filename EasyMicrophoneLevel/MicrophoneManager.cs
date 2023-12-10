using mrousavy;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MicLevel
{
    internal class MicrophoneManager
    {
        public MMDevice Device { get; set; }

        public float Volume
        {
            get => Device.GetVolume();
            set => Device.SetVolume(value);
        }

        public MicrophoneManager(MMDevice device)
        {
            Device = device;
        }

        public static MMDevice[] GetCaptureDevices()
        {
            var enumerator = new MMDeviceEnumerator();
            
            return enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToArray();
        }

        public static MMDevice GetDefaultCaptureDevice()
        {
            var enumerator = new MMDeviceEnumerator();
            var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);

            if(defaultDevice is null)
            {
                return enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).First();
            }
            
            return defaultDevice;
        }

        public  static MMDevice GetCaptureDevice(string deviceName)
        {
            var enumerator = new MMDeviceEnumerator();
            return enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).First(x=>x.DeviceFriendlyName == deviceName);
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
