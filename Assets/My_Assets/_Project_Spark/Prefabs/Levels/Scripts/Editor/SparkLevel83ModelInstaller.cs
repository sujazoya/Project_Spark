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
    public static class SparkLevel83ModelInstaller
    {
        public const string Folder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_83_Layout/";
        public const string Prefab = Folder + "Level_83_Layout.prefab";
        public const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private static readonly string[] Models = {
            "OrbitalEnergyQualificationArenaBase","PanoramicOrbitShadowDome","TransparentSpacecraftPowerTestChamber","FourChipletMissionComputerPayload","SpacecraftServiceBusStructure","DeployableSolarWingPort","DeployableSolarWingStarboard","SolarWingPortHingeAssembly","SolarWingStarboardHingeAssembly","SolarArrayPortTrackingGimbal","SolarArrayStarboardTrackingGimbal","SolarArrayAzimuthEncoder","SolarIncidenceVectorProjector","ProgrammableSolarIlluminationArc","EarthShadowOccluderStage","OrbitalDayNightHologram","SolarArrayCellStringMap","PVStringPortIVTracer","PVStringStarboardIVTracer","PVStringBypassDiodeBank","SolarArrayCombinerBox","SolarArrayCurrentSensors","SolarArrayVoltageSensors","MPPTControllerPort","MPPTControllerStarboard","MPPTPerturbObserveDisplay","PVMaximumPowerOperatingPointHologram","PowerConditioningConverterA","PowerConditioningConverterB","RegulatedMainDCBus","BusVoltageTransducer","BusCurrentShuntMonitor","BusBulkCapacitorModule","HighSpeedPowerTransientRecorder","BatteryPackA","BatteryPackB","ProtectedBatteryCellTray","BatteryCellVoltageTapArray","BatteryCurrentHallSensor","BatteryThermalSensorGrid","BatteryManagementController","BatteryBalancingCircuit","BatteryChargeController","BatteryDischargeController","BatteryIsolationContactor","BatteryPrechargeResistorModule","SupercapacitorRideThroughModule","BatteryThermalControlPlate","BatteryRadiatorFinAssembly","PowerDistributionUnit","PriorityLoadSwitchingRack","SafetyEssentialLoadBus","AttitudeControlLoadEmulator","MissionComputerLoadEmulator","OpticalTelemetryLoadEmulator","SciencePayloadLoadEmulator","HeaterThermalLoadEmulator","MissionTaskPowerScheduler","LoadForecastEnergyBudgetAnalyzer","EclipseEntryPredictionConsole","SunlightReturnChargePlanner","ContingencyPowerFaultInjector","SolarGimbalJamSimulator","BatteryCellImbalanceSimulator","OpticalBurstDemandInjector","IndependentUndervoltageProtector","EPSPowerSafetyPLC","MainBusEnergyManagementController","Protected5VControlSupply","OrbitalPowerSCADA","ThreeDimensionalEnergyFlowHologram","MissionSurvivalCertificationBeacon","EmergencyPowerIsolationStation","SpacecraftPowerThermalInspectionCamera"
        };
        private const string Materials = Folder + "Models/Materials";
        private const float SocketHeight = .0075f;
        private const float SocketDiameter = .0086f;

        private static readonly (string id, string label, string anchor, string model, float x, float y, float z, string color)[] Sockets =
        {
            ("SUPPLY_POS","5 V A","P198","L83_Protected5VControlSupply",-5.58500f,1.09730f,6.54600f,"red"),
            ("W001_IN","W001 ◂","P192","L83_EPSPowerSafetyPLC",10.71500f,1.56730f,6.64600f,"red"),
            ("W128_OUT","W128 ▸","P192","L83_EPSPowerSafetyPLC",10.48500f,1.56730f,6.64600f,"mint"),
            ("W131_OUT","W131 ▸","P192","L83_EPSPowerSafetyPLC",10.71500f,1.33730f,6.64600f,"mint"),
            ("W136_IN","W136 ◂","P192","L83_EPSPowerSafetyPLC",10.48500f,1.33730f,6.64600f,"mint"),
            ("SUPPLY_TAP_B","5 V B","P199","L83_Protected5VControlSupply",-5.41500f,1.09730f,6.54600f,"red"),
            ("EPS_TRUSTED_BUS_VOLTAGE_IN","W104 ◂","P195","L83_MainBusEnergyManagementController",5.71500f,2.26730f,5.59600f,"amber"),
            ("W002_IN","W002 ◂","P195","L83_MainBusEnergyManagementController",5.48500f,2.26730f,5.59600f,"red"),
            ("W013_OUT","W013 ▸","P195","L83_MainBusEnergyManagementController",5.71500f,2.03730f,5.59600f,"mint"),
            ("W016_IN","W016 ◂","P195","L83_MainBusEnergyManagementController",5.48500f,2.03730f,5.59600f,"blue"),
            ("W019_IN","W019 ◂","P195","L83_MainBusEnergyManagementController",5.48500f,2.49730f,5.59600f,"blue"),
            ("W022_IN","W022 ◂","P195","L83_MainBusEnergyManagementController",5.94500f,2.03730f,5.59600f,"blue"),
            ("W025_IN","W025 ◂","P195","L83_MainBusEnergyManagementController",5.94500f,2.49730f,5.59600f,"blue"),
            ("W028_OUT","W028 ▸","P195","L83_MainBusEnergyManagementController",5.25500f,2.26730f,5.59600f,"mint"),
            ("W036_OUT","W036 ▸","P195","L83_MainBusEnergyManagementController",6.17500f,2.26730f,5.59600f,"mint"),
            ("W041_IN","W041 ◂","P195","L83_MainBusEnergyManagementController",5.71500f,1.80730f,5.59600f,"blue"),
            ("W049_OUT","W049 ▸","P195","L83_MainBusEnergyManagementController",5.71500f,2.72730f,5.59600f,"mint"),
            ("W088_IN","W088 ◂","P195","L83_MainBusEnergyManagementController",5.25500f,2.03730f,5.59600f,"blue"),
            ("W101_IN","W101 ◂","P195","L83_MainBusEnergyManagementController",5.25500f,2.49730f,5.59600f,"mint"),
            ("W107_OUT","W107 ▸","P195","L83_MainBusEnergyManagementController",6.17500f,2.03730f,5.59600f,"mint"),
            ("W111_IN","W111 ◂","P195","L83_MainBusEnergyManagementController",6.17500f,2.49730f,5.59600f,"mint"),
            ("W124_IN","W124 ◂","P195","L83_MainBusEnergyManagementController",5.48500f,1.80730f,5.59600f,"blue"),
            ("W132_OUT","W132 ▸","P195","L83_MainBusEnergyManagementController",5.48500f,2.72730f,5.59600f,"blue"),
            ("W136_OUT","W136 ▸","P195","L83_MainBusEnergyManagementController",5.94500f,1.80730f,5.59600f,"mint"),
            ("SUPPLY_TAP_C","5 V C","P200","L83_Protected5VControlSupply",-5.58500f,1.25730f,6.54600f,"red"),
            ("W003_IN","W003 ◂","P052","L83_MPPTControllerPort",-10.88500f,2.07000f,5.74800f,"red"),
            ("W037_OUT","W037 ▸","P052","L83_MPPTControllerPort",-11.11500f,2.07000f,5.74800f,"mint"),
            ("W004_IN","W004 ◂","P056","L83_MPPTControllerStarboard",-9.53500f,2.07000f,5.74800f,"red"),
            ("W038_OUT","W038 ▸","P056","L83_MPPTControllerStarboard",-9.76500f,2.07000f,5.74800f,"mint"),
            ("W005_IN","W005 ◂","P112","L83_BatteryManagementController",7.45500f,1.48950f,4.51700f,"red"),
            ("W056_IN","W056 ◂","P112","L83_BatteryManagementController",7.22500f,1.48950f,4.51700f,"blue"),
            ("W060_OUT","W060 ▸","P112","L83_BatteryManagementController",7.45500f,1.25950f,4.51700f,"mint"),
            ("W064_OUT","W064 ▸","P112","L83_BatteryManagementController",7.22500f,1.25950f,4.51700f,"mint"),
            ("W073_IN","W073 ◂","P112","L83_BatteryManagementController",7.22500f,1.71950f,4.51700f,"blue"),
            ("W006_IN","W006 ◂","P141","L83_PowerDistributionUnit",0.51500f,1.92000f,4.29600f,"red"),
            ("W087_IN","W087 ◂","P141","L83_PowerDistributionUnit",0.28500f,1.92000f,4.29600f,"mint"),
            ("W128_IN","W128 ◂","P141","L83_PowerDistributionUnit",0.51500f,1.69000f,4.29600f,"mint"),
            ("W007_IN","W007 ◂","P144","L83_PriorityLoadSwitchingRack",2.21500f,1.92000f,4.36600f,"red"),
            ("W082_OUT","W082 ▸","P144","L83_PriorityLoadSwitchingRack",1.98500f,1.92000f,4.36600f,"mint"),
            ("W085_OUT","W085 ▸","P144","L83_PriorityLoadSwitchingRack",2.21500f,1.69000f,4.36600f,"mint"),
            ("W122_IN","W122 ◂","P144","L83_PriorityLoadSwitchingRack",1.98500f,1.69000f,4.36600f,"mint"),
            ("W008_IN","W008 ◂","P201","L83_OrbitalPowerSCADA",-0.08500f,7.18930f,-4.88100f,"red"),
            ("W097_IN","W097 ◂","P201","L83_OrbitalPowerSCADA",-0.31500f,7.18930f,-4.88100f,"blue"),
            ("W100_IN","W100 ◂","P201","L83_OrbitalPowerSCADA",-0.08500f,6.95930f,-4.88100f,"blue"),
            ("W133_IN","W133 ◂","P201","L83_OrbitalPowerSCADA",-0.31500f,6.95930f,-4.88100f,"blue"),
            ("W009_IN","W009 ◂","P189","L83_IndependentUndervoltageProtector",5.01500f,1.57250f,6.00150f,"red"),
            ("W106_OUT","W106 ▸","P189","L83_IndependentUndervoltageProtector",4.78500f,1.57250f,6.00150f,"blue"),
            ("MAIN_DC_BUS_VOLTAGE_OUT","W104 ▸","P075","L83_BusVoltageTransducer",-5.36500f,1.45220f,5.41900f,"amber"),
            ("W010_IN","W010 ◂","P075","L83_BusVoltageTransducer",-5.59500f,1.45220f,5.41900f,"red"),
            ("W011_OUT","W011 ▸","P196","L83_MainBusEnergyManagementController",5.88500f,2.26730f,5.59600f,"mint"),
            ("W014_IN","W014 ◂","P196","L83_MainBusEnergyManagementController",6.11500f,2.72730f,5.59600f,"blue"),
            ("W017_OUT","W017 ▸","P196","L83_MainBusEnergyManagementController",6.34500f,1.80730f,5.59600f,"mint"),
            ("W020_OUT","W020 ▸","P196","L83_MainBusEnergyManagementController",6.34500f,2.72730f,5.59600f,"mint"),
            ("W023_IN","W023 ◂","P196","L83_MainBusEnergyManagementController",6.57500f,2.26730f,5.59600f,"blue"),
            ("W026_OUT","W026 ▸","P196","L83_MainBusEnergyManagementController",6.57500f,2.03730f,5.59600f,"mint"),
            ("W029_IN","W029 ◂","P196","L83_MainBusEnergyManagementController",6.57500f,2.49730f,5.59600f,"blue"),
            ("W039_IN","W039 ◂","P196","L83_MainBusEnergyManagementController",5.88500f,2.95730f,5.59600f,"blue"),
            ("W042_IN","W042 ◂","P196","L83_MainBusEnergyManagementController",5.65500f,2.95730f,5.59600f,"blue"),
            ("W078_IN","W078 ◂","P196","L83_MainBusEnergyManagementController",6.11500f,2.95730f,5.59600f,"mint"),
            ("W090_OUT","W090 ▸","P196","L83_MainBusEnergyManagementController",5.19500f,1.80730f,5.59600f,"mint"),
            ("W102_IN","W102 ◂","P196","L83_MainBusEnergyManagementController",5.19500f,2.72730f,5.59600f,"mint"),
            ("W105_IN","W105 ◂","P196","L83_MainBusEnergyManagementController",6.57500f,1.80730f,5.59600f,"blue"),
            ("W108_OUT","W108 ▸","P196","L83_MainBusEnergyManagementController",6.57500f,2.72730f,5.59600f,"mint"),
            ("W112_OUT","W112 ▸","P196","L83_MainBusEnergyManagementController",5.42500f,2.95730f,5.59600f,"mint"),
            ("W125_OUT","W125 ▸","P196","L83_MainBusEnergyManagementController",6.34500f,2.95730f,5.59600f,"mint"),
            ("W133_OUT","W133 ▸","P196","L83_MainBusEnergyManagementController",4.96500f,2.26730f,5.59600f,"blue"),
            ("W138_OUT","W138 ▸","P196","L83_MainBusEnergyManagementController",6.80500f,2.26730f,5.59600f,"mint"),
            ("W011_IN","W011 ◂","P007","L83_SolarWingPortHingeAssembly",-2.43655f,4.96000f,-0.48200f,"mint"),
            ("W012_OUT","W012 ▸","P008","L83_SolarWingPortHingeAssembly",-2.26655f,4.96000f,-0.48200f,"blue"),
            ("W012_IN","W012 ◂","P197","L83_MainBusEnergyManagementController",5.71500f,2.42730f,5.59600f,"blue"),
            ("W015_OUT","W015 ▸","P197","L83_MainBusEnergyManagementController",5.02500f,1.96730f,5.59600f,"mint"),
            ("W018_IN","W018 ◂","P197","L83_MainBusEnergyManagementController",5.02500f,2.88730f,5.59600f,"blue"),
            ("W021_IN","W021 ◂","P197","L83_MainBusEnergyManagementController",5.25500f,3.11730f,5.59600f,"blue"),
            ("W024_OUT","W024 ▸","P197","L83_MainBusEnergyManagementController",4.79500f,2.42730f,5.59600f,"mint"),
            ("W027_IN","W027 ◂","P197","L83_MainBusEnergyManagementController",4.79500f,2.65730f,5.59600f,"blue"),
            ("W035_OUT","W035 ▸","P197","L83_MainBusEnergyManagementController",5.71500f,3.34730f,5.59600f,"mint"),
            ("W040_IN","W040 ◂","P197","L83_MainBusEnergyManagementController",5.02500f,3.11730f,5.59600f,"blue"),
            ("W048_OUT","W048 ▸","P197","L83_MainBusEnergyManagementController",4.79500f,1.96730f,5.59600f,"mint"),
            ("W087_OUT","W087 ▸","P197","L83_MainBusEnergyManagementController",4.79500f,2.88730f,5.59600f,"mint"),
            ("W093_IN","W093 ◂","P197","L83_MainBusEnergyManagementController",5.48500f,3.34730f,5.59600f,"blue"),
            ("W103_IN","W103 ◂","P197","L83_MainBusEnergyManagementController",5.94500f,3.34730f,5.59600f,"mint"),
            ("W106_IN","W106 ◂","P197","L83_MainBusEnergyManagementController",5.25500f,3.34730f,5.59600f,"blue"),
            ("W109_IN","W109 ◂","P197","L83_MainBusEnergyManagementController",6.17500f,3.34730f,5.59600f,"blue"),
            ("W116_OUT","W116 ▸","P197","L83_MainBusEnergyManagementController",4.56500f,2.42730f,5.59600f,"mint"),
            ("W127_IN","W127 ◂","P197","L83_MainBusEnergyManagementController",4.56500f,2.19730f,5.59600f,"mint"),
            ("W134_IN","W134 ◂","P197","L83_MainBusEnergyManagementController",4.56500f,2.65730f,5.59600f,"mint"),
            ("W013_IN","W013 ◂","P010","L83_SolarWingStarboardHingeAssembly",2.29595f,4.96000f,-0.48200f,"mint"),
            ("W014_OUT","W014 ▸","P011","L83_SolarWingStarboardHingeAssembly",2.46595f,4.96000f,-0.48200f,"blue"),
            ("W015_IN","W015 ◂","P013","L83_SolarArrayPortTrackingGimbal",-3.18500f,5.01901f,-0.44419f,"mint"),
            ("W016_OUT","W016 ▸","P014","L83_SolarArrayPortTrackingGimbal",-3.01500f,5.01901f,-0.44419f,"blue"),
            ("W017_IN","W017 ◂","P016","L83_SolarArrayStarboardTrackingGimbal",3.01500f,5.01901f,-0.44419f,"mint"),
            ("W018_OUT","W018 ▸","P017","L83_SolarArrayStarboardTrackingGimbal",3.18500f,5.01901f,-0.44419f,"blue"),
            ("W019_OUT","W019 ▸","P019","L83_SolarArrayAzimuthEncoder",-2.83500f,5.12000f,-0.38200f,"blue"),
            ("W020_IN","W020 ◂","P020","L83_SolarArrayAzimuthEncoder",-2.66500f,5.12000f,-0.38200f,"mint"),
            ("W021_OUT","W021 ▸","P022","L83_ProgrammableSolarIlluminationArc",-11.11628f,7.82000f,-3.20200f,"blue"),
            ("W022_OUT","W022 ▸","P025","L83_EarthShadowOccluderStage",8.71500f,7.70000f,-3.52450f,"blue"),
            ("W023_OUT","W023 ▸","P028","L83_OrbitalDayNightHologram",-0.08502f,7.54000f,-4.25200f,"blue"),
            ("W024_IN","W024 ◂","P180","L83_SolarGimbalJamSimulator",-3.58500f,5.56500f,-0.46628f,"mint"),
            ("W025_OUT","W025 ▸","P181","L83_SolarGimbalJamSimulator",-3.41500f,5.56500f,-0.46628f,"blue"),
            ("W026_IN","W026 ◂","P031","L83_SolarArrayCellStringMap",-6.23500f,5.52000f,-0.28700f,"mint"),
            ("W027_OUT","W027 ▸","P032","L83_SolarArrayCellStringMap",-6.06500f,5.52000f,-0.28700f,"blue"),
            ("W028_IN","W028 ◂","P043","L83_SolarArrayCombinerBox",-7.18500f,2.06250f,4.23800f,"mint"),
            ("W047_OUT","W047 ▸","P043","L83_SolarArrayCombinerBox",-7.41500f,2.06250f,4.23800f,"blue"),
            ("W029_OUT","W029 ▸","P044","L83_SolarArrayCombinerBox",-7.01500f,2.06250f,4.23800f,"blue"),
            ("W030_OUT","W030 ▸","P015","L83_SolarArrayPortTrackingGimbal",-3.18500f,5.17901f,-0.44419f,"blue"),
            ("W030_IN","W030 ◂","P193","L83_EPSPowerSafetyPLC",10.88500f,1.56730f,6.64600f,"blue"),
            ("W129_IN","W129 ◂","P193","L83_EPSPowerSafetyPLC",11.11500f,1.56730f,6.64600f,"blue"),
            ("W132_IN","W132 ◂","P193","L83_EPSPowerSafetyPLC",11.11500f,1.33730f,6.64600f,"blue"),
            ("W137_IN","W137 ◂","P193","L83_EPSPowerSafetyPLC",11.11500f,1.79730f,6.64600f,"blue"),
            ("W031_OUT","W031 ▸","P046","L83_SolarArrayCurrentSensors",-10.58500f,1.40220f,4.18900f,"blue"),
            ("W031_IN","W031 ◂","P053","L83_MPPTControllerPort",-10.71500f,2.07000f,5.74800f,"blue"),
            ("W041_OUT","W041 ▸","P053","L83_MPPTControllerPort",-10.48500f,2.07000f,5.74800f,"blue"),
            ("W032_OUT","W032 ▸","P049","L83_SolarArrayVoltageSensors",-9.25500f,1.40220f,4.18900f,"blue"),
            ("W032_IN","W032 ◂","P054","L83_MPPTControllerPort",-10.88500f,2.23000f,5.74800f,"blue"),
            ("W043_IN","W043 ◂","P054","L83_MPPTControllerPort",-10.88500f,2.46000f,5.74800f,"blue"),
            ("W033_OUT","W033 ▸","P047","L83_SolarArrayCurrentSensors",-10.41500f,1.40220f,4.18900f,"blue"),
            ("W033_IN","W033 ◂","P057","L83_MPPTControllerStarboard",-9.36500f,2.07000f,5.74800f,"blue"),
            ("W042_OUT","W042 ▸","P057","L83_MPPTControllerStarboard",-9.13500f,2.07000f,5.74800f,"blue"),
            ("W034_OUT","W034 ▸","P050","L83_SolarArrayVoltageSensors",-9.08500f,1.40220f,4.18900f,"blue"),
            ("W034_IN","W034 ◂","P058","L83_MPPTControllerStarboard",-9.53500f,2.23000f,5.74800f,"blue"),
            ("W044_IN","W044 ◂","P058","L83_MPPTControllerStarboard",-9.53500f,2.46000f,5.74800f,"blue"),
            ("W035_IN","W035 ◂","P055","L83_MPPTControllerPort",-10.71500f,2.23000f,5.74800f,"mint"),
            ("W045_IN","W045 ◂","P055","L83_MPPTControllerPort",-10.48500f,2.46000f,5.74800f,"blue"),
            ("W036_IN","W036 ◂","P059","L83_MPPTControllerStarboard",-9.36500f,2.23000f,5.74800f,"mint"),
            ("W037_IN","W037 ◂","P063","L83_PowerConditioningConverterA",-8.14500f,1.84815f,5.70150f,"mint"),
            ("W050_OUT","W050 ▸","P063","L83_PowerConditioningConverterA",-8.37500f,1.84815f,5.70150f,"mint"),
            ("W038_IN","W038 ◂","P067","L83_PowerConditioningConverterB",-6.88500f,1.84815f,5.70150f,"mint"),
            ("W051_OUT","W051 ▸","P067","L83_PowerConditioningConverterB",-7.11500f,1.84815f,5.70150f,"mint"),
            ("W039_OUT","W039 ▸","P064","L83_PowerConditioningConverterA",-7.97500f,1.84815f,5.70150f,"blue"),
            ("W040_OUT","W040 ▸","P068","L83_PowerConditioningConverterB",-6.71500f,1.84815f,5.70150f,"blue"),
            ("W043_OUT","W043 ▸","P034","L83_PVStringPortIVTracer",-11.33500f,2.26400f,3.48500f,"blue"),
            ("W044_OUT","W044 ▸","P037","L83_PVStringStarboardIVTracer",-10.00500f,2.26400f,3.48500f,"blue"),
            ("W045_OUT","W045 ▸","P040","L83_PVStringBypassDiodeBank",-8.68500f,1.68250f,4.43050f,"blue"),
            ("W046_OUT","W046 ▸","P045","L83_SolarArrayCombinerBox",-7.18500f,2.22250f,4.23800f,"blue"),
            ("W046_IN","W046 ◂","P065","L83_PowerConditioningConverterA",-8.14500f,2.00815f,5.70150f,"blue"),
            ("W047_IN","W047 ◂","P069","L83_PowerConditioningConverterB",-6.88500f,2.00815f,5.70150f,"blue"),
            ("W048_IN","W048 ◂","P066","L83_PowerConditioningConverterA",-7.97500f,2.00815f,5.70150f,"mint"),
            ("W049_IN","W049 ◂","P070","L83_PowerConditioningConverterB",-6.71500f,2.00815f,5.70150f,"mint"),
            ("W050_IN","W050 ◂","P071","L83_RegulatedMainDCBus",-0.08500f,1.36750f,2.07300f,"mint"),
            ("W077_OUT","W077 ▸","P071","L83_RegulatedMainDCBus",-0.31500f,1.36750f,2.07300f,"mint"),
            ("W051_IN","W051 ◂","P072","L83_RegulatedMainDCBus",0.08500f,1.36750f,2.07300f,"mint"),
            ("W079_OUT","W079 ▸","P072","L83_RegulatedMainDCBus",0.31500f,1.36750f,2.07300f,"mint"),
            ("W052_OUT","W052 ▸","P060","L83_MPPTPerturbObserveDisplay",-8.03500f,3.24450f,4.00300f,"blue"),
            ("W052_IN","W052 ◂","P202","L83_OrbitalPowerSCADA",0.08500f,7.18930f,-4.88100f,"blue"),
            ("W098_IN","W098 ◂","P202","L83_OrbitalPowerSCADA",0.31500f,7.18930f,-4.88100f,"blue"),
            ("W110_IN","W110 ◂","P202","L83_OrbitalPowerSCADA",0.31500f,6.95930f,-4.88100f,"mint"),
            ("W134_OUT","W134 ▸","P202","L83_OrbitalPowerSCADA",0.31500f,7.41930f,-4.88100f,"mint"),
            ("W053_OUT","W053 ▸","P089","L83_BatteryPackA",7.69500f,2.34500f,2.55550f,"mint"),
            ("W053_IN","W053 ◂","P113","L83_BatteryManagementController",7.62500f,1.48950f,4.51700f,"mint"),
            ("W057_IN","W057 ◂","P113","L83_BatteryManagementController",7.85500f,1.48950f,4.51700f,"blue"),
            ("W061_OUT","W061 ▸","P113","L83_BatteryManagementController",7.85500f,1.25950f,4.51700f,"mint"),
            ("W065_IN","W065 ◂","P113","L83_BatteryManagementController",7.85500f,1.71950f,4.51700f,"blue"),
            ("W074_IN","W074 ◂","P113","L83_BatteryManagementController",8.08500f,1.48950f,4.51700f,"mint"),
            ("W054_OUT","W054 ▸","P093","L83_BatteryPackB",10.01500f,2.34500f,2.55550f,"mint"),
            ("W054_IN","W054 ◂","P114","L83_BatteryManagementController",7.45500f,1.64950f,4.51700f,"mint"),
            ("W058_OUT","W058 ▸","P114","L83_BatteryManagementController",7.45500f,1.87950f,4.51700f,"mint"),
            ("W062_OUT","W062 ▸","P114","L83_BatteryManagementController",7.68500f,1.87950f,4.51700f,"mint"),
            ("W071_IN","W071 ◂","P114","L83_BatteryManagementController",6.99500f,1.64950f,4.51700f,"mint"),
            ("W098_OUT","W098 ▸","P114","L83_BatteryManagementController",7.45500f,2.10950f,4.51700f,"blue"),
            ("W055_OUT","W055 ▸","P100","L83_BatteryCellVoltageTapArray",7.45500f,3.49500f,2.43300f,"blue"),
            ("W055_IN","W055 ◂","P115","L83_BatteryManagementController",7.62500f,1.64950f,4.51700f,"blue"),
            ("W059_IN","W059 ◂","P115","L83_BatteryManagementController",8.08500f,1.87950f,4.51700f,"blue"),
            ("W063_IN","W063 ◂","P115","L83_BatteryManagementController",7.85500f,2.10950f,4.51700f,"blue"),
            ("W072_OUT","W072 ▸","P115","L83_BatteryManagementController",8.31500f,1.64950f,4.51700f,"blue"),
            ("W137_OUT","W137 ▸","P115","L83_BatteryManagementController",7.16500f,2.10950f,4.51700f,"blue"),
            ("W056_OUT","W056 ▸","P104","L83_BatteryCurrentHallSensor",8.91500f,3.47000f,2.50545f,"blue"),
            ("W057_OUT","W057 ▸","P108","L83_BatteryThermalSensorGrid",10.37244f,3.40536f,2.30180f,"blue"),
            ("W058_IN","W058 ◂","P116","L83_BatteryBalancingCircuit",8.91500f,1.48950f,4.51700f,"mint"),
            ("W059_OUT","W059 ▸","P117","L83_BatteryBalancingCircuit",9.08500f,1.48950f,4.51700f,"blue"),
            ("W060_IN","W060 ◂","P120","L83_BatteryChargeController",10.36500f,1.48950f,4.51700f,"mint"),
            ("W061_IN","W061 ◂","P123","L83_BatteryDischargeController",7.49500f,1.45950f,5.82700f,"mint"),
            ("W062_IN","W062 ◂","P126","L83_BatteryIsolationContactor",8.93500f,1.57000f,5.70300f,"mint"),
            ("W063_OUT","W063 ▸","P127","L83_BatteryIsolationContactor",9.10500f,1.57000f,5.70300f,"blue"),
            ("W064_IN","W064 ◂","P129","L83_BatteryPrechargeResistorModule",10.36500f,1.45950f,5.82700f,"mint"),
            ("W065_OUT","W065 ▸","P130","L83_BatteryPrechargeResistorModule",10.53500f,1.45950f,5.82700f,"blue"),
            ("W066_OUT","W066 ▸","P121","L83_BatteryChargeController",10.53500f,1.48950f,4.51700f,"mint"),
            ("W066_IN","W066 ◂","P090","L83_BatteryPackA",7.86500f,2.34500f,2.55550f,"mint"),
            ("W067_OUT","W067 ▸","P122","L83_BatteryChargeController",10.36500f,1.64950f,4.51700f,"mint"),
            ("W067_IN","W067 ◂","P094","L83_BatteryPackB",10.18500f,2.34500f,2.55550f,"mint"),
            ("W068_OUT","W068 ▸","P091","L83_BatteryPackA",7.69500f,2.50500f,2.55550f,"mint"),
            ("W068_IN","W068 ◂","P124","L83_BatteryDischargeController",7.66500f,1.45950f,5.82700f,"mint"),
            ("W069_OUT","W069 ▸","P095","L83_BatteryPackB",10.01500f,2.50500f,2.55550f,"mint"),
            ("W069_IN","W069 ◂","P125","L83_BatteryDischargeController",7.49500f,1.61950f,5.82700f,"mint"),
            ("W070_OUT","W070 ▸","P109","L83_BatteryThermalSensorGrid",10.54244f,3.40536f,2.30180f,"blue"),
            ("W070_IN","W070 ◂","P135","L83_BatteryThermalControlPlate",8.46500f,1.00000f,4.55800f,"blue"),
            ("W071_OUT","W071 ▸","P136","L83_BatteryThermalControlPlate",8.63500f,1.00000f,4.55800f,"mint"),
            ("W072_IN","W072 ◂","P138","L83_BatteryRadiatorFinAssembly",10.61500f,0.91750f,4.51800f,"blue"),
            ("W073_OUT","W073 ▸","P139","L83_BatteryRadiatorFinAssembly",10.78500f,0.91750f,4.51800f,"blue"),
            ("W074_OUT","W074 ▸","P183","L83_BatteryCellImbalanceSimulator",8.76500f,4.06500f,1.83800f,"mint"),
            ("W075_OUT","W075 ▸","P073","L83_RegulatedMainDCBus",-0.08500f,1.52750f,2.07300f,"blue"),
            ("W089_IN","W089 ◂","P073","L83_RegulatedMainDCBus",-0.08500f,1.75750f,2.07300f,"blue"),
            ("W075_IN","W075 ◂","P076","L83_BusVoltageTransducer",-5.19500f,1.45220f,5.41900f,"blue"),
            ("W105_OUT","W105 ▸","P076","L83_BusVoltageTransducer",-4.96500f,1.45220f,5.41900f,"blue"),
            ("W076_OUT","W076 ▸","P074","L83_RegulatedMainDCBus",0.08500f,1.52750f,2.07300f,"blue"),
            ("W092_OUT","W092 ▸","P074","L83_RegulatedMainDCBus",0.31500f,1.75750f,2.07300f,"blue"),
            ("W076_IN","W076 ◂","P079","L83_BusCurrentShuntMonitor",-4.28500f,1.45220f,5.41900f,"blue"),
            ("W077_IN","W077 ◂","P083","L83_BusBulkCapacitorModule",-3.23500f,1.57010f,5.56413f,"mint"),
            ("W078_OUT","W078 ▸","P084","L83_BusBulkCapacitorModule",-3.06500f,1.57010f,5.56413f,"mint"),
            ("W079_IN","W079 ◂","P142","L83_PowerDistributionUnit",0.68500f,1.92000f,4.29600f,"mint"),
            ("W088_OUT","W088 ▸","P142","L83_PowerDistributionUnit",0.91500f,1.92000f,4.29600f,"blue"),
            ("W080_OUT","W080 ▸","P143","L83_PowerDistributionUnit",0.51500f,2.08000f,4.29600f,"mint"),
            ("W091_IN","W091 ◂","P143","L83_PowerDistributionUnit",0.51500f,2.31000f,4.29600f,"blue"),
            ("W080_IN","W080 ◂","P145","L83_PriorityLoadSwitchingRack",2.38500f,1.92000f,4.36600f,"mint"),
            ("W083_OUT","W083 ▸","P145","L83_PriorityLoadSwitchingRack",2.61500f,1.92000f,4.36600f,"mint"),
            ("W086_OUT","W086 ▸","P145","L83_PriorityLoadSwitchingRack",2.61500f,1.69000f,4.36600f,"mint"),
            ("W081_OUT","W081 ▸","P146","L83_PriorityLoadSwitchingRack",2.21500f,2.08000f,4.36600f,"mint"),
            ("W084_OUT","W084 ▸","P146","L83_PriorityLoadSwitchingRack",2.21500f,2.31000f,4.36600f,"blue"),
            ("W116_IN","W116 ◂","P146","L83_PriorityLoadSwitchingRack",1.98500f,2.31000f,4.36600f,"mint"),
            ("W081_IN","W081 ◂","P147","L83_SafetyEssentialLoadBus",4.41500f,1.59250f,4.33800f,"mint"),
            ("W082_IN","W082 ◂","P150","L83_AttitudeControlLoadEmulator",-1.98500f,2.20770f,4.53200f,"mint"),
            ("W083_IN","W083 ◂","P153","L83_MissionComputerLoadEmulator",-0.78500f,2.20770f,4.53200f,"mint"),
            ("W084_IN","W084 ◂","P156","L83_OpticalTelemetryLoadEmulator",0.46500f,2.20770f,4.53200f,"blue"),
            ("W124_OUT","W124 ▸","P156","L83_OpticalTelemetryLoadEmulator",0.23500f,2.20770f,4.53200f,"blue"),
            ("W085_IN","W085 ◂","P159","L83_SciencePayloadLoadEmulator",1.71500f,2.20770f,4.53200f,"mint"),
            ("W086_IN","W086 ◂","P162","L83_HeaterThermalLoadEmulator",3.03500f,2.20770f,4.53200f,"mint"),
            ("W089_OUT","W089 ▸","P132","L83_SupercapacitorRideThroughModule",7.11500f,2.55000f,4.63408f,"blue"),
            ("W090_IN","W090 ◂","P133","L83_SupercapacitorRideThroughModule",7.28500f,2.55000f,4.63408f,"mint"),
            ("W091_OUT","W091 ▸","P190","L83_IndependentUndervoltageProtector",5.18500f,1.57250f,6.00150f,"blue"),
            ("W129_OUT","W129 ▸","P190","L83_IndependentUndervoltageProtector",5.41500f,1.57250f,6.00150f,"blue"),
            ("W092_IN","W092 ◂","P191","L83_IndependentUndervoltageProtector",5.01500f,1.73250f,6.00150f,"blue"),
            ("W093_OUT","W093 ▸","P086","L83_HighSpeedPowerTransientRecorder",-2.03500f,1.55800f,5.76300f,"blue"),
            ("W107_IN","W107 ◂","P086","L83_HighSpeedPowerTransientRecorder",-2.26500f,1.55800f,5.76300f,"mint"),
            ("W094_OUT","W094 ▸","P077","L83_BusVoltageTransducer",-5.36500f,1.61220f,5.41900f,"blue"),
            ("W094_IN","W094 ◂","P087","L83_HighSpeedPowerTransientRecorder",-1.86500f,1.55800f,5.76300f,"blue"),
            ("W110_OUT","W110 ▸","P087","L83_HighSpeedPowerTransientRecorder",-1.63500f,1.55800f,5.76300f,"mint"),
            ("W095_OUT","W095 ▸","P080","L83_BusCurrentShuntMonitor",-4.11500f,1.45220f,5.41900f,"blue"),
            ("W095_IN","W095 ◂","P088","L83_HighSpeedPowerTransientRecorder",-2.03500f,1.71800f,5.76300f,"blue"),
            ("W096_OUT","W096 ▸","P078","L83_BusVoltageTransducer",-5.19500f,1.61220f,5.41900f,"blue"),
            ("W096_IN","W096 ◂","P203","L83_OrbitalPowerSCADA",-0.08500f,7.34930f,-4.88100f,"blue"),
            ("W099_IN","W099 ◂","P203","L83_OrbitalPowerSCADA",-0.08500f,7.57930f,-4.88100f,"blue"),
            ("W126_IN","W126 ◂","P203","L83_OrbitalPowerSCADA",-0.31500f,7.57930f,-4.88100f,"blue"),
            ("W135_IN","W135 ◂","P203","L83_OrbitalPowerSCADA",0.14500f,7.57930f,-4.88100f,"mint"),
            ("W097_OUT","W097 ▸","P081","L83_BusCurrentShuntMonitor",-4.28500f,1.61220f,5.41900f,"blue"),
            ("W099_OUT","W099 ▸","P048","L83_SolarArrayCurrentSensors",-10.58500f,1.56220f,4.18900f,"blue"),
            ("W100_OUT","W100 ▸","P051","L83_SolarArrayVoltageSensors",-9.25500f,1.56220f,4.18900f,"blue"),
            ("W101_OUT","W101 ▸","P168","L83_LoadForecastEnergyBudgetAnalyzer",-2.48500f,1.56190f,6.28500f,"mint"),
            ("W102_OUT","W102 ▸","P171","L83_EclipseEntryPredictionConsole",-1.08500f,1.56190f,6.28500f,"mint"),
            ("W103_OUT","W103 ▸","P174","L83_SunlightReturnChargePlanner",0.31500f,1.56190f,6.28500f,"mint"),
            ("W108_IN","W108 ◂","P169","L83_LoadForecastEnergyBudgetAnalyzer",-2.31500f,1.56190f,6.28500f,"mint"),
            ("W109_OUT","W109 ▸","P177","L83_ContingencyPowerFaultInjector",1.96500f,1.51190f,6.33500f,"blue"),
            ("W111_OUT","W111 ▸","P165","L83_MissionTaskPowerScheduler",-3.98500f,1.56190f,6.28500f,"mint"),
            ("W114_IN","W114 ◂","P165","L83_MissionTaskPowerScheduler",-4.21500f,1.56190f,6.28500f,"mint"),
            ("W118_IN","W118 ◂","P165","L83_MissionTaskPowerScheduler",-3.98500f,1.33190f,6.28500f,"mint"),
            ("W121_IN","W121 ◂","P165","L83_MissionTaskPowerScheduler",-4.21500f,1.33190f,6.28500f,"mint"),
            ("W112_IN","W112 ◂","P166","L83_MissionTaskPowerScheduler",-3.81500f,1.56190f,6.28500f,"mint"),
            ("W115_IN","W115 ◂","P166","L83_MissionTaskPowerScheduler",-3.58500f,1.56190f,6.28500f,"mint"),
            ("W119_IN","W119 ◂","P166","L83_MissionTaskPowerScheduler",-3.58500f,1.33190f,6.28500f,"blue"),
            ("W122_OUT","W122 ▸","P166","L83_MissionTaskPowerScheduler",-3.58500f,1.79190f,6.28500f,"mint"),
            ("W113_OUT","W113 ▸","P170","L83_LoadForecastEnergyBudgetAnalyzer",-2.48500f,1.72190f,6.28500f,"mint"),
            ("W113_IN","W113 ◂","P167","L83_MissionTaskPowerScheduler",-3.98500f,1.72190f,6.28500f,"mint"),
            ("W117_IN","W117 ◂","P167","L83_MissionTaskPowerScheduler",-3.98500f,1.95190f,6.28500f,"mint"),
            ("W120_IN","W120 ◂","P167","L83_MissionTaskPowerScheduler",-4.21500f,1.95190f,6.28500f,"mint"),
            ("W126_OUT","W126 ▸","P167","L83_MissionTaskPowerScheduler",-3.75500f,1.95190f,6.28500f,"blue"),
            ("W114_OUT","W114 ▸","P172","L83_EclipseEntryPredictionConsole",-0.91500f,1.56190f,6.28500f,"mint"),
            ("W115_OUT","W115 ▸","P175","L83_SunlightReturnChargePlanner",0.48500f,1.56190f,6.28500f,"mint"),
            ("W117_OUT","W117 ▸","P151","L83_AttitudeControlLoadEmulator",-1.81500f,2.20770f,4.53200f,"mint"),
            ("W118_OUT","W118 ▸","P154","L83_MissionComputerLoadEmulator",-0.61500f,2.20770f,4.53200f,"mint"),
            ("W119_OUT","W119 ▸","P157","L83_OpticalTelemetryLoadEmulator",0.63500f,2.20770f,4.53200f,"blue"),
            ("W120_OUT","W120 ▸","P160","L83_SciencePayloadLoadEmulator",1.88500f,2.20770f,4.53200f,"mint"),
            ("W121_OUT","W121 ▸","P163","L83_HeaterThermalLoadEmulator",3.20500f,2.20770f,4.53200f,"mint"),
            ("W123_OUT","W123 ▸","P186","L83_OpticalBurstDemandInjector",3.16500f,1.34500f,6.10800f,"mint"),
            ("W123_IN","W123 ◂","P158","L83_OpticalTelemetryLoadEmulator",0.46500f,2.36770f,4.53200f,"mint"),
            ("W125_IN","W125 ◂","P187","L83_OpticalBurstDemandInjector",3.33500f,1.34500f,6.10800f,"mint"),
            ("W127_OUT","W127 ▸","P194","L83_EPSPowerSafetyPLC",10.71500f,1.72730f,6.64600f,"mint"),
            ("W130_IN","W130 ◂","P194","L83_EPSPowerSafetyPLC",10.71500f,1.95730f,6.64600f,"blue"),
            ("W135_OUT","W135 ▸","P194","L83_EPSPowerSafetyPLC",10.48500f,1.95730f,6.64600f,"mint"),
            ("W130_OUT","W130 ▸","P207","L83_EmergencyPowerIsolationStation",11.74500f,1.57035f,6.43800f,"blue"),
            ("W131_IN","W131 ◂","P128","L83_BatteryIsolationContactor",8.93500f,1.73000f,5.70300f,"mint"),
            ("W138_IN","W138 ◂","P204","L83_MissionSurvivalCertificationBeacon",-0.08500f,8.72000f,-4.92100f,"mint"),
            ("","P001","P001","L83_DeployableSolarWingPort",-6.31333f,5.17000f,-0.47200f,"gold"),
            ("","P002","P002","L83_DeployableSolarWingPort",-6.14333f,5.17000f,-0.47200f,"gold"),
            ("","P003","P003","L83_DeployableSolarWingPort",-6.31333f,5.33000f,-0.47200f,"gold"),
            ("","P004","P004","L83_DeployableSolarWingStarboard",6.14333f,5.17000f,-0.47200f,"gold"),
            ("","P005","P005","L83_DeployableSolarWingStarboard",6.31333f,5.17000f,-0.47200f,"gold"),
            ("","P006","P006","L83_DeployableSolarWingStarboard",6.14333f,5.33000f,-0.47200f,"gold"),
            ("","P009","P009","L83_SolarWingPortHingeAssembly",-2.43655f,5.12000f,-0.48200f,"gold"),
            ("","P012","P012","L83_SolarWingStarboardHingeAssembly",2.29595f,5.12000f,-0.48200f,"gold"),
            ("","P018","P018","L83_SolarArrayStarboardTrackingGimbal",3.01500f,5.17901f,-0.44419f,"gold"),
            ("","P021","P021","L83_SolarArrayAzimuthEncoder",-2.83500f,5.28000f,-0.38200f,"gold"),
            ("","P023","P023","L83_ProgrammableSolarIlluminationArc",-10.94628f,7.82000f,-3.20200f,"gold"),
            ("","P024","P024","L83_ProgrammableSolarIlluminationArc",-11.11628f,7.98000f,-3.20200f,"gold"),
            ("","P026","P026","L83_EarthShadowOccluderStage",8.88500f,7.70000f,-3.52450f,"gold"),
            ("","P027","P027","L83_EarthShadowOccluderStage",8.71500f,7.86000f,-3.52450f,"gold"),
            ("","P029","P029","L83_OrbitalDayNightHologram",0.08498f,7.54000f,-4.25200f,"gold"),
            ("","P030","P030","L83_OrbitalDayNightHologram",-0.08502f,7.70000f,-4.25200f,"gold"),
            ("","P033","P033","L83_SolarArrayCellStringMap",-6.23500f,5.68000f,-0.28700f,"gold"),
            ("","P035","P035","L83_PVStringPortIVTracer",-11.16500f,2.26400f,3.48500f,"gold"),
            ("","P036","P036","L83_PVStringPortIVTracer",-11.33500f,2.42400f,3.48500f,"gold"),
            ("","P038","P038","L83_PVStringStarboardIVTracer",-9.83500f,2.26400f,3.48500f,"gold"),
            ("","P039","P039","L83_PVStringStarboardIVTracer",-10.00500f,2.42400f,3.48500f,"gold"),
            ("","P041","P041","L83_PVStringBypassDiodeBank",-8.51500f,1.68250f,4.43050f,"gold"),
            ("","P042","P042","L83_PVStringBypassDiodeBank",-8.68500f,1.84250f,4.43050f,"gold"),
            ("","P061","P061","L83_MPPTPerturbObserveDisplay",-7.86500f,3.24450f,4.00300f,"gold"),
            ("","P062","P062","L83_MPPTPerturbObserveDisplay",-8.03500f,3.40450f,4.00300f,"gold"),
            ("","P082","P082","L83_BusCurrentShuntMonitor",-4.11500f,1.61220f,5.41900f,"gold"),
            ("","P085","P085","L83_BusBulkCapacitorModule",-3.23500f,1.73010f,5.56413f,"gold"),
            ("","P092","P092","L83_BatteryPackA",7.86500f,2.50500f,2.55550f,"gold"),
            ("","P096","P096","L83_BatteryPackB",10.18500f,2.50500f,2.55550f,"gold"),
            ("","P097","P097","L83_ProtectedBatteryCellTray",8.83500f,1.27000f,3.15550f,"gold"),
            ("","P098","P098","L83_ProtectedBatteryCellTray",9.00500f,1.27000f,3.15550f,"gold"),
            ("","P099","P099","L83_ProtectedBatteryCellTray",8.83500f,1.43000f,3.15550f,"gold"),
            ("","P101","P101","L83_BatteryCellVoltageTapArray",7.62500f,3.49500f,2.43300f,"gold"),
            ("","P102","P102","L83_BatteryCellVoltageTapArray",7.45500f,3.65500f,2.43300f,"gold"),
            ("","P103","P103","L83_BatteryCellVoltageTapArray",7.62500f,3.65500f,2.43300f,"gold"),
            ("","P105","P105","L83_BatteryCurrentHallSensor",9.08500f,3.47000f,2.50545f,"gold"),
            ("","P106","P106","L83_BatteryCurrentHallSensor",8.91500f,3.63000f,2.50545f,"gold"),
            ("","P107","P107","L83_BatteryCurrentHallSensor",9.08500f,3.63000f,2.50545f,"gold"),
            ("","P110","P110","L83_BatteryThermalSensorGrid",10.37244f,3.56536f,2.30180f,"gold"),
            ("","P111","P111","L83_BatteryThermalSensorGrid",10.54244f,3.56536f,2.30180f,"gold"),
            ("","P118","P118","L83_BatteryBalancingCircuit",8.91500f,1.64950f,4.51700f,"gold"),
            ("","P119","P119","L83_BatteryBalancingCircuit",9.08500f,1.64950f,4.51700f,"gold"),
            ("","P131","P131","L83_BatteryPrechargeResistorModule",10.36500f,1.61950f,5.82700f,"gold"),
            ("","P134","P134","L83_SupercapacitorRideThroughModule",7.11500f,2.71000f,4.63408f,"gold"),
            ("","P137","P137","L83_BatteryThermalControlPlate",8.46500f,1.16000f,4.55800f,"gold"),
            ("","P140","P140","L83_BatteryRadiatorFinAssembly",10.61500f,1.07750f,4.51800f,"gold"),
            ("","P148","P148","L83_SafetyEssentialLoadBus",4.58500f,1.59250f,4.33800f,"gold"),
            ("","P149","P149","L83_SafetyEssentialLoadBus",4.41500f,1.75250f,4.33800f,"gold"),
            ("","P152","P152","L83_AttitudeControlLoadEmulator",-1.98500f,2.36770f,4.53200f,"gold"),
            ("","P155","P155","L83_MissionComputerLoadEmulator",-0.78500f,2.36770f,4.53200f,"gold"),
            ("","P161","P161","L83_SciencePayloadLoadEmulator",1.71500f,2.36770f,4.53200f,"gold"),
            ("","P164","P164","L83_HeaterThermalLoadEmulator",3.03500f,2.36770f,4.53200f,"gold"),
            ("","P173","P173","L83_EclipseEntryPredictionConsole",-1.08500f,1.72190f,6.28500f,"gold"),
            ("","P176","P176","L83_SunlightReturnChargePlanner",0.31500f,1.72190f,6.28500f,"gold"),
            ("","P178","P178","L83_ContingencyPowerFaultInjector",2.13500f,1.51190f,6.33500f,"gold"),
            ("","P179","P179","L83_ContingencyPowerFaultInjector",1.96500f,1.67190f,6.33500f,"gold"),
            ("","P182","P182","L83_SolarGimbalJamSimulator",-3.58500f,5.72500f,-0.46628f,"gold"),
            ("","P184","P184","L83_BatteryCellImbalanceSimulator",8.93500f,4.06500f,1.83800f,"gold"),
            ("","P185","P185","L83_BatteryCellImbalanceSimulator",8.76500f,4.22500f,1.83800f,"gold"),
            ("","P188","P188","L83_OpticalBurstDemandInjector",3.16500f,1.50500f,6.10800f,"gold"),
            ("","P205","P205","L83_MissionSurvivalCertificationBeacon",0.08500f,8.72000f,-4.92100f,"gold"),
            ("","P269","P269","L83_MissionSurvivalCertificationBeacon",-0.08500f,8.88000f,-4.92100f,"gold"),
            ("","P208","P208","L83_EmergencyPowerIsolationStation",11.91500f,1.57035f,6.43800f,"gold"),
            ("","P209","P209","L83_EmergencyPowerIsolationStation",11.74500f,1.73035f,6.43800f,"gold"),
            ("","P210","P210","L83_SpacecraftPowerThermalInspectionCamera",5.44500f,4.42000f,2.85800f,"gold"),
            ("","P211","P211","L83_SpacecraftPowerThermalInspectionCamera",5.61500f,4.42000f,2.85800f,"gold"),
            ("","P212","P212","L83_SpacecraftPowerThermalInspectionCamera",5.44500f,4.58000f,2.85800f,"gold"),
        };

        public static IEnumerable<string> TerminalIds => SparkLevel83Wiring.Pins;

        private static readonly Dictionary<string, string> Palette = new()
        {
            {"red","EE485C"},{"blue","1711E2"},{"amber","FFA82B"},{"violet","A282FF"},{"plasma","A45DFF"},
            {"mint","34F6C5"},{"cyan","23DAFD"},{"void","07111C"},{"white","D4F5FF"},{"glass","498BA8"}
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 83")]
        public static void Install()
        {
            SparkLevel83Builder.Create();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (root == null || root.GetComponent<SparkLevel83Process>().rig == null) Fit();
            Register();
            ValidateAssets();
        }

        [MenuItem("Tools/Project Spark/Level 83/Validate Level Assets")]
        public static void ValidateAssets()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var control=root != null ? root.GetComponent<SparkLevel83Process>() : null;
            var bindings=root != null ? root.GetComponent<SparkLevelSceneBindings>() : null;
            if (control==null || bindings==null || control.rig==null || control.rig.GetComponentsInChildren<Transform>(true).Count(item=>item.name.StartsWith("ANIM_",StringComparison.Ordinal))!=56 ||
                control.pins.Length!=269 || control.pins.Distinct().Count()!=269 || 
                !TerminalIds.SequenceEqual(control.pins.Select(pin=>pin.name)) ||
                control.pins.Any(pin=>!bindings.TryGetTerminal(pin.name,out var bound) || bound!=pin) ||
                SparkLevel83Process.Gates.Any(id=>!bindings.IsComponentIdDeclared(id)))
                throw new InvalidOperationException("Level 83 wiring, motion or binding references are incomplete.");
            Debug.Log("[LEVEL 83] Validated 269 distinct terminals, 138 player wires and 6 evaluator gates.",root);
        }

        private static void Register()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool dirty = scene.isDirty;
            var objects = scene.GetRootGameObjects();
            T Find<T>() where T : Component => objects.SelectMany(item => item.GetComponentsInChildren<T>(true)).Single();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "Level_83.asset");
            var drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(Folder + "Level_83_Drawing.asset");
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
            else if (dirty) Debug.Log("MainMenu had unsaved edits. Save the scene to persist Level 83's campaign and teaching entries.");
        }

        private static void Append(Object target, string field, Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Level 83 asset for " + field);
            var data = new SerializedObject(target);
            var list = data.FindProperty(field);
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Project Spark/Level 83/Fit Breadboard Assembly")]
        public static void Fit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before fitting Level 83.");
            var models = Models.Select((name, index) =>
            {
                string path = Folder + $"Models/L83_{index:00}_{name}_Fitted.fbx";
                return AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
                    throw new InvalidOperationException("Missing fitted kit module " + path);
            }).ToArray();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
                throw new InvalidOperationException("The Level 83 prefab is missing.");

            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            GameObject root = null;
            try
            {
                var loader = scene.GetRootGameObjects().SelectMany(item => item.GetComponentsInChildren<SparkLevelLayoutLoader>(true)).Single();
                var layoutRoot = (Transform)new SerializedObject(loader).FindProperty("layoutRoot").objectReferenceValue;
                var board = FindBoard(scene);
                Physics.SyncTransforms();
                root = PrefabUtility.LoadPrefabContents(Prefab);
                Fit(root, models, board, layoutRoot != null ? layoutRoot.localToWorldMatrix : Matrix4x4.identity);
                if (PrefabUtility.SaveAsPrefabAsset(root, Prefab) == null)
                    throw new InvalidOperationException("Unity could not save the Level 83 layout.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 83 fitted to the MainMenu breadboard with 269 equipment-mounted terminals.");
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }

        public static BreadboardGrid FindBoard(Scene scene) => scene.GetRootGameObjects()
            .SelectMany(item => item.GetComponentsInChildren<BreadboardGrid>(true))
            .Single(item => item.name == "bread-board");

        public static Vector3 SurfacePoint(BreadboardGrid grid, Vector3 point, Vector3 up)
        {
            float nearest = float.PositiveInfinity;
            // A hole can return its recessed bottom. Seat the rim on the deck
            // surrounding the hole while keeping its horizontal center snapped.
            Vector3 right = Vector3.ProjectOnPlane(grid.GridOrigin.right, up).normalized * .071f;
            Vector3 forward = Vector3.ProjectOnPlane(grid.GridOrigin.forward, up).normalized * .071f;
            var colliders = grid.GetComponents<Collider>();
            foreach (var offset in new[] { Vector3.zero, right, -right, forward, -forward })
            {
                var ray = new Ray(point + offset + up * 2f, -up);
                foreach (var collider in colliders)
                {
                    if (collider.enabled && collider.Raycast(ray, out var hit, 4f) &&
                        hit.distance < nearest && Vector3.Dot(hit.normal, up) > .8f)
                        nearest = hit.distance;
                }
            }
            if (float.IsPositiveInfinity(nearest))
                throw new InvalidOperationException($"The breadboard has no surface under {point}.");
            return point + up * (2f - nearest);
        }

        private static void Fit(GameObject root, GameObject[] models, BreadboardGrid grid, Matrix4x4 layout)
        {
            var bench = root.transform.Find("Orbital Workbench");
            var bindings = root.GetComponent<SparkLevelSceneBindings>();
            var loop = root.GetComponent<SparkLevel83Process>();
            if (bench == null || bindings == null || loop == null)
                throw new InvalidOperationException("Level 83 is missing its workbench, controls, or scene bindings.");

            var gridData = new SerializedObject(grid);
            float spacing = gridData.FindProperty("spacing").floatValue;
            var halfGrid = new Vector3((gridData.FindProperty("columns").intValue - 1) * spacing / 2, 0,
                (gridData.FindProperty("rows").intValue - 1) * spacing / 2);
            var gridCenter = grid.GridOrigin.TransformPoint(halfGrid);
            Matrix4x4 parentWorld = layout * bench.parent.localToWorldMatrix;
            Vector3 up = parentWorld.MultiplyVector(bench.localRotation * Vector3.up).normalized;
            Vector3 difference = gridCenter - parentWorld.MultiplyPoint3x4(bench.localPosition);
            bench.localPosition += parentWorld.inverse.MultiplyVector(Vector3.ProjectOnPlane(difference, up));
            float width = 2 * grid.GridOrigin.TransformVector(Vector3.right * halfGrid.x).magnitude;
            float depth = 2 * grid.GridOrigin.TransformVector(Vector3.forward * halfGrid.z).magnitude;
            float worldScale = Mathf.Min(width / 2.05f, depth / 1.56f);
            bench.localScale = Vector3.one * (worldScale / parentWorld.MultiplyVector(Vector3.right).magnitude);
            Matrix4x4 workbenchWorld = layout * bench.localToWorldMatrix;
            Vector3 ToWorld(Vector3 point) => workbenchWorld.MultiplyPoint3x4(point);
            Vector3 ToLocal(Vector3 point) => workbenchWorld.inverse.MultiplyPoint3x4(point);
            float floor = ToLocal(SurfacePoint(grid, ToWorld(Vector3.zero), up)).y;

            foreach (var terminal in loop.pins)
                if (terminal.Owner != null)
                    terminal.transform.SetParent(terminal.Owner.transform, true);
            foreach (string name in new[] {"L83_HD_VISUAL_ART", "Equipment terminal panels", "Motion pivots"})
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
            var assembly = Child(bench, "L83_HD_VISUAL_ART", new Vector3(0,floor,-.010f));
            foreach (var model in models)
            {
                var instance = Object.Instantiate(model, assembly, false).transform;
                var markers = instance.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.StartsWith("L83_Board",StringComparison.Ordinal))
                    .ToDictionary(item => item.name);
                Transform Marker(string name) => markers["L83_Board" + name];
                Vector3 Axis(string name) => assembly.InverseTransformVector(Marker(name).position - Marker("Center").position);
                instance.localRotation = Quaternion.Inverse(Quaternion.LookRotation(-Axis("Front"), Axis("Up"))) * instance.localRotation;
                instance.localScale /= Axis("Right").magnitude;
                instance.localPosition -= assembly.InverseTransformPoint(Marker("Center").position);
                if (Axis("Right").x < .99f)
                    throw new InvalidOperationException("Unexpected axes on fitted model " + model.name);
            }

            assembly.localScale = Vector3.one * .95f;
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
                    "tint0D1D2F" or "tint132237"=>"InstrumentPanel",
                    "tint47657C"=>"BrushedTitanium",
                    "tint96B3C3" or "tintCAD8E7"=>"SpacecraftAlloy",
                    "tintC46A32" or "tintDC803E"=>"CopperBus",
                    _ when mesh.name.Contains("Battery") && key!="glass"=>"BatteryCells",
                    _ when mesh.name.Contains("SolarWing") && key!="glass"=>"SolarCells",
                    _ when mesh.name.Contains("Hologram") && key!="glass"=>"OrbitDisplay",
                    _ when mesh.name.Contains("Emergency") && key!="glass"=>"SafetyCoating",
                    _ when mesh.name.Contains("MissionComputer") && key!="glass"=>"ElectronicsPCB",
                    _ when mesh.name.Contains("Contactor") && key!="glass"=>"GoldContact",
                    _ when mesh.name.Contains("Bypass") && key!="glass"=>"BlackCeramic",
                    _=>null
                };

                if((mesh.name.Contains("PanoramicOrbitShadowDome")||mesh.name.Contains("TransparentSpacecraftPowerTestChamber"))
                    &&mesh.bounds.size.x>assembly.lossyScale.x*.43f&&mesh.bounds.size.z>assembly.lossyScale.z*.30f)
                {key="glass";surface="OpticalGlass";}
                var material = MaterialFor(shader,key,surface);

                // The fitted meshes can contain several imported submeshes.
                mesh.sharedMaterials = Enumerable.Repeat(material, mesh.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                if (key is "glass" or "acrylic") mesh.shadowCastingMode = ShadowCastingMode.Off;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loop.eventText == null)
            {
                var oldControls = root.transform.Find("Level 83 Controls");
                if (oldControls != null) Object.DestroyImmediate(oldControls.gameObject);
                SparkLevel83Builder.BuildControls(root.transform,loop,font);
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
                pinData.FindProperty("maxConnections").intValue=SparkLevel83Wiring.Wires.Count(wire=>wire.a==index||wire.b==index);
                pinData.ApplyModifiedPropertiesWithoutUndo();
                Seat(terminal,bench,local,pinLayer,rim,MaterialFor(shader,"void"));
                var tag=Label(mount,font,"Label "+spec.id,spec.label,new Vector3(0,.009f,-.007f),.014f);
                tag.transform.localRotation=Quaternion.Euler(22,0,0);
                loop.pinLabels[index]=tag;
                tag.gameObject.SetActive(false);
            }
            foreach(var body in bench.GetComponentsInChildren<MeshRenderer>(true))
                if(body.name=="Body") body.gameObject.SetActive(false);
            var title=Label(labels,font,"Level name","BEYOND THE SHADOW",new Vector3(0,floor+.018f,-.688f),.023f);
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
            Lamp("Orbital bench key",new Vector3(-.42f,.92f,-.38f),new Color(1,.93f,.81f),2.8f,1.95f);
            Lamp("Solar array fill",new Vector3(.68f,.62f,.21f),new Color(.48f,.77f,1),1.6f,1.55f);
            foreach(var item in assembly.GetComponentsInChildren<Transform>(true))
            {
                int index=item.name.IndexOf("ANIM_",StringComparison.Ordinal);
                if(index>0&&item.name.StartsWith("fitted_",StringComparison.Ordinal)) item.name=item.name[index..];
            }
            loop.rig=assembly.gameObject.AddComponent<SparkLevel83VisualRig>();
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
            string materialName=surface==null?"L83 "+key:"L83 "+key+" "+surface;
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
                    throw new InvalidOperationException("Missing Level 83 surface maps for "+surface);
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





















