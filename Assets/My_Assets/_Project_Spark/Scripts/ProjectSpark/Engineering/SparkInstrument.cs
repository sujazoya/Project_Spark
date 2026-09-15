using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public enum SparkInstrumentState { Off, Idle, Acquiring, Paused, Fault }

    public class SparkInstrument : SparkElectricalComponent
    {
        [SerializeField] private SparkInstrumentState instrumentState = SparkInstrumentState.Off;
        public SparkInstrumentState InstrumentState => instrumentState;
        public bool IsOn => instrumentState != SparkInstrumentState.Off && instrumentState != SparkInstrumentState.Fault;
        public bool SetInstrumentState(SparkInstrumentState state) { if (instrumentState == state) return false; instrumentState = state; return true; }
    }
}
