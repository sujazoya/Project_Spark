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
    public static class SparkLevel86ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_86_Layout/";
        public const string Prefab = Folder + "Level_86_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "OrbitalForgeArenaBase","PanoramicEarthMaintenanceViewport","StationExteriorServiceTruss","StationRobotServiceBayFrame","DockedCargoSpacecraft","CargoServiceBayHatch","RobotLinearTraverseRail","SevenAxisRobotBase","RobotJoint1BaseYaw","RobotJoint2Shoulder","RobotJoint3UpperArm","RobotJoint4Elbow","RobotJoint5Forearm","RobotJoint6WristPitch","RobotJoint7WristRoll","RobotCounterbalanceModule","JointServoDriveRack","JointEncoderFeedbackArray","JointTorqueCurrentSensorRack","RobotBrakeAndTorqueLimiter","SevenAxisReachEnvelopeHologram","RoboticHomingCalibrationFixture","StereoMachineVisionCameraLeft","StereoMachineVisionCameraRight","StructuredLightDepthScanner","OpticalFiducialTargetArray","PrecisionVisionCalibrationBoard","LaserAlignmentMetrologyUnit","SixAxisWristForceTorqueSensor","ForceTorqueCalibrationFixture","CompliantInsertionController","SixAxisContactForceHologram","RobotJointPathPlanningComputer","InverseKinematicsSolverDisplay","CollisionProximityScanner","RobotTrajectorySafetySupervisor","FlexiblePayloadModalSensor","ActiveVibrationDampingController","AdaptiveRadiatorHandlingGripper","AutomaticEndEffectorToolChanger","PrecisionTorqueFasteningDriver","CaptiveFastenerAndBoltRack","VacuumCompatibleInspectionProbe","RoboticToolDockAndCalibrationNest","DualJawGripperPressureSensorArray","RadiatorGrappleHandleFixture","FailedThermalRadiatorModule","ReplacementThermalRadiatorModule","RadiatorMountingSpaceTruss","RadiatorThermalInterfacePlate","CoolantQuickDisconnectA","CoolantQuickDisconnectB","CoolantValveManifold","CoolantPressureTransducer","CoolantFlowMeter","RadiatorTemperatureSensorArray","StationAvionicsHeatLoadEmulator","ThermalLoopPumpSimulator","RadiatorLeakTestController","CoolantIsolationAndVentStation","RadiatorFastenerActuatorDriver","StationThermalTrendHologram","RadiatorThermalPerformanceAnalyzer","FailedRadiatorDisposalCradle","PayloadVibrationFaultInjector","JointEncoderDriftFaultInjector","AsymmetricGripperFaultInjector","RoboticContactSafetyPLC","ManipulatorEmergencyStopStation","Protected5VControlSupply","RobotMotorPowerDistributionUnit","IndependentContactSafetyRelay","SevenAxisJointServoController","ContactForceSignalConditioner","StereoVisionFusionComputer","RadiatorServiceSequencingPLC","HolographicOrbitalRoboticsSCADA","W129CriticalFeedbackPatchStation","HardwareInterlockDiagnosticRack","ServicingMissionCertificationBeacon","ExternalInspectionCameraGantry","RoboticServicingEventRecorder"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .0075f;
        private const float SocketDiameter = .0086f;

        private static readonly (string id, string label, string anchor, string model, float x, float y, float z, string color)[] Sockets =
        {
            ("W129_OUT_ANALOG_FEEDBACK","W129 OUT","P045","L86_SixAxisWristForceTorqueSensor",0.48000f,5.14880f,-1.30000f,"amber"),
            ("W057_OUT_ANALOG_FEEDBACK","W057 OUT","P045","L86_SixAxisWristForceTorqueSensor",0.26000f,5.14880f,-1.30000f,"blue"),
            ("W129_IN_ANALOG_FEEDBACK","W129 IN","P119","L86_RoboticContactSafetyPLC",-10.68000f,3.34880f,5.98000f,"amber"),
            ("","P001","P001","L86_RobotJoint1BaseYaw",-4.97000f,2.36880f,-1.30000f,"gold"),
            ("W013_IN_ACTUATOR_COMMAND","W013 IN","P002","L86_RobotJoint1BaseYaw",-4.83000f,2.36880f,-1.30000f,"mint"),
            ("W014_OUT_ANALOG_FEEDBACK","W014 OUT","P002","L86_RobotJoint1BaseYaw",-4.61000f,2.36880f,-1.30000f,"blue"),
            ("W016_IN_ACTUATOR_COMMAND","W016 IN","P003","L86_RobotJoint2Shoulder",-4.57000f,2.87880f,-1.30000f,"mint"),
            ("W017_OUT_ANALOG_FEEDBACK","W017 OUT","P003","L86_RobotJoint2Shoulder",-4.79000f,2.87880f,-1.30000f,"blue"),
            ("","P004","P004","L86_RobotJoint2Shoulder",-4.43000f,2.87880f,-1.30000f,"gold"),
            ("","P005","P005","L86_RobotJoint3UpperArm",-3.72000f,3.51880f,-1.30000f,"gold"),
            ("W019_IN_ACTUATOR_COMMAND","W019 IN","P006","L86_RobotJoint3UpperArm",-3.58000f,3.51880f,-1.30000f,"mint"),
            ("W020_OUT_ANALOG_FEEDBACK","W020 OUT","P006","L86_RobotJoint3UpperArm",-3.36000f,3.51880f,-1.30000f,"blue"),
            ("W022_IN_ACTUATOR_COMMAND","W022 IN","P007","L86_RobotJoint4Elbow",-2.77000f,4.06880f,-1.30000f,"mint"),
            ("W023_OUT_ANALOG_FEEDBACK","W023 OUT","P007","L86_RobotJoint4Elbow",-2.99000f,4.06880f,-1.30000f,"blue"),
            ("","P008","P008","L86_RobotJoint4Elbow",-2.63000f,4.06880f,-1.30000f,"gold"),
            ("","P009","P009","L86_RobotJoint5Forearm",-1.72000f,4.58880f,-1.30000f,"gold"),
            ("W025_IN_ACTUATOR_COMMAND","W025 IN","P010","L86_RobotJoint5Forearm",-1.58000f,4.58880f,-1.30000f,"mint"),
            ("W026_OUT_ANALOG_FEEDBACK","W026 OUT","P010","L86_RobotJoint5Forearm",-1.36000f,4.58880f,-1.30000f,"blue"),
            ("W028_IN_ACTUATOR_COMMAND","W028 IN","P011","L86_RobotJoint6WristPitch",-0.97000f,4.99880f,-1.30000f,"mint"),
            ("W029_OUT_ANALOG_FEEDBACK","W029 OUT","P011","L86_RobotJoint6WristPitch",-1.19000f,4.99880f,-1.30000f,"blue"),
            ("","P012","P012","L86_RobotJoint6WristPitch",-0.83000f,4.99880f,-1.30000f,"gold"),
            ("","P013","P013","L86_RobotJoint7WristRoll",-0.17000f,5.26880f,-1.30000f,"gold"),
            ("W031_IN_ACTUATOR_COMMAND","W031 IN","P014","L86_RobotJoint7WristRoll",-0.03000f,5.26880f,-1.30000f,"mint"),
            ("W032_OUT_ANALOG_FEEDBACK","W032 OUT","P014","L86_RobotJoint7WristRoll",-0.03000f,5.04880f,-1.30000f,"blue"),
            ("W006_IN_POWER_CONTROL","W006 IN","P015","L86_JointServoDriveRack",-12.77000f,1.55880f,6.01000f,"red"),
            ("","P016","P016","L86_JointEncoderFeedbackArray",-11.52000f,1.55880f,6.01000f,"gold"),
            ("W034_OUT_DATA_STREAM","W034 OUT","P017","L86_JointEncoderFeedbackArray",-11.38000f,1.55880f,6.01000f,"mint"),
            ("W139_OUT_DATA_STREAM","W139 OUT","P017","L86_JointEncoderFeedbackArray",-11.38000f,1.33880f,6.01000f,"mint"),
            ("W017_IN_ANALOG_FEEDBACK","W017 IN","P018","L86_JointEncoderFeedbackArray",-11.24000f,1.55880f,6.01000f,"blue"),
            ("W026_IN_ANALOG_FEEDBACK","W026 IN","P018","L86_JointEncoderFeedbackArray",-11.02000f,1.33880f,6.01000f,"blue"),
            ("W069_IN_SAFETY_CONTROL","W069 IN","P019","L86_JointEncoderFeedbackArray",-11.10000f,1.55880f,6.01000f,"mint"),
            ("W117_OUT_SAFETY_CONTROL","W117 OUT","P019","L86_JointEncoderFeedbackArray",-11.10000f,1.77880f,6.01000f,"mint"),
            ("","P020","P020","L86_JointEncoderFeedbackArray",-10.96000f,1.55880f,6.01000f,"gold"),
            ("W014_IN_ANALOG_FEEDBACK","W014 IN","P021","L86_JointEncoderFeedbackArray",-10.82000f,1.55880f,6.01000f,"blue"),
            ("W023_IN_ANALOG_FEEDBACK","W023 IN","P021","L86_JointEncoderFeedbackArray",-10.82000f,1.33880f,6.01000f,"blue"),
            ("W032_IN_ANALOG_FEEDBACK","W032 IN","P021","L86_JointEncoderFeedbackArray",-10.82000f,1.77880f,6.01000f,"blue"),
            ("","P022","P022","L86_JointEncoderFeedbackArray",-10.68000f,1.55880f,6.01000f,"gold"),
            ("W070_OUT_ANALOG_FEEDBACK","W070 OUT","P023","L86_JointEncoderFeedbackArray",-11.52000f,1.66680f,6.01000f,"blue"),
            ("W020_IN_ANALOG_FEEDBACK","W020 IN","P024","L86_JointEncoderFeedbackArray",-11.38000f,1.66680f,6.01000f,"blue"),
            ("W029_IN_ANALOG_FEEDBACK","W029 IN","P024","L86_JointEncoderFeedbackArray",-11.38000f,1.88680f,6.01000f,"blue"),
            ("","P025","P025","L86_JointTorqueCurrentSensorRack",-10.27000f,1.55880f,6.01000f,"gold"),
            ("","P026","P026","L86_JointTorqueCurrentSensorRack",-10.13000f,1.55880f,6.01000f,"gold"),
            ("","P027","P027","L86_JointTorqueCurrentSensorRack",-9.99000f,1.55880f,6.01000f,"gold"),
            ("W015_OUT_ANALOG_FEEDBACK","W015 OUT","P028","L86_JointTorqueCurrentSensorRack",-9.85000f,1.55880f,6.01000f,"blue"),
            ("W018_OUT_ANALOG_FEEDBACK","W018 OUT","P028","L86_JointTorqueCurrentSensorRack",-9.85000f,1.33880f,6.01000f,"blue"),
            ("W021_OUT_ANALOG_FEEDBACK","W021 OUT","P028","L86_JointTorqueCurrentSensorRack",-9.85000f,1.77880f,6.01000f,"blue"),
            ("W024_OUT_ANALOG_FEEDBACK","W024 OUT","P028","L86_JointTorqueCurrentSensorRack",-10.07000f,1.33880f,6.01000f,"blue"),
            ("W027_OUT_ANALOG_FEEDBACK","W027 OUT","P028","L86_JointTorqueCurrentSensorRack",-10.07000f,1.77880f,6.01000f,"blue"),
            ("W030_OUT_ANALOG_FEEDBACK","W030 OUT","P028","L86_JointTorqueCurrentSensorRack",-9.63000f,1.33880f,6.01000f,"blue"),
            ("W033_OUT_ANALOG_FEEDBACK","W033 OUT","P028","L86_JointTorqueCurrentSensorRack",-9.63000f,1.77880f,6.01000f,"blue"),
            ("","P029","P029","L86_JointTorqueCurrentSensorRack",-9.71000f,1.55880f,6.01000f,"gold"),
            ("W140_OUT_DATA_STREAM","W140 OUT","P030","L86_JointTorqueCurrentSensorRack",-9.57000f,1.55880f,6.01000f,"mint"),
            ("","P031","P031","L86_RobotBrakeAndTorqueLimiter",-9.02000f,1.55880f,6.01000f,"gold"),
            ("W035_IN_SAFETY_CONTROL","W035 IN","P032","L86_RobotBrakeAndTorqueLimiter",-8.88000f,1.55880f,6.01000f,"mint"),
            ("W036_OUT_DIGITAL_STATUS","W036 OUT","P032","L86_RobotBrakeAndTorqueLimiter",-8.66000f,1.55880f,6.01000f,"mint"),
            ("W053_IN_DATA_STREAM","W053 IN","P033","L86_SevenAxisReachEnvelopeHologram",-3.52000f,7.49880f,-5.44000f,"mint"),
            ("W054_OUT_DIGITAL_STATUS","W054 OUT","P034","L86_RoboticHomingCalibrationFixture",-6.37000f,1.54880f,4.71000f,"mint"),
            ("W043_IN_DIGITAL_CONTROL","W043 IN","P035","L86_StereoMachineVisionCameraLeft",-1.62000f,6.14880f,-0.64000f,"mint"),
            ("W037_OUT_DATA_STREAM","W037 OUT","P036","L86_StereoMachineVisionCameraLeft",-1.48000f,6.14880f,-0.64000f,"mint"),
            ("W038_OUT_DATA_STREAM","W038 OUT","P037","L86_StereoMachineVisionCameraRight",-0.87000f,6.14880f,-0.64000f,"mint"),
            ("W044_IN_DIGITAL_CONTROL","W044 IN","P038","L86_StereoMachineVisionCameraRight",-0.73000f,6.14880f,-0.64000f,"mint"),
            ("W045_IN_DIGITAL_CONTROL","W045 IN","P039","L86_StructuredLightDepthScanner",0.63000f,6.09880f,-1.24000f,"mint"),
            ("W039_OUT_DATA_STREAM","W039 OUT","P040","L86_StructuredLightDepthScanner",0.77000f,6.09880f,-1.24000f,"mint"),
            ("W040_OUT_DATA_STREAM","W040 OUT","P041","L86_OpticalFiducialTargetArray",7.08000f,5.69880f,-2.19000f,"mint"),
            ("W041_OUT_DATA_STREAM","W041 OUT","P042","L86_PrecisionVisionCalibrationBoard",-3.62000f,1.53880f,4.82000f,"mint"),
            ("W042_OUT_ANALOG_FEEDBACK","W042 OUT","P043","L86_LaserAlignmentMetrologyUnit",1.98000f,6.34880f,-2.29000f,"blue"),
            ("W046_IN_DIGITAL_CONTROL","W046 IN","P044","L86_LaserAlignmentMetrologyUnit",2.12000f,6.34880f,-2.29000f,"mint"),
            ("W073_OUT_DATA_STREAM","W073 OUT","P046","L86_SixAxisWristForceTorqueSensor",0.62000f,5.14880f,-1.30000f,"mint"),
            ("","P047","P047","L86_SixAxisWristForceTorqueSensor",0.76000f,5.14880f,-1.30000f,"gold"),
            ("W060_OUT_ANALOG_FEEDBACK","W060 OUT","P048","L86_ForceTorqueCalibrationFixture",-2.37000f,1.54880f,5.91000f,"blue"),
            ("","P049","P049","L86_CompliantInsertionController",-1.12000f,1.54880f,5.91000f,"gold"),
            ("W063_IN_SAFETY_CONTROL","W063 IN","P050","L86_CompliantInsertionController",-0.98000f,1.54880f,5.91000f,"mint"),
            ("W064_IN_ANALOG_FEEDBACK","W064 IN","P050","L86_CompliantInsertionController",-0.98000f,1.32880f,5.91000f,"blue"),
            ("W065_OUT_DIGITAL_STATUS","W065 OUT","P050","L86_CompliantInsertionController",-0.98000f,1.76880f,5.91000f,"mint"),
            ("W122_IN_SAFETY_CONTROL","W122 IN","P050","L86_CompliantInsertionController",-1.20000f,1.32880f,5.91000f,"mint"),
            ("W058_IN_ANALOG_FEEDBACK","W058 IN","P051","L86_CompliantInsertionController",-0.84000f,1.54880f,5.91000f,"blue"),
            ("W059_OUT_ACTUATOR_COMMAND","W059 OUT","P051","L86_CompliantInsertionController",-0.62000f,1.32880f,5.91000f,"mint"),
            ("","P052","P052","L86_CompliantInsertionController",-0.70000f,1.54880f,5.91000f,"gold"),
            ("W061_IN_DATA_STREAM","W061 IN","P053","L86_SixAxisContactForceHologram",2.98000f,7.16880f,-5.49000f,"mint"),
            ("W010_IN_POWER_CONTROL","W010 IN","P054","L86_RobotJointPathPlanningComputer",0.14000f,1.54880f,5.91000f,"red"),
            ("W047_IN_DATA_STREAM","W047 IN","P054","L86_RobotJointPathPlanningComputer",-0.08000f,1.54880f,5.91000f,"mint"),
            ("W053_OUT_DATA_STREAM","W053 OUT","P054","L86_RobotJointPathPlanningComputer",0.14000f,1.32880f,5.91000f,"mint"),
            ("W050_OUT_DATA_STREAM","W050 OUT","P055","L86_RobotJointPathPlanningComputer",0.28000f,1.54880f,5.91000f,"mint"),
            ("W052_IN_DATA_STREAM","W052 IN","P055","L86_RobotJointPathPlanningComputer",0.28000f,1.76880f,5.91000f,"mint"),
            ("W067_IN_ACTUATOR_COMMAND","W067 IN","P056","L86_RobotJointPathPlanningComputer",0.42000f,1.54880f,5.91000f,"mint"),
            ("W145_OUT_DATA_STREAM","W145 OUT","P057","L86_RobotJointPathPlanningComputer",0.56000f,1.54880f,5.91000f,"mint"),
            ("","P058","P058","L86_RobotJointPathPlanningComputer",0.70000f,1.54880f,5.91000f,"gold"),
            ("W048_IN_DATA_STREAM","W048 IN","P059","L86_InverseKinematicsSolverDisplay",1.39000f,1.54880f,5.91000f,"mint"),
            ("W052_OUT_DATA_STREAM","W052 OUT","P060","L86_InverseKinematicsSolverDisplay",1.53000f,1.54880f,5.91000f,"mint"),
            ("","P061","P061","L86_CollisionProximityScanner",2.64000f,1.54880f,5.91000f,"gold"),
            ("W049_OUT_ANALOG_FEEDBACK","W049 OUT","P062","L86_CollisionProximityScanner",2.78000f,1.54880f,5.91000f,"blue"),
            ("W056_OUT_DATA_STREAM","W056 OUT","P062","L86_CollisionProximityScanner",3.00000f,1.54880f,5.91000f,"mint"),
            ("W115_OUT_SAFETY_CONTROL","W115 OUT","P062","L86_CollisionProximityScanner",2.78000f,1.32880f,5.91000f,"mint"),
            ("","P063","P063","L86_RobotTrajectorySafetySupervisor",3.89000f,1.54880f,5.91000f,"gold"),
            ("W049_IN_ANALOG_FEEDBACK","W049 IN","P064","L86_RobotTrajectorySafetySupervisor",4.03000f,1.54880f,5.91000f,"blue"),
            ("W050_IN_DATA_STREAM","W050 IN","P064","L86_RobotTrajectorySafetySupervisor",4.03000f,1.32880f,5.91000f,"mint"),
            ("W051_OUT_SAFETY_CONTROL","W051 OUT","P064","L86_RobotTrajectorySafetySupervisor",4.03000f,1.76880f,5.91000f,"mint"),
            ("W070_IN_ANALOG_FEEDBACK","W070 IN","P065","L86_RobotTrajectorySafetySupervisor",4.17000f,1.54880f,5.91000f,"blue"),
            ("W062_IN_SAFETY_CONTROL","W062 IN","P066","L86_RobotTrajectorySafetySupervisor",4.31000f,1.54880f,5.91000f,"mint"),
            ("W116_OUT_SAFETY_CONTROL","W116 OUT","P066","L86_RobotTrajectorySafetySupervisor",4.53000f,1.54880f,5.91000f,"mint"),
            ("W066_OUT_ANALOG_FEEDBACK","W066 OUT","P067","L86_FlexiblePayloadModalSensor",0.73000f,5.39880f,-1.19000f,"blue"),
            ("W074_OUT_ANALOG_FEEDBACK","W074 OUT","P067","L86_FlexiblePayloadModalSensor",0.51000f,5.39880f,-1.19000f,"blue"),
            ("W068_IN_SAFETY_CONTROL","W068 IN","P068","L86_FlexiblePayloadModalSensor",0.87000f,5.39880f,-1.19000f,"mint"),
            ("W066_IN_ANALOG_FEEDBACK","W066 IN","P069","L86_ActiveVibrationDampingController",5.14000f,1.54880f,5.91000f,"blue"),
            ("W075_OUT_ACTUATOR_COMMAND","W075 OUT","P069","L86_ActiveVibrationDampingController",4.92000f,1.54880f,5.91000f,"mint"),
            ("W067_OUT_ACTUATOR_COMMAND","W067 OUT","P070","L86_ActiveVibrationDampingController",5.28000f,1.54880f,5.91000f,"mint"),
            ("W146_OUT_DATA_STREAM","W146 OUT","P071","L86_ActiveVibrationDampingController",5.42000f,1.54880f,5.91000f,"mint"),
            ("W121_IN_SAFETY_CONTROL","W121 IN","P072","L86_AdaptiveRadiatorHandlingGripper",1.08000f,5.06880f,-1.30000f,"mint"),
            ("W077_IN_ACTUATOR_COMMAND","W077 IN","P073","L86_AdaptiveRadiatorHandlingGripper",1.22000f,5.06880f,-1.30000f,"mint"),
            ("W078_OUT_ANALOG_FEEDBACK","W078 OUT","P073","L86_AdaptiveRadiatorHandlingGripper",1.22000f,4.84880f,-1.30000f,"blue"),
            ("W087_IN_ANALOG_FEEDBACK","W087 IN","P073","L86_AdaptiveRadiatorHandlingGripper",1.22000f,5.28880f,-1.30000f,"blue"),
            ("W088_OUT_SAFETY_CONTROL","W088 OUT","P073","L86_AdaptiveRadiatorHandlingGripper",1.00000f,4.84880f,-1.30000f,"mint"),
            ("W094_OUT_DATA_STREAM","W094 OUT","P074","L86_AdaptiveRadiatorHandlingGripper",1.36000f,5.06880f,-1.30000f,"mint"),
            ("","P075","P075","L86_AdaptiveRadiatorHandlingGripper",1.50000f,5.06880f,-1.30000f,"gold"),
            ("W086_IN_DIGITAL_STATUS","W086 IN","P076","L86_AutomaticEndEffectorToolChanger",0.43000f,5.02880f,-1.06000f,"mint"),
            ("W079_IN_ACTUATOR_COMMAND","W079 IN","P077","L86_AutomaticEndEffectorToolChanger",0.57000f,5.02880f,-1.06000f,"mint"),
            ("W080_OUT_DIGITAL_STATUS","W080 OUT","P077","L86_AutomaticEndEffectorToolChanger",0.79000f,5.02880f,-1.06000f,"mint"),
            ("W081_IN_ACTUATOR_COMMAND","W081 IN","P078","L86_PrecisionTorqueFasteningDriver",1.90000f,4.94880f,-1.06000f,"mint"),
            ("W082_OUT_ANALOG_FEEDBACK","W082 OUT","P079","L86_PrecisionTorqueFasteningDriver",2.04000f,4.94880f,-1.06000f,"blue"),
            ("W091_IN_DIGITAL_STATUS","W091 IN","P079","L86_PrecisionTorqueFasteningDriver",2.04000f,4.72880f,-1.06000f,"mint"),
            ("W083_IN_DIGITAL_STATUS","W083 IN","P080","L86_PrecisionTorqueFasteningDriver",2.18000f,4.94880f,-1.06000f,"mint"),
            ("W083_OUT_DIGITAL_STATUS","W083 OUT","P081","L86_CaptiveFastenerAndBoltRack",-5.62000f,1.29880f,5.81000f,"mint"),
            ("W084_IN_DIGITAL_CONTROL","W084 IN","P082","L86_VacuumCompatibleInspectionProbe",0.51000f,5.40880f,-0.99000f,"mint"),
            ("W085_OUT_DATA_STREAM","W085 OUT","P082","L86_VacuumCompatibleInspectionProbe",0.29000f,5.40880f,-0.99000f,"mint"),
            ("","P083","P083","L86_VacuumCompatibleInspectionProbe",0.65000f,5.40880f,-0.99000f,"gold"),
            ("W086_OUT_DIGITAL_STATUS","W086 OUT","P084","L86_RoboticToolDockAndCalibrationNest",-6.07000f,1.52880f,4.28000f,"mint"),
            ("","P085","P085","L86_DualJawGripperPressureSensorArray",1.18000f,4.94880f,-1.04000f,"gold"),
            ("W078_IN_ANALOG_FEEDBACK","W078 IN","P086","L86_DualJawGripperPressureSensorArray",1.32000f,4.94880f,-1.04000f,"blue"),
            ("W118_OUT_SAFETY_CONTROL","W118 OUT","P087","L86_DualJawGripperPressureSensorArray",1.46000f,4.94880f,-1.04000f,"mint"),
            ("W071_OUT_ANALOG_FEEDBACK","W071 OUT","P088","L86_DualJawGripperPressureSensorArray",1.60000f,4.94880f,-1.04000f,"blue"),
            ("W089_IN_SAFETY_CONTROL","W089 IN","P088","L86_DualJawGripperPressureSensorArray",1.60000f,4.72880f,-1.04000f,"mint"),
            ("W090_OUT_ANALOG_FEEDBACK","W090 OUT","P088","L86_DualJawGripperPressureSensorArray",1.60000f,5.16880f,-1.04000f,"blue"),
            ("W087_OUT_ANALOG_FEEDBACK","W087 OUT","P089","L86_RadiatorGrappleHandleFixture",6.38000f,5.34880f,-1.71000f,"blue"),
            ("W097_IN_ACTUATOR_COMMAND","W097 IN","P090","L86_CoolantQuickDisconnectA",6.58000f,4.44880f,-1.39000f,"mint"),
            ("W099_OUT_ANALOG_FEEDBACK","W099 OUT","P090","L86_CoolantQuickDisconnectA",6.36000f,4.44880f,-1.39000f,"blue"),
            ("W098_IN_ACTUATOR_COMMAND","W098 IN","P091","L86_CoolantQuickDisconnectB",8.98000f,4.44880f,-1.39000f,"mint"),
            ("W100_OUT_ANALOG_FEEDBACK","W100 OUT","P091","L86_CoolantQuickDisconnectB",8.76000f,4.44880f,-1.39000f,"blue"),
            ("W101_IN_ACTUATOR_COMMAND","W101 IN","P092","L86_CoolantValveManifold",8.73000f,1.48880f,5.91000f,"mint"),
            ("W102_OUT_ANALOG_FEEDBACK","W102 OUT","P092","L86_CoolantValveManifold",8.95000f,1.48880f,5.91000f,"blue"),
            ("W103_OUT_ANALOG_FEEDBACK","W103 OUT","P093","L86_CoolantPressureTransducer",9.93000f,1.48880f,5.91000f,"blue"),
            ("W104_OUT_ANALOG_FEEDBACK","W104 OUT","P094","L86_CoolantFlowMeter",11.13000f,1.48880f,5.91000f,"blue"),
            ("W105_OUT_ANALOG_FEEDBACK","W105 OUT","P095","L86_RadiatorTemperatureSensorArray",9.48000f,5.04880f,-2.34000f,"blue"),
            ("W112_OUT_DATA_STREAM","W112 OUT","P095","L86_RadiatorTemperatureSensorArray",9.26000f,5.04880f,-2.34000f,"mint"),
            ("W106_OUT_ANALOG_FEEDBACK","W106 OUT","P096","L86_StationAvionicsHeatLoadEmulator",-3.12000f,6.04880f,-2.89000f,"blue"),
            ("W107_OUT_ANALOG_FEEDBACK","W107 OUT","P097","L86_ThermalLoopPumpSimulator",6.63000f,1.51880f,5.96000f,"blue"),
            ("W110_IN_ACTUATOR_COMMAND","W110 IN","P097","L86_ThermalLoopPumpSimulator",6.41000f,1.51880f,5.96000f,"mint"),
            ("W144_OUT_DATA_STREAM","W144 OUT","P098","L86_RadiatorLeakTestController",7.98000f,1.50880f,5.98000f,"mint"),
            ("W009_IN_POWER_CONTROL","W009 IN","P099","L86_RadiatorLeakTestController",8.12000f,1.50880f,5.98000f,"red"),
            ("W108_OUT_DIGITAL_STATUS","W108 OUT","P099","L86_RadiatorLeakTestController",8.12000f,1.28880f,5.98000f,"mint"),
            ("W103_IN_ANALOG_FEEDBACK","W103 IN","P100","L86_RadiatorLeakTestController",8.26000f,1.50880f,5.98000f,"blue"),
            ("W104_IN_ANALOG_FEEDBACK","W104 IN","P100","L86_RadiatorLeakTestController",8.26000f,1.72880f,5.98000f,"blue"),
            ("","P101","P101","L86_RadiatorLeakTestController",8.40000f,1.50880f,5.98000f,"gold"),
            ("W095_IN_ACTUATOR_COMMAND","W095 IN","P102","L86_CoolantIsolationAndVentStation",9.68000f,3.14880f,5.62000f,"mint"),
            ("W096_OUT_ANALOG_FEEDBACK","W096 OUT","P102","L86_CoolantIsolationAndVentStation",9.46000f,3.14880f,5.62000f,"blue"),
            ("W091_OUT_DIGITAL_STATUS","W091 OUT","P103","L86_RadiatorFastenerActuatorDriver",6.72000f,5.49880f,-2.24000f,"mint"),
            ("W092_IN_ACTUATOR_COMMAND","W092 IN","P103","L86_RadiatorFastenerActuatorDriver",6.50000f,5.49880f,-2.24000f,"mint"),
            ("W111_IN_DATA_STREAM","W111 IN","P104","L86_StationThermalTrendHologram",-0.42000f,8.49880f,-5.64000f,"mint"),
            ("","P105","P105","L86_RadiatorThermalPerformanceAnalyzer",10.86000f,3.14880f,5.62000f,"gold"),
            ("W105_IN_ANALOG_FEEDBACK","W105 IN","P106","L86_RadiatorThermalPerformanceAnalyzer",11.00000f,3.14880f,5.62000f,"blue"),
            ("W106_IN_ANALOG_FEEDBACK","W106 IN","P106","L86_RadiatorThermalPerformanceAnalyzer",11.00000f,2.92880f,5.62000f,"blue"),
            ("W107_IN_ANALOG_FEEDBACK","W107 IN","P106","L86_RadiatorThermalPerformanceAnalyzer",11.00000f,3.36880f,5.62000f,"blue"),
            ("W109_OUT_DATA_STREAM","W109 OUT","P107","L86_RadiatorThermalPerformanceAnalyzer",11.14000f,3.14880f,5.62000f,"mint"),
            ("W143_OUT_DATA_STREAM","W143 OUT","P107","L86_RadiatorThermalPerformanceAnalyzer",11.36000f,2.92880f,5.62000f,"mint"),
            ("W111_OUT_DATA_STREAM","W111 OUT","P108","L86_RadiatorThermalPerformanceAnalyzer",11.28000f,3.14880f,5.62000f,"mint"),
            ("W093_OUT_DIGITAL_STATUS","W093 OUT","P109","L86_FailedRadiatorDisposalCradle",8.83000f,0.94880f,3.66000f,"mint"),
            ("W068_OUT_SAFETY_CONTROL","W068 OUT","P110","L86_PayloadVibrationFaultInjector",6.33000f,3.19880f,6.11000f,"mint"),
            ("W125_OUT_DIGITAL_STATUS","W125 OUT","P110","L86_PayloadVibrationFaultInjector",6.11000f,3.19880f,6.11000f,"mint"),
            ("W069_OUT_SAFETY_CONTROL","W069 OUT","P111","L86_JointEncoderDriftFaultInjector",-7.54000f,3.19880f,6.11000f,"mint"),
            ("W124_OUT_DIGITAL_STATUS","W124 OUT","P111","L86_JointEncoderDriftFaultInjector",-7.76000f,3.19880f,6.11000f,"mint"),
            ("W089_OUT_SAFETY_CONTROL","W089 OUT","P112","L86_AsymmetricGripperFaultInjector",-6.27000f,3.19880f,6.11000f,"mint"),
            ("W126_OUT_DIGITAL_STATUS","W126 OUT","P112","L86_AsymmetricGripperFaultInjector",-6.49000f,3.19880f,6.11000f,"mint"),
            ("","P113","P113","L86_RoboticContactSafetyPLC",-11.52000f,3.34880f,5.98000f,"gold"),
            ("W001_IN_POWER_CONTROL","W001 IN","P114","L86_RoboticContactSafetyPLC",-11.38000f,3.34880f,5.98000f,"red"),
            ("W117_IN_SAFETY_CONTROL","W117 IN","P114","L86_RoboticContactSafetyPLC",-11.38000f,3.12880f,5.98000f,"mint"),
            ("W122_OUT_SAFETY_CONTROL","W122 OUT","P114","L86_RoboticContactSafetyPLC",-11.60000f,3.12880f,5.98000f,"mint"),
            ("W127_OUT_DIGITAL_STATUS","W127 OUT","P114","L86_RoboticContactSafetyPLC",-11.16000f,3.12880f,5.98000f,"mint"),
            ("W076_IN_DIGITAL_STATUS","W076 IN","P115","L86_RoboticContactSafetyPLC",-11.24000f,3.34880f,5.98000f,"mint"),
            ("W133_OUT_SAFETY_CONTROL","W133 OUT","P115","L86_RoboticContactSafetyPLC",-11.24000f,2.90880f,5.98000f,"mint"),
            ("","P116","P116","L86_RoboticContactSafetyPLC",-11.10000f,3.34880f,5.98000f,"gold"),
            ("","P117","P117","L86_RoboticContactSafetyPLC",-10.96000f,3.34880f,5.98000f,"gold"),
            ("W113_IN_SAFETY_CONTROL","W113 IN","P118","L86_RoboticContactSafetyPLC",-10.82000f,3.34880f,5.98000f,"mint"),
            ("W118_IN_SAFETY_CONTROL","W118 IN","P118","L86_RoboticContactSafetyPLC",-10.82000f,3.12880f,5.98000f,"mint"),
            ("W123_OUT_SAFETY_CONTROL","W123 OUT","P118","L86_RoboticContactSafetyPLC",-10.60000f,3.12880f,5.98000f,"mint"),
            ("W147_OUT_DIGITAL_STATUS","W147 OUT","P118","L86_RoboticContactSafetyPLC",-10.82000f,2.90880f,5.98000f,"mint"),
            ("W135_OUT_DIGITAL_STATUS","W135 OUT","P120","L86_RoboticContactSafetyPLC",-11.52000f,3.45680f,5.98000f,"mint"),
            ("","P121","P121","L86_RoboticContactSafetyPLC",-11.38000f,3.45680f,5.98000f,"gold"),
            ("W114_IN_SAFETY_CONTROL","W114 IN","P122","L86_RoboticContactSafetyPLC",-11.24000f,3.45680f,5.98000f,"mint"),
            ("W119_IN_SAFETY_CONTROL","W119 IN","P122","L86_RoboticContactSafetyPLC",-11.24000f,3.89680f,5.98000f,"mint"),
            ("W124_IN_DIGITAL_STATUS","W124 IN","P122","L86_RoboticContactSafetyPLC",-11.68000f,3.67680f,5.98000f,"mint"),
            ("W130_IN_DIGITAL_STATUS","W130 IN","P123","L86_RoboticContactSafetyPLC",-11.10000f,3.45680f,5.98000f,"mint"),
            ("W136_OUT_DIGITAL_STATUS","W136 OUT","P124","L86_RoboticContactSafetyPLC",-10.96000f,3.45680f,5.98000f,"mint"),
            ("","P125","P125","L86_RoboticContactSafetyPLC",-10.82000f,3.45680f,5.98000f,"gold"),
            ("W115_IN_SAFETY_CONTROL","W115 IN","P126","L86_RoboticContactSafetyPLC",-10.68000f,3.45680f,5.98000f,"mint"),
            ("W120_OUT_SAFETY_CONTROL","W120 OUT","P126","L86_RoboticContactSafetyPLC",-10.46000f,3.45680f,5.98000f,"mint"),
            ("W125_IN_DIGITAL_STATUS","W125 IN","P126","L86_RoboticContactSafetyPLC",-10.46000f,3.67680f,5.98000f,"mint"),
            ("W131_OUT_DIGITAL_STATUS","W131 OUT","P127","L86_RoboticContactSafetyPLC",-11.52000f,3.56480f,5.98000f,"mint"),
            ("","P128","P128","L86_RoboticContactSafetyPLC",-11.38000f,3.56480f,5.98000f,"gold"),
            ("","P129","P129","L86_RoboticContactSafetyPLC",-11.24000f,3.56480f,5.98000f,"gold"),
            ("W088_IN_SAFETY_CONTROL","W088 IN","P130","L86_RoboticContactSafetyPLC",-11.10000f,3.56480f,5.98000f,"mint"),
            ("W116_IN_SAFETY_CONTROL","W116 IN","P130","L86_RoboticContactSafetyPLC",-10.88000f,3.78480f,5.98000f,"mint"),
            ("W121_OUT_SAFETY_CONTROL","W121 OUT","P130","L86_RoboticContactSafetyPLC",-10.66000f,3.78480f,5.98000f,"mint"),
            ("W126_IN_DIGITAL_STATUS","W126 IN","P130","L86_RoboticContactSafetyPLC",-10.88000f,4.00480f,5.98000f,"mint"),
            ("W132_IN_SAFETY_CONTROL","W132 IN","P131","L86_RoboticContactSafetyPLC",-10.96000f,3.56480f,5.98000f,"mint"),
            ("W036_IN_DIGITAL_STATUS","W036 IN","P132","L86_RoboticContactSafetyPLC",-10.82000f,3.56480f,5.98000f,"mint"),
            ("W113_OUT_SAFETY_CONTROL","W113 OUT","P133","L86_ManipulatorEmergencyStopStation",-10.27000f,3.34880f,5.98000f,"mint"),
            ("W003_OUT_POWER_CONTROL","W003 OUT","P134","L86_Protected5VControlSupply",-9.02000f,3.34880f,5.98000f,"red"),
            ("W012_OUT_POWER_CONTROL","W012 OUT","P134","L86_Protected5VControlSupply",-9.24000f,3.34880f,5.98000f,"red"),
            ("W001_OUT_POWER_CONTROL","W001 OUT","P135","L86_Protected5VControlSupply",-8.88000f,3.34880f,5.98000f,"red"),
            ("W010_OUT_POWER_CONTROL","W010 OUT","P135","L86_Protected5VControlSupply",-8.88000f,3.12880f,5.98000f,"red"),
            ("W008_OUT_POWER_CONTROL","W008 OUT","P136","L86_Protected5VControlSupply",-8.74000f,3.34880f,5.98000f,"red"),
            ("W006_OUT_POWER_CONTROL","W006 OUT","P137","L86_Protected5VControlSupply",-8.60000f,3.34880f,5.98000f,"red"),
            ("W004_OUT_POWER_CONTROL","W004 OUT","P138","L86_Protected5VControlSupply",-8.46000f,3.34880f,5.98000f,"red"),
            ("W002_OUT_POWER_CONTROL","W002 OUT","P139","L86_Protected5VControlSupply",-8.32000f,3.34880f,5.98000f,"red"),
            ("W011_OUT_POWER_CONTROL","W011 OUT","P139","L86_Protected5VControlSupply",-8.32000f,3.12880f,5.98000f,"red"),
            ("W009_OUT_POWER_CONTROL","W009 OUT","P140","L86_Protected5VControlSupply",-8.18000f,3.34880f,5.98000f,"red"),
            ("W007_OUT_POWER_CONTROL","W007 OUT","P141","L86_Protected5VControlSupply",-9.02000f,3.45680f,5.98000f,"red"),
            ("W005_OUT_POWER_CONTROL","W005 OUT","P142","L86_Protected5VControlSupply",-8.88000f,3.45680f,5.98000f,"red"),
            ("W134_OUT_SAFETY_CONTROL","W134 OUT","P143","L86_RobotMotorPowerDistributionUnit",-7.77000f,3.34880f,5.98000f,"mint"),
            ("W063_OUT_SAFETY_CONTROL","W063 OUT","P144","L86_IndependentContactSafetyRelay",-6.52000f,3.34880f,5.98000f,"mint"),
            ("W114_OUT_SAFETY_CONTROL","W114 OUT","P144","L86_IndependentContactSafetyRelay",-6.74000f,3.34880f,5.98000f,"mint"),
            ("W076_OUT_DIGITAL_STATUS","W076 OUT","P145","L86_IndependentContactSafetyRelay",-6.38000f,3.34880f,5.98000f,"mint"),
            ("W008_IN_POWER_CONTROL","W008 IN","P146","L86_IndependentContactSafetyRelay",-6.24000f,3.34880f,5.98000f,"red"),
            ("W027_IN_ANALOG_FEEDBACK","W027 IN","P147","L86_SevenAxisJointServoController",-5.27000f,3.34880f,5.98000f,"blue"),
            ("W021_IN_ANALOG_FEEDBACK","W021 IN","P148","L86_SevenAxisJointServoController",-5.13000f,3.34880f,5.98000f,"blue"),
            ("W138_OUT_DATA_STREAM","W138 OUT","P148","L86_SevenAxisJointServoController",-5.13000f,3.12880f,5.98000f,"mint"),
            ("W015_IN_ANALOG_FEEDBACK","W015 IN","P149","L86_SevenAxisJointServoController",-4.99000f,3.34880f,5.98000f,"blue"),
            ("W059_IN_ACTUATOR_COMMAND","W059 IN","P149","L86_SevenAxisJointServoController",-4.77000f,3.12880f,5.98000f,"mint"),
            ("W003_IN_POWER_CONTROL","W003 IN","P150","L86_SevenAxisJointServoController",-4.85000f,3.34880f,5.98000f,"red"),
            ("W034_IN_DATA_STREAM","W034 IN","P150","L86_SevenAxisJointServoController",-4.85000f,2.90880f,5.98000f,"mint"),
            ("W028_OUT_ACTUATOR_COMMAND","W028 OUT","P151","L86_SevenAxisJointServoController",-4.71000f,3.34880f,5.98000f,"mint"),
            ("W022_OUT_ACTUATOR_COMMAND","W022 OUT","P152","L86_SevenAxisJointServoController",-4.57000f,3.34880f,5.98000f,"mint"),
            ("W075_IN_ACTUATOR_COMMAND","W075 IN","P152","L86_SevenAxisJointServoController",-4.57000f,3.12880f,5.98000f,"mint"),
            ("W016_OUT_ACTUATOR_COMMAND","W016 OUT","P153","L86_SevenAxisJointServoController",-4.43000f,3.34880f,5.98000f,"mint"),
            ("W035_OUT_SAFETY_CONTROL","W035 OUT","P154","L86_SevenAxisJointServoController",-5.27000f,3.45680f,5.98000f,"mint"),
            ("W051_IN_SAFETY_CONTROL","W051 IN","P154","L86_SevenAxisJointServoController",-5.49000f,3.45680f,5.98000f,"mint"),
            ("","P155","P155","L86_SevenAxisJointServoController",-5.13000f,3.45680f,5.98000f,"gold"),
            ("W030_IN_ANALOG_FEEDBACK","W030 IN","P156","L86_SevenAxisJointServoController",-4.99000f,3.45680f,5.98000f,"blue"),
            ("W024_IN_ANALOG_FEEDBACK","W024 IN","P157","L86_SevenAxisJointServoController",-4.85000f,3.45680f,5.98000f,"blue"),
            ("W064_OUT_ANALOG_FEEDBACK","W064 OUT","P157","L86_SevenAxisJointServoController",-4.85000f,3.89680f,5.98000f,"blue"),
            ("W018_IN_ANALOG_FEEDBACK","W018 IN","P158","L86_SevenAxisJointServoController",-4.71000f,3.45680f,5.98000f,"blue"),
            ("","P159","P159","L86_SevenAxisJointServoController",-4.57000f,3.45680f,5.98000f,"gold"),
            ("W031_OUT_ACTUATOR_COMMAND","W031 OUT","P160","L86_SevenAxisJointServoController",-4.43000f,3.45680f,5.98000f,"mint"),
            ("W054_IN_DIGITAL_STATUS","W054 IN","P160","L86_SevenAxisJointServoController",-4.21000f,3.45680f,5.98000f,"mint"),
            ("W134_IN_SAFETY_CONTROL","W134 IN","P160","L86_SevenAxisJointServoController",-4.43000f,3.67680f,5.98000f,"mint"),
            ("W025_OUT_ACTUATOR_COMMAND","W025 OUT","P161","L86_SevenAxisJointServoController",-5.27000f,3.56480f,5.98000f,"mint"),
            ("W019_OUT_ACTUATOR_COMMAND","W019 OUT","P162","L86_SevenAxisJointServoController",-5.13000f,3.56480f,5.98000f,"mint"),
            ("W120_IN_SAFETY_CONTROL","W120 IN","P162","L86_SevenAxisJointServoController",-5.13000f,3.78480f,5.98000f,"mint"),
            ("W013_OUT_ACTUATOR_COMMAND","W013 OUT","P163","L86_SevenAxisJointServoController",-4.99000f,3.56480f,5.98000f,"mint"),
            ("","P164","P164","L86_SevenAxisJointServoController",-4.85000f,3.56480f,5.98000f,"gold"),
            ("W033_IN_ANALOG_FEEDBACK","W033 IN","P165","L86_SevenAxisJointServoController",-4.71000f,3.56480f,5.98000f,"blue"),
            ("W062_OUT_SAFETY_CONTROL","W062 OUT","P166","L86_ContactForceSignalConditioner",-4.02000f,3.34880f,5.98000f,"mint"),
            ("W141_OUT_DATA_STREAM","W141 OUT","P166","L86_ContactForceSignalConditioner",-4.02000f,3.12880f,5.98000f,"mint"),
            ("W058_OUT_ANALOG_FEEDBACK","W058 OUT","P167","L86_ContactForceSignalConditioner",-3.88000f,3.34880f,5.98000f,"blue"),
            ("W072_OUT_DIGITAL_STATUS","W072 OUT","P168","L86_ContactForceSignalConditioner",-3.74000f,3.34880f,5.98000f,"mint"),
            ("W061_OUT_DATA_STREAM","W061 OUT","P169","L86_ContactForceSignalConditioner",-3.60000f,3.34880f,5.98000f,"mint"),
            ("W057_IN_ANALOG_FEEDBACK","W057 IN","P170","L86_ContactForceSignalConditioner",-3.46000f,3.34880f,5.98000f,"blue"),
            ("W005_IN_POWER_CONTROL","W005 IN","P171","L86_ContactForceSignalConditioner",-3.32000f,3.34880f,5.98000f,"red"),
            ("W071_IN_ANALOG_FEEDBACK","W071 IN","P171","L86_ContactForceSignalConditioner",-3.32000f,3.12880f,5.98000f,"blue"),
            ("W128_OUT_ANALOG_FEEDBACK","W128 OUT","P171","L86_ContactForceSignalConditioner",-3.32000f,3.56880f,5.98000f,"blue"),
            ("W060_IN_ANALOG_FEEDBACK","W060 IN","P172","L86_ContactForceSignalConditioner",-3.18000f,3.34880f,5.98000f,"blue"),
            ("W039_IN_DATA_STREAM","W039 IN","P173","L86_StereoVisionFusionComputer",-2.77000f,3.34880f,5.98000f,"mint"),
            ("W042_IN_ANALOG_FEEDBACK","W042 IN","P173","L86_StereoVisionFusionComputer",-2.77000f,3.12880f,5.98000f,"blue"),
            ("W045_OUT_DIGITAL_CONTROL","W045 OUT","P173","L86_StereoVisionFusionComputer",-2.99000f,3.12880f,5.98000f,"mint"),
            ("W048_OUT_DATA_STREAM","W048 OUT","P173","L86_StereoVisionFusionComputer",-2.99000f,3.56880f,5.98000f,"mint"),
            ("","P174","P174","L86_StereoVisionFusionComputer",-2.63000f,3.34880f,5.98000f,"gold"),
            ("","P175","P175","L86_StereoVisionFusionComputer",-2.49000f,3.34880f,5.98000f,"gold"),
            ("","P176","P176","L86_StereoVisionFusionComputer",-2.35000f,3.34880f,5.98000f,"gold"),
            ("W004_IN_POWER_CONTROL","W004 IN","P177","L86_StereoVisionFusionComputer",-2.21000f,3.34880f,5.98000f,"red"),
            ("W037_IN_DATA_STREAM","W037 IN","P177","L86_StereoVisionFusionComputer",-2.21000f,3.12880f,5.98000f,"mint"),
            ("W040_IN_DATA_STREAM","W040 IN","P177","L86_StereoVisionFusionComputer",-1.99000f,3.12880f,5.98000f,"mint"),
            ("W043_OUT_DIGITAL_CONTROL","W043 OUT","P177","L86_StereoVisionFusionComputer",-1.99000f,3.56880f,5.98000f,"mint"),
            ("W046_OUT_DIGITAL_CONTROL","W046 OUT","P177","L86_StereoVisionFusionComputer",-2.43000f,3.12880f,5.98000f,"mint"),
            ("","P178","P178","L86_StereoVisionFusionComputer",-2.07000f,3.34880f,5.98000f,"gold"),
            ("","P179","P179","L86_StereoVisionFusionComputer",-1.93000f,3.34880f,5.98000f,"gold"),
            ("W085_IN_DATA_STREAM","W085 IN","P180","L86_StereoVisionFusionComputer",-2.77000f,3.45680f,5.98000f,"mint"),
            ("W142_OUT_DATA_STREAM","W142 OUT","P180","L86_StereoVisionFusionComputer",-2.77000f,3.67680f,5.98000f,"mint"),
            ("W038_IN_DATA_STREAM","W038 IN","P181","L86_StereoVisionFusionComputer",-2.63000f,3.45680f,5.98000f,"mint"),
            ("W041_IN_DATA_STREAM","W041 IN","P181","L86_StereoVisionFusionComputer",-2.41000f,3.67680f,5.98000f,"mint"),
            ("W044_OUT_DIGITAL_CONTROL","W044 OUT","P181","L86_StereoVisionFusionComputer",-2.63000f,3.89680f,5.98000f,"mint"),
            ("W047_OUT_DATA_STREAM","W047 OUT","P181","L86_StereoVisionFusionComputer",-2.19000f,3.67680f,5.98000f,"mint"),
            ("","P182","P182","L86_StereoVisionFusionComputer",-2.49000f,3.45680f,5.98000f,"gold"),
            ("W055_OUT_DATA_STREAM","W055 OUT","P183","L86_StereoVisionFusionComputer",-2.35000f,3.45680f,5.98000f,"mint"),
            ("","P184","P184","L86_StereoVisionFusionComputer",-2.21000f,3.45680f,5.98000f,"gold"),
            ("W093_IN_DIGITAL_STATUS","W093 IN","P185","L86_RadiatorServiceSequencingPLC",-1.52000f,3.34880f,5.98000f,"mint"),
            ("W099_IN_ANALOG_FEEDBACK","W099 IN","P185","L86_RadiatorServiceSequencingPLC",-1.52000f,3.12880f,5.98000f,"blue"),
            ("W108_IN_DIGITAL_STATUS","W108 IN","P185","L86_RadiatorServiceSequencingPLC",-1.74000f,3.12880f,5.98000f,"mint"),
            ("","P186","P186","L86_RadiatorServiceSequencingPLC",-1.38000f,3.34880f,5.98000f,"gold"),
            ("W002_IN_POWER_CONTROL","W002 IN","P187","L86_RadiatorServiceSequencingPLC",-1.24000f,3.34880f,5.98000f,"red"),
            ("W077_OUT_ACTUATOR_COMMAND","W077 OUT","P187","L86_RadiatorServiceSequencingPLC",-1.24000f,3.12880f,5.98000f,"mint"),
            ("W084_OUT_DIGITAL_CONTROL","W084 OUT","P188","L86_RadiatorServiceSequencingPLC",-1.10000f,3.34880f,5.98000f,"mint"),
            ("W150_OUT_DIGITAL_STATUS","W150 OUT","P188","L86_RadiatorServiceSequencingPLC",-0.88000f,3.12880f,5.98000f,"mint"),
            ("W100_IN_ANALOG_FEEDBACK","W100 IN","P189","L86_RadiatorServiceSequencingPLC",-0.96000f,3.34880f,5.98000f,"blue"),
            ("W109_IN_DATA_STREAM","W109 IN","P189","L86_RadiatorServiceSequencingPLC",-0.96000f,2.90880f,5.98000f,"mint"),
            ("W065_IN_DIGITAL_STATUS","W065 IN","P190","L86_RadiatorServiceSequencingPLC",-0.82000f,3.34880f,5.98000f,"mint"),
            ("W095_OUT_ACTUATOR_COMMAND","W095 OUT","P190","L86_RadiatorServiceSequencingPLC",-0.60000f,3.12880f,5.98000f,"mint"),
            ("W137_OUT_DATA_STREAM","W137 OUT","P190","L86_RadiatorServiceSequencingPLC",-0.82000f,3.78880f,5.98000f,"mint"),
            ("W123_IN_SAFETY_CONTROL","W123 IN","P191","L86_RadiatorServiceSequencingPLC",-0.68000f,3.34880f,5.98000f,"mint"),
            ("W079_OUT_ACTUATOR_COMMAND","W079 OUT","P192","L86_RadiatorServiceSequencingPLC",-1.52000f,3.45680f,5.98000f,"mint"),
            ("W101_OUT_ACTUATOR_COMMAND","W101 OUT","P193","L86_RadiatorServiceSequencingPLC",-1.38000f,3.45680f,5.98000f,"mint"),
            ("W110_OUT_ACTUATOR_COMMAND","W110 OUT","P193","L86_RadiatorServiceSequencingPLC",-1.38000f,3.89680f,5.98000f,"mint"),
            ("W096_IN_ANALOG_FEEDBACK","W096 IN","P194","L86_RadiatorServiceSequencingPLC",-1.24000f,3.45680f,5.98000f,"blue"),
            ("","P195","P195","L86_RadiatorServiceSequencingPLC",-1.10000f,3.45680f,5.98000f,"gold"),
            ("W080_IN_DIGITAL_STATUS","W080 IN","P196","L86_RadiatorServiceSequencingPLC",-0.96000f,3.45680f,5.98000f,"mint"),
            ("W090_IN_ANALOG_FEEDBACK","W090 IN","P197","L86_RadiatorServiceSequencingPLC",-0.82000f,3.45680f,5.98000f,"blue"),
            ("W102_IN_ANALOG_FEEDBACK","W102 IN","P197","L86_RadiatorServiceSequencingPLC",-0.38000f,3.67680f,5.98000f,"blue"),
            ("W097_OUT_ACTUATOR_COMMAND","W097 OUT","P198","L86_RadiatorServiceSequencingPLC",-0.68000f,3.45680f,5.98000f,"mint"),
            ("","P199","P199","L86_RadiatorServiceSequencingPLC",-1.52000f,3.56480f,5.98000f,"gold"),
            ("W072_IN_DIGITAL_STATUS","W072 IN","P200","L86_RadiatorServiceSequencingPLC",-1.38000f,3.56480f,5.98000f,"mint"),
            ("W081_OUT_ACTUATOR_COMMAND","W081 OUT","P200","L86_RadiatorServiceSequencingPLC",-1.60000f,3.78480f,5.98000f,"mint"),
            ("","P201","P201","L86_RadiatorServiceSequencingPLC",-1.24000f,3.56480f,5.98000f,"gold"),
            ("W092_OUT_ACTUATOR_COMMAND","W092 OUT","P202","L86_RadiatorServiceSequencingPLC",-1.10000f,3.56480f,5.98000f,"mint"),
            ("W098_OUT_ACTUATOR_COMMAND","W098 OUT","P202","L86_RadiatorServiceSequencingPLC",-1.10000f,3.78480f,5.98000f,"mint"),
            ("","P203","P203","L86_RadiatorServiceSequencingPLC",-0.96000f,3.56480f,5.98000f,"gold"),
            ("W082_IN_ANALOG_FEEDBACK","W082 IN","P204","L86_RadiatorServiceSequencingPLC",-0.82000f,3.56480f,5.98000f,"blue"),
            ("W133_IN_SAFETY_CONTROL","W133 IN","P204","L86_RadiatorServiceSequencingPLC",-0.60000f,3.78480f,5.98000f,"mint"),
            ("W119_OUT_SAFETY_CONTROL","W119 OUT","P205","L86_RadiatorServiceSequencingPLC",-0.68000f,3.56480f,5.98000f,"mint"),
            ("W149_IN_DATA_STREAM","W149 IN","P205","L86_RadiatorServiceSequencingPLC",-0.46000f,3.34480f,5.98000f,"mint"),
            ("","P206","P206","L86_HolographicOrbitalRoboticsSCADA",-0.42000f,8.64880f,-5.87000f,"gold"),
            ("W137_IN_DATA_STREAM","W137 IN","P207","L86_HolographicOrbitalRoboticsSCADA",-0.28000f,8.64880f,-5.87000f,"mint"),
            ("W141_IN_DATA_STREAM","W141 IN","P207","L86_HolographicOrbitalRoboticsSCADA",-0.28000f,8.42880f,-5.87000f,"mint"),
            ("W145_IN_DATA_STREAM","W145 IN","P207","L86_HolographicOrbitalRoboticsSCADA",-0.50000f,8.42880f,-5.87000f,"mint"),
            ("W073_IN_DATA_STREAM","W073 IN","P208","L86_HolographicOrbitalRoboticsSCADA",-0.14000f,8.64880f,-5.87000f,"mint"),
            ("W112_IN_DATA_STREAM","W112 IN","P208","L86_HolographicOrbitalRoboticsSCADA",0.08000f,8.42880f,-5.87000f,"mint"),
            ("W151_IN_DIGITAL_STATUS","W151 IN","P209","L86_HolographicOrbitalRoboticsSCADA",0.00000f,8.64880f,-5.87000f,"mint"),
            ("","P210","P210","L86_HolographicOrbitalRoboticsSCADA",0.14000f,8.64880f,-5.87000f,"gold"),
            ("W138_IN_DATA_STREAM","W138 IN","P211","L86_HolographicOrbitalRoboticsSCADA",0.28000f,8.64880f,-5.87000f,"mint"),
            ("W142_IN_DATA_STREAM","W142 IN","P211","L86_HolographicOrbitalRoboticsSCADA",0.28000f,8.42880f,-5.87000f,"mint"),
            ("W146_IN_DATA_STREAM","W146 IN","P211","L86_HolographicOrbitalRoboticsSCADA",0.50000f,8.42880f,-5.87000f,"mint"),
            ("W074_IN_ANALOG_FEEDBACK","W074 IN","P212","L86_HolographicOrbitalRoboticsSCADA",0.42000f,8.64880f,-5.87000f,"blue"),
            ("W007_IN_POWER_CONTROL","W007 IN","P213","L86_HolographicOrbitalRoboticsSCADA",-0.42000f,8.75680f,-5.87000f,"red"),
            ("W152_IN_DIGITAL_STATUS","W152 IN","P213","L86_HolographicOrbitalRoboticsSCADA",-0.64000f,8.75680f,-5.87000f,"mint"),
            ("","P214","P214","L86_HolographicOrbitalRoboticsSCADA",-0.28000f,8.75680f,-5.87000f,"gold"),
            ("W139_IN_DATA_STREAM","W139 IN","P215","L86_HolographicOrbitalRoboticsSCADA",-0.14000f,8.75680f,-5.87000f,"mint"),
            ("W143_IN_DATA_STREAM","W143 IN","P215","L86_HolographicOrbitalRoboticsSCADA",0.08000f,8.97680f,-5.87000f,"mint"),
            ("W147_IN_DIGITAL_STATUS","W147 IN","P215","L86_HolographicOrbitalRoboticsSCADA",-0.14000f,9.19680f,-5.87000f,"mint"),
            ("W055_IN_DATA_STREAM","W055 IN","P216","L86_HolographicOrbitalRoboticsSCADA",0.00000f,8.75680f,-5.87000f,"mint"),
            ("","P217","P217","L86_HolographicOrbitalRoboticsSCADA",0.14000f,8.75680f,-5.87000f,"gold"),
            ("W135_IN_DIGITAL_STATUS","W135 IN","P218","L86_HolographicOrbitalRoboticsSCADA",0.28000f,8.75680f,-5.87000f,"mint"),
            ("W094_IN_DATA_STREAM","W094 IN","P219","L86_HolographicOrbitalRoboticsSCADA",0.42000f,8.75680f,-5.87000f,"mint"),
            ("W140_IN_DATA_STREAM","W140 IN","P219","L86_HolographicOrbitalRoboticsSCADA",0.64000f,8.75680f,-5.87000f,"mint"),
            ("W144_IN_DATA_STREAM","W144 IN","P219","L86_HolographicOrbitalRoboticsSCADA",0.42000f,8.97680f,-5.87000f,"mint"),
            ("W148_OUT_DATA_STREAM","W148 OUT","P219","L86_HolographicOrbitalRoboticsSCADA",0.64000f,8.97680f,-5.87000f,"mint"),
            ("W056_IN_DATA_STREAM","W056 IN","P220","L86_HolographicOrbitalRoboticsSCADA",-0.42000f,8.86480f,-5.87000f,"mint"),
            ("","P221","P221","L86_HolographicOrbitalRoboticsSCADA",-0.28000f,8.86480f,-5.87000f,"gold"),
            ("W012_IN_POWER_CONTROL","W012 IN","P222","L86_W129CriticalFeedbackPatchStation",-0.27000f,3.34880f,5.98000f,"red"),
            ("W130_OUT_DIGITAL_STATUS","W130 OUT","P223","L86_W129CriticalFeedbackPatchStation",-0.13000f,3.34880f,5.98000f,"mint"),
            ("W128_IN_ANALOG_FEEDBACK","W128 IN","P224","L86_W129CriticalFeedbackPatchStation",0.01000f,3.34880f,5.98000f,"blue"),
            ("W152_OUT_DIGITAL_STATUS","W152 OUT","P224","L86_W129CriticalFeedbackPatchStation",0.23000f,3.34880f,5.98000f,"mint"),
            ("W132_OUT_SAFETY_CONTROL","W132 OUT","P225","L86_HardwareInterlockDiagnosticRack",0.98000f,3.34880f,5.98000f,"mint"),
            ("W127_IN_DIGITAL_STATUS","W127 IN","P226","L86_HardwareInterlockDiagnosticRack",1.12000f,3.34880f,5.98000f,"mint"),
            ("W011_IN_POWER_CONTROL","W011 IN","P227","L86_HardwareInterlockDiagnosticRack",1.26000f,3.34880f,5.98000f,"red"),
            ("W131_IN_DIGITAL_STATUS","W131 IN","P227","L86_HardwareInterlockDiagnosticRack",1.48000f,3.34880f,5.98000f,"mint"),
            ("W136_IN_DIGITAL_STATUS","W136 IN","P228","L86_ServicingMissionCertificationBeacon",-0.42000f,10.14880f,-5.86000f,"mint"),
            ("W150_IN_DIGITAL_STATUS","W150 IN","P229","L86_ServicingMissionCertificationBeacon",-0.28000f,10.14880f,-5.86000f,"mint"),
            ("W151_OUT_DIGITAL_STATUS","W151 OUT","P229","L86_ServicingMissionCertificationBeacon",-0.06000f,10.14880f,-5.86000f,"mint"),
            ("W148_IN_DATA_STREAM","W148 IN","P230","L86_RoboticServicingEventRecorder",2.23000f,3.34880f,5.98000f,"mint"),
            ("W149_OUT_DATA_STREAM","W149 OUT","P230","L86_RoboticServicingEventRecorder",2.01000f,3.34880f,5.98000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => SparkLevel86Wiring.Pins;

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 86")]
        public static void Install()
        {
            SparkLevel86Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel86Process>().rig == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 86/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel86Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.rig==null || control.rig.GetComponentsInChildren<Transform>(true).Count(item=>item.name.StartsWith("ANIM_",StringComparison.Ordinal))!=64 ||
                control.pins.Length!=304 || control.pins.Distinct().Count()!=304 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel86Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 86 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 86] Validated 304 distinct terminals, 152 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_86.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_86_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 86's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 86 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }


        [MenuItem("Tools/Project Spark/Level 86/Fit Service Arena")]
        public static void Fit()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode to fit the service arena.");
            var models=Models.Select((name,index)=>AssetDatabase.LoadAssetAtPath<GameObject>(
                Folder+$"Models/L86_{index:00}_{name}_Fitted.fbx")??throw new InvalidOperationException("Missing module "+name)).ToArray();
            var root=PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                Fit(root,models);
                if(PrefabUtility.SaveAsPrefabAsset(root,Prefab)==null)throw new InvalidOperationException("Could not save Level 86 arena.");
                AssetDatabase.SaveAssets();
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }

        private static void Fit(GameObject root, GameObject[] models)
        {
            var bench = root.transform.Find("Orbital Service Arena");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel86Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 86 is missing its workbench, controls, or scene bindings.");


            const float floor=.425f;
            foreach (var terminal in loop.pins)
                if (terminal.Owner != null)
                    terminal.transform.SetParent(terminal.Owner.transform, true);
            foreach (string name in new[] {"L86_HD_VISUAL_ART", "Equipment terminal panels", "Motion pivots"})
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
            var assembly = Child(bench, "L86_HD_VISUAL_ART", new Vector3(0,floor,-.010f));
            for (var index=0;index<models.Length;index++)
            {
                var model=models[index];
                var instance = Object.Instantiate(model, assembly, false).transform;
                instance.name="L86_"+Models[index];
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L86_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L86_Board" + name];
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
                    "glass"=>"RobotOpticalSensorGlass",
                    _ when mesh.name.Contains("Floor")||mesh.name.Contains("StageBase")=>"SpaceStationServiceFloor",
                    _ when mesh.name.Contains("Hologram")=>"OrbitalThermalHologram",
                    _ when mesh.name.Contains("Coolant")=>"CoolantPortSteelCeramic",
                    _ when mesh.name.Contains("HeatPipe")=>"RadiatorThermalCopperHeatPipe",
                    _ when mesh.name.Contains("Radiator")=>"RadiatorOpticalBlackFins",
                    _ when mesh.name.Contains("Joint")&&key is "tintE8BE52" or "tintD19637"=>"ArmActuatorGoldAnodized",
                    _ when mesh.name.Contains("RobotJoint")=>"MachinedTitaniumLink",
                    _ when mesh.name.Contains("Gripper")=>"RoboticArmCeramicCoating",
                    _ when mesh.name.Contains("Safety")=>"RoboticSafetyRedWarning",
                    _ when mesh.name.Contains("Truss")=>"StationDarkCarbonComposite",
                    _ when mesh.name.Contains("SCADA")=>"AvionicsInstrumentPanel",
                    _=>null
                };
                if(mesh.name.Contains("Panoramic")&&mesh.bounds.size.x>assembly.lossyScale.x*.76f)
                    {key="glass";surface="RobotOpticalSensorGlass";}

                var material = MaterialFor(shader,key,surface);

                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loop.eventText == null || loop.buttons.Length!=26)
            {
                var oldControls = root.transform.Find("Level 86 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel86Builder.BuildControls(root.transform,loop,font);
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
                    // Attach the socket bank to its device frame below when the device moves.
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
                pinData.FindProperty("maxConnections").intValue=SparkLevel86Wiring.Wires.Count(wire=>wire.a==index||wire.b==index);
                pinData.ApplyModifiedPropertiesWithoutUndo();
                Seat(terminal,bench,local,pinLayer,rim,MaterialFor(shader,"void"));
                var tag=Label(mount,font,"Label "+spec.id,spec.label,new Vector3(0,.009f,-.007f),.014f);
                tag.transform.localRotation=Quaternion.Euler(22,0,0);
                loop.pinLabels[index]=tag;
                tag.gameObject.SetActive(false);
            }
            foreach(var body in bench.GetComponentsInChildren<MeshRenderer>(true))
                if(body.name=="Body") body.gameObject.SetActive(false);
            var title=Label(labels,font,"Level name","THE ORBITAL FORGE",new Vector3(0,floor+.018f,-.688f),.023f);
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
            Lamp("Neutral service key",new Vector3(-.52f,.96f,-.31f),new Color(1,.93f,.81f),4.6f,2.6f);
            Lamp("Cool rim fill",new Vector3(.68f,.62f,.21f),new Color(.48f,.77f,1),3.3f,2.05f);
            foreach(var item in assembly.GetComponentsInChildren<Transform>(true))
            {
                int index=item.name.IndexOf("ANIM_",StringComparison.Ordinal);
                if(index>0&&item.name.StartsWith("fitted_",StringComparison.Ordinal)) item.name=item.name[index..];
            }
            loop.rig=assembly.gameObject.AddComponent<SparkLevel86VisualRig>();

            var fx=new SerializedObject(loop.rig);
            fx.FindProperty("artRoot").objectReferenceValue=assembly;
            fx.FindProperty("criticalFaultLamp").objectReferenceValue=Lamp("Contact feedback fault",new Vector3(.52f,.54f,-.37f),new Color(1,.09f,.13f),.8f,.65f);
            fx.FindProperty("thermalHealthyLamp").objectReferenceValue=Lamp("Loop running",new Vector3(.68f,.55f,.11f),new Color(.16f,1,.62f),.9f,.82f);
            fx.FindProperty("contactFeedbackLamp").objectReferenceValue=Lamp("Valid wrench",new Vector3(.15f,.60f,.08f),new Color(.16f,.68f,1),.8f,.68f);
            loop.failedVisual=assembly.Find("L86_FailedThermalRadiatorModule");
            loop.replacementVisual=assembly.Find("L86_ReplacementThermalRadiatorModule");
            var stageMaterial=MaterialFor(shader,"tint43546A","MachinedTitaniumLink");
            for(int i=0;i<2;i++)
            {
                var rail=GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(rail.GetComponent<Collider>());
                rail.name="Service carriage rail extension "+(i+1);
                rail.transform.SetParent(assembly,false);
                rail.transform.localPosition=new Vector3(.058f,.113f,.145f+(i==0?-.048f:.048f));
                rail.transform.localScale=new Vector3(1.18f,.018f,.017f);
                rail.GetComponent<Renderer>().sharedMaterial=stageMaterial;
            }
            foreach(var entry in frames)
            {
                if(!(entry.Key.StartsWith("L86_RobotJoint",StringComparison.Ordinal)||entry.Key is
                    "L86_SixAxisWristForceTorqueSensor" or "L86_AdaptiveRadiatorHandlingGripper" or
                    "L86_DualJawGripperPressureSensorArray" or "L86_AutomaticEndEffectorToolChanger" or
                    "L86_PrecisionTorqueFasteningDriver" or "L86_VacuumCompatibleInspectionProbe" or
                    "L86_FailedThermalRadiatorModule" or "L86_ReplacementThermalRadiatorModule"))continue;
                var module=assembly.Find(entry.Key);
                if(!module)continue;
                Transform motion=entry.Key.StartsWith("L86_RobotJoint",StringComparison.Ordinal)?
                    module.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name.StartsWith("ANIM_ROBOT_",StringComparison.Ordinal)):module;
                if(motion)entry.Value.gameObject.AddComponent<SparkLevel86MountFollower>().Configure(motion);
            }

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
            string materialName=surface==null?"L86 "+key:"L86 "+key+" "+surface;
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
                    throw new InvalidOperationException("Missing Level 86 surface maps for "+surface);
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






















