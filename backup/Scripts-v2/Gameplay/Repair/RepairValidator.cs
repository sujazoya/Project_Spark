namespace ProjectSpark.Gameplay.Repair
{
    public sealed class RepairValidator
    {
        public bool Validate(
            Fault fault,
            RepairAction action)
        {
            switch (fault.Type)
            {
                case FaultType.BlownFuse:

                    return action ==
                        RepairAction.ReplaceFuse;

                case FaultType.BurntResistor:

                    return action ==
                        RepairAction.ReplaceResistor;

                case FaultType.DeadBattery:

                    return action ==
                        RepairAction.ReplaceBattery;

                default:

                    return false;
            }
        }
    }
}
