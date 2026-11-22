using System;
namespace ProjectSpark.Level130
{
    /// <summary>Isolated fictional diagnostic exercise. Never drives real terminals or level objectives.</summary>
    public sealed class SparkL130RecoveryTeachingSession
    {
        public enum Fault { EpsSeating, FirmwareRead, FanFeedback }
        public enum Evidence { None, CpuRailInvalid, FirmwareIntegrityInvalid, FanTachInvalid, Healthy }
        public Fault CurrentFault { get; private set; }
        public bool Powered { get; private set; }
        public bool Repaired { get; private set; }
        public bool FreshEvidence { get; private set; }
        public bool RetestPassed { get; private set; }
        public void Begin(Fault fault)
        {
            if (!Enum.IsDefined(typeof(Fault),fault)) throw new ArgumentOutOfRangeException(nameof(fault));
            CurrentFault=fault; Powered=false; Repaired=false; FreshEvidence=false; RetestPassed=false;
        }
        public void SetPower(bool value) { if (Powered==value) return; Powered=value; RetestPassed=false; }
        public Evidence Measure()
        {
            if (!Powered) return Evidence.None;
            if (Repaired) return Evidence.Healthy;
            FreshEvidence=true;
            switch(CurrentFault)
            {
                case Fault.EpsSeating: return Evidence.CpuRailInvalid;
                case Fault.FirmwareRead: return Evidence.FirmwareIntegrityInvalid;
                case Fault.FanFeedback: return Evidence.FanTachInvalid;
                default: return Evidence.None;
            }
        }
        public bool TryRepair(Fault target)
        {
            if (Powered || !FreshEvidence || Repaired || target!=CurrentFault) return false;
            Repaired=true; FreshEvidence=false; RetestPassed=false; return true;
        }
        public bool Retest()
        {
            RetestPassed=Powered && Repaired && Measure()==Evidence.Healthy;
            return RetestPassed;
        }
    }
}
