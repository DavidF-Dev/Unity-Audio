// File: Music.cs
// Purpose: Create a music asset that can be played at any time.
// Created by: DavidFDev

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DavidFDev.Audio
{
    /// <summary>
    ///     Music that can be played at any time.
    /// </summary>
    [CreateAssetMenu(menuName = "DavidFDev/Audio/Music")]
    public sealed class Music : ScriptableObject
    {
        #region Static Methods

#if UNITY_EDITOR
        [MenuItem("Tools/DavidFDev/Audio/Stop All Music (Runtime)")]
#endif
        private static void StopAllMenuItem()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            AudioHelper.StopMusic();
        }

        #endregion

        #region Serialized Fields

        [Tooltip("Audio clip to play.")]
        [SerializeField]
        [CanBeNull]
        private AudioClip clip;

        [Space]
        [Tooltip("Group that the audio playback should output to.")]
        [SerializeField]
        [CanBeNull]
        private AudioMixerGroup output;

        [Space]
        [Tooltip("Volume of the audio playback [0.0 - 1.0].")]
        [SerializeField]
        [Range(0f, 1f)]
        private float volume = 1f;

        [Tooltip("Pitch of the audio playback [-3.0 - 3.0].")]
        [SerializeField]
        [Range(-3f, 3f)]
        private float pitch = 1f;

        [Space]
        [Tooltip("Priority of the audio playback [0 (highest) - 256 (lowest)].")]
        [SerializeField]
        [Range(0, 256)]
        private int priority = 128;

        [Tooltip("Pan the location of a stereo or mono audio playback [-1.0 (left) - 1.0 (right)].")]
        [SerializeField]
        [Range(-1f, 1f)]
        private float stereoPan;

        #endregion

        #region Constructors

        private Music()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Audio clip to play.
        /// </summary>
        [PublicAPI]
        [CanBeNull]
        public AudioClip Clip
        {
            get => clip;
            set => clip = value;
        }

        /// <summary>
        ///     Group that the audio playback should output to.
        /// </summary>
        [PublicAPI]
        [CanBeNull]
        public AudioMixerGroup Output
        {
            get => output;
            set => output = value;
        }

        /// <summary>
        ///     Volume of the audio playback [0.0 - 1.0].
        /// </summary>
        [PublicAPI]
        public float Volume
        {
            get => volume;
            set => volume = Mathf.Clamp01(value);
        }

        /// <summary>
        ///     Pitch of the audio playback [-3.0 - 3.0].
        /// </summary>
        [PublicAPI]
        public float Pitch
        {
            get => pitch;
            set => pitch = Mathf.Clamp(value, -3f, 3f);
        }

        /// <summary>
        ///     Priority of the audio playback [0 (highest) - 256 (lowest)].
        /// </summary>
        [PublicAPI]
        public int Priority
        {
            get => priority;
            set => priority = Mathf.Clamp(value, 0, 256);
        }

        /// <summary>
        ///     Pan the location of a stereo or mono audio playback [-1.0 (left) - 1.0 (right)].
        /// </summary>
        [PublicAPI]
        public float StereoPan
        {
            get => stereoPan;
            set => stereoPan = Mathf.Clamp(value, -1f, 1f);
        }

        #endregion

        #region Unity Methods

        private void Reset()
        {
            Clip = null;
            Output = null;
            Volume = 1f;
            Pitch = 1f;
            Priority = 128;
            StereoPan = 0f;
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || !AudioHelper.IsMusicAssetPlaying(this))
            {
                return;
            }

            // Allow *LIVE* control of the music in the editor
            AudioHelper.MusicPlayback.Output = output;
            AudioHelper.MusicPlayback.Volume = volume;
            AudioHelper.MusicPlayback.Pitch = pitch;
            AudioHelper.MusicPlayback.Priority = priority;
            AudioHelper.MusicPlayback.StereoPan = stereoPan;
#endif
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Play the music track. If a track is already playing, it will be faded out.
        /// </summary>
        /// <param name="fadeIn">Duration, in seconds, that the music should take to fade in.</param>
        /// <param name="fadeOut">Duration, in seconds, that the old music should take to fade out.</param>
        [PublicAPI]
        public void Play(float fadeIn = 1f, float fadeOut = 0.75f)
        {
            AudioHelper.PlayMusicAsset(this, fadeIn, fadeOut);
        }

        [ContextMenu("Play Music (Runtime)")]
        private void PlayContextMenu()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Play();
        }

        [ContextMenu("Stop Music (Runtime)")]
        private void StopContextMenu()
        {
            if (!Application.isPlaying || !AudioHelper.IsMusicAssetPlaying(this))
            {
                return;
            }

            AudioHelper.StopMusic();
        }

        [ContextMenu("Stop All Music (Runtime)")]
        private void StopAllContextMenu()
        {
            StopAllMenuItem();
        }

        #endregion
    }
}