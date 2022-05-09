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
        ///     Music to play.
        /// </summary>
        [field: SerializeField]
        [PublicAPI, CanBeNull]
        public AudioClip Music { get; protected set; }

        /// <summary>
        ///     Whether the music should be played automatically when the component awakens.
        /// </summary>
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

        #region Methods

        /// <summary>
        ///     Begin playing the music attached to this component.
        /// </summary>
        [PublicAPI]
        public void PlayMusic()
        {
            AudioHelper.PlayMusic(Music, FadeInDuration, FadeOutDuration);
        }

        /// <summary>
        ///     Stop the current music if it matches the music attached to this component.
        /// </summary>
        [PublicAPI]
        public void StopMusic()
        {
            if (AudioHelper.CurrentMusic != Music)
            {
                return;
            }

            AudioHelper.StopMusic(FadeOutDuration);
        }

        private void Awake()
        {
            if (PlayOnAwake)
            {
                PlayMusic();
            }
        }

        #endregion
    }
}