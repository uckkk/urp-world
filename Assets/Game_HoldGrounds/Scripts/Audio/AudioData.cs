using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Game_HoldGrounds.Scripts.Audio
{
    /// <summary>
    /// Stores data for audio.
    /// </summary>
    [Serializable]
    public class AudioData
    {
        public AudioMixer audioMixer;
        [Tooltip("Values based on Audio Mixer.")]
        [Range(-80.0f, 1.0f)] public float audioOverallVolume;
        [Tooltip("Values based on Audio Mixer.")]
        [Range(-80.0f, 1.0f)] public float audioMusicVolume;
        [Tooltip("Values based on Audio Mixer.")]
        [Range(-80.0f, 1.0f)] public float audioSfxVolume;
        
        /// <summary>
        /// Audio music name used to save as configuration.
        /// </summary>
        /// <returns></returns>
        public const string PlayerPrefAudioOverall = "masterVol";
        /// <summary>
        /// Audio music name used to save as configuration.
        /// </summary>
        /// <returns></returns>
        public const string PlayerPrefAudioMusic = "musicVol";
        /// <summary>
        /// Audio sound effects name used to save as configuration.
        /// </summary>
        /// <returns></returns>
        public const string PlayerPrefAudioSfx = "sfxVol";

        public AudioData()
        {
            audioOverallVolume = 0;
            audioMusicVolume = 0;
            audioSfxVolume = 0;
        }
    }
}