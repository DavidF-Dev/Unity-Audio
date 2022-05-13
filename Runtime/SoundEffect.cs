// File: SoundEffect.cs
// Purpose: Create a sound effect that can be played at any time.
// Created by: DavidFDev

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace DavidFDev.Audio
{
    /// <summary>
    ///     Sound effect that can be played at any time.
    /// </summary>
    [CreateAssetMenu(menuName = "DavidFDev/Audio/Sound Effect")]
    public sealed class SoundEffect : ScriptableObject
    {
        #region Serialized Fields

        [Tooltip("A random clip is chosen when the sound effect is played.")]
        [SerializeField]
        [NotNull]
        private AudioClip[] clips = Array.Empty<AudioClip>();

        [Tooltip("Prevents the same clip from being played twice in a row, if there is more than one.")]
        [SerializeField]
        private bool smartRandom = true;

        [Space]
        [Tooltip("Group that the audio playback should output to.")]
        [SerializeField]
        [CanBeNull]
        private AudioMixerGroup output;

        [Header("Volume")]
        [Tooltip("Minimum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float minVolume = 1f;

        [Tooltip("Maximum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float maxVolume = 1f;

        [Header("Pitch")]
        [Tooltip("Minimum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.")]
        [SerializeField]
        [Range(-3f, 3f)]
        private float minPitch = 1f;

        [Tooltip("Maximum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.")]
        [SerializeField]
        [Range(-3f, 3f)]
        private float maxPitch = 1f;

        [Space]
        [Tooltip("Whether the audio playback should loop.\nIf looping, the playback must be stopped manually.")]
        [SerializeField]
        private bool loop;

        [Tooltip("Priority of the audio playback [0 (highest) - 256 (lowest)].")]
        [SerializeField]
        [Range(0, 256)]
        private int priority = 128;

        [Tooltip("Pan the location of a stereo or mono audio playback [-1.0 (left) - 1.0 (right)].")]
        [SerializeField]
        [Range(0f, 1f)]
        private float stereoPan;

        [Tooltip("Amount that the audio playback is affected by spatialisation calculations [0.0 (2D) - 1.0 (3D)].")]
        [SerializeField]
        [Range(0f, 1f)]
        private float spatialBlend;

        [Tooltip("Settings used to calculate 3D spatialisation.")]
        [SerializeField]
        [CanBeNull]
        private SpatialAudioSettings spatialSettings;

        [Space]
        [Tooltip("Allows audio to play even though AudioListener.pause is set to true.")]
        [SerializeField]
        private bool ignoreListenerPause;

        [Tooltip("Whether to take into account the volume of the audio listener.")]
        [SerializeField]
        private bool ignoreListenerVolume;

        #endregion

        #region Fields

        [NonSerialized]
        private int _previousClipIndex = -1;

        #endregion

        #region Constructors

        private SoundEffect()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     A random clip is chosen when the sound effect is played.
        /// </summary>
        [PublicAPI]
        [NotNull]
        public AudioClip[] Clips
        {
            get => clips;
            set => clips = value;
        }

        /// <summary>
        ///     Prevents the same clip from being played twice in a row, if there is more than one.
        /// </summary>
        [PublicAPI]
        public bool SmartRandom
        {
            get => smartRandom;
            set => smartRandom = value;
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
        ///     Minimum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.
        /// </summary>
        [PublicAPI]
        public float MinVolume
        {
            get => minVolume;
            set => minVolume = Mathf.Clamp01(value);
        }

        /// <summary>
        ///     Maximum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.
        /// </summary>
        [PublicAPI]
        public float MaxVolume
        {
            get => maxVolume;
            set => maxVolume = Mathf.Clamp01(value);
        }

        /// <summary>
        ///     Minimum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.
        /// </summary>
        [PublicAPI]
        public float MinPitch
        {
            get => minPitch;
            set => minPitch = Mathf.Clamp(value, -3f, 3f);
        }

        /// <summary>
        ///     Maximum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.
        /// </summary>
        [PublicAPI]
        public float MaxPitch
        {
            get => maxPitch;
            set => maxPitch = Mathf.Clamp(value, -3f, 3f);
        }

        /// <summary>
        ///     Whether the audio playback should loop.<br />
        ///     If looping, the playback must be stopped manually.
        /// </summary>
        [PublicAPI]
        public bool Loop
        {
            get => loop;
            set => loop = value;
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
        ///     Amount that the audio playback is affected by spatialisation calculations [0.0 (2D) - 1.0 (3D)].
        /// </summary>
        [PublicAPI]
        public float SpatialBlend
        {
            get => spatialBlend;
            set => spatialBlend = Mathf.Clamp01(value);
        }

        /// <summary>
        ///     Settings used to calculate 3D spatialisation.
        /// </summary>
        [PublicAPI] [CanBeNull]
        public SpatialAudioSettings SpatialSettings
        {
            get => spatialSettings;
            set => spatialSettings = value;
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

        public event Action Played;

        #endregion

        #region Unity Methods

        private void Reset()
        {
            clips = Array.Empty<AudioClip>();
            smartRandom = true;
            minVolume = 1f;
            maxVolume = 1f;
            minPitch = 1f;
            maxPitch = 1f;
            loop = false;
            priority = 128;
            stereoPan = 0f;
            spatialBlend = 0f;
            spatialSettings = null;
            ignoreListenerPause = false;
            ignoreListenerVolume = false;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Play the sound effect.
        /// </summary>
        /// <param name="position">Position of the audio playback in 3D world-space.</param>
        /// <returns>Instance for controlling audio playback.</returns>
        [PublicAPI]
        [CanBeNull]
        public Playback Play(Vector3 position = default)
        {
            return AudioHelper.PlaySfx(this, position);
        }

        [CanBeNull]
        internal AudioClip GetClipAtRandom()
        {
            if (!clips.Any())
            {
                return null;
            }

            if (clips.Length == 1)
            {
                return clips.First();
            }

            // If not smart random, return a truly random clip
            if (!smartRandom)
            {
                return clips[Random.Range(0, clips.Length)];
            }

            // Determine choices for the random number generator (don't include the previously chosen clip index)
            var choices = new List<int>(clips.Length);
            for (var i = 0; i < clips.Length; i += 1)
            {
                if (i == _previousClipIndex)
                {
                    continue;
                }

                choices.Add(i);
            }

            // Get a random clip
            var clipIndex = choices[Random.Range(0, choices.Count)];
            _previousClipIndex = clipIndex;

            return clips[clipIndex];
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

        [ContextMenu("Play Sound Effect @ Listener (Runtime)")]
        private void PlayListenerContextMenu()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var listener = FindObjectOfType<AudioListener>();
            if (listener == null)
            {
                return;
            }

            Play(listener.transform.position);
        }

        [ContextMenu("Play Sound Effect @ 0,0,0 (Runtime)")]
        private void PlayDefaultContextMenu()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Play();
        }

        [ContextMenu("Play Sound Effect @ Screen Centre (Runtime)")]
        private void PlayCentreContextMenu()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Camera cam;
            if ((cam = Camera.main) == null)
            {
                return;
            }

            var pos = cam.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
            pos.z = 0f;

            Play(pos);
        }

        #endregion
    }
}