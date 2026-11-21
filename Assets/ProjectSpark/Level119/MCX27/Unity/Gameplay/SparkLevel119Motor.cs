using UnityEngine;

namespace ProjectSpark.Gameplay.MotionCitadel
{
    // Averaged fictional PMSM teaching plant. The native circuit provides the bus,
    // phase and sensor permission inputs. This class models the mechanical load.
    internal sealed class SparkLevel119Motor
    {
        internal const float Step = .0005f;
        internal const float SpeedTarget = 51f;
        internal const float BrakeAt = 2.15f;
        internal const float RunLength = 4.25f;
        private const float Resistance = .24f, Inductance = .0036f, Flux = .025f;
        private const float Inertia = .0026f, Damping = .0018f, PolePairs = 3f;
        private const float LinkCapacitance = .028f, LinkLimit = 14.65f;
        private const float MaxCurrent = 3.4f, Sqrt3 = 1.7320508f, DeadTime = .024f;
        private float direct, quadrature, angle, measuredAngle, encoderAge;
        private float speedIntegrator, directIntegrator, quadratureIntegrator;
        internal float Bus { get; private set; }
        internal float Speed { get; private set; }
        internal float Torque { get; private set; }
        internal float BrakeTemp { get; private set; } = 24f;
        internal float BrakeEnergy { get; private set; }
        internal float RegenEnergy { get; private set; }
        internal float Elapsed { get; private set; }
        internal float PeakBus { get; private set; }
        internal float PeakCurrent { get; private set; }
        internal float CruiseSpeed { get; private set; }
        internal float CurrentU { get; private set; }
        internal float CurrentV { get; private set; }
        internal float CurrentW { get; private set; }
        internal float RotorDegrees => angle * Mathf.Rad2Deg;
        internal float QCurrent => quadrature;
        internal float EncoderError { get; private set; }
        internal float SpeedAtBrake { get; private set; }
        internal float PeakBrakeTemp { get; private set; }
        internal float CruiseTorque { get; private set; }
        internal float CruiseRmsSpread { get; private set; }
        internal bool Overvoltage { get; private set; }
        internal bool Pwm { get; private set; }
        internal int PhaseVClipped { get; private set; }
        internal int EncoderOutages { get; private set; }
        private float sumU, sumV, sumW, sumSpeed, sumTorque;
        private int cruiseSamples;

        internal bool DampedTuning { get; set; }

        internal void ClearDischargedLatch()
        {
            if (Bus < .6f) Overvoltage = false;
        }

        internal bool Qualified => Elapsed >= RunLength && !Overvoltage && PhaseVClipped == 0 && EncoderOutages == 0 &&
            CruiseSpeed >= 44f && CruiseSpeed <= 56f && SpeedAtBrake >= 45f && Mathf.Abs(Speed) <= 5f &&
            CruiseTorque >= .08f && CruiseTorque <= .26f && CruiseRmsSpread < .38f &&
            PeakCurrent < 4.7f && PeakBus <= 14.4f && BrakeEnergy > .28f && RegenEnergy > BrakeEnergy * .9f &&
            PeakBrakeTemp < 48f;

        internal void Begin()
        {
            Elapsed = Speed = direct = quadrature = angle = measuredAngle = encoderAge = 0f;
            speedIntegrator = directIntegrator = quadratureIntegrator = Torque = 0f;
            BrakeEnergy = RegenEnergy = PeakCurrent = CruiseSpeed = SpeedAtBrake = CruiseTorque = CruiseRmsSpread = 0f;
            sumU = sumV = sumW = sumSpeed = sumTorque = 0f;
            cruiseSamples = EncoderOutages = PhaseVClipped = 0;
            EncoderError = 0f;
            PeakBus = Bus;
            PeakBrakeTemp = BrakeTemp;
            Overvoltage = Pwm = false;
        }

        internal void CutPwm()
        {
            Pwm = false;
            Torque = 0f;
            directIntegrator = quadratureIntegrator = speedIntegrator = 0f;
        }

