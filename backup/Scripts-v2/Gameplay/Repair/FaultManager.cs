using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Repair
{
    public sealed class FaultManager
        : MonoBehaviour
    {
        [SerializeField]
        private FaultDatabase database;

        private readonly List<Fault>
            activeFaults =
                new();

        private readonly RepairValidator
            validator =
                new();

        public IReadOnlyList<Fault>
            ActiveFaults =>
                activeFaults;

        public void InjectFault(
            Fault fault)
        {
            activeFaults.Add(fault);
        }

        public RepairResult Repair(
            Fault fault,
            RepairAction action)
        {
            bool ok =
                validator.Validate(
                    fault,
                    action);

            if (!ok)
            {
                return new RepairResult
                {
                    Success = false,
                    Message = "Wrong repair."
                };
            }

            fault.Repaired = true;

            RepairEvents
                .RaiseRepaired(fault);

            return new RepairResult
            {
                Success = true,
                Score = 100,
                Experience = 25,
                Message = "Repair completed."
            };
        }
    }
}
