// File: Audio.cs
// Purpose: Static class for playing audio (clips, sound effects and music).
// Created by: DavidFDev

#define HIDE_IN_EDITOR
//#define DEBUG_AUDIO

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace DavidFDev.Audio
{
    /// <summary>
    ///     Play audio clips, sound effects and music. Also provides other helpful audio methods.
    /// </summary>
    public static class Audio
    {
        #region Static fields

        private static Transform _parent;

        private static MonoBehaviour _mono;

        private static readonly Dictionary<AudioSource, Playback> _current = new Dictionary<AudioSource, Playback>();

        private static readonly Stack<AudioSource> _available = new Stack<AudioSource>();

        private static readonly Dictionary<string, AudioClip> _cachedClips = new Dictionary<string, AudioClip>();

        private static readonly Dictionary<string, SoundEffect> _cachedAssets = new Dictionary<string, SoundEffect>();

        private static AudioSource _musicPlayback;

        private static AudioSource _musicFader;

        private static Coroutine _musicFadeIn;

        private static Coroutine _musicFadeOut;

        #endregion

        #region Static properties

        /// <summary>
        ///     Current audio clip used by the music playback.
        /// </summary>
        [Pure]
        public static AudioClip CurrentMusic
        {
            get => _musicPlayback.clip;
        }

        /// <summary>
        ///     Whether the music playback is currently fading between two tracks.
        /// </summary>
        [Pure]
        public static bool IsMusicFading
        {
            get => _musicFadeIn != null || _musicFadeOut != null;
        }

        #endregion

        #region Static methods

        /// <summary>
        ///     Play an audio clip.
        /// </summary>
        /// <param name="clip">Audio clip to play.</param>
        /// <param name="position">Position of the audio playback in 3D world-space.</param>
        /// <param name="output">Group that the audio playback should output to.</param>
        /// <returns>Playback instance for controlling the audio.</returns>
        public static Playback Play(AudioClip clip, Vector3 position = default, AudioMixerGroup output = null)
        {
            if (clip == null)
            {
                throw new ArgumentNullException(nameof(clip));
            }

            AudioSource source = GetAudioSource();
            source.clip = clip;

#if !HIDE_IN_EDITOR
            source.gameObject.name = $"Audio Source ({clip.name})";
#endif

            // Set defaults
            Playback playback = _current[source];
            playback.Output = output;
            playback.Volume = PlaybackDefaults.Volume;
            playback.IsMuted = false;
            playback.Pitch = PlaybackDefaults.Pitch;
            playback.Loop = false;
            playback.Priority = 128;
            playback.StereoPan = 0f;
            playback.SpatialBlend = PlaybackDefaults.SpatialBlend;
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
        public static Playback Play(string path, Vector3 position = default, AudioMixerGroup output = null)
        {
            return Play(TryGetClipFromResource(path), position, output);
        }

        /// <summary>
        ///     Play a sound effect.
        /// </summary>
        /// <param name="asset">Sound effect to play.</param>
        /// <param name="position">Position of the audio playback in 3D world-space.</param>
        /// <returns>Playback instance for controlling the audio.</returns>
        public static Playback PlaySfx(SoundEffect asset, Vector3 position = default)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            Playback playback = Play(asset.GetClipAtRandom(), position, asset.Output);
            playback.Volume = UnityEngine.Random.Range(asset.MinVolume, asset.MaxVolume);
            playback.Pitch = UnityEngine.Random.Range(asset.MinPitch, asset.MaxPitch);
            playback.Loop = asset.Loop;
            playback.Priority = asset.Priority;
            playback.StereoPan = asset.StereoPan;
            playback.SpatialBlend = asset.SpatialBlend;
            return playback;
        }

        /// <summary>
        ///     Play a sound effect loaded from a resource.
        /// </summary>
        /// <param name="path">Path to the sound effect to play.</param>
        /// <param name="position">Position of the audio playback in 3D world-space.</param>
        /// <returns>Playback instance for controlling the audio.</returns>
        public static Playback PlaySfx(string path, Vector3 position = default)
        {
            return PlaySfx(TryGetAssetFromResource(path), position);
        }

        /// <summary>
        ///     Play a music track. If a music track is already playing, it will be faded out.
        ///     <para>Note: .mp3 files are known to cause popping sounds in Unity under certain circumstances.</para>
        /// </summary>
        /// <param name="music">Music to play.</param>
        /// <param name="fadeIn">Duration, in seconds, that the music should take to fade in.</param>
        /// <param name="fadeOut">Duration, in seconds, that the old music should take to fade out.</param>
        public static void PlayMusic(AudioClip music, float fadeIn = 1f, float fadeOut = 0.75f)
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

                _musicFadeOut = _mono.StartCoroutine(SimpleLerp(_musicPlayback.volume, 0f, fadeOut, Mathf.Lerp, x => _musicFader.volume = x, () => { _musicFadeOut = null; _musicFader.Stop(); _musicFader.clip = null; }));

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

                _musicFadeIn = _mono.StartCoroutine(SimpleLerp(0f, MusicPlayback.Volume, fadeIn, Mathf.Lerp, x => _musicPlayback.volume = x, () => _musicFadeIn = null));

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
        public static void PlayMusic(string path, float fadeIn = 1f, float fadeOut = 0.75f)
        {
            AudioClip music = TryGetClipFromResource(path);

            if (music == null)
            {
                throw new Exception($"Failed to load the music track at {path}.");
            }

            PlayMusic(music, fadeIn, fadeOut);
        }

        /// <summary>
        ///     Stop music playback.
        /// </summary>
        /// <param name="fadeOut">Duration, in seconds, that the music should take to fade out.</param>
        public static void StopMusic(float fadeOut = 0.75f)
        {
            PlayMusic((AudioClip)null, 0f, fadeOut);
        }

        /// <summary>
        ///     Stop all audio playbacks, freeing up resources.
        /// </summary>
        /// <param name="stopMusic">Stop music playback.</param>
        /// <param name="destroyObjects">Destroy pooled game objects.</param>
        public static void StopAllAudio(bool stopMusic = false, bool destroyObjects = false)
        {
            if (stopMusic)
            {
                StopMusic(0f);
            }

            if (destroyObjects)
            {
                foreach (AudioSource source in _current.Keys)
                {
                    UnityEngine.Object.Destroy(source.gameObject);
                }

                _current.Clear();

                while (_available.Any())
                {
                    UnityEngine.Object.Destroy(_available.Pop());
                }

#if DEBUG_AUDIO
                Debug.Log("Destroyed all audio sources.");
#endif

                return;
            }

            // Release all the in-use sources back into the pool
            foreach (AudioSource source in _current.Keys)
            {
                _current[source].ForceFinish();
                _current[source].Dispose();
                _available.Push(source);
            }

            _current.Clear();

#if DEBUG_AUDIO
            Debug.Log("Freed all audio sources back into the pool.");
#endif
        }

        /// <summary>
        ///     Clear cached audio clips and sound effect assets, freeing up memory.
        /// </summary>
        public static void ClearCache()
        {
            _cachedClips.Clear();
            _cachedAssets.Clear();
        }

        /// <summary>
        ///     Get an attenuation (volume) scaled logarithmically.
        ///     Use this instead of a linear scale - decibels do not scale linearly!
        /// </summary>
        /// <param name="volume01">
        ///     0.0 returns -80.0db (practically silent).
        ///     <para>0.5 returns approximately -14.0db (half volume).</para>
        ///     <para>1.0 returns 0.0db (full volume - no gain).</para>
        /// </param>
        /// <returns>Attenuation (volume) in decibals.</returns>
        [Pure]
        public static float GetAttenuation(float volume01)
        {
            volume01 = Mathf.Clamp01(volume01);
            return volume01 == 0f ? -80f : (Mathf.Log(volume01) * 20f);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            // Create the object that will hold the audio sources
            _parent = new GameObject("Audio Pool").transform;
            _mono = _parent.gameObject.AddComponent<DummyMono>();
            UnityEngine.Object.DontDestroyOnLoad(_parent.gameObject);
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

        private static AudioSource GetAudioSource()
        {
            AudioSource source;

            // Check for an available audio source in the pool
            if (_available.Any())
            {
                source = _available.Pop();

#if DEBUG_AUDIO
                Debug.Log("Retrieved available audio source from the pool.");
#endif
            }

            // Otherwise, search for a finished audio source
            else if ((source = _current.FirstOrDefault(x => x.Value.IsFinished).Key) != null)
            {
                // Get the source ready for re-use
                _current[source].Dispose();
                _current.Remove(source);

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

            _current.Add(source, new Playback(source));
            source.gameObject.SetActive(true);

            return source;
        }

        private static AudioClip TryGetClipFromResource(string path)
        {
            // Check if the clip has been cached for easy retrieval
            if (_cachedClips.TryGetValue(path, out AudioClip clip))
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
                throw new Exception($"Failed to load AudioClip at {path}");
            }

#if DEBUG_AUDIO
            Debug.Log("Loaded audio clip from resources.");
#endif

            // Cache
            _cachedClips.Add(path, clip);

            return clip;
        }

        private static SoundEffect TryGetAssetFromResource(string path)
        {
            // Check if the asset has been cached for easy retrieval
            if (_cachedAssets.TryGetValue(path, out SoundEffect asset))
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
                throw new Exception($"Failed to load {nameof(SoundEffect)} at {path}");
            }

#if DEBUG_AUDIO
            Debug.Log("Loaded asset from resources.");
#endif

            // Cache
            _cachedAssets.Add(path, asset);

            return asset;
        }

        private static IEnumerator SimpleLerp<T>(T start, T end, float duration, Func<T, T, float, T> lerpFunction, Action<T> onUpdate, Action onComplete)
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
        public static class PlaybackDefaults
        {
            #region Static fields

            private static float _volume = 1f;

            private static float _pitch = 1f;

            private static float _spatialBlend;

            #endregion

            #region Static properties

            /// <summary>
            ///     Volume of the audio playback [0.0 - 1.0].
            /// </summary>
            public static float Volume
            {
                get => _volume;
                set => _volume = Mathf.Clamp01(value);
            }

            /// <summary>
            ///     Pitch of the audio playback [-3.0 - 3.0].
            /// </summary>
            public static float Pitch
            {
                get => _pitch;
                set => _pitch = Mathf.Clamp(value, -3f, 3f);
            }

            /// <summary>
            ///     Amount that the audio playback is affected by spatialisation calculations [0.0 (2D) - 1.0 (3D)].
            /// </summary>
            public static float SpatialBlend
            {
                get => _spatialBlend;
                set => _spatialBlend = Mathf.Clamp01(value);
            }

            #endregion
        }

        /// <summary>
        ///     Settings for manipulating the music playback (similar to AudioSource).
        /// </summary>
        public static class MusicPlayback
        {
            #region Static fields

            private static float _targetVolume = 1f;

            #endregion

            #region Static properties

            /// <summary>
            ///     Group that the music playback should output to.
            /// </summary>
            public static AudioMixerGroup Output
            {
                get => _musicPlayback.outputAudioMixerGroup;
                set => _musicPlayback.outputAudioMixerGroup = value;
            }

            /// <summary>
            ///     Volume of the music playback outside of crossfade transitions [0.0 - 1.0].
            /// </summary>
            public static float Volume
            {
                get => _targetVolume;
                set
                {
                    if (IsMusicFading)
                    {
                        return;
                    }

                    _musicPlayback.volume = _musicFader.volume = _targetVolume = Mathf.Clamp01(value);
                }
            }

            /// <summary>
            ///     Whether the music playback is muted.
            /// </summary>
            public static bool IsMuted
            {
                get => _musicPlayback.mute;
                set => _musicPlayback.mute = _musicFader.mute = value;
            }

            /// <summary>
            ///     Pitch of the music playback [-3.0 - 3.0].
            /// </summary>
            public static float Pitch
            {
                get => _musicPlayback.pitch;
                set => _musicPlayback.pitch = Mathf.Clamp(value, -3f, 3f);
            }

            /// <summary>
            ///     Priority of the music playback [0 - 256].
            /// </summary>
            public static int Priority
            {
                get => _musicPlayback.priority;
                set => _musicPlayback.priority = _musicFader.priority = Mathf.Clamp(value, 0, 256);
            }

            /// <summary>
            ///     Pan the location of a stereo or mono music playback [-1.0 (left) - 1.0 (right)].
            /// </summary>
            public static float StereoPan
            {
                get => _musicPlayback.panStereo;
                set => _musicPlayback.panStereo = Mathf.Clamp(value, -1f, 1f);
            }

            /// <summary>
            ///     Music playback position in seconds.
            /// </summary>
            public static float Time
            {
                get => _musicPlayback.time;
                set => _musicPlayback.time = Mathf.Max(0f, value);
            }

            /// <summary>
            ///     Music playback position in PCM samples.
            /// </summary>
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