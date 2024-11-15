// (c) 2016-2024 Martin Cvengros. All rights reserved. Redistribution of source code without permission not allowed.
// uses FMOD by Firelight Technologies Pty Ltd

using AudioStreamSupport;
using System.Collections;
using UnityEngine;

namespace AudioStream
{
    public class ResonanceSource : AudioStreamBase
    {
        // ========================================================================================================================================
        #region Resonance Plugin + Resonance parameters
        ResonancePlugin resonancePlugin = null;
        FMOD.DSP resonanceSource_DSP;

        [Header("[3D Settings]")]
        [Tooltip("If left empty, main camera transform will be set to be listener at Start")]
        public Transform listener;

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

        /// <summary>
        /// previous positions for velocity
        /// </summary>
		Vector3 last_relative_position = Vector3.zero;
        Vector3 last_abs_position = Vector3.zero;
        #endregion

        // ========================================================================================================================================
        #region Unity lifecycle
        protected override IEnumerator OnStart()
        {
            yield return base.OnStart();

            if (this.listener == null)
                this.listener = Camera.main.transform;

            this.last_relative_position = this.transform.position - this.listener.position;
            this.last_abs_position = this.transform.position;
        }

        void Update()
        {
            // update source position, and parameters for resonance plugin
            if (this.resonancePlugin != null
                && this.resonanceSource_DSP.hasHandle()
                )
            {
                // The position of the sound relative to the listeners.
                Vector3 rel_position = this.transform.position - this.listener.position;
                Vector3 rel_velocity = rel_position - this.last_relative_position;
                this.last_relative_position = rel_position;

                // The position of the sound in world coordinates.
                Vector3 abs_position = this.transform.position;
                Vector3 abs_velocity = abs_position - this.last_abs_position;
                this.last_abs_position = this.transform.position;

                this.resonancePlugin.ResonanceSource_SetGain(this.gain, this.resonanceSource_DSP);
                this.resonancePlugin.ResonanceSource_SetSpread(this.spread, this.resonanceSource_DSP);
                this.resonancePlugin.ResonanceSource_SetDistanceRolloff(this.distanceRolloff, this.resonanceSource_DSP);
                this.resonancePlugin.ResonanceSource_SetOcclusion(this.occlusion, this.resonanceSource_DSP);
                this.resonancePlugin.ResonanceSource_SetDirectivity(this.directivity, this.resonanceSource_DSP);
                this.resonancePlugin.ResonanceSource_SetDirectivitySharpness(this.directivitySharpness, this.resonanceSource_DSP);
                this.resonancePlugin.ResonanceSource_SetAttenuationRange(this.attenuationRangeMin, this.attenuationRangeMax, this.resonanceSource_DSP);

                this.resonancePlugin.ResonanceSource_Set3DAttributes(
                    this.listener.InverseTransformPoint(this.transform.position)
                    , rel_velocity
                    , this.listener.InverseTransformDirection(this.transform.forward)
                    , this.listener.InverseTransformDirection(this.transform.up)
                    , abs_position
                    , abs_velocity
                    , this.transform.forward
                    , this.transform.up
                    , this.resonanceSource_DSP
                    );

                this.resonancePlugin.ResonanceSource_SetBypassRoom(this.bypassRoom, this.resonanceSource_DSP);
                this.resonancePlugin.ResonanceSource_SetNearFieldFX(this.nearFieldEffects, this.resonanceSource_DSP);
                this.resonancePlugin.ResonanceSource_SetNearFieldGain(this.nearFieldGain, this.resonanceSource_DSP);
                this.resonancePlugin.ResonanceSource_SetOverallGain(this.overallLinearGain, this.overallLinearGainAdditive, this.resonanceSource_DSP);
            }
        }

        public override void OnDestroy()
        {
            // was even started
            if (this.resonancePlugin != null)
            {
                // remove DSPs first (if e.g. changing scene directly)
                this.StreamStopping();
            }

            base.OnDestroy();
        }
        #endregion

        // ========================================================================================================================================
        #region AudioStreamBase
        protected override void StreamStarting()
        {
            this.SetOutput(this.outputDriverID);

            //
            // Load Resonance + DSPs
            // has to be just here since it needs a valid system (needs to wait for the base, but base can call record from Start)
            //
            this.resonancePlugin = ResonancePlugin.Load(this.fmodsystem.system, this.logLevel);
            this.resonanceSource_DSP = ResonancePlugin.New_ResonanceSource_DSP(this.fmodsystem.system);
            //
            // Add source DSP
            //
            result = this.channel.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, this.resonanceSource_DSP);
            ERRCHECK(result, "resonanceChannelGroup.addDSP", false);

            this.channel.setVolume(1);
            ERRCHECK(result, "channel.setVolume");
        }

        protected override void StreamStarving()
        {
            fmodsystem.Update();
        }

        protected override void StreamStopping()
        {
            if (this.channel.hasHandle())
            {
                if (this.resonancePlugin != null)
                {
                    result = this.channel.removeDSP(this.resonanceSource_DSP);
                    ERRCHECK(result, "channel.removeDSP", false);

                    result = this.resonanceSource_DSP.release();
                    ERRCHECK(result, "resonanceSource_DSP.release", false);

                    ResonancePlugin.Unload(this.fmodsystem.system);
                    this.resonancePlugin = null;
                }
            }
        }

        protected override void StreamChanged(float samplerate, int channels, FMOD.SOUND_FORMAT sound_format)
        {
            float defFrequency;
            int defPriority;
            result = sound.getDefaults(out defFrequency, out defPriority);
            ERRCHECK(result, "sound.getDefaults", false);

            LOG(LogLevel.INFO, "Stream samplerate change from {0}, {1}", defFrequency, sound_format);

            result = sound.setDefaults(samplerate, defPriority);
            ERRCHECK(result, "sound.setDefaults", false);

            LOG(LogLevel.INFO, "Stream samplerate changed to {0}, {1}", samplerate, sound_format);
        }
        #endregion
    }
}