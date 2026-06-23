# [RSPE-CORE: SYSTEMS ARCHITECTURE MANUAL]

**PROTOCOL ID:** RSPE-REPLACE-01  
**CODENAME:** Replace Stuff: Performance Edition  
**SYSTEM STATUS:** PRE-ALPHA / ACTIVE REFACTOR  
**MAINTAINER:** K. GIVLER (ADMIN)  

---

## 1.0 SYSTEM OVERVIEW

**Replace Stuff: Performance Edition** is a performance-focused continuation, modernization, and refactor of the original *Replace Stuff* modification for RimWorld.

In the vanilla execution environment, upgrading structural components (e.g., replacing a wooden wall with granite) requires complete deconstruction prior to reconstruction. This legacy process introduces systemic vulnerabilities, including defensive breaches, structural exposure to ambient outdoor temperatures, and potential roof collapses. Replacing utility infrastructure also causes data loss, wiping out storage settings, cooler configurations, and manual building setups.

This utility corrects these inefficiencies by allowing structures and utility buildings to be upgraded in-place without disrupting colony operations.

## 2.0 FUNCTIONAL CAPABILITIES

The framework executes three primary automation protocols to handle structural replacement:

* **In-Place Upgrades:** Structures are targeted via a dedicated replacement frame. The original asset remains completely operational and intact until the replacement construction sequence concludes, eliminating structural gaps.
* **State Preservation:** The system intercepts the replacement sequence to transfer critical operational states from the legacy asset to the newly constructed asset.
* **Dynamic Mod Compatibility:** Replacement frames are generated dynamically at runtime. This allows assets from third-party modifications to utilize the replacement pipeline automatically without requiring hardcoded configuration patches.

## 3.0 STATE TRANSFER MATRIX

The underlying architecture relies on a generic state transfer system designed to automatically handle component data. Current and planned state serialization includes:

| Data Category | Target Metrics | Status |
| --- | --- | --- |
| **Logistics** | Storage priorities and item filter configurations | DEBUGGING |
| **Thermal Control** | Target cooler temperature settings | DEBUGGING |
| **Structural / Identity** | Building orientation, rotation, and pawn ownership assignments | DEBUGGING |
| **Extended Assets** | Complex third-party component data modules | DEVELOPMENT PHASE |

## 4.0 DEVELOPMENT FOCUS & ROADMAP

> **⚠️ SYSTEM NOTICE:** This optimization suite is currently in its pre-alpha development phase. Core systems are undergoing active refactoring. Users should back up critical simulation saves and report runtime anomalies via the GitHub issue tracker.

Current optimization vectors are concentrated on the following subsystems:

* Implementing a cleaner, decoupled code architecture.
* Minimizing CPU cycles and optimizing performance bottlenecks.
* Establishing a highly maintainable, lightweight core codebase.
* Increasing reliability of data state transfers.
* Streamlining compatibility entry points for external mods.

## 5.0 HISTORICAL DATA & ACKNOWLEDGMENTS

This modernization suite is built upon foundational assets developed by the upstream open-source community:

* **Uuugggg (Alex Tearse-Doyle):** Original architect who established the core concept and execution framework.
* **MemeGoddess & Hexnet111:** Upstream maintainers who preserved codebase compatibility for modern environments and identified core performance bottlenecks.
* **The Git Contributors:** Open-source contributors who provided patches, bug tracking, and code maintenance over an eight-year operational cycle.

## 6.0 LEGAL & COMPLIANCE

**COPYRIGHT NOTICE:** © 2026 Kyle Givler.

**DISTRIBUTION:** This software adaptation is provided under open-source compliance standards. The maintainer assumes no responsibility for broken save states or structural failures within unbacked simulation environments. Use at your own risk.
