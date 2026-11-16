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
    public static class SparkLevel84ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_84_Layout/";
        public const string Prefab = Folder + "Level_84_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "CelestialDynamicsArenaBase","PanoramicStarfieldSphere","ThreeAxisSpacecraftTestRig","RotationalTestRigPedestal","SpacecraftAttitudeTestVehicle","ProtectedAvionicsChassis","FourChipletMissionPayload","CoPackagedOpticalTransmitter","PortSolarArrayWing","StarboardSolarArrayWing","HolographicEarthReferenceSphere","StellarNavigationCatalogDome","InertialReferenceTargetHologram","OpticalGroundStationAimReticle","OrbitalEphemerisDisplayRail","AngularMomentumVectorHologram","SpacecraftBodyAxesHologram","ReactionWheelClusterHousing","ReactionWheelX","ReactionWheelY","ReactionWheelZ","SkewBackupReactionWheel","ReactionWheelMotorDriverRack","ReactionWheelTachometerArray","ReactionWheelMotorCurrentMonitor","ReactionWheelThermalControlModule","MagneticTorqueRodX","MagneticTorqueRodY","MagneticTorqueRodZ","MagneticTorqueRodDriveController","TriaxialMagnetometer","OrbitalMagneticFieldEmulator","HolographicMagneticFieldLines","MomentumDesaturationController","PrecisionGyroscopeIMU","GyroscopeCalibrationStation","GyroBiasThermalFaultEmulator","PrimaryStarTracker","StarTrackerOpticalBaffle","SecondaryDiagnosticStarTracker","StarCatalogPatternMatcher","CoarseSunSensorCluster","EarthHorizonSensor","AttitudeStateEstimator","ComplementaryFusionDisplay","HolographicQuaternionAttitudeSphere","EstimatorUncertaintyDisplay","AttitudeFlightController","ThreeAxisTorqueAllocationComputer","SlewTrajectoryPlanner","PrecisionPointingErrorAnalyzer","InertialReferenceClock","ExternalDisturbanceTorqueInjector","GravityGradientTorqueEmulator","OrbitalDragTorqueEmulator","SolarRadiationPressureTorqueModel","SolarWingFlexibleModeEmulator","StructuralVibrationSensor","ActivePointingVibrationFilter","OpticalLineOfSightSensor","GroundStationTargetProjector","OpticalLinkAcquisitionFixture","OpticalSignalQualityTelemetry","MissionPayloadBoresightReference","SunSafeAttitudeController","AttitudeSafeModeSupervisor","ReactionWheelSaturationAlarm","ReactionWheelDesaturationSupervisor","CriticalAttitudeFeedbackStation","IntegratedADCSPLCRack","Regulated5VControlSupply","IndependentMotionSafetyStation","HolographicAttitudeControlSCADA","ADCSCertificationBeacon","EmergencyAttitudeStopStation","AttitudeFlightDataRecorder"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .0075f;
        private const float SocketDiameter = .0086f;

        private static readonly (string id, string label, string anchor, string model, float x, float y, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","5 V OUT","P001","L84_Regulated5VControlSupply",6.07000f,1.44000f,5.09000f,"red"),
            ("ADCS_PLC_LOGIC_POWER_IN","W001 IN","P002","L84_IntegratedADCSPLCRack",4.77000f,1.44000f,5.09000f,"red"),
            ("FLIGHT_CONTROLLER_5V_SUPPLY_IN","W002 IN","P003","L84_AttitudeFlightController",-1.88000f,1.44000f,5.09000f,"red"),
            ("STATE_ESTIMATOR_5V_LOGIC_IN","W003 IN","P004","L84_AttitudeStateEstimator",-5.68000f,1.44000f,5.09000f,"red"),
            ("WHEEL_MOTOR_DRIVER_CONTROL_LOGIC_IN","W004 IN","P005","L84_ReactionWheelMotorDriverRack",-11.43000f,1.34000f,4.39000f,"red"),
            ("MAGNETORQUER_DRIVER_CONTROL_LOGIC_IN","W005 IN","P006","L84_MagneticTorqueRodDriveController",-5.53000f,1.89000f,-4.47000f,"red"),
            ("GYROSCOPE_SIGNAL_CONDITIONING_IN","W006 IN","P007","L84_PrecisionGyroscopeIMU",7.17000f,2.09000f,4.24000f,"red"),
            ("PRIMARY_STAR_TRACKER_SIGNAL_CONDITIONING_IN","W007 IN","P008","L84_PrimaryStarTracker",8.87000f,3.99000f,0.69000f,"red"),
            ("SECONDARY_STAR_TRACKER_LOGIC_IN","W008 IN","P009","L84_SecondaryDiagnosticStarTracker",11.32000f,3.99000f,0.69000f,"red"),
            ("HOLOGRAPHIC_SCADA_LOGIC_POWER_IN","W009 IN","P010","L84_HolographicAttitudeControlSCADA",-0.23000f,7.59000f,-4.91000f,"red"),
            ("INDEPENDENT_MOTION_SAFETY_LOGIC_POWER_IN","W010 IN","P011","L84_IndependentMotionSafetyStation",7.37000f,1.44000f,5.09000f,"red"),
            ("WHEEL_X_REQUESTED_TORQUE_COMMAND_OUT","W011 OUT","P012","L84_AttitudeFlightController",-1.76500f,1.44000f,5.09000f,"mint"),
            ("WHEEL_Z_REQUESTED_TORQUE_COMMAND_OUT","W019 OUT","P012","L84_AttitudeFlightController",-1.76500f,1.23000f,5.09000f,"mint"),
            ("DESIRED_SPACECRAFT_BODY_TORQUE_OUT","W078 OUT","P012","L84_AttitudeFlightController",-1.97500f,1.23000f,5.09000f,"mint"),
            ("WHEEL_X_REQUESTED_TORQUE_COMMAND_IN","W011 IN","P013","L84_ReactionWheelMotorDriverRack",-11.31500f,1.34000f,4.39000f,"mint"),
            ("WHEEL_Z_REQUESTED_TORQUE_COMMAND_IN","W019 IN","P013","L84_ReactionWheelMotorDriverRack",-11.31500f,1.13000f,4.39000f,"mint"),
            ("FOUR_WHEEL_TORQUE_ALLOCATION_IN","W079 IN","P013","L84_ReactionWheelMotorDriverRack",-11.52500f,1.13000f,4.39000f,"mint"),
            ("WHEEL_X_ACTUAL_MOTOR_DRIVE_OUT","W012 OUT","P014","L84_ReactionWheelMotorDriverRack",-11.20000f,1.34000f,4.39000f,"mint"),
            ("WHEEL_Z_ACTUAL_MOTOR_DRIVE_OUT","W020 OUT","P014","L84_ReactionWheelMotorDriverRack",-11.20000f,1.55000f,4.39000f,"mint"),
            ("WHEEL_X_ACTUAL_MOTOR_DRIVE_IN","W012 IN","P015","L84_ReactionWheelX",-11.78000f,2.94000f,0.85000f,"mint"),
            ("WHEEL_X_SHAFT_VELOCITY_FEEDBACK_OUT","W013 OUT","P016","L84_ReactionWheelX",-11.66500f,2.94000f,0.85000f,"blue"),
            ("WHEEL_X_SHAFT_VELOCITY_FEEDBACK_IN","W013 IN","P017","L84_ReactionWheelTachometerArray",-10.18000f,1.34000f,4.39000f,"blue"),
            ("WHEEL_Z_SHAFT_VELOCITY_FEEDBACK_IN","W021 IN","P017","L84_ReactionWheelTachometerArray",-10.39000f,1.34000f,4.39000f,"blue"),
            ("WHEEL_X_ELECTRICAL_CURRENT_FEEDBACK_OUT","W014 OUT","P018","L84_ReactionWheelX",-11.55000f,2.94000f,0.85000f,"blue"),
            ("WHEEL_X_ELECTRICAL_CURRENT_FEEDBACK_IN","W014 IN","P019","L84_ReactionWheelMotorCurrentMonitor",-8.93000f,1.34000f,4.39000f,"blue"),
            ("WHEEL_Z_ELECTRICAL_CURRENT_FEEDBACK_IN","W022 IN","P019","L84_ReactionWheelMotorCurrentMonitor",-9.14000f,1.34000f,4.39000f,"blue"),
            ("WHEEL_Y_REQUESTED_TORQUE_COMMAND_OUT","W015 OUT","P020","L84_AttitudeFlightController",-1.65000f,1.44000f,5.09000f,"mint"),
            ("WHEEL_S_REQUESTED_TORQUE_COMMAND_OUT","W023 OUT","P020","L84_AttitudeFlightController",-1.44000f,1.23000f,5.09000f,"mint"),
            ("WHEEL_Y_REQUESTED_TORQUE_COMMAND_IN","W015 IN","P021","L84_ReactionWheelMotorDriverRack",-11.08500f,1.34000f,4.39000f,"mint"),
            ("WHEEL_S_REQUESTED_TORQUE_COMMAND_IN","W023 IN","P021","L84_ReactionWheelMotorDriverRack",-11.08500f,1.13000f,4.39000f,"mint"),
            ("WHEEL_Y_ACTUAL_MOTOR_DRIVE_OUT","W016 OUT","P022","L84_ReactionWheelMotorDriverRack",-10.97000f,1.34000f,4.39000f,"mint"),
            ("WHEEL_S_ACTUAL_MOTOR_DRIVE_OUT","W024 OUT","P022","L84_ReactionWheelMotorDriverRack",-10.76000f,1.34000f,4.39000f,"mint"),
            ("WHEEL_Y_ACTUAL_MOTOR_DRIVE_IN","W016 IN","P023","L84_ReactionWheelY",-10.41000f,2.94000f,0.85000f,"mint"),
            ("WHEEL_Y_SHAFT_VELOCITY_FEEDBACK_OUT","W017 OUT","P024","L84_ReactionWheelY",-10.29500f,2.94000f,0.85000f,"blue"),
            ("WHEEL_Y_SHAFT_VELOCITY_FEEDBACK_IN","W017 IN","P025","L84_ReactionWheelTachometerArray",-10.06500f,1.34000f,4.39000f,"blue"),
            ("WHEEL_S_SHAFT_VELOCITY_FEEDBACK_IN","W025 IN","P025","L84_ReactionWheelTachometerArray",-10.06500f,1.13000f,4.39000f,"blue"),
            ("WHEEL_Y_ELECTRICAL_CURRENT_FEEDBACK_OUT","W018 OUT","P026","L84_ReactionWheelY",-10.18000f,2.94000f,0.85000f,"blue"),
            ("WHEEL_Y_ELECTRICAL_CURRENT_FEEDBACK_IN","W018 IN","P027","L84_ReactionWheelMotorCurrentMonitor",-8.81500f,1.34000f,4.39000f,"blue"),
            ("WHEEL_S_ELECTRICAL_CURRENT_FEEDBACK_IN","W026 IN","P027","L84_ReactionWheelMotorCurrentMonitor",-8.81500f,1.13000f,4.39000f,"blue"),
            ("WHEEL_Z_ACTUAL_MOTOR_DRIVE_IN","W020 IN","P028","L84_ReactionWheelZ",-9.03000f,2.94000f,0.85000f,"mint"),
            ("WHEEL_Z_SHAFT_VELOCITY_FEEDBACK_OUT","W021 OUT","P029","L84_ReactionWheelZ",-8.91500f,2.94000f,0.85000f,"blue"),
            ("WHEEL_Z_ELECTRICAL_CURRENT_FEEDBACK_OUT","W022 OUT","P030","L84_ReactionWheelZ",-8.80000f,2.94000f,0.85000f,"blue"),
            ("WHEEL_S_ACTUAL_MOTOR_DRIVE_IN","W024 IN","P031","L84_SkewBackupReactionWheel",-7.66000f,2.94000f,0.85000f,"mint"),
            ("WHEEL_S_SHAFT_VELOCITY_FEEDBACK_OUT","W025 OUT","P032","L84_SkewBackupReactionWheel",-7.54500f,2.94000f,0.85000f,"blue"),
            ("WHEEL_S_ELECTRICAL_CURRENT_FEEDBACK_OUT","W026 OUT","P033","L84_SkewBackupReactionWheel",-7.43000f,2.94000f,0.85000f,"blue"),
            ("WHEEL_CLUSTER_SPEED_LIMIT_FEEDBACK_OUT","W027 OUT","P034","L84_ReactionWheelTachometerArray",-9.95000f,1.34000f,4.39000f,"blue"),
            ("WHEEL_CLUSTER_SPEED_LIMIT_FEEDBACK_IN","W027 IN","P035","L84_AttitudeFlightController",-1.53500f,1.44000f,5.09000f,"blue"),
            ("ESTIMATOR_CONFIDENCE_GATE_IN","W076 IN","P035","L84_AttitudeFlightController",-1.11500f,1.44000f,5.09000f,"blue"),
            ("QUALIFIED_JITTER_FILTER_OUTPUT_IN","W084 IN","P035","L84_AttitudeFlightController",-1.53500f,1.02000f,5.09000f,"blue"),
            ("OPTICAL_LOCK_QUALIFICATION_FEEDBACK_IN","W117 IN","P035","L84_AttitudeFlightController",-1.53500f,1.86000f,5.09000f,"blue"),
            ("WHEEL_MOTOR_CURRENT_PROTECTION_FEEDBACK_OUT","W028 OUT","P036","L84_ReactionWheelMotorCurrentMonitor",-8.70000f,1.34000f,4.39000f,"blue"),
            ("WHEEL_MOTOR_CURRENT_PROTECTION_FEEDBACK_IN","W028 IN","P037","L84_AttitudeFlightController",-1.42000f,1.44000f,5.09000f,"blue"),
            ("MEASURED_POINTING_ERROR_MAGNITUDE_IN","W082 IN","P037","L84_AttitudeFlightController",-1.21000f,1.23000f,5.09000f,"blue"),
            ("EXTERNAL_DISTURBANCE_TORQUE_ESTIMATE_IN","W085 IN","P037","L84_AttitudeFlightController",-1.21000f,1.65000f,5.09000f,"blue"),
            ("WHEEL_THERMAL_HEALTH_FEEDBACK_OUT","W029 OUT","P038","L84_ReactionWheelThermalControlModule",-7.68000f,1.34000f,4.39000f,"mint"),
            ("WHEEL_THERMAL_HEALTH_FEEDBACK_IN","W029 IN","P039","L84_AttitudeFlightController",-1.88000f,1.54500f,5.09000f,"mint"),
            ("WHEEL_THERMAL_REGULATION_REQUEST_OUT","W030 OUT","P040","L84_AttitudeFlightController",-1.76500f,1.54500f,5.09000f,"mint"),
            ("OPTICAL_POINTING_STABILIZATION_ENABLE_OUT","W113 OUT","P040","L84_AttitudeFlightController",-2.18500f,1.54500f,5.09000f,"mint"),
            ("WHEEL_THERMAL_REGULATION_REQUEST_IN","W030 IN","P041","L84_ReactionWheelThermalControlModule",-7.56500f,1.34000f,4.39000f,"mint"),
            ("WHEEL_SATURATION_WARNING_OUT","W031 OUT","P042","L84_ReactionWheelSaturationAlarm",-11.45000f,4.04000f,-0.16000f,"mint"),
            ("WHEEL_SATURATION_WARNING_IN","W031 IN","P043","L84_AttitudeFlightController",-1.65000f,1.54500f,5.09000f,"mint"),
            ("WHEEL_DESATURATION_REQUEST_OUT","W032 OUT","P044","L84_ReactionWheelDesaturationSupervisor",-10.43000f,4.04000f,-0.16000f,"mint"),
            ("WHEEL_DESATURATION_REQUEST_IN","W032 IN","P045","L84_AttitudeFlightController",-1.53500f,1.54500f,5.09000f,"mint"),
            ("VALIDATED_COARSE_SUN_SAFE_TARGET_IN","W101 IN","P045","L84_AttitudeFlightController",-1.74500f,1.96500f,5.09000f,"mint"),
            ("X_MAGNETORQUER_DIPOLE_REQUEST_OUT","W033 OUT","P046","L84_MomentumDesaturationController",1.92000f,3.29000f,-4.71000f,"mint"),
            ("Z_MAGNETORQUER_DIPOLE_REQUEST_OUT","W039 OUT","P046","L84_MomentumDesaturationController",1.71000f,3.29000f,-4.71000f,"mint"),
            ("X_MAGNETORQUER_DIPOLE_REQUEST_IN","W033 IN","P047","L84_MagneticTorqueRodDriveController",-5.41500f,1.89000f,-4.47000f,"mint"),
            ("Z_MAGNETORQUER_DIPOLE_REQUEST_IN","W039 IN","P047","L84_MagneticTorqueRodDriveController",-5.41500f,1.68000f,-4.47000f,"mint"),
            ("X_TORQUE_ROD_DRIVE_CURRENT_OUT","W034 OUT","P048","L84_MagneticTorqueRodDriveController",-5.30000f,1.89000f,-4.47000f,"mint"),
            ("Z_TORQUE_ROD_DRIVE_CURRENT_OUT","W040 OUT","P048","L84_MagneticTorqueRodDriveController",-5.09000f,1.68000f,-4.47000f,"mint"),
            ("X_TORQUE_ROD_DRIVE_CURRENT_IN","W034 IN","P049","L84_MagneticTorqueRodX",-3.03000f,1.79000f,-4.41000f,"mint"),
            ("X_ACTUAL_MAGNETIC_DIPOLE_RESPONSE_OUT","W035 OUT","P050","L84_MagneticTorqueRodX",-2.91500f,1.79000f,-4.41000f,"blue"),
            ("X_ACTUAL_MAGNETIC_DIPOLE_RESPONSE_IN","W035 IN","P051","L84_MomentumDesaturationController",2.03500f,3.29000f,-4.71000f,"blue"),
            ("Z_ACTUAL_MAGNETIC_DIPOLE_RESPONSE_IN","W041 IN","P051","L84_MomentumDesaturationController",2.03500f,3.08000f,-4.71000f,"blue"),
            ("Y_MAGNETORQUER_DIPOLE_REQUEST_OUT","W036 OUT","P052","L84_MomentumDesaturationController",2.15000f,3.29000f,-4.71000f,"mint"),
            ("Y_MAGNETORQUER_DIPOLE_REQUEST_IN","W036 IN","P053","L84_MagneticTorqueRodDriveController",-5.18500f,1.89000f,-4.47000f,"mint"),
            ("Y_TORQUE_ROD_DRIVE_CURRENT_OUT","W037 OUT","P054","L84_MagneticTorqueRodDriveController",-5.07000f,1.89000f,-4.47000f,"mint"),
            ("Y_TORQUE_ROD_DRIVE_CURRENT_IN","W037 IN","P055","L84_MagneticTorqueRodY",-0.23000f,1.79000f,-4.41000f,"mint"),
            ("Y_ACTUAL_MAGNETIC_DIPOLE_RESPONSE_OUT","W038 OUT","P056","L84_MagneticTorqueRodY",-0.11500f,1.79000f,-4.41000f,"blue"),
            ("Y_ACTUAL_MAGNETIC_DIPOLE_RESPONSE_IN","W038 IN","P057","L84_MomentumDesaturationController",2.26500f,3.29000f,-4.71000f,"blue"),
            ("CALIBRATED_LOCAL_MAGNETIC_FIELD_VECTOR_IN","W042 IN","P057","L84_MomentumDesaturationController",2.26500f,3.08000f,-4.71000f,"blue"),
            ("Z_TORQUE_ROD_DRIVE_CURRENT_IN","W040 IN","P058","L84_MagneticTorqueRodZ",2.57000f,1.79000f,-4.41000f,"mint"),
            ("Z_ACTUAL_MAGNETIC_DIPOLE_RESPONSE_OUT","W041 OUT","P059","L84_MagneticTorqueRodZ",2.68500f,1.79000f,-4.41000f,"blue"),
            ("CALIBRATED_LOCAL_MAGNETIC_FIELD_VECTOR_OUT","W042 OUT","P060","L84_TriaxialMagnetometer",-5.43000f,3.39000f,-4.45000f,"blue"),
            ("MAGNETOMETER_TRI_AXIS_FIELD_MEASUREMENT_OUT","W065 OUT","P060","L84_TriaxialMagnetometer",-5.43000f,3.18000f,-4.45000f,"blue"),
            ("ORBITAL_FIELD_ENVIRONMENT_REFERENCE_OUT","W043 OUT","P061","L84_OrbitalMagneticFieldEmulator",-2.23000f,3.39000f,-4.76000f,"mint"),
            ("ORBITAL_FIELD_ENVIRONMENT_REFERENCE_IN","W043 IN","P062","L84_TriaxialMagnetometer",-5.31500f,3.39000f,-4.45000f,"mint"),
            ("MAGNETIC_VECTOR_MEASUREMENT_TO_ESTIMATOR_OUT","W044 OUT","P063","L84_TriaxialMagnetometer",-5.20000f,3.39000f,-4.45000f,"blue"),
            ("ADCS_ABSOLUTE_ATTITUDE_IN","W097 IN","P064","L84_AttitudeStateEstimator",-5.56500f,1.44000f,5.09000f,"amber"),
            ("MAGNETIC_VECTOR_MEASUREMENT_TO_ESTIMATOR_IN","W044 IN","P064","L84_AttitudeStateEstimator",-5.56500f,1.23000f,5.09000f,"blue"),
            ("GYRO_MEASUREMENT_TIMESTAMP_VALIDITY_IN","W056 IN","P064","L84_AttitudeStateEstimator",-5.77500f,1.23000f,5.09000f,"blue"),
            ("EARTH_HORIZON_DIRECTION_MEASUREMENT_IN","W064 IN","P064","L84_AttitudeStateEstimator",-5.35500f,1.23000f,5.09000f,"blue"),
            ("ATTITUDE_AND_FIELD_DESATURATION_COORDINATION_OUT","W045 OUT","P065","L84_AttitudeStateEstimator",-5.45000f,1.44000f,5.09000f,"mint"),
            ("ATTITUDE_AND_FIELD_DESATURATION_COORDINATION_IN","W045 IN","P066","L84_MomentumDesaturationController",2.38000f,3.29000f,-4.71000f,"mint"),
            ("ACCUMULATED_WHEEL_MOMENTUM_DUMP_STATUS_OUT","W046 OUT","P067","L84_MomentumDesaturationController",1.92000f,3.39500f,-4.71000f,"mint"),
            ("DESATURATION_STATE_SCADA_FEEDBACK_OUT","W052 OUT","P067","L84_MomentumDesaturationController",1.92000f,3.60500f,-4.71000f,"mint"),
            ("MAGNETIC_MOMENTUM_DUMPING_CERTIFICATE_OUT","W136 OUT","P067","L84_MomentumDesaturationController",1.71000f,3.60500f,-4.71000f,"mint"),
            ("ACCUMULATED_WHEEL_MOMENTUM_DUMP_STATUS_IN","W046 IN","P068","L84_ReactionWheelDesaturationSupervisor",-10.31500f,4.04000f,-0.16000f,"mint"),
            ("PROTECTED_MOMENTUM_UNLOADING_AUTHORIZATION_OUT","W047 OUT","P069","L84_ReactionWheelDesaturationSupervisor",-10.20000f,4.04000f,-0.16000f,"mint"),
            ("PROTECTED_MOMENTUM_UNLOADING_AUTHORIZATION_IN","W047 IN","P070","L84_MagneticTorqueRodDriveController",-5.53000f,1.99500f,-4.47000f,"mint"),
            ("INDEPENDENT_TORQUER_CURRENT_INHIBIT_IN","W105 IN","P070","L84_MagneticTorqueRodDriveController",-5.74000f,1.99500f,-4.47000f,"mint"),
            ("MAGNETIC_TORQUER_SAFETY_INTERLOCK_OUT","W048 OUT","P071","L84_IndependentMotionSafetyStation",7.48500f,1.44000f,5.09000f,"mint"),
            ("INDEPENDENT_WHEEL_DRIVER_INHIBIT_OUT","W104 OUT","P071","L84_IndependentMotionSafetyStation",7.48500f,1.23000f,5.09000f,"mint"),
            ("INDEPENDENT_MOTION_PROTECTION_STATUS_OUT","W142 OUT","P071","L84_IndependentMotionSafetyStation",7.27500f,1.23000f,5.09000f,"mint"),
            ("MAGNETIC_TORQUER_SAFETY_INTERLOCK_IN","W048 IN","P072","L84_MagneticTorqueRodDriveController",-5.41500f,1.99500f,-4.47000f,"mint"),
            ("PLC_TORQUE_MODE_SELECTION_OUT","W049 OUT","P073","L84_IntegratedADCSPLCRack",4.88500f,1.44000f,5.09000f,"mint"),
            ("CERTIFICATION_BEACON_COMMAND_OUT","W138 OUT","P073","L84_IntegratedADCSPLCRack",4.88500f,1.23000f,5.09000f,"mint"),
            ("PLC_TORQUE_MODE_SELECTION_IN","W049 IN","P074","L84_MomentumDesaturationController",2.03500f,3.39500f,-4.71000f,"mint"),
            ("MAGNETORQUER_AMPLIFIER_READY_FEEDBACK_OUT","W050 OUT","P075","L84_MagneticTorqueRodDriveController",-5.30000f,1.99500f,-4.47000f,"mint"),
            ("MAGNETORQUER_AMPLIFIER_READY_FEEDBACK_IN","W050 IN","P076","L84_IntegratedADCSPLCRack",5.00000f,1.44000f,5.09000f,"mint"),
            ("GYROSCOPE_CALIBRATION_REPORT_IN","W070 IN","P076","L84_IntegratedADCSPLCRack",5.21000f,1.23000f,5.09000f,"mint"),
            ("SECONDARY_TRACKER_OPERATIONAL_HEALTH_IN","W072 IN","P076","L84_IntegratedADCSPLCRack",4.58000f,1.44000f,5.09000f,"mint"),
            ("HORIZON_SENSOR_VALIDITY_IN","W074 IN","P076","L84_IntegratedADCSPLCRack",5.42000f,1.44000f,5.09000f,"mint"),
            ("THREE_AXIS_CONTROL_STABILITY_QUALIFICATION_IN","W133 IN","P076","L84_IntegratedADCSPLCRack",5.00000f,1.02000f,5.09000f,"mint"),
            ("WHEEL_SPEED_MARGIN_CERTIFICATE_IN","W135 IN","P076","L84_IntegratedADCSPLCRack",5.00000f,1.86000f,5.09000f,"mint"),
            ("MAGNETOMETER_MEASUREMENT_HEALTH_OUT","W051 OUT","P077","L84_TriaxialMagnetometer",-5.08500f,3.39000f,-4.45000f,"mint"),
            ("MAGNETOMETER_MEASUREMENT_HEALTH_IN","W051 IN","P078","L84_IntegratedADCSPLCRack",5.11500f,1.44000f,5.09000f,"mint"),
            ("PRIMARY_TRACKER_OPERATIONAL_HEALTH_IN","W071 IN","P078","L84_IntegratedADCSPLCRack",4.69500f,1.23000f,5.09000f,"mint"),
            ("SUN_SENSOR_VALIDITY_IN","W073 IN","P078","L84_IntegratedADCSPLCRack",5.53500f,1.23000f,5.09000f,"mint"),
            ("VERIFIED_OPTICAL_DOWNLINK_PACKET_DELIVERY_IN","W124 IN","P078","L84_IntegratedADCSPLCRack",5.53500f,1.65000f,5.09000f,"mint"),
            ("ESTIMATOR_READINESS_AND_CONFIDENCE_IN","W134 IN","P078","L84_IntegratedADCSPLCRack",5.32500f,1.02000f,5.09000f,"mint"),
            ("MAGNETIC_MOMENTUM_DUMPING_CERTIFICATE_IN","W136 IN","P078","L84_IntegratedADCSPLCRack",5.32500f,1.86000f,5.09000f,"mint"),
            ("DESATURATION_STATE_SCADA_FEEDBACK_IN","W052 IN","P079","L84_HolographicAttitudeControlSCADA",-0.11500f,7.59000f,-4.91000f,"mint"),
            ("ESTIMATOR_UNCERTAINTY_VISUALIZATION_IN","W092 IN","P079","L84_HolographicAttitudeControlSCADA",-0.11500f,7.38000f,-4.91000f,"mint"),
            ("LATCHED_ATTITUDE_SAFE_MODE_SCADA_ALARM_IN","W110 IN","P079","L84_HolographicAttitudeControlSCADA",0.09500f,7.38000f,-4.91000f,"mint"),
            ("MOVING_GROUND_STATION_TARGET_TELEMETRY_IN","W119 IN","P079","L84_HolographicAttitudeControlSCADA",0.09500f,7.80000f,-4.91000f,"mint"),
            ("SIX_OBJECTIVE_CERTIFICATE_STATE_IN","W139 IN","P079","L84_HolographicAttitudeControlSCADA",-0.32500f,7.38000f,-4.91000f,"mint"),
            ("RAW_GYROSCOPE_ANGULAR_RATE_MEASUREMENT_OUT","W053 OUT","P080","L84_PrecisionGyroscopeIMU",7.28500f,2.09000f,4.24000f,"blue"),
            ("RAW_GYROSCOPE_ANGULAR_RATE_MEASUREMENT_IN","W053 IN","P081","L84_AttitudeStateEstimator",-5.33500f,1.44000f,5.09000f,"blue"),
            ("COARSE_SUN_DIRECTION_MEASUREMENT_IN","W063 IN","P081","L84_AttitudeStateEstimator",-5.12500f,1.23000f,5.09000f,"blue"),
            ("MAGNETOMETER_TRI_AXIS_FIELD_MEASUREMENT_IN","W065 IN","P081","L84_AttitudeStateEstimator",-5.12500f,1.65000f,5.09000f,"blue"),
            ("GYROSCOPE_ZERO_RATE_CALIBRATION_COMMAND_OUT","W054 OUT","P082","L84_GyroscopeCalibrationStation",8.47000f,2.09000f,4.24000f,"mint"),
            ("GYROSCOPE_CALIBRATION_REPORT_OUT","W070 OUT","P082","L84_GyroscopeCalibrationStation",8.26000f,2.09000f,4.24000f,"mint"),
            ("GYROSCOPE_ZERO_RATE_CALIBRATION_COMMAND_IN","W054 IN","P083","L84_PrecisionGyroscopeIMU",7.40000f,2.09000f,4.24000f,"mint"),
            ("GYRO_TEMPERATURE_BIAS_DISTURBANCE_INPUT_OUT","W055 OUT","P084","L84_GyroBiasThermalFaultEmulator",9.77000f,2.09000f,4.24000f,"mint"),
            ("GYRO_TEMPERATURE_BIAS_DISTURBANCE_INPUT_IN","W055 IN","P085","L84_PrecisionGyroscopeIMU",7.51500f,2.09000f,4.24000f,"mint"),
            ("GYRO_MEASUREMENT_TIMESTAMP_VALIDITY_OUT","W056 OUT","P086","L84_PrecisionGyroscopeIMU",7.63000f,2.09000f,4.24000f,"blue"),
            ("PRIMARY_TRACKER_STAR_FRAME_PATTERN_OUT","W057 OUT","P087","L84_PrimaryStarTracker",8.98500f,3.99000f,0.69000f,"mint"),
            ("PRIMARY_TRACKER_DIAGNOSTIC_ATTITUDE_TELEMETRY_OUT","W059 OUT","P087","L84_PrimaryStarTracker",8.98500f,3.78000f,0.69000f,"mint"),
            ("PRIMARY_TRACKER_OPERATIONAL_HEALTH_OUT","W071 OUT","P087","L84_PrimaryStarTracker",8.98500f,4.20000f,0.69000f,"mint"),
            ("STAR_TRACKER_SAMPLE_FRESHNESS_DIAGNOSTIC_OUT","W096 OUT","P087","L84_PrimaryStarTracker",8.77500f,3.78000f,0.69000f,"mint"),
            ("PRIMARY_TRACKER_STAR_FRAME_PATTERN_IN","W057 IN","P088","L84_StarCatalogPatternMatcher",8.92000f,2.39000f,0.79000f,"mint"),
            ("CATALOG_MATCH_ACCEPTANCE_OUT","W058 OUT","P089","L84_StarCatalogPatternMatcher",9.03500f,2.39000f,0.79000f,"mint"),
            ("STAR_CATALOG_VALIDATION_SUMMARY_OUT","W069 OUT","P089","L84_StarCatalogPatternMatcher",9.03500f,2.18000f,0.79000f,"mint"),
            ("CATALOG_MATCH_ACCEPTANCE_IN","W058 IN","P090","L84_PrimaryStarTracker",9.10000f,3.99000f,0.69000f,"mint"),
            ("PRIMARY_TRACKER_DIAGNOSTIC_ATTITUDE_TELEMETRY_IN","W059 IN","P091","L84_AttitudeStateEstimator",-5.22000f,1.44000f,5.09000f,"mint"),
            ("INERTIAL_CLOCK_SYNCHRONIZATION_IN","W066 IN","P091","L84_AttitudeStateEstimator",-5.01000f,1.44000f,5.09000f,"mint"),
            ("PRIMARY_TRACKER_BAFFLE_STATE_OUT","W060 OUT","P092","L84_StarTrackerOpticalBaffle",10.17000f,3.91000f,0.69000f,"mint"),
            ("PRIMARY_TRACKER_BAFFLE_STATE_IN","W060 IN","P093","L84_PrimaryStarTracker",9.21500f,3.99000f,0.69000f,"mint"),
            ("SECONDARY_DIAGNOSTIC_TRACKER_STAR_PATTERN_OUT","W061 OUT","P094","L84_SecondaryDiagnosticStarTracker",11.43500f,3.99000f,0.69000f,"mint"),
            ("SECONDARY_TRACKER_DIAGNOSTIC_CONFIDENCE_OUT","W062 OUT","P094","L84_SecondaryDiagnosticStarTracker",11.64500f,3.99000f,0.69000f,"mint"),
            ("SECONDARY_TRACKER_OPERATIONAL_HEALTH_OUT","W072 OUT","P094","L84_SecondaryDiagnosticStarTracker",11.43500f,3.78000f,0.69000f,"mint"),
            ("SECONDARY_DIAGNOSTIC_TRACKER_STAR_PATTERN_IN","W061 IN","P095","L84_StarCatalogPatternMatcher",9.15000f,2.39000f,0.79000f,"mint"),
            ("SECONDARY_TRACKER_DIAGNOSTIC_CONFIDENCE_IN","W062 IN","P096","L84_AttitudeStateEstimator",-5.68000f,1.54500f,5.09000f,"mint"),
            ("COARSE_SUN_DIRECTION_MEASUREMENT_OUT","W063 OUT","P097","L84_CoarseSunSensorCluster",10.22000f,2.39000f,0.79000f,"blue"),
            ("EARTH_HORIZON_DIRECTION_MEASUREMENT_OUT","W064 OUT","P098","L84_EarthHorizonSensor",11.42000f,2.39000f,0.79000f,"blue"),
            ("INERTIAL_CLOCK_SYNCHRONIZATION_OUT","W066 OUT","P099","L84_InertialReferenceClock",3.32000f,1.44000f,5.09000f,"mint"),
            ("ATTITUDE_CONTROLLER_PRECISE_TIMING_REFERENCE_OUT","W090 OUT","P099","L84_InertialReferenceClock",3.11000f,1.44000f,5.09000f,"mint"),
            ("ESTIMATOR_COVARIANCE_UNCERTAINTY_OUTPUT_OUT","W067 OUT","P100","L84_AttitudeStateEstimator",-5.56500f,1.54500f,5.09000f,"mint"),
            ("COMPLEMENTARY_FUSION_STATUS_OUT","W068 OUT","P100","L84_AttitudeStateEstimator",-5.56500f,1.75500f,5.09000f,"mint"),
            ("ATTITUDE_ESTIMATOR_STATUS_SUMMARY_NON_AUTHORITATIVE_OUT","W109 OUT","P100","L84_AttitudeStateEstimator",-5.77500f,1.75500f,5.09000f,"mint"),
            ("ESTIMATOR_READINESS_AND_CONFIDENCE_OUT","W134 OUT","P100","L84_AttitudeStateEstimator",-5.35500f,1.75500f,5.09000f,"mint"),
            ("ESTIMATOR_COVARIANCE_UNCERTAINTY_OUTPUT_IN","W067 IN","P101","L84_EstimatorUncertaintyDisplay",-3.18000f,1.44000f,5.09000f,"mint"),
            ("COMPLEMENTARY_FUSION_STATUS_IN","W068 IN","P102","L84_ComplementaryFusionDisplay",-4.41000f,1.44000f,5.09000f,"mint"),
            ("STAR_CATALOG_VALIDATION_SUMMARY_IN","W069 IN","P103","L84_ComplementaryFusionDisplay",-4.29500f,1.44000f,5.09000f,"mint"),
            ("SUN_SENSOR_VALIDITY_OUT","W073 OUT","P104","L84_CoarseSunSensorCluster",10.33500f,2.39000f,0.79000f,"mint"),
            ("HORIZON_SENSOR_VALIDITY_OUT","W074 OUT","P105","L84_EarthHorizonSensor",11.53500f,2.39000f,0.79000f,"mint"),
            ("ESTIMATED_QUATERNION_TO_FLIGHT_CONTROLLER_OUT","W075 OUT","P106","L84_AttitudeStateEstimator",-5.45000f,1.54500f,5.09000f,"mint"),
            ("ESTIMATED_QUATERNION_TO_FLIGHT_CONTROLLER_IN","W075 IN","P107","L84_AttitudeFlightController",-1.42000f,1.54500f,5.09000f,"mint"),
            ("ESTIMATOR_CONFIDENCE_GATE_OUT","W076 OUT","P108","L84_EstimatorUncertaintyDisplay",-3.06500f,1.44000f,5.09000f,"blue"),
            ("SLEW_TRAJECTORY_COMMAND_OUT","W077 OUT","P109","L84_SlewTrajectoryPlanner",0.72000f,1.44000f,5.09000f,"mint"),
            ("SLEW_TRAJECTORY_COMMAND_IN","W077 IN","P110","L84_AttitudeFlightController",-1.88000f,1.65000f,5.09000f,"mint"),
            ("OPTICAL_TARGET_SLEW_REQUEST_IN","W112 IN","P110","L84_AttitudeFlightController",-2.09000f,1.86000f,5.09000f,"mint"),
            ("DESIRED_SPACECRAFT_BODY_TORQUE_IN","W078 IN","P111","L84_ThreeAxisTorqueAllocationComputer",-0.58000f,1.44000f,5.09000f,"mint"),
            ("FOUR_WHEEL_TORQUE_ALLOCATION_OUT","W079 OUT","P112","L84_ThreeAxisTorqueAllocationComputer",-0.46500f,1.44000f,5.09000f,"mint"),
            ("REQUESTED_TARGET_QUATERNION_OUT","W080 OUT","P113","L84_AttitudeFlightController",-1.76500f,1.65000f,5.09000f,"mint"),
            ("REQUESTED_TARGET_QUATERNION_IN","W080 IN","P114","L84_SlewTrajectoryPlanner",0.83500f,1.44000f,5.09000f,"mint"),
            ("PREDICTED_SLEW_ENDPOINT_OUT","W081 OUT","P115","L84_SlewTrajectoryPlanner",0.95000f,1.44000f,5.09000f,"mint"),
            ("PREDICTED_SLEW_ENDPOINT_IN","W081 IN","P116","L84_PrecisionPointingErrorAnalyzer",2.02000f,1.44000f,5.09000f,"mint"),
            ("MEASURED_POINTING_ERROR_MAGNITUDE_OUT","W082 OUT","P117","L84_PrecisionPointingErrorAnalyzer",2.13500f,1.44000f,5.09000f,"blue"),
            ("MEASURED_STRUCTURAL_VIBRATION_RESPONSE_OUT","W083 OUT","P118","L84_StructuralVibrationSensor",5.22000f,3.54000f,-4.61000f,"blue"),
            ("MEASURED_STRUCTURAL_VIBRATION_RESPONSE_IN","W083 IN","P119","L84_ActivePointingVibrationFilter",6.42000f,3.54000f,-4.61000f,"blue"),
            ("QUALIFIED_JITTER_FILTER_OUTPUT_OUT","W084 OUT","P120","L84_ActivePointingVibrationFilter",6.53500f,3.54000f,-4.61000f,"blue"),
            ("EXTERNAL_DISTURBANCE_TORQUE_ESTIMATE_OUT","W085 OUT","P121","L84_ExternalDisturbanceTorqueInjector",-4.33000f,3.39000f,-4.41000f,"blue"),
            ("GRAVITY_GRADIENT_TORQUE_CONTRIBUTION_OUT","W086 OUT","P122","L84_GravityGradientTorqueEmulator",-3.23000f,3.54000f,-4.61000f,"mint"),
            ("GRAVITY_GRADIENT_TORQUE_CONTRIBUTION_IN","W086 IN","P123","L84_ExternalDisturbanceTorqueInjector",-4.21500f,3.39000f,-4.41000f,"mint"),
            ("SOLAR_PRESSURE_TORQUE_CONTRIBUTION_IN","W088 IN","P123","L84_ExternalDisturbanceTorqueInjector",-4.21500f,3.18000f,-4.41000f,"mint"),
            ("ORBITAL_DRAG_TORQUE_CONTRIBUTION_OUT","W087 OUT","P124","L84_OrbitalDragTorqueEmulator",-2.13000f,3.54000f,-4.61000f,"mint"),
            ("ORBITAL_DRAG_TORQUE_CONTRIBUTION_IN","W087 IN","P125","L84_ExternalDisturbanceTorqueInjector",-4.10000f,3.39000f,-4.41000f,"mint"),
            ("SOLAR_PRESSURE_TORQUE_CONTRIBUTION_OUT","W088 OUT","P126","L84_SolarRadiationPressureTorqueModel",-0.98000f,3.54000f,-4.61000f,"mint"),
            ("SOLAR_WING_FLEXIBLE_MODE_AMPLITUDE_OUT","W089 OUT","P127","L84_SolarWingFlexibleModeEmulator",4.02000f,3.54000f,-4.61000f,"blue"),
            ("SOLAR_WING_FLEXIBLE_MODE_AMPLITUDE_IN","W089 IN","P128","L84_StructuralVibrationSensor",5.33500f,3.54000f,-4.61000f,"blue"),
            ("ATTITUDE_CONTROLLER_PRECISE_TIMING_REFERENCE_IN","W090 IN","P129","L84_AttitudeFlightController",-1.65000f,1.65000f,5.09000f,"mint"),
            ("QUATERNION_ORIENTATION_VISUALIZATION_OUT","W091 OUT","P130","L84_HolographicQuaternionAttitudeSphere",3.52000f,7.24000f,-4.73000f,"mint"),
            ("QUATERNION_ORIENTATION_VISUALIZATION_IN","W091 IN","P131","L84_HolographicAttitudeControlSCADA",0.00000f,7.59000f,-4.91000f,"mint"),
            ("TORQUE_ALLOCATION_TELEMETRY_IN","W094 IN","P131","L84_HolographicAttitudeControlSCADA",-0.42000f,7.59000f,-4.91000f,"mint"),
            ("DOWNLINK_RECEIVE_QUALITY_TELEMETRY_IN","W118 IN","P131","L84_HolographicAttitudeControlSCADA",0.42000f,7.59000f,-4.91000f,"mint"),
            ("ADCS_PLC_MODE_AND_SEQUENCING_TELEMETRY_IN","W131 IN","P131","L84_HolographicAttitudeControlSCADA",0.00000f,7.17000f,-4.91000f,"mint"),
            ("TIMESTAMPED_FLIGHT_RECORDER_EVENT_TELEMETRY_IN","W140 IN","P131","L84_HolographicAttitudeControlSCADA",0.00000f,8.01000f,-4.91000f,"mint"),
            ("ESTIMATOR_UNCERTAINTY_VISUALIZATION_OUT","W092 OUT","P132","L84_EstimatorUncertaintyDisplay",-2.95000f,1.44000f,5.09000f,"mint"),
            ("POINTING_ACCURACY_TELEMETRY_OUT","W093 OUT","P133","L84_PrecisionPointingErrorAnalyzer",2.25000f,1.44000f,5.09000f,"blue"),
            ("POINTING_ACCURACY_TELEMETRY_IN","W093 IN","P134","L84_HolographicAttitudeControlSCADA",0.11500f,7.59000f,-4.91000f,"blue"),
            ("TORQUE_ALLOCATION_TELEMETRY_OUT","W094 OUT","P135","L84_ThreeAxisTorqueAllocationComputer",-0.35000f,1.44000f,5.09000f,"mint"),
            ("INDEPENDENT_ATTITUDE_MOTION_SAFETY_PERMISSION_OUT","W095 OUT","P136","L84_IndependentMotionSafetyStation",7.60000f,1.44000f,5.09000f,"mint"),
            ("INDEPENDENT_TORQUER_CURRENT_INHIBIT_OUT","W105 OUT","P136","L84_IndependentMotionSafetyStation",7.60000f,1.65000f,5.09000f,"mint"),
            ("INDEPENDENT_ATTITUDE_MOTION_SAFETY_PERMISSION_IN","W095 IN","P137","L84_AttitudeSafeModeSupervisor",-6.63000f,1.44000f,5.09000f,"mint"),
            ("WHEEL_SATURATION_PROTECTION_THRESHOLD_IN","W108 IN","P137","L84_AttitudeSafeModeSupervisor",-6.84000f,1.44000f,5.09000f,"mint"),
            ("STAR_TRACKER_SAMPLE_FRESHNESS_DIAGNOSTIC_IN","W096 IN","P138","L84_CriticalAttitudeFeedbackStation",10.07000f,1.43000f,5.09000f,"mint"),
            ("STAR_TRACKER_QUATERNION_OUT","W097 OUT","P139","L84_PrimaryStarTracker",9.33000f,3.99000f,0.69000f,"amber"),
            ("CRITICAL_ABSOLUTE_ATTITUDE_FEEDBACK_QUALIFICATION_OUT","W098 OUT","P140","L84_CriticalAttitudeFeedbackStation",10.18500f,1.43000f,5.09000f,"mint"),
            ("CRITICAL_ABSOLUTE_ATTITUDE_FEEDBACK_QUALIFICATION_IN","W098 IN","P141","L84_AttitudeSafeModeSupervisor",-6.51500f,1.44000f,5.09000f,"mint"),
            ("ATTITUDE_SAFETY_MODE_AUTHORIZATION_OUT","W099 OUT","P142","L84_AttitudeSafeModeSupervisor",-6.40000f,1.44000f,5.09000f,"mint"),
            ("ATTITUDE_SAFE_MODE_AND_ALARM_STATUS_OUT","W137 OUT","P142","L84_AttitudeSafeModeSupervisor",-6.40000f,1.23000f,5.09000f,"mint"),
            ("ATTITUDE_SAFETY_MODE_AUTHORIZATION_IN","W099 IN","P143","L84_AttitudeFlightController",-1.53500f,1.65000f,5.09000f,"mint"),
            ("SUN_SAFE_ACQUISITION_REQUEST_OUT","W100 OUT","P144","L84_AttitudeSafeModeSupervisor",-6.28500f,1.44000f,5.09000f,"mint"),
            ("SUN_SAFE_ACQUISITION_REQUEST_IN","W100 IN","P145","L84_SunSafeAttitudeController",-5.65000f,3.44000f,-4.41000f,"mint"),
            ("VALIDATED_COARSE_SUN_SAFE_TARGET_OUT","W101 OUT","P146","L84_SunSafeAttitudeController",-5.53500f,3.44000f,-4.41000f,"mint"),
            ("SUN_SAFE_STARBOARD_ARRAY_POINTING_REQUEST_OUT","W129 OUT","P146","L84_SunSafeAttitudeController",-5.53500f,3.65000f,-4.41000f,"mint"),
            ("ABSOLUTE_FEEDBACK_LOSS_CRITICAL_EVENT_OUT","W102 OUT","P147","L84_CriticalAttitudeFeedbackStation",10.30000f,1.43000f,5.09000f,"mint"),
            ("ABSOLUTE_FEEDBACK_LOSS_CRITICAL_EVENT_IN","W102 IN","P148","L84_IntegratedADCSPLCRack",5.23000f,1.44000f,5.09000f,"mint"),
            ("INDEPENDENT_MOTION_PROTECTION_STATUS_IN","W142 IN","P148","L84_IntegratedADCSPLCRack",5.65000f,1.44000f,5.09000f,"mint"),
            ("ADCS_PLC_PROTECTED_MOTION_RESET_REQUEST_OUT","W103 OUT","P149","L84_IntegratedADCSPLCRack",4.77000f,1.54500f,5.09000f,"mint"),
            ("ADCS_PLC_PROTECTED_MOTION_RESET_REQUEST_IN","W103 IN","P150","L84_IndependentMotionSafetyStation",7.71500f,1.44000f,5.09000f,"mint"),
            ("INDEPENDENT_WHEEL_DRIVER_INHIBIT_IN","W104 IN","P151","L84_ReactionWheelMotorDriverRack",-11.43000f,1.44500f,4.39000f,"mint"),
            ("EMERGENCY_STOP_CONTACT_STATE_OUT","W106 OUT","P152","L84_EmergencyAttitudeStopStation",10.92000f,1.31000f,5.24000f,"mint"),
            ("EMERGENCY_STOP_CONTACT_STATE_IN","W106 IN","P153","L84_IndependentMotionSafetyStation",7.83000f,1.44000f,5.09000f,"mint"),
            ("EMERGENCY_STOP_RESET_READINESS_OUT","W107 OUT","P154","L84_IndependentMotionSafetyStation",7.37000f,1.54500f,5.09000f,"mint"),
            ("EMERGENCY_STOP_RESET_READINESS_IN","W107 IN","P155","L84_EmergencyAttitudeStopStation",11.03500f,1.31000f,5.24000f,"mint"),
            ("WHEEL_SATURATION_PROTECTION_THRESHOLD_OUT","W108 OUT","P156","L84_ReactionWheelSaturationAlarm",-11.33500f,4.04000f,-0.16000f,"mint"),
            ("ATTITUDE_ESTIMATOR_STATUS_SUMMARY_NON_AUTHORITATIVE_IN","W109 IN","P157","L84_CriticalAttitudeFeedbackStation",10.41500f,1.43000f,5.09000f,"mint"),
            ("LATCHED_ATTITUDE_SAFE_MODE_SCADA_ALARM_OUT","W110 OUT","P158","L84_AttitudeSafeModeSupervisor",-6.17000f,1.44000f,5.09000f,"mint"),
            ("GROUND_STATION_PREDICTED_LINE_OF_SIGHT_OUT","W111 OUT","P159","L84_GroundStationTargetProjector",8.87000f,3.54000f,-4.61000f,"mint"),
            ("GROUND_STATION_PREDICTED_LINE_OF_SIGHT_IN","W111 IN","P160","L84_SlewTrajectoryPlanner",1.06500f,1.44000f,5.09000f,"mint"),
            ("OPTICAL_TARGET_SLEW_REQUEST_OUT","W112 OUT","P161","L84_SlewTrajectoryPlanner",1.18000f,1.44000f,5.09000f,"mint"),
            ("OPTICAL_POINTING_STABILIZATION_ENABLE_IN","W113 IN","P162","L84_OpticalLineOfSightSensor",7.67000f,3.54000f,-4.61000f,"mint"),
            ("OPTICAL_LINE_OF_SIGHT_MEASUREMENT_OUT","W114 OUT","P163","L84_OpticalLineOfSightSensor",7.78500f,3.54000f,-4.61000f,"blue"),
            ("OPTICAL_LINE_OF_SIGHT_MEASUREMENT_IN","W114 IN","P164","L84_OpticalLinkAcquisitionFixture",10.12000f,3.54000f,-4.61000f,"blue"),
            ("MISSION_PAYLOAD_BORESIGHT_REFERENCE_OUT","W115 OUT","P165","L84_MissionPayloadBoresightReference",3.27000f,5.39000f,-1.01000f,"mint"),
            ("MISSION_PAYLOAD_BORESIGHT_REFERENCE_IN","W115 IN","P166","L84_OpticalLineOfSightSensor",7.90000f,3.54000f,-4.61000f,"mint"),
            ("ACQUIRED_OPTICAL_SIGNAL_QUALITY_OUT","W116 OUT","P167","L84_OpticalLinkAcquisitionFixture",10.23500f,3.54000f,-4.61000f,"blue"),
            ("ACQUIRED_OPTICAL_SIGNAL_QUALITY_IN","W116 IN","P168","L84_OpticalSignalQualityTelemetry",11.32000f,3.54000f,-4.61000f,"blue"),
            ("OPTICAL_LOCK_QUALIFICATION_FEEDBACK_OUT","W117 OUT","P169","L84_OpticalSignalQualityTelemetry",11.43500f,3.54000f,-4.61000f,"blue"),
            ("DOWNLINK_RECEIVE_QUALITY_TELEMETRY_OUT","W118 OUT","P170","L84_OpticalSignalQualityTelemetry",11.55000f,3.54000f,-4.61000f,"mint"),
            ("VERIFIED_OPTICAL_DOWNLINK_PACKET_DELIVERY_OUT","W124 OUT","P170","L84_OpticalSignalQualityTelemetry",11.55000f,3.33000f,-4.61000f,"mint"),
            ("OPTICAL_COMMUNICATION_QUALIFICATION_LOG_OUT","W130 OUT","P170","L84_OpticalSignalQualityTelemetry",11.55000f,3.75000f,-4.61000f,"mint"),
            ("MOVING_GROUND_STATION_TARGET_TELEMETRY_OUT","W119 OUT","P171","L84_GroundStationTargetProjector",8.98500f,3.54000f,-4.61000f,"mint"),
            ("OPTICAL_TRANSMITTER_ACQUISITION_PERMIT_OUT","W120 OUT","P172","L84_OpticalLinkAcquisitionFixture",10.35000f,3.54000f,-4.61000f,"mint"),
            ("OPTICAL_TRANSMITTER_ACQUISITION_PERMIT_IN","W120 IN","P173","L84_CoPackagedOpticalTransmitter",-0.23000f,5.64000f,0.09000f,"mint"),
            ("OPTICAL_TRANSMITTER_ACTUAL_EMISSION_STATUS_OUT","W121 OUT","P174","L84_CoPackagedOpticalTransmitter",-0.11500f,5.64000f,0.09000f,"mint"),
            ("OPTICAL_TRANSMITTER_ACTUAL_EMISSION_STATUS_IN","W121 IN","P175","L84_OpticalSignalQualityTelemetry",11.66500f,3.54000f,-4.61000f,"mint"),
            ("MISSION_CHIPLET_OUTGOING_DATA_PACKETS_OUT","W122 OUT","P176","L84_FourChipletMissionPayload",-0.23000f,4.91000f,-0.13000f,"mint"),
            ("MISSION_CHIPLET_OUTGOING_DATA_PACKETS_IN","W122 IN","P177","L84_CoPackagedOpticalTransmitter",0.00000f,5.64000f,0.09000f,"mint"),
            ("AVIONICS_SUPPLY_AND_HEALTH_STATUS_OUT","W123 OUT","P178","L84_ProtectedAvionicsChassis",-0.23000f,4.79000f,-0.41000f,"mint"),
            ("AVIONICS_SUPPLY_AND_HEALTH_STATUS_IN","W123 IN","P179","L84_FourChipletMissionPayload",-0.11500f,4.91000f,-0.13000f,"mint"),
            ("TWO_PASS_OPTICAL_SCHEDULE_AUTHORIZATION_OUT","W125 OUT","P180","L84_IntegratedADCSPLCRack",4.88500f,1.54500f,5.09000f,"mint"),
            ("FLIGHT_EVENT_RECORDING_REQUEST_OUT","W141 OUT","P180","L84_IntegratedADCSPLCRack",4.67500f,1.75500f,5.09000f,"mint"),
            ("TWO_PASS_OPTICAL_SCHEDULE_AUTHORIZATION_IN","W125 IN","P181","L84_GroundStationTargetProjector",9.10000f,3.54000f,-4.61000f,"mint"),
            ("BEAM_POINTING_ERROR_TO_ANALYZER_OUT","W126 OUT","P182","L84_OpticalLineOfSightSensor",8.01500f,3.54000f,-4.61000f,"blue"),
            ("BEAM_POINTING_ERROR_TO_ANALYZER_IN","W126 IN","P183","L84_PrecisionPointingErrorAnalyzer",2.36500f,1.44000f,5.09000f,"blue"),
            ("STRUCTURAL_JITTER_OPTICAL_QUALITY_IMPACT_OUT","W127 OUT","P184","L84_StructuralVibrationSensor",5.45000f,3.54000f,-4.61000f,"blue"),
            ("STRUCTURAL_JITTER_OPTICAL_QUALITY_IMPACT_IN","W127 IN","P185","L84_OpticalSignalQualityTelemetry",11.78000f,3.54000f,-4.61000f,"blue"),
            ("SUN_SAFE_PORT_ARRAY_POINTING_REQUEST_OUT","W128 OUT","P186","L84_SunSafeAttitudeController",-5.42000f,3.44000f,-4.41000f,"mint"),
            ("SUN_SAFE_PORT_ARRAY_POINTING_REQUEST_IN","W128 IN","P187","L84_PortSolarArrayWing",-5.23000f,4.86000f,-1.00000f,"mint"),
            ("SUN_SAFE_STARBOARD_ARRAY_POINTING_REQUEST_IN","W129 IN","P188","L84_StarboardSolarArrayWing",4.77000f,4.86000f,-1.00000f,"mint"),
            ("OPTICAL_COMMUNICATION_QUALIFICATION_LOG_IN","W130 IN","P189","L84_AttitudeFlightDataRecorder",8.72000f,1.44000f,5.09000f,"mint"),
            ("ADCS_PLC_MODE_AND_SEQUENCING_TELEMETRY_OUT","W131 OUT","P190","L84_IntegratedADCSPLCRack",5.00000f,1.54500f,5.09000f,"mint"),
            ("OPERATOR_MODE_REQUEST_OUT","W132 OUT","P191","L84_HolographicAttitudeControlSCADA",0.23000f,7.59000f,-4.91000f,"mint"),
            ("OPERATOR_MODE_REQUEST_IN","W132 IN","P192","L84_IntegratedADCSPLCRack",5.11500f,1.54500f,5.09000f,"mint"),
            ("THREE_AXIS_CONTROL_STABILITY_QUALIFICATION_OUT","W133 OUT","P193","L84_AttitudeFlightController",-1.42000f,1.65000f,5.09000f,"mint"),
            ("WHEEL_SPEED_MARGIN_CERTIFICATE_OUT","W135 OUT","P194","L84_ReactionWheelTachometerArray",-9.83500f,1.34000f,4.39000f,"mint"),
            ("ATTITUDE_SAFE_MODE_AND_ALARM_STATUS_IN","W137 IN","P195","L84_IntegratedADCSPLCRack",5.23000f,1.54500f,5.09000f,"mint"),
            ("CERTIFICATION_BEACON_COMMAND_IN","W138 IN","P196","L84_ADCSCertificationBeacon",-0.23000f,8.39000f,-4.81000f,"mint"),
            ("SIX_OBJECTIVE_CERTIFICATE_STATE_OUT","W139 OUT","P197","L84_ADCSCertificationBeacon",-0.11500f,8.39000f,-4.81000f,"mint"),
            ("TIMESTAMPED_FLIGHT_RECORDER_EVENT_TELEMETRY_OUT","W140 OUT","P198","L84_AttitudeFlightDataRecorder",8.83500f,1.44000f,5.09000f,"mint"),
            ("FLIGHT_EVENT_RECORDING_REQUEST_IN","W141 IN","P199","L84_AttitudeFlightDataRecorder",8.95000f,1.44000f,5.09000f,"mint"),
            ("","P200","P200","L84_RotationalTestRigPedestal",-0.23000f,1.09000f,-1.05000f,"gold"),
            ("","P201","P201","L84_SpacecraftBodyAxesHologram",-0.23000f,5.99000f,-1.00000f,"gold"),
            ("","P202","P202","L84_MagneticTorqueRodDriveController",-5.18500f,1.99500f,-4.47000f,"gold"),
            ("","P203","P203","L84_EarthHorizonSensor",11.65000f,2.39000f,0.79000f,"gold"),
            ("","P204","P204","L84_SolarRadiationPressureTorqueModel",-0.86500f,3.54000f,-4.61000f,"gold"),
            ("","P205","P205","L84_CriticalAttitudeFeedbackStation",10.53000f,1.43000f,5.09000f,"gold"),
            ("","P206","P206","L84_ProtectedAvionicsChassis",-0.11500f,4.79000f,-0.41000f,"gold"),
            ("","P207","P207","L84_ReactionWheelX",-11.43500f,2.94000f,0.85000f,"gold"),
            ("","P208","P208","L84_OrbitalMagneticFieldEmulator",-2.11500f,3.39000f,-4.76000f,"gold"),
            ("","P209","P209","L84_ComplementaryFusionDisplay",-4.18000f,1.44000f,5.09000f,"gold"),
            ("","P210","P210","L84_StructuralVibrationSensor",5.56500f,3.54000f,-4.61000f,"gold"),
            ("","P211","P211","L84_Regulated5VControlSupply",6.18500f,1.44000f,5.09000f,"gold"),
            ("","P212","P212","L84_CoPackagedOpticalTransmitter",0.11500f,5.64000f,0.09000f,"gold"),
            ("","P213","P213","L84_ReactionWheelZ",-8.68500f,2.94000f,0.85000f,"gold"),
            ("","P214","P214","L84_MomentumDesaturationController",2.15000f,3.39500f,-4.71000f,"gold"),
            ("","P215","P215","L84_EstimatorUncertaintyDisplay",-2.83500f,1.44000f,5.09000f,"gold"),
            ("","P216","P216","L84_OpticalLineOfSightSensor",8.13000f,3.54000f,-4.61000f,"gold"),
            ("","P217","P217","L84_HolographicAttitudeControlSCADA",-0.23000f,7.69500f,-4.91000f,"gold"),
            ("","P218","P218","L84_StarboardSolarArrayWing",4.88500f,4.86000f,-1.00000f,"gold"),
        };

        public static IEnumerable<string> TerminalIds => SparkLevel84Wiring.Pins;

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 84")]
        public static void Install()
        {
            SparkLevel84Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel84Process>().rig == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 84/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel84Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.rig==null || control.rig.GetComponentsInChildren<Transform>(true).Count(item=>item.name.StartsWith("ANIM_",StringComparison.Ordinal))!=58 ||
                control.pins.Length!=275 || control.pins.Distinct().Count()!=275 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel84Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 84 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 84] Validated 275 distinct terminals, 142 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_84.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_84_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 84's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 84 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }


        [MenuItem("Tools/Project Spark/Level 84/Fit Celestial Arena")]
        public static void Fit()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode to fit the arena.");
            var models=Models.Select((name,index)=>AssetDatabase.LoadAssetAtPath<GameObject>(
                Folder+$"Models/L84_{index:00}_{name}_Fitted.fbx")??throw new InvalidOperationException("Missing module "+name)).ToArray();
            var root=PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                Fit(root,models);
                if(PrefabUtility.SaveAsPrefabAsset(root,Prefab)==null)throw new InvalidOperationException("Could not save Level 84 arena.");
                AssetDatabase.SaveAssets();
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }

        private static void Fit(GameObject root, GameObject[] models)
        {
            var bench = root.transform.Find("Celestial Test Arena");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel84Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 84 is missing its workbench, controls, or scene bindings.");


            const float floor=.425f;
            foreach (var terminal in loop.pins)
                if (terminal.Owner != null)
                    terminal.transform.SetParent(terminal.Owner.transform, true);
            foreach (string name in new[] {"L84_HD_VISUAL_ART", "Equipment terminal panels", "Motion pivots"})
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
            var assembly = Child(bench, "L84_HD_VISUAL_ART", new Vector3(0,floor,-.010f));
            for (var index=0;index<models.Length;index++)
            {
                var model=models[index];
                var instance = Object.Instantiate(model, assembly, false).transform;
                instance.name="L84_"+Models[index];
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L84_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L84_Board" + name];
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
                    "glass"=>"OpticalGlass",
                    _ when mesh.name.Contains("ArenaBase")=>"StageFloor",
                    _ when mesh.name.Contains("Starfield")||mesh.name.Contains("Hologram")=>"CelestialHologram",
                    _ when mesh.name.Contains("TorqueRod")=>"CopperCoils",
                    _ when mesh.name.Contains("Solar")=>"SolarCellBlue",
                    _ when mesh.name.Contains("TestRig")=>"TitaniumAlloy",
                    _ when mesh.name.Contains("Spacecraft")=>"SpacecraftAlloy",
                    _ when mesh.name.Contains("Payload")=>"CarbonComposite",
                    _ when mesh.name.Contains("Wheel")=>"BrushedSteel",
                    _ when mesh.name.Contains("Gyro")=>"GoldContacts",
                    _ when mesh.name.Contains("Tracker")=>"BlackCeramic",
                    _ when mesh.name.Contains("SCADA")=>"InstrumentPanel",
                    _=>null
                };
                if(mesh.name.Contains("PanoramicStarfield")&&mesh.bounds.size.x>assembly.lossyScale.x*.7f)
                    {key="glass";surface="OpticalGlass";}
                var material = MaterialFor(shader,key,surface);

                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loop.eventText == null || loop.buttons.Length!=21)
            {
                var oldControls = root.transform.Find("Level 84 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel84Builder.BuildControls(root.transform,loop,font);
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
                pinData.FindProperty("maxConnections").intValue=SparkLevel84Wiring.Wires.Count(wire=>wire.a==index||wire.b==index);
                pinData.ApplyModifiedPropertiesWithoutUndo();
                Seat(terminal,bench,local,pinLayer,rim,MaterialFor(shader,"void"));
                var tag=Label(mount,font,"Label "+spec.id,spec.label,new Vector3(0,.009f,-.007f),.014f);
                tag.transform.localRotation=Quaternion.Euler(22,0,0);
                loop.pinLabels[index]=tag;
                tag.gameObject.SetActive(false);
            }
            foreach(var body in bench.GetComponentsInChildren<MeshRenderer>(true))
                if(body.name=="Body") body.gameObject.SetActive(false);
            var title=Label(labels,font,"Level name","DEFY THE TUMBLE",new Vector3(0,floor+.018f,-.688f),.023f);
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
            Lamp("Celestial key",new Vector3(-.42f,.92f,-.38f),new Color(1,.93f,.81f),3.3f,2.3f);
            Lamp("Cool rim fill",new Vector3(.68f,.62f,.21f),new Color(.48f,.77f,1),2.1f,1.65f);
            foreach(var item in assembly.GetComponentsInChildren<Transform>(true))
            {
                int index=item.name.IndexOf("ANIM_",StringComparison.Ordinal);
                if(index>0&&item.name.StartsWith("fitted_",StringComparison.Ordinal)) item.name=item.name[index..];
            }
            loop.rig=assembly.gameObject.AddComponent<SparkLevel84VisualRig>();
            var fx=new SerializedObject(loop.rig);
            fx.FindProperty("criticalFeedbackFaultLight").objectReferenceValue=Lamp("Trust fault",new Vector3(.61f,.31f,-.23f),new Color(1,.12f,.18f),.7f,.59f);
            fx.FindProperty("opticalBoresightLight").objectReferenceValue=Lamp("Optical lock",new Vector3(-.35f,.57f,.13f),new Color(.18f,.80f,1),.8f,.69f);
            fx.FindProperty("momentumDumpLight").objectReferenceValue=Lamp("Magnetic unloading",new Vector3(.31f,.32f,.14f),new Color(.73f,.20f,1),.6f,.68f);
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
            string materialName=surface==null?"L84 "+key:"L84 "+key+" "+surface;
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
                    throw new InvalidOperationException("Missing Level 84 surface maps for "+surface);
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





















