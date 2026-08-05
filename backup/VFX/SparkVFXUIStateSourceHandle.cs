using System;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Lightweight runtime handle for controlling a
    /// SparkVFXUIStateSource.
    ///
    /// Useful for:
    /// - Dynamically spawned UI elements
    /// - Runtime target indicators
    /// - Tutorial highlights
    /// - Quest markers
    /// - Warning indicators
    /// - Temporary interaction states
    ///
    /// The handle does NOT own the source.
    /// The source remains responsible for its own lifecycle.
    /// </summary>
    public sealed class SparkVFXUIStateSourceHandle
        : IDisposable
    {
        // ============================================================
        // SOURCE
        // ============================================================

        private SparkVFXUIStateSource source;


        // ============================================================
        // RELEASED
        // ============================================================

        private bool released;


        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public SparkVFXUIStateSourceHandle(
            SparkVFXUIStateSource source)
        {
            this.source =
                source;
        }


        // ============================================================
        // SOURCE
        // ============================================================

        public SparkVFXUIStateSource Source
        {
            get
            {
                if (released)
                {
                    return null;
                }


                return source;
            }
        }


        // ============================================================
        // VALID
        // ============================================================

        public bool IsValid
        {
            get
            {
                return
                    !released &&
                    source != null;
            }
        }


        // ============================================================
        // ACTIVE
        // ============================================================

        public bool IsActive
        {
            get
            {
                if (!IsValid)
                {
                    return false;
                }


                return source.IsActive;
            }
        }


        // ============================================================
        // SOURCE ID
        // ============================================================

        public string SourceID
        {
            get
            {
                if (!IsValid)
                {
                    return string.Empty;
                }


                return source.SourceID;
            }
        }


        // ============================================================
        // ACTIVATE
        // ============================================================

        public bool Activate()
        {
            if (!IsValid)
            {
                return false;
            }


            source.Activate();

            return true;
        }


        // ============================================================
        // DEACTIVATE
        // ============================================================

        public bool Deactivate()
        {
            if (!IsValid)
            {
                return false;
            }


            source.Deactivate();

            return true;
        }


        // ============================================================
        // SET STATE
        // ============================================================

        public bool SetState(
            SparkVFXEventType eventType)
        {
            if (!IsValid)
            {
                return false;
            }


            source.SetState(
                eventType
            );


            return true;
        }


        // ============================================================
        // SET STATE + PRIORITY
        // ============================================================

        public bool SetState(
            SparkVFXEventType eventType,
            int priority)
        {
            if (!IsValid)
            {
                return false;
            }


            source.SetState(
                eventType,
                priority
            );


            return true;
        }


        // ============================================================
        // SET PRIORITY
        // ============================================================

        public bool SetPriority(
            int priority)
        {
            if (!IsValid)
            {
                return false;
            }


            source.SetPriority(
                priority
            );


            return true;
        }


        // ============================================================
        // SET INSTANT
        // ============================================================

        public bool SetInstantPlayback(
            bool instant)
        {
            if (!IsValid)
            {
                return false;
            }


            source.SetInstantPlayback(
                instant
            );


            return true;
        }


        // ============================================================
        // TARGET
        // ============================================================

        public bool SetTarget()
        {
            if (!IsValid)
            {
                return false;
            }


            source.SetTarget();

            return true;
        }


        // ============================================================
        // WARNING
        // ============================================================

        public bool SetWarning()
        {
            if (!IsValid)
            {
                return false;
            }


            source.SetWarning();

            return true;
        }


        // ============================================================
        // ERROR
        // ============================================================

        public bool SetError()
        {
            if (!IsValid)
            {
                return false;
            }


            source.SetError();

            return true;
        }


        // ============================================================
        // SELECTED
        // ============================================================

        public bool SetSelected()
        {
            if (!IsValid)
            {
                return false;
            }


            source.SetSelected();

            return true;
        }


        // ============================================================
        // HOVER
        // ============================================================

        public bool SetHover()
        {
            if (!IsValid)
            {
                return false;
            }


            source.SetHover();

            return true;
        }


        // ============================================================
        // NORMAL
        // ============================================================

        public bool SetNormal()
        {
            if (!IsValid)
            {
                return false;
            }


            source.SetNormal();

            return true;
        }


        // ============================================================
        // DISABLED
        // ============================================================

        public bool SetDisabled()
        {
            if (!IsValid)
            {
                return false;
            }


            source.SetDisabled();

            return true;
        }


        // ============================================================
        // SUBMIT
        // ============================================================

        public bool Submit()
        {
            if (!IsValid)
            {
                return false;
            }


            source.Submit();

            return true;
        }


        // ============================================================
        // CLEAR
        // ============================================================

        public bool Clear()
        {
            if (!IsValid)
            {
                return false;
            }


            source.Clear();

            return true;
        }


        // ============================================================
        // RELEASE
        // ============================================================

        public void Release()
        {
            if (released)
            {
                return;
            }


            released =
                true;


            source =
                null;
        }


        // ============================================================
        // DISPOSE
        // ============================================================

        public void Dispose()
        {
            Release();
        }
    }
}