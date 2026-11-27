# Project Spark Phase 2 cable physics

## Inspected path

The physical lead path runs from `SparkNativeWireCinematic` and `SparkCableSimulation` through `SparkAINativeSignalWirePort` into `SignalWireBuilder`, `SignalPath` and `SignalPathMesh`. Insulation comes from the existing native tube mesh. The committed wire continues through the builder and native registry.

## Changes

- The fixed-step solve still handles gravity sag, damping, continuity and knot sweeps. It limits transverse variation between adjacent knots to bound individual segment lengths.
- The AI adapter validates finite geometry, source continuity, forward progression, total rest-length budget and per-segment length before publishing a physical shape. The authored visible span uses a 1.55 route ratio cap. The selected lead also keeps its existing extension limit.
- PC defaults remain 24 segments and seven iterations. Android caps them at 16 and four. Both pass real endpoint completion to the same authoritative Wire Tool.
- Diagnostics sample peak segment ratio, path length, extension ratio, endpoint drift, knot motion and collision sweeps. Collision sweeps measure solver interactions. A complete collider-clearance proof for the rendered tube remains open.

## Evidence

The synthetic geometry test measured and rejected a 10.540-unit shape over a 1.160-unit terminal span. The historical game trace was unavailable for literal reproduction. A normal test span with sag measured 1.185 units and a 1.021 peak segment ratio. The same geometry test also rejected an undersized lead, a stale source and a backward segment. The first authored Level 20 cable measured 1.312 over 1.160 before Phase 2, 1.249 after the cable change and 1.226 after the hand update. Each of those replays used 25 path points.

The final Phase 2 capture measured 1.213 units for that 1.160-unit span with 25 path points. The first attempt at the complete fixture encountered a body collision before wire-nine pickup. The original hand now uses a reachable source from the grounded stance when clearance permits, with a bounded alternate pickup path when things change. The final replay registered wires nine and ten.

See the Phase 2 test report for commands, evidence and rollback.
