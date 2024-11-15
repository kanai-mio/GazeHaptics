// (c) 2016-2024 Martin Cvengros. All rights reserved. Redistribution of source code without permission not allowed.
// uses FMOD by Firelight Technologies Pty Ltd

using FMOD;
using UnityEngine;

namespace AudioStream
{
    public partial class AudioStreamListener : ABase
    {
        // ========================================================================================================================================
        #region Resonance Plugin
        ResonancePlugin resonancePlugin = null;
        FMOD.DSP[] resonanceSource_DSPs;
        // static FMOD.DSP resonanceListener_DSP;
        /// <summary>
        /// previous positions for velocity
        /// </summary>
		Vector3[] last_relative_positions = null;
        Vector3[] last_abs_positions = null;
        #endregion

        void ResLoad()
        {
            //
            // Load Resonance + DSPs
            // has to be just here since it needs a valid system (needs to wait for the base, but base can call record from Start)
            //
            this.resonancePlugin = ResonancePlugin.Load(this.fmodsystem.system, this.logLevel);
            //if (!resonanceListener_DSP.hasHandle())
            //    resonanceListener_DSP = ResonancePlugin.New_ResonanceListener_DSP(this.fmodsystem.system);
            this.resonanceSource_DSPs = new DSP[this.audioStreamListenerAudioSources.Length];
            for(var i = 0; i < this.resonanceSource_DSPs.Length; ++i)
                this.resonanceSource_DSPs[i] = ResonancePlugin.New_ResonanceSource_DSP(this.fmodsystem.system);
            //
            // Resonance channels
            //
            var sourcesCount = this.audioStreamListenerAudioSources.Length;
            for(var i = 0; i < sourcesCount; ++i)
            {
                result = this.reChannels[i].addDSP(CHANNELCONTROL_DSP_INDEX.FADER, this.resonanceSource_DSPs[i]);
                ERRCHECK(result, "reChannels[i].addDSP", false);
            }

            //result = this.reMaster.addDSP(CHANNELCONTROL_DSP_INDEX.FADER, resonanceListener_DSP);
            //ERRCHECK(result, "reMaster.addDSP", false);
        }

        void ResUnload()
        {
            if (this.resonancePlugin != null)
            {
                var sourcesCount = this.audioStreamListenerAudioSources.Length;
                for (var i = 0; i < sourcesCount; ++i)
                {
                    if (this.reChannels[i].hasHandle())
                    {
                        result = this.reChannels[i].removeDSP(this.resonanceSource_DSPs[i]);
                        ERRCHECK(result, "reChannels[i].removeDSP", false);

                        result = this.resonanceSource_DSPs[i].release();
                        ERRCHECK(result, "resonanceSource_DSP.release", false);

                        this.reChannels[i].clearHandle();
                    }
                }

                //if (resonanceListener_DSP.hasHandle())
                //{
                //    result = this.reMaster.removeDSP(resonanceListener_DSP);
                //    ERRCHECK(result, "reMaster.removeDSP", false);

                //    result = resonanceListener_DSP.release();
                //    ERRCHECK(result, "resonanceListener_DSP.release", false);

                //    resonanceListener_DSP.clearHandle();
                //}

                ResonancePlugin.Unload(this.fmodsystem.system);
                this.resonancePlugin = null;
            }
        }
    }
}