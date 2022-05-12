using JetBrains.Annotations;
using UnityEngine;

namespace DavidFDev.Audio
{
    /// <summary>
    ///     Settings used to calculate 3D spatialisation.
    /// </summary>
    [CreateAssetMenu(menuName = "DavidFDev/Audio/Spatial Audio Settings")]
    public sealed class SpatialAudioSettings : ScriptableObject
    {
        #region Enums

        [PublicAPI]
        public enum SpatialRolloffMode
        {
            [InspectorName("Logarithmic Rolloff")]
            Logarithmic,

            [InspectorName("Linear Rolloff")]
            Linear
        }

        #endregion

        #region Serialized Fields

        [Tooltip("Doppler scale for 3D spatialisation [0.0 - 5.0].")]
        [SerializeField]
        [Range(0f, 5f)]
        private float dopplerLevel;

        [Tooltip("Spread angle (in degrees) of a 3D stereo or multichannel sound in speaker space [0.0 - 360.0].")]
        [SerializeField]
        [Range(0f, 360f)]
        private float spread;

        [Tooltip("How the audio source attenuates over distance.")]
        [SerializeField]
        private SpatialRolloffMode rolloffMode;

        [Tooltip("Within the minimum distance the audio source will cease to grow louder in volume.")]
        [SerializeField]
        [Min(0f)]
        private float minDistance;

        [Tooltip(
            "Logarithmic rolloff: Distance at which the sound stops attenuating.\nLinear rolloff: Distance at which the sound is completely inaudible.")]
        [SerializeField]
        [Min(0f)]
        private float maxDistance;

        #endregion

        #region Properties

        /// <summary>
        ///     Doppler scale for 3D spatialisation [0.0 - 5.0].
        /// </summary>
        [PublicAPI]
        public float Doppler
        {
            get => dopplerLevel;
            set => dopplerLevel = Mathf.Clamp(value, 0, 5);
        }

        /// <summary>
        ///     Spread angle (in degrees) of a 3D stereo or multichannel sound in speaker space [0.0 - 360.0].
        /// </summary>
        [PublicAPI]
        public float Spread
        {
            get => spread;
            set => spread = Mathf.Clamp(value, 0, 360);
        }

        /// <summary>
        ///     How the audio source attenuates over distance.
        /// </summary>
        [PublicAPI]
        public SpatialRolloffMode RolloffMode
        {
            get => rolloffMode;
            set => rolloffMode = value;
        }

        /// <summary>
        ///     Within the minimum distance the audio source will cease to grow louder in volume.
        /// </summary>
        [PublicAPI]
        public float MinDistance
        {
            get => minDistance;
            set => minDistance = Mathf.Max(value, 0f);
        }

        /// <summary>
        ///     Logarithmic rolloff: Distance at which the sound stops attenuating.<br />
        ///     Linear rolloff: Distance at which the sound is completely inaudible.<br />
        ///     Used in 3D spatialisation calculations.
        /// </summary>
        [PublicAPI]
        public float MaxDistance
        {
            get => maxDistance;
            set => maxDistance = Mathf.Max(value, 0f);
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            Reset();
        }

        private void Reset()
        {
            dopplerLevel = 1f;
            minDistance = 1f;
            maxDistance = 500f;
        }

        #endregion
    }
}