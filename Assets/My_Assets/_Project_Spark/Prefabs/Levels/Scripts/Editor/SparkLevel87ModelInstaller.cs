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
    public static class SparkLevel87ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_87_Layout/";
        public const string Prefab = Folder + "Level_87_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "GravityQualificationArenaBase","PanoramicOrbitalObservationViewport","RotatingHabitatRingMainRotor","RotatingHabitatSpokeAssembly","StationaryGravityHub","HabitatSegmentA","HabitatSegmentB","HabitatSegmentC","HabitatSegmentD","HabitatSegmentE","HabitatSegmentF","HabitatPressureShellRing","GravityRingStructuralGuideTrack","GravityStationFixedSupportFrame","GravityStationIsolationTruss","HabitatObservationDeck","MagneticBearingStationA","MagneticBearingStationB","MagneticBearingStationC","MagneticBearingStationD","MagneticBearingStationE","MagneticBearingStationF","MagneticBearingStationG","MagneticBearingStationH","PrimaryRadialGapSensor","SecondaryRadialGapSensor","AxialGapDisplacementSensor","MagneticBearingCoilCurrentMonitor","MagneticFluxVisualizationHologram","EmergencyTouchdownBearing","BearingPowerAmplifierCabinet","BearingGapCalibrationStage","SegmentedRimMotorStator","RimDriveRotorMagnetArray","MotorInverterDriveA","MotorInverterDriveB","MotorPhaseCurrentSensor","MotorTorqueEstimator","RotorSpeedOpticalEncoder","SafeSpinUpSequencer","MotorPrechargeContactor","DriveThermalCoolingManifold","MotorAccelerationProfileConsole","RingAngularMomentumCompensationUnit","MultiAxisVibrationSensorArray","VibrationFrequencySpectrumAnalyzer","RotorShaftOrbitProbe","RotorImbalanceVectorDisplay","AdaptiveVibrationDampingController","PrecisionBalanceWeightActuatorA","PrecisionBalanceWeightActuatorB","RotorMassDistributionScanner","ResonanceSweepTestFixture","BearingHousingTemperatureSensor","DynamicRotorHealthRecorder","PrecisionGapAndVibrationSCADA","VibrationFaultCalibrationInjector","ContactlessRotaryPowerCouplerPrimary","ContactlessRotaryPowerCouplerBackup","RotaryOpticalTelemetryLink","RotaryPowerIsolationContactor","HabitatElectricalLoadEmulator","HabitatEmergencyLightingLoad","HabitatThermalControlLoad","RotatingDataQualityMonitor","RotaryPowerTransferMonitor","HabitatLoadDistributionSimulator","GravityHabitatEnvironmentHologram","OrbitalRingPowerBudgetAnalyzer","GravitySafetySupervisoryPLC","MagneticBearingSafetyPLC","IndependentOverspeedProtector","RingEmergencyStopStation","Protected5VControlSupply","MagneticBearingControlPLC","RotorMotorControlPLC","HabitatPowerDistributionPLC","RadialSensorDriftFaultInjector","BearingAmplifierHeatFaultInjector","MassImbalanceFaultInjector","W137CriticalFeedbackPatchStation","GravityEngineeringMasterSCADA","RingCertificationBeacon","GravityMachineEventRecorder","GravityEngineInspectionCamera","StationSafetyIsolationRelay"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .0075f;
        private const float SocketDiameter = .0086f;

        private static readonly (string id, string label, string anchor, string model, float x, float y, float z, string color)[] Sockets =
        {
            ("W137_OUT_ANALOG_FEEDBACK","W137 OUT","P019","L87_PrimaryRadialGapSensor",-12.51000f,1.56100f,2.60000f,"amber"),
            ("W029_OUT_ANALOG_FEEDBACK","W029 OUT","P019","L87_PrimaryRadialGapSensor",-12.29000f,1.56100f,2.60000f,"blue"),
            ("W131_IN_DIGITAL_CONTROL","W131 IN","P019","L87_PrimaryRadialGapSensor",-12.51000f,1.34100f,2.60000f,"mint"),
            ("W137_IN_ANALOG_FEEDBACK","W137 IN","P135","L87_MagneticBearingSafetyPLC",-10.61500f,3.56100f,7.60000f,"amber"),
            ("W149_OUT_DIGITAL_STATUS","W149 OUT","P135","L87_MagneticBearingSafetyPLC",-10.61500f,3.34100f,7.60000f,"mint"),
            ("","P001","P001","L87_MagneticBearingStationA",5.51000f,7.06100f,-6.44000f,"gold"),
            ("W013_IN_ACTUATOR_COMMAND","W013 IN","P002","L87_MagneticBearingStationA",5.59500f,7.06100f,-6.44000f,"mint"),
            ("W014_OUT_ANALOG_FEEDBACK","W014 OUT","P002","L87_MagneticBearingStationA",5.81500f,7.06100f,-6.44000f,"blue"),
            ("","P003","P003","L87_MagneticBearingStationB",3.79657f,11.19757f,-6.44000f,"gold"),
            ("W015_IN_ACTUATOR_COMMAND","W015 IN","P004","L87_MagneticBearingStationB",3.88157f,11.19757f,-6.44000f,"mint"),
            ("W016_OUT_ANALOG_FEEDBACK","W016 OUT","P004","L87_MagneticBearingStationB",4.10157f,11.19757f,-6.44000f,"blue"),
            ("","P005","P005","L87_MagneticBearingStationC",-0.34000f,12.91100f,-6.44000f,"gold"),
            ("W017_IN_ACTUATOR_COMMAND","W017 IN","P006","L87_MagneticBearingStationC",-0.25500f,12.91100f,-6.44000f,"mint"),
            ("W018_OUT_ANALOG_FEEDBACK","W018 OUT","P006","L87_MagneticBearingStationC",-0.03500f,12.91100f,-6.44000f,"blue"),
            ("","P007","P007","L87_MagneticBearingStationD",-4.47657f,11.19757f,-6.44000f,"gold"),
            ("W019_IN_ACTUATOR_COMMAND","W019 IN","P008","L87_MagneticBearingStationD",-4.39157f,11.19757f,-6.44000f,"mint"),
            ("W020_OUT_ANALOG_FEEDBACK","W020 OUT","P008","L87_MagneticBearingStationD",-4.17157f,11.19757f,-6.44000f,"blue"),
            ("","P009","P009","L87_MagneticBearingStationE",-6.19000f,7.06100f,-6.44000f,"gold"),
            ("W021_IN_ACTUATOR_COMMAND","W021 IN","P010","L87_MagneticBearingStationE",-6.10500f,7.06100f,-6.44000f,"mint"),
            ("W022_OUT_ANALOG_FEEDBACK","W022 OUT","P010","L87_MagneticBearingStationE",-5.88500f,7.06100f,-6.44000f,"blue"),
            ("","P011","P011","L87_MagneticBearingStationF",-4.47657f,2.92443f,-6.44000f,"gold"),
            ("W023_IN_ACTUATOR_COMMAND","W023 IN","P012","L87_MagneticBearingStationF",-4.39157f,2.92443f,-6.44000f,"mint"),
            ("W024_OUT_ANALOG_FEEDBACK","W024 OUT","P012","L87_MagneticBearingStationF",-4.17157f,2.92443f,-6.44000f,"blue"),
            ("","P013","P013","L87_MagneticBearingStationG",-0.34000f,1.21100f,-6.44000f,"gold"),
            ("W025_IN_ACTUATOR_COMMAND","W025 IN","P014","L87_MagneticBearingStationG",-0.25500f,1.21100f,-6.44000f,"mint"),
            ("W026_OUT_ANALOG_FEEDBACK","W026 OUT","P014","L87_MagneticBearingStationG",-0.03500f,1.21100f,-6.44000f,"blue"),
            ("","P015","P015","L87_MagneticBearingStationH",3.79657f,2.92443f,-6.44000f,"gold"),
            ("W027_IN_ACTUATOR_COMMAND","W027 IN","P016","L87_MagneticBearingStationH",3.88157f,2.92443f,-6.44000f,"mint"),
            ("W028_OUT_ANALOG_FEEDBACK","W028 OUT","P016","L87_MagneticBearingStationH",4.10157f,2.92443f,-6.44000f,"blue"),
            ("","P017","P017","L87_PrimaryRadialGapSensor",-12.72000f,1.56100f,2.60000f,"gold"),
            ("","P018","P018","L87_PrimaryRadialGapSensor",-12.61500f,1.56100f,2.60000f,"gold"),
            ("W030_OUT_ANALOG_FEEDBACK","W030 OUT","P020","L87_SecondaryRadialGapSensor",-11.42000f,1.56100f,2.60000f,"blue"),
            ("W135_OUT_ANALOG_FEEDBACK","W135 OUT","P020","L87_SecondaryRadialGapSensor",-11.64000f,1.56100f,2.60000f,"blue"),
            ("","P021","P021","L87_SecondaryRadialGapSensor",-11.31500f,1.56100f,2.60000f,"gold"),
            ("W031_OUT_ANALOG_FEEDBACK","W031 OUT","P022","L87_AxialGapDisplacementSensor",-10.12000f,1.56100f,2.60000f,"blue"),
            ("W032_OUT_ANALOG_FEEDBACK","W032 OUT","P023","L87_MagneticBearingCoilCurrentMonitor",-8.82000f,1.56100f,2.60000f,"blue"),
            ("W038_IN_DATA_STREAM","W038 IN","P024","L87_MagneticFluxVisualizationHologram",-2.52000f,7.31100f,-3.70000f,"mint"),
            ("W134_IN_SAFETY_CONTROL","W134 IN","P025","L87_EmergencyTouchdownBearing",-12.12000f,4.01100f,-5.20000f,"mint"),
            ("W035_IN_SAFETY_CONTROL","W035 IN","P026","L87_EmergencyTouchdownBearing",-12.01500f,4.01100f,-5.20000f,"mint"),
            ("W036_OUT_DIGITAL_STATUS","W036 OUT","P026","L87_EmergencyTouchdownBearing",-11.79500f,4.01100f,-5.20000f,"mint"),
            ("W034_OUT_ANALOG_FEEDBACK","W034 OUT","P027","L87_BearingPowerAmplifierCabinet",-7.52000f,1.61100f,2.60000f,"blue"),
            ("W132_IN_DIGITAL_CONTROL","W132 IN","P027","L87_BearingPowerAmplifierCabinet",-7.74000f,1.61100f,2.60000f,"mint"),
            ("W033_IN_ACTUATOR_COMMAND","W033 IN","P028","L87_BearingPowerAmplifierCabinet",-7.41500f,1.61100f,2.60000f,"mint"),
            ("W104_OUT_ANALOG_FEEDBACK","W104 OUT","P028","L87_BearingPowerAmplifierCabinet",-7.41500f,1.39100f,2.60000f,"blue"),
            ("W116_OUT_DATA_STREAM","W116 OUT","P028","L87_BearingPowerAmplifierCabinet",-7.41500f,1.83100f,2.60000f,"mint"),
            ("W103_IN_ANALOG_FEEDBACK","W103 IN","P029","L87_BearingPowerAmplifierCabinet",-7.31000f,1.61100f,2.60000f,"blue"),
            ("W008_IN_POWER_CONTROL","W008 IN","P030","L87_BearingPowerAmplifierCabinet",-7.20500f,1.61100f,2.60000f,"red"),
            ("","P031","P031","L87_BearingPowerAmplifierCabinet",-7.10000f,1.61100f,2.60000f,"gold"),
            ("W037_OUT_ANALOG_FEEDBACK","W037 OUT","P032","L87_BearingGapCalibrationStage",-6.22000f,1.61100f,2.60000f,"blue"),
            ("","P033","P033","L87_SegmentedRimMotorStator",-0.42000f,7.06100f,-6.69000f,"gold"),
            ("W041_IN_ACTUATOR_COMMAND","W041 IN","P034","L87_SegmentedRimMotorStator",-0.31500f,7.06100f,-6.69000f,"mint"),
            ("W042_IN_ACTUATOR_COMMAND","W042 IN","P034","L87_SegmentedRimMotorStator",-0.09500f,7.06100f,-6.69000f,"mint"),
            ("W043_OUT_ANALOG_FEEDBACK","W043 OUT","P035","L87_RimDriveRotorMagnetArray",-0.42000f,7.06100f,-6.99000f,"blue"),
            ("","P036","P036","L87_MotorInverterDriveA",-4.92000f,1.56100f,2.60000f,"gold"),
            ("W009_IN_POWER_CONTROL","W009 IN","P037","L87_MotorInverterDriveA",-4.81500f,1.56100f,2.60000f,"red"),
            ("W039_IN_ACTUATOR_COMMAND","W039 IN","P038","L87_MotorInverterDriveA",-4.71000f,1.56100f,2.60000f,"mint"),
            ("W049_IN_ANALOG_FEEDBACK","W049 IN","P038","L87_MotorInverterDriveA",-4.71000f,1.34100f,2.60000f,"blue"),
            ("W059_OUT_DIGITAL_STATUS","W059 OUT","P038","L87_MotorInverterDriveA",-4.71000f,1.78100f,2.60000f,"mint"),
            ("W041_OUT_ACTUATOR_COMMAND","W041 OUT","P039","L87_MotorInverterDriveA",-4.60500f,1.56100f,2.60000f,"mint"),
            ("W055_OUT_ANALOG_FEEDBACK","W055 OUT","P039","L87_MotorInverterDriveA",-4.38500f,1.56100f,2.60000f,"blue"),
            ("W042_OUT_ACTUATOR_COMMAND","W042 OUT","P040","L87_MotorInverterDriveB",-3.62000f,1.56100f,2.60000f,"mint"),
            ("W056_OUT_ANALOG_FEEDBACK","W056 OUT","P040","L87_MotorInverterDriveB",-3.84000f,1.56100f,2.60000f,"blue"),
            ("","P041","P041","L87_MotorInverterDriveB",-3.51500f,1.56100f,2.60000f,"gold"),
            ("W010_IN_POWER_CONTROL","W010 IN","P042","L87_MotorInverterDriveB",-3.41000f,1.56100f,2.60000f,"red"),
            ("W040_IN_ACTUATOR_COMMAND","W040 IN","P043","L87_MotorInverterDriveB",-3.30500f,1.56100f,2.60000f,"mint"),
            ("W050_IN_ANALOG_FEEDBACK","W050 IN","P043","L87_MotorInverterDriveB",-3.08500f,1.56100f,2.60000f,"blue"),
            ("W060_OUT_DIGITAL_STATUS","W060 OUT","P043","L87_MotorInverterDriveB",-3.30500f,1.34100f,2.60000f,"mint"),
            ("W055_IN_ANALOG_FEEDBACK","W055 IN","P044","L87_MotorPhaseCurrentSensor",-2.32000f,1.56100f,2.60000f,"blue"),
            ("W056_IN_ANALOG_FEEDBACK","W056 IN","P044","L87_MotorPhaseCurrentSensor",-2.54000f,1.56100f,2.60000f,"blue"),
            ("W045_OUT_ANALOG_FEEDBACK","W045 OUT","P045","L87_MotorPhaseCurrentSensor",-2.21500f,1.56100f,2.60000f,"blue"),
            ("W046_OUT_ANALOG_FEEDBACK","W046 OUT","P046","L87_MotorTorqueEstimator",-1.02000f,1.56100f,2.60000f,"blue"),
            ("W126_OUT_ANALOG_FEEDBACK","W126 OUT","P047","L87_RotorSpeedOpticalEncoder",0.28000f,1.56100f,2.60000f,"blue"),
            ("W043_IN_ANALOG_FEEDBACK","W043 IN","P048","L87_RotorSpeedOpticalEncoder",0.38500f,1.56100f,2.60000f,"blue"),
            ("W058_OUT_ANALOG_FEEDBACK","W058 OUT","P048","L87_RotorSpeedOpticalEncoder",0.38500f,1.34100f,2.60000f,"blue"),
            ("W044_OUT_ANALOG_FEEDBACK","W044 OUT","P049","L87_RotorSpeedOpticalEncoder",0.49000f,1.56100f,2.60000f,"blue"),
            ("W057_IN_DIGITAL_CONTROL","W057 IN","P050","L87_SafeSpinUpSequencer",1.58000f,1.56100f,2.60000f,"mint"),
            ("W058_IN_ANALOG_FEEDBACK","W058 IN","P050","L87_SafeSpinUpSequencer",1.36000f,1.56100f,2.60000f,"blue"),
            ("W051_OUT_DIGITAL_CONTROL","W051 OUT","P051","L87_SafeSpinUpSequencer",1.68500f,1.56100f,2.60000f,"mint"),
            ("W047_IN_ACTUATOR_COMMAND","W047 IN","P052","L87_MotorPrechargeContactor",2.88000f,1.56100f,2.60000f,"mint"),
            ("W048_OUT_ANALOG_FEEDBACK","W048 OUT","P052","L87_MotorPrechargeContactor",2.66000f,1.56100f,2.60000f,"blue"),
            ("","P053","P053","L87_DriveThermalCoolingManifold",4.18000f,1.56100f,2.60000f,"gold"),
            ("W049_OUT_ANALOG_FEEDBACK","W049 OUT","P054","L87_DriveThermalCoolingManifold",4.28500f,1.56100f,2.60000f,"blue"),
            ("W050_OUT_ANALOG_FEEDBACK","W050 OUT","P054","L87_DriveThermalCoolingManifold",4.50500f,1.56100f,2.60000f,"blue"),
            ("W105_OUT_ANALOG_FEEDBACK","W105 OUT","P054","L87_DriveThermalCoolingManifold",4.28500f,1.34100f,2.60000f,"blue"),
            ("W052_OUT_DATA_STREAM","W052 OUT","P055","L87_MotorAccelerationProfileConsole",5.48000f,1.56100f,2.60000f,"mint"),
            ("W053_OUT_ANALOG_FEEDBACK","W053 OUT","P056","L87_RingAngularMomentumCompensationUnit",6.78000f,1.56100f,2.60000f,"blue"),
            ("W054_IN_ACTUATOR_COMMAND","W054 IN","P056","L87_RingAngularMomentumCompensationUnit",6.56000f,1.56100f,2.60000f,"mint"),
            ("W061_OUT_ANALOG_FEEDBACK","W061 OUT","P057","L87_MultiAxisVibrationSensorArray",-12.72000f,1.56100f,5.30000f,"blue"),
            ("W075_OUT_ANALOG_FEEDBACK","W075 OUT","P057","L87_MultiAxisVibrationSensorArray",-12.94000f,1.56100f,5.30000f,"blue"),
            ("W078_OUT_SAFETY_CONTROL","W078 OUT","P058","L87_VibrationFrequencySpectrumAnalyzer",-11.42000f,1.56100f,5.30000f,"mint"),
            ("W061_IN_ANALOG_FEEDBACK","W061 IN","P059","L87_VibrationFrequencySpectrumAnalyzer",-11.31500f,1.56100f,5.30000f,"blue"),
            ("W077_IN_DIGITAL_STATUS","W077 IN","P060","L87_VibrationFrequencySpectrumAnalyzer",-11.21000f,1.56100f,5.30000f,"mint"),
            ("W063_OUT_DATA_STREAM","W063 OUT","P061","L87_VibrationFrequencySpectrumAnalyzer",-11.10500f,1.56100f,5.30000f,"mint"),
            ("W069_OUT_DATA_STREAM","W069 OUT","P061","L87_VibrationFrequencySpectrumAnalyzer",-11.10500f,1.34100f,5.30000f,"mint"),
            ("","P062","P062","L87_VibrationFrequencySpectrumAnalyzer",-11.00000f,1.56100f,5.30000f,"gold"),
            ("W062_IN_ANALOG_FEEDBACK","W062 IN","P063","L87_VibrationFrequencySpectrumAnalyzer",-10.89500f,1.56100f,5.30000f,"blue"),
            ("W068_IN_DIGITAL_CONTROL","W068 IN","P063","L87_VibrationFrequencySpectrumAnalyzer",-10.67500f,1.56100f,5.30000f,"mint"),
            ("W146_OUT_DATA_STREAM","W146 OUT","P063","L87_VibrationFrequencySpectrumAnalyzer",-10.89500f,1.34100f,5.30000f,"mint"),
            ("W062_OUT_ANALOG_FEEDBACK","W062 OUT","P064","L87_RotorShaftOrbitProbe",-10.12000f,1.56100f,5.30000f,"blue"),
            ("","P065","P065","L87_RotorImbalanceVectorDisplay",-8.82000f,1.56100f,5.30000f,"gold"),
            ("W067_IN_ANALOG_FEEDBACK","W067 IN","P066","L87_RotorImbalanceVectorDisplay",-8.71500f,1.56100f,5.30000f,"blue"),
            ("","P067","P067","L87_RotorImbalanceVectorDisplay",-8.61000f,1.56100f,5.30000f,"gold"),
            ("W063_IN_DATA_STREAM","W063 IN","P068","L87_RotorImbalanceVectorDisplay",-8.50500f,1.56100f,5.30000f,"mint"),
            ("W064_OUT_DATA_STREAM","W064 OUT","P068","L87_RotorImbalanceVectorDisplay",-8.28500f,1.56100f,5.30000f,"mint"),
            ("W070_OUT_ACTUATOR_COMMAND","W070 OUT","P068","L87_RotorImbalanceVectorDisplay",-8.50500f,1.34100f,5.30000f,"mint"),
            ("W071_OUT_ACTUATOR_COMMAND","W071 OUT","P068","L87_RotorImbalanceVectorDisplay",-8.50500f,1.78100f,5.30000f,"mint"),
            ("W066_OUT_SAFETY_CONTROL","W066 OUT","P069","L87_AdaptiveVibrationDampingController",-7.52000f,1.56100f,5.30000f,"mint"),
            ("W064_IN_DATA_STREAM","W064 IN","P070","L87_AdaptiveVibrationDampingController",-7.41500f,1.56100f,5.30000f,"mint"),
            ("W065_OUT_ACTUATOR_COMMAND","W065 OUT","P071","L87_AdaptiveVibrationDampingController",-7.31000f,1.56100f,5.30000f,"mint"),
            ("W080_OUT_DATA_STREAM","W080 OUT","P071","L87_AdaptiveVibrationDampingController",-7.09000f,1.56100f,5.30000f,"mint"),
            ("W070_IN_ACTUATOR_COMMAND","W070 IN","P072","L87_PrecisionBalanceWeightActuatorA",-6.92000f,7.31100f,-4.59000f,"mint"),
            ("W072_OUT_ANALOG_FEEDBACK","W072 OUT","P072","L87_PrecisionBalanceWeightActuatorA",-7.14000f,7.31100f,-4.59000f,"blue"),
            ("W071_IN_ACTUATOR_COMMAND","W071 IN","P073","L87_PrecisionBalanceWeightActuatorB",6.08000f,7.31100f,-4.59000f,"mint"),
            ("W073_OUT_ANALOG_FEEDBACK","W073 OUT","P073","L87_PrecisionBalanceWeightActuatorB",5.86000f,7.31100f,-4.59000f,"blue"),
            ("W109_IN_DIGITAL_CONTROL","W109 IN","P074","L87_RotorMassDistributionScanner",-6.22000f,1.56100f,5.30000f,"mint"),
            ("W110_OUT_DATA_STREAM","W110 OUT","P074","L87_RotorMassDistributionScanner",-6.44000f,1.56100f,5.30000f,"mint"),
            ("W133_IN_DIGITAL_CONTROL","W133 IN","P075","L87_RotorMassDistributionScanner",-6.11500f,1.56100f,5.30000f,"mint"),
            ("","P076","P076","L87_RotorMassDistributionScanner",-6.01000f,1.56100f,5.30000f,"gold"),
            ("W067_OUT_ANALOG_FEEDBACK","W067 OUT","P077","L87_RotorMassDistributionScanner",-5.90500f,1.56100f,5.30000f,"blue"),
            ("W122_OUT_DATA_STREAM","W122 OUT","P077","L87_RotorMassDistributionScanner",-5.68500f,1.56100f,5.30000f,"mint"),
            ("W068_OUT_DIGITAL_CONTROL","W068 OUT","P078","L87_ResonanceSweepTestFixture",-4.92000f,1.56100f,5.30000f,"mint"),
            ("W069_IN_DATA_STREAM","W069 IN","P078","L87_ResonanceSweepTestFixture",-5.14000f,1.56100f,5.30000f,"mint"),
            ("W074_OUT_ANALOG_FEEDBACK","W074 OUT","P079","L87_BearingHousingTemperatureSensor",-3.62000f,1.56100f,5.30000f,"blue"),
            ("W103_OUT_ANALOG_FEEDBACK","W103 OUT","P079","L87_BearingHousingTemperatureSensor",-3.84000f,1.56100f,5.30000f,"blue"),
            ("W074_IN_ANALOG_FEEDBACK","W074 IN","P080","L87_DynamicRotorHealthRecorder",-2.32000f,1.56100f,5.30000f,"blue"),
            ("W073_IN_ANALOG_FEEDBACK","W073 IN","P081","L87_DynamicRotorHealthRecorder",-2.21500f,1.56100f,5.30000f,"blue"),
            ("W072_IN_ANALOG_FEEDBACK","W072 IN","P082","L87_DynamicRotorHealthRecorder",-2.11000f,1.56100f,5.30000f,"blue"),
            ("W076_OUT_DATA_STREAM","W076 OUT","P083","L87_DynamicRotorHealthRecorder",-2.00500f,1.56100f,5.30000f,"mint"),
            ("W075_IN_ANALOG_FEEDBACK","W075 IN","P084","L87_DynamicRotorHealthRecorder",-1.90000f,1.56100f,5.30000f,"blue"),
            ("W079_OUT_DATA_STREAM","W079 OUT","P084","L87_DynamicRotorHealthRecorder",-1.68000f,1.56100f,5.30000f,"mint"),
            ("W156_OUT_DATA_STREAM","W156 OUT","P084","L87_DynamicRotorHealthRecorder",-1.90000f,1.34100f,5.30000f,"mint"),
            ("W076_IN_DATA_STREAM","W076 IN","P085","L87_PrecisionGapAndVibrationSCADA",-1.02000f,1.56100f,5.30000f,"mint"),
            ("W077_OUT_DIGITAL_STATUS","W077 OUT","P086","L87_VibrationFaultCalibrationInjector",0.28000f,1.56100f,5.30000f,"mint"),
            ("W083_OUT_ANALOG_FEEDBACK","W083 OUT","P087","L87_ContactlessRotaryPowerCouplerPrimary",7.93000f,2.66100f,1.91000f,"blue"),
            ("W081_IN_DIGITAL_CONTROL","W081 IN","P088","L87_ContactlessRotaryPowerCouplerPrimary",8.03500f,2.66100f,1.91000f,"mint"),
            ("W117_OUT_ANALOG_FEEDBACK","W117 OUT","P088","L87_ContactlessRotaryPowerCouplerPrimary",8.25500f,2.66100f,1.91000f,"blue"),
            ("W082_IN_DIGITAL_CONTROL","W082 IN","P089","L87_ContactlessRotaryPowerCouplerBackup",9.28000f,2.66100f,1.91000f,"mint"),
            ("W118_OUT_ANALOG_FEEDBACK","W118 OUT","P089","L87_ContactlessRotaryPowerCouplerBackup",9.06000f,2.66100f,1.91000f,"blue"),
            ("W084_OUT_ANALOG_FEEDBACK","W084 OUT","P090","L87_ContactlessRotaryPowerCouplerBackup",9.38500f,2.66100f,1.91000f,"blue"),
            ("W085_OUT_DATA_STREAM","W085 OUT","P091","L87_RotaryOpticalTelemetryLink",10.63000f,2.66100f,2.10000f,"mint"),
            ("W100_OUT_DATA_STREAM","W100 OUT","P091","L87_RotaryOpticalTelemetryLink",10.41000f,2.66100f,2.10000f,"mint"),
            ("","P092","P092","L87_RotaryPowerIsolationContactor",11.98000f,2.66100f,2.10000f,"gold"),
            ("W087_IN_ACTUATOR_COMMAND","W087 IN","P093","L87_RotaryPowerIsolationContactor",12.08500f,2.66100f,2.10000f,"mint"),
            ("W088_OUT_ANALOG_FEEDBACK","W088 OUT","P093","L87_RotaryPowerIsolationContactor",12.30500f,2.66100f,2.10000f,"blue"),
            ("W119_OUT_DIGITAL_STATUS","W119 OUT","P093","L87_RotaryPowerIsolationContactor",12.08500f,2.44100f,2.10000f,"mint"),
            ("W097_IN_DIGITAL_CONTROL","W097 IN","P094","L87_HabitatElectricalLoadEmulator",7.93000f,1.51100f,5.05000f,"mint"),
            ("W106_OUT_ANALOG_FEEDBACK","W106 OUT","P094","L87_HabitatElectricalLoadEmulator",7.71000f,1.51100f,5.05000f,"blue"),
            ("W089_OUT_ANALOG_FEEDBACK","W089 OUT","P095","L87_HabitatElectricalLoadEmulator",8.03500f,1.51100f,5.05000f,"blue"),
            ("W090_OUT_SAFETY_CONTROL","W090 OUT","P096","L87_HabitatEmergencyLightingLoad",9.28000f,1.51100f,5.05000f,"mint"),
            ("W115_OUT_DATA_STREAM","W115 OUT","P097","L87_HabitatEmergencyLightingLoad",9.38500f,1.51100f,5.05000f,"mint"),
            ("W098_IN_SAFETY_CONTROL","W098 IN","P098","L87_HabitatEmergencyLightingLoad",9.49000f,1.51100f,5.05000f,"mint"),
            ("W107_OUT_ANALOG_FEEDBACK","W107 OUT","P098","L87_HabitatEmergencyLightingLoad",9.71000f,1.51100f,5.05000f,"blue"),
            ("W099_IN_DIGITAL_CONTROL","W099 IN","P099","L87_HabitatThermalControlLoad",10.63000f,1.51100f,5.05000f,"mint"),
            ("W108_OUT_ANALOG_FEEDBACK","W108 OUT","P099","L87_HabitatThermalControlLoad",10.41000f,1.51100f,5.05000f,"blue"),
            ("W114_OUT_DATA_STREAM","W114 OUT","P099","L87_HabitatThermalControlLoad",10.63000f,1.29100f,5.05000f,"mint"),
            ("W091_OUT_ANALOG_FEEDBACK","W091 OUT","P100","L87_HabitatThermalControlLoad",10.73500f,1.51100f,5.05000f,"blue"),
            ("","P101","P101","L87_HabitatThermalControlLoad",10.84000f,1.51100f,5.05000f,"gold"),
            ("W085_IN_DATA_STREAM","W085 IN","P102","L87_RotatingDataQualityMonitor",11.98000f,1.51100f,5.05000f,"mint"),
            ("W086_OUT_DATA_STREAM","W086 OUT","P102","L87_RotatingDataQualityMonitor",11.76000f,1.51100f,5.05000f,"mint"),
            ("W118_IN_ANALOG_FEEDBACK","W118 IN","P103","L87_RotaryPowerTransferMonitor",7.93000f,3.76100f,5.05000f,"blue"),
            ("W147_OUT_DATA_STREAM","W147 OUT","P103","L87_RotaryPowerTransferMonitor",7.71000f,3.76100f,5.05000f,"mint"),
            ("W117_IN_ANALOG_FEEDBACK","W117 IN","P104","L87_RotaryPowerTransferMonitor",8.03500f,3.76100f,5.05000f,"blue"),
            ("W094_OUT_ANALOG_FEEDBACK","W094 OUT","P105","L87_RotaryPowerTransferMonitor",8.14000f,3.76100f,5.05000f,"blue"),
            ("W093_IN_DIGITAL_CONTROL","W093 IN","P106","L87_RotaryPowerTransferMonitor",8.24500f,3.76100f,5.05000f,"mint"),
            ("W113_OUT_DATA_STREAM","W113 OUT","P107","L87_RotaryPowerTransferMonitor",8.35000f,3.76100f,5.05000f,"mint"),
            ("W119_IN_DIGITAL_STATUS","W119 IN","P107","L87_RotaryPowerTransferMonitor",8.57000f,3.76100f,5.05000f,"mint"),
            ("","P108","P108","L87_HabitatLoadDistributionSimulator",9.28000f,3.76100f,5.05000f,"gold"),
            ("W109_OUT_DIGITAL_CONTROL","W109 OUT","P109","L87_HabitatLoadDistributionSimulator",9.38500f,3.76100f,5.05000f,"mint"),
            ("W092_OUT_ANALOG_FEEDBACK","W092 OUT","P110","L87_HabitatLoadDistributionSimulator",9.49000f,3.76100f,5.05000f,"blue"),
            ("W101_OUT_DATA_STREAM","W101 OUT","P110","L87_HabitatLoadDistributionSimulator",9.71000f,3.76100f,5.05000f,"mint"),
            ("W110_IN_DATA_STREAM","W110 IN","P110","L87_HabitatLoadDistributionSimulator",9.49000f,3.54100f,5.05000f,"mint"),
            ("W102_OUT_DATA_STREAM","W102 OUT","P111","L87_GravityHabitatEnvironmentHologram",1.98000f,8.31100f,-3.30000f,"mint"),
            ("W120_OUT_DATA_STREAM","W120 OUT","P112","L87_GravityHabitatEnvironmentHologram",2.08500f,8.31100f,-3.30000f,"mint"),
            ("W157_OUT_DATA_STREAM","W157 OUT","P112","L87_GravityHabitatEnvironmentHologram",2.30500f,8.31100f,-3.30000f,"mint"),
            ("W107_IN_ANALOG_FEEDBACK","W107 IN","P113","L87_OrbitalRingPowerBudgetAnalyzer",10.63000f,3.76100f,5.05000f,"blue"),
            ("W112_OUT_DATA_STREAM","W112 OUT","P113","L87_OrbitalRingPowerBudgetAnalyzer",10.41000f,3.76100f,5.05000f,"mint"),
            ("W148_OUT_DATA_STREAM","W148 OUT","P113","L87_OrbitalRingPowerBudgetAnalyzer",10.63000f,3.54100f,5.05000f,"mint"),
            ("W096_IN_DIGITAL_CONTROL","W096 IN","P114","L87_OrbitalRingPowerBudgetAnalyzer",10.73500f,3.76100f,5.05000f,"mint"),
            ("W101_IN_DATA_STREAM","W101 IN","P115","L87_OrbitalRingPowerBudgetAnalyzer",10.84000f,3.76100f,5.05000f,"mint"),
            ("W106_IN_ANALOG_FEEDBACK","W106 IN","P116","L87_OrbitalRingPowerBudgetAnalyzer",10.94500f,3.76100f,5.05000f,"blue"),
            ("W111_OUT_DATA_STREAM","W111 OUT","P116","L87_OrbitalRingPowerBudgetAnalyzer",10.94500f,3.54100f,5.05000f,"mint"),
            ("W095_OUT_DATA_STREAM","W095 OUT","P117","L87_OrbitalRingPowerBudgetAnalyzer",11.05000f,3.76100f,5.05000f,"mint"),
            ("W108_IN_ANALOG_FEEDBACK","W108 IN","P117","L87_OrbitalRingPowerBudgetAnalyzer",11.05000f,3.98100f,5.05000f,"blue"),
            ("W121_OUT_DATA_STREAM","W121 OUT","P118","L87_OrbitalRingPowerBudgetAnalyzer",11.15500f,3.76100f,5.05000f,"mint"),
            ("","P119","P119","L87_OrbitalRingPowerBudgetAnalyzer",11.26000f,3.76100f,5.05000f,"gold"),
            ("W053_IN_ANALOG_FEEDBACK","W053 IN","P120","L87_GravitySafetySupervisoryPLC",-12.12000f,3.56100f,7.60000f,"blue"),
            ("W078_IN_SAFETY_CONTROL","W078 IN","P120","L87_GravitySafetySupervisoryPLC",-12.34000f,3.56100f,7.60000f,"mint"),
            ("W127_OUT_SAFETY_CONTROL","W127 OUT","P120","L87_GravitySafetySupervisoryPLC",-12.12000f,3.34100f,7.60000f,"mint"),
            ("W140_OUT_DIGITAL_STATUS","W140 OUT","P120","L87_GravitySafetySupervisoryPLC",-12.34000f,3.34100f,7.60000f,"mint"),
            ("W001_IN_POWER_CONTROL","W001 IN","P121","L87_GravitySafetySupervisoryPLC",-12.01500f,3.56100f,7.60000f,"red"),
            ("W104_IN_ANALOG_FEEDBACK","W104 IN","P121","L87_GravitySafetySupervisoryPLC",-11.79500f,3.34100f,7.60000f,"blue"),
            ("W112_IN_DATA_STREAM","W112 IN","P121","L87_GravitySafetySupervisoryPLC",-12.01500f,3.12100f,7.60000f,"mint"),
            ("W123_IN_SAFETY_CONTROL","W123 IN","P121","L87_GravitySafetySupervisoryPLC",-12.01500f,4.00100f,7.60000f,"mint"),
            ("","P122","P122","L87_GravitySafetySupervisoryPLC",-11.91000f,3.56100f,7.60000f,"gold"),
            ("","P123","P123","L87_GravitySafetySupervisoryPLC",-11.80500f,3.56100f,7.60000f,"gold"),
            ("W128_OUT_SAFETY_CONTROL","W128 OUT","P124","L87_GravitySafetySupervisoryPLC",-11.70000f,3.56100f,7.60000f,"mint"),
            ("W141_IN_SAFETY_CONTROL","W141 IN","P124","L87_GravitySafetySupervisoryPLC",-11.48000f,3.34100f,7.60000f,"mint"),
            ("W105_IN_ANALOG_FEEDBACK","W105 IN","P125","L87_GravitySafetySupervisoryPLC",-11.59500f,3.56100f,7.60000f,"blue"),
            ("W124_IN_SAFETY_CONTROL","W124 IN","P125","L87_GravitySafetySupervisoryPLC",-11.37500f,3.78100f,7.60000f,"mint"),
            ("W138_IN_DIGITAL_STATUS","W138 IN","P126","L87_GravitySafetySupervisoryPLC",-11.49000f,3.56100f,7.60000f,"mint"),
            ("W152_IN_DATA_STREAM","W152 IN","P127","L87_GravitySafetySupervisoryPLC",-11.38500f,3.56100f,7.60000f,"mint"),
            ("W129_OUT_SAFETY_CONTROL","W129 OUT","P128","L87_GravitySafetySupervisoryPLC",-11.28000f,3.56100f,7.60000f,"mint"),
            ("W125_IN_SAFETY_CONTROL","W125 IN","P129","L87_GravitySafetySupervisoryPLC",-12.12000f,3.66600f,7.60000f,"mint"),
            ("W143_OUT_DATA_STREAM","W143 OUT","P129","L87_GravitySafetySupervisoryPLC",-12.34000f,3.88600f,7.60000f,"mint"),
            ("W139_OUT_DATA_STREAM","W139 OUT","P130","L87_GravitySafetySupervisoryPLC",-12.01500f,3.66600f,7.60000f,"mint"),
            ("W153_OUT_DIGITAL_STATUS","W153 OUT","P131","L87_GravitySafetySupervisoryPLC",-11.91000f,3.66600f,7.60000f,"mint"),
            ("","P132","P132","L87_GravitySafetySupervisoryPLC",-11.80500f,3.66600f,7.60000f,"gold"),
            ("","P133","P133","L87_GravitySafetySupervisoryPLC",-11.70000f,3.66600f,7.60000f,"gold"),
            ("W135_IN_ANALOG_FEEDBACK","W135 IN","P134","L87_MagneticBearingSafetyPLC",-10.72000f,3.56100f,7.60000f,"blue"),
            ("W002_IN_POWER_CONTROL","W002 IN","P136","L87_MagneticBearingSafetyPLC",-10.51000f,3.56100f,7.60000f,"red"),
            ("W130_OUT_SAFETY_CONTROL","W130 OUT","P137","L87_MagneticBearingSafetyPLC",-10.40500f,3.56100f,7.60000f,"mint"),
            ("W134_OUT_SAFETY_CONTROL","W134 OUT","P137","L87_MagneticBearingSafetyPLC",-10.40500f,3.34100f,7.60000f,"mint"),
            ("W036_IN_DIGITAL_STATUS","W036 IN","P138","L87_MagneticBearingSafetyPLC",-10.30000f,3.56100f,7.60000f,"mint"),
            ("W136_IN_DIGITAL_STATUS","W136 IN","P138","L87_MagneticBearingSafetyPLC",-10.30000f,3.78100f,7.60000f,"mint"),
            ("W138_OUT_DIGITAL_STATUS","W138 OUT","P139","L87_MagneticBearingSafetyPLC",-10.19500f,3.56100f,7.60000f,"mint"),
            ("W142_OUT_DIGITAL_STATUS","W142 OUT","P139","L87_MagneticBearingSafetyPLC",-10.19500f,3.34100f,7.60000f,"mint"),
            ("","P140","P140","L87_MagneticBearingSafetyPLC",-10.09000f,3.56100f,7.60000f,"gold"),
            ("W126_IN_ANALOG_FEEDBACK","W126 IN","P141","L87_IndependentOverspeedProtector",-9.32000f,3.56100f,7.60000f,"blue"),
            ("W150_OUT_DIGITAL_STATUS","W150 OUT","P141","L87_IndependentOverspeedProtector",-9.54000f,3.56100f,7.60000f,"mint"),
            ("W007_IN_POWER_CONTROL","W007 IN","P142","L87_IndependentOverspeedProtector",-9.21500f,3.56100f,7.60000f,"red"),
            ("W125_OUT_SAFETY_CONTROL","W125 OUT","P143","L87_IndependentOverspeedProtector",-9.11000f,3.56100f,7.60000f,"mint"),
            ("W123_OUT_SAFETY_CONTROL","W123 OUT","P144","L87_RingEmergencyStopStation",-7.92000f,3.56100f,7.60000f,"mint"),
            ("W003_OUT_POWER_CONTROL","W003 OUT","P145","L87_Protected5VControlSupply",-6.52000f,3.56100f,7.60000f,"red"),
            ("W012_OUT_POWER_CONTROL","W012 OUT","P145","L87_Protected5VControlSupply",-6.74000f,3.56100f,7.60000f,"red"),
            ("W001_OUT_POWER_CONTROL","W001 OUT","P146","L87_Protected5VControlSupply",-6.41500f,3.56100f,7.60000f,"red"),
            ("W010_OUT_POWER_CONTROL","W010 OUT","P146","L87_Protected5VControlSupply",-6.41500f,3.34100f,7.60000f,"red"),
            ("W008_OUT_POWER_CONTROL","W008 OUT","P147","L87_Protected5VControlSupply",-6.31000f,3.56100f,7.60000f,"red"),
            ("W006_OUT_POWER_CONTROL","W006 OUT","P148","L87_Protected5VControlSupply",-6.20500f,3.56100f,7.60000f,"red"),
            ("W004_OUT_POWER_CONTROL","W004 OUT","P149","L87_Protected5VControlSupply",-6.10000f,3.56100f,7.60000f,"red"),
            ("W002_OUT_POWER_CONTROL","W002 OUT","P150","L87_Protected5VControlSupply",-5.99500f,3.56100f,7.60000f,"red"),
            ("W011_OUT_POWER_CONTROL","W011 OUT","P150","L87_Protected5VControlSupply",-5.99500f,3.34100f,7.60000f,"red"),
            ("W009_OUT_POWER_CONTROL","W009 OUT","P151","L87_Protected5VControlSupply",-5.89000f,3.56100f,7.60000f,"red"),
            ("W007_OUT_POWER_CONTROL","W007 OUT","P152","L87_Protected5VControlSupply",-5.78500f,3.56100f,7.60000f,"red"),
            ("W005_OUT_POWER_CONTROL","W005 OUT","P153","L87_Protected5VControlSupply",-5.68000f,3.56100f,7.60000f,"red"),
            ("W020_IN_ANALOG_FEEDBACK","W020 IN","P154","L87_MagneticBearingControlPLC",-5.12000f,3.56100f,7.60000f,"blue"),
            ("W031_IN_ANALOG_FEEDBACK","W031 IN","P154","L87_MagneticBearingControlPLC",-5.34000f,3.56100f,7.60000f,"blue"),
            ("","P155","P155","L87_MagneticBearingControlPLC",-5.01500f,3.56100f,7.60000f,"gold"),
            ("W015_OUT_ACTUATOR_COMMAND","W015 OUT","P156","L87_MagneticBearingControlPLC",-4.91000f,3.56100f,7.60000f,"mint"),
            ("W026_IN_ANALOG_FEEDBACK","W026 IN","P156","L87_MagneticBearingControlPLC",-4.91000f,3.34100f,7.60000f,"blue"),
            ("W003_IN_POWER_CONTROL","W003 IN","P157","L87_MagneticBearingControlPLC",-4.80500f,3.56100f,7.60000f,"red"),
            ("W038_OUT_DATA_STREAM","W038 OUT","P157","L87_MagneticBearingControlPLC",-4.58500f,3.34100f,7.60000f,"mint"),
            ("W021_OUT_ACTUATOR_COMMAND","W021 OUT","P158","L87_MagneticBearingControlPLC",-4.70000f,3.56100f,7.60000f,"mint"),
            ("W032_IN_ANALOG_FEEDBACK","W032 IN","P158","L87_MagneticBearingControlPLC",-4.70000f,3.12100f,7.60000f,"blue"),
            ("","P159","P159","L87_MagneticBearingControlPLC",-4.59500f,3.56100f,7.60000f,"gold"),
            ("W016_IN_ANALOG_FEEDBACK","W016 IN","P160","L87_MagneticBearingControlPLC",-4.49000f,3.56100f,7.60000f,"blue"),
            ("W027_OUT_ACTUATOR_COMMAND","W027 OUT","P160","L87_MagneticBearingControlPLC",-4.27000f,3.34100f,7.60000f,"mint"),
            ("","P161","P161","L87_MagneticBearingControlPLC",-4.38500f,3.56100f,7.60000f,"gold"),
            ("W022_IN_ANALOG_FEEDBACK","W022 IN","P162","L87_MagneticBearingControlPLC",-4.28000f,3.56100f,7.60000f,"blue"),
            ("W033_OUT_ACTUATOR_COMMAND","W033 OUT","P162","L87_MagneticBearingControlPLC",-4.06000f,3.56100f,7.60000f,"mint"),
            ("","P163","P163","L87_MagneticBearingControlPLC",-5.12000f,3.66600f,7.60000f,"gold"),
            ("W017_OUT_ACTUATOR_COMMAND","W017 OUT","P164","L87_MagneticBearingControlPLC",-5.01500f,3.66600f,7.60000f,"mint"),
            ("W028_IN_ANALOG_FEEDBACK","W028 IN","P164","L87_MagneticBearingControlPLC",-5.01500f,4.10600f,7.60000f,"blue"),
            ("W065_IN_ACTUATOR_COMMAND","W065 IN","P165","L87_MagneticBearingControlPLC",-4.91000f,3.66600f,7.60000f,"mint"),
            ("W128_IN_SAFETY_CONTROL","W128 IN","P165","L87_MagneticBearingControlPLC",-5.35000f,3.88600f,7.60000f,"mint"),
            ("W144_OUT_DATA_STREAM","W144 OUT","P165","L87_MagneticBearingControlPLC",-4.47000f,3.88600f,7.60000f,"mint"),
            ("W023_OUT_ACTUATOR_COMMAND","W023 OUT","P166","L87_MagneticBearingControlPLC",-4.80500f,3.66600f,7.60000f,"mint"),
            ("W034_IN_ANALOG_FEEDBACK","W034 IN","P166","L87_MagneticBearingControlPLC",-4.80500f,4.10600f,7.60000f,"blue"),
            ("","P167","P167","L87_MagneticBearingControlPLC",-4.70000f,3.66600f,7.60000f,"gold"),
            ("W018_IN_ANALOG_FEEDBACK","W018 IN","P168","L87_MagneticBearingControlPLC",-4.59500f,3.66600f,7.60000f,"blue"),
            ("W029_IN_ANALOG_FEEDBACK","W029 IN","P168","L87_MagneticBearingControlPLC",-4.59500f,4.10600f,7.60000f,"blue"),
            ("","P169","P169","L87_MagneticBearingControlPLC",-4.49000f,3.66600f,7.60000f,"gold"),
            ("W013_OUT_ACTUATOR_COMMAND","W013 OUT","P170","L87_MagneticBearingControlPLC",-4.38500f,3.66600f,7.60000f,"mint"),
            ("W024_IN_ANALOG_FEEDBACK","W024 IN","P170","L87_MagneticBearingControlPLC",-4.16500f,3.88600f,7.60000f,"blue"),
            ("W035_OUT_SAFETY_CONTROL","W035 OUT","P170","L87_MagneticBearingControlPLC",-4.38500f,4.10600f,7.60000f,"mint"),
            ("W130_IN_SAFETY_CONTROL","W130 IN","P170","L87_MagneticBearingControlPLC",-3.94500f,3.88600f,7.60000f,"mint"),
            ("","P171","P171","L87_MagneticBearingControlPLC",-4.28000f,3.66600f,7.60000f,"gold"),
            ("W019_OUT_ACTUATOR_COMMAND","W019 OUT","P172","L87_MagneticBearingControlPLC",-5.12000f,3.77100f,7.60000f,"mint"),
            ("W030_IN_ANALOG_FEEDBACK","W030 IN","P172","L87_MagneticBearingControlPLC",-5.56000f,3.77100f,7.60000f,"blue"),
            ("","P173","P173","L87_MagneticBearingControlPLC",-5.01500f,3.77100f,7.60000f,"gold"),
            ("W014_IN_ANALOG_FEEDBACK","W014 IN","P174","L87_MagneticBearingControlPLC",-4.91000f,3.77100f,7.60000f,"blue"),
            ("W025_OUT_ACTUATOR_COMMAND","W025 OUT","P174","L87_MagneticBearingControlPLC",-5.13000f,3.33100f,7.60000f,"mint"),
            ("W037_IN_ANALOG_FEEDBACK","W037 IN","P175","L87_MagneticBearingControlPLC",-4.80500f,3.77100f,7.60000f,"blue"),
            ("W047_OUT_ACTUATOR_COMMAND","W047 OUT","P176","L87_RotorMotorControlPLC",-3.72000f,3.56100f,7.60000f,"mint"),
            ("W111_IN_DATA_STREAM","W111 IN","P176","L87_RotorMotorControlPLC",-3.72000f,3.34100f,7.60000f,"mint"),
            ("W044_IN_ANALOG_FEEDBACK","W044 IN","P177","L87_RotorMotorControlPLC",-3.61500f,3.56100f,7.60000f,"blue"),
            ("W052_IN_DATA_STREAM","W052 IN","P177","L87_RotorMotorControlPLC",-3.39500f,3.34100f,7.60000f,"mint"),
            ("W145_OUT_DATA_STREAM","W145 OUT","P177","L87_RotorMotorControlPLC",-3.61500f,3.12100f,7.60000f,"mint"),
            ("","P178","P178","L87_RotorMotorControlPLC",-3.51000f,3.56100f,7.60000f,"gold"),
            ("W039_OUT_ACTUATOR_COMMAND","W039 OUT","P179","L87_RotorMotorControlPLC",-3.40500f,3.56100f,7.60000f,"mint"),
            ("W004_IN_POWER_CONTROL","W004 IN","P180","L87_RotorMotorControlPLC",-3.30000f,3.56100f,7.60000f,"red"),
            ("W048_IN_ANALOG_FEEDBACK","W048 IN","P180","L87_RotorMotorControlPLC",-3.08000f,3.34100f,7.60000f,"blue"),
            ("W059_IN_DIGITAL_STATUS","W059 IN","P180","L87_RotorMotorControlPLC",-3.08000f,3.78100f,7.60000f,"mint"),
            ("W066_IN_SAFETY_CONTROL","W066 IN","P180","L87_RotorMotorControlPLC",-3.30000f,3.12100f,7.60000f,"mint"),
            ("W045_IN_ANALOG_FEEDBACK","W045 IN","P181","L87_RotorMotorControlPLC",-3.19500f,3.56100f,7.60000f,"blue"),
            ("W054_OUT_ACTUATOR_COMMAND","W054 OUT","P182","L87_RotorMotorControlPLC",-3.09000f,3.56100f,7.60000f,"mint"),
            ("W127_IN_SAFETY_CONTROL","W127 IN","P182","L87_RotorMotorControlPLC",-2.87000f,3.34100f,7.60000f,"mint"),
            ("W040_OUT_ACTUATOR_COMMAND","W040 OUT","P183","L87_RotorMotorControlPLC",-2.98500f,3.56100f,7.60000f,"mint"),
            ("W060_IN_DIGITAL_STATUS","W060 IN","P184","L87_RotorMotorControlPLC",-2.88000f,3.56100f,7.60000f,"mint"),
            ("W046_IN_ANALOG_FEEDBACK","W046 IN","P185","L87_RotorMotorControlPLC",-3.72000f,3.66600f,7.60000f,"blue"),
            ("W051_IN_DIGITAL_CONTROL","W051 IN","P186","L87_RotorMotorControlPLC",-3.61500f,3.66600f,7.60000f,"mint"),
            ("","P187","P187","L87_RotorMotorControlPLC",-3.51000f,3.66600f,7.60000f,"gold"),
            ("W057_OUT_DIGITAL_CONTROL","W057 OUT","P188","L87_RotorMotorControlPLC",-3.40500f,3.66600f,7.60000f,"mint"),
            ("W084_IN_ANALOG_FEEDBACK","W084 IN","P189","L87_HabitatPowerDistributionPLC",-2.32000f,3.56100f,7.60000f,"blue"),
            ("W089_IN_ANALOG_FEEDBACK","W089 IN","P190","L87_HabitatPowerDistributionPLC",-2.21500f,3.56100f,7.60000f,"blue"),
            ("W093_OUT_DIGITAL_CONTROL","W093 OUT","P190","L87_HabitatPowerDistributionPLC",-2.21500f,3.34100f,7.60000f,"mint"),
            ("W097_OUT_DIGITAL_CONTROL","W097 OUT","P190","L87_HabitatPowerDistributionPLC",-1.99500f,3.34100f,7.60000f,"mint"),
            ("","P191","P191","L87_HabitatPowerDistributionPLC",-2.11000f,3.56100f,7.60000f,"gold"),
            ("","P192","P192","L87_HabitatPowerDistributionPLC",-2.00500f,3.56100f,7.60000f,"gold"),
            ("W081_OUT_DIGITAL_CONTROL","W081 OUT","P193","L87_HabitatPowerDistributionPLC",-1.90000f,3.56100f,7.60000f,"mint"),
            ("W005_IN_POWER_CONTROL","W005 IN","P194","L87_HabitatPowerDistributionPLC",-1.79500f,3.56100f,7.60000f,"red"),
            ("W086_IN_DATA_STREAM","W086 IN","P194","L87_HabitatPowerDistributionPLC",-1.79500f,3.34100f,7.60000f,"mint"),
            ("W090_IN_SAFETY_CONTROL","W090 IN","P194","L87_HabitatPowerDistributionPLC",-1.57500f,3.34100f,7.60000f,"mint"),
            ("W094_IN_ANALOG_FEEDBACK","W094 IN","P194","L87_HabitatPowerDistributionPLC",-1.79500f,3.12100f,7.60000f,"blue"),
            ("W098_OUT_SAFETY_CONTROL","W098 OUT","P194","L87_HabitatPowerDistributionPLC",-1.79500f,4.00100f,7.60000f,"mint"),
            ("","P195","P195","L87_HabitatPowerDistributionPLC",-1.69000f,3.56100f,7.60000f,"gold"),
            ("","P196","P196","L87_HabitatPowerDistributionPLC",-1.58500f,3.56100f,7.60000f,"gold"),
            ("W082_OUT_DIGITAL_CONTROL","W082 OUT","P197","L87_HabitatPowerDistributionPLC",-1.48000f,3.56100f,7.60000f,"mint"),
            ("W087_OUT_ACTUATOR_COMMAND","W087 OUT","P198","L87_HabitatPowerDistributionPLC",-2.32000f,3.66600f,7.60000f,"mint"),
            ("W091_IN_ANALOG_FEEDBACK","W091 IN","P198","L87_HabitatPowerDistributionPLC",-2.54000f,3.66600f,7.60000f,"blue"),
            ("W095_IN_DATA_STREAM","W095 IN","P198","L87_HabitatPowerDistributionPLC",-2.32000f,3.88600f,7.60000f,"mint"),
            ("W099_OUT_DIGITAL_CONTROL","W099 OUT","P198","L87_HabitatPowerDistributionPLC",-2.54000f,3.44600f,7.60000f,"mint"),
            ("","P199","P199","L87_HabitatPowerDistributionPLC",-2.21500f,3.66600f,7.60000f,"gold"),
            ("","P200","P200","L87_HabitatPowerDistributionPLC",-2.11000f,3.66600f,7.60000f,"gold"),
            ("W083_IN_ANALOG_FEEDBACK","W083 IN","P201","L87_HabitatPowerDistributionPLC",-2.00500f,3.66600f,7.60000f,"blue"),
            ("W088_IN_ANALOG_FEEDBACK","W088 IN","P202","L87_HabitatPowerDistributionPLC",-1.90000f,3.66600f,7.60000f,"blue"),
            ("W092_IN_ANALOG_FEEDBACK","W092 IN","P202","L87_HabitatPowerDistributionPLC",-2.12000f,3.88600f,7.60000f,"blue"),
            ("W096_OUT_DIGITAL_CONTROL","W096 OUT","P202","L87_HabitatPowerDistributionPLC",-1.46000f,3.88600f,7.60000f,"mint"),
            ("W100_IN_DATA_STREAM","W100 IN","P202","L87_HabitatPowerDistributionPLC",-2.12000f,4.10600f,7.60000f,"mint"),
            ("W129_IN_SAFETY_CONTROL","W129 IN","P202","L87_HabitatPowerDistributionPLC",-2.34000f,4.10600f,7.60000f,"mint"),
            ("","P203","P203","L87_HabitatPowerDistributionPLC",-1.79500f,3.66600f,7.60000f,"gold"),
            ("","P204","P204","L87_HabitatPowerDistributionPLC",-1.69000f,3.66600f,7.60000f,"gold"),
            ("W131_OUT_DIGITAL_CONTROL","W131 OUT","P205","L87_RadialSensorDriftFaultInjector",-0.92000f,3.56100f,7.60000f,"mint"),
            ("W132_OUT_DIGITAL_CONTROL","W132 OUT","P206","L87_BearingAmplifierHeatFaultInjector",0.48000f,3.56100f,7.60000f,"mint"),
            ("W133_OUT_DIGITAL_CONTROL","W133 OUT","P207","L87_MassImbalanceFaultInjector",1.88000f,3.56100f,7.60000f,"mint"),
            ("","P208","P208","L87_W137CriticalFeedbackPatchStation",3.28000f,3.56100f,7.60000f,"gold"),
            ("W136_OUT_DIGITAL_STATUS","W136 OUT","P209","L87_W137CriticalFeedbackPatchStation",3.38500f,3.56100f,7.60000f,"mint"),
            ("W011_IN_POWER_CONTROL","W011 IN","P210","L87_W137CriticalFeedbackPatchStation",3.49000f,3.56100f,7.60000f,"red"),
            ("W140_IN_DIGITAL_STATUS","W140 IN","P210","L87_W137CriticalFeedbackPatchStation",3.71000f,3.56100f,7.60000f,"mint"),
            ("W155_OUT_DATA_STREAM","W155 OUT","P210","L87_W137CriticalFeedbackPatchStation",3.49000f,3.34100f,7.60000f,"mint"),
            ("W143_IN_DATA_STREAM","W143 IN","P211","L87_GravityEngineeringMasterSCADA",-0.42000f,11.61100f,-8.30000f,"mint"),
            ("W080_IN_DATA_STREAM","W080 IN","P212","L87_GravityEngineeringMasterSCADA",-0.31500f,11.61100f,-8.30000f,"mint"),
            ("W116_IN_DATA_STREAM","W116 IN","P212","L87_GravityEngineeringMasterSCADA",-0.31500f,11.39100f,-8.30000f,"mint"),
            ("","P213","P213","L87_GravityEngineeringMasterSCADA",-0.21000f,11.61100f,-8.30000f,"gold"),
            ("W148_IN_DATA_STREAM","W148 IN","P214","L87_GravityEngineeringMasterSCADA",-0.10500f,11.61100f,-8.30000f,"mint"),
            ("W158_OUT_DATA_STREAM","W158 OUT","P214","L87_GravityEngineeringMasterSCADA",-0.10500f,11.39100f,-8.30000f,"mint"),
            ("W144_IN_DATA_STREAM","W144 IN","P215","L87_GravityEngineeringMasterSCADA",0.00000f,11.61100f,-8.30000f,"mint"),
            ("W154_IN_DIGITAL_STATUS","W154 IN","P215","L87_GravityEngineeringMasterSCADA",0.22000f,11.39100f,-8.30000f,"mint"),
            ("","P216","P216","L87_GravityEngineeringMasterSCADA",0.10500f,11.61100f,-8.30000f,"gold"),
            ("W006_IN_POWER_CONTROL","W006 IN","P217","L87_GravityEngineeringMasterSCADA",0.21000f,11.61100f,-8.30000f,"red"),
            ("W113_IN_DATA_STREAM","W113 IN","P217","L87_GravityEngineeringMasterSCADA",0.43000f,11.39100f,-8.30000f,"mint"),
            ("W149_IN_DIGITAL_STATUS","W149 IN","P218","L87_GravityEngineeringMasterSCADA",0.31500f,11.61100f,-8.30000f,"mint"),
            ("W120_IN_DATA_STREAM","W120 IN","P219","L87_GravityEngineeringMasterSCADA",0.42000f,11.61100f,-8.30000f,"mint"),
            ("W145_IN_DATA_STREAM","W145 IN","P219","L87_GravityEngineeringMasterSCADA",0.64000f,11.61100f,-8.30000f,"mint"),
            ("W155_IN_DATA_STREAM","W155 IN","P219","L87_GravityEngineeringMasterSCADA",0.64000f,11.39100f,-8.30000f,"mint"),
            ("W102_IN_DATA_STREAM","W102 IN","P220","L87_GravityEngineeringMasterSCADA",-0.42000f,11.71600f,-8.30000f,"mint"),
            ("W114_IN_DATA_STREAM","W114 IN","P221","L87_GravityEngineeringMasterSCADA",-0.31500f,11.71600f,-8.30000f,"mint"),
            ("W150_IN_DIGITAL_STATUS","W150 IN","P222","L87_GravityEngineeringMasterSCADA",-0.21000f,11.71600f,-8.30000f,"mint"),
            ("W146_IN_DATA_STREAM","W146 IN","P223","L87_GravityEngineeringMasterSCADA",-0.10500f,11.71600f,-8.30000f,"mint"),
            ("W156_IN_DATA_STREAM","W156 IN","P223","L87_GravityEngineeringMasterSCADA",-0.10500f,11.93600f,-8.30000f,"mint"),
            ("","P224","P224","L87_GravityEngineeringMasterSCADA",0.00000f,11.71600f,-8.30000f,"gold"),
            ("W079_IN_DATA_STREAM","W079 IN","P225","L87_GravityEngineeringMasterSCADA",0.10500f,11.71600f,-8.30000f,"mint"),
            ("W115_IN_DATA_STREAM","W115 IN","P225","L87_GravityEngineeringMasterSCADA",0.10500f,11.93600f,-8.30000f,"mint"),
            ("W151_OUT_DATA_STREAM","W151 OUT","P226","L87_GravityEngineeringMasterSCADA",0.21000f,11.71600f,-8.30000f,"mint"),
            ("W147_IN_DATA_STREAM","W147 IN","P227","L87_GravityEngineeringMasterSCADA",0.31500f,11.71600f,-8.30000f,"mint"),
            ("W157_IN_DATA_STREAM","W157 IN","P227","L87_GravityEngineeringMasterSCADA",0.31500f,11.93600f,-8.30000f,"mint"),
            ("W153_IN_DIGITAL_STATUS","W153 IN","P228","L87_RingCertificationBeacon",-0.42000f,12.56100f,-7.60000f,"mint"),
            ("W142_IN_DIGITAL_STATUS","W142 IN","P229","L87_RingCertificationBeacon",-0.31500f,12.56100f,-7.60000f,"mint"),
            ("W154_OUT_DIGITAL_STATUS","W154 OUT","P229","L87_RingCertificationBeacon",-0.31500f,12.34100f,-7.60000f,"mint"),
            ("W158_IN_DATA_STREAM","W158 IN","P230","L87_RingCertificationBeacon",-0.21000f,12.56100f,-7.60000f,"mint"),
            ("W152_OUT_DATA_STREAM","W152 OUT","P231","L87_GravityMachineEventRecorder",4.68000f,3.56100f,7.60000f,"mint"),
            ("W151_IN_DATA_STREAM","W151 IN","P232","L87_GravityMachineEventRecorder",4.78500f,3.56100f,7.60000f,"mint"),
            ("W012_IN_POWER_CONTROL","W012 IN","P233","L87_GravityMachineEventRecorder",4.89000f,3.56100f,7.60000f,"red"),
            ("W122_IN_DATA_STREAM","W122 IN","P234","L87_GravityMachineEventRecorder",4.99500f,3.56100f,7.60000f,"mint"),
            ("W139_IN_DATA_STREAM","W139 IN","P234","L87_GravityMachineEventRecorder",4.99500f,3.34100f,7.60000f,"mint"),
            ("W141_OUT_SAFETY_CONTROL","W141 OUT","P234","L87_GravityMachineEventRecorder",4.99500f,3.78100f,7.60000f,"mint"),
            ("W121_IN_DATA_STREAM","W121 IN","P235","L87_GravityMachineEventRecorder",5.10000f,3.56100f,7.60000f,"mint"),
            ("W124_OUT_SAFETY_CONTROL","W124 OUT","P236","L87_StationSafetyIsolationRelay",6.08000f,3.56100f,7.60000f,"mint"),
        };

        public static IEnumerable<string> TerminalIds => SparkLevel87Wiring.Pins;

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 87")]
        public static void Install()
        {
            SparkLevel87Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel87Process>().rig == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 87/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel87Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.rig==null || control.rig.GetComponentsInChildren<Transform>(true).Count(item=>item.name.StartsWith("ANIM_",StringComparison.Ordinal))!=68 ||
                control.pins.Length!=316 || control.pins.Distinct().Count()!=316 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel87Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 87 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 87] Validated 316 distinct terminals, 158 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_87.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_87_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 87's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 87 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }


        [MenuItem("Tools/Project Spark/Level 87/Fit Gravity Arena")]
        public static void Fit()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode to fit the service arena.");
            var models=Models.Select((name,index)=>AssetDatabase.LoadAssetAtPath<GameObject>(
                Folder+$"Models/L87_{index:00}_{name}_Fitted.fbx")??throw new InvalidOperationException("Missing module "+name)).ToArray();
            var root=PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                Fit(root,models);
                if(PrefabUtility.SaveAsPrefabAsset(root,Prefab)==null)throw new InvalidOperationException("Could not save Level 87 arena.");
                AssetDatabase.SaveAssets();
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }

        private static void Fit(GameObject root, GameObject[] models)
        {
            var bench = root.transform.Find("Gravity Engine Arena");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel87Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 87 is missing its workbench, controls, or scene bindings.");


            const float floor=.425f;
            foreach (var terminal in loop.pins)
                if (terminal.Owner != null)
                    terminal.transform.SetParent(terminal.Owner.transform, true);
            foreach (string name in new[] {"L87_HD_VISUAL_ART", "Equipment terminal panels", "Motion pivots"})
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
            var assembly = Child(bench, "L87_HD_VISUAL_ART", new Vector3(0,floor,-.010f));
            for (var index=0;index<models.Length;index++)
            {
                var model=models[index];
                var instance = Object.Instantiate(model, assembly, false).transform;
                instance.name="L87_"+Models[index];
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L87_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L87_Board" + name];
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
                    "glass"=>"VerticalStationGlass",
                    _ when mesh.name.Contains("Floor")||mesh.name.Contains("ArenaBase")=>"OrbitalMachineServiceFloor",
                    _ when mesh.name.Contains("Hologram")=>"MagneticFieldCyanHologram",
                    _ when mesh.name.Contains("Windings")||mesh.name.Contains("Copper")=>"ElectromagneticCopperWindings",
                    _ when mesh.name.Contains("Magnet")=>"ElectroMagnetPoleSteel",
                    _ when mesh.name.Contains("Bearing")=>"PrecisionBearingTitanium",
                    _ when mesh.name.Contains("HabitatSegment")=>"OrbitalHabitatCeramicHull",
                    _ when mesh.name.Contains("RingMain")=>"CarbonCompositeRotor",
                    _ when mesh.name.Contains("Emergency")=>"EmergencyRedSafetyHousing",
                    _ when mesh.name.Contains("SCADA")=>"GravityControlInstrumentPanel",
                    _=>null
                };
                var material = MaterialFor(shader,key,surface);

                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loop.eventText == null || loop.buttons.Length!=25)
            {
                var oldControls = root.transform.Find("Level 87 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel87Builder.BuildControls(root.transform,loop,font);
            }
            var labels = Child(bench, "Equipment terminal panels", Vector3.zero);
            int pinLayer = LayerMask.NameToLayer("Pin");
            if (pinLayer < 0) throw new InvalidOperationException("Missing Pin interaction layer.");
            var frames = new Dictionary<string, Transform>();
            loop.pinLabels=new TextMesh[loop.pins.Length];
            loop.componentLabels=new TextMesh[loop.pins.Length];
            loop.componentNames=new string[loop.pins.Length];
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
                pinData.FindProperty("maxConnections").intValue=SparkLevel87Wiring.Wires.Count(wire=>wire.a==index||wire.b==index);
                pinData.ApplyModifiedPropertiesWithoutUndo();
                Seat(terminal,bench,local,pinLayer,rim,MaterialFor(shader,"void"));
                string display=DisplayName(spec.model);
                loop.componentNames[index]=display;
                var tag=Label(mount,font,"Label "+spec.id,spec.label,new Vector3(0,.014f,-.012f),.034f);
                tag.transform.localRotation=Quaternion.Euler(22,0,0);
                loop.pinLabels[index]=tag;
                tag.gameObject.SetActive(false);
            }
            foreach(var entry in frames)
            {
                var frame=entry.Value;
                var mounts=frame.Cast<Transform>().ToArray();
                var center=mounts.Aggregate(Vector3.zero,(sum,item)=>sum+item.localPosition)/mounts.Length;
                center.y=mounts.Max(item=>item.localPosition.y)+.101f;
                center.z=mounts.Min(item=>item.localPosition.z)-.020f;
                var label=Label(frame,font,"Equipment label",DisplayName(entry.Key),center,.041f);
                label.transform.localRotation=Quaternion.Euler(17,0,0);
                label.color=new Color(.53f,.91f,1f);
                label.gameObject.SetActive(false);
                for(int i=0;i<loop.pinLabels.Length;i++)
                    if(loop.pinLabels[i]&&loop.pinLabels[i].transform.parent.parent==frame)loop.componentLabels[i]=label;
            }
            foreach(var body in bench.GetComponentsInChildren<MeshRenderer>(true))
                if(body.name=="Body") body.gameObject.SetActive(false);
            var title=Label(labels,font,"Level name","THE GRAVITY ENGINE",new Vector3(0,floor+.018f,-.688f),.023f);
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
            loop.rig=assembly.gameObject.AddComponent<SparkLevel87VisualRig>();

            var fx=new SerializedObject(loop.rig);
            fx.FindProperty("artRoot").objectReferenceValue=assembly;
            fx.FindProperty("criticalFaultLamp").objectReferenceValue=Lamp("Feedback isolation",new Vector3(.44f,.65f,.50f),new Color(1,.10f,.17f),.9f,.65f);
            fx.FindProperty("driveHealthyLamp").objectReferenceValue=Lamp("Drive ready",new Vector3(.48f,1.01f,.46f),new Color(.19f,1,.65f),1.10f,1.05f);
            fx.FindProperty("levitationLamp").objectReferenceValue=Lamp("Levitation field",new Vector3(-.45f,1.03f,.57f),new Color(.12f,.68f,1),1.3f,1.10f);
            foreach(var entry in frames)
            {
                var module=assembly.Find(entry.Key);
                if(!module)continue;
                Transform moving=module.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>
                    t.name is "ANIM_GRAVITY_RING_MAIN_ROTATION" or "ANIM_GRAVITY_RING_SPOKES" or
                    "ANIM_HABITAT_PRESSURE_SHELL" or "ANIM_ROTOR_MAGNET_ARRAY" || t.name.StartsWith("ANIM_HABITAT_SEGMENT_",StringComparison.Ordinal));
                if(moving)entry.Value.gameObject.AddComponent<SparkLevel87MountFollower>().Configure(moving);
            }
            fx.ApplyModifiedPropertiesWithoutUndo();
            loop.arena=bench;

            // The supplied pivot artwork is driven from the authoritative plant each frame.
            // The 236 decorative socket records remain in the imported art and source plan.
        }

        private static string DisplayName(string model)
        {
            string name=model.StartsWith("L87_")?model.Substring(4):model;
            if(name.StartsWith("MagneticBearingStation"))return "Bearing Station "+name[name.Length-1];
            return name switch
            {
                "Protected5VControlSupply"=>"Protected 5 V Supply",
                "PrimaryRadialGapSensor"=>"Primary Gap Sensor",
                "SecondaryRadialGapSensor"=>"Secondary Gap Sensor",
                "MagneticBearingSafetyPLC"=>"Bearing Safety PLC",
                "GravitySafetySupervisoryPLC"=>"Gravity Safety PLC",
                "BearingPowerAmplifierCabinet"=>"Bearing Amplifier",
                "GravityEngineeringMasterSCADA"=>"Gravity Control Display",
                "W137CriticalFeedbackPatchStation"=>"W137 Patch Monitor",
                _=>System.Text.RegularExpressions.Regex.Replace(name,@"(?<=[a-z0-9])(?=[A-Z])"," ")
            };
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
            string materialName=surface==null?"L87 "+key:"L87 "+key+" "+surface;
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
                    throw new InvalidOperationException("Missing Level 87 surface maps for "+surface);
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

























