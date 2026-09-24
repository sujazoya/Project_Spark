
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Physical socket/jack that can accept a connector.
    ///
    /// This is purely a physical placement component.
    /// It does not simulate electricity.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkConnectorJack :
        MonoBehaviour
    {
        #region Inspector

        [Header("Jack")]
        [SerializeField]
        private string jackName = "USB Jack";

        [SerializeField]
        private SparkConnectorType acceptedConnector =
            SparkConnectorType.USB_A;

        [Header("Socket")]
        [SerializeField]
        private Transform socketTransform;

        [Header("Options")]
        [SerializeField]
        private bool occupied;

        #endregion


        #region Properties

        public string JackName =>
            jackName;

        public SparkConnectorType AcceptedConnector =>
            acceptedConnector;

        public Transform SocketTransform =>
            socketTransform != null
                ? socketTransform
                : transform;

        public bool IsOccupied =>
            occupied;

        #endregion


        #region Compatibility

        public bool CanAccept(
            SparkConnectorType connector)
        {
            return
                !occupied &&
                connector ==
                acceptedConnector;
        }

        #endregion


        #region Attach

        public bool Attach(
            SparkConnectorPlacement connector)
        {
            if (connector == null)
                return false;

            if (occupied)
                return false;

            if (!CanAccept(
                    connector.ConnectorType))
            {
                return false;
            }

            occupied = true;

            return true;
        }

        #endregion


        #region Detach

        public void Detach(
            SparkConnectorPlacement connector)
        {
            occupied = false;
        }

        #endregion
    }
}
