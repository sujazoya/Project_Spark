using UnityEngine;

namespace AAAUI
{
    [DisallowMultipleComponent]
    public sealed class Level1CircuitChecker : MonoBehaviour
    {
        [Header("Required Connections")]
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
        private bool won;

        public void RegisterConnection(
            CircuitTerminal a,
            CircuitTerminal b)
        {
            if (won ||
                a == null ||
                b == null)
                return;

            CheckPositive(
                a,
                b
            );

            CheckNegative(
                a,
                b
            );

            CheckWin();
        }

        private void CheckPositive(
            CircuitTerminal a,
            CircuitTerminal b)
        {
            if ((a == positiveSource &&
                 b == positiveLight) ||
                (a == positiveLight &&
                 b == positiveSource))
            {
                positiveConnected = true;
            }
        }

        private void CheckNegative(
            CircuitTerminal a,
            CircuitTerminal b)
        {
            if ((a == negativeSource &&
                 b == negativeLight) ||
                (a == negativeLight &&
                 b == negativeSource))
            {
                negativeConnected = true;
            }
        }

        private void CheckWin()
        {
            if (!positiveConnected ||
                !negativeConnected)
            {
                return;
            }

            won = true;

            if (lightObject != null)
            {
                lightObject.SetActive(true);
            }

            GameWon();
        }

        private void GameWon()
        {
            Debug.Log(
                "LEVEL 1 COMPLETE!"
            );

            // Call your game-won system here.
        }
    }
}