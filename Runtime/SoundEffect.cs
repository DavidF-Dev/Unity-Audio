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
        #region Static Methods

        /// <summary>
        ///     Create a new sound effect instance via code.
        /// </summary>
        /// <param name="clips">A random clip is chosen when the sound effect is played.</param>
        /// <param name="smartRandom">Prevents the same clip from being played twice in a row.</param>
        /// <param name="output">Group that the audio playback should output to.</param>
        /// <param name="minVolume">Minimum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.</param>
        /// <param name="maxVolume">Maximum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.</param>
        /// <param name="minPitch">Minimum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.</param>
        /// <param name="maxPitch">Maximum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.</param>
        /// <param name="loop">Whether the audio playback should loop.</param>
        /// <param name="priority">Priority of the audio playback [0 - 256].</param>
        /// <param name="stereoPan">Pan the location of a stereo or mono audio playback [-1.0 (left) - 1.0 (right)].</param>
        /// <param name="spatialBlend">
        ///     Amount that the audio playback is affected by spatialisation calculations [0.0 (2D) - 1.0
        ///     (3D)].
        /// </param>
        [PublicAPI]
        [NotNull]
        public static SoundEffect Create([NotNull] AudioClip[] clips, bool smartRandom = true,
            [CanBeNull]
            AudioMixerGroup output = null,
            float minVolume = 1f, float maxVolume = 1f, float minPitch = 1f, float maxPitch = 1f,
            bool loop = false, int priority = 128, float stereoPan = 0f, float spatialBlend = 0f)
        {
            var instance = CreateInstance<SoundEffect>();
            instance.Clips = clips;
            instance.SmartRandom = smartRandom;
            instance.Output = output;
            instance.MinVolume = Mathf.Clamp01(minVolume);
            instance.MaxVolume = Mathf.Clamp01(maxVolume);
            instance.MinPitch = Mathf.Clamp(minPitch, -3f, 3f);
            instance.MaxPitch = Mathf.Clamp(maxPitch, -3f, 3f);
            instance.Loop = loop;
            instance.Priority = Mathf.Clamp(priority, 0, 256);
            instance.StereoPan = Mathf.Clamp(stereoPan, -1f, 1f);
            instance.SpatialBlend = Mathf.Clamp01(spatialBlend);
            return instance;
        }

        #endregion

        #region Fields

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
        [field: Tooltip("A random clip is chosen when the sound effect is played.")]
        [field: SerializeField]
        [PublicAPI]
        [NotNull]
        public AudioClip[] Clips { get; private set; } = Array.Empty<AudioClip>();

        /// <summary>
        ///     Prevents the same clip from being played twice in a row.
        /// </summary>
        [field: Tooltip("Prevents the same clip from being played twice in a row.")]
        [field: SerializeField]
        [PublicAPI]
        public bool SmartRandom { get; private set; }

        /// <summary>
        ///     Group that the audio playback should output to.
        /// </summary>
        [field: Space]
        [field: Tooltip("Group that the audio playback should output to.")]
        [field: SerializeField]
        [PublicAPI]
        [CanBeNull]
        public AudioMixerGroup Output { get; private set; }

        /// <summary>
        ///     Minimum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.
        /// </summary>
        [field: Tooltip("Minimum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.")]
        [field: Header("Volume")]
        [field: SerializeField]
        [field: Range(0f, 1f)]
        [PublicAPI]
        public float MinVolume { get; private set; }

        /// <summary>
        ///     Maximum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.
        /// </summary>
        [field: Tooltip("Maximum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.")]
        [field: SerializeField]
        [field: Range(0f, 1f)]
        [PublicAPI]
        public float MaxVolume { get; private set; }

        /// <summary>
        ///     Minimum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.
        /// </summary>
        [field: Tooltip("Minimum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.")]
        [field: Header("Pitch")]
        [field: SerializeField]
        [field: Range(-3f, 3f)]
        [PublicAPI]
        public float MinPitch { get; private set; }

        /// <summary>
        ///     Maximum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.
        /// </summary>
        [field: Tooltip("Maximum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.")]
        [field: SerializeField]
        [field: Range(-3f, 3f)]
        [PublicAPI]
        public float MaxPitch { get; private set; }

        /// <summary>
        ///     Whether the audio playback should loop.
        /// </summary>
        [field: Space]
        [field: Tooltip("Whether the audio playback should loop.")]
        [field: SerializeField]
        [PublicAPI]
        public bool Loop { get; private set; }

        /// <summary>
        ///     Priority of the audio playback [0 - 256].
        /// </summary>
        [field: Tooltip("Priority of the audio playback [0 - 256].")]
        [field: SerializeField]
        [field: Range(0, 256)]
        [PublicAPI]
        public int Priority { get; private set; }

        /// <summary>
        ///     Pan the location of a stereo or mono audio playback [-1.0 (left) - 1.0 (right)].
        /// </summary>
        [field: Tooltip("Pan the location of a stereo or mono audio playback [-1.0 (left) - 1.0 (right)].")]
        [field: SerializeField]
        [field: Range(-1f, 1f)]
        [PublicAPI]
        public float StereoPan { get; private set; }

        /// <summary>
        ///     Amount that the audio playback is affected by spatialisation calculations [0.0 (2D) - 1.0 (3D)].
        /// </summary>
        [field:
            Tooltip("Amount that the audio playback is affected by spatialisation calculations [0.0 (2D) - 1.0 (3D)].")]
        [field: SerializeField]
        [field: Range(0f, 1f)]
        [PublicAPI]
        public float SpatialBlend { get; private set; }

        /// <summary>
        ///     Settings used to calculate 3D spatialisation.
        /// </summary>
        [field: Tooltip("Settings used to calculate 3D spatialisation.")]
        [field: SerializeField]
        [PublicAPI] [CanBeNull]
        public SpatialAudioSettings SpatialSettings { get; private set; }

        /// <summary>
        ///     Allows audio to play even though AudioListener.pause is set to true.
        /// </summary>
        [field: Space]
        [field: Tooltip("Allows audio to play even though AudioListener.pause is set to true.")]
        [field: SerializeField]
        [PublicAPI]
        public bool IgnoreListenerPause { get; private set; }

        /// <summary>
        ///     Whether to take into account the volume of the audio listener.
        /// </summary>
        [field: Tooltip("Whether to take into account the volume of the audio listener.")]
        [field: SerializeField]
        [PublicAPI]
        public bool IgnoreListenerVolume { get; private set; }

        #endregion

        #region Unity Methods

        private void Reset()
        {
            Clips = Array.Empty<AudioClip>();
            SmartRandom = true;
            MinVolume = 1f;
            MaxVolume = 1f;
            MinPitch = 1f;
            MaxPitch = 1f;
            Priority = 128;
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
            if (!Clips.Any())
            {
                return null;
            }

            if (Clips.Length == 1)
            {
                return Clips.First();
            }

            // If not smart random, return a truly random clip
            if (!SmartRandom)
            {
                return Clips[Random.Range(0, Clips.Length)];
            }

            // Determine choices for the random number generator (don't include the previously chosen clip index)
            var choices = new List<int>(Clips.Length);
            for (var i = 0; i < Clips.Length; i += 1)
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

            return Clips[clipIndex];
        }

        #endregion
    }
}