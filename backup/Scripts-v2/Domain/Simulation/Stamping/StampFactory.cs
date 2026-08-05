using ProjectSpark.Gameplay.Electronics;

namespace ProjectSpark.Domain.Simulation.Stamping
{
    public sealed class StampFactory
    {
        public ICircuitStamp Create(
            ElectronicComponent component)
        {
            return component switch
            {
                ResistorComponent =>
                    new ResistorStamp(),

                BatteryComponent =>
                    new BatteryStamp(),

                LEDComponent =>
                    new LEDStamp(),

                WireComponent =>
                    new WireStamp(),

                _ => null
            };
        }
    }
}
