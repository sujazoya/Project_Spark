# Project Spark AAAUI v1.2

## Install
1. Delete the old `AAAUI` folder completely from the Unity project before copying this one. Do not merge folders.
2. Copy `AAAUI` into `Assets/`.
3. Let Unity finish importing/compiling.
4. If Unity still reports stale assembly errors, use Assets > Reimport All, then restart Unity.

## Assembly layout
- `Runtime/AAAUI.Runtime.asmdef` contains all runtime code under Runtime/.
- `Editor/AAAUI.Editor.asmdef` contains all editor code and references AAAUI.Runtime.
