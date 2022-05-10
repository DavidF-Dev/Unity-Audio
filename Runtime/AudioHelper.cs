// File: AudioHelper.cs
// Purpose: Static class for playing audio (clips, sound effects and music).
// Created by: DavidFDev

#define HIDE_IN_EDITOR
//#define DEBUG_AUDIO

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DavidFDev.Audio
{
    /// <summary>
    ///     Play audio clips, sound effects and music. Also provides other helpful audio methods.
    /// </summary>
    public static class AudioHelper
    {
        #region Static fields

        [NotNull]
        private static Transform _parent = null!;

        [NotNull]
        private static MonoBehaviour _mono = null!;

        [NotNull]
        private static readonly Dictionary<AudioSource, Playback> Current = new Dictionary<AudioSource, Playback>();

        [NotNull]
        private static readonly Stack<AudioSource> Available = new Stack<AudioSource>();

        [NotNull]
        private static readonly Dictionary<string, AudioClip> CachedClips = new Dictionary<string, AudioClip>();

        [NotNull]
        private static readonly Dictionary<string, SoundEffect> CachedSfxAssets = new Dictionary<string, SoundEffect>();

        [NotNull]
        private static readonly Dictionary<string, Music> CachedMusicAssets = new Dictionary<string, Music>();

        [NotNull]
        private static AudioSource _musicPlayback = null!;

        [NotNull]
        private static AudioSource _musicFader = null!;

        [CanBeNull]
        private static Coroutine _musicFadeIn;

        [CanBeNull]
        private static Coroutine _musicFadeOut;

        #endregion

        #region Static properties

        /// <summary>
        ///     Current audio clip used by the music playback.
        /// </summary>
        [PublicAPI] [CanBeNull]
        public static AudioClip CurrentMusic => _musicPlayback.clip;

        /// <summary>
        ///     Whether the music playback is currently fading between two tracks.
        /// </summary>
        [PublicAPI]
        public static bool IsMusicFading => _musicFadeIn != null || _musicFadeOut != null;

        #endregion

        #region Static methods

        /// <summary>
        ///     Play an audio clip.
        /// </summary>
        /// <param name="clip">Audio clip to play.</param>
        /// <param name="position">Position of the audio playback in 3D world-space.</param>
        /// <param name="output">Group that the audio playback should output to.</param>
        /// <returns>Playback instance for controlling the audio.</returns>
        [PublicAPI] [CanBeNull]
        public static Playback Play([CanBeNull] AudioClip clip, Vector3 position = default,
            [CanBeNull] AudioMixerGroup output = null)
        {
            if (clip == null)
            {
                Debug.LogError("Failed to play clip: provided clip is null.");
                return null;
            }

            var source = GetAudioSource();
            source.clip = clip;

#if !HIDE_IN_EDITOR
            source.gameObject.name = $"Audio Source ({clip.name})";
#endif

            // Set defaults
            var playback = Current[source];
            playback.Output = output;
            playback.Volume = PlaybackDefaults.Volume;
            playback.IsMuted = false;
            playback.Pitch = PlaybackDefaults.Pitch;
            playback.Loop = false;
            playback.Priority = 128;
            playback.StereoPan = 0f;
            playback.SpatialBlend = PlaybackDefaults.SpatialBlend;
            playback.Doppler = PlaybackDefaults.Doppler;
            playback.Spread = PlaybackDefaults.Spread;
            playback.RolloffMode = PlaybackDefaults.RolloffMode;
            playback.MinDistance = PlaybackDefaults.MinDistance;
            playback.MaxDistance = PlaybackDefaults.MaxDistance;
            playback.Position = position;

            source.Play();

#if DEBUG_AUDIO
            Debug.Log($"Started audio playback for {clip.name} at {playback.Position}{(output != null ? $" [{output.name}]" : "")}.");
#endif

            return playback;
        }

        /// <summary>
        ///     Play an audio clip loaded from a resource.
        /// </summary>
        /// <param name="path">Path to the audio clip to play.</param>
        /// <param name="position">Position of the audio playback in 3D world-space.</param>
        /// <param name="output">Group that the audio playback should output to.</param>
        /// <returns>Playback instance for controlling the audio.</returns>
        [PublicAPI] [CanBeNull]
        public static Playback Play([NotNull] string path, Vector3 position = default,
            [CanBeNull] AudioMixerGroup output = null)
        {
            return Play(TryGetClipFromResource(path), position, output);
        }

        /// <summary>
        ///     Play a sound effect.
        /// </summary>
        /// <param name="asset">Sound effect to play.</param>
        /// <param name="position">Position of the audio playback in 3D world-space.</param>
        /// <returns>Playback instance for controlling the audio.</returns>
        [PublicAPI] [CanBeNull]
        public static Playback PlaySfx([CanBeNull] SoundEffect asset, Vector3 position = default)
        {
            if (asset == null)
            {
                Debug.LogError("Failed to play sound effect: provided asset is null.");
                return null;
            }

            if (!asset.Clips.Any())
            {
                Debug.LogError("Failed to play sound effect: asset contains no clips.");
                return null;
            }

            var playback = Play(asset.GetClipAtRandom(), position, asset.Output);
            if (playback == null)
            {
                Debug.LogError("Failed to play sound effect: an issue occurred with the chosen clip.");
                return null;
            }

            playback.Volume = Random.Range(asset.MinVolume, asset.MaxVolume);
            playback.Pitch = Random.Range(asset.MinPitch, asset.MaxPitch);
            playback.Loop = asset.Loop;
            playback.Priority = asset.Priority;
            playback.StereoPan = asset.StereoPan;
            playback.SpatialBlend = asset.SpatialBlend;
            playback.IgnoreListenerPause = asset.IgnoreListenerPause;
            playback.IgnoreListenerVolume = asset.IgnoreListenerVolume;
            return playback;
        }

        /// <summary>
        ///     Play a sound effect loaded from a resource.
        /// </summary>
        /// <param name="path">Path to the sound effect to play.</param>
        /// <param name="position">Position of the audio playback in 3D world-space.</param>
        /// <returns>Playback instance for controlling the audio.</returns>
        [PublicAPI] [CanBeNull]
        public static Playback PlaySfx([NotNull] string path, Vector3 position = default)
        {
            return PlaySfx(TryGetSfxFromResource(path), position);
        }

        /// <summary>
        ///     Play a music track. If a music track is already playing, it will be faded out.
        ///     <para>Note: .mp3 files are known to cause popping sounds in Unity under certain circumstances.</para>
        /// </summary>
        /// <param name="music">Music to play.</param>
        /// <param name="fadeIn">Duration, in seconds, that the music should take to fade in.</param>
        /// <param name="fadeOut">Duration, in seconds, that the old music should take to fade out.</param>
        [PublicAPI]
        public static void PlayMusic([CanBeNull] AudioClip music, float fadeIn = 1f, float fadeOut = 0.75f)
        {
#if DEBUG_AUDIO
            static string GetAudioClipName(AudioSource source)
            {
                return source.clip == null ? "none" : source.clip.name;
            }
#endif

            // Cancel if the new music is the same as the current music
            if (music == _musicPlayback.clip)
            {
#if DEBUG_AUDIO
                Debug.Log("Cancelled music change as the new music is the same as the current music.");
#endif

                return;
            }

            // Start fading out the old music playback
            if (fadeOut > 0f && _musicPlayback.clip != null)
            {
                _musicFader.Stop();
                _musicFader.clip = _musicPlayback.clip;
                _musicFader.outputAudioMixerGroup = _musicPlayback.outputAudioMixerGroup;
                _musicFader.pitch = _musicPlayback.pitch;
                _musicFader.panStereo = _musicPlayback.panStereo;
                _musicFader.Play();
                _musicFader.timeSamples = _musicPlayback.timeSamples;

                // If there is fading happening already, forcefully stop it
                if (_musicFadeOut != null)
                {
                    _mono.StopCoroutine(_musicFadeOut);
                }

                _musicFadeOut = _mono.StartCoroutine(SimpleLerp(_musicPlayback.volume, 0f, fadeOut, Mathf.Lerp,
                    x => _musicFader.volume = x, () =>
                    {
                        _musicFadeOut = null;
                        _musicFader.Stop();
                        _musicFader.clip = null;
                    }));

#if DEBUG_AUDIO
                Debug.Log($"Began fading out old music, {GetAudioClipName(_musicFader)}, over {fadeOut} seconds.");
#endif
            }

            // Stop the music playback if no clip is provided
            if (music == null)
            {
                _musicPlayback.Stop();
                _musicPlayback.clip = null;

#if DEBUG_AUDIO
                Debug.Log($"Music changed from {GetAudioClipName(_musicFader)} to {GetAudioClipName(_musicPlayback)}.");
#endif

                return;
            }

            // Start the new music playback
            _musicPlayback.Stop();
            _musicPlayback.clip = music;
            _musicPlayback.Play();

#if DEBUG_AUDIO
            Debug.Log($"Music changed from {GetAudioClipName(_musicFader)} to {GetAudioClipName(_musicPlayback)}.");
#endif

            // Start fading in the new music playback
            if (fadeIn > 0f)
            {
                // If there is fading happening already, forcefully stop it
                if (_musicFadeIn != null)
                {
                    _mono.StopCoroutine(_musicFadeIn);
                }

                _musicFadeIn = _mono.StartCoroutine(SimpleLerp(0f, MusicPlayback.Volume, fadeIn, Mathf.Lerp,
                    x => _musicPlayback.volume = x, () => _musicFadeIn = null));

#if DEBUG_AUDIO
                Debug.Log($"Began fading in new music, {GetAudioClipName(_musicPlayback)}, over {fadeIn} seconds.");
#endif
            }
        }

        /// <summary>
        ///     Play a music track loaded from a resource. If a music track is already playing, it will be faded out.
        ///     <para>Note: .mp3 files are known to cause popping sounds in Unity under certain circumstances.</para>
        /// </summary>
        /// <param name="path">Path to the music to play.</param>
        /// <param name="fadeIn">Duration, in seconds, that the music should take to fade in.</param>
        /// <param name="fadeOut">Duration, in seconds, that the old music should take to fade out.</param>
        [PublicAPI]
        public static void PlayMusic([NotNull] string path, float fadeIn = 1f, float fadeOut = 0.75f)
        {
            var music = TryGetClipFromResource(path);

            if (music == null)
            {
                Debug.LogError("Failed to play music: track couldn't be found.");
                return;
            }

            PlayMusic(music, fadeIn, fadeOut);
        }

        /// <summary>
        ///     Play a music track asset. If a music track is already playing, it will be faded out.
        ///     <para>Note: .mp3 files are known to cause popping sounds in Unity under certain circumstances.</para>
        /// </summary>
        /// <param name="asset">Music asset to play.</param>
        /// <param name="fadeIn">Duration, in seconds, that the music should take to fade in.</param>
        /// <param name="fadeOut">Duration, in seconds, that the old music should take to fade out.</param>
        [PublicAPI]
        public static void PlayMusicAsset([CanBeNull] Music asset, float fadeIn = 1f, float fadeOut = 0.75f)
        {
            if (asset == null)
            {
                Debug.LogError("Failed to play music: provided asset is null.");
                return;
            }

            PlayMusic(asset.Clip, fadeIn, fadeOut);
            MusicPlayback.Output = asset.Output;
            MusicPlayback.Volume = Random.Range(asset.MinVolume, asset.MaxVolume);
            MusicPlayback.Pitch = Random.Range(asset.MinPitch, asset.MaxPitch);
            MusicPlayback.Priority = asset.Priority;
            MusicPlayback.StereoPan = asset.StereoPan;
        }

        /// <summary>
        ///     Play a music track asset loaded from a resource. If a music track is already playing, it will be faded out.
        ///     <para>Note: .mp3 files are known to cause popping sounds in Unity under certain circumstances.</para>
        /// </summary>
        /// <param name="path">Path to the music asset to play.</param>
        /// <param name="fadeIn">Duration, in seconds, that the music should take to fade in.</param>
        /// <param name="fadeOut">Duration, in seconds, that the old music should take to fade out.</param>
        [PublicAPI]
        public static void PlayMusicAsset([NotNull] string path, float fadeIn = 1f, float fadeOut = 0.75f)
        {
            PlayMusicAsset(TryGetMusicFromResource(path), fadeIn, fadeOut);
        }

        /// <summary>
        ///     Stop music playback.
        /// </summary>
        /// <param name="fadeOut">Duration, in seconds, that the music should take to fade out.</param>
        [PublicAPI]
        public static void StopMusic(float fadeOut = 0.75f)
        {
            PlayMusic((AudioClip)null, 0f, fadeOut);
        }

        /// <summary>
        ///     Stop all audio playbacks, freeing up resources.
        /// </summary>
        /// <param name="stopMusic">Stop music playback.</param>
        /// <param name="destroyObjects">Destroy pooled game objects.</param>
        [PublicAPI]
        public static void StopAllAudio(bool stopMusic = false, bool destroyObjects = false)
        {
            if (stopMusic)
            {
                StopMusic(0f);
            }

            if (destroyObjects)
            {
                foreach (var source in Current.Keys)
                {
                    Object.Destroy(source.gameObject);
                }

                Current.Clear();

                while (Available.Any())
                {
                    Object.Destroy(Available.Pop());
                }

#if DEBUG_AUDIO
                Debug.Log("Destroyed all audio sources.");
#endif

                return;
            }

            // Release all the in-use sources back into the pool
            foreach (var source in Current.Keys)
            {
                Current[source].ForceFinish();
                Current[source].Dispose();
                Available.Push(source);
            }

            Current.Clear();

#if DEBUG_AUDIO
            Debug.Log("Freed all audio sources back into the pool.");
#endif
        }

        /// <summary>
        ///     Clear cached audio clips and sound effect assets, freeing up memory.
        /// </summary>
        [PublicAPI]
        public static void ClearCache()
        {
            CachedClips.Clear();
            CachedSfxAssets.Clear();
            CachedMusicAssets.Clear();
        }

        /// <summary>
        ///     Get an attenuation (volume) scaled logarithmically.
        ///     Use this instead of a linear scale - decibels do not scale linearly!
        /// </summary>
        /// <param name="volume01">
        ///     0.0 returns -80.0db (practically silent).<br />
        ///     0.5 returns approximately -14.0db (half volume).<br />
        ///     1.0 returns 0.0db (full volume - no gain).
        /// </param>
        /// <returns>Attenuation (volume) in decibels.</returns>
        [PublicAPI] [Pure]
        public static float GetAttenuation(float volume01)
        {
            volume01 = Mathf.Clamp01(volume01);
            return volume01 == 0f ? -80f : Mathf.Log(volume01) * 20f;
        }

        /// <summary>
        ///     Get a normalised attenuation (volume) clamped between 0 and 1, using an inversed logarithmic scale.
        /// </summary>
        /// <param name="volume">
        ///     -80.0db returns 0.0.<br />
        ///     -14.0db returns 0.5.<br />
        ///     0.0db returns 1.0.
        /// </param>
        /// <returns>
        ///     Normalised attenuation [0.0 - 1.0].
        /// </returns>
        [PublicAPI] [Pure]
        public static float GetNormalisedAttenuation(float volume)
        {
            return Mathf.Pow((float)Math.E, volume / 20f);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            // Create the object that will hold the audio sources
            _parent = new GameObject("Audio Pool").transform;
            GameObject parentObj;
            _mono = (parentObj = _parent.gameObject).AddComponent<DummyMono>();
            Object.DontDestroyOnLoad(parentObj);
#if HIDE_IN_EDITOR
            _parent.gameObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
#endif

#if DEBUG_AUDIO
            Debug.Log("Created audio pool game object.");
#endif

            // Create audio sources for music playback and fading
            {
                _musicPlayback = new GameObject("Music Playback").AddComponent<AudioSource>();
                _musicPlayback.gameObject.transform.SetParent(_parent);
                _musicPlayback.loop = true;
                _musicPlayback.spatialBlend = 0f;

                _musicFader = new GameObject("Music Fader").AddComponent<AudioSource>();
                _musicFader.gameObject.transform.SetParent(_parent);
                _musicFader.spatialBlend = 0f;
            }
        }

        [NotNull]
        private static AudioSource GetAudioSource()
        {
            AudioSource source;

            // Check for an available audio source in the pool
            if (Available.Any())
            {
                source = Available.Pop();

#if DEBUG_AUDIO
                Debug.Log("Retrieved available audio source from the pool.");
#endif
            }

            // Otherwise, search for a finished audio source
            else if ((source = Current.FirstOrDefault(x => x.Value.IsFinished).Key) != null)
            {
                // Get the source ready for re-use
                Current[source].Dispose();
                Current.Remove(source);

#if DEBUG_AUDIO
                Debug.Log("Retrieved a previously finished audio source.");
#endif
            }

            // Otherwise, create a new audio source
            else
            {
                source = new GameObject("Audio Source").AddComponent<AudioSource>();
                source.transform.SetParent(_parent);
                source.playOnAwake = false;

#if DEBUG_AUDIO
                Debug.Log("Created a new audio source.");
#endif
            }

            Current.Add(source, new Playback(source));
            source.gameObject.SetActive(true);

            return source;
        }

        [CanBeNull]
        private static AudioClip TryGetClipFromResource([NotNull] string path)
        {
            // Check if the clip has been cached for easy retrieval
            if (CachedClips.TryGetValue(path, out var clip))
            {
#if DEBUG_AUDIO
                Debug.Log("Retrieved audio clip from cached resources.");
#endif

                return clip;
            }

            // Load the clip from the resources folder
            clip = Resources.Load<AudioClip>(path);

            if (clip == null)
            {
#if DEBUG_AUDIO
                Debug.LogError($"Failed to load clip: no clip could be found at \"{path}\".");
#endif
                return null;
            }

#if DEBUG_AUDIO
            Debug.Log("Loaded audio clip from resources.");
#endif

            // Cache
            CachedClips.Add(path, clip);

            return clip;
        }

        [CanBeNull]
        private static SoundEffect TryGetSfxFromResource([NotNull] string path)
        {
            // Check if the asset has been cached for easy retrieval
            if (CachedSfxAssets.TryGetValue(path, out var asset))
            {
#if DEBUG_AUDIO
                Debug.Log("Retrieved asset from cached resources.");
#endif

                return asset;
            }

            // Load the asset from the resources folder
            asset = Resources.Load<SoundEffect>(path);

            if (asset == null)
            {
#if DEBUG_AUDIO
                Debug.LogError($"Failed to load asset: no asset could be found at \"{path}\".");
#endif
                return null;
            }

#if DEBUG_AUDIO
            Debug.Log("Loaded asset from resources.");
#endif

            // Cache
            CachedSfxAssets.Add(path, asset);

            return asset;
        }

        [CanBeNull]
        private static Music TryGetMusicFromResource([NotNull] string path)
        {
            // Check if the asset has been cached for easy retrieval
            if (CachedMusicAssets.TryGetValue(path, out var asset))
            {
#if DEBUG_AUDIO
                Debug.Log("Retrieved asset from cached resources.");
#endif

                return asset;
            }

            // Load the asset from the resources folder
            asset = Resources.Load<Music>(path);

            if (asset == null)
            {
#if DEBUG_AUDIO
                Debug.LogError($"Failed to load asset: no asset could be found at \"{path}\".");
#endif
                return null;
            }

#if DEBUG_AUDIO
            Debug.Log("Loaded asset from resources.");
#endif

            // Cache
            CachedMusicAssets.Add(path, asset);

            return asset;
        }

        private static IEnumerator SimpleLerp<T>([NotNull] T start, [NotNull] T end, float duration,
            [NotNull] Func<T, T, float, T> lerpFunction, [NotNull] Action<T> onUpdate, [CanBeNull] Action onComplete)
        {
            for (float t = 0; t <= duration; t += Time.deltaTime)
            {
                onUpdate(lerpFunction(start, end, t / duration));
                yield return null;
            }

            onUpdate(end);
            onComplete?.Invoke();
        }

        #endregion

        #region Nested types

        /// <summary>
        ///     Default values used for audio playback.
        /// </summary>
        [PublicAPI]
        public static class PlaybackDefaults
        {
            #region Static fields

            private static float _volume = 1f;

            private static float _pitch = 1f;

            private static float _spatialBlend;

            private static float _doppler = 1f;

            private static float _spread;

            private static float _minDistance = 1f;

            private static float _maxDistance = 500f;

            #endregion

            #region Static properties

            /// <summary>
            ///     Volume of the audio playback [0.0 - 1.0].
            /// </summary>
            [PublicAPI]
            public static float Volume
            {
                get => _volume;
                set => _volume = Mathf.Clamp01(value);
            }

            /// <summary>
            ///     Pitch of the audio playback [-3.0 - 3.0].
            /// </summary>
            [PublicAPI]
            public static float Pitch
            {
                get => _pitch;
                set => _pitch = Mathf.Clamp(value, -3f, 3f);
            }

            /// <summary>
            ///     Amount that the audio playback is affected by spatialisation calculations [0.0 (2D) - 1.0 (3D)].
            /// </summary>
            [PublicAPI]
            public static float SpatialBlend
            {
                get => _spatialBlend;
                set => _spatialBlend = Mathf.Clamp01(value);
            }

            /// <summary>
            ///     Doppler scale for 3D spatialisation [0.0 - 5.0].<br />
            ///     Used in 3D spatialisation calculations.
            /// </summary>
            [PublicAPI]
            public static float Doppler
            {
                get => _doppler;
                set => _doppler = Mathf.Clamp(value, 0, 5);
            }

            /// <summary>
            ///     Spread angle (in degrees) of a 3D stereo or multichannel sound in speaker space [0.0 - 360.0].<br />
            ///     Used in 3D spatialisation calculations.
            /// </summary>
            [PublicAPI]
            public static float Spread
            {
                get => _spread;
                set => _spread = Mathf.Clamp(value, 0, 360);
            }

            /// <summary>
            ///     How the audio source attenuates over distance.<br />
            ///     Used in 3D spatialisation calculations.
            /// </summary>
            [PublicAPI]
            public static AudioRolloffMode RolloffMode { get; set; }

            /// <summary>
            ///     Within the minimum distance the audio source will cease to grow louder in volume.<br />
            ///     Used in 3D spatialisation calculations.
            /// </summary>
            [PublicAPI]
            public static float MinDistance
            {
                get => _minDistance;
                set => _minDistance = Mathf.Max(value, 0f);
            }

            /// <summary>
            ///     Logarithmic rolloff: Distance at which the sound stops attenuating.<br />
            ///     Linear rolloff: Distance at which the sound is completely inaudible.<br />
            ///     Used in 3D spatialisation calculations.
            /// </summary>
            [PublicAPI]
            public static float MaxDistance
            {
                get => _maxDistance;
                set => _maxDistance = Mathf.Max(value, 0f);
            }

            #endregion
        }

        /// <summary>
        ///     Settings for manipulating the music playback (similar to AudioSource).
        /// </summary>
        [PublicAPI]
        public static class MusicPlayback
        {
            #region Static Fields and Constants

            #region Static fields

            private static float _targetVolume = 1f;

            #endregion

            #endregion

            #region Static properties

            /// <summary>
            ///     Group that the music playback should output to.
            /// </summary>
            [PublicAPI] [CanBeNull]
            public static AudioMixerGroup Output
            {
                get => _musicPlayback.outputAudioMixerGroup;
                set => _musicPlayback.outputAudioMixerGroup = value;
            }

            /// <summary>
            ///     Volume of the music playback outside of cross-fade transitions [0.0 - 1.0].
            /// </summary>
            [PublicAPI]
            public static float Volume
            {
                get => _targetVolume;
                set
                {
                    if (IsMusicFading)
                    {
                        _targetVolume = Mathf.Clamp01(value);
                        return;
                    }

                    _musicPlayback.volume = _musicFader.volume = _targetVolume = Mathf.Clamp01(value);
                }
            }

            /// <summary>
            ///     Whether the music playback is muted.
            /// </summary>
            [PublicAPI]
            public static bool IsMuted
            {
                get => _musicPlayback.mute;
                set => _musicPlayback.mute = _musicFader.mute = value;
            }

            /// <summary>
            ///     Pitch of the music playback [-3.0 - 3.0].
            /// </summary>
            [PublicAPI]
            public static float Pitch
            {
                get => _musicPlayback.pitch;
                set => _musicPlayback.pitch = Mathf.Clamp(value, -3f, 3f);
            }

            /// <summary>
            ///     Priority of the music playback [0 - 256].
            /// </summary>
            [PublicAPI]
            public static int Priority
            {
                get => _musicPlayback.priority;
                set => _musicPlayback.priority = _musicFader.priority = Mathf.Clamp(value, 0, 256);
            }

            /// <summary>
            ///     Pan the location of a stereo or mono music playback [-1.0 (left) - 1.0 (right)].
            /// </summary>
            [PublicAPI]
            public static float StereoPan
            {
                get => _musicPlayback.panStereo;
                set => _musicPlayback.panStereo = Mathf.Clamp(value, -1f, 1f);
            }

            /// <summary>
            ///     Music playback position in seconds.
            /// </summary>
            [PublicAPI]
            public static float Time
            {
                get => _musicPlayback.time;
                set => _musicPlayback.time = Mathf.Max(0f, value);
            }

            /// <summary>
            ///     Music playback position in PCM samples.
            /// </summary>
            [PublicAPI]
            public static int TimeSamples
            {
                get => _musicPlayback.timeSamples;
                set => _musicPlayback.timeSamples = Mathf.Max(0, value);
            }

            #endregion
        }

        [AddComponentMenu("")]
        private sealed class DummyMono : MonoBehaviour
        {
        }

        #endregion
    }
}