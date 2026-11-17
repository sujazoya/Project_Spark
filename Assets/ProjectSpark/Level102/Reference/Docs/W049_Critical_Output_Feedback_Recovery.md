# W049 — TRUSTED PWM FEEDBACK AND PROTECTED OUTPUT RECOVERY

- Physical service part: `L95_W049_DetachableMonitorJumper`.
- Contact source: `J3.A`, branch fed from actual J2.LOAD_NEG output-monitor path.
- Contact destination: `J3.B`, branch to `U7.TRUSTED_PWM_MONITOR_IN`.
- Functional block source: `L95_PWMOutputFeedbackMonitor.TRUSTED_PWM_MONITOR_OUT`.
- Functional safety input: `L95_TimingSafetyMonitor_U7.TRUSTED_PWM_MONITOR_IN`.
- Initial state: **W049 is physically uninstalled**. It must be installed to commission normal operation.

### Certified fault sequence (Objective 6, after completed Objectives 1–5)
1. Under a guarded low-energy test, player removes the *real* W049 component.
2. The authoritative circuit/connection graph reports J3.A -> J3.B **open**.
3. Any cached PWM monitor reading is invalidated; independently supervised protected output is disabled.
4. A critical feedback fault latches. Diagnostic oscillator and oscilloscope reading remain available where electrically safe.
5. Player reinstalls the physical jumper with proper contact quality and matching orientation.
6. New measured monitor samples and cross-checks must agree with actual output behavior. Previously cached samples are ineligible.
7. The safety controller accepts an **explicit operator reset** only after all prerequisites qualify.
8. Fresh independent commissioning verifies 1024/256/2 Hz, 25/50/75% PWM and safe load behavior.
9. Authoritative `LevelGamePlayManager` reaches 6/6. Never bypass the actual objective evaluator.

### No false claims
The supplied FBX contains the shunt geometry and contact planning data only. Functional topology, current sensing, PLC logic and gameplay must be implemented in the real Project Spark Unity project.