        internal void Advance(float supply, bool precharge, bool contactor, bool run, bool enable,
            bool highV, bool encoder, bool brakeSense)
        {
            if (precharge || contactor)
            {
                float seconds = contactor ? .10f : .40f;
                Bus += Mathf.Max(0f, supply - Bus) * Step / seconds;
            }
            else Bus = Mathf.Max(0f, Bus - Step * (2.6f + .34f * Bus));

            if (run) Elapsed += Step;
            if (encoder)
            {
                measuredAngle = angle;
                encoderAge = 0f;
            }
            else
            {
                encoderAge += Step;
                if (run)
                {
                    EncoderOutages++;
                    EncoderError = Mathf.Max(EncoderError, Mathf.Abs(Mathf.DeltaAngle(
                        measuredAngle * Mathf.Rad2Deg, angle * Mathf.Rad2Deg)));
                }
            }

            bool braking = Elapsed >= BrakeAt;
            if (run && !braking) SpeedAtBrake = Speed;
            bool runPwm = run && enable && !Overvoltage && encoderAge < .014f && Bus > 10.65f;
            float vAlpha = 0f, vBeta = 0f;
            float electricalAngle = angle * PolePairs;
            float omega = Speed * PolePairs;
            float alpha = direct * Mathf.Cos(electricalAngle) - quadrature * Mathf.Sin(electricalAngle);
            float beta = direct * Mathf.Sin(electricalAngle) + quadrature * Mathf.Cos(electricalAngle);
            if (runPwm)
            {
                float sensedAngle = measuredAngle * PolePairs;
                // Clarke and Park follow a positive U→V→W convention.
                float measuredD = alpha * Mathf.Cos(sensedAngle) + beta * Mathf.Sin(sensedAngle);
                float measuredQ = -alpha * Mathf.Sin(sensedAngle) + beta * Mathf.Cos(sensedAngle);
                float target = braking ? 0f : SpeedTarget;
                float eSpeed = target - Speed;
                float qRaw = (DampedTuning ? .22f : .28f) * eSpeed + speedIntegrator;
                float qRequest = Mathf.Clamp(qRaw, -MaxCurrent, MaxCurrent);
                speedIntegrator = Mathf.Clamp(speedIntegrator + (.95f * eSpeed + 5f * (qRequest - qRaw)) * Step,
                    -MaxCurrent, MaxCurrent);

                float dError = -measuredD;
                float qError = qRequest - measuredQ;
                float dRequest = 2.25f * dError + directIntegrator - omega * Inductance * measuredQ;
                float qVoltage = 2.25f * qError + quadratureIntegrator + omega * (Inductance * measuredD + Flux);
                Vector2 vector = Vector2.ClampMagnitude(new Vector2(dRequest, qVoltage), Bus * .53f);
                directIntegrator = Mathf.Clamp(directIntegrator + (62f * dError + 85f * (vector.x - dRequest)) * Step, -6f, 6f);
                quadratureIntegrator = Mathf.Clamp(quadratureIntegrator + (62f * qError + 85f * (vector.y - qVoltage)) * Step, -6f, 6f);

                float wantedAlpha = vector.x * Mathf.Cos(sensedAngle) - vector.y * Mathf.Sin(sensedAngle);
                float wantedBeta = vector.x * Mathf.Sin(sensedAngle) + vector.y * Mathf.Cos(sensedAngle);
                float dutyU = LimitDuty(.5f + wantedAlpha / Bus);
                float dutyV = LimitDuty(.5f + (-.5f * wantedAlpha + .5f * Sqrt3 * wantedBeta) / Bus);
                float dutyW = LimitDuty(.5f + (-.5f * wantedAlpha - .5f * Sqrt3 * wantedBeta) / Bus);
                if (!highV && dutyV > .5f)
                {
                    dutyV = .5f;
                    PhaseVClipped++;
                }
                float neutral = (dutyU + dutyV + dutyW) / 3f;
                vAlpha = (dutyU - neutral) * Bus;
                vBeta = (dutyV - dutyW) * Bus / Sqrt3;
                float actualD = vAlpha * Mathf.Cos(electricalAngle) + vBeta * Mathf.Sin(electricalAngle);
                float actualQ = -vAlpha * Mathf.Sin(electricalAngle) + vBeta * Mathf.Cos(electricalAngle);
                float nextD = direct + ((actualD - Resistance * direct) / Inductance + omega * quadrature) * Step;
                float nextQ = quadrature + ((actualQ - Resistance * quadrature - omega * Flux) / Inductance - omega * direct) * Step;
                direct = Mathf.Clamp(nextD, -7f, 7f);
                quadrature = Mathf.Clamp(nextQ, -7f, 7f);
            }
            else
            {
                direct *= 1f - Step * 125f;
                quadrature *= 1f - Step * 125f;
                if (Pwm) CutPwm();
            }
            Pwm = runPwm;
            Torque = runPwm ? 1.5f * PolePairs * Flux * quadrature : 0f;
            float load = !braking && Speed >= 1f ? .052f : 0f;
            Speed += (Torque - Damping * Speed - load) * (Step / Inertia);
            angle = Mathf.Repeat(angle + Speed * Step, Mathf.PI * 2f);

            CurrentU = alpha;
            CurrentV = -.5f * alpha + .5f * Sqrt3 * beta;
            CurrentW = -.5f * alpha - .5f * Sqrt3 * beta;
            float power = runPwm ? 1.5f * (vAlpha * alpha + vBeta * beta) : 0f;
            if (contactor && power < 0f)
            {
                float recovered = -power * Step;
                RegenEnergy += recovered;
                Bus = Mathf.Sqrt(Bus * Bus + 2f * recovered / LinkCapacitance);
            }
            if (contactor && power > 0f)
                Bus = Mathf.Max(0f, Bus - power * Step / (Mathf.Max(1f, Bus) * .34f));
            float sensedLink = Bus - (brakeSense ? 0f : 2.55f);
            float duty = contactor ? Mathf.Clamp01((sensedLink - 12.08f) * 3.1f) : 0f;
            if (duty > 0f)
            {
                float heat = Bus * Bus / 7.2f * duty * Step;
                BrakeEnergy += heat;
                Bus = Mathf.Sqrt(Mathf.Max(0f, Bus * Bus - 2f * heat / LinkCapacitance));
                BrakeTemp += heat / 5.8f;
            }
            BrakeTemp -= (BrakeTemp - 24f) * Step * .10f;
            if (contactor && Bus >= LinkLimit) { Overvoltage = true; CutPwm(); }
            if (!run) return;
            PeakBus = Mathf.Max(PeakBus, Bus);
            PeakBrakeTemp = Mathf.Max(PeakBrakeTemp, BrakeTemp);
            PeakCurrent = Mathf.Max(Mathf.Max(PeakCurrent, Mathf.Abs(CurrentU)), Mathf.Max(Mathf.Abs(CurrentV), Mathf.Abs(CurrentW)));
            if (Elapsed < 1.35f || braking) return;
            cruiseSamples++;
            sumSpeed += Speed;
            sumTorque += Torque;
            sumU += CurrentU * CurrentU;
            sumV += CurrentV * CurrentV;
            sumW += CurrentW * CurrentW;
            CruiseSpeed = sumSpeed / cruiseSamples;
            CruiseTorque = sumTorque / cruiseSamples;
            float u = Mathf.Sqrt(sumU / cruiseSamples), v = Mathf.Sqrt(sumV / cruiseSamples), w = Mathf.Sqrt(sumW / cruiseSamples);
            CruiseRmsSpread = Mathf.Max(Mathf.Max(u, v), w) - Mathf.Min(Mathf.Min(u, v), w);
        }

        private static float LimitDuty(float value) => Mathf.Clamp(value, DeadTime, 1f - DeadTime);
    }
}
