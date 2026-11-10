# Level 29 — approved automatic-backup package

Start with **Level29_Creation_Process.md**. This package supersedes ProjectSpark_Level29_Complete_Package.zip and the rejected BRIGHT/ECO lesson.

Install three scripts: one NEW runtime observer, one NEW Editor setup helper, and the REPLACEMENT Level 29 drawing creator. Import Level_29_AUTOMATIC_Backup_Editable.txt into your existing Level_29.asset. Configure the six NEW slot roles using the guide/CSV, then rebuild the circuit. Generating this package did not change the Unity project.

Required sequence: verified wiring -> charge -> MAIN OFF -> automatic isolated backup -> MAIN restored -> recharge. The existing manager remains responsible for completion. Six runtime gate aliases prevent static targets from completing early.

Parts: 5 V source, main switch, three 1N4007 diodes, 220 ohm charge resistor, 10000 uF/16 V capacitor, 330 ohm LED resistor, 2 V LED. No SPDT.

The third diode blocks reverse current through the charging branch, correcting a missing isolation path in the initial two-diode proposal.

Compile and nominal calculation checks passed. Play Mode behavior has not been verified. See VALIDATION.md and acceptance tests in the guide. No shared electrical solver changes are supplied.
