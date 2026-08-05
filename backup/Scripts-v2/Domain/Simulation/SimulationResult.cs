namespace ProjectSpark.Domain.Simulation
{
    public readonly struct SimulationResult
    {
        public readonly bool Success;

        public readonly bool HasFault;

        public readonly string Message;

        public SimulationResult(
            bool success,
            bool fault,
            string message)
        {
            Success = success;
            HasFault = fault;
            Message = message;
        }
    }
}
