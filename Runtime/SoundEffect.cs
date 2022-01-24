// File: SoundEffect.cs
// Purpose: Create a sound effect that can be played at any time.
// Created by: DavidFDev

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace DavidFDev.Audio
{
    /// <summary>
    ///     Sound effect that can be played at any time.
    /// </summary>
    [CreateAssetMenu(menuName = "DavidFDev/Audio/Sound Effect")]
    public sealed class SoundEffect : ScriptableObject
    {
        #region Fields

        private int _previousClipIndex = -1;

        #endregion

        #region Constructors

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
        /// <param name="spatialBlend">Amount that the audio playback is affected by spatialisation calculations [0.0 (2D) - 1.0 (3D)].</param>
        public SoundEffect(AudioClip[] clips, bool smartRandom = true, AudioMixerGroup output = null,
            float minVolume = 1f, float maxVolume = 1f, float minPitch = 1f, float maxPitch = 1f,
            bool loop = false, int priority = 128, float stereoPan = 0f, float spatialBlend = 0f)
        {
            Clips = clips;
            SmartRandom = smartRandom;
            Output = output;
            MinVolume = Mathf.Clamp01(minVolume);
            MaxVolume = Mathf.Clamp01(maxVolume);
            MinPitch = Mathf.Clamp(minPitch, -3f, 3f);
            MaxPitch = Mathf.Clamp(maxPitch, -3f, 3f);
            Loop = loop;
            Priority = Mathf.Clamp(priority, 0, 256);
            StereoPan = Mathf.Clamp(stereoPan, -1f, 1f);
            SpatialBlend = Mathf.Clamp01(spatialBlend);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     A random clip is chosen when the sound effect is played.
        /// </summary>
        [field: Tooltip("A random clip is chosen when the sound effect is played."), SerializeField]
        public AudioClip[] Clips { get; private set; }

        /// <summary>
        ///     Prevents the same clip from being played twice in a row.
        /// </summary>
        [field: Tooltip("Prevents the same clip from being played twice in a row."), SerializeField]
        public bool SmartRandom { get; private set; } = true;

        /// <summary>
        ///     Group that the audio playback should output to.
        /// </summary>
        [field: Space, SerializeField]
        public AudioMixerGroup Output { get; private set; }

        /// <summary>
        ///     Minimum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.
        /// </summary>
        [field: Header("Volume"), SerializeField, Range(0f, 1f)]
        public float MinVolume { get; private set; } = 1f;

        /// <summary>
        ///     Maximum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.
        /// </summary>
        [field: SerializeField, Range(0f, 1f)]
        public float MaxVolume { get; private set; } = 1f;

        /// <summary>
        ///     Minimum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.
        /// </summary>
        [field: Header("Pitch"), SerializeField, Range(-3f, 3f)]
        public float MinPitch { get; private set; } = 1f;

        /// <summary>
        ///     Maximum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.
        /// </summary>
        [field: SerializeField, Range(-3f, 3f)]
        public float MaxPitch { get; private set; } = 1f;

        /// <summary>
        ///     Whether the audio playback should loop.
        /// </summary>
        [field: Space, SerializeField]
        public bool Loop { get; private set; }

        /// <summary>
        ///     Priority of the audio playback [0 - 256].
        /// </summary>
        [field: SerializeField, Range(0, 256)]
        public int Priority { get; private set; } = 128;

        /// <summary>
        ///     Pan the location of a stereo or mono audio playback [-1.0 (left) - 1.0 (right)].
        /// </summary>
        [field: SerializeField, Range(-1f, 1f)]
        public float StereoPan { get; private set; }

        /// <summary>
        ///     Amount that the audio playback is affected by spatialisation calculations [0.0 (2D) - 1.0 (3D)].
        /// </summary>
        [field: SerializeField, Range(0f, 1f)]
        public float SpatialBlend { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Play the sound effect.
        /// </summary>
        /// <param name="position">Position of the audio playback in 3D world-space.</param>
        /// <returns>Instance for controlling audio playback.</returns>
        public Playback Play(Vector3 position = default)
        {
            return Audio.PlaySfx(this, position);
        }

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
            List<int> choices = new List<int>(Clips.Length);
            for (int i = 0; i < Clips.Length; i += 1)
            {
                if (i == _previousClipIndex)
                {
                    continue;
                }

                choices.Add(i);
            }

            // Get a random clip
            int clipIndex = choices[Random.Range(0, choices.Count)];
            _previousClipIndex = clipIndex;

            return Clips[clipIndex];
        }

        #endregion
    }
}