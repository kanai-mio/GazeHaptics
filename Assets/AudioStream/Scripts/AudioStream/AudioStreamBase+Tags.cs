// (c) 2016-2024 Martin Cvengros. All rights reserved. Redistribution of source code without permission not allowed.
// uses FMOD by Firelight Technologies Pty Ltd

using AudioStreamSupport;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AudioStream
{
    /// <summary>
    /// tags processing
    /// </summary>
    public abstract partial class AudioStreamBase : ABase
    {
        // ========================================================================================================================================
        #region Tags support
        bool tagsChanged = false;
        readonly object tagsLock = new object();
        #region Main thread execution queue for texture creation
        readonly Queue<System.Action> executionQueue = new Queue<System.Action>();
        /// <summary>
        /// Locks the queue and adds the Action to the queue
        /// </summary>
        /// <param name="action">function that will be executed from the main thread.</param>
        void ScheduleAction(System.Action action)
        {
            lock (this.executionQueue)
            {
                this.executionQueue.Enqueue(action);
            }
        }
        #endregion
        /// <summary>
        /// Reads 1 tag from running stream
        /// Sets flag for CR to update due to threading
        /// </summary>
        protected void ReadTags()
        {
            // Read any tags that have arrived, this could happen if a radio station switches to a new song.
            FMOD.TAG streamTag;
            // Have to use FMOD >= 1.10.01 for tags to work - https://github.com/fmod/UnityIntegration/pull/11

            while (sound.getTag(null, -1, out streamTag) == FMOD.RESULT.OK)
            {
                // do some tag examination and logging for unhandled tag types
                // special FMOD tag type for detecting sample rate change

                var FMODtag_Type = streamTag.type;
                string FMODtag_Name = (string)streamTag.name;
                object FMODtag_Value = null;

                if (FMODtag_Type == FMOD.TAGTYPE.FMOD
                    && FMODtag_Name == "Sample Rate Change"
                    )
                {
                    // When a song changes, the samplerate may also change, so update here.
                    // resampling is done via the AudioClip - but we have to recreate it for AudioStream ( will cause small stream disruption but there's probably no other way )
                    // , do it via direct calls without events

                    // float frequency = *((float*)streamTag.data);
                    float[] frequency = new float[1];
                    Marshal.Copy(streamTag.data, frequency, 0, 1);

                    LOG(LogLevel.WARNING, "Stream sample rate changed to: {0}", frequency[0]);

                    // get new sound_format
                    result = sound.getFormat(out this.streamSoundType, out this.streamFormat, out this.streamChannels, out this.streamBits);
                    ERRCHECK(result, "sound.getFormat", false);

                    this.StreamChanged(frequency[0], this.streamChannels, this.streamFormat);
                }
                else
                {
                    switch (streamTag.datatype)
                    {
                        case FMOD.TAGDATATYPE.BINARY:

                            FMODtag_Value = "binary data";

                            // check if it's ID3v2 'APIC' tag for album/cover art
                            if (FMODtag_Type == FMOD.TAGTYPE.ID3V2)
                            {
                                if (FMODtag_Name == "APIC" || FMODtag_Name == "PIC")
                                {
                                    byte[] picture_data;
                                    byte picture_type;

                                    // read all texture bytes into picture_data
                                    this.ReadID3V2TagValue_APIC(streamTag.data, streamTag.datalen, out picture_data, out picture_type);

                                    // since 'There may be several pictures attached to one file, each in their individual "APIC" frame, but only one with the same content descriptor.'
                                    // we store its type alongside tag name and create every texture present
                                    if (picture_data != null)
                                    {
                                        // Load texture on the main thread, if needed
                                        this.LoadTexture_OnMainThread(picture_data, picture_type);
                                    }
                                }
                            }

                            break;

                        case FMOD.TAGDATATYPE.FLOAT:
                            FMODtag_Value = this.ReadFMODTagValue_float(streamTag.data, streamTag.datalen);
                            break;

                        case FMOD.TAGDATATYPE.INT:
                            FMODtag_Value = this.ReadFMODTagValue_int(streamTag.data, streamTag.datalen);
                            break;

                        case FMOD.TAGDATATYPE.STRING:
                        case FMOD.TAGDATATYPE.STRING_UTF16:
                        case FMOD.TAGDATATYPE.STRING_UTF16BE:
                        case FMOD.TAGDATATYPE.STRING_UTF8:
                            FMODtag_Value = FMODHelpers.StringFromNative(streamTag.data, out var bytesRead);
                            break;
                    }

                    // update tags, binary data (texture) is handled separately
                    if (streamTag.datatype != FMOD.TAGDATATYPE.BINARY)
                    {
                        lock (this.tagsLock)
                            this.tags[FMODtag_Name] = FMODtag_Value;
                        this.tagsChanged = true;
                    }
                }

                LOG(LogLevel.INFO, "{0} tag: {1}, [{2}] value: {3}", FMODtag_Type, FMODtag_Name, streamTag.datatype, FMODtag_Value);
            }
        }
        /// <summary>
        /// Calls texture creation if on main thread, otherwise schedules its creation to the main thread
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="picture_bytes"></param>
        /// <param name="picture_type"></param>
        void LoadTexture_OnMainThread(byte[] picture_bytes, byte picture_type)
        {
            // follow APIC tag for now
            var tagName = "APIC_" + picture_type;

            // if on main thread, create & load texture
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == this.mainThreadId)
            {
                this.LoadTexture(tagName, picture_bytes);
            }
            else
            {
                this.ScheduleAction(() => { this.LoadTexture(tagName, picture_bytes); });
            }
        }
        /// <summary>
        /// Creates new texture from raw jpg/png bytes and adds it to the tags dictionary
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="picture_bytes"></param>
        void LoadTexture(string tagName, byte[] picture_bytes)
        {
            Texture2D image = new Texture2D(2, 2);
            image.LoadImage(picture_bytes);

            lock (this.tagsLock)
                this.tags[tagName] = image;
            this.tagsChanged = true;
        }
        // ID3V2 APIC tag specification
        // 
        // Following http://id3.org/id3v2.3.0
        // 
        // Numbers preceded with $ are hexadecimal and numbers preceded with % are binary. $xx is used to indicate a byte with unknown content.
        // 
        // <Header for 'Attached picture', ID: "APIC">
        // Text encoding   $xx
        // MIME type<text string> $00
        // Picture type    $xx
        // Description<text string according to encoding> $00 (00)
        // Picture data<binary data>
        //
        // Frames that allow different types of text encoding have a text encoding description byte directly after the frame size. If ISO-8859-1 is used this byte should be $00, if Unicode is used it should be $01
        //
        // Picture type:
        // $00     Other
        // $01     32x32 pixels 'file icon' (PNG only)
        // $02     Other file icon
        // $03     Cover(front)
        // $04     Cover(back)
        // $05     Leaflet page
        // $06     Media(e.g.lable side of CD)
        // $07     Lead artist/lead performer/soloist
        // $08     Artist/performer
        // $09     Conductor
        // $0A     Band/Orchestra
        // $0B     Composer
        // $0C     Lyricist/text writer
        // $0D     Recording Location
        // $0E     During recording
        // $0F     During performance
        // $10     Movie/video screen capture
        // $11     A bright coloured fish
        // $12     Illustration
        // $13     Band/artist logotype
        // $14     Publisher/Studio logotype

        /// <summary>
        /// Reads value (image data) of the 'APIC' tag as per specification from http://id3.org/id3v2.3.0
        /// We are *hoping* that any and all strings are ASCII/Ansi *only*
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="datalen"></param>
        /// <returns></returns>
        protected void ReadID3V2TagValue_APIC(System.IntPtr fromAddress, uint datalen, out byte[] picture_data, out byte picture_type)
        {
            picture_data = null;

            // Debug.LogFormat("IntPtr: {0}, length: {1}", fromAddress, datalen);

            var text_encoding = (byte)Marshal.PtrToStructure(fromAddress, typeof(byte));
            fromAddress = new System.IntPtr(fromAddress.ToInt64() + 1);
            datalen--;

            // Debug.LogFormat("text_encoding: {0}", text_encoding);

            // Frames that allow different types of text encoding have a text encoding description byte directly after the frame size. If ISO-8859-1 is used this byte should be $00, if Unicode is used it should be $01
            uint terminator_size;
            if (text_encoding == 0)
                terminator_size = 1;
            else if (text_encoding == 1)
                terminator_size = 2;
            else
                // Not 1 && 2 text encoding is invalid - should we try the string to be terminated by... single 0 ?
                terminator_size = 1;

            // Debug.LogFormat("terminator_size: {0}", terminator_size);

            uint bytesRead;

            string MIMEtype = FMODHelpers.StringFromNative(fromAddress, out bytesRead);
            fromAddress = new System.IntPtr(fromAddress.ToInt64() + bytesRead + terminator_size);
            datalen -= bytesRead;
            datalen -= terminator_size;

            // Debug.LogFormat("MIMEtype: {0}", MIMEtype);

            picture_type = (byte)Marshal.PtrToStructure(fromAddress, typeof(byte));
            fromAddress = new System.IntPtr(fromAddress.ToInt64() + 1);
            datalen--;

            // Debug.LogFormat("picture_type: {0}", picture_type);

            // string description_text = 
            FMODHelpers.StringFromNative(fromAddress, out bytesRead);
            fromAddress = new System.IntPtr(fromAddress.ToInt64() + bytesRead + terminator_size);
            datalen -= bytesRead;
            datalen -= terminator_size;

            // Debug.LogFormat("description_text: {0}", description_text);

            // Debug.LogFormat("Supposed picture byte size: {0}", datalen);
            if (
                // "image/" prefix is from spec, but some tags them are broken
                // MIMEtype.ToLowerInvariant().StartsWith("image/", System.StringComparison.OrdinalIgnoreCase)
                // &&
                (
                    MIMEtype.ToLowerInvariant().EndsWith("jpeg")
                    || MIMEtype.ToLowerInvariant().EndsWith("jpg")
                    || MIMEtype.ToLowerInvariant().EndsWith("png")
                    )
                )
            {
                picture_data = new byte[datalen];
                Marshal.Copy(fromAddress, picture_data, 0, (int)datalen);
            }
        }
        /// <summary>
        /// https://www.fmod.com/docs/2.02/api/core-api-sound.html#fmod_tagdatatype
        /// IEEE floating point number. See FMOD_TAG structure to confirm if the float data is 32bit or 64bit (4 vs 8 bytes).
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="datalen"></param>
        /// <returns></returns>
#if ENABLE_IL2CPP
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
        protected double ReadFMODTagValue_float(System.IntPtr fromAddress, uint datalen)
        {
            byte[] barray = new byte[datalen];

            for (var offset = 0; offset < datalen; ++offset)
                barray[offset] = Marshal.ReadByte(fromAddress, offset);

            switch (datalen)
            {
                case 4:
                    return System.BitConverter.ToSingle(barray, 0);
                case 8:
                    return System.BitConverter.ToDouble(barray, 0);
            }

            return 0;
        }
        /// <summary>
        /// https://www.fmod.com/docs/2.02/api/core-api-sound.html#fmod_tagdatatype
        /// Integer - Note this integer could be 8bit / 16bit / 32bit / 64bit. See FMOD_TAG structure for integer size (1 vs 2 vs 4 vs 8 bytes).
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="datalen"></param>
        /// <returns></returns>
#if ENABLE_IL2CPP
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
        protected long ReadFMODTagValue_int(System.IntPtr fromAddress, uint datalen)
        {
            byte[] barray = new byte[datalen];

            for (var offset = 0; offset < datalen; ++offset)
                barray[offset] = Marshal.ReadByte(fromAddress, offset);

            switch (datalen)
            {
                case 1:
                    return barray[0];
                case 2:
                    return System.BitConverter.ToInt16(barray, 0);
                case 4:
                    return System.BitConverter.ToInt32(barray, 0);
                case 8:
                    return System.BitConverter.ToInt64(barray, 0);
            }

            return 0;
        }
        #endregion
    }
}