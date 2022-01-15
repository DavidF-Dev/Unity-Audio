﻿using UnityEngine;
using UnityEngine.Audio;

namespace DavidFDev.Audio
{
    [CreateAssetMenu()]
    public sealed class SoundEffect : ScriptableObject
    {
        #region Properties

        [field: SerializeField]
        public AudioClip[] Clips { get; private set; }

        [field: SerializeField]
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
    }
}