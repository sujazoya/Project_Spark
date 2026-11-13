# Level 46 — Keep in Step

The master tracks the 30 or 60 RPM preset. The follower tracks measured master speed and compensates for its own brake load. Both encoders decode 80 counts per turn using 0.5-second measurement windows. The web display reports a signed load estimate in newtons. Positive means tension and negative means slack.

## Install

1. Merge the ZIP's Assets folder and metadata into Project Spark. The fitted assembly was made from the twelve individual FBXs.
2. Open MainMenu in Edit Mode and run **Tools → Project Spark → Level 46 → Install and Register**. The tool creates or reuses the definition, fits the layout to the board, and registers the campaign, teaching drawing and layout catalog.
3. Use **Validate Level Assets** in the same menu. Expect 24 distinct terminals, 12 wire declarations and six objective bindings. If the scene already contained unsaved changes, save MainMenu to persist registration.
4. Complete Level 45 to unlock Level 46. The existing SparkMotor definition from Level 30 must be in the active solver registry.

| Prefab location | Inspector references |
| --- | --- |
| Root | Spark Level Layout, scene bindings, Spark Level 46 Sync and the supplied visual rig |
| Sync Workbench | Two motor channels with separate switches, equivalent impedances, PWM sources and encoder inputs |
| Sync Workbench → Motion pivots | Motor shafts, rollers, encoder discs, brake rotor, brake arm and pad. The rig binds the original moving groups. Web grip seams follow average roller travel. |
| Sync Workbench → Breadboard Labels | Fixed terminal captions and the three live TextMeshPro readouts |
| Level 46 Controls | Target, master/follower speed, difference, PWM, web load, brake state, safety status and evaluator progress |

## Play

1. Wire W01–W12 from the exact diagram. Add **W11 and W12 last**. Power for both driver channels and encoders is prewired.
2. Press **Jog Forward**. Both shafts must turn forward and both signed encoder counts must rise.
3. Press **30 RPM** and **Sync**. Once it settles, press **60 RPM**. Each speed must hold within ±3 RPM, with the two channels no more than 2 RPM apart, for two seconds.
4. At steady 60 RPM, press **Test Load**. Watch follower speed fall, its duty increase and the web settle as speed recovers.
5. Stop Sync. Use **Undo Last** twice and cross the follower leads. Connect follower A to controller follower B, and follower B to controller follower A. Press **Test Direction**. Both drives enter **Safe Stop** after three reverse decoded transitions during forward shaft motion.
6. Undo the crossed leads and restore W11/W12. Wait for the shafts to coast down, press **Reset Fault**, then **Sync** at 60 RPM to reach 6/6.

## Diagnostics

- At **0/6**, follow the missing connection shown in the status line. Verify the 5 V reference and the terminal IDs from the wiring CSV.
- At a speed target, check that Sync is active and that both speeds, speed difference and web load stay inside the band continuously.
- At **5/6**, a missing lead alone cannot pass the exercise. Cross both leads and run the direction test, restore both, then explicitly reset and resynchronize.
- A stall or overload opens both drives. Release the brake and reset once stopped. The Console reports `[LEVEL 46] SAFE STOP` with the reason.
- The electrical model uses solved DC-equivalent PWM and current. Quadrature edges come from the simulated shaft and follow the actual player-wired nets. The decoder processes these digital transitions locally, avoiding a full DC solve for every encoder edge. Shaft inertia and brake torque are level-local. The web uses a reduced compliance and damping model driven by the difference in roller surface travel.
