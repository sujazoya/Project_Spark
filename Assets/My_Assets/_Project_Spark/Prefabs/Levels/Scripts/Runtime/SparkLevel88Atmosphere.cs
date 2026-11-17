using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Fictional training units. Gas transfer moves each species and its heat together.
    public sealed class SparkLevel88Atmosphere
    {
        public sealed class Zone
        {
            public float Oxygen = 21, Carbon = .45f, Vapor = 1.1f, Inert = 77;
            public float Heat = 99.55f * 294;
            public float Total => Oxygen + Carbon + Vapor + Inert;
            public float Temperature => Heat / Mathf.Max(Total, .001f);
            public float Pressure => Total * Temperature / 294;
            public float OxygenPressure => Oxygen * Temperature / 294;
            public float CarbonPressure => Carbon * Temperature / 294;
        }

        public readonly Zone[] Zones = { new(), new(), new(), new() };
        public readonly float[] Dampers = new float[4];
        public readonly float[] Beds = new float[2];
        public float Water { get; private set; } = 90;
        public float Recovered { get; private set; }
        public float Energy { get; private set; } = 360;
        public float Hydrogen { get; private set; }
        public float CapturedCarbon { get; private set; }
        public float Reserve { get; private set; } = 18;
        public float Fan { get; private set; }
        public float OxygenFlow { get; private set; }
        public float Condenser { get; private set; }
        public float Regulator { get; private set; }
        public float Clock { get; private set; }
        public int ActiveBed => (int)(Clock / 8) % 2;
        public bool Healthy => Array.TrueForAll(Zones, z => z.Pressure is > 96 and < 104 &&
            z.OxygenPressure is > 18 and < 24 && z.CarbonPressure < 1.5f &&
            z.Vapor is > .22f and < 2.8f && z.Temperature is > 289 and < 300);
        public bool Ventilated => Fan > .85f && Array.TrueForAll(Dampers, v => v > .8f);

        public void Tick(float dt, float demand, bool ventilate, bool scrub, bool oxygen, bool water, bool regulate, int fault)
        {
            Clock += dt;
            Fan = Mathf.MoveTowards(Fan, ventilate ? 1 : 0, dt * .9f);
            float deliveredOxygen = 0;
            Condenser = Mathf.MoveTowards(Condenser, water && fault != 2 ? 1 : 0, dt);
            Regulator = Mathf.MoveTowards(Regulator, regulate ? 1 : 0, dt);
            int bed = ActiveBed;
            if (scrub && Energy > dt * .18f)
            {
                float regenerated = Mathf.Min(Beds[1-bed], dt * .12f);
                Beds[1-bed] -= regenerated;
                CapturedCarbon += regenerated;
                Energy -= dt * .18f;
            }
            for (int i = 0; i < Zones.Length; i++)
            {
                var zone = Zones[i];
                Dampers[i] = Mathf.MoveTowards(Dampers[i], ventilate ? 1 : 0, dt);
                float used = Mathf.Min(zone.Oxygen, dt * demand * (.021f + i * .002f));
                zone.Oxygen -= used;
                zone.Carbon += used;
                AddVapor(zone, used * .4f);
                if (scrub && Energy > 0 && fault != 1)
                {
                    float capture = Mathf.Min(zone.Carbon, (1.6f - Beds[bed]) * .2f, Fan * Dampers[i] * dt * .07f);
                    zone.Heat -= capture * zone.Temperature;
                    zone.Carbon -= capture;
                    Beds[bed] += capture;
                }
                if (oxygen && Water > 0 && Energy > 0)
                {
                    float requested = Mathf.Clamp(used / dt + (21 - zone.OxygenPressure) * .25f, 0, .065f);
                    float flow = Mathf.Min(Water * .5f, Energy / 5, Fan * dt * requested);
                    deliveredOxygen += flow;
                    Water -= 2 * flow;
                    Energy -= 5 * flow;
                    Hydrogen += 2 * flow;
                    zone.Oxygen += flow;
                    zone.Heat += flow * 294;
                }
                float condensed = Mathf.Min(zone.Vapor, Mathf.Max(0, zone.Vapor - .8f) * dt * Condenser * .25f);
                AddVapor(zone, -condensed);
                Recovered += condensed;
                float reclaim = water ? Mathf.Min(Recovered, dt * .012f) : 0;
                Water += reclaim;
                Recovered -= reclaim;
                if (fault == 3 && i == 1) Transfer(zone, null, dt * .012f);
                if (zone.Pressure > 103) Transfer(zone, null, dt * .032f);
                if (regulate && zone.Pressure < 98 && Reserve > 0)
                {
                    float add = Mathf.Min(Reserve, dt * .9f * Regulator, 98 - zone.Pressure);
                    Reserve -= add;
                    zone.Inert += add;
                    zone.Heat += add * 294;
                }
                if (Fan > 0 && Energy > dt * .09f)
                {
                    Energy -= dt * .09f;
                    zone.Heat += (294 + demand * 1.3f - zone.Temperature) * zone.Total * dt * Fan * .45f;
                }
            }
            OxygenFlow = Mathf.MoveTowards(OxygenFlow, deliveredOxygen / (dt * .26f), dt);
            for (int i = 0; i < Zones.Length; i++)
            {
                int next = (i+1) % Zones.Length;
                float exchange = dt * Fan * Mathf.Min(Dampers[i], Dampers[next]) * .018f;
                Transfer(Zones[i], Zones[next], exchange);
                Transfer(Zones[next], Zones[i], exchange / (1 + exchange));
            }
        }

        private static void AddVapor(Zone zone, float amount)
        {
            zone.Heat += amount * zone.Temperature;
            zone.Vapor += amount;
        }

        private static void Transfer(Zone from, Zone to, float fraction)
        {
            float oxygen = from.Oxygen * fraction, carbon = from.Carbon * fraction;
            float vapor = from.Vapor * fraction, inert = from.Inert * fraction, heat = from.Heat * fraction;
            from.Oxygen -= oxygen; from.Carbon -= carbon; from.Vapor -= vapor; from.Inert -= inert; from.Heat -= heat;
            if (to == null) return;
            to.Oxygen += oxygen; to.Carbon += carbon; to.Vapor += vapor; to.Inert += inert; to.Heat += heat;
        }
    }
}


