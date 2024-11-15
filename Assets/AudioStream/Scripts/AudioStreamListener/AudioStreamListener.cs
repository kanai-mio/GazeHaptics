// (c) 2016-2024 Martin Cvengros. All rights reserved. Redistribution of source code without permission not allowed.
// uses FMOD by Firelight Technologies Pty Ltd

using AudioStreamSupport;
using FMOD;
using System;
using System.Collections;
using UnityEngine;

namespace AudioStream
{
    [RequireComponent(typeof(AudioSource))]
    public partial class AudioStreamListener : ABase
    {
        public AudioStreamListenerAudioSource[] audioStreamListenerAudioSources;
        /// <summary>
        /// all sounds on master group
        /// </summary>
        ChannelGroup reMaster;

        [Tooltip("FMOD channel volume - independent of Unity AudioSource")]
        [Range(0f,1f)]
        public float masterVolume = 1f;

        protected override IEnumerator OnStart()
        {
#if AUDIOSTREAM_IOS_DEVICES
            // AllowBluetooth is only valid with AVAudioSessionCategoryRecord and AVAudioSessionCategoryPlayAndRecord
            // DefaultToSpeaker is only valid with AVAudioSessionCategoryPlayAndRecord
            // So user has 'Prepare iOS for recording' and explicitely enable it here
            AVAudioSessionWrapper.UpdateAVAudioSession(false, true);

            // be sure to wait until session change notification
            while (!AVAudioSessionWrapper.IsSessionReady())
                yield return null;
#endif
            // create audio pump
            var @as = this.GetComponent<AudioSource>();
            @as.clip = null;
            @as.loop = true;
            @as.Stop(); // make sure it's stopped before FMOD sounds are created ('Play On Awake')

            /*
             * Create and init required FMOD system
             */
            /*
             * Resonance Listener DSP seems to be required to be unique per system => in order to play multiple Resonance sounds on the same output
             * we also have to have separate FMOD system objects
             * TODO / Q: is there DSP chain which ensures the played sounds relative to system listener are independent from each other
             */
            this.fmodsystem = 
                // FMOD_SystemW.FMOD_System_Create(this.outputDriverID, true, this.logLevel, this.gameObjectName, this.OnError, out var fmDspBufferLength, out var fmDspBufferCount)
                new FMOD_SystemW.FMOD_System(this.OutputDriverID, true, this.logLevel, this.gameObjectName, this.OnError, out var dspBufferLength, out var dspNumBUffers)
                ;

            this.fmodVersion = this.fmodsystem.VersionString;

            LOG(LogLevel.INFO, "FMOD samplerate: {0}, speaker mode: {1}, num. of raw speakers: {2}", this.fmodsystem.output_sampleRate, this.fmodsystem.output_speakerMode, this.fmodsystem.output_numOfRawSpeakers);

            // wait for FMDO to catch up to be safe if requested to play immediately [i.e. autoStart]
            int numDrivers;
            int retries = 0;

            do
            {
                result = fmodsystem.system.getNumDrivers(out numDrivers);
                ERRCHECK(result, "fmodsystem.system.getNumDrivers");

                LOG(LogLevel.INFO, "Got {0} driver/s available", numDrivers);

                if (++retries > 500)
                {
                    var msg = string.Format("There seems to be no audio output device connected");

                    ERRCHECK(FMOD.RESULT.ERR_OUTPUT_NODRIVERS, msg, false);

                    yield break;
                }

                yield return null;

            } while (numDrivers < 1);

            // || Start_Post
            this.ready = true;

            result = this.fmodsystem.system.getMasterChannelGroup(out this.reMaster);
                // this.fmodsystem.system.createChannelGroup(this.GetInstanceID().ToString(), out this.reMaster)
                // ;
            ERRCHECK(result
                , "system.getMasterChannelGroup"
                // , "system.createChannelGroup"
                );

            // . populate output structure .
            // this.SetOutput(this.outputDriverID);

            // array content is fixed
            var sourcesCount = this.audioStreamListenerAudioSources.Length;

            if (this.audioStreamListenerAudioSources.Length < 1)
                this.LOG(LogLevel.WARNING, "There are no source audio GameObjects of type 'AudioStreamListenerAudioSource' specified on this component" +
                    " - assign them to field 'AudioStreamListenerAudioSources', otherwise it will produce no sound");

            for (var i = 0; i < sourcesCount; ++i)
            {
                if (!this.audioStreamListenerAudioSources[i])
                    this.LOG(LogLevel.ERROR, "Source audio object {0} is not assigned - will not play/be spatialized if it's in scene", i);
            }

            this.reSounds = new FMOD.Sound[sourcesCount];
            this.reChannels = new FMOD.Channel[sourcesCount];

            var unitySampleRate = AudioSettings.outputSampleRate;
            var unityChannels = UnityAudio.ChannelsFromUnityDefaultSpeakerMode();

            for (var i = 0; i < sourcesCount; ++i)
                // create user sound
                this.CreateAndPlayUserSound(i, unitySampleRate, unityChannels);

            // load resonance + dsps
            this.last_relative_positions = new Vector3[sourcesCount];
            this.last_abs_positions = new Vector3[sourcesCount];

            for (var i = 0; i < sourcesCount; ++i)
            {
                if (!this.audioStreamListenerAudioSources[i])
                    continue;

                this.last_relative_positions[i] = this.audioStreamListenerAudioSources[i].transform.position - this.transform.position;
                this.last_abs_positions[i] = this.audioStreamListenerAudioSources[i].transform.position;
            }

            this.ResLoad();

            @as.Play();
        }

