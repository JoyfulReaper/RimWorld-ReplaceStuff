# Replace Stuff: Performance Edition

**Replace Stuff: Performance Edition** is a performance-focused fork of the original *Replace Stuff* mod for RimWorld.

The goal is the same: **upgrade buildings in place** without the usual deconstruct, wait, and rebuild loop. This fork keeps the core gameplay while reorganizing the internals around a more explicit replacement pipeline, broader compatibility handling, and cleaner state restoration.

> **⚠️ Alpha Warning:** This mod is still under active development. Back up your saves and report issues with logs and reproduction steps.

---

## What This Version Adds

### Seamless Upgrades

* Replace walls, workbenches, storage, and other supported buildings without manually tearing them down first.
* Builders handle the deconstruct-and-rebuild sequence automatically so colony flow stays intact.

### State and Settings Preservation

* Coolers and heaters keep their temperature targets.
* Workbenches preserve bills and production setup.
* Storage buildings keep settings, priorities, filters, and related state.
* Wall attachments such as lights and vents are handled during wall upgrades.

### Smarter Placement Logic

* Stored items are handled safely during replacement.
* Blueprints can interact with rock, fog, and bridge-adjacent cases more gracefully.
* Over-wall coolers and vents are supported with RimWorld 1.6 compatibility in mind.

---

## Technical Notes

The internal codebase has been reorganized around a centralized replacement flow:

* `ReplacementPipeline` coordinates the replacement sequence.
* `ReplacementValidator` handles cached rule checks and compatibility decisions.
* `ReplacementUtility` and related helpers keep the implementation modular.
* `BuildingStateTransfer` and storage replacement logic preserve persistent data during rebuilds.
* `ReplacementRegistry` and compatibility handlers allow XML- or code-driven integration for supported mods.

The patch layout is now grouped by feature area:

* `Replace/Patches` for replacement flow, reservations, and attachments.
* `NewThing/Patches` for blueprint and frame placement.
* `OverMineable/Patches` for rock, fog, and mining-related cases.
* `PlaceBridges/Patches` for bridge terrain and placement logic.
* `DestroyedRestore/Patches` for state capture and rebuild support.

---

## Compatibility

This fork is centered on RimWorld 1.6 and is intended to be easier to extend for modded buildings and replacement rules than the original layout.

Supported compatibility work currently includes:

* Registry-based replacement handlers.
* Legacy replacement bridges for older data and behavior.
* Third-party support hooks such as QualityBuilder and interchangeable item groups.
* Expanded support for storage, wall attachments, and other complex replacement targets.

---

## Credits

* Original mod concept and foundation: Uuugggg / Alex Tearse-Doyle.
* Fork and performance edition work: Kyle Givler.
* Earlier optimization and compatibility ideas: Hexnet111 and other community contributors.

### Source Code

The code for this project is open source. If you encounter bugs or want to contribute to the performance or compatibility effort, visit:

**[GitHub: RimWorld-ReplaceStuff](https://github.com/JoyfulReaper/RimWorld-ReplaceStuff)**
