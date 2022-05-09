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
        /// <param name="minVolume">Minimum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.</param>
        /// <param name="maxVolume">Maximum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.</param>
        /// <param name="minPitch">Minimum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.</param>
        /// <param name="maxPitch">Maximum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.</param>
        /// <param name="priority">Priority of the audio playback [0 - 256].</param>
        /// <param name="stereoPan">Pan the location of a stereo or mono audio playback [-1.0 (left) - 1.0 (right)].</param>
        [PublicAPI]
        [NotNull]
        public static Music Create([CanBeNull] AudioClip clip, [CanBeNull] AudioMixerGroup output = null,
            float minVolume = 1f,
            float maxVolume = 1f, float minPitch = 1f, float maxPitch = 1f, int priority = 128, float stereoPan = 0f)
        {
            var instance = CreateInstance<Music>();
            instance.Clip = clip;
            instance.Output = output;
            instance.MinVolume = Mathf.Clamp01(minVolume);
            instance.MaxVolume = Mathf.Clamp01(maxVolume);
            instance.MinPitch = Mathf.Clamp01(minPitch);
            instance.MaxPitch = Mathf.Clamp01(maxPitch);
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
        [field: SerializeField]
        [PublicAPI]
        [CanBeNull]
        public AudioClip Clip { get; private set; }

        /// <summary>
        ///     Group that the audio playback should output to.
        /// </summary>
        [field: Space]
        [field: SerializeField]
        [PublicAPI]
        [CanBeNull]
        public AudioMixerGroup Output { get; private set; }

        /// <summary>
        ///     Minimum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.
        /// </summary>
        [field: Header("Volume")]
        [field: SerializeField]
        [field: Range(0f, 1f)]
        [PublicAPI]
        public float MinVolume { get; private set; }

        /// <summary>
        ///     Maximum possible volume of the audio playback [0.0 - 1.0]. Volume is chosen randomly.
        /// </summary>
        [field: SerializeField]
        [field: Range(0f, 1f)]
        [PublicAPI]
        public float MaxVolume { get; private set; }

        /// <summary>
        ///     Minimum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.
        /// </summary>
        [field: Header("Pitch")]
        [field: SerializeField]
        [field: Range(-3f, 3f)]
        [PublicAPI]
        public float MinPitch { get; private set; }

        /// <summary>
        ///     Maximum pitch of the audio playback [-3.0 - 3.0]. Pitch is chosen randomly.
        /// </summary>
        [field: SerializeField]
        [field: Range(-3f, 3f)]
        [PublicAPI]
        public float MaxPitch { get; private set; }

        /// <summary>
        ///     Priority of the audio playback [0 - 256].
        /// </summary>
        [field: SerializeField]
        [field: Range(0, 256)]
        [PublicAPI]
        public int Priority { get; private set; }

        /// <summary>
        ///     Pan the location of a stereo or mono audio playback [-1.0 (left) - 1.0 (right)].
        /// </summary>
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
            MinVolume = 1f;
            MaxVolume = 1f;
            MinPitch = 1f;
            MaxPitch = 1f;
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