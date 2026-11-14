using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Rigid shaft with two transverse translations and two small tilt angles.
    internal sealed class SparkLevel55Shaft
    {
        public const float Clearance = .0012f, Span = .42f, Gravity = 9.81f, Mass = .22f;
        private const float Inertia = .0055f, Gap = .0036f, CoilFactor = .013f;
        private Vector2 center = new(0, -Clearance), velocity, tilt, angularVelocity;
        private Vector2 leftPositive, leftNegative, rightPositive, rightNegative;
        public Vector2 Left => center - tilt * (Span / 2);
        public Vector2 Right => center + tilt * (Span / 2);
        public Vector2 LeftVelocity => velocity - angularVelocity * (Span / 2);
        public Vector2 RightVelocity => velocity + angularVelocity * (Span / 2);
        public float Tilt => tilt.magnitude;
        public bool Contact { get; private set; } = true;
        public bool LeftContact { get; private set; } = true;
        public bool RightContact { get; private set; } = true;
        public bool Limited { get; private set; }
        public Vector2 LeftForce { get; private set; }
        public Vector2 RightForce { get; private set; }
        public float LeftCoil => Mathf.Max(leftPositive.x,leftPositive.y,leftNegative.x,leftNegative.y);
        public float RightCoil => Mathf.Max(rightPositive.x,rightPositive.y,rightNegative.x,rightNegative.y);

        public void Step(float dt, float angle, float omega, Vector2 leftDemand, Vector2 rightDemand,
            float leftBus, float rightBus, float lift, Vector2 leftDisturbance, Vector2 rightDisturbance)
        {
            Limited = false;
            LeftForce = Magnetic(Left, leftDemand * lift, leftBus, ref leftPositive, ref leftNegative, dt);
            RightForce = Magnetic(Right, rightDemand * lift, rightBus, ref rightPositive, ref rightNegative, dt);
            Vector2 imbalance = .00042f * omega * omega * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var left = LeftForce + leftDisturbance + imbalance;
            var right = RightForce + rightDisturbance + new Vector2(-imbalance.y, imbalance.x) * .82f;
            velocity += ((left + right) / Mass + Vector2.down * Gravity - velocity * .8f) * dt;
            angularVelocity += ((right - left) * (Span / (2 * Inertia)) - angularVelocity * 1.6f) * dt;
            center += velocity * dt;
            tilt += angularVelocity * dt;
            LeftContact = RightContact = false;
            for (int i=0;i<4;i++)
            {
                LeftContact |= Constrain(-Span/2);
                RightContact |= Constrain(Span/2);
            }
            Contact = LeftContact || RightContact;
        }

        private Vector2 Magnetic(Vector2 position,Vector2 request,float bus,
            ref Vector2 positive,ref Vector2 negative,float dt)
        {
            if (bus < .04f)
            {
                positive = Vector2.zero;
                negative = Vector2.zero;
                return Vector2.zero;
            }
            float maximum = Mathf.Min(.089f, bus * .88f);
            float Axis(float offset,float demand, ref float p, ref float n)
            {
                float gp=Mathf.Max(.0015f,Gap-offset);
                float gn=Mathf.Max(.0015f,Gap+offset);
                float constrained=Mathf.Clamp(demand,-4.5f,4.5f);
                float bias=.28f;
                float iP=gp*Mathf.Sqrt((bias+Mathf.Max(0,constrained))/CoilFactor);
                float iN=gn*Mathf.Sqrt((bias+Mathf.Max(0,-constrained))/CoilFactor);
                Limited |= Mathf.Abs(demand)>4.5f || iP>maximum || iN>maximum;
                p += (Mathf.Min(iP,maximum)-p) * dt/(.0008f+dt);
                n += (Mathf.Min(iN,maximum)-n) * dt/(.0008f+dt);
                return Mathf.Clamp(CoilFactor*(p*p/(gp*gp)-n*n/(gn*gn)),-4.5f,4.5f);
            }
            return new(Axis(position.x,request.x,ref positive.x,ref negative.x),
                Axis(position.y,request.y,ref positive.y,ref negative.y));
        }

        private bool Constrain(float arm)
        {
            Vector2 tip = center + tilt * arm;
            if (tip.sqrMagnitude < Clearance*Clearance) return false;
            Vector2 normal = tip.normalized;
            float inverseMass = 1/Mass + arm*arm/Inertia;
            Vector2 push = -normal * ((tip.magnitude-Clearance) / inverseMass);
            center += push / Mass;
            tilt += push * (arm/Inertia);
            float outward = Vector2.Dot(velocity + angularVelocity*arm,normal);
            if (outward > 0)
            {
                Vector2 impulse = -normal * (outward/inverseMass);
                velocity += impulse / Mass;
                angularVelocity += impulse * (arm / Inertia);
            }
            return true;
        }
    }

    internal sealed class SparkLevel55Sensor
    {
        private readonly float mountingOffset;
        private float offset, last;
        public float Position { get; private set; }
        public float Velocity { get; private set; }
        public bool Calibrated { get; private set; }
        public bool Valid { get; private set; }
        public SparkLevel55Sensor(float mountingOffset) { this.mountingOffset=mountingOffset; }
        public void Calibrate(float raw, float reference)
        {
            offset = raw + mountingOffset - reference;
            Position = last = reference;
            Velocity = 0;
            Calibrated = true;
        }
        public void Invalidate() => Calibrated = false;
        public void Sample(float dt,float raw,bool signal)
        {
            Valid = signal && float.IsFinite(raw) && Mathf.Abs(raw+mountingOffset)<.0025f;
            if (!Valid) return;
            Position += (raw + mountingOffset - offset - Position) * (dt / (.0005f + dt));
            Velocity += ((Position-last)/dt-Velocity) * (dt/(.001f+dt));
            last=Position;
        }
    }
    internal sealed class SparkLevel55Encoder
    {
        private const int CountsPerTurn = 80;
        private static readonly int[] Gray = {0,2,3,1};
        private static readonly int[] Delta = {0,-1,1,0,1,0,0,-1,-1,0,0,1,0,1,-1,0};
        private int edge, state, count, indexCount, tachCount;
        private float tachClock;
        public bool Locked { get; private set; }
        public bool Index { get; private set; }
        public float Rpm { get; private set; }

        public void Reacquire(float angle)
        {
            edge = ToEdge(angle);
            state = Gray[edge & 3];
            count = indexCount = tachCount = 0;
            tachClock = Rpm = 0;
            Locked = Index = false;
        }

        private static int ToEdge(float angle) => Mathf.FloorToInt(angle * (CountsPerTurn / (2 * Mathf.PI)));

        public bool Step(float dt,float angle,bool a,bool b,bool z)
        {
            Index = false;
            if (!a || !b || !z)
            {
                Reacquire(angle);
                return false;
            }
            int end = ToEdge(angle);
            while (edge < end)
            {
                int received = Gray[++edge & 3];
                int transition = Delta[4*state+received];
                state = received;
                if (transition != 1)
                {
                    Reacquire(angle);
                    return false;
                }
                count += transition;
                if (edge % CountsPerTurn != 0) continue;
                // An incomplete turn after a stop cannot invalidate the first Z pulse.
                // Require a full count before retaining an already acquired reference.
                bool indexValid = !Locked || count-indexCount == CountsPerTurn;
                indexCount = count;
                Index = indexValid;
                Locked = indexValid;
            }
            tachClock += dt;
            if (tachClock>=.60f)
            {
                Rpm=(count-tachCount)*60/(CountsPerTurn*tachClock);
                tachCount=count;
                tachClock=0;
            }
            return true;
        }
    }
}
