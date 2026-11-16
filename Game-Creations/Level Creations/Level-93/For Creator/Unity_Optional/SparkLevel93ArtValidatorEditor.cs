#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level93.EditorTools
{
    public static class SparkLevel93ArtValidatorEditor
    {
        public static readonly string[] RequiredPivots = new string[]
        {
            "ANIM_OBSERVATION_GLASS_INDICATOR",
            "ANIM_CEILING_LIGHT_STATUS",
            "ANIM_ARCHIVE_VAULT_LOCK",
            "ANIM_VAULT_GLASS_OPEN",
            "ANIM_KHEPRI_CORE_TURN",
            "ANIM_CORE_CRADLE_SLIDE",
            "ANIM_ELEMENTAL_ARCHIVE_HOLOGRAM",
            "ANIM_COMPOSITION_WALL_DISPLAY",
            "ANIM_CLEAN_ROOM_AIRLOCK",
            "ANIM_SCIENCE_CONTROL_DISPLAY",
            "ANIM_MISSION_BEACON",
            "ANIM_CT_GANTRY_BORE",
            "ANIM_CT_ROTATING_BORE",
            "ANIM_CT_SOURCE_SAFETY_COVER",
            "ANIM_CT_COLLIMATOR_SLIDER",
            "ANIM_CT_DETECTOR_ARRAY",
            "ANIM_CT_PRECISION_STAGE",
            "ANIM_CT_STAGE_SERVO",
            "ANIM_CT_STAGE_ENCODER",
            "ANIM_CT_SAMPLE_TRANSLATION",
            "ANIM_CT_COOLING_FLOW",
            "ANIM_CT_RECONSTRUCTION_DISPLAY",
            "ANIM_CT_CALIBRATION_PHANTOM",
            "ANIM_CT_INTERLOCK_DOOR",
            "ANIM_CT_POWER_CONTACTOR",
            "ANIM_CT_FIDUCIAL_POSITION",
            "ANIM_CT_HOLOGRAM_PROJECTION",
            "ANIM_CT_DIAGNOSTIC_DISPLAY",
            "ANIM_RAMAN_MICROSCOPE_FOCUS",
            "ANIM_RAMAN_LASER_SCAN",
            "ANIM_RAMAN_OPTICAL_STAGE",
            "ANIM_RAMAN_REFERENCE_WAVELENGTH",
            "ANIM_RAMAN_DETECTOR_GATE",
            "ANIM_INFRARED_OPTICAL_SCAN",
            "ANIM_IR_INTERFEROMETER_MIRROR",
            "ANIM_IR_SAMPLE_STAGE",
            "ANIM_IR_COOLING",
            "ANIM_XRF_ENERGY_ANALYZER",
            "ANIM_XRF_SOURCE_SHUTTER",
            "ANIM_XRF_DETECTOR_SCAN",
            "ANIM_XRF_SAMPLE_ALIGN",
            "ANIM_SPECTRAL_FUSION_DISPLAY",
            "ANIM_MINERAL_REFERENCE_CAROUSEL",
            "ANIM_OPTICAL_INTERLOCK",
            "ANIM_SPECTRUM_HOLOGRAM",
            "ANIM_VACUUM_CHAMBER_ROTATION",
            "ANIM_VACUUM_ACCESS_HATCH",
            "ANIM_MICROSAMPLE_HEATER",
            "ANIM_HEATER_THERMOCOUPLE",
            "ANIM_TURBOPUMP_ROTOR",
            "ANIM_BACKING_PUMP_ROTOR",
            "ANIM_VACUUM_ISOLATION_VALVE",
            "ANIM_TRUSTED_PRESSURE_GAUGE",
            "ANIM_RGA_DETECTOR_SCAN",
            "ANIM_QUADRUPOLE_SPECTROMETER",
            "ANIM_ION_SOURCE_EMITTER",
            "ANIM_MASS_FILTER_RODS",
            "ANIM_MASS_DETECTOR_GAIN",
            "ANIM_SAMPLE_GAS_TRAP",
            "ANIM_VOLATILE_COLLECTION",
            "ANIM_VACUUM_PURGE_VALVE",
            "ANIM_THERMAL_CONTROL_READOUT",
            "ANIM_MASS_SPECTRUM_READOUT",
            "ANIM_INERT_GLOVEBOX_LATCH",
            "ANIM_GLOVEBOX_GLASS_DOOR",
            "ANIM_DUAL_GLOVE_PORTS",
            "ANIM_CLEAN_TRANSFER_ARM",
            "ANIM_SAMPLE_CARTRIDGE_A",
            "ANIM_SAMPLE_CARTRIDGE_B",
            "ANIM_MICROSAMPLE_ABLATION",
            "ANIM_SAMPLE_CAROUSEL",
            "ANIM_INERT_PURGE_FLOW",
            "ANIM_CONTAMINATION_WITNESS",
            "ANIM_PRECISION_BALANCE",
            "ANIM_SEAL_COMPRESSION_STAGE",
            "ANIM_SAMPLE_CUSTODY_DISPLAY",
            "ANIM_ARCHIVE_RETURN_LATCH",
            "ANIM_POWER_BUS_STATUS",
            "ANIM_ANALYTICAL_SAFETY_PLC_STATUS",
            "ANIM_SCADA_PANEL_STATUS",
            "ANIM_CT_INTERLOCK_STATUS",
            "ANIM_VACUUM_SAFETY_RELAY",
            "ANIM_THERMAL_FAULT_SWITCH",
            "ANIM_TOMOGRAPHY_FAULT_SWITCH",
            "ANIM_MASS_GAIN_FAULT_SWITCH",
            "ANIM_W179_CRITICAL_PATCH",
            "ANIM_EMERGENCY_STOP",
            "ANIM_MISSION_CERTIFICATION_DISPLAY",
            "ANIM_CORE_OPTICAL_RECONSTRUCTION",
            "ANIM_CT_RADIUS_INSPECTION",
            "ANIM_CT_SOURCE_ORBIT",
            "ANIM_CT_STAGE_Z_FEED"
        };
        [MenuItem("Tools/Project Spark/Levels/93/4. Validate Imported HD Art")]
        public static void Validate()
        {
            var obj=Selection.activeGameObject;
            if(obj==null){Debug.LogError("[L93] Select L93_HD_VISUAL_ART to validate.");return;}
            var found=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int meshcount=0,verts=0,sockets=0;
            foreach(var t in obj.GetComponentsInChildren<Transform>(true))
            {
                found.Add(t.name);
                if(t.name.StartsWith("SOCKET_P")) sockets++;
                var f=t.GetComponent<MeshFilter>();if(f!=null&&f.sharedMesh!=null){meshcount++;verts+=f.sharedMesh.vertexCount;}
            }
            int missing=0;
            foreach(var p in RequiredPivots) if(!found.Contains(p)){missing++;Debug.LogWarning("[L93] Missing pivot "+p);}
            int models=0;
            foreach(Transform t in obj.transform)if(t.name.StartsWith("L93_"))models++;
            Debug.Log("[L93] Art validation: "+models+"/100 model roots, "+(RequiredPivots.Length-missing)+"/92 pivots, "+sockets+"/272 sockets, "+meshcount+" mesh chunks, "+verts+" vertices. Unity Play Mode/electrical safety must be checked separately.");
        }
    }
}
#endif
