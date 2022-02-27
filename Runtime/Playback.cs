﻿// File: Playback.cs
// Purpose: Control audio playback, abstracting the audio source component.
// Created by: DavidFDev

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;

namespace DavidFDev.Audio
{
    /// <summary>
    ///     Control audio playback properties (similar to AudioSource).
    /// </summary>
    public sealed class Playback
    {
        #region Fields

        [NotNull]
        private AudioSource _source;

        private bool _isPaused;

        #endregion

        #region Constructors

        internal Playback([NotNull] AudioSource source)
        {
            _source = source;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Audio clip being played.
        /// </summary>
        [PublicAPI, CanBeNull]
        public AudioClip Clip => _source == null ? null : _source.clip;

        /// <summary>
        ///     Whether the audio playback is active.
        /// </summary>
        [PublicAPI]
        public bool IsPlaying => _source != null && (_source.isPlaying && !_isPaused);

        /// <summary>
        ///     Whether the audio playback is paused.
        /// </summary>
        [PublicAPI]
        public bool IsPaused
        {
            get => _isPaused;
            private set => _isPaused = value && IsPlaying;
        }

        /// <summary>
        ///     Whether the audio playback is finished and can no longer be used.
        /// </summary>
        [PublicAPI]
        public bool IsFinished => _source == null || !_source.isPlaying && !_isPaused;

        /// <summary>
        ///     Group that the audio playback should output to.
        /// </summary>
        [PublicAPI, CanBeNull]
        public AudioMixerGroup Output
        {
            get => _source.outputAudioMixerGroup;
            set => _source.outputAudioMixerGroup = value;
        }

        /// <summary>
        ///     Volume of the audio playback [0.0 - 1.0].
        /// </summary>
        [PublicAPI]
        public float Volume
        {
            get => _source.volume;
            set => _source.volume = Mathf.Clamp01(value);
        }

        /// <summary>
        ///     Whether the audio playback is muted.
        /// </summary>
        [PublicAPI]
        public bool IsMuted
        {
            get => _source.mute;
            set => _source.mute = value;
        }

        /// <summary>
        ///     Pitch of the audio playback [-3.0 - 3.0].
        /// </summary>
        [PublicAPI]
        public float Pitch
        {
            get => _source.pitch;
            set => _source.pitch = Mathf.Clamp(value, -3f, 3f);
        }

        /// <summary>
        ///     Whether the audio playback should loop.
        /// </summary>
        [PublicAPI]
        public bool Loop
        {
            get => _source.loop;
            set => _source.loop = value;
        }

        /// <summary>
        ///     Priority of the audio playback [0 - 256].
        /// </summary>
        [PublicAPI]
        public int Priority
        {
            get => _source.priority;
            set => _source.priority = Mathf.Clamp(value, 0, 256);
        }
        
        /// <summary>
        ///     Pan the location of a stereo or mono audio playback [-1.0 (left) - 1.0 (right)].
        /// </summary>
        [PublicAPI]
        public float StereoPan
        {
            get => _source.panStereo;
            set => _source.panStereo = Mathf.Clamp(value, -1f, 1f);
        }

        /// <summary>
        ///     Amount that the audio playback is affected by spatialisation calculations [0.0 (2D) - 1.0 (3D)].
        /// </summary>
        [PublicAPI]
        public float SpatialBlend
        {
            get => _source.spatialBlend;
            set => _source.spatialBlend = Mathf.Clamp01(value);
        }

        /// <summary>
        ///     Position of the audio playback in 3D world-space.
        /// </summary>
        [PublicAPI]
        public Vector3 Position
        {
            get => _source.transform.position;
            set => _source.transform.position = value;
        }

        /// <summary>
        ///     Playback position in seconds.
        /// </summary>
        [PublicAPI]
        public float Time
        {
            get => _source.time;
            set => _source.time = Mathf.Max(0f, value);
        }

        /// <summary>
        ///     Playback position in PCM samples.
        /// </summary>
        [PublicAPI]
        public int TimeSamples
        {
            get => _source.timeSamples;
            set => _source.timeSamples = Mathf.Max(0, value);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Pauses playback.
        /// </summary>
        [PublicAPI]
        public void Pause()
        {
            if (_source == null) return;
            IsPaused = true;

            if (IsPaused)
            {
                _source.Pause();
            }
        }

        /// <summary>
        ///     Unpauses playback.
        /// </summary>
        [PublicAPI]
        public void Unpause()
        {
            if (_source == null) return;
            IsPaused = false;

            if (!IsPaused)
            {
                _source.UnPause();
            }
        }

        /// <summary>
        ///     Forcefully finish the playback.
        /// </summary>
        [PublicAPI]
        public void ForceFinish()
        {
            if (_source == null) return;
            _source.Stop();
            IsPaused = false;
        }

        [PublicAPI, Pure]
        public override string ToString()
        {
            return IsFinished ? "Finished" : $"{_source.clip.name} ({(IsPaused ? "Paused" : "Playing")})";
        }

        internal void Dispose()
        {
            _source = null!;
        }

        #endregion
    }
}