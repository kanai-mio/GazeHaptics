// (c) 2016-2024 Martin Cvengros. All rights reserved. Redistribution of source code without permission not allowed.
// uses FMOD by Firelight Technologies Pty Ltd

using AudioStreamSupport;
using FMOD;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AudioStream
{
    public abstract partial class AudioStreamBase : ABase
    {
        // TODO: OutputSound/Device == Listener OutputSound/Device

        // ========================================================================================================================================
        #region output and devices updates
        [Header("[Output]")]
        [Tooltip("You can specify any available audio output device present in the system.\r\nPass an interger number between 0 and (# of audio output devices) - 1 (0 is default output).\r\nThis ID will change to reflect addition/removal of devices at runtime when a game object with AudioStreamDevicesChangedNotify component is in the scene. See FMOD_SystemW.AvailableOutputs / notification event from AudioStreamDevicesChangedNotify in the demo scene.")]
        [SerializeField]
        protected int outputDriverID = 0;
        /// <summary>
        /// public facing property
        /// </summary>
        public int OutputDriverID { get { return this.outputDriverID; } protected set { this.outputDriverID = value; } }
        /// <summary>
        /// Complete info about output #outputDriverID - relevant for 'FMOD based' descendants only for SetOutput/ReflectOutput
        /// </summary>
        FMOD_SystemW.OUTPUT_DEVICE_INFO outputDevice;
        /// <summary>
        /// Allows setting the output device 
        /// default directly FMOD based implementation (for AudioStreamMinimal/Resonance and similar)
        /// </summary>
        /// <param name="outputDriverId"></param>
        public virtual void SetOutput(int _outputDriverID)
        {
            if (!this.ready)
            {
                LOG(LogLevel.ERROR, "Please make sure to wait for 'ready' flag before calling this method");
                return;
            }

            LOG(LogLevel.INFO, "Setting output to driver {0} ", _outputDriverID);

            // try to help the system to update its surroundings
            result = fmodsystem.Update();
            ERRCHECK(result, "fmodsystem.Update", false);

            if (result != RESULT.OK)
                return;

            result = fmodsystem.system.setDriver(_outputDriverID);
            // ERRCHECK(result, string.Format("fmodsystem.System.setDriver {0}", _outputDriverID), false);

            if (result != RESULT.OK)
            {
                // TODO
                LOG(LogLevel.WARNING, "Setting output to newly appeared device {0} during runtime is currently not supported for FMOD/minimal components", _outputDriverID);
                return;
            }

            this.outputDriverID = _outputDriverID;

            /*
             * Log output device info
             */
            int od_namelen = 255;
            string od_name;
            System.Guid od_guid;

            result = fmodsystem.system.getDriverInfo(_outputDriverID, out od_name, od_namelen, out od_guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels);
            ERRCHECK(result, "fmodsystem.system.getDriverInfo", false);

            if (result != RESULT.OK)
                return;

            LOG(LogLevel.INFO, "Driver {0} Info: name: {1}, guid: {2}", _outputDriverID, od_name, od_guid);

            this.outputDriverID = _outputDriverID;
            this.outputDevice = new FMOD_SystemW.OUTPUT_DEVICE_INFO() { id = this.outputDriverID, name = od_name, guid = od_guid, samplerate_driver = systemrate, speakermode_driver = speakermode, channels_driver = speakermodechannels };
        }
        /// <summary>
        /// default directly FMOD based implementation (for AudioStreamMinimal and similar)
        /// first phase of reflecting updated outputs on device/s change
        /// (does effectively nothing since there's no sound to stop)
        /// </summary>
        public virtual void ReflectOutput_Start()
        {
            if (!this.ready)
            {
                LOG(LogLevel.ERROR, "Please make sure to wait for 'ready' flag before calling this method");
                return;
            }

            // redirection is always running so restart it with new output
            // this.StopFMODSound();
        }
        /// <summary>
        /// Updates this' runtimeOutputDriverID and FMOD sound/system based on new/updated system devices list
        /// Tries to find output in new list if it was played already, or sets output to be user output id, if possible
        /// default directly FMOD based implementation (for AudioStreamMinimal and similar)
        /// </summary>
        /// <param name="updatedOutputDevices"></param>
        public virtual void ReflectOutput_Finish(List<FMOD_SystemW.OUTPUT_DEVICE_INFO> updatedOutputDevices)
        {
            // sounds end up on default (0) if device is not present
            var outputID = 0;

            // if output wasn't running it was requested on initially non present output
            if (this.outputDevice.guid != Guid.Empty)
            {
                var output = updatedOutputDevices.FirstOrDefault(f => f.guid == this.outputDevice.guid);

                // Guid.Empty indicates OUTPUT_DEVICE_INFO default (not found) item
                if (output.guid != Guid.Empty)
                {
                    outputID = updatedOutputDevices.IndexOf(output);
                }
            }
            else
            {
                if (this.outputDriverID < updatedOutputDevices.Count)
                {
                    outputID = this.outputDriverID;
                }
            }

            // this.StartFMODSound(outputID);

            // might fail in case initial FMOD snapshot didn't capture new id...
            this.SetOutput(outputID);
        }
        #endregion
    }
}