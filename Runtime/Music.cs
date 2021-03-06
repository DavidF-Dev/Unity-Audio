// File: Music.cs
// Purpose: Create a music asset that can be played at any time.
// Created by: DavidFDev

using System;
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
        [MenuItem("Tools/DavidFDev/Audio/Stop Music (Runtime)")]
#endif
        private static void StopMenuItem()
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

        [Space]
        [Tooltip("Allows audio to play even though AudioListener.pause is set to true.")]
        [SerializeField]
        private bool ignoreListenerPause;

        [Tooltip("Whether to take into account the volume of the audio listener.")]
        [SerializeField]
        private bool ignoreListenerVolume;

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

        /// <summary>
        ///     Allows audio to play even though AudioListener.pause is set to true.
        /// </summary>
        [PublicAPI]
        public bool IgnoreListenerPause
        {
            get => ignoreListenerPause;
            set => ignoreListenerPause = value;
        }

        /// <summary>
        ///     Whether to take into account the volume of the audio listener.
        /// </summary>
        [PublicAPI]
        public bool IgnoreListenerVolume
        {
            get => ignoreListenerVolume;
            set => ignoreListenerVolume = value;
        }

        #endregion

        #region Events

        [PublicAPI]
        public event Action Played;

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
            AudioHelper.MusicPlayback.IgnoreListenerPause = ignoreListenerPause;
            AudioHelper.MusicPlayback.IgnoreListenerVolume = ignoreListenerVolume;
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

        internal void InvokePlayed()
        {
            try
            {
                Played?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
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

        [ContextMenu("Play Music as Sound Effect (Runtime)")]
        private void PlayAsSfxContextMenu()
        {
            if (!Application.isPlaying || clip == null)
            {
                return;
            }

            Vector3 pos;
            var listener = FindObjectOfType<AudioListener>();
            if (listener != null)
            {
                pos = listener.transform.position;
            }
            else
            {
                Camera cam;
                if ((cam = Camera.main) == null)
                {
                    pos = default;
                }
                else
                {
                    pos = cam.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
                    pos.z = 0f;
                }
            }

            var pb = AudioHelper.Play(clip, pos, output);
            if (pb == null)
            {
                return;
            }

            pb.Volume = volume;
            pb.Pitch = pitch;
            pb.Priority = priority;
            pb.StereoPan = stereoPan;
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

        [ContextMenu("Stop Any Music (Runtime)")]
        private void StopAnyContextMenu()
        {
            StopMenuItem();
        }

        #endregion
    }
}