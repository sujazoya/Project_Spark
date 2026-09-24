# Project Spark Advanced Display System

Production-oriented Unity 6.x display foundation for Project Spark.

## Included

- Data-driven `SparkDisplayData`
- `SparkAdvancedDisplay` controller
- ScriptableObject display profiles
- ScriptableObject animation profiles
- Engineering-unit formatting
- Smooth/instrument/instant value presentation
- Animated text modes
- Status states
- Battery/progress bar
- Min/Max/Average presentation
- Bounded measurement history
- Source abstraction for device integration
- Demo source for immediate scene testing
- Unity Editor menu for creating default profiles

## Installation

Copy the `Assets` folder into the root of the Project Spark Unity project.

No third-party package is required.

## First setup

1. Open Unity.
2. Let scripts compile.
3. Use:
   Project Spark > Display > Create Default Profiles
4. Create a world-space Canvas or a RenderTexture-backed display surface.
5. Add `SparkAdvancedDisplay`.
6. Assign the generated Display Profile and Animation Profile.
7. Add TMP text objects for the primary value, mode, unit, status, and optional fields.
8. Add the matching renderer components and wire their references.
9. For an immediate test, add `SparkDisplayDemoSource` and assign the `SparkAdvancedDisplay`.

## Integration rule

Electrical simulation remains authoritative.

Device/sensor systems produce `SparkDisplayData`.

The display system only formats, animates and renders that data.

Do not move voltage/current/fault calculations into display classes.

## Existing Project Spark systems

This package does not recreate or replace the existing Wire System, Measurement System, Scan System, or electrical solvers.
