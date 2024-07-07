<p align="center">
	“Portions of the materials used to create this content/mod are trademarks and/or copyrighted works of Ludeon Studios Inc. All rights reserved by Ludeon. This content/mod is not official and is not endorsed by Ludeon.”
</p>

# Description
A framework using Harmony and Humanoid Alien Races (HAR) to allow for inorganic beings ranging from elementals and ethereals to constructs and robots. This frameworks is designed only for Humanlike or Animal intelligence pawns, and has neither functionality nor interaction with mechanoids.

## Features
This mod itself has the bare minimum code necessary to have artificial beings. It includes no content and exists solely for other mods to leverage to make their own.

## For Modders
Once my mods are stable for version 1.5, I will work on documenting how to use ABF to make your own custom artificial beings. Nearly all functionality is handled through DefModExtensions, StatDefs, or reuse of vanilla mechanics. [Mechcloud Chemwalkers](https://github.com/RWDevathon/Mechcloud-Chemwalkers) is a relatively simple race mod that exemplifies how to use the framework while including some custom features of its own that may help paint a picture of what can be done with the framework.

I am accessible on Discord either on the ABF discord (linked below) or in the Mod development channel of the RimWorld discord. Don't be afraid to reach out with questions, concerns, or ideas on how to improve the framework - there is a good chance I haven't thought of it before and am happy to receive feedback.

## Known Issues / Incompatibilities
* Activating Self-Tend on an artificial pawn will throw a message at you complaining that the Doctor work type is not assigned even if the pawn has the Artificer work type assigned (It's harmless, artificial pawns will self-tend based on Artificer, not Doctor).

## Links
[Discord](https://discord.gg/udNCpbkABT)

[GitHub](https://github.com/RWDevathon/Artificial-Beings-Framework)