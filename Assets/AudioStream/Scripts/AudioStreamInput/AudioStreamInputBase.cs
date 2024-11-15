﻿// (c) 2016-2024 Martin Cvengros. All rights reserved. Redistribution of source code without permission not allowed.
// uses FMOD by Firelight Technologies Pty Ltd

using AudioStreamSupport;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AudioStream
{
    public abstract class AudioStreamInputBase : ABase
    {
        // ========================================================================================================================================
        #region Required descendant's implementation
        protected abstract void RecordingStarted();
        protected abstract void RecordingUpdate();
        protected abstract void RecordingStopped();
        #endregion
        // ========================================================================================================================================
        #region Editor
        // "No. of audio channels provided by selected recording device.
        [HideInInspector]
        public int recChannels = 0;
        [HideInInspector]
        protected int recRate = 0;

        [Header("[Source]")]
        [Tooltip("Audio input driver ID")]
        public int recordDeviceId = 0;

        [Header("[Setup]")]
        [Tooltip("When checked the recording will start automatically on Start with parameters set in Inspector. Otherwise StartCoroutine(Record()) of this component.")]
        public bool recordOnStart = true;

		[Tooltip("Input gain (non Resonance only). Default 1\r\nBoosting this artificially to high value can help with faint signals for e.g. further reactive processing (although will probably distort audible signal)")]
		[Range(0f, 10f)]
		public float recordGain = 1f;

        [Header("[Input mixer latency (ms)]")]
        [ReadOnly]
        public float latencyBlock;
        [ReadOnly]
        public float latencyTotal;
        [ReadOnly]
        public float latencyAverage;

        #region Unity events
        [Header("[Events]")]
        public EventWithStringParameter OnRecordingStarted;
        public EventWithStringBoolParameter OnRecordingPaused;
        public EventWithStringParameter OnRecordingStopped;
        #endregion

        [Header("[Advanced]")]
        [Tooltip("This is useful to turn off if the original samplerate of the input has to be preserved - e.g. when some other plugin needs original samplerate and/or does resampling on its own\r\n\r\nOtherwise - when on (default) - the input signal is resampled to current Unity output samplerate, either via AudioClip or custom resampler depending on user setting")]
        public bool resampleInput = true;
        [Tooltip("Use Unity's builtin resampling (either by setting the pitch, or directly via AudioClip) and input/output channels mapping.\r\nNote: if input and output samplerates differ significantly this might lead to drifting over time (resulting signal will be delayed)\r\nIn that case it's possible to use custom resampler (by setting this to false), and also provide custom mix matrix if needed in that case (see 'SetCustomMixMatrix' method).\r\n\r\nA default mix matrix is computed automatically if not specified.")]
        public bool useUnityToResampleAndMapChannels = true;
        /// <summary>
        /// default sizes, currently info only
        /// </summary>
        uint dspBufferLength_Auto = 1024;
        uint dspBufferCount_Auto = 4;
        #endregion
        // ========================================================================================================================================
        #region Init && FMOD structures
        /// <summary>
        /// System created/used by all input/recording components
        /// Autorelease in OnDestroy
        /// </summary>
        protected FMOD_SystemW.FMOD_System recording_system;
        /// <summary>
        /// This component specific sound created on the system
        /// </summary>
        protected FMOD.Sound sound;
        /// <summary>
        /// cached size for single channel for used PCMFLOAT format
        /// </summary>
        protected readonly int channelSize = sizeof(float);
        /// <summary>
        /// Unity output sample rate (retrieved from main thread to be consumed at all places)
        /// </summary>
        public int outputSampleRate { get; protected set; }
        /// <summary>
        /// Unity output channels from default speaker mode (retrieved from main thread to be consumed at all places)
        /// </summary>
        public int outputChannels { get; protected set; }
        protected override IEnumerator OnStart()
        {
            // Reference Microphone class on Android in order for Unity to include necessary manifest permission automatically
#if UNITY_ANDROID
            for (var i = 0; i < Microphone.devices.Length; ++i)
                print(string.Format("Enumerating Unity input devices on Android - {0}: {1}", i, Microphone.devices[i]));
#endif
            this.gameObjectName = this.gameObject.name;

            // setup the AudioSource if/when it's being used
            var audiosrc = this.GetComponent<AudioSource>();
            if (audiosrc)
            {
                if (audiosrc.clip != null)
                {
                    this.LOG(LogLevel.WARNING, "Existing AudioClip '{0}' on '{1}' will be ignored", audiosrc.clip.name, this.gameObjectName);
                    audiosrc.clip = null;
                }

                audiosrc.playOnAwake = false;
                audiosrc.Stop();
            }

            // create/get recording system
            this.recording_system = FMOD_SystemW.FMOD_System_Create(
                0
                , true
                , this.logLevel
                , this.gameObjectName
                , this.OnError
                , out this.dspBufferLength_Auto
                , out this.dspBufferCount_Auto
                );

            this.fmodVersion = this.recording_system.VersionString;

            // wait for FMDO to catch up - recordDrivers are not populated if called immediately [e.g. from Start]

            int numAllDrivers = 0;
            int numConnectedDrivers = 0;
            int retries = 0;

            while (numConnectedDrivers < 1)
            {
                result = this.recording_system.Update();
                ERRCHECK(result, "this.recording_system.Update");

                result = this.recording_system.system.getRecordNumDrivers(out numAllDrivers, out numConnectedDrivers);
                ERRCHECK(result, "this.recording_system.system.getRecordNumDrivers");

                LOG(LogLevel.INFO, "Drivers\\Connected drivers: {0}\\{1}", numAllDrivers, numConnectedDrivers);

                if (numConnectedDrivers > 0)
                    break;

                if (++retries > 5)
                {
                    var msg = string.Format("There seems to be no audio input device connected");

                    LOG(LogLevel.ERROR, msg);

                    if (this.OnError != null)
                        this.OnError.Invoke(this.gameObjectName, msg);

                    yield break;
                }

                // this timeout is necessary for recordOnStart
                // TODO: is ad hoc value
                yield return new WaitForSeconds(1f);
            }

            // || Start_Post
            this.ready = true;

            if (this.recordOnStart)
                this.Record();
        }

        protected virtual void Update()
        {
        }
        #endregion

        // ========================================================================================================================================
        #region Recording
        [Header("[Runtime]")]
        [Tooltip("Set during recording.")]
        [ReadOnly]
        public bool isRecording = false;
        [Tooltip("Set during recording.")]
        [ReadOnly]
        public bool isPaused = false;
        public void Record()
        {
            if (!this.ready
                || this.isRecording
                || this.isPaused
                )
            {
                var msg = "Not yet ready or not enabled or already recording (make sure to check 'ready' flag before Recording)";
                this.LOG(LogLevel.ERROR, msg);
                this.OnError.Invoke(msg, "");
                return;
            }

            // (these should be the same since recording_system is system 0 though...)
            if (!UnityAudio.UnitytAudioDisabled())
            {
                this.outputSampleRate = AudioSettings.outputSampleRate;
                this.outputChannels = UnityAudio.ChannelsFromUnityDefaultSpeakerMode();
            }
            else
            {
                this.outputSampleRate = this.recording_system.output_sampleRate;
                this.outputChannels = this.recording_system.output_channels;
            }

            StartCoroutine(this.Record_CR());
        }
        IEnumerator Record_CR()
        {
            if (!this.ready)
            {
                LOG(LogLevel.ERROR, "Will not start recording - system not ready");
                yield break;
            }

            if (this.isRecording)
            {
                LOG(LogLevel.WARNING, "Already recording.");
                yield break;
            }

            if (!this.isActiveAndEnabled)
            {
                LOG(LogLevel.ERROR, "Will not start on disabled GameObject.");
                yield break;
            }

            this.isRecording = false;
            this.isPaused = false;

            this.Stop_Internal(); // try to clean partially started recording / Start initialized system

            // Unity 2017.1 and up has iOS Player Setting 'Force iOS Speakers when Recording' which should be respected
            #if !UNITY_2017_1_OR_NEWER
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                LOG(LogLevel.INFO, "Setting audio output to default/earspeaker ...");
                iOSSpeaker.RouteForRecording();
            }
            #endif

            /*
             * clear previous run if any
             */

            // reset FMOD record buffer positions
            this.lastrecordpos = this.recordpos = 0;

            // and clear previously retrieved recording data
            this.outputBuffer.Clear();

            /*
             * create FMOD sound
             */
            int namelen = 255;
            string name;
            System.Guid guid;
            FMOD.SPEAKERMODE speakermode;
            FMOD.DRIVER_STATE driverstate;
            result = this.recording_system.system.getRecordDriverInfo(this.recordDeviceId, out name, namelen, out guid, out this.recRate, out speakermode, out this.recChannels, out driverstate);
            ERRCHECK(result, "this.recording_system.system.getRecordDriverInfo");

            var exinfo = new FMOD.CREATESOUNDEXINFO();
            exinfo.numchannels = this.recChannels;
            exinfo.format = FMOD.SOUND_FORMAT.PCMFLOAT;                                     // this implies higher bandwidth (i.e. 4 bytes per channel) but seems to work on desktops with DSP sizes low enough
            exinfo.defaultfrequency = this.recRate;
            exinfo.length = (uint)(this.recRate * this.channelSize * this.recChannels * 5); // .. 5 seconds buffer, size here doesn't change latency
            exinfo.cbsize = Marshal.SizeOf(exinfo);

            result = this.recording_system.system.createSound(string.Empty, FMOD.MODE.LOOP_NORMAL | FMOD.MODE.OPENUSER, ref exinfo, out this.sound);
            ERRCHECK(result, "this.recording_system.system.createSound");

            LOG(LogLevel.INFO, "Opened device {0}, channels: {1}, format: {2}, rate: {3} (sound length: {4}, info size: {5}))", this.recordDeviceId, this.recChannels, exinfo.format, exinfo.defaultfrequency, exinfo.length, exinfo.cbsize);

            result = this.recording_system.system.recordStart(this.recordDeviceId, this.sound, true);
            ERRCHECK(result, "this.recording_system.system.recordStart");

            result = this.sound.getLength(out this.soundlength, FMOD.TIMEUNIT.PCM);
            ERRCHECK(result, "sound.getLength");

            // compute rec latency
            int numblocks;
            uint blocksize;
            result = this.recording_system.system.getDSPBufferSize(out blocksize, out numblocks);
            ERRCHECK(result, "this.recording_system.system.getDSPBufferSize");

            // Debug.LogFormat("DSP block: {0} # blocks: {1}", this.blocksize, numblocks);
            // DSP block: 1024 # blocks: 4

            var samplerate = this.recording_system.output_sampleRate;

            float ms = blocksize * 1000.0f / samplerate;
            this.latencyBlock = ms;
            this.latencyTotal = ms * numblocks;
            this.latencyAverage = ms * ((float)numblocks - 1.5f);

            // this is really for playSound which is used in Resonance components:
            // defer a complete derived startup until desired (*) latency is reached to avoid glitches
            // (*) this is computed incl. drift in FMOD record.cpp sample - here the avg. is taken for simplicity for now
            // TODO: user desired latency (/slider)
            //
            // and for FMOD 'warmup' not ready when running from Start on 'recordOnStart'
            // compute how much latency is worth one rec. block ( how much samples "make sense" for sound.lock )
            // if (first) rec. position is further than this on read from sound.lock buffer, ignore the content since it yields to glitched garbage content
            // (this lasts until rec/last rec positions settle and it's present when e.g. 'recordOnStart' is on, but not on subsequent starts..)
            // TODO: user desired latency (/slider)

            // this effectively deos a 'dry run' of rec buffer over few frames - prob. well worth over anything else;

            var df = this.latencyTotal / 1000f;
            var time = 0f;
            var c = 0;
            uint recordDeltaRunningAvg = 0;
            do
            {
                // returned position is in samples
                result = this.recording_system.system.getRecordPosition(this.recordDeviceId, out this.recordpos);
                ERRCHECK(result, "this.recording_system.system.getRecordPosition", false);

                if (this.recordpos != this.lastrecordpos)
                {
                    this.recordDelta = (int)this.recordpos - (int)this.lastrecordpos;
                    if (this.recordDelta < 0)
                        this.recordDelta += (int)this.soundlength;

                    recordDeltaRunningAvg = (uint)((recordDeltaRunningAvg + this.recordDelta) / 2f);

                    this.lastrecordpos = this.recordpos;
                }

                this.LOG(LogLevel.INFO, "Playback delay to meet buffer latency {0}/{1} ms + avg. samples: {2}, pass {3}", time, df, recordDeltaRunningAvg, c);

                result = this.recording_system.Update();
                ERRCHECK(result, "recording_system.Update");

                yield return null;

            } while (((time += Time.deltaTime) < df) || ++c < 5);

            // get 'reasonable' max. samples pre rec. read from current latency (total should make this work on lower performing devices)
            this.soundLockMaxSamples = (uint)(this.latencyTotal * this.recRate / 1000f * 1.5f);
            this.LOG(LogLevel.INFO, "Running max samples: {0}", this.soundLockMaxSamples);

            //
            // playSound is OK, resample & quality, only has latency gap
            // FMOD.Channel channel;
            // FMOD.ChannelGroup chmaster;
            // this.recording_system.system.getMasterChannelGroup(out chmaster);
            // result = this.recording_system.system.playSound(this.sound, chmaster, false, out channel);
            // ERRCHECK(result, "this.recording_system.system.playSound");
            //

            // specific setup/s now
            this.RecordingStarted();

            this.isRecording = true;

            if (this.OnRecordingStarted != null)
                this.OnRecordingStarted.Invoke(this.gameObjectName);

            while (this.isRecording)
            {
                result = this.recording_system.Update();
                ERRCHECK(result, "this.recording_system.Update", false);

                this.RecordingUpdate();
                yield return null;
            }
        }
        // TODO: implement RMS and other potentially useful stats per input channel
        // - add meters to demo scenes separately for input...

        /// <summary>
        /// sound.@lock offset + length
        /// (even ChatGPT says access to member variables is faster than methods locals)
        /// </summary>
        uint soundlength;
        uint recordpos = 0;
        uint lastrecordpos = 0;
        uint soundLockMaxSamples;
        System.IntPtr ptr1, ptr2;
        uint len1, len2;
        protected int recordDelta;
        /// <summary>
        /// Helper method called from descendant which copies raw audio of FMOD sound to Unity buffer - if needed
        /// Updates FMOD system
        /// Since it might be required at different times, i.e. either from OnAudioFilterRead or from normal Update, it can't be called directly by base from here
        /// </summary>
        protected void UpdateRecordBuffer()
        {
            // returned position is in samples
            result = this.recording_system.system.getRecordPosition(this.recordDeviceId, out this.recordpos);
            ERRCHECK(result, "this.recording_system.system.getRecordPosition", false);

            if (this.recordpos != this.lastrecordpos)
            {
                this.recordDelta = (int)this.recordpos - (int)this.lastrecordpos;
                if (this.recordDelta < 0)
                    this.recordDelta += (int)this.soundlength;

                // Debug.LogFormat("Rec delta {0} | {1} - {2} |", this.recordDelta, this.recordpos, this.lastrecordpos);

                var offset = (uint)(this.lastrecordpos * this.channelSize * this.recChannels);
                var length = (uint)(this.recordDelta * this.channelSize * this.recChannels);

                result = this.sound.@lock(offset, length, out this.ptr1, out this.ptr2, out this.len1, out this.len2);
                ERRCHECK(result, "sound.@lock", false);
                if (result != FMOD.RESULT.OK)
                    return;
                //
                // should skip garbage glitches present on autostart
                // 
                if (this.recordDelta <= this.soundLockMaxSamples)
                {
                    // Write it to output.
                    if (this.ptr1 != System.IntPtr.Zero && this.len1 > 0)
                    {
                        byte[] barr = new byte[this.len1];
                        Marshal.Copy(this.ptr1, barr, 0, (int)this.len1);

                        this.AddBytesToOutputBuffer(barr);
                    }
                    if (this.ptr2 != System.IntPtr.Zero && this.len2 > 0)
                    {
                        byte[] barr = new byte[this.len2];
                        Marshal.Copy(this.ptr2, barr, 0, (int)this.len2);

                        this.AddBytesToOutputBuffer(barr);
                    }
                }
                else
                {
                    // this *still* happens even after the 'dry run'
                    this.LOG(LogLevel.INFO, "Runaway delta samples: {0}, max: {1}", this.recordDelta, this.soundLockMaxSamples);
                }

                // Unlock the sound to allow FMOD to use it again.
                result = this.sound.unlock(this.ptr1, this.ptr2, this.len1, this.len2);
                ERRCHECK(result, "sound.unlock", false);
                if (result != FMOD.RESULT.OK)
                    return;
            }

            this.lastrecordpos = this.recordpos;
        }

        public void Pause(bool pause)
        {
            if (!this.isRecording)
            {
                LOG(LogLevel.WARNING, "Not recording..");
                return;
            }

            this.isPaused = pause;

            LOG(LogLevel.INFO, "{0}", this.isPaused ? "paused." : "resumed.");

            if (this.OnRecordingPaused != null)
                this.OnRecordingPaused.Invoke(this.gameObjectName, this.isPaused);
        }
        #endregion

        // ========================================================================================================================================
        #region input and devices updates
        // ReflectInput_Start + ReflectInput_Finish will just stop recording + release and create new system on 0
        // should be possible to improve by not releasing/stopping when *(0) default is preserved...
        public virtual void ReflectInput_Start()
        {
            this.OnDestroy();
        }
        public virtual void ReflectInput_Finish()
        {
            if (this.recording_system != null)
            {
                LOG(LogLevel.ERROR, "ReflectInput_Finish recording_system {0} != null", this.recording_system.system.handle);
                return;
            }

            this.recording_system = FMOD_SystemW.FMOD_System_Create(
                0
                , true
                , this.logLevel
                , this.gameObjectName
                , this.OnError
                , out this.dspBufferLength_Auto
                , out this.dspBufferCount_Auto
                );
        }
        #endregion
        // ========================================================================================================================================
        #region Shutdown
        public void Stop()
        {
            LOG(LogLevel.INFO, "Stopping..");

            this.StopAllCoroutines();

            this.RecordingStopped();

            this.Stop_Internal();

            // this.outputBuffer.Close();

            if (this.OnRecordingStopped != null)
                this.OnRecordingStopped.Invoke(this.gameObjectName);
        }

        /// <summary>
        /// Stop and try to release FMOD sound resources
        /// </summary>
        void Stop_Internal()
        {
            this.isRecording = false;
            this.isPaused = false;

            result = this.recording_system.system.recordStop(this.recordDeviceId);
            ERRCHECK(result, "system.recordStop", false);

            /*
                Shut down sound
            */
            if (this.sound.hasHandle())
            {
                result = this.sound.release();
                ERRCHECK(result, "sound.release", false);

                this.sound.clearHandle();
            }
        }

        protected virtual void OnDestroy()
        {
            this.Stop();

            /*
                Shut down
            */
            FMOD_SystemW.FMOD_System_Release(ref this.recording_system, this.logLevel, this.gameObjectName, this.OnError);
        }
        #endregion

        // ========================================================================================================================================
        #region Support
        /// <summary>
        /// Exchange buffer between FMOD and Unity
        /// this needs capacity at contruction but will be resized later if needed
        /// </summary>
        BasicBufferFloat outputBuffer = new BasicBufferFloat(10000);
        /// <summary>
        /// Stores audio retrieved from FMOD's sound for Unity
        /// </summary>
        /// <param name="arr"></param>
        void AddBytesToOutputBuffer(byte[] arr)
        {
            lock (this.outputBuffer)
            {
                // if it's paused discard retrieved input ( but the whole recording update loop is running to allow for seamless continuation )
                if (this.isPaused)
                    return;

                // check if there's still enough space in noncircular buffer
                // (since circular buffer does not seem to work / )
                if (this.outputBuffer.Available() + arr.Length > this.outputBuffer.Capacity())
                {
                    var newCap = this.outputBuffer.Capacity() * 2;

                    LOG(LogLevel.INFO, "Resizing output buffer from: {0} to {1} (FMOD is retrieving more data than Unity is able to drain continuosly : input channels: {2}, output channels: {3})"
                        , this.outputBuffer.Capacity()
                        , newCap
                        , this.recChannels
                        , this.outputChannels
                        );

                    // preserve existing data
                    BasicBufferFloat newBuffer = new BasicBufferFloat(newCap);
                    newBuffer.Write(this.outputBuffer.Read(this.outputBuffer.Available()));

                    this.outputBuffer = newBuffer;
                }

                var farr = new float[arr.Length / this.channelSize];
                System.Buffer.BlockCopy(arr, 0, farr, 0, arr.Length);
                
                // Apply input gain
                for (var i = 0; i < farr.Length; i++)
                    farr[i] *= this.recordGain;

                this.outputBuffer.Write(farr);
            }
        }
        /// <summary>
        /// Retrieves recorded data for Unity callbacks
        /// </summary>
        /// <param name="_len"></param>
        /// <returns></returns>
        protected float[] GetAudioOutputBuffer(uint len)
        {
            lock (this.outputBuffer)
            {
                // adjust to what's available
                len = (uint)Mathf.Min((int)len, this.outputBuffer.Available());

                // read available
                return this.outputBuffer.Read(len);
            }
        }
        #endregion

        // ========================================================================================================================================
        #region User support
        /*
         * to not complicate things for now all DSP buffer related settings need to be set before system init / creation
         * (otherwise we'd need to tear down and recreate the whole system..)
         */
        /*
        /// <summary>
        /// Changing DSP buffers before Init means we have to restart
        /// </summary>
        /// <param name="_dspBufferLength"></param>
        /// <param name="_dspBufferCount"></param>
        public void SetUserDSPBuffers(uint _dspBufferLength, uint _dspBufferCount)
        {
            // prevent crashing on macOS/iOS/Android and overall nonsensical too small values for FMOD
            if (_dspBufferLength < 16)
            {
                Debug.LogWarningFormat("Not setting too small value {0} for DSP buffer length", _dspBufferLength);
                return;
            }

            if (_dspBufferCount < 2)
            {
                Debug.LogWarningFormat("Not setting too small value {0} for DSP buffer count", _dspBufferCount);
                return;
            }

            LOG(LogLevel.INFO, "SetDSPBuffers _dspBufferLength: {0}, _dspBufferCount: {1}", _dspBufferLength, _dspBufferCount);

            this.dspBufferLength_Custom = _dspBufferLength;
            this.dspBufferCount_Custom = _dspBufferCount;

            // using custom DSP buffer size -
            this.useAutomaticDSPBufferSize = false;

            // if input is running, restart it
            var wasRunning = this.isRecording;
            this.Stop();
            if (wasRunning)
                this.Record();
        }
        /// <summary>
        /// Changing DSP buffers before Init means we have to restart
        /// </summary>
        /// <param name="_useAutomaticDSPBufferSize"></param>
        public void SetAutomaticDSPBufferSize()
        {
            LOG(LogLevel.INFO, "SetAutomaticDSPBufferSize");

            this.useAutomaticDSPBufferSize = true;

            // if input is running, restart it
            var wasRunning = this.isRecording;
            this.Stop();
            if (wasRunning)
                this.Record();
        }
        */
        /// <summary>
        /// Gets current DSP buffer size
        /// </summary>
        /// <param name="dspBufferLength"></param>
        /// <param name="dspBufferCount"></param>
        public void GetDSPBufferSize(out uint dspBufferLength, out uint dspBufferCount)
        {
            dspBufferLength = this.dspBufferLength_Auto;
            dspBufferCount = this.dspBufferCount_Auto;
        }
        #endregion
    }
}