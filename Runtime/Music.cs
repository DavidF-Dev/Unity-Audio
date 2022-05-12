// File: Music.cs
// Purpose: Create a music asset that can be played at any time.
// Created by: DavidFDev

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;

namespace DavidFDev.Audio
{
    /// <summary>
    ///     Music that can be played at any time.
    /// </summary>
    [CreateAssetMenu(menuName = "DavidFDev/Audio/Music")]
    public sealed class Music : ScriptableObject
    {
        #region Static Methods

        /// <summary>
        ///     Create a new music instance via code.
        /// </summary>
        /// <param name="clip">Audio clip to play.</param>
        /// <param name="output">Group that the audio playback should output to.</param>
        /// <param name="volume">Volume of the audio playback [0.0 - 1.0].</param>
        /// <param name="pitch">Pitch of the audio playback [-3.0 - 3.0].</param>
        /// <param name="priority">Priority of the audio playback [0 - 256].</param>
        /// <param name="stereoPan">Pan the location of a stereo or mono audio playback [-1.0 (left) - 1.0 (right)].</param>
        [PublicAPI]
        [NotNull]
        public static Music Create([CanBeNull] AudioClip clip, [CanBeNull] AudioMixerGroup output = null,
            float volume = 1f, float pitch = 1f, int priority = 128, float stereoPan = 0f)
        {
            var instance = CreateInstance<Music>();
            instance.Clip = clip;
            instance.Output = output;
            instance.Volume = Mathf.Clamp01(volume);
            instance.Pitch = Mathf.Clamp01(pitch);
            instance.Priority = Mathf.Clamp(priority, 0, 256);
            instance.StereoPan = Mathf.Clamp(stereoPan, -1f, 1f);
            return instance;
        }

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
        [field: Tooltip("Audio clip to play.")]
        [field: SerializeField]
        [PublicAPI]
        [CanBeNull]
        public AudioClip Clip { get; private set; }

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
        ///     Volume of the audio playback [0.0 - 1.0].
        /// </summary>
        [field: Space]
        [field: Tooltip("Volume of the audio playback [0.0 - 1.0].")]
        [field: SerializeField]
        [PublicAPI]
        public float Volume { get; private set; }
        
        /// <summary>
        ///     Pitch of the audio playback [-3.0 - 3.0].
        /// </summary>
        [field: Tooltip("Pitch of the audio playback [-3.0 - 3.0].")]
        [field: SerializeField]
        [PublicAPI]
        public float Pitch { get; private set; }

        /// <summary>
        ///     Priority of the audio playback [0 (highest) - 256 (lowest)].
        /// </summary>
        [field: Tooltip("Priority of the audio playback [0 (highest) - 256 (lowest)].")]
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

        #endregion

        #region Unity Methods

        private void Reset()
        {
            Clip = null;
            Output = null;
            Volume = 1f;
            Pitch = 1f;
            Priority = 128;
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

        #endregion
    }
}