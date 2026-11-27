# Remaining issues

- The campaign has 51 source-backed wiring candidates for Levels 43 through 93. Their terminal bindings passed the Editor scan. Each remains a draft until its exact authored revision receives a gameplay review.
- The current scan did not import complete construction recipes for 67 levels. Several mid-campaign evaluators specify required nets inside their mission logic. Those nets still need to be exposed as authored construction data, mapped to native terminal pairs where appropriate, and tested.
- Ten campaign layouts expose native PCB routes or parts. Their completion depends on additional native actions such as placement, soldering, copper routing, via plating, or diagnostics. The regular Wire Tool cannot carry those sequences by itself. Some separate board source packages explicitly mark parts or tasks unverified.
- Levels 1 and 20 use their native supply controls to verify powered objectives after the approved wiring. Level 20 also requires the existing shelf controls to install the four bridge resistors.
- Android work is deferred. Desktop Spark Live text passed its Level 20 check. In-Editor voice playback timed out, and the Piper command-line smoke test produced a WAV successfully.

Level 48 is the first verified physical blocker among the generated process recipes. Step 4 stopped at SENSOR_GND with an 8.681-unit grip gap. The native route and hand stance need correction before the full wiring program and six objectives can be approved.
