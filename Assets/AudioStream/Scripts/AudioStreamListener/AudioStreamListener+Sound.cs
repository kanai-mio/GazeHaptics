// (c) 2016-2024 Martin Cvengros. All rights reserved. Redistribution of source code without permission not allowed.
// uses FMOD by Firelight Technologies Pty Ltd

using FMOD;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AudioStream
{
    public partial class AudioStreamListener : ABase
    {
        FMOD.Sound[] reSounds;
        FMOD.Channel[] reChannels;
        FMOD.SOUND_PCMREAD_CALLBACK pcmreadcallback;
        FMOD.SOUND_PCMSETPOS_CALLBACK pcmsetposcallback;
        /// <summary>
        /// map
        /// </summary>
        static Dictionary<System.IntPtr, PCMCallbackBuffer> reSounds_CBs = new Dictionary<IntPtr, PCMCallbackBuffer>();
        static object pcm_callback_lock = new object();
        [AOT.MonoPInvokeCallback(typeof(FMOD.SOUND_PCMREAD_CALLBACK))]
#if ENABLE_IL2CPP
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
        static FMOD.RESULT PCMReadCallback(System.IntPtr soundraw, System.IntPtr data, uint datalen)
        {
            lock (AudioStreamListener.pcm_callback_lock)
            {
                // UnityEngine.Debug.LogFormat("PCMReadCallback {0} {1}", soundraw, datalen);

                // clear the array first - fix for non running AudioSource
                var zeroArr = new byte[datalen];
                global::System.Runtime.InteropServices.Marshal.Copy(zeroArr, 0, data, (int)datalen);

                // retrieve sound instance buffer
                if (AudioStreamListener.reSounds_CBs.TryGetValue(soundraw, out var pcmCallbackBuffer))
                {
                    var av = pcmCallbackBuffer.Available;
                    if (datalen <= av)
                    {
                        // UnityEngine.Debug.LogFormat("PCMReadCallback len: {0}, av: {1}", datalen, pcmCallbackBuffer.Available);
                        var bytes = pcmCallbackBuffer.Dequeue(datalen);
                        var bLength = bytes.Length;

                        global::System.Runtime.InteropServices.Marshal.Copy(bytes, 0, data, bLength);
                    }
                    // no point to resize if the feed is steady / empty
                    else if (av > 0)
                    {
                        UnityEngine.Debug.LogWarningFormat("PCMReadCallback adjusting: {0} - {1} av: {2} / {3}", soundraw, datalen, pcmCallbackBuffer.Available, pcmCallbackBuffer.Capacity);
                        pcmCallbackBuffer.Resize();
                    }
                }
            }

            return FMOD.RESULT.OK;
        }
        [AOT.MonoPInvokeCallback(typeof(FMOD.SOUND_PCMSETPOS_CALLBACK))]
#if ENABLE_IL2CPP
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
        static FMOD.RESULT PCMSetPosCallback(System.IntPtr soundraw, int subsound, uint position, FMOD.TIMEUNIT postype)
        {
            /*
            Debug.LogFormat("PCMSetPosCallback sound {0}, subsound {1} requesting position {2}, postype {3}, time: {4}"
                , soundraw
                , subsound
                , position
                , postype
                , AudioSettings.dspTime
                );
                */
            return FMOD.RESULT.OK;
        }

        void CreateAndPlayUserSound(int ofSource
            , int soundSampleRate
            , int soundChannels
            )
        {
            CREATESOUNDEXINFO exinfo = default(CREATESOUNDEXINFO);
            this.pcmreadcallback = new FMOD.SOUND_PCMREAD_CALLBACK(PCMReadCallback);
            this.pcmsetposcallback = new FMOD.SOUND_PCMSETPOS_CALLBACK(PCMSetPosCallback);

            // for Sound + PCMReadCallback to work consistently in streamed mode:
            // required size `datalen` in PCMReadCallback is in bytes
            // derived from CREATESOUNDEXINFO.decodebuffersize samples as:
            // `datalen` == samples * sizeof(float) [sound format] * channels [...]
            //
            // && source/pcmCallbackBuffer capacity >= `datalen` and
            // to satisfy exactly at least one whole PCM callback chunk at all times it needs * 2 [ double bufffer ]

            // (
            // decodebuffersize:
            // https://www.fmod.com/docs/2.03/api/core-api-system.html#fmod_createsoundexinfo 
            // Size of the decoded buffer for FMOD_CREATESTREAM, or the block size used with pcmreadcallback for FMOD_OPENUSER.
            // Units: Samples Default: FMOD_ADVANCEDSETTINGS::defaultDecodeBufferSize based on defaultfrequency.
            // 
            // defaultDecodeBufferSize:
            // https://www.fmod.com/docs/2.03/api/core-api-system.html#fmod_advancedsettings_defaultdecodebuffersize
            // For use with Streams, the default size of the double buffer.
            // Units: Milliseconds Default: 400 Range: [0, 30000]
            //
            // createSound calls back immediately once after it is created
            // )

            // source audio is - Unity - sound on output 0
            var cbBytes = soundSampleRate * sizeof(float) * soundChannels;
            var pcmCallbackBuffer = new PCMCallbackBuffer((uint)cbBytes * 2);

            //var nAvgBytesPerSec = o.channels * o.samplerate * sizeof(float);
            //var msPerSample = 25 / (float)o.channels / 1000f;
            //var decodebuffersize = nAvgBytesPerSec * msPerSample;
            //UnityEngine.Debug.LogFormat("{0} {1}", cbBytes, decodebuffersize);

            exinfo.numchannels = soundChannels;                                    /* Number of channels in the sound. */
            exinfo.defaultfrequency = soundSampleRate;                             /* Default playback rate of sound. */
            exinfo.decodebuffersize = (uint)soundSampleRate;                           /* Chunk size of stream update in samples. This will be the amount of data passed to the user callback. */
            exinfo.length = (uint)(exinfo.defaultfrequency * exinfo.numchannels * sizeof(float) * 5);   /* Length of PCM data in bytes of whole song (for Sound::getLength) */
            exinfo.format = SOUND_FORMAT.PCMFLOAT;         /* Data format of sound. */
            exinfo.pcmreadcallback = pcmreadcallback;                 /* User callback for reading. */
            exinfo.pcmsetposcallback = pcmsetposcallback;               /* User callback for seeking. */
            exinfo.cbsize = Marshal.SizeOf(exinfo);  /* Required. */

            this.reSounds[ofSource] = default(FMOD.Sound);
            this.reChannels[ofSource] = default(FMOD.Channel);

            var mode = MODE.OPENUSER | MODE.LOOP_NORMAL | MODE.CREATESTREAM;
            result = this.fmodsystem.system.createSound(String.Empty, mode, ref exinfo, out this.reSounds[ofSource]);
            ERRCHECK(result, "system.createSound");

            AudioStreamListener.reSounds_CBs.Add(this.reSounds[ofSource].handle, pcmCallbackBuffer);
            UnityEngine.Debug.LogFormat("CreateAndPlayUserSound {0} {1}", this.reSounds[ofSource].handle, pcmCallbackBuffer.Capacity);

            result = this.fmodsystem.system.playSound(this.reSounds[ofSource], this.reMaster, false, out this.reChannels[ofSource]);
            ERRCHECK(result, "system.playSound");
        }

        void StopAndReleaseUserSound(int ofSource)
        {
            FMOD.Sound reSound = this.reSounds[ofSource];

            if (reSound.hasHandle())
            {
                AudioStreamListener.reSounds_CBs.Remove(reSound.handle);

                result = reSound.release();
                ERRCHECK(result, "reSound.release", false);

                reSound.clearHandle();
            }
        }
    }
}