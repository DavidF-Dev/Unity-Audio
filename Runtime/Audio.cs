// File: Audio.cs
// Purpose: Static class for playing audio (clips, sound effects and music).
// Created by: DavidFDev

//#define HIDE_IN_EDITOR
#define DEBUG_AUDIO

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace DavidFDev.Audio
{
    /// <summary>
    ///     Play audio clips, sound effects and music.
    /// </summary>
    public static class Audio
    {
        #region Static fields

        private static Transform _parent;

        private static readonly Dictionary<AudioSource, Playback> _current = new Dictionary<AudioSource, Playback>();

        private static readonly Stack<AudioSource> _available = new Stack<AudioSource>();

        private static readonly Dictionary<string, AudioClip> _cachedClips = new Dictionary<string, AudioClip>();

        private static readonly Dictionary<string, SoundEffect> _cachedAssets = new Dictionary<string, SoundEffect>();

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

        public static AudioSource PlayMusic(AudioClip music, float fadeTime = 0.25f, AudioMixerGroup output = null)
        {
            throw new NotImplementedException();
        }

        public static AudioSource PlayMusic(string path, float fadeTime = 0.25f, AudioMixerGroup output = null)
        {
            return PlayMusic(TryGetClipFromResource(path), fadeTime, output);
        }

        /// <summary>
        ///     Stop all audio playbacks, freeing up resources.
        /// </summary>
        /// <param name="destroyObjects">Destroy pooled game objects.</param>
        public static void StopAllAudio(bool destroyObjects = false)
        {
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            // Create the object that will hold the audio sources
            _parent = new GameObject("Audio Pool").transform;
            UnityEngine.Object.DontDestroyOnLoad(_parent.gameObject);
#if HIDE_IN_EDITOR
            _parent.gameObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy; 
#endif

#if DEBUG_AUDIO
            Debug.Log("Created audio pool game object.");
#endif
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

        #endregion
    }
}