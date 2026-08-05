using UnityEngine;

namespace ProjectSpark.Gameplay.Placement
{
    public sealed class PlacementManager : MonoBehaviour
    {
        [SerializeField]
        private LayerMask collisionMask;

        private readonly PlacementPreview _preview =
            new();

        private readonly PlacementValidator _validator =
            new();

        private PlacementSession _session;

        public void Begin(GameObject prefab)
        {
            _session = new PlacementSession
            {
                Prefab = prefab,
                Preview = Instantiate(prefab),
                Rotation = Quaternion.identity,
                State = PlacementState.Preview
            };

            PlacementEvents.RaiseStarted();
        }

        public void UpdatePlacement(Vector3 position)
        {
            if (_session == null)
                return;

            _session.Position = position;

            _preview.UpdateTransform(_session);

            _session.State =
                _validator.Validate(_session)
                    ? PlacementState.Valid
                    : PlacementState.Invalid;
        }

        public void Confirm()
        {
            if (_session == null)
                return;

            if (_session.State != PlacementState.Valid)
                return;

            Instantiate(
                _session.Prefab,
                _session.Position,
                _session.Rotation);

            Destroy(_session.Preview);

            PlacementEvents.RaiseConfirmed();

            _session = null;
        }

        public void Cancel()
        {
            if (_session == null)
                return;

            Destroy(_session.Preview);

            PlacementEvents.RaiseCancelled();

            _session = null;
        }
    }
}
