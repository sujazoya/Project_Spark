using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel95Process
    {
        public SparkTerminal loadPin, loadPositive, dutyReference, dutyHigh;
        public SparkResistor timingA, timingB, trimHigh, trimLow;
        public SparkSwitch capacitanceFault;
        public SparkCapacitor timingCapacitor, extraCapacitor;
        private float rawHz, dividedHz, slowHz, actualDuty, loadHigh, loadLow;
        private float captureTime, captureDutyArea, requestedDuty = 50f;
        private double clockCycles, captureStartCycles;
        private bool sampledOn, sampledOff;
        private int provedDuty;
        private int repairedPending = -1;

        private void FinishRoute(SparkTerminal end)
        {
            effects.Emit(end.transform.position, 1);
            backLayer = false;
            focusPending = true;
            QueueCheckpoint();
            if (route == JumperIndex)
            {
                notice = lost ? "W049 fitted. Capture fresh feedback before resetting the latch." :
                    "W049 fitted. Switch on BENCH POWER.";
                return;
            }

            notice = $"W{route + 1:000} routed. Follow the highlighted pads.";
            if (stage != 1) return;
            if (routed == CopperCount)
            {
                route = JumperIndex;
                notice = "Copper complete. Fit W049 across the highlighted J3 contacts.";
                return;
            }
            for (int i = 0; i < CopperCount; i++)
            {
                route = (route + 1) % CopperCount;
                if (!RouteConnected(routes[route])) break;
            }
            if (routes[route].via && !routes[route].via.IsClosed)
                notice = $"Plate {routes[route].viaName} before drawing both copper segments.";
        }

        private bool Voltage(SparkTerminal pin, out float voltage)
        {
            voltage = 0;
            var result = manager.ElectricalSolver.LatestSolveResult;
            return result != null && result.TryGetVoltage(pin,supplyReturn,out voltage);
        }

        private void Update()
        {
            if (!Active) return;
            ReadGraph();
            clock += Time.deltaTime;
            bool protectedPower = supply.IsOutputActive && !supply.IsOverloaded &&
                Voltage(protectedInput,out float input) && input is > 3.65f and < 5.15f;
            Output(regulator,protectedPower);
            validRail = protectedPower && Voltage(rail,out float monitor) && monitor is > 3.15f and < 3.45f;

            // TLC555 astable approximation: charge through R1+R2, discharge through R2.
            bool oscillatorReady = protectedPower && RouteConnected(routes[8]);
            for (int i = 17; oscillatorReady && i < 27; i++)
                oscillatorReady = RouteConnected(routes[i]);
            double capacitance = timingCapacitor.CapacitanceFarads +
                (capacitanceFault.IsClosed ? extraCapacitor.CapacitanceFarads : 0f);
            double period = Math.Log(2d) * (timingA.Resistance + 2d * timingB.Resistance) * capacitance;
            rawHz = oscillatorReady && period > 0 ? (float)(1d / period) : 0f;
            bool dividerReady = rawHz > 0 && RouteConnected(routes[27]) && RouteConnected(routes[28]) &&
                RouteConnected(routes[29]) && RouteConnected(routes[30]);
            dividedHz = dividerReady ? rawHz / 4f : 0f;
            slowHz = dividerReady ? rawHz / 512f : 0f;

            actualDuty = Voltage(dutyReference,out float reference) &&
                Voltage(dutyHigh,out float fullScale) && fullScale > 3f ? Mathf.Clamp01(reference / fullScale) : 0f;
            bool jumper = RouteConnected(routes[JumperIndex]);
            bool feedbackMeasured = Voltage(feedbackInput,out float received) && Voltage(loadPin,out float switched) &&
                Mathf.Abs(received-switched) < .12f;
            trusted = validRail && jumper && feedbackMeasured && fault < 0;
            if (armed && !jumper)
            {
                armed = false;
                lost = true;
                measured = reset = running = false;
                QueueCheckpoint();
                effects.Emit(routes[JumperIndex].from.transform.position,2);
                notice = "W049 open. Output latched off. Refit the jumper, then capture feedback.";
            }

            bool outputPermitted = trusted && !lost && dividedHz > 0 && routed == CopperCount &&
                Mathf.Abs(rawHz-1024f) < 32f && actualDuty is > .1f and < .9f;
            // The native solver settles both DC switch states. The scope displays
            // the equivalent faster timing model and voltage-weighted PWM duty.
            bool nextLoad = outputPermitted && clock % .56f < .28f;
            if (nextLoad != load)
            {
                load = nextLoad;
                loadGate.SetState(load ? SparkSwitchState.Closed : SparkSwitchState.Open);
            }

            // Measure across R9. The board return itself can sit above the bench reference.
            shuntVoltage = 0f;
            manager.ElectricalSolver.LatestSolveResult?.TryGetVoltage(shunt,routes[36].from,out shuntVoltage);
            if (Voltage(loadPin,out float drain) && Voltage(loadPositive,out float positive))
            {
                if (load && shuntVoltage > .001f) loadHigh = Mathf.Max(0,positive-drain);
                if (!load && Mathf.Abs(shuntVoltage) < .0003f) loadLow = Mathf.Max(0,positive-drain);
            }

            if (!jumper || routed != CopperCount)
            {
                if (stage == 3) provedDuty = 0;
            }
            clockCycles += rawHz * Time.deltaTime;
            if (stage == 1 && validRail && trusted && routed == CopperCount)
            {
                Award();
                notice = "Network commissioned. CAPTURE / MEASURE to qualify the clocks.";
            }
            if (running) UpdateCapture(outputPermitted);
            Present();
            if (followRoute && focusPending && !AAAUI.VFX.SignalWireBuilder.AnyWireDrawing)
            {
                focusPending = false;
                FocusCurrent();
            }
            if (Time.unscaledTime >= checkpointDue) FlushCheckpoint();
        }

        internal bool TryReadInstrumentPad(string id, out float voltage)
        {
            voltage = 0f;
            return Active && loader.ActiveBindings.TryGetTerminal(id,out var pin) && Voltage(pin,out voltage);
        }

        internal string SupplyReadout()
        {
            string input = Voltage(protectedInput,out float beforeRegulator) ? $"{beforeRegulator:F3} V" : "—";
            string monitor = Voltage(rail,out float afterRegulator) ? $"{afterRegulator:F3} V" : "—";
            return supply.IsOutputActive
                ? $"BENCH   {supply.OutputVoltage:F2} V    {Mathf.Abs(supply.ElectricalState.Current)*1000f:F1} mA\nAFTER D1   {input}    /    U5 MONITOR   {monitor}"
                : "OUTPUT OFF    /    Use ENABLE OUTPUT to energize the profiled board.";
        }

        internal string LogicReadout()
        {
            if (rawHz <= 0) return "NO CLOCK EDGES   /   Check bench power and the U1 timing path.";
            string raw = $"TP01  RAW   {rawHz:F1} Hz   /   {1000f / rawHz:F3} ms";
            string clock = dividedHz > 0
                ? $"TP02  ÷4   {dividedHz:F2} Hz   /   TP03  ÷512   {slowHz:F3} Hz" 
                : "TP02 / TP03   NO EDGES   /   Inspect V06 and W028.";
            string gated = trusted && !lost && dividedHz > 0
                ? $"PWM WIDTH   {1000f * actualDuty / dividedHz:F3} ms"
                : "OUTPUT INHIBITED";
            return $"{raw}\n{clock}\n{gated}";
        }

        internal bool LogicOutputEnabled => trusted && !lost && dividedHz > 0 && Mathf.Abs(rawHz-1024f)<32f;

        internal void UpdateInstrumentScope(SparkLevel95Scope display) =>
            display.UpdateDisplay(rawHz,dividedHz,slowHz,actualDuty,loadHigh,loadLow,LogicOutputEnabled);

        private void UpdateCapture(bool outputPermitted)
        {
            if (!outputPermitted || !trusted || routed != CopperCount || lost)
            {
                running = false;
                notice = "Capture invalid. Check power, clock path, duty reference and W049.";
                return;
            }
            captureTime += Time.deltaTime;
            captureDutyArea += actualDuty * Time.deltaTime;
            sampledOn |= load && shuntVoltage > .001f && loadHigh > 3.4f;
            sampledOff |= !load && Mathf.Abs(shuntVoltage) < .0003f && loadLow < .3f;
            if (captureTime < 2.1f) return;

            running = false;
            float master = (float)((Math.Floor(clockCycles) - Math.Floor(captureStartCycles)) / captureTime);
            float divider = (float)((Math.Floor(clockCycles/4d) - Math.Floor(captureStartCycles/4d)) / captureTime);
            float slow = (float)((Math.Floor(clockCycles/512d) - Math.Floor(captureStartCycles/512d)) / captureTime);
            float duty = captureDutyArea / captureTime;
            bool clocksPass = Mathf.Abs(master-1024f)<32f && Mathf.Abs(divider-256f)<10f && Mathf.Abs(slow-2f)<.5f;
            bool pwmPass = sampledOn && sampledOff && Mathf.Abs(duty-requestedDuty/100f)<.035f;
            if (!clocksPass || stage >= 3 && !pwmPass)
            {
                notice = $"Capture failed: {duty:P1} / {requestedDuty}% | ON {loadHigh:F3} V ({sampledOn}) OFF {loadLow:F3} V ({sampledOff}) | {master:F0}/{divider:F0}/{slow:F2} Hz.";
                return;
            }
            if (stage == 3)
            {
                provedDuty |= requestedDuty == 25 ? 1 : requestedDuty == 50 ? 2 : 4;
                if (provedDuty == 7) Award();
                QueueCheckpoint();
            }
            else if (stage == 2 || stage == 5 && reset) Award();
            else if (stage == 4 && repairedPending >= 0)
            {
                repairs[repairedPending] = true;
                repairedPending = -1;
                if (Array.TrueForAll(repairs, passed => passed)) Award();
                QueueCheckpoint();
                notice = "Repair retest passed. " + (stage == 5 ? "Arm W049 for the recovery exercise." : "Select the next fault.");
                return;
            }

            int duties = ((provedDuty & 1)>0 ? 1 : 0) + ((provedDuty & 2)>0 ? 1 : 0) + ((provedDuty & 4)>0 ? 1 : 0);
            notice = $"RUN {run}  /  {master:F0} / {divider:F0} / {slow:F2} Hz  /  PWM {duty:P0}  /  DUTIES {duties}/3";
        }

        public void TogglePower()
        {
            if (!Active || stage < 1) return;
            ReadGraph();
            if (!supply.IsOutputActive && (stage==1 && (routed!=CopperCount || !RouteConnected(routes[JumperIndex]))))
            {
                notice = "Finish the 48 copper tasks and fit W049 before applying power.";
                return;
            }
            Output(supply,!supply.IsOutputActive);
            running = false;
            notice = supply.IsOutputActive ? "Bench power on. Wait for a valid monitor rail." : "Bench power off.";
        }

        public void Duty25() => SetDuty(25);
        public void Duty50() => SetDuty(50);
        public void Duty75() => SetDuty(75);

        private void SetDuty(float duty)
        {
            if (!Active || running) return;
            requestedDuty = duty;
            trimHigh.SetResistance(10000f * (1f-duty/100f));
            trimLow.SetResistance(10000f * duty/100f);
            notice = $"RV1 set to {duty}% duty. Capture once the reference settles.";
        }

        public void TestBoard()
        {
            if (!Active || stage is < 2 or > 5 || running) return;
            if (!validRail || !trusted || !AllCopper() || fault >= 0 || lost || (stage == 5 && !reset))
            {
                notice = "Restore clock path, valid rail and W049 before capturing.";
                return;
            }
            sampledOn = sampledOff = false;
            captureTime = captureDutyArea = loadHigh = loadLow = 0f;
            captureStartCycles = clockCycles;
            running = true;
            run++;
            notice = "Scope acquiring clock edges and both load states.";
        }

        public void InjectVia() => Inject(0);
        public void InjectPullup() => Inject(1);
        public void InjectBridge() => Inject(2);

        private void Inject(int kind)
        {
            if (!Active || stage != 4 || running || fault >= 0 || repairedPending >= 0 ||
                kind != Array.FindIndex(repairs, passed => !passed) || !supply.IsOutputActive) return;
            fault = kind;
            measured = false;
            if (kind == 0) routes[27].via.SetState(SparkSwitchState.Open);
            else if (kind == 1) capacitanceFault.SetState(SparkSwitchState.Closed);
            else bridge.SetState(SparkSwitchState.Closed);
            notice = "Fault active. Use Measure to diagnose it.";
            QueueCheckpoint();
            effects.Emit(board.TransformPoint(new Vector3(0,1.3f,0)),2);
        }

        public void Measure()
        {
            if (!Active || running) return;
            ReadGraph();
            if (fault == 0 && rawHz > 0 && dividedHz == 0 && !RouteConnected(routes[27]))
            {
                measured = true;
                notice = "V06 open: raw clock survives, divided clocks are absent. Power off to repair.";
            }
            else if (fault == 1 && rawHz is > 400 and < 550)
            {
                measured = true;
                notice = $"C1 reads 22 nF in circuit. Master is {rawHz:F0} Hz. Power off to replace C1.";
            }
            else if (fault == 2 && bridge.IsClosed)
            {
                measured = true;
                notice = "Q1 gate-to-drain continuity detected. Power off to remove the bridge.";
            }
            else if (lost && validRail && trusted)
            {
                measured = true;
                notice = "Fresh W049 feedback confirmed. Select RESET SAFETY.";
            }
            else if (fault < 0) TestBoard();
            else notice = "Power the board and wait for the diagnostic readings.";
        }

        public void Repair()
        {
            if (!Active || fault < 0 || !measured) return;
            if (supply.IsOutputActive)
            {
                notice = "Isolate the bench before repairing.";
                return;
            }
            if (fault == 0) routes[27].via.SetState(SparkSwitchState.Closed);
            else if (fault == 1) capacitanceFault.SetState(SparkSwitchState.Open);
            else bridge.SetState(SparkSwitchState.Open);
            repairedPending = fault;
            fault = -1;
            measured = false;
            notice = "Repair applied. Restore bench power and capture a fresh result.";
            QueueCheckpoint();
            effects.Emit(board.TransformPoint(new Vector3(0,1.3f,0)),0);
        }
    }
}
