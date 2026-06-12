# Replace Stuff: Performance Edition
A performance-focused continuation and modernization of Replace Stuff for RimWorld.
Upgrade buildings in-place without tearing apart your colony.
> **⚠️ Project Status:** This project is currently in its early stages of development. While core systems are being actively refactored, please consider this pre-alpha/early work. Be sure to back up critical saves and report any bugs or edge cases on GitHub.
> 
# The Vanilla Problem
Upgrading your base in vanilla RimWorld can be frustrating.
Want to replace a wooden wall with granite? Your colonists first tear down the old wall, leaving gaps in your defenses, exposing rooms to the outdoors, and potentially causing temperature problems or roof collapses.
Replacing utility buildings can be just as tedious. Storage shelves lose their settings, coolers need to be reconfigured, and many buildings require manual setup after construction.
# What Replace Stuff: Performance Edition Does
## In-Place Upgrades
Buildings are replaced using a dedicated replacement frame. The original structure remains until the replacement is ready, minimizing many of the problems caused by traditional deconstruction and rebuilding.
## Preserves Building State
Whenever possible, important building settings are transferred to the replacement.
Current and planned state preservation includes:
 * Storage priorities and filters.
 * Cooler temperature settings.
 * Building orientation and ownership.
 * Other compatible building data as development continues.
## Broad Mod Compatibility
Replacement frames are generated dynamically, allowing many buildings from other mods to participate without requiring dedicated compatibility patches.
The goal is to support as many artificial buildings as possible while remaining lightweight, reliable, and maintainable.
# Performance Edition
This project is an ongoing effort in its early stages to modernize and refactor the original Replace Stuff codebase.
Current areas of focus include:
 * Cleaner architecture.
 * Better performance.
 * Improved maintainability.
 * More reliable state preservation.
 * Easier compatibility with other mods.
# Long-Term Vision
Replace Stuff: Performance Edition is being designed around a generic building state transfer system.
* Port to use my Divine Intervention framework.
* Rather than requiring custom support for every building type, the mod will automatically preserve compatible building and component data whenever possible while providing extension points for more complex behaviors.
* The objective is to make replacing both vanilla and modded buildings as seamless as possible while minimizing the need for dedicated compatibility patches.
* Another possibility is having a framework library (DevineIntervention, in planning) provide a message bus to allow other mods to publish their replacement lists to the bus.
* Development is ongoing, and support for additional building behaviors and component types will continue to expand over time.
# 🙌 Acknowledgments & Thanks
As the person forking and refactoring this project, I owe a massive debt of gratitude to the developers who built and maintained the foundation before me. This performance edition is built on the shoulders of giants:
 * **Uuugggg (Alex Tearse-Doyle):** The original creator whose genius gave the RimWorld community one of its absolute best quality-of-life mods.
 * **MemeGoddess & Hexnet111:** For stepping up to maintain the codebase for modern versions and identifying the critical performance bottlenecks that inspired this entire rewrite.
 * **The Git Contributors:** A huge thank you to everyone in the upstream repositories who has submitted patches, tracked down bugs, or contributed code over the last eight years.
