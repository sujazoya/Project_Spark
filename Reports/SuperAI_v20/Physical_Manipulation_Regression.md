# Physical manipulation regression

The v19 Level 48 attempt registered wire 4 and later failed on wire 10, `DRIVER_MOTOR_MINUS → PRINT_MOTOR_MINUS`. A full-body sweep reported the source pin collider on layer Pin. The v20 path keeps all collision layers and the original fingertip tolerance. It adds a bounded replan from the last safe pose and logs the obstacle, stopped pose, and chosen alternate route when a recovery is attempted.

The Level 48 v20 fixture uses the authored 13-wire recipe with a temporary in-memory review. It separately asserts electrical registration for wires 4 and 10, then checks the full circuit and the six native objectives. The fixture does not write a player-approved plan.

The first v20 full run passed wire 4, then stopped before wire 5 could register. The ENCODER_A pickup search used a 0.359 m fixed-arm radius and found zero safe routes, with ENCODER_B the dominant Pin-layer obstacle. The follow-up gives original-hand wiring a bounded wider candidate search and verifies candidate shoulder-to-terminal clearance through the existing telescopic controller. A separate fresh-level wire-10 fixture captures the real native callback and does not need the preceding nine wires.

In the full rerun, wire 4 again registered. The new 1.75 m original-hand search found 4,582 blocked corridors and zero clear arrival routes for the wire-5 pickup at ENCODER_A. The root was grounded at approximately (-5.209, 0.743, -0.766). ENCODER_B was the dominant blocker. No collision layer or contact threshold was changed. The next physical check is to validate a safe alternate takeoff stance before entering this crowded ground approach.
