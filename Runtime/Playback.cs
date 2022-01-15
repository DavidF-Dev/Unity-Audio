using UnityEngine;

namespace DavidFDev.Audio
{
    public sealed class Playback
    {
        #region Fields

        private AudioSource _source;

        private bool _isPaused;

        #endregion

        #region Constructors

        internal Playback(AudioSource source)
        {
            _source = source;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Whether the audio playback is active.
        /// </summary>
        public bool IsPlaying => _source != null && (_source.isPlaying && !_isPaused);

        /// <summary>
        ///     Whether the audio playback is paused.
        /// </summary>
        public bool IsPaused
        {
            get => _isPaused;
            private set => _isPaused = value && IsPlaying;
        }

        /// <summary>
        ///     Whether the audio playback is finished and can no longer be used.
        /// </summary>
        public bool IsFinished => _source == null || (!_source.isPlaying && !_isPaused);

        public float Volume
        {
            get => _source.volume;
            set => _source.volume = Mathf.Clamp01(value);
        }

        public float Pitch
        {
            get => _source.pitch;
            set => _source.pitch = Mathf.Clamp(value, -3f, 3f);
        }

        public bool Loop
        {
            get => _source.loop;
            set => _source.loop = value;
        }

        public float Time
        {
            get => _source.time;
            set => _source.time = Mathf.Max(0f, value);
        }

        public int TimeSamples
        {
            get => _source.timeSamples;
            set => _source.timeSamples = Mathf.Max(0, value);
        }

        public Vector3 Position
        {
            get => _source.transform.position;
            set => _source.transform.position = value;
        }

        #endregion

        #region Methods

        public void Pause()
        {
            IsPaused = true;
        }

        public void Unpause()
        {
            IsPaused = false;
        }

        public void ForceFinish()
        {
            _source.Stop();
            IsPaused = false;
        }

        public override string ToString()
        {
            if (IsFinished)
            {
                return "Finished";
            }

            return $"{_source.clip.name} ({(IsPaused ? "Paused" : "Playing")})";
        }

        internal void Dispose()
        {
            _source = null;
        }

        #endregion
    }
}