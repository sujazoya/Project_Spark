# NEXUS command registry

Availability is determined from the live handler and workbench. Spark Live continues to route free-form questions.

| ID | Domain | Phrase | Route | Mode | Approval | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| mode.manual | Construction | spark manual mode | Engineering | Manual | None | Mode |
| mode.assisted | Construction | spark assisted mode | Engineering | Assisted | None | Mode |
| mode.auto | Construction | spark auto mode | Engineering | Auto | Confirm | Mode |
| wire.draw | Construction | draw current level | Engineering | Auto | Confirm | Plan |
| wire.continue | Construction | spark continue drawing | Engineering | Auto | Confirm | Resume |
| wire.pause | Construction | spark pause drawing | Engineering | Retain | None | Pause |
| wire.resume | Construction | spark resume drawing | Engineering | Retain | None | Resume |
| wire.cancel | Construction | spark stop drawing | Engineering | Retain | None | CancelPlan |
| wire.status | Construction | spark drawing status | Engineering | Retain | None | Reply |
| wire.explain | Construction | spark explain current construction | Engineering | Retain | None | Reply |
| wire.engineering | Construction | spark engineering status | Engineering | Retain | None | Reply |
| wire.preview | Construction | spark preview practice plan | Engineering | Retain | None | Reply |
| wire.suggest | Construction | spark suggest next wire | Engineering | Assisted | None | Suggestion |
| wire.source | Construction | spark show me where the source terminal is | Engineering | Retain | None | Reply |
| wire.failure | Construction | spark explain why this connection failed | Engineering | Retain | None | Reply |
| wire.approve | Construction | spark approve suggested wire | Engineering | Assisted | NativeProposal | Wire |
| wire.approved | Construction | spark start approved plan | Engineering | Auto | Confirm | Plan |
| wire.reload | Construction | spark reload engineering plans | Engineering | Retain | None | Reply |
| wire.narrate.on | Construction | spark engineering narration on | Engineering | Retain | None | Reply |
| wire.narrate.off | Construction | spark engineering narration off | Engineering | Retain | None | Reply |
| wire.record | Construction | start wiring recording | NativeRecorder | Retain | None | Reply |
| wire.record.stop | Construction | stop wiring recording | NativeRecorder | Retain | None | Reply |
| wire.record.status | Construction | spark wiring status | NativeRecorder | Retain | None | Reply |
| wire.record.reload | Construction | reload wiring plans | NativeRecorder | Retain | None | Reply |
| wire.native.status | Construction | native wire status | NativeWire | Retain | None | Reply |
| wire.undo | Construction | undo last AI wire | NativeWire | Retain | Confirm | Observe |
| lab.help | Construction | agent help | Lab | Retain | None | Reply |
| lab.status | Construction | agent status | Lab | Retain | None | Reply |
| lab.preview | Construction | preview level solution | Lab | Retain | None | Reply |
| lab.guide | Teaching | show next objective in 3d | Lab | Retain | None | Reply |
| work.objectives | Workbench | which objective is pending | Ask | Retain | None | Reply |
| work.evidence | Workbench | show objective evidence | Ask | Retain | None | Reply |
| work.diagnose | Workbench | diagnose level | Ask | Retain | None | Reply |
| work.progress | Workbench | show level progress | Ask | Retain | None | Reply |
| work.hint | Workbench | give me a hint | Ask | Retain | None | Reply |
| work.connections | Workbench | show circuit connections | Ask | Retain | None | Reply |
| work.selected | Workbench | point at selected component | Ask | Retain | None | Reply |
| work.positive | Workbench | show me the positive terminal | Ask | Retain | None | Reply |
| work.negative | Workbench | show me the negative terminal | Ask | Retain | None | Reply |
| work.clear | Workbench | clear highlight | Ask | Retain | None | Reply |
| work.inventory | Workbench | show my inventory | Ask | Retain | None | Reply |
| work.tool | Workbench | which tool is active | Ask | Retain | None | Reply |
| work.power | Workbench | power supply settings | Ask | Retain | None | Reply |
| work.voltage | Workbench | what is the voltage | Ask | Retain | None | Reply |
| work.current | Workbench | what is the current | Ask | Retain | None | Reply |
| work.resistance | Workbench | what is the resistance | Ask | Retain | None | Reply |
| work.watts | Workbench | what is the power | Ask | Retain | None | Reply |
| mission.status | Workbench | spark inspect mission | Mission | Retain | None | Reply |
| mission.export | Workbench | spark export mission report | Mission | Retain | None | Reply |
| device.list | Robot | spark inspect devices | Devices | Retain | None | Reply |
| device.status | Robot | spark device status | Devices | Retain | None | Reply |
| device.on | Robot | spark propose device {value} on | Devices | Assisted | None | Proposal |
| device.off | Robot | spark propose device {value} off | Devices | Assisted | None | Proposal |
| device.approve | Robot | spark approve device action | Devices | Assisted | NativeProposal | Device |
| device.cancel | Robot | spark cancel device action | Devices | Retain | None | CancelDevice |
| flight.up | Robot | spark take off | Flight | Retain | None | Observe |
| flight.down | Robot | spark land | Flight | Retain | None | Observe |
| flight.hover | Robot | spark hover | Flight | Retain | None | Observe |
| flight.selected | Robot | spark fly to selected | Flight | Retain | None | Observe |
| flight.status | Robot | spark flight status | Flight | Retain | None | Reply |
| arm.status | Robot | spark arm status | Arms | Retain | None | Reply |
| arm.retract | Robot | spark retract arms | Arms | Retain | None | Observe |
| arm.right | Robot | spark extend right arm to {value} | Arms | Retain | None | Observe |
| arm.left | Robot | spark extend left arm to {value} | Arms | Retain | None | Observe |
| arm.range | Robot | spark set arm reach {value} | Arms | Retain | None | Observe |
| teach.whiteboard | Teaching | show current level on whiteboard | Ask | Retain | None | Reply |
| teach.wiring | Teaching | show live circuit on whiteboard | Ask | Retain | None | Reply |
| teach.objectives | Teaching | draw objective map in 2d | Ask | Retain | None | Reply |
| teach.workbench | Teaching | show current level in 3d | Ask | Retain | None | Reply |
| teach.draw | Teaching | draw level {value} | Ask | Retain | None | Reply |
| teach.models | Teaching | teaching models | Teaching | Retain | None | Reply |
| teach.model | Teaching | show teaching model {value} | Teaching | Retain | None | Reply |
| teach.model.close | Teaching | close teaching model | Teaching | Retain | None | Reply |
| teach.lesson | Teaching | teach me {value} | Ask | Retain | None | Reply |
| teach.definition | Teaching | define {value} | Ask | Retain | None | Reply |
| teach.online | Teaching | online explain {value} | Ask | Retain | None | Reply |
| teach.practice | Teaching | practice | Ask | Retain | None | Reply |
| teach.practice.topic | Teaching | practice {value} | Ask | Retain | None | Reply |
| teach.practice.hint | Teaching | practice hint | Ask | Retain | None | Reply |
| teach.answer | Teaching | answer {value} | Ask | Retain | None | Reply |
| teach.lesson.pause | Teaching | pause the lesson | Ask | Retain | None | Reply |
| teach.lesson.resume | Teaching | continue our lesson | Ask | Retain | None | Reply |
| teach.lesson.next | Teaching | go to the next lesson step | Ask | Retain | None | Reply |
| teach.lesson.stop | Teaching | cancel the lesson | Ask | Retain | None | Reply |
| teach.practice.stop | Teaching | stop practice | Ask | Retain | None | Reply |
| teach.report | Teaching | my learning report | Ask | Retain | None | Reply |
| demo.diode | Teaching | demonstrate diode polarity | Demo | Retain | None | Reply |
| demo.switch | Teaching | demonstrate a switch | Demo | Retain | None | Reply |
| demo.meter | Teaching | demonstrate voltage measurement | Demo | Retain | None | Reply |
| demo.next | Teaching | next demo | Demo | Retain | None | Reply |
| demo.repeat | Teaching | repeat demo | Demo | Retain | None | Reply |
| demo.stop | Teaching | stop demo | Demo | Retain | None | Reply |
| demo.status | Teaching | demo status | Demo | Retain | None | Reply |
| session.chapter | Session | current chapter | Ask | Retain | None | Reply |
| session.catalog | Session | catalog coverage | Ask | Retain | None | Reply |
| session.level | Session | catalog level {value} | Ask | Retain | None | Reply |
| session.stop | Session | stop speaking | Ask | Retain | None | Reply |
| session.mute | Session | mute microphone | Ask | Retain | None | Reply |
| session.coach.on | Session | turn proactive coaching on | Ask | Retain | None | Reply |
| session.coach.off | Session | turn proactive coaching off | Ask | Retain | None | Reply |
| session.reset | Session | reset optional personalization | Ask | Retain | Confirm | Reply |
