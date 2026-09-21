# Compile-time API references

The project uses minimal signature-only stubs for verified Graveyard Keeper 1.407 assemblies.

- Assembly-CSharp contains only signatures required by Crafting Planner.
- Assembly-CSharp-firstpass contains only NGUI/localization signatures required by the native-style HUD and localization lookup.
- Neither stub DLL may be shipped.
- Production behavior binds to the game's real assemblies at runtime.
- CI fails if either stub DLL leaks into the distribution output.

Add signatures only after verifying their actual owner/shape from target-game evidence.
