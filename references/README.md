# Compile-time API references

The project uses minimal signature-only stubs for verified Graveyard Keeper 1.407 assemblies.

- Assembly-CSharp contains only signatures required by Crafting Planner.
- Assembly-CSharp-firstpass contains only NGUI/localization and native Rewired gamepad-template signatures required by the UI.
- Rewired_Core contains only the verified installed Rewired player/controller/template query signatures.
- None of these stub DLLs may be shipped.
- Production behavior binds to the game's real assemblies at runtime.
- CI fails if any stub DLL leaks into the distribution output.

Add signatures only after verifying their actual owner/shape from target-game evidence.
