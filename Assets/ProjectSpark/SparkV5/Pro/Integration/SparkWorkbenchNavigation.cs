using System.Collections.Generic;
using ProjectSpark.Gameplay;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectSpark.SparkV5.Pro
{
    [DisallowMultipleComponent]
    public sealed class SparkWorkbenchNavigation : MonoBehaviour
    {
        [SerializeField] private BoxCollider tabletop;

        internal const float WalkTimeout = 24f;
        private const float SampleRadius = .24f;
        private const float MaximumStepHeight = .09f;
        private static readonly float[] Standoffs = { .65f, 1f, 1.45f, 2f };
        private readonly List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        private readonly List<Collider> colliders = new List<Collider>();
        private NavMeshData data;
        private NavMeshDataInstance registration;
        private NavMeshAgent boundAgent;
        private Transform boundLayout;
        private Bounds surface;

        internal bool TryGetTabletop(out BoxCollider collider)
        {
            collider = tabletop;
            return isActiveAndEnabled && collider && collider.enabled && collider.gameObject.activeInHierarchy;
        }

        internal bool NeedsRefresh(NavMeshAgent agent, Transform layout) =>
            boundAgent == agent && boundLayout != layout;

        internal bool TryPlan(SparkProCharacter character, Transform layout, SparkTerminal source,
            SparkTerminal destination, out Vector3 sourceStance, out Vector3 destinationStance, out string error)
        {
            sourceStance = destinationStance = default;
            if (!TryPrepare(character, layout, out var spawn, out var filter, out error))
                return false;

            if (!TryFindStance(spawn, source.transform.position, filter, out sourceStance) ||
                !TryFindStance(sourceStance, destination.transform.position, filter, out destinationStance))
            {
                error = "Spark cannot reach a clear tabletop position beside both terminals.";
                return false;
            }
            return true;
        }

        internal bool TryPrepare(SparkProCharacter character, Transform layout, out Vector3 spawn,
            out NavMeshQueryFilter filter, out string error, float placementRadius = 2.25f)
        {
            spawn = default;
            if (!TryEnsureSurface(character, layout, out filter, out error, rebuild: true)) return false;
            var agent = character.NavigationAgent;
            Vector3 previousPosition = agent.transform.position;
            if (!TrySample(previousPosition, placementRadius, filter, out spawn) ||
                Vector3.Distance(previousPosition, spawn) > placementRadius)
            {
                error = "Spark has no clear starting position on the tabletop.";
                return false;
            }

            agent.transform.position = spawn;
            agent.enabled = true;
            if (!agent.isOnNavMesh)
            {
                agent.enabled = false;
                agent.transform.position = previousPosition;
                error = "Spark could not attach to the tabletop NavMesh.";
                return false;
            }
            return true;
        }

        internal bool TryGetLanding(Vector3 position, NavMeshQueryFilter filter, out Vector3 landing) =>
            TrySample(position, .8f, filter, out landing);

        internal bool TryEnsureSurface(SparkProCharacter character, Transform layout,
            out NavMeshQueryFilter filter, out string error, bool rebuild = false)
        {
            filter = default;
            error = "Spark's tabletop or navigation agent is unavailable.";
            var agent = character ? character.NavigationAgent : null;
            if (!TryGetTabletop(out _) || !agent || !character.isActiveAndEnabled || !layout) return false;

            filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            error = null;
            if (!rebuild && registration.valid && boundAgent == agent && boundLayout == layout && surface == tabletop.bounds)
                return true;

            surface = tabletop.bounds;
            var settings = NavMesh.GetSettingsByID(agent.agentTypeID);
            if (settings.agentTypeID == -1)
            {
                error = "Spark's navigation agent type has no build settings.";
                return false;
            }

            // Bake only this table. Raised level props carve holes instead of becoming walking surfaces.
            settings.agentRadius = agent.radius;
            settings.agentHeight = agent.height;
            settings.agentClimb = MaximumStepHeight;
            settings.minRegionArea = .15f;
            settings.overrideVoxelSize = true;
            settings.voxelSize = .1f;
            sources.Clear();
            sources.Add(new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Box,
                transform = tabletop.transform.localToWorldMatrix * Matrix4x4.Translate(tabletop.center),
                size = tabletop.size,
                area = NavMesh.GetAreaFromName("Walkable")
            });
            int blockedArea = NavMesh.GetAreaFromName("Not Walkable");
            layout.GetComponentsInChildren(false, colliders);
            foreach (var collider in colliders)
            {
                if (!collider.enabled || collider.isTrigger) continue;
                var bounds = collider.bounds;
                if (bounds.max.y <= surface.max.y + MaximumStepHeight ||
                    bounds.min.y >= surface.max.y + agent.height ||
                    bounds.max.x <= surface.min.x || bounds.min.x >= surface.max.x ||
                    bounds.max.z <= surface.min.z || bounds.min.z >= surface.max.z)
                    continue;

                sources.Add(new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.ModifierBox,
                    transform = Matrix4x4.Translate(new Vector3(bounds.center.x, surface.max.y, bounds.center.z)),
                    size = new Vector3(bounds.size.x, .6f, bounds.size.z),
                    area = blockedArea
                });
            }
            colliders.Clear();
            var buildBounds = surface;
            buildBounds.Expand(new Vector3(1f, 4f, 1f));
            var next = NavMeshBuilder.BuildNavMeshData(settings, sources, buildBounds, Vector3.zero, Quaternion.identity);
            sources.Clear();
            if (!next)
            {
                error = "The tabletop NavMesh could not be built.";
                return false;
            }

            Release();
            agent.enabled = false;
            data = next;
            registration = NavMesh.AddNavMeshData(data);
            if (!registration.valid)
            {
                Release();
                error = "The tabletop NavMesh could not be registered.";
                return false;
            }

            boundAgent = agent;
            boundLayout = layout;
            return true;
        }

        private bool TryFindStance(Vector3 start, Vector3 terminal, NavMeshQueryFilter filter, out Vector3 stance)
        {
            stance = default;
            if (terminal.y < surface.max.y - .45f || terminal.y > surface.max.y + 1.8f)
                return false;

            float best = float.PositiveInfinity;
            var path = new NavMeshPath();
            foreach (float radius in Standoffs)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = i * Mathf.PI / 6f;
                    var point = terminal + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                    if (!TrySample(point, SampleRadius, filter, out var sample) ||
                        !NavMesh.CalculatePath(start, sample, filter, path) ||
                        path.status != NavMeshPathStatus.PathComplete)
                        continue;

                    var offset = sample - terminal;
                    offset.y = 0f;
                    float distance = offset.magnitude;
                    if (distance < .52f || distance > 2.1f) continue;

                    float walkingDistance = 0f;
                    var corners = path.corners;
                    for (int c = 1; c < corners.Length; c++)
                        walkingDistance += Vector3.Distance(corners[c - 1], corners[c]);
                    float score = distance * 2f + walkingDistance * .25f;
                    if (score >= best) continue;
                    best = score;
                    stance = sample;
                }
            }
            return !float.IsPositiveInfinity(best);
        }

        private bool TrySample(Vector3 point, float radius, NavMeshQueryFilter filter, out Vector3 position)
        {
            point.y = surface.max.y;
            position = default;
            if (!NavMesh.SamplePosition(point, out var hit, radius, filter) ||
                Mathf.Abs(hit.position.y - surface.max.y) > .2f ||
                hit.position.x < surface.min.x || hit.position.x > surface.max.x ||
                hit.position.z < surface.min.z || hit.position.z > surface.max.z)
                return false;
            position = hit.position;
            return true;
        }

        private void OnDisable() => Release();

        private void Release()
        {
            if (boundAgent) boundAgent.enabled = false;
            boundAgent = null;
            boundLayout = null;
            if (registration.valid) registration.Remove();
            if (data) Destroy(data);
            data = null;
        }
    }
}
