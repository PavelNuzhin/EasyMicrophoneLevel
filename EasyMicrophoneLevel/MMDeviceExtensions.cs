using NAudio.CoreAudioApi;

namespace MicLevel
{
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
