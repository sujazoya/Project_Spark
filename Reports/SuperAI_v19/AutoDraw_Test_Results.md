# Auto Draw v19.1 test results

## Unity Play Mode

- Level 1. The exact command registered all three native wires. After native power-on, the evaluator completed at 1/1. The run returned to Edit Mode successfully.
- Level 20. The authored layout registered all ten wires. Auto pause and resume, the runtime resistor prerequisite, the standing switch, original-arm retraction, and cancellation on level change passed. The powered evaluator reported 5/5. A separate fixture that moved the sockets and reduced arm reach stopped at the real cable-length constraint.
- Level 48. The character reached the original wire 4 destination and the native circuit registered the connection. That run later stopped on wire 10 at a flight-corridor obstruction. A separate flight regression hit a real-time timeout at wire 4. The follow-up aligns motion with the planned corridor and uses the full body sweep at lower frame rates. The logs preserve each attempt.

## Campaign audit

The final coverage table is an asset and gameplay evidence index. Each row reports the active campaign layout, source evidence, revision, draft or review state, dependency checks, execution, official evaluator evidence, and any unresolved requirement. An exported plan is a review candidate. Current gameplay results cover the levels listed above.

## Raw evidence

- `Logs/SuperAI_v19/StarterPlayMode.txt`
- `Logs/SuperAI_v19/Level20PlayMode.txt`
- `Logs/SuperAI_v19/Level48PlayMode.txt`
- `Logs/SuperAI_v19/EditorValidation.txt`

No distribution packaging was run for this revision.

## Final authored audit

Unity resolved and structurally validated all 130 saved campaign layouts. The export includes 2 reviewed plans and 62 drafts containing 4,329 ordered connections. Of the remaining layouts, 10 require a board workflow adapter and 56 had no complete plan that could be sourced and mapped by this scan. The standalone browser navigation and displayed connection counts were checked for all 130 entries.

The Level 100 board fixture compiled but did not reach its first gameplay assertion in the observed Editor session. Native fitting, soldering, routing and diagnosis remain open for end-to-end automation. Level 48 wire 4 has native registration evidence. A further flight carry attempt is tracked separately from the confirmed wire 4 result.

The last Level 48 retry did not reach a gameplay assertion in the Editor session and was stopped. The wire 4 pass comes from the earlier completed native test. The added original-hand flight carry remains unverified in Play Mode.
