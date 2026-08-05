using UnityEngine;
using UnityEngine.UI;
namespace AAAUI
{
    [DisallowMultipleComponent]
    public sealed class Level1CircuitChecker : MonoBehaviour
    {
        [Header("Correct Terminals")]
        [SerializeField]
        private CircuitTerminal positiveSource;

        [SerializeField]
        private CircuitTerminal positiveLight;

        [SerializeField]
        private CircuitTerminal negativeSource;

        [SerializeField]
        private CircuitTerminal negativeLight;




        [Header("Light")]
        [SerializeField]
        private GameObject lightObject;

        private bool positiveConnected;
        private bool negativeConnected;
        private bool gameWon;

        public void RegisterConnection(
            CircuitTerminal a,
            CircuitTerminal b)
        {
            if (gameWon ||
                a == null ||
                b == null)
            {
                return;
            }

            Debug.Log(
                $"[LEVEL 1] Connection: {a.name} ({a.Polarity}) → {b.name} ({b.Polarity})"
            );

            if (IsPair(
                    a,
                    b,
                    positiveSource,
                    positiveLight))
            {
                positiveConnected = true;

                Debug.Log(
                    "[LEVEL 1] + → + CORRECT"
                );
            }

            if (IsPair(
                    a,
                    b,
                    negativeSource,
                    negativeLight))
            {
                negativeConnected = true;

                Debug.Log(
                    "[LEVEL 1] - → - CORRECT"
                );
            }

            CheckWin();
        }

        private bool IsPair(
            CircuitTerminal a,
            CircuitTerminal b,
            CircuitTerminal first,
            CircuitTerminal second)
        {
            return
                (a == first && b == second) ||
                (a == second && b == first);
        }

        private void CheckWin()
        {
            if (!positiveConnected ||
                !negativeConnected)
            {
                return;
            }

            gameWon = true;

            Debug.Log(
                "[LEVEL 1] GAME WON!"
            );

            if (lightObject != null)
            {
                lightObject.SetActive(true);
            }
        }
    }
}