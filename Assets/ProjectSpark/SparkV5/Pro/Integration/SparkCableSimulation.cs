using Unity.Profiling;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    // Geometry only. SignalWireBuilder owns the preview mesh and the connection.
    internal sealed class SparkCableSimulation
    {
        private const float Step = 1f / 90f;
        private static readonly ProfilerMarker SolveMarker = new ProfilerMarker("Spark.NativeCable.Solve");
        private readonly RaycastHit[] hits = new RaycastHit[12];
        private Vector3[] offsets, previousOffsets;
        private Vector3 previousSource, previousGrip;
        private float accumulator;
        private float lastStepMotion;

        public Vector3[] Points { get; private set; }
        public float Length { get; private set; }
        public float Extension { get; private set; }
        public int Contacts { get; private set; }
        public int Iterations { get; private set; }
        public bool Settled(float tolerance) => lastStepMotion <= tolerance;

        public void Reset(Vector3 source, Vector3 grip, int segments, float length, int iterations)
        {
            int count = segments + 1;
            if (Points == null || Points.Length != count)
            {
                Points = new Vector3[count];
                offsets = new Vector3[count];
                previousOffsets = new Vector3[count];
            }
            Length = length;
            Iterations = iterations;
            accumulator = Step;
            lastStepMotion = float.PositiveInfinity;
            Contacts = 0;
            Extension = 0;
            previousSource = source;
            previousGrip = grip;
            for (int i = 0; i < count; i++)
            {
                offsets[i] = previousOffsets[i] = Vector3.zero;
                Points[i] = Vector3.Lerp(source, grip, i / (float)segments);
            }
        }

        public bool Advance(Vector3 source, Vector3 grip, float dt, float gravity, float damping,
            float stiffness, float radius, float extensionLimit, LayerMask layers)
        {
            using var profile = SolveMarker.Auto();
            Vector3 chord = grip - source;
            float span = chord.magnitude;
            Extension = Mathf.Max(0f, span / Mathf.Max(.001f, Length) - 1f);
            if (Extension > extensionLimit) return false;
            int end = Points.Length - 1;
            Vector3 tangent = span > .001f ? chord / span : Vector3.forward;
            Vector3 down = Vector3.ProjectOnPlane(Physics.gravity, tangent);
            float gravityAlongPlane = down.magnitude / 9.81f * gravity;
            down = down.normalized;
            // A loaded lead bows between its ends. A short visible span must
            // not be forced to store the entire lead as compressive kinks.
            float availableSag = .32f * Mathf.Sqrt(Mathf.Max(0f, Length * Length - span * span));
            float corridor = Mathf.Min(.27f, Mathf.Min(span * .20f, availableSag));
            Vector3 carryDelta = Vector3.ProjectOnPlane((grip - previousGrip) - (source - previousSource), tangent);
            accumulator = Mathf.Min(accumulator + Mathf.Max(dt, 0f), Step * 4f);
            Contacts = 0;
            Vector3 sag = down * (corridor * Mathf.Min(1f, gravityAlongPlane) * .70f);
            while (accumulator >= Step)
            {
                accumulator -= Step;
                lastStepMotion = 0;
                for (int i = 1; i < end; i++)
                {
                    float t = i / (float)end;
                    float envelope = 4f * t * (1f - t);
                    Vector3 current = Vector3.ProjectOnPlane(offsets[i], tangent);
                    Vector3 velocity = Vector3.ProjectOnPlane(offsets[i] - previousOffsets[i], tangent) * (1f - damping);
                    previousOffsets[i] = current;
                    Vector3 equilibrium = sag * envelope;
                    Vector3 next = current + velocity + (equilibrium - current) * (0.045f + stiffness * .11f);
                    next -= carryDelta * (.10f * t * envelope);
                    offsets[i] = Vector3.ClampMagnitude(next, corridor * envelope);
                    lastStepMotion = Mathf.Max(lastStepMotion, Vector3.Distance(current, offsets[i]));
                }
                carryDelta = Vector3.zero;
            }

            float bend = stiffness / Iterations;
            for (int pass = 0; pass < Iterations; pass++)
                for (int i = 1; i < end; i++)
                    offsets[i] = Vector3.Lerp(offsets[i], (offsets[i - 1] + offsets[i + 1]) * .5f, bend);

            Points[0] = source;
            Points[end] = grip;
            for (int i = 1; i < end; i++)
            {
                float t = i / (float)end;
                Vector3 axisPoint = source + chord * t;
                float limit = corridor * 4f * t * (1f - t);
                Vector3 offset = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(offsets[i], tangent), limit);
                if (radius > 0f && span > radius * 2f)
                {
                    Vector3 contact = Sweep(Points[i], axisPoint + offset, radius, layers);
                    offset = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(contact - axisPoint, tangent), limit);
                }
                // Each knot owns a fixed axial location. The curve can sag and
                // sway but cannot turn backward and wrap around an attachment.
                offsets[i] = offset;
                Points[i] = axisPoint + offset;
            }
            float maxLength = Length * (1f + extensionLimit);
            if (RouteLength(source, chord, end, 1f) > maxLength)
            {
                float low = 0f, high = 1f;
                for (int step = 0; step < 8; step++)
                {
                    float factor = (low + high) * .5f;
                    if (RouteLength(source, chord, end, factor) <= maxLength) low = factor;
                    else high = factor;
                }
                for (int i = 1; i < end; i++)
                    Points[i] = source + chord * (i / (float)end) + (offsets[i] *= low);
            }
            previousSource = source;
            previousGrip = grip;
            return true;
        }

        private float RouteLength(Vector3 source, Vector3 chord, int end, float factor)
        {
            Vector3 previous = source;
            float length = 0f;
            for (int i = 1; i <= end; i++)
            {
                Vector3 point = source + chord * (i / (float)end) + offsets[i] * factor;
                length += Vector3.Distance(previous, point);
                previous = point;
            }
            return length;
        }

        private Vector3 Sweep(Vector3 start, Vector3 finish, float radius, LayerMask layers)
        {
            Vector3 delta = finish - start;
            float distance = delta.magnitude;
            if (distance < .0001f) return finish;
            Vector3 direction = delta / distance;
            int count = Physics.SphereCastNonAlloc(start, radius, direction, hits, distance, layers,
                QueryTriggerInteraction.Ignore);
            int nearest = -1;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].distance <= .0001f || Vector3.Dot(hits[i].normal, direction) >= -.001f) continue;
                if (nearest < 0 || hits[i].distance < hits[nearest].distance) nearest = i;
            }
            if (nearest < 0) return finish;
            Contacts++;
            var hit = hits[nearest];
            return start + direction * Mathf.Max(0f, hit.distance - .001f) + hit.normal * .001f;
        }

    }
}
