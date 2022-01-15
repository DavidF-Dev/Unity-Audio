﻿//#define HIDE_IN_EDITOR
#define DEBUG_AUDIO

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DavidFDev.Audio
{
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

        public static Playback Play(AudioClip clip)
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
            playback.Output = null;
            playback.Volume = 1f;
            playback.Pitch = 1f;
            playback.Loop = false;
            playback.Priority = 128;
            playback.StereoPan = 0f;
            playback.SpatialBlend = 0f;
            playback.Position = Vector3.zero;
            
            source.Play();

#if DEBUG_AUDIO
            Debug.Log($"Started audio playback for {clip.name}.");
#endif

            return playback;
        }

        public static Playback Play(AudioClip clip, Vector3 position)
        {
            Playback playback = Play(clip);
            playback.Position = position;
            return playback;
        }

        public static Playback Play(string path)
        {
            return Play(TryGetClipFromResource(path));
        }

        public static Playback Play(string path, Vector3 position)
        {
            return Play(TryGetClipFromResource(path), position);
        }

        public static Playback PlaySfx(SoundEffect asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            Playback playback = Play(asset.Clips[UnityEngine.Random.Range(0, asset.Clips.Length)]);
            playback.Output = asset.Output;
            playback.Volume = UnityEngine.Random.Range(asset.MinVolume, asset.MaxVolume);
            playback.Pitch = UnityEngine.Random.Range(asset.MinPitch, asset.MaxPitch);
            playback.Loop = asset.Loop;
            return playback;
        }

        public static Playback PlaySfx(SoundEffect asset, Vector3 position)
        {
            Playback playback = PlaySfx(asset);
            playback.Position = position;
            return playback;
        }

        public static Playback PlaySfx(string path)
        {
            return PlaySfx(TryGetAssetFromResource(path));
        }

        public static Playback PlaySfx(string path, Vector3 position)
        {
            return PlaySfx(TryGetAssetFromResource(path), position);
        }

        public static Playback PlayMusic(AudioClip music)
        {
            throw new NotImplementedException();
        }

        public static void StopAllAudio(bool destroyObjects = false)
        {
            if(destroyObjects)
            {
                foreach(AudioSource source in _current.Keys)
                {
                    UnityEngine.Object.Destroy(source.gameObject);
                }

                _current.Clear();

                while(_available.Any())
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
                _current[source].Dispose();
                _available.Push(source);
            }

            _current.Clear();

#if DEBUG_AUDIO
            Debug.Log("Freed all audio sources back into the pool.");
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#pragma warning disable IDE0051
        private static void Init()
#pragma warning restore IDE0051
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
    }
}