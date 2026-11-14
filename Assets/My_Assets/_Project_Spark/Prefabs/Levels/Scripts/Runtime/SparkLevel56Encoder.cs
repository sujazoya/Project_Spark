using UnityEngine;
namespace ProjectSpark.Gameplay
{
    internal sealed class SparkLevel56Encoder
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

