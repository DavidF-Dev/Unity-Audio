using UnityEngine;

namespace DavidFDev.Audio
{
    [CreateAssetMenu()]
    public sealed class SoundEffect : ScriptableObject
    {
        #region Properties

        [field: SerializeField]
        public AudioClip[] Clips { get; private set; }

        [field: Header("Volume"), SerializeField, Range(0f, 1f)]
        public float MinVolume { get; private set; } = 1f;

        [field: SerializeField, Range(0f, 1f)]
        public float MaxVolume { get; private set; } = 1f;

        [field: Header("Pitch"), SerializeField, Range(-3f, 3f)]
        public float MinPitch { get; private set; } = 1f;

        [field: SerializeField, Range(-3f, 3f)]
        public float MaxPitch { get; private set; } = 1f;

        [field: Space, SerializeField]
        public bool Loop { get; private set; }

        #endregion
    }
}