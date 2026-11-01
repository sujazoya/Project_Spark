using System.Collections;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;
using ProjectSpark.Electrical;

namespace ProjectSpark.Electrical
{
    /// <summary>
    /// Optional, minimal refresh adapter for shelf-placed resistors.
    /// Attach once to an always-active gameplay object and assign the shelf/solver.
    /// Does not alter component electrical properties or move GameObjects.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkShelfPlacementElectricalRefresh : MonoBehaviour
    {
        [SerializeField] private SparkObjectShelf shelf;
        [SerializeField] private SparkElectricalSolver solver;
        [SerializeField] private bool logPlacement = true;
        private Coroutine scheduled;
        private SparkResistor lastResistor;

        private void OnEnable()
        {
            if (shelf == null) shelf = FindFirstObjectByType<SparkObjectShelf>();
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (shelf != null) shelf.ItemPlaced += OnItemPlaced;
        }

        private void OnDisable()
        {
            if (shelf != null) shelf.ItemPlaced -= OnItemPlaced;
            if (scheduled != null) StopCoroutine(scheduled);
            scheduled = null;
        }

        private void OnItemPlaced(SparkShelfItem item, SparkShelfSlot slot)
        {
            if (item == null) return;
            SparkResistor resistor = item.GetComponentInChildren<SparkResistor>(true);
            if (resistor == null) return;
            lastResistor = resistor;
            if (scheduled != null) StopCoroutine(scheduled);
            scheduled = StartCoroutine(RefreshWhenPlacementSettles());
        }

        private IEnumerator RefreshWhenPlacementSettles()
        {
            // Allow placement parenting, connection creation and item activation to finish.
            yield return new WaitForEndOfFrame();
            yield return null;
            scheduled = null;
            if (solver == null || !solver.isActiveAndEnabled) yield break;

            solver.MarkTopologyDirty();
            bool solved = solver.SolveNow();
            if (!solved && logPlacement)
                Debug.LogWarning("[SPARK SHELF] Solve did not complete after placement. Check solver diagnostics.", this);

            if (logPlacement && lastResistor != null)
            {
                bool found = false;
                var components = solver.ElectricalComponents;
                for (int i = 0; i < components.Count; i++)
                {
                    if (components[i] == lastResistor) { found = true; break; }
                }
                if (!found && lastResistor.gameObject.activeInHierarchy)
                    Debug.LogWarning(
                        "[SPARK SHELF] Placed resistor is not in SparkElectricalSolver.ElectricalComponents. " +
                        "Check Component Search Root; placement refresh alone cannot discover an object outside that hierarchy.",
                        lastResistor);
                else
                    Debug.Log("[SPARK SHELF] Resistor placement processed; found by solver=" + found + ", solveSuccess=" + solved, lastResistor);
            }
        }
    }
}
