// File: Playback.cs
// Purpose: Control audio playback, abstracting the audio source component.
// Created by: DavidFDev

using System;
using System.Collections;
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
        [PublicAPI] [CanBeNull]
        public AudioClip Clip => _source == null ? null : _source.clip;

        /// <summary>
        ///     Whether the audio playback is active.
        /// </summary>
        [PublicAPI]
        public bool IsPlaying => _source != null && _source.isPlaying && !_isPaused;

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
        public bool IsFinished => _source == null ||
                                  (!_source.isPlaying && !_isPaused && (!AudioListener.pause || IgnoreListenerPause));

        /// <summary>
        ///     Group that the audio playback should output to.
        /// </summary>
        [PublicAPI] [CanBeNull]
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
        ///     Whether the audio playback should loop.<br />
        ///     If looping, the playback must be stopped manually
        /// </summary>
        [PublicAPI]
        public bool Loop
        {
            get => _source.loop;
            set => _source.loop = value;
        }

        /// <summary>
        ///     Priority of the audio playback [0 (highest) - 256 (lowest)].
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
        ///     Doppler scale for 3D spatialisation [0.0 - 5.0].<br />
        ///     Used in 3D spatialisation calculations.
        /// </summary>
        [PublicAPI]
        public float Doppler
        {
            get => _source.dopplerLevel;
            set => _source.dopplerLevel = Mathf.Clamp(value, 0f, 5f);
        }

        /// <summary>
        ///     Spread angle (in degrees) of a 3D stereo or multichannel sound in speaker space [0.0 - 360.0].<br />
        ///     Used in 3D spatialisation calculations.
        /// </summary>
        [PublicAPI]
        public float Spread
        {
            get => _source.spread;
            set => _source.spread = Mathf.Clamp(value, 0, 360);
        }

        /// <summary>
        ///     How the audio source attenuates over distance.<br />
        ///     Used in 3D spatialisation calculations.
        /// </summary>
        [PublicAPI]
        public AudioRolloffMode RolloffMode
        {
            get => _source.rolloffMode;
            set => _source.rolloffMode = value;
        }

        /// <summary>
        ///     Within the minimum distance the audio source will cease to grow louder in volume.<br />
        ///     Used in 3D spatialisation calculations.
        /// </summary>
        [PublicAPI]
        public float MinDistance
        {
            get => _source.minDistance;
            set => _source.minDistance = Mathf.Max(value, 0f);
        }

        /// <summary>
        ///     Logarithmic rolloff: Distance at which the sound stops attenuating.<br />
        ///     Linear rolloff: Distance at which the sound is completely inaudible.<br />
        ///     Used in 3D spatialisation calculations.
        /// </summary>
        [PublicAPI]
        public float MaxDistance
        {
            get => _source.maxDistance;
            set => _source.maxDistance = Mathf.Max(value, 0f);
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

        /// <summary>
        ///     Allows audio to play even though AudioListener.pause is set to true.
        /// </summary>
        [PublicAPI]
        public bool IgnoreListenerPause
        {
            get => _source.ignoreListenerPause;
            set => _source.ignoreListenerPause = value;
        }

        /// <summary>
        ///     Whether to take into account the volume of the audio listener.
        /// </summary>
        [PublicAPI]
        public bool IgnoreListenerVolume
        {
            get => _source.ignoreListenerVolume;
            set => _source.ignoreListenerVolume = value;
        }

        #endregion

        #region Events

        /// <summary>
        ///     Invoked when the playback finishes.
        /// </summary>
        [PublicAPI]
        public event Action Finished;

        /// <summary>
        ///     Invoked when the playback pauses or unpauses.<br />
        ///     Parameter is true when paused.
        /// </summary>
        [PublicAPI]
        public event Action<bool> Paused;

        /// <summary>
        ///     Invoked when playback finishes, after the Finished event.
        /// </summary>
        [PublicAPI]
        internal event Action InternalFinished;

        #endregion

        #region Methods

        /// <summary>
        ///     Pauses playback.
        /// </summary>
        [PublicAPI]
        public void Pause()
        {
            if (IsFinished)
            {
                return;
            }

            IsPaused = true;

            if (IsPaused)
            {
                _source.Pause();
                Paused?.Invoke(true);
            }
        }

        /// <summary>
        ///     Unpauses playback.
        /// </summary>
        [PublicAPI]
        public void Unpause()
        {
            if (IsFinished)
            {
                return;
            }

            IsPaused = false;

            if (!IsPaused)
            {
                _source.UnPause();
                Paused?.Invoke(false);
            }
        }

        /// <summary>
        ///     Forcefully finish the playback.
        /// </summary>
        [PublicAPI]
        public void ForceFinish()
        {
            if (IsFinished)
            {
                return;
            }

            _source.Stop();
            IsPaused = false;
        }

        [PublicAPI] [Pure]
        public override string ToString()
        {
            return IsFinished ? "Finished" : $"{_source.clip.name} ({(IsPaused ? "Paused" : "Playing")})";
        }

        internal void Dispose()
        {
            _source = null!;
        }

        internal IEnumerator C_WaitForFinish()
        {
            yield return new WaitForPlayback(this);

            try
            {
                Finished?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                InternalFinished?.Invoke();
            }
        }

        #endregion
    }

    /// <summary>
    ///     Yield instruction that waits for a given playback instance to finish.<br />
    ///     Usage: yield return new WaitForPlayback(...)
    /// </summary>
    public sealed class WaitForPlayback : CustomYieldInstruction
    {
        #region Fields

        [PublicAPI]
        [CanBeNull]
        public readonly Playback Playback;

        #endregion

        #region Constructors

        public WaitForPlayback([CanBeNull] Playback playback)
        {
            Playback = playback;
        }

        #endregion

        #region Properties

        public override bool keepWaiting => !Playback?.IsFinished ?? false;

        #endregion
    }
}