# SparkLevel88VisualRig — verified supplied art-only API

`ProjectSpark.Visuals.SparkLevel88VisualRig` public methods:

```csharp
BindVisuals();
SetActualCO2Processing(float actualBedASaturation01,float actualBedBSaturation01,float actualValveAngleDegrees);
SetActualOxygenProcess(float actualDeliveryFlow01,float actualRegulatorAngleDegrees);
SetActualWaterAndHumidity(float actualWaterPumpSpeed01,float actualCondenserFlow01,float actualFilterDrawerOpen01);
SetActualVentilation(float actualMainFanSpeed01,float actualBackupFanSpeed01,
    float actualDamperAOpen01,float actualDamperBOpen01,float actualDamperCOpen01,float actualDamperDOpen01);
SetActualSafety(bool actualW145TrustedAndFresh,bool actualCriticalFaultLatched,bool actualAtmosphereQualified,
    bool actualLowOxygen,bool actualAirlockOpen,float actualPressureRegulatorDegrees);
```

This component never computes physics, electrical continuity or certification; pass measured real gameplay authority values after separate simulation. It uses 72 authored pivot names. Indicator pivots may remain static and need separately authored screen shader/VFX during AAA finish.
