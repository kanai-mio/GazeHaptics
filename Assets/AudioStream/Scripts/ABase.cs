// (c) 2016-2024 Martin Cvengros. All rights reserved. Redistribution of source code without permission not allowed.
// uses FMOD by Firelight Technologies Pty Ltd

using AudioStreamSupport;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace AudioStream
{
    /// <summary>
    /// Widely used asset's common MonoBehaviour shared non audio related parts
    /// </summary>
    public abstract partial class ABase : MonoBehaviour
    {
        // ========================================================================================================================================
        #region Editor
        [Tooltip("Turn on/off logging to the Console. Errors are always printed.")]
        public LogLevel logLevel = LogLevel.ERROR;
        /// <summary>
        /// GO name to be accessible from all the threads if needed
        /// </summary>
        protected string gameObjectName = string.Empty;
        #region Unity events
        [Header("[Events]")]
        public EventWithStringStringParameter OnError;
        #endregion
        #endregion
        // ========================================================================================================================================
        #region Init && FMOD / info
        /// <summary>
        /// Component startup sync - set once FMOD had been fully initialized
        /// delayed in cases when e.g. FMOD needs some time to enumerate all present recording devices
        /// should always be checked before using this component
        /// </summary>
        public bool ready { get; protected set; } = false;
        [ReadOnly]
        public string fmodVersion;
        protected FMOD_SystemW.FMOD_System fmodsystem = null;
        public FMOD.RESULT result { get; protected set; }
        protected FMOD.RESULT lastError = FMOD.RESULT.OK;
        /// <summary>
        /// Have Awake declared so it's used within inheritance chain
        /// </summary>
        protected virtual void Awake()
        {
            // + (development) reflection check for `Start` existence in descendant/s to enforce its usage only from here
#if UNITY_EDITOR
            this.CheckForStartInInheritance(this.GetType());
#endif
        }
#if UNITY_EDITOR
        private void CheckForStartInInheritance(Type currentType)
        {
            if (currentType == typeof(ABase)
                )
                return; // Reached this base

            var startMethod = currentType.GetMethod("Start", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (startMethod != null)
                throw new AmbiguousMatchException(string.Format("`{0} Start()` in: {1}", startMethod.ReturnType, currentType.Name));

            // check the parent
            this.CheckForStartInInheritance(currentType.BaseType);
        }
#endif
        /// <summary>
        /// single Start point
        /// </summary>
        /// <returns></returns>
        private IEnumerator Start()
        {
            // FMOD_SystemW.InitializeFMODDiagnostics(FMOD.DEBUG_FLAGS.LOG);

            this.gameObjectName = string.Format("{0} ({1}) ", this.gameObject.name, this.GetType());

            yield return this.OnStart();

            this.ready = true;
        }
        protected abstract IEnumerator OnStart();
        // protected virtual void OnDestroy() { }
        #endregion
        // ========================================================================================================================================
        #region (logging)
        public void ERRCHECK(FMOD.RESULT result, string customMessage, bool throwOnError = true)
        {
            this.lastError = result;

            if (throwOnError)
            {
                try
                {
                    FMODHelpers.ERRCHECK(result, this.logLevel, this.gameObjectName, this.OnError, customMessage, throwOnError);
                }
                catch (System.Exception ex)
                {
                    // clear the startup flag only when requesting abort on error
                    throw ex;
                }
            }
            else
            {
                FMODHelpers.ERRCHECK(result, this.logLevel, this.gameObjectName, this.OnError, customMessage, throwOnError);
            }
        }
        protected void LOG(LogLevel requestedLogLevel, string format, params object[] args)
        {
            Log.LOG(requestedLogLevel, this.logLevel, this.gameObjectName, format, args);
        }
        public string GetLastError(out FMOD.RESULT errorCode)
        {
            errorCode = this.lastError;
            return FMOD.Error.String(errorCode);
        }
        #endregion
    }
}