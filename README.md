# Replace Stuff: Performance Edition

**Replace Stuff: Performance Edition** is a modern, high-performance rewrite of the classic *Replace Stuff* mod for RimWorld.

The goal remains the same: **Upgrade your buildings in-place** without the tedious work of manually deconstructing, waiting, and rebuilding. We’ve rebuilt the core logic from the ground up to be more stable, faster, and easier to support for mod compatibility.

> **⚠️ Alpha Warning:** This mod is currently in active development. We recommend backing up your saves and reporting issues with logs and reproduction steps.

---

## 🛠 For Players: Why use this version?

If you loved the original *Replace Stuff*, you will love the *Performance Edition*. It offers the same gameplay experience but is optimized to ensure your colony runs smoother, especially during large-scale renovation projects.

### Seamless Upgrades

* **Build over existing structures:** For example, easily swap wooden walls for stone walls without manually tearing down your walls first.
* **Smart Construction:** Builders automatically handle the deconstruction and construction sequence, keeping your colony functional throughout the process.

### State & Setting Preservation

Gone are the days of resetting your machines every time you upgrade them. This mod remembers:

* **Temperature:** Your coolers and heaters keep their target temperatures.
* **Bills:** Your workbenches retain their production queues.
* **Storage:** Your storage settings, priorities, and filters are automatically restored.
* **Attachments:** Lights, vents, and other wall-mounted items are preserved during wall upgrades.

### Smarter Building Logic

* **Storage Groups:** Stored items are safely managed during upgrades, so you don't lose track of your inventory.
* **Environment Aware:** Builders can now mine or smooth rock, place bridge blueprints, or clear fogged areas automatically as part of the construction flow.
* **Over-Wall Support:** Continue to use your favorite over-wall coolers and vents with full 1.6 compatibility.

---

## ⚙️ Developer & Technical Information

For fellow developers, modders, and power users, the *Performance Edition* provides a vastly more modular and performant architecture. We have moved away from legacy event hooks in favor of a centralized pipeline.

### Architectural Highlights

* **Centralized Pipeline:** All replacement logic is funneled through the `ReplacementPipeline`, moving away from "patch-everything" hacks to a deterministic flow: *Validate Target -> Extract State -> Destroy -> Spawn -> Apply State.*
* **Rule-Based Validation:** The `ReplacementValidator` now uses a clean, cached, predicate-based system. Adding compatibility for new buildings no longer requires complex Harmony patches.
* **Performance Optimization:** We have implemented aggressive caching for replacement rules and construction costs, drastically reducing CPU overhead during drag-select and large-area renovations.

### Data & State Transfer

* **`BuildingStateTransfer`:** This system handles the capture and reapplication of persistent data. It is decoupled from the `Thing` class, making it easier to extend to modded buildings.
* **`DestroyedBuildingStore`:** A `MapComponent` that safely serializes building metadata, preventing data loss during the frame-transition between destruction and auto-rebuild.

### Compatibility Registry

* **Registry-First Design:** Compatibility is no longer hardcoded in version-specific folders. The `ReplacementRegistry` allows other mods to define interchangeable building groups via XML or C# handlers without needing a hard dependency on this mod.

### Harmony Patch Organization

We have cleaned up the patch landscape significantly. Patches are categorized by feature area for easier debugging:

* `Replace/Patches`: Construction, reservations, and attachments.
* `NewThing/Patches`: Blueprint/Frame placement logic.
* `OverMineable/Patches`: Rock, fog, and bridge-like terrain.
* `DestroyedRestore/Patches`: State capture for auto-rebuilds.

---

## 📜 Credits

This project stands on the shoulders of the community giants who built the original foundation.

* **Original Creator:** Uuugggg / Alex Tearse-Doyle.
* **Maintainers & Contributors:** MemeGoddess, Hexnet111, and the many community members who submitted fixes and translations.
* **Fork & Refactor:** Kyle Givler (Performance Edition).

### Source Code

The code for this project is open source. If you encounter bugs or want to contribute to the performance or compatibility efforts, please visit the repository:

**[GitHub: RimWorld-ReplaceStuff](https://github.com/JoyfulReaper/RimWorld-ReplaceStuff)**