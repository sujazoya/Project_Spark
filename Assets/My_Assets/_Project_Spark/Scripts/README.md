# Project Spark — Gameplay Foundation (Restarted / Final Architecture)

This package is the clean production baseline for the Project Spark gameplay
interaction layer.

## Architecture decision

The foundation deliberately uses only these runtime responsibilities:

- SparkGameplayController
  - world target acquisition
  - UI priority
  - input ownership
  - interaction-session coordination
- SparkToolController
  - active tool ownership
  - tool lookup/activation
- SparkTool
  - tool lifecycle
  - user intent
- SparkInteractionSession
  - one continuous interaction's lifetime
- SparkElectronicObject
  - object identity
  - common operational state
  - hover/selection state
  - generic interaction validation
- SparkTerminal
  - connection endpoint compatibility and connection-capacity validation
- narrow capability interfaces
  - movable
  - rotatable
  - connectable
  - Wire/Measurement/Scan integration

The existing Wire, Measurement and Scan systems remain authoritative.

## Important

The system intentionally does NOT include:

- an event bus
- service locator
- DI container
- generic repository
- command bus
- entity framework
- massive registry
- global gameplay singleton
- duplicate wire topology
- duplicate measurement solver
- duplicate scan implementation

## Folder structure

ProjectSpark/
  Gameplay/
    SparkInteractionTypes.cs
    SparkInteractionContracts.cs
    SparkInteractionSession.cs
    SparkElectronicObject.cs
    SparkTool.cs
    SparkToolController.cs
    SparkGameplayController.cs
  Objects/
    SparkComponent.cs
    SparkInstrument.cs
    SparkTerminal.cs
    SparkSwitch.cs
    SparkPowerSupply.cs
  Tools/
    SparkObjectTools.cs
    SparkSystemTools.cs

## Setup

Create:

Workshop
  Gameplay
    SparkGameplayController

On the same object add:
- SparkToolController
- SparkGameplayController

Create child tool components:
- SparkSelectTool
- SparkInspectTool
- SparkMoveTool
- SparkRotateTool
- SparkWireTool
- SparkMeasureTool
- SparkProbeTool
- SparkScanTool

Put all tool components into SparkToolController.Tools.

Assign:
- interaction camera
- EventSystem
- primary action
- cancel action
- interaction layer mask
- Wire integration component
- Measurement integration component
- Scan integration component

Existing systems should implement:
- ISparkWireIntegration
- ISparkMeasurementIntegration
- ISparkScanIntegration

They can implement these interfaces directly on existing manager/system components
or on thin adapter MonoBehaviours that call their existing authoritative APIs.

## Input

Primary:
- started = begin
- canceled = end

Cancel:
- performed = cancel

The gameplay controller uses Update only while a continuous session is active and
for target acquisition. It does not poll every object.

## Engineering state

SparkSwitch:
Open/Closed
IsConducting reflects electrical path state.

SparkPowerSupply:
Off/Standby/Active/Fault
IsOutputActive reflects actual output availability.

Visual FX/UI must consume these states and never become the source of truth.

## Important extension rule

A new engineering object should generally:
1. derive from SparkComponent or SparkInstrument when appropriate
2. implement only the capabilities it genuinely supports
3. own its domain-specific operational state
4. delegate electrical/topology/measurement/scan truth to the authoritative subsystem

The gameplay controller should not be modified to add a new object type.

# Engineering Object Foundation — Milestone 02

Added production electrical-object foundations:
- SparkElectricalComponent
- SparkResistor
- SparkLED
- SparkElectricalFault
- upgraded SparkSwitch
- upgraded SparkPowerSupply
- solver-state receiver contract

The gameplay layer does not solve circuits. It exposes component domain data and
receives authoritative solver results.

