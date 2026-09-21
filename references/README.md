# Compile-time API references

This research branch uses deliberately minimal compile-time stubs for verified Graveyard Keeper 1.407 type/member identities.

Rules:
- never ship the stub DLL;
- never add copied game method bodies/data/assets;
- add only signatures directly needed by the research probe;
- verify each signature against the target game/decompile evidence before relying on it;
- verify the build output does not contain `Assembly-CSharp.dll`.

The stub is a compiler boundary only. At runtime the probe binds to Graveyard Keeper's real `Assembly-CSharp`.
