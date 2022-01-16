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
    [CreateAssetMenu()]
    public sealed class SoundEffect : ScriptableObject
    {
        #region Fields

        private int _previousClipIndex = -1;

        #endregion

        #region Properties

        [field: Tooltip("A random clip is chosen when the sound effect is played."), SerializeField]
        public AudioClip[] Clips { get; private set; }

        [field: Tooltip("Prevents the same clip from being played twice in a row."), SerializeField]
        public bool SmartRandom { get; private set; } = true;

        [field: Space, SerializeField]
        public AudioMixerGroup Output { get; private set; }

        [field: Header("Volume"), SerializeField, Range(0f, 1f)]
        public float MinVolume { get; private set; } = 1f;

        [field: SerializeField, Range(0f, 1f)]
        public float MaxVolume { get; private set; } = 1f;

        [field: Header("Pitch"), SerializeField, Range(-3f, 3f)]
        public float MinPitch { get; private set; } = 1f;

        [field: SerializeField, Range(-3f, 3f)]
        public float MaxPitch { get; private set; } = 1f;

        [field: Space, SerializeField]
        public bool Loop { get; private set; }

        [field: SerializeField, Range(0, 256)]
        public int Priority { get; private set; } = 128;

        [field: SerializeField, Range(-1f, 1f)]
        public float StereoPan { get; private set; }

        [field: SerializeField, Range(0f, 1f)]
        public float SpatialBlend { get; private set; }

        #endregion

        #region Methods

        public void Play(Vector3 position)
        {
            Audio.PlaySfx(this, position);
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