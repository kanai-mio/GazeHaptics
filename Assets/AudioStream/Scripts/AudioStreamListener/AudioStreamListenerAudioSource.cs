// (c) 2016-2024 Martin Cvengros. All rights reserved. Redistribution of source code without permission not allowed.
// uses FMOD by Firelight Technologies Pty Ltd

using System.Collections;
using UnityEngine;

namespace AudioStream
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioStreamListenerAudioSource : ABase
    {
        // ========================================================================================================================================
        #region Init base
        protected override IEnumerator OnStart()
        {
            yield return null;

            var @as = this.GetComponent<AudioSource>();
            @as.loop = true;
            @as.Play();
        }
        #endregion
        // ========================================================================================================================================
        #region Resonance parameters
        [Header("[ Resonance 3D Source ]")]

        [Range(-80f, 24f)]
        [Tooltip("Gain")]
        public float gain = 0f;

        [Range(0f, 360f)]
        [Tooltip("Spread")]
        public float spread = 0f;

        [Tooltip("rolloff")]
        public ResonancePlugin.DistanceRolloff distanceRolloff = ResonancePlugin.DistanceRolloff.LOGARITHMIC;

        [Range(0f, 10f)]
        [Tooltip("occlusion")]
        public float occlusion = 0f;

        // very narrow forward oriented cone for testing
        // directivity          -   0.8 -   forward cone only
        // directivitySharpness -   10  -   narrow focused cone

        [Range(0f, 1f)]
        [Tooltip("directivity")]
        public float directivity = 0f;

        [Range(1f, 10f)]
        [Tooltip("directivity sharpness")]
        public float directivitySharpness = 1f;

        [Range(0f, 10000f)]
        [Tooltip("Attenuation Range Min")]
        public float attenuationRangeMin = 1f;

        [Range(0f, 10000f)]
        [Tooltip("Attenuation Range Max")]
        public float attenuationRangeMax = 500f;

        [Tooltip("Room is not fully implemented. If OFF a default room will be applied resulting in slight reverb")]
        public bool bypassRoom = true;

        [Tooltip("Enable Near-Field Effects")]
        public bool nearFieldEffects = false;

        [Range(0f, 9f)]
        [Tooltip("Near-Field Gain")]
        public float nearFieldGain = 1f;

        [Range(-80f, 24f)]
        [Tooltip("Overall Gain")]
        public float overallLinearGain = 0f;

        [Range(-80f, 24f)]
        [Tooltip("Overall Gain")]
        public float overallLinearGainAdditive = 0f;
        #endregion
        // ========================================================================================================================================
        #region source audio
        [Header("[ Unity monitor ]")]
        [Tooltip("Post volume (doesn't affect output audio)")]
        [Range(0f, 1f)]
        public float monitoringVolume = 1f;
        [Header("[ AudioStreamSourceListener ]")]
        public float[] captureBuffer = null;
#if ENABLE_IL2CPP
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
        void OnAudioFilterRead(float[] data, int channels)
        {
            var dlength = data.Length;

            if (this.captureBuffer == null
                || this.captureBuffer.Length != dlength
                )
                this.captureBuffer = new float[dlength];

            System.Array.Copy(data, 0, this.captureBuffer, 0, dlength);

            for (var i = 0; i < dlength; ++i)
                data[i] *= this.monitoringVolume;
        }
        #endregion
    }
}