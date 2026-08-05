using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Career
{
    public sealed class CustomerManager
        : MonoBehaviour
    {
        [SerializeField]
        private List<WorkOrder> orders =
            new();

        public IReadOnlyList<WorkOrder>
            Orders => orders;

        public void Accept(
            WorkOrder order)
        {
            order.Status =
                WorkOrderStatus.Accepted;

            CareerEvents
                .RaiseAccepted(order);
        }

        public void Complete(
            WorkOrder order)
        {
            order.Status =
                WorkOrderStatus.Completed;

            CareerEvents
                .RaiseCompleted(order);
        }
    }
}
