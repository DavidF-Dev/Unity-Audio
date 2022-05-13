// File: MusicChanger.cs
// Purpose: Component that can be used to play music.
// Created by: DavidFDev

using JetBrains.Annotations;
using UnityEngine;

namespace DavidFDev.Audio
{
    /// <summary>
    ///     Change the music being played.
    /// </summary>
    [AddComponentMenu("DavidFDev/Audio/Music Player")]
    public class MusicPlayer : MonoBehaviour
    {
        #region Properties

        /// <summary>
        ///     Audio clip to play. Trumps the music asset.
        /// </summary>
        [field: Tooltip("Audio clip to play. Trumps the music asset.")]
        [field: SerializeField]
        [PublicAPI] [CanBeNull]
        public AudioClip MusicClip { get; protected set; }

        /// <summary>
        ///     Music asset to play if no audio clip is provided.
        /// </summary>
        [field: Tooltip("Music asset to play if no audio clip is provided.")]
        [field: SerializeField]
        [PublicAPI] [CanBeNull]
        public Music MusicAsset { get; protected set; }

        /// <summary>
        ///     Whether the music should be played automatically when the component awakens.
        /// </summary>
        [field: Tooltip("Whether the music should be played automatically when the component awakens.")]
        [field: SerializeField]
        [PublicAPI]
        public bool PlayOnAwake { get; set; } = true;

        /// <summary>
        ///     Duration, in seconds, that the music should take to fade in.
        /// </summary>
        [PublicAPI]
        public float FadeInDuration { get; set; } = 1f;

        /// <summary>
        ///     Duration, in seconds, that the old music should take to fade out.
        /// </summary>
        [PublicAPI]
        public float FadeOutDuration { get; set; } = 0.75f;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (PlayOnAwake)
            {
                PlayMusic();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Begin playing the music attached to this component.
        /// </summary>
        [PublicAPI]
        public void PlayMusic()
        {
            if (MusicClip != null)
            {
                AudioHelper.PlayMusic(MusicClip, FadeInDuration, FadeOutDuration);
                return;
            }

            if (MusicAsset == null)
            {
                return;
            }

            AudioHelper.PlayMusicAsset(MusicAsset, FadeInDuration, FadeOutDuration);
        }

        /// <summary>
        ///     Stop the current music if it matches the music attached to this component.
        /// </summary>
        [PublicAPI]
        public void StopMusic()
        {
            var clip = MusicClip != null
                ? MusicClip
                : MusicAsset != null
                    ? MusicAsset.Clip
                    : null;
            if (clip == null || AudioHelper.CurrentMusic != clip)
            {
                return;
            }

            AudioHelper.StopMusic(FadeOutDuration);
        }

        [ContextMenu("Play Music (Runtime)")]
        private void PlayContextMenu()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            PlayMusic();
        }

        [ContextMenu("Stop Music (Runtime)")]
        private void StopContextMenu()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (MusicClip != null)
            {
                if (AudioHelper.CurrentMusic != MusicClip)
                {
                    return;
                }
            }
            else if (MusicAsset == null || !AudioHelper.IsMusicAssetPlaying(MusicAsset))
            {
                return;
            }

            AudioHelper.StopMusic();
        }

        #endregion
    }
}