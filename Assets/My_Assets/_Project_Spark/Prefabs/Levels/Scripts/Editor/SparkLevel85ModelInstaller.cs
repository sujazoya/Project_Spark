#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using ProjectSpark.Visuals;
using ProjectSpark.UI.VisualTeaching;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel85ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_85_Layout/";
        public const string Prefab = Folder + "Level_85_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "OrbitalDockingArenaBase","PanoramicEarthAndStarfieldViewport","StationOrbitalTrussSpine","StationPrimaryDockingPort","DockingOuterGuideRing","DockingPassiveCaptureCone","StationOpticalTargetMarkerArray","StationSolarWingModule","StationThermalRadiatorWing","StationApproachBeaconCluster","StationServiceAirlockModule","StationDockingPowerInterface","StationCaptureStructuralFrame","ThreeDimensionalApproachCorridorHologram","RelativeOrbitTrajectoryHologram","EarthOrbitalReferenceHologram","ExternalInspectionCameraGantry","StationApproachIlluminationRig","AutonomousServiceSpacecraftBody","ServiceSpacecraftExtendableDockingProbe","ProtectedServiceVehicleAvionics","ServiceSpacecraftPortSolarWing","ServiceSpacecraftStarboardSolarWing","ReactionControlPropellantManifold","RCSPlusXThruster","RCSMinusXThruster","RCSPlusYThruster","RCSMinusYThruster","RCSPlusZThruster","RCSMinusZThruster","PropellantTankA","PropellantTankB","ThrusterValveDriverRack","ThrusterFlowAndPressureMonitor","PrecisionThrusterCalibrationFixture","SpacecraftInertialMeasurementUnit","StereoNavigationCameraLeft","StereoNavigationCameraRight","PrimaryScanningLiDAR","SecondaryScanningLiDAR","ShortRangeRendezvousRadar","DockingBeaconReceiver","OpticalTargetPatternTracker","RelativePositionVelocityEstimator","RelativeNavigationFusionComputer","NavigationUncertaintyVolumeDisplay","PrecisionFlightTimeReferenceClock","RelativeTrajectoryPlanningComputer","ProximityOperationsGuidanceComputer","SixDegreeFreedomFlightController","ThrusterForceAllocationComputer","DockingCorridorSafetyGuardian","CollisionAvoidancePredictionMonitor","FuelReserveAndAbortPredictor","ProtectedAutonomousAbortController","ActiveSoftCaptureRing","MotorizedCaptureGuidePetals","SixActuatorDockingLatchArray","SixAxisContactForceSensor","CompliantContactDampingActuator","DockingElectricalUmbilicalCoupler","PressureInterfaceSealGasket","PressureLeakQualificationController","HardDockStructuralLoadMonitor","DockingMechanismSequencingPLC","PrecisionDockingCameraMonitor","LiDARBlindSpotFaultEmulator","StuckThrusterValveFaultEmulator","AsymmetricLatchFaultEmulator","CriticalClosingRateFeedbackStation","DockingSafetyPLC","Protected5VControlSupply","DockingPowerDistributionCabinet","HolographicRendezvousSCADA","ElectricalTerminalDiagnosticRack","EmergencyApproachHoldStation","DockingMissionCertificationBeacon","PrecisionFlightEventDataRecorder"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .0075f;
        private const float SocketDiameter = .0086f;

        private static readonly (string id, string label, string anchor, string model, float x, float y, float z, string color)[] Sockets =
        {
            ("TRUSTED_RANGE_RATE_IN","W123 IN","P046","L85_DockingSafetyPLC",-9.73750f,3.39000f,4.98000f,"amber"),
            ("TRUSTED_RANGE_RATE_OUT","W123 OUT","P148","L85_RelativeNavigationFusionComputer",-6.17250f,1.51000f,4.98000f,"amber"),
            ("W080_NAVIGATION_ESTIMATION_CONFIDENCE_FOR_APPROACH_OUT","W080 OUT","P148","L85_RelativeNavigationFusionComputer",-6.39250f,1.51000f,4.98000f,"blue"),
            ("W098_MEASURED_ACTIVE_RING_EXTENSION_STATUS_OUT","W098 OUT","P001","L85_ActiveSoftCaptureRing",2.38750f,4.84000f,-1.20000f,"blue"),
            ("W097_ACTIVE_SOFT_CAPTURE_RING_ACTUATION_REQUEST_IN","W097 IN","P002","L85_ActiveSoftCaptureRing",2.49250f,4.84000f,-1.20000f,"mint"),
            ("W138_ASYMMETRIC_LATCH_ABNORMALITY_DIAGNOSIS_OUT","W138 OUT","P003","L85_AsymmetricLatchFaultEmulator",8.08750f,3.51000f,4.98000f,"mint"),
            ("W113_ASYMMETRIC_MECHANICAL_LATCH_FAULT_INJECTION_OUT","W113 OUT","P004","L85_AsymmetricLatchFaultEmulator",8.19250f,3.51000f,4.98000f,"mint"),
            ("W093_COLLISION_AVOIDANCE_PREDICTION_TIMELINE_OUT","W093 OUT","P005","L85_CollisionAvoidancePredictionMonitor",3.26750f,1.51000f,4.98000f,"mint"),
            ("W068_PREDICTED_COLLISION_FREE_MANEUVER_CERTIFICATE_OUT","W068 OUT","P006","L85_CollisionAvoidancePredictionMonitor",3.37250f,1.51000f,4.98000f,"mint"),
            ("W120_INDEPENDENT_COLLISION_PREDICTION_STOP_REQUEST_OUT","W120 OUT","P006","L85_CollisionAvoidancePredictionMonitor",3.37250f,1.29000f,4.98000f,"mint"),
            ("W082_PREDICTIVE_KEEP_OUT_ZONE_INTERSECTION_STATUS_OUT","W082 OUT","P007","L85_CollisionAvoidancePredictionMonitor",3.47750f,1.51000f,4.98000f,"mint"),
            ("W105_ACTUAL_COMPLIANT_DAMPER_DISPLACEMENT_OUT","W105 OUT","P008","L85_CompliantContactDampingActuator",2.33750f,4.84000f,-1.20000f,"blue"),
            ("W104_CONTACT_DAMPING_REQUEST_UNDER_SAFE_LOADS_IN","W104 IN","P009","L85_CompliantContactDampingActuator",2.44250f,4.84000f,-1.20000f,"mint"),
            ("W121_CLOSING_RATE_MONITOR_CALIBRATION_HEALTH_DIAGN_OUT","W121 OUT","P010","L85_CriticalClosingRateFeedbackStation",-11.60250f,3.39000f,4.98000f,"mint"),
            ("W122_NAVIGATION_FUSION_RAW_RANGE_RATE_CALIBRATION__IN","W122 IN","P011","L85_CriticalClosingRateFeedbackStation",-11.49750f,3.39000f,4.98000f,"blue"),
            ("W127_CRITICAL_W123_FEEDBACK_STATE_AND_FAULT_LATCH_IN","W127 IN","P012","L85_CriticalClosingRateFeedbackStation",-11.39250f,3.39000f,4.98000f,"mint"),
            ("W031_DOCK_BEACON_ASSOCIATION_TO_OPTICAL_MARKERS_OUT","W031 OUT","P013","L85_DockingBeaconReceiver",-2.56250f,5.99000f,-1.05000f,"mint"),
            ("W018_DOCK_TARGET_BEACON_IDENTITY_VALIDATION_OUT","W018 OUT","P014","L85_DockingBeaconReceiver",-2.45750f,5.99000f,-1.05000f,"mint"),
            ("W087_DOCK_BEACON_CODED_ACQUISITION_SIGNAL_IN","W087 IN","P015","L85_DockingBeaconReceiver",-2.35250f,5.99000f,-1.05000f,"mint"),
            ("W091_ACTUAL_PROXIMITY_WARNING_BOUNDARY_VISUAL_OUT","W091 OUT","P016","L85_DockingCorridorSafetyGuardian",1.91750f,1.51000f,4.98000f,"mint"),
            ("W092_APPROACH_HOLD_POINT_READINESS_REPORT_OUT","W092 OUT","P017","L85_DockingCorridorSafetyGuardian",2.02250f,1.51000f,4.98000f,"mint"),
            ("W067_APPROACH_HOLD_POINT_PERMISSION_AND_MODE_OUT","W067 OUT","P018","L85_DockingCorridorSafetyGuardian",2.12750f,1.51000f,4.98000f,"mint"),
            ("W094_EMERGENCY_HOLD_RETREAT_ARBITRATION_REQUEST_OUT","W094 OUT","P018","L85_DockingCorridorSafetyGuardian",2.12750f,1.29000f,4.98000f,"mint"),
            ("W083_COLLISION_BOUNDARY_CLEARANCE_AUTHORIZATION_OUT","W083 OUT","P019","L85_DockingCorridorSafetyGuardian",2.23250f,1.51000f,4.98000f,"mint"),
            ("W096_FINAL_DOCKING_CONTACT_AUTHORIZATION_OUT","W096 OUT","P019","L85_DockingCorridorSafetyGuardian",2.45250f,1.29000f,4.98000f,"mint"),
            ("W079_SHORT_RANGE_APPROACHING_OBJECT_RADIAL_DISTANC_IN","W079 IN","P020","L85_DockingCorridorSafetyGuardian",2.33750f,1.51000f,4.98000f,"blue"),
            ("W081_RELATIVE_TARGET_APPROACH_SPEED_ESTIMATE_IN","W081 IN","P020","L85_DockingCorridorSafetyGuardian",2.33750f,1.73000f,4.98000f,"blue"),
            ("W080_NAVIGATION_ESTIMATION_CONFIDENCE_FOR_APPROACH_IN","W080 IN","P021","L85_DockingCorridorSafetyGuardian",2.44250f,1.51000f,4.98000f,"blue"),
            ("W084_LIVE_FINAL_DOCKING_CAMERA_STATUS_IN","W084 IN","P022","L85_DockingCorridorSafetyGuardian",1.91750f,1.61500f,4.98000f,"mint"),
            ("W095_MEASURED_RETREAT_MANEUVER_EXECUTED_STATUS_IN","W095 IN","P023","L85_DockingCorridorSafetyGuardian",2.02250f,1.61500f,4.98000f,"mint"),
            ("W082_PREDICTIVE_KEEP_OUT_ZONE_INTERSECTION_STATUS_IN","W082 IN","P024","L85_DockingCorridorSafetyGuardian",2.12750f,1.61500f,4.98000f,"mint"),
            ("W107_ACTUAL_UMBILICAL_CONTINUITY_VERIFICATION_OUT","W107 OUT","P025","L85_DockingElectricalUmbilicalCoupler",2.11750f,4.84000f,-1.20000f,"blue"),
            ("W106_QUALIFIED_DOCKING_UMBILICAL_MATE_COMMAND_IN","W106 IN","P026","L85_DockingElectricalUmbilicalCoupler",2.44250f,4.62000f,-1.20000f,"mint"),
            ("W097_ACTIVE_SOFT_CAPTURE_RING_ACTUATION_REQUEST_OUT","W097 OUT","P027","L85_DockingMechanismSequencingPLC",8.73750f,1.51000f,4.98000f,"mint"),
            ("W101_SIX_INDEPENDENT_HARD_LATCH_ACTUATION_PERMITS_OUT","W101 OUT","P027","L85_DockingMechanismSequencingPLC",8.51750f,1.51000f,4.98000f,"mint"),
            ("W106_QUALIFIED_DOCKING_UMBILICAL_MATE_COMMAND_OUT","W106 OUT","P027","L85_DockingMechanismSequencingPLC",8.73750f,1.29000f,4.98000f,"mint"),
            ("W099_GUIDE_PETAL_POSITIONING_REQUEST_OUT","W099 OUT","P028","L85_DockingMechanismSequencingPLC",8.84250f,1.51000f,4.98000f,"mint"),
            ("W104_CONTACT_DAMPING_REQUEST_UNDER_SAFE_LOADS_OUT","W104 OUT","P028","L85_DockingMechanismSequencingPLC",9.06250f,1.29000f,4.98000f,"mint"),
            ("W108_PRESSURE_GASKET_COMPRESSION_AUTHORIZATION_OUT","W108 OUT","P028","L85_DockingMechanismSequencingPLC",8.84250f,1.07000f,4.98000f,"mint"),
            ("W142_DOCKING_STATE_TRANSITIONS_FLIGHT_RECORDER_OUT","W142 OUT","P029","L85_DockingMechanismSequencingPLC",8.94750f,1.51000f,4.98000f,"mint"),
            ("W112_DOCKING_MECHANISM_STEP_BY_STEP_CAMERA_STATUS_OUT","W112 OUT","P030","L85_DockingMechanismSequencingPLC",9.05250f,1.51000f,4.98000f,"mint"),
            ("W114_SOFT_CAPTURE_TO_HARD_DOCKING_STATE_EVENTS_OUT","W114 OUT","P031","L85_DockingMechanismSequencingPLC",9.15750f,1.51000f,4.98000f,"mint"),
            ("W098_MEASURED_ACTIVE_RING_EXTENSION_STATUS_IN","W098 IN","P032","L85_DockingMechanismSequencingPLC",9.26250f,1.51000f,4.98000f,"blue"),
            ("W102_SIX_HARD_LATCH_ACTUAL_LOCK_FEEDBACK_CHANNELS_IN","W102 IN","P032","L85_DockingMechanismSequencingPLC",9.48250f,1.51000f,4.98000f,"blue"),
            ("W105_ACTUAL_COMPLIANT_DAMPER_DISPLACEMENT_IN","W105 IN","P032","L85_DockingMechanismSequencingPLC",9.26250f,1.29000f,4.98000f,"blue"),
            ("W111_DOCKED_STRUCTURE_LOAD_DISTRIBUTION_MONITOR_IN","W111 IN","P032","L85_DockingMechanismSequencingPLC",9.48250f,1.29000f,4.98000f,"blue"),
            ("W100_MEASURED_GUIDE_PETAL_ALIGNMENT_FEEDBACK_IN","W100 IN","P033","L85_DockingMechanismSequencingPLC",8.73750f,1.61500f,4.98000f,"blue"),
            ("W103_SIX_AXIS_CAPTURED_CONTACT_FORCE_FEEDBACK_IN","W103 IN","P033","L85_DockingMechanismSequencingPLC",8.73750f,1.83500f,4.98000f,"blue"),
            ("W107_ACTUAL_UMBILICAL_CONTINUITY_VERIFICATION_IN","W107 IN","P033","L85_DockingMechanismSequencingPLC",8.51750f,1.83500f,4.98000f,"blue"),
            ("W110_COMPLETED_FICTIONAL_PRESSURE_LEAK_TEST_RESULT_IN","W110 IN","P034","L85_DockingMechanismSequencingPLC",8.84250f,1.61500f,4.98000f,"mint"),
            ("W002_CAPTURE_SEQUENCING_PLC_5V_LOGIC_IN","W002 IN","P035","L85_DockingMechanismSequencingPLC",8.94750f,1.61500f,4.98000f,"red"),
            ("W096_FINAL_DOCKING_CONTACT_AUTHORIZATION_IN","W096 IN","P036","L85_DockingMechanismSequencingPLC",9.05250f,1.61500f,4.98000f,"mint"),
            ("W118_INDEPENDENT_HARD_CAPTURE_INHIBIT_OUTPUT_IN","W118 IN","P036","L85_DockingMechanismSequencingPLC",9.05250f,1.83500f,4.98000f,"mint"),
            ("W113_ASYMMETRIC_MECHANICAL_LATCH_FAULT_INJECTION_IN","W113 IN","P037","L85_DockingMechanismSequencingPLC",9.15750f,1.61500f,4.98000f,"mint"),
            ("W146_DOCKING_MISSION_6_OF_6_OBJECTIVE_CERTIFICATE_OUT","W146 OUT","P038","L85_DockingMissionCertificationBeacon",-0.26250f,8.99000f,-5.37000f,"mint"),
            ("W145_SIX_OBJECTIVE_AUTHORIZED_CERTIFICATION_DISPLA_IN","W145 IN","P039","L85_DockingMissionCertificationBeacon",-0.15750f,8.99000f,-5.37000f,"mint"),
            ("W130_PROTECTED_DOCKING_MISSION_RELEASE_AUTHORIZATI_IN","W130 IN","P040","L85_DockingMissionCertificationBeacon",-0.05250f,8.99000f,-5.37000f,"mint"),
            ("W143_LATCHED_SAFETY_ALARM_FLIGHT_RECORDER_OUT","W143 OUT","P041","L85_DockingSafetyPLC",-10.26250f,3.39000f,4.98000f,"mint"),
            ("W129_CRITICAL_SAFETY_STATUS_AND_EVENT_TELEMETRY_OUT","W129 OUT","P042","L85_DockingSafetyPLC",-10.15750f,3.39000f,4.98000f,"mint"),
            ("W117_INDEPENDENT_THRUSTER_INHIBIT_AND_AUTHORIZATIO_OUT","W117 OUT","P043","L85_DockingSafetyPLC",-10.05250f,3.39000f,4.98000f,"mint"),
            ("W125_SAFETY_AUTHORIZED_ESCAPE_RETREAT_ENABLE_OUT","W125 OUT","P043","L85_DockingSafetyPLC",-10.05250f,3.17000f,4.98000f,"mint"),
            ("W130_PROTECTED_DOCKING_MISSION_RELEASE_AUTHORIZATI_OUT","W130 OUT","P043","L85_DockingSafetyPLC",-10.27250f,3.17000f,4.98000f,"mint"),
            ("W118_INDEPENDENT_HARD_CAPTURE_INHIBIT_OUTPUT_OUT","W118 OUT","P044","L85_DockingSafetyPLC",-9.94750f,3.39000f,4.98000f,"mint"),
            ("W127_CRITICAL_W123_FEEDBACK_STATE_AND_FAULT_LATCH_OUT","W127 OUT","P044","L85_DockingSafetyPLC",-9.72750f,3.17000f,4.98000f,"mint"),
            ("W119_MINIMUM_PHYSICAL_FUEL_RESERVE_FOR_SAFE_ABORT_IN","W119 IN","P045","L85_DockingSafetyPLC",-9.84250f,3.39000f,4.98000f,"blue"),
            ("W128_OPERATOR_EXPLICIT_SAFETY_RESET_REQUEST_IN","W128 IN","P047","L85_DockingSafetyPLC",-10.26250f,3.49500f,4.98000f,"mint"),
            ("W121_CLOSING_RATE_MONITOR_CALIBRATION_HEALTH_DIAGN_IN","W121 IN","P048","L85_DockingSafetyPLC",-10.15750f,3.49500f,4.98000f,"mint"),
            ("W126_MEASURED_SAFE_ABORT_COMPLETION_FEEDBACK_IN","W126 IN","P049","L85_DockingSafetyPLC",-10.05250f,3.49500f,4.98000f,"mint"),
            ("W001_DOCKING_SAFETY_PLC_CONTROL_POWER_IN","W001 IN","P050","L85_DockingSafetyPLC",-9.94750f,3.49500f,4.98000f,"red"),
            ("W115_INDEPENDENT_DOCKING_SAFETY_PLC_PROTECTED_CONT_IN","W115 IN","P051","L85_DockingSafetyPLC",-9.84250f,3.49500f,4.98000f,"red"),
            ("W116_MANUAL_EMERGENCY_APPROACH_HOLD_PHYSICAL_CONTA_IN","W116 IN","P052","L85_DockingSafetyPLC",-9.73750f,3.49500f,4.98000f,"mint"),
            ("W120_INDEPENDENT_COLLISION_PREDICTION_STOP_REQUEST_IN","W120 IN","P053","L85_DockingSafetyPLC",-10.26250f,3.60000f,4.98000f,"mint"),
            ("W128_OPERATOR_EXPLICIT_SAFETY_RESET_REQUEST_OUT","W128 OUT","P054","L85_ElectricalTerminalDiagnosticRack",-6.23250f,3.39000f,4.98000f,"mint"),
            ("W136_LIDAR_DROPOUT_EVENT_DIAGNOSTIC_FINGERPRINT_IN","W136 IN","P055","L85_ElectricalTerminalDiagnosticRack",-6.12750f,3.39000f,4.98000f,"mint"),
            ("W138_ASYMMETRIC_LATCH_ABNORMALITY_DIAGNOSIS_IN","W138 IN","P055","L85_ElectricalTerminalDiagnosticRack",-6.12750f,3.17000f,4.98000f,"mint"),
            ("W137_STUCK_VALVE_FAULT_AND_ISOLATION_REPORT_IN","W137 IN","P056","L85_ElectricalTerminalDiagnosticRack",-6.02250f,3.39000f,4.98000f,"mint"),
            ("W010_TERMINAL_DIAGNOSTIC_MONITOR_5V_IN","W010 IN","P057","L85_ElectricalTerminalDiagnosticRack",-5.91750f,3.39000f,4.98000f,"red"),
            ("W116_MANUAL_EMERGENCY_APPROACH_HOLD_PHYSICAL_CONTA_OUT","W116 OUT","P058","L85_EmergencyApproachHoldStation",-4.88250f,3.39000f,4.98000f,"mint"),
            ("W088_INSPECTION_GANTRY_MONITORING_POSE_OUT","W088 OUT","P059","L85_ExternalInspectionCameraGantry",5.53750f,6.89000f,-0.30000f,"mint"),
            ("W065_PROPELLANT_AND_ABORT_MARGIN_CONSTRAINT_OUT","W065 OUT","P060","L85_FuelReserveAndAbortPredictor",4.61750f,1.51000f,4.98000f,"blue"),
            ("W119_MINIMUM_PHYSICAL_FUEL_RESERVE_FOR_SAFE_ABORT_OUT","W119 OUT","P060","L85_FuelReserveAndAbortPredictor",4.39750f,1.51000f,4.98000f,"blue"),
            ("W076_FUEL_RESERVE_AND_RETREAT_ENVELOPE_OUT","W076 OUT","P061","L85_FuelReserveAndAbortPredictor",4.72250f,1.51000f,4.98000f,"blue"),
            ("W056_ABORT_FUEL_RESERVE_THRUST_ALLOCATION_LIMIT_OUT","W056 OUT","P062","L85_FuelReserveAndAbortPredictor",4.82750f,1.51000f,4.98000f,"mint"),
            ("W057_PREDICTED_COMMANDED_THRUSTER_FUEL_USAGE_IN","W057 IN","P063","L85_FuelReserveAndAbortPredictor",4.93250f,1.51000f,4.98000f,"blue"),
            ("W066_FORECASTED_RENDEZVOUS_IMPULSE_BUDGET_IN","W066 IN","P064","L85_FuelReserveAndAbortPredictor",5.03750f,1.51000f,4.98000f,"mint"),
            ("W111_DOCKED_STRUCTURE_LOAD_DISTRIBUTION_MONITOR_OUT","W111 OUT","P065","L85_HardDockStructuralLoadMonitor",2.48750f,4.84000f,-1.20000f,"blue"),
            ("W140_CAPTURED_STRUCTURAL_LOAD_SAFETY_REPORT_OUT","W140 OUT","P066","L85_HardDockStructuralLoadMonitor",2.59250f,4.84000f,-1.20000f,"blue"),
            ("W145_SIX_OBJECTIVE_AUTHORIZED_CERTIFICATION_DISPLA_OUT","W145 OUT","P067","L85_HolographicRendezvousSCADA",-0.26250f,8.29000f,-5.34000f,"mint"),
            ("W076_FUEL_RESERVE_AND_RETREAT_ENVELOPE_IN","W076 IN","P068","L85_HolographicRendezvousSCADA",-0.15750f,8.29000f,-5.34000f,"blue"),
            ("W132_MEASURED_TARGET_CLOSING_RATE_DISPLAY_SUMMARY_IN","W132 IN","P068","L85_HolographicRendezvousSCADA",-0.15750f,8.07000f,-5.34000f,"blue"),
            ("W140_CAPTURED_STRUCTURAL_LOAD_SAFETY_REPORT_IN","W140 IN","P068","L85_HolographicRendezvousSCADA",-0.37750f,8.07000f,-5.34000f,"blue"),
            ("W124_DISPLAY_ONLY_RELATIVE_SPEED_TELEMETRY_NOT_W12_IN","W124 IN","P069","L85_HolographicRendezvousSCADA",-0.05250f,8.29000f,-5.34000f,"blue"),
            ("W135_PROPELLANT_FLOW_AND_PULSE_HISTORY_TREND_IN","W135 IN","P069","L85_HolographicRendezvousSCADA",0.16750f,8.07000f,-5.34000f,"blue"),
            ("W032_RELATIVE_RANGE_AND_VELOCITY_TELEMETRY_DISPLAY_IN","W032 IN","P070","L85_HolographicRendezvousSCADA",0.05250f,8.29000f,-5.34000f,"mint"),
            ("W075_TRAJECTORY_DELTA_V_FORECAST_TELEMETRY_IN","W075 IN","P070","L85_HolographicRendezvousSCADA",0.05250f,8.51000f,-5.34000f,"mint"),
            ("W131_FUSED_POSITION_AND_VELOCITY_STATE_SCADA_IN","W131 IN","P070","L85_HolographicRendezvousSCADA",0.27250f,8.51000f,-5.34000f,"mint"),
            ("W144_SIGNED_FRESH_MISSION_RUN_EVENT_LOG_IN","W144 IN","P070","L85_HolographicRendezvousSCADA",0.49250f,8.29000f,-5.34000f,"mint"),
            ("W033_COVARIANCE_VISUALIZATION_AND_QUALITY_TRACE_IN","W033 IN","P071","L85_HolographicRendezvousSCADA",0.15750f,8.29000f,-5.34000f,"mint"),
            ("W093_COLLISION_AVOIDANCE_PREDICTION_TIMELINE_IN","W093 IN","P071","L85_HolographicRendezvousSCADA",0.37750f,8.07000f,-5.34000f,"mint"),
            ("W133_SIX_DEGREE_TRANSLATION_ACTUAL_CONTROL_TELEMET_IN","W133 IN","P071","L85_HolographicRendezvousSCADA",0.15750f,8.73000f,-5.34000f,"mint"),
            ("W034_RAW_SPACECRAFT_IMU_HEALTH_TELEMETRY_IN","W034 IN","P072","L85_HolographicRendezvousSCADA",0.26250f,8.29000f,-5.34000f,"mint"),
            ("W077_ABORT_CONTROLLER_ACTUAL_MODE_STATUS_IN","W077 IN","P072","L85_HolographicRendezvousSCADA",0.48250f,8.51000f,-5.34000f,"mint"),
            ("W114_SOFT_CAPTURE_TO_HARD_DOCKING_STATE_EVENTS_IN","W114 IN","P072","L85_HolographicRendezvousSCADA",0.70250f,8.29000f,-5.34000f,"mint"),
            ("W134_THRUSTER_VALVE_COMMANDED_VERSUS_ACTUAL_STATUS_IN","W134 IN","P072","L85_HolographicRendezvousSCADA",0.26250f,7.85000f,-5.34000f,"mint"),
            ("W141_SYNCHRONIZED_APPROACH_CAMERA_FEED_HEALTH_IN","W141 IN","P072","L85_HolographicRendezvousSCADA",0.70250f,8.07000f,-5.34000f,"mint"),
            ("W074_GUIDANCE_CONTROLLER_STATE_AND_TRACKING_TREND_IN","W074 IN","P073","L85_HolographicRendezvousSCADA",-0.26250f,8.39500f,-5.34000f,"mint"),
            ("W092_APPROACH_HOLD_POINT_READINESS_REPORT_IN","W092 IN","P073","L85_HolographicRendezvousSCADA",-0.48250f,8.39500f,-5.34000f,"mint"),
            ("W129_CRITICAL_SAFETY_STATUS_AND_EVENT_TELEMETRY_IN","W129 IN","P073","L85_HolographicRendezvousSCADA",-0.26250f,8.61500f,-5.34000f,"mint"),
            ("W139_PRESSURE_SEAL_POST_DOCKING_INTEGRITY_REPORT_IN","W139 IN","P073","L85_HolographicRendezvousSCADA",-0.48250f,8.61500f,-5.34000f,"mint"),
            ("W146_DOCKING_MISSION_6_OF_6_OBJECTIVE_CERTIFICATE_IN","W146 IN","P073","L85_HolographicRendezvousSCADA",-0.70250f,8.39500f,-5.34000f,"mint"),
            ("W009_SCADA_CONTROL_AND_VISUALIZATION_5V_IN","W009 IN","P074","L85_HolographicRendezvousSCADA",-0.15750f,8.39500f,-5.34000f,"red"),
            ("W026_INJECTED_LIDAR_BLIND_SPOT_SIGNAL_QUALITY_OUT","W026 OUT","P075","L85_LiDARBlindSpotFaultEmulator",10.05750f,1.51000f,4.98000f,"mint"),
            ("W136_LIDAR_DROPOUT_EVENT_DIAGNOSTIC_FINGERPRINT_OUT","W136 OUT","P076","L85_LiDARBlindSpotFaultEmulator",10.16250f,1.51000f,4.98000f,"mint"),
            ("W024_PRIMARY_LIDAR_SCAN_VALIDITY_DIAGNOSTIC_IN","W024 IN","P077","L85_LiDARBlindSpotFaultEmulator",10.26750f,1.51000f,4.98000f,"mint"),
            ("W025_SECONDARY_LIDAR_SENSOR_REDUNDANCY_DIAGNOSTIC_IN","W025 IN","P078","L85_LiDARBlindSpotFaultEmulator",10.37250f,1.51000f,4.98000f,"mint"),
            ("W100_MEASURED_GUIDE_PETAL_ALIGNMENT_FEEDBACK_OUT","W100 OUT","P079","L85_MotorizedCaptureGuidePetals",2.33750f,5.06000f,-1.20000f,"blue"),
            ("W099_GUIDE_PETAL_POSITIONING_REQUEST_IN","W099 IN","P080","L85_MotorizedCaptureGuidePetals",2.22250f,4.62000f,-1.20000f,"mint"),
            ("W033_COVARIANCE_VISUALIZATION_AND_QUALITY_TRACE_OUT","W033 OUT","P081","L85_NavigationUncertaintyVolumeDisplay",-8.46250f,6.39000f,-5.45000f,"mint"),
            ("W023_NAVIGATION_COVARIANCE_AND_CONFIDENCE_ELLIPSOI_IN","W023 IN","P082","L85_NavigationUncertaintyVolumeDisplay",-8.35750f,6.39000f,-5.45000f,"mint"),
            ("W073_ESTIMATED_POSITIONAL_UNCERTAINTY_DISTRIBUTION_IN","W073 IN","P083","L85_NavigationUncertaintyVolumeDisplay",-8.25250f,6.39000f,-5.45000f,"mint"),
            ("W019_DOCKING_FIDUCIAL_PATTERN_CONFIDENCE_OUT","W019 OUT","P084","L85_OpticalTargetPatternTracker",-2.59250f,4.49000f,-1.05000f,"blue"),
            ("W014_STEREO_TRIANGULATED_OPTICAL_TARGET_POSE_OUT","W014 OUT","P085","L85_OpticalTargetPatternTracker",-2.48750f,4.49000f,-1.05000f,"mint"),
            ("W085_OPTICAL_DOCKING_TARGET_VIDEO_ALIGNMENT_OUT","W085 OUT","P086","L85_OpticalTargetPatternTracker",-2.38250f,4.49000f,-1.05000f,"mint"),
            ("W012_LEFT_NAVIGATION_CAMERA_IMAGE_SAMPLES_IN","W012 IN","P087","L85_OpticalTargetPatternTracker",-2.27750f,4.49000f,-1.05000f,"mint"),
            ("W031_DOCK_BEACON_ASSOCIATION_TO_OPTICAL_MARKERS_IN","W031 IN","P087","L85_OpticalTargetPatternTracker",-2.27750f,4.27000f,-1.05000f,"mint"),
            ("W013_RIGHT_NAVIGATION_CAMERA_IMAGE_SAMPLES_IN","W013 IN","P088","L85_OpticalTargetPatternTracker",-2.17250f,4.49000f,-1.05000f,"mint"),
            ("W086_STATION_TARGET_FIDUCIAL_COORDINATES_IN","W086 IN","P088","L85_OpticalTargetPatternTracker",-2.17250f,4.71000f,-1.05000f,"mint"),
            ("W030_TARGET_OPTICAL_ACQUISITION_COMMAND_IN","W030 IN","P089","L85_OpticalTargetPatternTracker",-2.06750f,4.49000f,-1.05000f,"mint"),
            ("W084_LIVE_FINAL_DOCKING_CAMERA_STATUS_OUT","W084 OUT","P090","L85_PrecisionDockingCameraMonitor",9.83750f,3.99000f,-5.12000f,"mint"),
            ("W141_SYNCHRONIZED_APPROACH_CAMERA_FEED_HEALTH_OUT","W141 OUT","P091","L85_PrecisionDockingCameraMonitor",9.94250f,3.99000f,-5.12000f,"mint"),
            ("W085_OPTICAL_DOCKING_TARGET_VIDEO_ALIGNMENT_IN","W085 IN","P092","L85_PrecisionDockingCameraMonitor",10.04750f,3.99000f,-5.12000f,"mint"),
            ("W088_INSPECTION_GANTRY_MONITORING_POSE_IN","W088 IN","P093","L85_PrecisionDockingCameraMonitor",10.15250f,3.99000f,-5.12000f,"mint"),
            ("W089_DOCKING_APPROACH_LIGHTING_STATUS_IN","W089 IN","P094","L85_PrecisionDockingCameraMonitor",10.25750f,3.99000f,-5.12000f,"mint"),
            ("W112_DOCKING_MECHANISM_STEP_BY_STEP_CAMERA_STATUS_IN","W112 IN","P095","L85_PrecisionDockingCameraMonitor",10.36250f,3.99000f,-5.12000f,"mint"),
            ("W008_DOCKING_CAMERA_MONITOR_5V_IN","W008 IN","P096","L85_PrecisionDockingCameraMonitor",9.83750f,4.09500f,-5.12000f,"red"),
            ("W144_SIGNED_FRESH_MISSION_RUN_EVENT_LOG_OUT","W144 OUT","P097","L85_PrecisionFlightEventDataRecorder",-3.53250f,3.39000f,4.98000f,"mint"),
            ("W142_DOCKING_STATE_TRANSITIONS_FLIGHT_RECORDER_IN","W142 IN","P098","L85_PrecisionFlightEventDataRecorder",-3.42750f,3.39000f,4.98000f,"mint"),
            ("W143_LATCHED_SAFETY_ALARM_FLIGHT_RECORDER_IN","W143 IN","P099","L85_PrecisionFlightEventDataRecorder",-3.32250f,3.39000f,4.98000f,"mint"),
            ("W022_NAVIGATION_TIMESTAMP_AND_CLOCK_SYNCHRONIZATIO_OUT","W022 OUT","P100","L85_PrecisionFlightTimeReferenceClock",-4.83250f,1.51000f,4.98000f,"mint"),
            ("W064_TRAJECTORY_PROPAGATION_TIME_REFERENCE_OUT","W064 OUT","P101","L85_PrecisionFlightTimeReferenceClock",-4.72750f,1.51000f,4.98000f,"mint"),
            ("W053_MEASURED_THRUSTER_CALIBRATION_THRUST_TABLE_OUT","W053 OUT","P102","L85_PrecisionThrusterCalibrationFixture",-8.89250f,1.51000f,4.98000f,"mint"),
            ("W109_MEASURED_GASKET_COMPRESSION_AND_SEAL_HEALTH_OUT","W109 OUT","P103","L85_PressureInterfaceSealGasket",2.60750f,5.06000f,-1.20000f,"blue"),
            ("W108_PRESSURE_GASKET_COMPRESSION_AUTHORIZATION_IN","W108 IN","P104","L85_PressureInterfaceSealGasket",2.71250f,4.62000f,-1.20000f,"mint"),
            ("W110_COMPLETED_FICTIONAL_PRESSURE_LEAK_TEST_RESULT_OUT","W110 OUT","P105","L85_PressureLeakQualificationController",7.35750f,1.51000f,4.98000f,"mint"),
            ("W139_PRESSURE_SEAL_POST_DOCKING_INTEGRITY_REPORT_OUT","W139 OUT","P106","L85_PressureLeakQualificationController",7.46250f,1.51000f,4.98000f,"mint"),
            ("W109_MEASURED_GASKET_COMPRESSION_AND_SEAL_HEALTH_IN","W109 IN","P107","L85_PressureLeakQualificationController",7.56750f,1.51000f,4.98000f,"blue"),
            ("W015_PRIMARY_LIDAR_MEASURED_RANGE_SCAN_OUT","W015 OUT","P108","L85_PrimaryScanningLiDAR",-3.16250f,5.36000f,-0.80000f,"blue"),
            ("W024_PRIMARY_LIDAR_SCAN_VALIDITY_DIAGNOSTIC_OUT","W024 OUT","P109","L85_PrimaryScanningLiDAR",-3.05750f,5.36000f,-0.80000f,"mint"),
            ("W027_PRIMARY_LIDAR_TIMED_SCAN_TRIGGER_IN","W027 IN","P110","L85_PrimaryScanningLiDAR",-2.95250f,5.36000f,-0.80000f,"mint"),
            ("W007_LIDAR_SIGNAL_CONDITIONING_5V_IN","W007 IN","P111","L85_PrimaryScanningLiDAR",-2.84750f,5.36000f,-0.80000f,"red"),
            ("W050_PROPELLANT_TANK_A_AVAILABLE_FEED_PRESSURE_OUT","W050 OUT","P112","L85_PropellantTankA",-5.94250f,4.84000f,-1.65000f,"blue"),
            ("W051_PROPELLANT_TANK_B_AVAILABLE_FEED_PRESSURE_OUT","W051 OUT","P113","L85_PropellantTankB",-5.36250f,4.84000f,-1.65000f,"blue"),
            ("W001_DOCKING_SAFETY_PLC_CONTROL_POWER_OUT","W001 OUT","P114","L85_Protected5VControlSupply",-8.92250f,3.39000f,4.98000f,"red"),
            ("W003_NAVIGATION_FUSION_COMPUTER_5V_OUT","W003 OUT","P114","L85_Protected5VControlSupply",-9.14250f,3.39000f,4.98000f,"red"),
            ("W005_THRUSTER_ALLOCATION_CONTROL_5V_OUT","W005 OUT","P114","L85_Protected5VControlSupply",-8.92250f,3.17000f,4.98000f,"red"),
            ("W007_LIDAR_SIGNAL_CONDITIONING_5V_OUT","W007 OUT","P114","L85_Protected5VControlSupply",-8.92250f,3.61000f,4.98000f,"red"),
            ("W009_SCADA_CONTROL_AND_VISUALIZATION_5V_OUT","W009 OUT","P114","L85_Protected5VControlSupply",-9.14250f,3.17000f,4.98000f,"red"),
            ("W115_INDEPENDENT_DOCKING_SAFETY_PLC_PROTECTED_CONT_OUT","W115 OUT","P114","L85_Protected5VControlSupply",-9.14250f,3.61000f,4.98000f,"red"),
            ("W002_CAPTURE_SEQUENCING_PLC_5V_LOGIC_OUT","W002 OUT","P115","L85_Protected5VControlSupply",-8.81750f,3.39000f,4.98000f,"red"),
            ("W004_SIX_DEGREE_FLIGHT_CONTROL_LOGIC_5V_OUT","W004 OUT","P115","L85_Protected5VControlSupply",-8.59750f,3.39000f,4.98000f,"red"),
            ("W006_VALVE_CONTROLLER_5V_COMMAND_POWER_OUT","W006 OUT","P115","L85_Protected5VControlSupply",-8.59750f,3.17000f,4.98000f,"red"),
            ("W008_DOCKING_CAMERA_MONITOR_5V_OUT","W008 OUT","P115","L85_Protected5VControlSupply",-8.59750f,3.61000f,4.98000f,"red"),
            ("W010_TERMINAL_DIAGNOSTIC_MONITOR_5V_OUT","W010 OUT","P115","L85_Protected5VControlSupply",-8.37750f,3.39000f,4.98000f,"red"),
            ("W070_PROTECTED_AUTONOMOUS_RETREAT_GUIDANCE_OUT","W070 OUT","P116","L85_ProtectedAutonomousAbortController",5.96750f,1.51000f,4.98000f,"mint"),
            ("W077_ABORT_CONTROLLER_ACTUAL_MODE_STATUS_OUT","W077 OUT","P117","L85_ProtectedAutonomousAbortController",6.07250f,1.51000f,4.98000f,"mint"),
            ("W126_MEASURED_SAFE_ABORT_COMPLETION_FEEDBACK_OUT","W126 OUT","P117","L85_ProtectedAutonomousAbortController",6.07250f,1.29000f,4.98000f,"mint"),
            ("W095_MEASURED_RETREAT_MANEUVER_EXECUTED_STATUS_OUT","W095 OUT","P118","L85_ProtectedAutonomousAbortController",6.17750f,1.51000f,4.98000f,"mint"),
            ("W071_PROPULSION_ABILITY_AND_ATTITUDE_CONTROL_FEEDB_IN","W071 IN","P119","L85_ProtectedAutonomousAbortController",6.28250f,1.51000f,4.98000f,"mint"),
            ("W069_GUIDANCE_PROTECTIVE_ABORT_REQUEST_IN","W069 IN","P120","L85_ProtectedAutonomousAbortController",6.38750f,1.51000f,4.98000f,"mint"),
            ("W125_SAFETY_AUTHORIZED_ESCAPE_RETREAT_ENABLE_IN","W125 IN","P120","L85_ProtectedAutonomousAbortController",6.38750f,1.29000f,4.98000f,"mint"),
            ("W094_EMERGENCY_HOLD_RETREAT_ARBITRATION_REQUEST_IN","W094 IN","P121","L85_ProtectedAutonomousAbortController",6.49250f,1.51000f,4.98000f,"mint"),
            ("W061_DESIRED_SIX_DEGREE_TRANSLATIONAL_GUIDANCE_VEC_OUT","W061 OUT","P122","L85_ProximityOperationsGuidanceComputer",-2.13250f,1.51000f,4.98000f,"mint"),
            ("W072_PREDICTED_ORBITAL_APPROACH_OVERLAY_OUT","W072 OUT","P123","L85_ProximityOperationsGuidanceComputer",-2.02750f,1.51000f,4.98000f,"mint"),
            ("W090_COMMANDED_APPROACH_CORRIDOR_OVERLAY_OUT","W090 OUT","P124","L85_ProximityOperationsGuidanceComputer",-1.92250f,1.51000f,4.98000f,"mint"),
            ("W074_GUIDANCE_CONTROLLER_STATE_AND_TRACKING_TREND_OUT","W074 OUT","P125","L85_ProximityOperationsGuidanceComputer",-1.81750f,1.51000f,4.98000f,"mint"),
            ("W069_GUIDANCE_PROTECTIVE_ABORT_REQUEST_OUT","W069 OUT","P126","L85_ProximityOperationsGuidanceComputer",-1.71250f,1.51000f,4.98000f,"mint"),
            ("W062_ACTUAL_TRANSLATIONAL_CONTROL_TRACKING_ERROR_IN","W062 IN","P127","L85_ProximityOperationsGuidanceComputer",-1.60750f,1.51000f,4.98000f,"blue"),
            ("W063_MEASURED_RANGE_RATE_AND_STOPPING_DISTANCE_IN","W063 IN","P128","L85_ProximityOperationsGuidanceComputer",-2.13250f,1.61500f,4.98000f,"blue"),
            ("W060_PLANNED_RENDEZVOUS_TRAJECTORY_AND_CORRIDOR_IN","W060 IN","P129","L85_ProximityOperationsGuidanceComputer",-2.02750f,1.61500f,4.98000f,"mint"),
            ("W067_APPROACH_HOLD_POINT_PERMISSION_AND_MODE_IN","W067 IN","P130","L85_ProximityOperationsGuidanceComputer",-1.92250f,1.61500f,4.98000f,"mint"),
            ("W083_COLLISION_BOUNDARY_CLEARANCE_AUTHORIZATION_IN","W083 IN","P130","L85_ProximityOperationsGuidanceComputer",-1.92250f,1.83500f,4.98000f,"mint"),
            ("W068_PREDICTED_COLLISION_FREE_MANEUVER_CERTIFICATE_IN","W068 IN","P131","L85_ProximityOperationsGuidanceComputer",-1.81750f,1.61500f,4.98000f,"mint"),
            ("W044_RCS_THRUSTER_Y_ACTUAL_PULSE_FEEDBACK_OUT","W044 OUT","P132","L85_RCSMinusXThruster",-5.96250f,4.17000f,-0.25000f,"blue"),
            ("W038_RCS_THRUSTER_Y_VALVE_PULSE_COMMAND_IN","W038 IN","P133","L85_RCSMinusXThruster",-5.85750f,4.17000f,-0.25000f,"mint"),
            ("W046_RCS_THRUSTER_X_ACTUAL_PULSE_FEEDBACK_OUT","W046 OUT","P134","L85_RCSMinusYThruster",-5.06250f,3.91000f,-1.35000f,"blue"),
            ("W040_RCS_THRUSTER_X_VALVE_PULSE_COMMAND_IN","W040 IN","P135","L85_RCSMinusYThruster",-4.95750f,3.91000f,-1.35000f,"mint"),
            ("W048_RCS_THRUSTER_Z_ACTUAL_PULSE_FEEDBACK_OUT","W048 OUT","P136","L85_RCSMinusZThruster",-4.51250f,4.84000f,-2.65000f,"blue"),
            ("W042_RCS_THRUSTER_Z_VALVE_PULSE_COMMAND_IN","W042 IN","P137","L85_RCSMinusZThruster",-4.40750f,4.84000f,-2.65000f,"mint"),
            ("W043_RCS_THRUSTER_X_ACTUAL_PULSE_FEEDBACK_OUT","W043 OUT","P138","L85_RCSPlusXThruster",-4.46250f,5.56000f,-0.25000f,"blue"),
            ("W037_RCS_THRUSTER_X_VALVE_PULSE_COMMAND_IN","W037 IN","P139","L85_RCSPlusXThruster",-4.35750f,5.56000f,-0.25000f,"mint"),
            ("W045_RCS_THRUSTER_Z_ACTUAL_PULSE_FEEDBACK_OUT","W045 OUT","P140","L85_RCSPlusYThruster",-5.06250f,5.74000f,-1.35000f,"blue"),
            ("W039_RCS_THRUSTER_Z_VALVE_PULSE_COMMAND_IN","W039 IN","P141","L85_RCSPlusYThruster",-4.95750f,5.74000f,-1.35000f,"mint"),
            ("W047_RCS_THRUSTER_Y_ACTUAL_PULSE_FEEDBACK_OUT","W047 OUT","P142","L85_RCSPlusZThruster",-5.71250f,4.84000f,0.50000f,"blue"),
            ("W041_RCS_THRUSTER_Y_VALVE_PULSE_COMMAND_IN","W041 IN","P143","L85_RCSPlusZThruster",-5.60750f,4.84000f,0.50000f,"mint"),
            ("W058_PROPELLANT_FEED_PRESSURE_TRANSDUCER_RESPONSE_OUT","W058 OUT","P144","L85_ReactionControlPropellantManifold",-5.76250f,4.44000f,-0.15000f,"blue"),
            ("W052_PROPELLANT_MANIFOLD_DELIVERY_READINESS_OUT","W052 OUT","P145","L85_ReactionControlPropellantManifold",-5.65750f,4.44000f,-0.15000f,"mint"),
            ("W050_PROPELLANT_TANK_A_AVAILABLE_FEED_PRESSURE_IN","W050 IN","P146","L85_ReactionControlPropellantManifold",-5.55250f,4.44000f,-0.15000f,"blue"),
            ("W051_PROPELLANT_TANK_B_AVAILABLE_FEED_PRESSURE_IN","W051 IN","P147","L85_ReactionControlPropellantManifold",-5.44750f,4.44000f,-0.15000f,"blue"),
            ("W122_NAVIGATION_FUSION_RAW_RANGE_RATE_CALIBRATION__OUT","W122 OUT","P149","L85_RelativeNavigationFusionComputer",-6.06750f,1.51000f,4.98000f,"blue"),
            ("W023_NAVIGATION_COVARIANCE_AND_CONFIDENCE_ELLIPSOI_OUT","W023 OUT","P150","L85_RelativeNavigationFusionComputer",-5.96250f,1.51000f,4.98000f,"mint"),
            ("W073_ESTIMATED_POSITIONAL_UNCERTAINTY_DISTRIBUTION_OUT","W073 OUT","P150","L85_RelativeNavigationFusionComputer",-5.96250f,1.29000f,4.98000f,"mint"),
            ("W059_FUSED_RELATIVE_STATE_FOR_ORBITAL_TRAJECTORY_P_OUT","W059 OUT","P151","L85_RelativeNavigationFusionComputer",-5.85750f,1.51000f,4.98000f,"mint"),
            ("W131_FUSED_POSITION_AND_VELOCITY_STATE_SCADA_OUT","W131 OUT","P151","L85_RelativeNavigationFusionComputer",-5.63750f,1.29000f,4.98000f,"mint"),
            ("W027_PRIMARY_LIDAR_TIMED_SCAN_TRIGGER_OUT","W027 OUT","P152","L85_RelativeNavigationFusionComputer",-5.75250f,1.51000f,4.98000f,"mint"),
            ("W029_RENDEZVOUS_RADAR_MODE_REQUEST_OUT","W029 OUT","P152","L85_RelativeNavigationFusionComputer",-5.31250f,1.51000f,4.98000f,"mint"),
            ("W028_SECONDARY_LIDAR_TIMED_SCAN_TRIGGER_OUT","W028 OUT","P153","L85_RelativeNavigationFusionComputer",-5.64750f,1.51000f,4.98000f,"mint"),
            ("W030_TARGET_OPTICAL_ACQUISITION_COMMAND_OUT","W030 OUT","P153","L85_RelativeNavigationFusionComputer",-5.42750f,1.29000f,4.98000f,"mint"),
            ("W011_IMU_MEASURED_ACCELERATION_AND_ANGULAR_RATE_IN","W011 IN","P154","L85_RelativeNavigationFusionComputer",-6.17250f,1.61500f,4.98000f,"blue"),
            ("W019_DOCKING_FIDUCIAL_PATTERN_CONFIDENCE_IN","W019 IN","P155","L85_RelativeNavigationFusionComputer",-6.06750f,1.61500f,4.98000f,"blue"),
            ("W014_STEREO_TRIANGULATED_OPTICAL_TARGET_POSE_IN","W014 IN","P156","L85_RelativeNavigationFusionComputer",-5.96250f,1.61500f,4.98000f,"mint"),
            ("W020_ESTIMATED_RELATIVE_POSITION_AND_SPEED_IN","W020 IN","P157","L85_RelativeNavigationFusionComputer",-5.85750f,1.61500f,4.98000f,"mint"),
            ("W018_DOCK_TARGET_BEACON_IDENTITY_VALIDATION_IN","W018 IN","P158","L85_RelativeNavigationFusionComputer",-5.75250f,1.61500f,4.98000f,"mint"),
            ("W026_INJECTED_LIDAR_BLIND_SPOT_SIGNAL_QUALITY_IN","W026 IN","P158","L85_RelativeNavigationFusionComputer",-5.75250f,1.83500f,4.98000f,"mint"),
            ("W022_NAVIGATION_TIMESTAMP_AND_CLOCK_SYNCHRONIZATIO_IN","W022 IN","P159","L85_RelativeNavigationFusionComputer",-5.64750f,1.61500f,4.98000f,"mint"),
            ("W003_NAVIGATION_FUSION_COMPUTER_5V_IN","W003 IN","P160","L85_RelativeNavigationFusionComputer",-6.17250f,1.72000f,4.98000f,"red"),
            ("W072_PREDICTED_ORBITAL_APPROACH_OVERLAY_IN","W072 IN","P161","L85_RelativeOrbitTrajectoryHologram",-0.26250f,7.59000f,-5.05000f,"mint"),
            ("W063_MEASURED_RANGE_RATE_AND_STOPPING_DISTANCE_OUT","W063 OUT","P162","L85_RelativePositionVelocityEstimator",-7.51250f,1.51000f,4.98000f,"blue"),
            ("W124_DISPLAY_ONLY_RELATIVE_SPEED_TELEMETRY_NOT_W12_OUT","W124 OUT","P162","L85_RelativePositionVelocityEstimator",-7.73250f,1.51000f,4.98000f,"blue"),
            ("W081_RELATIVE_TARGET_APPROACH_SPEED_ESTIMATE_OUT","W081 OUT","P163","L85_RelativePositionVelocityEstimator",-7.40750f,1.51000f,4.98000f,"blue"),
            ("W132_MEASURED_TARGET_CLOSING_RATE_DISPLAY_SUMMARY_OUT","W132 OUT","P163","L85_RelativePositionVelocityEstimator",-7.40750f,1.29000f,4.98000f,"blue"),
            ("W020_ESTIMATED_RELATIVE_POSITION_AND_SPEED_OUT","W020 OUT","P164","L85_RelativePositionVelocityEstimator",-7.30250f,1.51000f,4.98000f,"mint"),
            ("W032_RELATIVE_RANGE_AND_VELOCITY_TELEMETRY_DISPLAY_OUT","W032 OUT","P165","L85_RelativePositionVelocityEstimator",-7.19750f,1.51000f,4.98000f,"mint"),
            ("W015_PRIMARY_LIDAR_MEASURED_RANGE_SCAN_IN","W015 IN","P166","L85_RelativePositionVelocityEstimator",-7.09250f,1.51000f,4.98000f,"blue"),
            ("W017_RADAR_INDEPENDENT_RELATIVE_RANGE_RATE_IN","W017 IN","P166","L85_RelativePositionVelocityEstimator",-7.09250f,1.29000f,4.98000f,"blue"),
            ("W016_SECONDARY_LIDAR_MEASURED_RANGE_SCAN_IN","W016 IN","P167","L85_RelativePositionVelocityEstimator",-6.98750f,1.51000f,4.98000f,"blue"),
            ("W021_INERTIAL_RELATIVE_STATE_PROPAGATION_SAMPLES_IN","W021 IN","P167","L85_RelativePositionVelocityEstimator",-6.76750f,1.51000f,4.98000f,"blue"),
            ("W060_PLANNED_RENDEZVOUS_TRAJECTORY_AND_CORRIDOR_OUT","W060 OUT","P168","L85_RelativeTrajectoryPlanningComputer",-3.48250f,1.51000f,4.98000f,"mint"),
            ("W075_TRAJECTORY_DELTA_V_FORECAST_TELEMETRY_OUT","W075 OUT","P168","L85_RelativeTrajectoryPlanningComputer",-3.70250f,1.51000f,4.98000f,"mint"),
            ("W066_FORECASTED_RENDEZVOUS_IMPULSE_BUDGET_OUT","W066 OUT","P169","L85_RelativeTrajectoryPlanningComputer",-3.37750f,1.51000f,4.98000f,"mint"),
            ("W065_PROPELLANT_AND_ABORT_MARGIN_CONSTRAINT_IN","W065 IN","P170","L85_RelativeTrajectoryPlanningComputer",-3.27250f,1.51000f,4.98000f,"blue"),
            ("W059_FUSED_RELATIVE_STATE_FOR_ORBITAL_TRAJECTORY_P_IN","W059 IN","P171","L85_RelativeTrajectoryPlanningComputer",-3.16750f,1.51000f,4.98000f,"mint"),
            ("W064_TRAJECTORY_PROPAGATION_TIME_REFERENCE_IN","W064 IN","P172","L85_RelativeTrajectoryPlanningComputer",-3.06250f,1.51000f,4.98000f,"mint"),
            ("W016_SECONDARY_LIDAR_MEASURED_RANGE_SCAN_OUT","W016 OUT","P173","L85_SecondaryScanningLiDAR",-3.16250f,4.34000f,-0.80000f,"blue"),
            ("W025_SECONDARY_LIDAR_SENSOR_REDUNDANCY_DIAGNOSTIC_OUT","W025 OUT","P174","L85_SecondaryScanningLiDAR",-3.05750f,4.34000f,-0.80000f,"mint"),
            ("W028_SECONDARY_LIDAR_TIMED_SCAN_TRIGGER_IN","W028 IN","P175","L85_SecondaryScanningLiDAR",-2.95250f,4.34000f,-0.80000f,"mint"),
            ("W017_RADAR_INDEPENDENT_RELATIVE_RANGE_RATE_OUT","W017 OUT","P176","L85_ShortRangeRendezvousRadar",-3.46250f,4.84000f,-1.90000f,"blue"),
            ("W079_SHORT_RANGE_APPROACHING_OBJECT_RADIAL_DISTANC_OUT","W079 OUT","P177","L85_ShortRangeRendezvousRadar",-3.35750f,4.84000f,-1.90000f,"blue"),
            ("W029_RENDEZVOUS_RADAR_MODE_REQUEST_IN","W029 IN","P178","L85_ShortRangeRendezvousRadar",-3.25250f,4.84000f,-1.90000f,"mint"),
            ("W102_SIX_HARD_LATCH_ACTUAL_LOCK_FEEDBACK_CHANNELS_OUT","W102 OUT","P179","L85_SixActuatorDockingLatchArray",2.38750f,4.40000f,-1.20000f,"blue"),
            ("W101_SIX_INDEPENDENT_HARD_LATCH_ACTUATION_PERMITS_IN","W101 IN","P180","L85_SixActuatorDockingLatchArray",2.93250f,4.84000f,-1.20000f,"mint"),
            ("W103_SIX_AXIS_CAPTURED_CONTACT_FORCE_FEEDBACK_OUT","W103 OUT","P181","L85_SixAxisContactForceSensor",2.45750f,4.84000f,-1.20000f,"blue"),
            ("W035_REQUESTED_SIX_DEGREE_THRUSTER_WRENCH_OUT","W035 OUT","P182","L85_SixDegreeFreedomFlightController",-0.78250f,1.51000f,4.98000f,"mint"),
            ("W078_REAL_TIME_MICROTHRUSTER_BODY_WRENCH_UPDATE_OUT","W078 OUT","P183","L85_SixDegreeFreedomFlightController",-0.67750f,1.51000f,4.98000f,"mint"),
            ("W062_ACTUAL_TRANSLATIONAL_CONTROL_TRACKING_ERROR_OUT","W062 OUT","P184","L85_SixDegreeFreedomFlightController",-0.57250f,1.51000f,4.98000f,"blue"),
            ("W133_SIX_DEGREE_TRANSLATION_ACTUAL_CONTROL_TELEMET_OUT","W133 OUT","P185","L85_SixDegreeFreedomFlightController",-0.46750f,1.51000f,4.98000f,"mint"),
            ("W071_PROPULSION_ABILITY_AND_ATTITUDE_CONTROL_FEEDB_OUT","W071 OUT","P186","L85_SixDegreeFreedomFlightController",-0.36250f,1.51000f,4.98000f,"mint"),
            ("W061_DESIRED_SIX_DEGREE_TRANSLATIONAL_GUIDANCE_VEC_IN","W061 IN","P187","L85_SixDegreeFreedomFlightController",-0.25750f,1.51000f,4.98000f,"mint"),
            ("W070_PROTECTED_AUTONOMOUS_RETREAT_GUIDANCE_IN","W070 IN","P188","L85_SixDegreeFreedomFlightController",-0.78250f,1.61500f,4.98000f,"mint"),
            ("W049_ACTUAL_DELIVERED_IMPULSE_AND_PRESSURE_SAMPLES_IN","W049 IN","P189","L85_SixDegreeFreedomFlightController",-0.67750f,1.61500f,4.98000f,"blue"),
            ("W004_SIX_DEGREE_FLIGHT_CONTROL_LOGIC_5V_IN","W004 IN","P190","L85_SixDegreeFreedomFlightController",-0.57250f,1.61500f,4.98000f,"red"),
            ("W117_INDEPENDENT_THRUSTER_INHIBIT_AND_AUTHORIZATIO_IN","W117 IN","P191","L85_SixDegreeFreedomFlightController",-0.46750f,1.61500f,4.98000f,"mint"),
            ("W011_IMU_MEASURED_ACCELERATION_AND_ANGULAR_RATE_OUT","W011 OUT","P192","L85_SpacecraftInertialMeasurementUnit",-4.01250f,4.34000f,-0.67000f,"blue"),
            ("W021_INERTIAL_RELATIVE_STATE_PROPAGATION_SAMPLES_OUT","W021 OUT","P193","L85_SpacecraftInertialMeasurementUnit",-3.90750f,4.34000f,-0.67000f,"blue"),
            ("W034_RAW_SPACECRAFT_IMU_HEALTH_TELEMETRY_OUT","W034 OUT","P194","L85_SpacecraftInertialMeasurementUnit",-3.80250f,4.34000f,-0.67000f,"mint"),
            ("W087_DOCK_BEACON_CODED_ACQUISITION_SIGNAL_OUT","W087 OUT","P195","L85_StationApproachBeaconCluster",5.53750f,4.84000f,-1.15000f,"mint"),
            ("W089_DOCKING_APPROACH_LIGHTING_STATUS_OUT","W089 OUT","P196","L85_StationApproachIlluminationRig",0.73750f,6.89000f,-2.85000f,"mint"),
            ("W086_STATION_TARGET_FIDUCIAL_COORDINATES_OUT","W086 OUT","P197","L85_StationOpticalTargetMarkerArray",2.73750f,4.84000f,-1.20000f,"mint"),
            ("W012_LEFT_NAVIGATION_CAMERA_IMAGE_SAMPLES_OUT","W012 OUT","P198","L85_StereoNavigationCameraLeft",-3.51250f,5.16000f,-0.65000f,"mint"),
            ("W013_RIGHT_NAVIGATION_CAMERA_IMAGE_SAMPLES_OUT","W013 OUT","P199","L85_StereoNavigationCameraRight",-3.51250f,4.54000f,-0.65000f,"mint"),
            ("W137_STUCK_VALVE_FAULT_AND_ISOLATION_REPORT_OUT","W137 OUT","P200","L85_StuckThrusterValveFaultEmulator",11.41750f,1.51000f,4.98000f,"mint"),
            ("W054_STUCK_THRUSTER_VALVE_MALFUNCTION_INJECTION_OUT","W054 OUT","P201","L85_StuckThrusterValveFaultEmulator",11.52250f,1.51000f,4.98000f,"mint"),
            ("W055_FAULT_INJECTOR_CONFIGURED_ISOLATION_READINESS_IN","W055 IN","P202","L85_StuckThrusterValveFaultEmulator",11.62750f,1.51000f,4.98000f,"mint"),
            ("W090_COMMANDED_APPROACH_CORRIDOR_OVERLAY_IN","W090 IN","P203","L85_ThreeDimensionalApproachCorridorHologram",-0.26250f,4.84000f,-1.20000f,"mint"),
            ("W091_ACTUAL_PROXIMITY_WARNING_BOUNDARY_VISUAL_IN","W091 IN","P204","L85_ThreeDimensionalApproachCorridorHologram",-0.15750f,4.84000f,-1.20000f,"mint"),
            ("W049_ACTUAL_DELIVERED_IMPULSE_AND_PRESSURE_SAMPLES_OUT","W049 OUT","P205","L85_ThrusterFlowAndPressureMonitor",-10.29250f,1.51000f,4.98000f,"blue"),
            ("W135_PROPELLANT_FLOW_AND_PULSE_HISTORY_TREND_OUT","W135 OUT","P206","L85_ThrusterFlowAndPressureMonitor",-10.18750f,1.51000f,4.98000f,"blue"),
            ("W043_RCS_THRUSTER_X_ACTUAL_PULSE_FEEDBACK_IN","W043 IN","P207","L85_ThrusterFlowAndPressureMonitor",-10.08250f,1.51000f,4.98000f,"blue"),
            ("W045_RCS_THRUSTER_Z_ACTUAL_PULSE_FEEDBACK_IN","W045 IN","P207","L85_ThrusterFlowAndPressureMonitor",-10.08250f,1.29000f,4.98000f,"blue"),
            ("W047_RCS_THRUSTER_Y_ACTUAL_PULSE_FEEDBACK_IN","W047 IN","P207","L85_ThrusterFlowAndPressureMonitor",-10.08250f,1.73000f,4.98000f,"blue"),
            ("W058_PROPELLANT_FEED_PRESSURE_TRANSDUCER_RESPONSE_IN","W058 IN","P207","L85_ThrusterFlowAndPressureMonitor",-10.30250f,1.29000f,4.98000f,"blue"),
            ("W044_RCS_THRUSTER_Y_ACTUAL_PULSE_FEEDBACK_IN","W044 IN","P208","L85_ThrusterFlowAndPressureMonitor",-9.97750f,1.51000f,4.98000f,"blue"),
            ("W046_RCS_THRUSTER_X_ACTUAL_PULSE_FEEDBACK_IN","W046 IN","P208","L85_ThrusterFlowAndPressureMonitor",-9.75750f,1.51000f,4.98000f,"blue"),
            ("W048_RCS_THRUSTER_Z_ACTUAL_PULSE_FEEDBACK_IN","W048 IN","P208","L85_ThrusterFlowAndPressureMonitor",-9.75750f,1.29000f,4.98000f,"blue"),
            ("W036_THRUSTER_COMMAND_ALLOCATION_AND_PULSE_TRAIN_OUT","W036 OUT","P209","L85_ThrusterForceAllocationComputer",0.56750f,1.51000f,4.98000f,"mint"),
            ("W057_PREDICTED_COMMANDED_THRUSTER_FUEL_USAGE_OUT","W057 OUT","P210","L85_ThrusterForceAllocationComputer",0.67250f,1.51000f,4.98000f,"blue"),
            ("W035_REQUESTED_SIX_DEGREE_THRUSTER_WRENCH_IN","W035 IN","P211","L85_ThrusterForceAllocationComputer",0.77750f,1.51000f,4.98000f,"mint"),
            ("W078_REAL_TIME_MICROTHRUSTER_BODY_WRENCH_UPDATE_IN","W078 IN","P212","L85_ThrusterForceAllocationComputer",0.88250f,1.51000f,4.98000f,"mint"),
            ("W053_MEASURED_THRUSTER_CALIBRATION_THRUST_TABLE_IN","W053 IN","P213","L85_ThrusterForceAllocationComputer",0.98750f,1.51000f,4.98000f,"mint"),
            ("W005_THRUSTER_ALLOCATION_CONTROL_5V_IN","W005 IN","P214","L85_ThrusterForceAllocationComputer",1.09250f,1.51000f,4.98000f,"red"),
            ("W056_ABORT_FUEL_RESERVE_THRUST_ALLOCATION_LIMIT_IN","W056 IN","P215","L85_ThrusterForceAllocationComputer",0.56750f,1.61500f,4.98000f,"mint"),
            ("W037_RCS_THRUSTER_X_VALVE_PULSE_COMMAND_OUT","W037 OUT","P216","L85_ThrusterValveDriverRack",-11.66250f,1.51000f,4.98000f,"mint"),
            ("W039_RCS_THRUSTER_Z_VALVE_PULSE_COMMAND_OUT","W039 OUT","P216","L85_ThrusterValveDriverRack",-11.88250f,1.51000f,4.98000f,"mint"),
            ("W041_RCS_THRUSTER_Y_VALVE_PULSE_COMMAND_OUT","W041 OUT","P216","L85_ThrusterValveDriverRack",-11.66250f,1.29000f,4.98000f,"mint"),
            ("W038_RCS_THRUSTER_Y_VALVE_PULSE_COMMAND_OUT","W038 OUT","P217","L85_ThrusterValveDriverRack",-11.55750f,1.51000f,4.98000f,"mint"),
            ("W040_RCS_THRUSTER_X_VALVE_PULSE_COMMAND_OUT","W040 OUT","P217","L85_ThrusterValveDriverRack",-11.33750f,1.29000f,4.98000f,"mint"),
            ("W042_RCS_THRUSTER_Z_VALVE_PULSE_COMMAND_OUT","W042 OUT","P217","L85_ThrusterValveDriverRack",-11.33750f,1.73000f,4.98000f,"mint"),
            ("W055_FAULT_INJECTOR_CONFIGURED_ISOLATION_READINESS_OUT","W055 OUT","P218","L85_ThrusterValveDriverRack",-11.45250f,1.51000f,4.98000f,"mint"),
            ("W134_THRUSTER_VALVE_COMMANDED_VERSUS_ACTUAL_STATUS_OUT","W134 OUT","P219","L85_ThrusterValveDriverRack",-11.34750f,1.51000f,4.98000f,"mint"),
            ("W036_THRUSTER_COMMAND_ALLOCATION_AND_PULSE_TRAIN_IN","W036 IN","P220","L85_ThrusterValveDriverRack",-11.24250f,1.51000f,4.98000f,"mint"),
            ("W052_PROPELLANT_MANIFOLD_DELIVERY_READINESS_IN","W052 IN","P221","L85_ThrusterValveDriverRack",-11.13750f,1.51000f,4.98000f,"mint"),
            ("W006_VALVE_CONTROLLER_5V_COMMAND_POWER_IN","W006 IN","P222","L85_ThrusterValveDriverRack",-11.66250f,1.61500f,4.98000f,"red"),
            ("W054_STUCK_THRUSTER_VALVE_MALFUNCTION_INJECTION_IN","W054 IN","P223","L85_ThrusterValveDriverRack",-11.55750f,1.61500f,4.98000f,"mint"),
            ("","P224","P224","L85_SixAxisContactForceSensor",2.56250f,4.84000f,-1.20000f,"gold"),
        };

        public static IEnumerable<string> TerminalIds => SparkLevel85Wiring.Pins;

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 85")]
        public static void Install()
        {
            SparkLevel85Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel85Process>().rig == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 85/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel85Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.rig==null || control.rig.GetComponentsInChildren<Transform>(true).Count(item=>item.name.StartsWith("ANIM_",StringComparison.Ordinal))!=60 ||
                control.pins.Length!=292 || control.pins.Distinct().Count()!=292 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel85Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 85 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 85] Validated 292 distinct terminals, 146 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_85.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_85_Drawing.asset");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab).GetComponent<SparkLevelLayout>();
            Append(Find<LevelGamePlayManager>(), "levels", level);
            Append(Find<SparkDrawCurrentLevel>(), "drawings", drawing);
            var loader = new SerializedObject(Find<SparkLevelLayoutLoader>());
            var catalog = new SerializedObject(loader.FindProperty("catalog").objectReferenceValue);
            var entries = catalog.FindProperty("entries");
            var entry = Enumerable.Range(0, entries.arraySize).Select(entries.GetArrayElementAtIndex)
                .FirstOrDefault(item => item.FindPropertyRelative("level").objectReferenceValue == level);
            if (entry == null)
            {
                entries.arraySize++;
                entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
            }
            entry.FindPropertyRelative("level").objectReferenceValue = level;
            entry.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            catalog.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            if (!dirty && scene.isDirty && !EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("Could not save campaign registration.");
            if (opened) EditorSceneManager.CloseScene(scene, true);
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 85's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 85 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }


        [MenuItem("Tools/Project Spark/Level 85/Fit Docking Arena")]
        public static void Fit()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode to fit the docking arena.");
            var models=Models.Select((name,index)=>AssetDatabase.LoadAssetAtPath<GameObject>(
                Folder+$"Models/L85_{index:00}_{name}_Fitted.fbx")??throw new InvalidOperationException("Missing module "+name)).ToArray();
            var root=PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                Fit(root,models);
                if(PrefabUtility.SaveAsPrefabAsset(root,Prefab)==null)throw new InvalidOperationException("Could not save Level 85 arena.");
                AssetDatabase.SaveAssets();
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }

        private static void Fit(GameObject root, GameObject[] models)
        {
            var bench = root.transform.Find("Orbital Docking Arena");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel85Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 85 is missing its workbench, controls, or scene bindings.");


            const float floor=.425f;
            foreach (var terminal in loop.pins)
                if (terminal.Owner != null)
                    terminal.transform.SetParent(terminal.Owner.transform, true);
            foreach (string name in new[] {"L85_HD_VISUAL_ART", "Equipment terminal panels", "Motion pivots"})
            {
                var existing = bench.Find(name);
                if (existing != null) Object.DestroyImmediate(existing.gameObject);
            }
            foreach (string name in new[] {"Temporary board"})
            {
                var existing = bench.Find(name);
                if (existing != null) existing.gameObject.SetActive(false);
            }

            foreach (var renderer in bench.GetComponentsInChildren<MeshRenderer>(true))
                if (renderer.name=="Body") renderer.gameObject.SetActive(false);
            var assembly = Child(bench, "L85_HD_VISUAL_ART", new Vector3(0,floor,-.010f));
            for (var index=0;index<models.Length;index++)
            {
                var model=models[index];
                var instance = Object.Instantiate(model, assembly, false).transform;
                instance.name="L85_"+Models[index];
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L85_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L85_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * 1.12f;
            if (!AssetDatabase.IsValidFolder(Materials)) AssetDatabase.CreateFolder(Folder + "Models", "Materials");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("The project URP Lit shader is missing.");
            ConfigureTextures();
            foreach (var mesh in assembly.GetComponentsInChildren<MeshRenderer>(true))
            {
                string key = mesh.name.Substring(mesh.name.LastIndexOf("__", StringComparison.Ordinal) + 2);



                string surface=key switch
                {
                    "glass"=>"LiDAROpticalGlass",
                    _ when mesh.name.Contains("ArenaBase")=>"SpaceportStageFloor",
                    _ when mesh.name.Contains("Viewport")||mesh.name.Contains("Hologram")=>"ApproachCorridorHologram",
                    _ when mesh.name.Contains("Thruster")=>"MicrothrusterCopperValves",
                    _ when mesh.name.Contains("Solar")=>"StationSolarCellBlue",
                    _ when mesh.name.Contains("Guide")=>"MachinedTitaniumAlloy",
                    _ when mesh.name.Contains("Spacecraft")=>"SpacecraftHullComposite",
                    _ when mesh.name.Contains("Truss")=>"StationCarbonComposite",
                    _ when mesh.name.Contains("Latch")=>"DockingBrushedSteel",
                    _ when mesh.name.Contains("Interface")=>"DockingGoldContacts",
                    _ when mesh.name.Contains("LiDAR")=>"ThermalShieldCeramic",
                    _ when mesh.name.Contains("SCADA")=>"NavigationInstrumentPanel",
                    _=>null
                };
                if(mesh.name.Contains("PanoramicEarthAndStarfield")&&mesh.bounds.size.x>assembly.lossyScale.x*.78f)
                    {key="glass";surface="LiDAROpticalGlass";}
                var material = MaterialFor(shader,key,surface);

                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loop.eventText == null || loop.buttons.Length!=22)
            {
                var oldControls = root.transform.Find("Level 85 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel85Builder.BuildControls(root.transform,loop,font);
            }
            var labels = Child(bench, "Equipment terminal panels", Vector3.zero);
            int pinLayer = LayerMask.NameToLayer("Pin");
            if (pinLayer < 0) throw new InvalidOperationException("Missing Pin interaction layer.");
            var frames = new Dictionary<string, Transform>();
            loop.pinLabels=new TextMesh[loop.pins.Length];
            var allArt = assembly.GetComponentsInChildren<Transform>(true);
            foreach(var spec in Sockets)
            {
                if(!frames.TryGetValue(spec.model,out var frame))
                {
                    var module=allArt.FirstOrDefault(item=>item.name==spec.model);
                    if(module==null) throw new InvalidOperationException("Missing socket equipment "+spec.model);
                    frame=Child(labels,spec.model+" Fixed Terminals",Vector3.zero);
                    // The panel stays with the bench. It never follows a deployable wing or rotating pivot.
                    frames.Add(spec.model,frame);
                }
                Vector3 seat=assembly.TransformPoint(new Vector3(spec.x,spec.y+.08f,-spec.z)*.096f);
                Vector3 local=bench.InverseTransformPoint(seat);
                var mount=Child(frame,spec.anchor+" "+spec.id,local);
                var rim=MaterialFor(shader,spec.color);
                if(string.IsNullOrEmpty(spec.id))
                {
                    var disc=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    Object.DestroyImmediate(disc.GetComponent<Collider>());
                    disc.name="Decorative "+spec.anchor;
                    disc.transform.SetParent(mount,false);
                    disc.transform.localPosition=Vector3.back*.0014f;
                    disc.transform.localRotation=Quaternion.Euler(-90,0,0);
                    disc.transform.localScale=new Vector3(.005f,.0014f,.005f);
                    disc.GetComponent<Renderer>().sharedMaterial=rim;
                    continue;
                }
                if(!bindings.TryGetTerminal(spec.id,out var terminal))
                    throw new InvalidOperationException("Missing fixed terminal "+spec.id);
                int index=Array.IndexOf(loop.pins,terminal);
                terminal.transform.parent.Find("Body")?.gameObject.SetActive(false);
                terminal.transform.SetParent(mount,true);
                var pinData=new SerializedObject(terminal);
                pinData.FindProperty("maxConnections").intValue=SparkLevel85Wiring.Wires.Count(wire=>wire.a==index||wire.b==index);
                pinData.ApplyModifiedPropertiesWithoutUndo();
                Seat(terminal,bench,local,pinLayer,rim,MaterialFor(shader,"void"));
                var tag=Label(mount,font,"Label "+spec.id,spec.label,new Vector3(0,.009f,-.007f),.014f);
                tag.transform.localRotation=Quaternion.Euler(22,0,0);
                loop.pinLabels[index]=tag;
                tag.gameObject.SetActive(false);
            }
            foreach(var body in bench.GetComponentsInChildren<MeshRenderer>(true))
                if(body.name=="Body") body.gameObject.SetActive(false);
            var title=Label(labels,font,"Level name","THE LAST METER",new Vector3(0,floor+.018f,-.688f),.023f);
            title.color=new Color(.53f,.83f,.99f);
            Light Lamp(string name,Vector3 local,Color color,float intensity,float range)
            {
                var model=Child(assembly,name,local);
                var lamp=model.gameObject.AddComponent<Light>();
                lamp.type=LightType.Point;
                lamp.color=color;
                lamp.intensity=intensity;
                lamp.range=range;
                lamp.shadows=LightShadows.None;
                return lamp;
            }
            Lamp("Docking key",new Vector3(-.52f,.96f,-.31f),new Color(1,.93f,.81f),4.1f,2.3f);
            Lamp("Cool rim fill",new Vector3(.68f,.62f,.21f),new Color(.48f,.77f,1),2.9f,1.8f);
            foreach(var item in assembly.GetComponentsInChildren<Transform>(true))
            {
                int index=item.name.IndexOf("ANIM_",StringComparison.Ordinal);
                if(index>0&&item.name.StartsWith("fitted_",StringComparison.Ordinal)) item.name=item.name[index..];
            }
            loop.rig=assembly.gameObject.AddComponent<SparkLevel85VisualRig>();

            var spacecraft=Child(assembly,"L85_DYNAMIC_SERVICE_SPACECRAFT",new Vector3(-4.8f,5.13f,1.45f)*.096f);
            string[] exterior={"AutonomousServiceSpacecraftBody","ServiceSpacecraftExtendableDockingProbe","ProtectedServiceVehicleAvionics",
                "ServiceSpacecraftPortSolarWing","ServiceSpacecraftStarboardSolarWing"};
            foreach(string model in exterior)
                assembly.Find("L85_"+model)?.SetParent(spacecraft,true);
            var fx=new SerializedObject(loop.rig);
            fx.FindProperty("dynamicServiceSpacecraft").objectReferenceValue=spacecraft;
            var plumes=fx.FindProperty("sixThrusterPlumes");
            plumes.arraySize=6;
            var directions=new[]{Vector3.left,Vector3.right,Vector3.down,Vector3.up,Vector3.forward,Vector3.back};
            for(int i=0;i<directions.Length;i++)
            {
                Vector3 direction=directions[i];
                var nozzle=Child(spacecraft,"RCS exhaust "+(i+1),direction*.10f);
                nozzle.localRotation=Quaternion.LookRotation(direction,Mathf.Abs(direction.y)>.9f?Vector3.forward:Vector3.up);
                var plume=nozzle.gameObject.AddComponent<ParticleSystem>();
                plume.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=plume.main;
                main.playOnAwake=false;
                main.startColor=new Color(.26f,.75f,1f,.9f);
                main.startSize=.008f;
                main.startLifetime=.22f;
                main.startSpeed=.26f;
                main.maxParticles=45;
                var shape=plume.shape;
                shape.shapeType=ParticleSystemShapeType.Cone;
                shape.angle=13;
                shape.radius=.004f;
                var emission=plume.emission;
                emission.rateOverTime=0;
                var renderer=plume.GetComponent<ParticleSystemRenderer>();
                renderer.sharedMaterial=MaterialFor(shader,"cyan");
                renderer.shadowCastingMode=ShadowCastingMode.Off;
                plumes.GetArrayElementAtIndex(i).objectReferenceValue=plume;
            }

            fx.FindProperty("criticalFeedbackFaultLight").objectReferenceValue=Lamp("Trust fault",new Vector3(.61f,.31f,-.23f),new Color(1,.12f,.18f),.7f,.59f);
            fx.FindProperty("dockingQualificationLight").objectReferenceValue=Lamp("Hard-dock indicator",new Vector3(-.35f,.57f,.13f),new Color(.18f,.80f,1),.8f,.69f);
            fx.FindProperty("approachWarningLight").objectReferenceValue=Lamp("Approach warning",new Vector3(.31f,.32f,.14f),new Color(.73f,.20f,1),.6f,.68f);
            fx.ApplyModifiedPropertiesWithoutUndo();
            loop.arena=bench;

            // The supplied pivot artwork is driven from the authoritative plant each frame.
            // The 212 decorative socket records remain in the imported art and source plan.
        }

        private static void Seat(SparkTerminal terminal, Transform bench, Vector3 seat, int pinLayer, Material rim, Material center)
        {
            var jack = terminal.transform;
            jack.SetPositionAndRotation(bench.TransformPoint(seat + Vector3.back * (SocketHeight * .5f)), bench.rotation * Quaternion.Euler(-90,0,0));
            jack.localScale = new Vector3(SocketDiameter, SocketHeight * .5f, SocketDiameter);
            jack.gameObject.layer = pinLayer;
            jack.GetComponent<Renderer>().sharedMaterial = rim;
            var disc = jack.Find("Jack recess");
            if (disc == null)
            {
                var primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Object.DestroyImmediate(primitive.GetComponent<Collider>());
                disc = primitive.transform;
                disc.name = "Jack recess";
                disc.SetParent(jack, false);
                disc.localPosition = new Vector3(0, 1.02f, 0);
                disc.localScale = new Vector3(.59f, .035f, .59f);
            }
            disc.GetComponent<Renderer>().sharedMaterial = center;
            var box = jack.GetComponent<BoxCollider>();
            foreach (var collider in jack.GetComponents<Collider>())
                if (collider != box) Object.DestroyImmediate(collider);
            if (box == null) box = jack.gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(1.04f, 2.24f, 1.04f);
        }

        private static Transform Child(Transform parent, string name, Vector3 position)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = position;
            return child;
        }

        private static TextMesh Label(Transform parent, Font font, string name, string text, Vector3 position, float size)
        {
            var child = Child(parent, name, position);
            child.localRotation = Quaternion.Euler(90, 0, 0);
            child.localScale = Vector3.one * .20f;
            var label = child.gameObject.AddComponent<TextMesh>();
            label.font = font;
            label.fontSize = 76;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(.89f, .96f, 1f);
            label.text = text;
            child.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return label;
        }

        private static void ConfigureTextures()
        {
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{Folder+"Textures"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                if(AssetImporter.GetAtPath(path) is not TextureImporter importer)continue;
                var type=path.EndsWith("_Normal.png",StringComparison.Ordinal)?TextureImporterType.NormalMap:TextureImporterType.Default;
                bool srgb=path.EndsWith("_Albedo.png",StringComparison.Ordinal);
                if(importer.textureType==type&&importer.sRGBTexture==srgb&&importer.wrapMode==TextureWrapMode.Repeat)continue;
                importer.textureType=type;
                importer.sRGBTexture=srgb;
                importer.wrapMode=TextureWrapMode.Repeat;
                importer.SaveAndReimport();
            }
        }

        private static Material MaterialFor(Shader shader, string key,string surface=null)
        {
            string materialName=surface==null?"L85 "+key:"L85 "+key+" "+surface;
            string path = $"{Materials}/{materialName}.mat";
            if (key.StartsWith("tint",StringComparison.Ordinal) && key.Length==10) Palette[key]=key.Substring(4);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null && key is not ("glass" or "acrylic") &&
                ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var savedTint) ? savedTint : "9FB3C8"), out var savedColor) &&
                existing.GetColor("_BaseColor") == savedColor) return existing;
            ColorUtility.TryParseHtmlString("#" + (Palette.TryGetValue(key, out var tint) ? tint : "9FB3C8"), out var color);
            var material = existing != null ? existing : new Material(shader) { name = materialName };
            bool metal = key is "chrome" or "titanium" or "steel" or "gold" or "copper" or "aluminum" or "tintCFDCE2" or "tintA4B8C4" or "tintE36C27" or "tintDC9C2F";
            material.SetFloat("_Metallic", metal ? .71f : .02f);
            material.SetFloat("_Smoothness", metal ? .71f : key == "glass" ? .12f : .35f);
            if (key is "green" or "cyan" or "red" or "blue" or "orange" or "amber" or "mint" or "warm" or "ice" or "plasma")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * .71f);
            }
            else if (key.StartsWith("tint", StringComparison.Ordinal))
            {
                material.EnableKeyword("_EMISSION");
                float strength = key is "tint19CCEF" or "tint27D700" or "tintFFCE34" ? .30f : .12f;
                material.SetColor("_EmissionColor", color * strength);
            }
            if (key is "glass" or "glass_body" or "glass_soft" or "acrylic" or "glassblue" or "film" or "Glass" or "guardglass" or "plasma")
            {
                color.a = key=="plasma" ? .27f : key=="film" ? .70f : .10f;
                material.SetFloat("_BlendModePreserveSpecular", 0);
                material.SetFloat("_Cull", 0);
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.SetFloat("_Surface", 1);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0);
                material.SetOverrideTag("RenderType", "Transparent");
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.SetShaderPassEnabled("ShadowCaster", false);
                material.renderQueue = (int)RenderQueue.Transparent;
            }
            if(surface!=null)
            {
                string prefix=Folder+"Textures/";
                var albedo=AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+"Authored/"+surface+"_Albedo.png");
                var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+"Authored/"+surface+"_Normal.png");
                var packed=AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+surface+"_MetalSmoothAO.png");
                if(albedo==null||normal==null||packed==null)
                    throw new InvalidOperationException("Missing Level 85 surface maps for "+surface);
                material.SetTexture("_BaseMap",albedo);
                material.SetTexture("_BumpMap",normal);
                material.SetFloat("_BumpScale",.47f);
                material.EnableKeyword("_NORMALMAP");
                material.SetTexture("_MetallicGlossMap",packed);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
                material.SetTexture("_OcclusionMap",packed);
                material.SetFloat("_OcclusionStrength",.42f);
                material.EnableKeyword("_OCCLUSIONMAP");
                material.SetFloat("_Smoothness",.84f);
                float alpha=color.a;
                color=Color.Lerp(color,Color.white,.48f);
                color.a=alpha;
            }
            material.SetColor("_BaseColor", color);
            if (existing == null) AssetDatabase.CreateAsset(material, path);
            else EditorUtility.SetDirty(material);
            return material;
        }
    }
}
#endif





















