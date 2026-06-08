# Replace Stuff Perfomance Edition for RimWorld
RimWorld mod - Replace the stuff a thing is made from.

#The Problem with Vanilla:
In standard RimWorld, upgrading your base is a destructive, dangerous hassle. Want to change a wooden wall to stone? Your pawns tear down the wood, leaving a gaping hole in your defenses for raiders to walk through, venting all the cold air out of your freezer, and dropping the roof on your colonist's head. On top of that, if you rebuild a storage shelf or a cooler, you have to manually set up all your storage filters and target temperatures all over again.

How "Replace Stuff: Performance Edition" Fixes It:
This mod lets you upgrade buildings in-place.

No Base Breaches: Your pawns build a "frame" over the existing structure. The old wall isn't destroyed until the exact moment the new wall is finished. No holes, no temperature leaks, no collapsed roofs.

It Remembers Your Settings: The mod captures the "soul" of the building. When you swap a tailored wooden shelf for a steel one, your custom storage filters and priorities are instantly transferred to the new shelf. Coolers remember their target temperatures.

Automatic Mod Compatibility: Because of how the code dynamically generates replacement frames behind the scenes, you can upgrade almost any artificial building from almost any other mod without waiting for compatibility patches.

I am in the process of completly reviewing, re-writing from performance and refactoring the source code for this mod.



# Planned Vision:
Ultimate, Future-Proof Mod Compatibility:
Normally, if you download a massive new mod that adds complex nuclear reactors or custom alien incubators, replacement mods won't know how to copy their custom settings over when you upgrade them.

With our new Dynamic State Transfer engine, the mod physically looks inside the components of the old building, finds the exact matching components in the new building, and directly injects the settings across. It doesn't matter if the mod came out today or three years from now—if it has settings, "Replace Stuff" will seamlessly copy them to the upgraded version without a single compatibility patch required.