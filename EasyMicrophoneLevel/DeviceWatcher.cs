using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace MicLevel
{
    class DeviceWatcher : IMMNotificationClient
    {
        public Action<string, DeviceState>? DeviceStateChanged;
        
        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            DeviceStateChanged?.Invoke(deviceId, newState);
        }

        public void OnDeviceAdded(string deviceId) {}

        public void OnDeviceRemoved(string deviceId) {}

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) { }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
    }
}