        void Update()
        {
            this.fmodsystem.Update();

            // update source positions, and parameters for resonance plugin
            if (this.resonancePlugin != null
                )
            {
                var sourcesCount = this.audioStreamListenerAudioSources.Length;
                for (var i = 0; i < sourcesCount; ++i)
                {
                    var resonanceSource_DSP = this.resonanceSource_DSPs[i];
                    if (!resonanceSource_DSP.hasHandle())
                        continue;

                    // The position of the sounds relative to the listener.
                    var source = this.audioStreamListenerAudioSources[i];
                    if (!source)
                        continue;

                    Vector3 rel_position = source.transform.position - this.transform.position;
                    Vector3 rel_velocity = rel_position - this.last_relative_positions[i];
                    this.last_relative_positions[i] = rel_position;

                    // The position of the sound in world coordinates.
                    Vector3 abs_position = source.transform.position;
                    Vector3 abs_velocity = abs_position - this.last_abs_positions[i];
                    this.last_abs_positions[i] = abs_position;

                    this.resonancePlugin.ResonanceSource_SetGain(source.gain, resonanceSource_DSP);
                    this.resonancePlugin.ResonanceSource_SetSpread(source.spread, resonanceSource_DSP);
                    this.resonancePlugin.ResonanceSource_SetDistanceRolloff(source.distanceRolloff, resonanceSource_DSP);
                    this.resonancePlugin.ResonanceSource_SetOcclusion(source.occlusion, resonanceSource_DSP);
                    this.resonancePlugin.ResonanceSource_SetDirectivity(source.directivity, resonanceSource_DSP);
                    this.resonancePlugin.ResonanceSource_SetDirectivitySharpness(source.directivitySharpness, resonanceSource_DSP);
                    this.resonancePlugin.ResonanceSource_SetAttenuationRange(source.attenuationRangeMin, source.attenuationRangeMax, resonanceSource_DSP);

                    // pannerAttributes.relative is the emitter position and orientation transformed into the listener's space:
                    this.resonancePlugin.ResonanceSource_Set3DAttributes(
                        this.transform.InverseTransformPoint(source.transform.position)
                        , rel_velocity
                        , this.transform.InverseTransformDirection(source.transform.forward)
                        , this.transform.InverseTransformDirection(source.transform.up)
                        , abs_position
                        , abs_velocity
                        , source.transform.forward
                        , source.transform.up
                        , resonanceSource_DSP
                        );

                    this.resonancePlugin.ResonanceSource_SetBypassRoom(source.bypassRoom, resonanceSource_DSP);
                    this.resonancePlugin.ResonanceSource_SetNearFieldFX(source.nearFieldEffects, resonanceSource_DSP);
                    this.resonancePlugin.ResonanceSource_SetNearFieldGain(source.nearFieldGain, resonanceSource_DSP);
                    this.resonancePlugin.ResonanceSource_SetOverallGain(source.overallLinearGain, source.overallLinearGainAdditive, resonanceSource_DSP);

                    this.reChannels[i].setVolume(this.masterVolume);
                }
            }
        }
#if ENABLE_IL2CPP
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
        void OnAudioFilterRead(float[] data, int channels)
        {
            var dlength = data.Length;

            // Debug.Log("OnAudioFilterRead " + dlength);
            // project audio best latency stereo => dlength == 512

            // array content is fixed
            var sourcesCount = this.audioStreamListenerAudioSources.Length;

            for (var i = 0; i < sourcesCount; ++i)
            {
                var source = this.audioStreamListenerAudioSources[i];
                if (source && source.captureBuffer != null)
                {
                    var cp = source.captureBuffer;
                    if (cp.Length == dlength)
                    {
                        if (this.reSounds[i].hasHandle())
                        {
                            var key = this.reSounds[i].handle;
                            byte[] bArr = new byte[dlength * sizeof(float)];
                            Buffer.BlockCopy(cp, 0, bArr, 0, bArr.Length);

                            if (AudioStreamListener.reSounds_CBs.ContainsKey(key))
                                AudioStreamListener.reSounds_CBs[key].Enqueue(bArr);
                        }
                    }
                }
            }
        }

        void OnDestroy()
        {
            // unload resonance + dsps
            // was even started
            if (this.resonancePlugin != null)
            {
                // remove DSPs first (if e.g. changing scene directly)
                this.ResUnload();
            }

            // release user sounds
            // array content is fixed
            var sourcesCount = this.audioStreamListenerAudioSources.Length;
            for (var i = 0; i < sourcesCount; ++i)
                this.StopAndReleaseUserSound(i);

            //foreach (var chgrp in this.reChannels)
            //{
            //    result = chgrp.stop();
            //    ERRCHECK(result, "reMaster.stop", false);

            //    result = chgrp.release();
            //    ERRCHECK(result, "reMaster.release", false);
            //}

            // release system
            FMOD_SystemW.FMOD_System_Release(ref this.fmodsystem, this.logLevel, this.gameObjectName, this.OnError);
        }
    }
}