using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace Miniaturisation_Overloaded
{
	[StaticConstructorOnStartup]
	public static class MiniaturisationConstructor
	{
		static MiniaturisationConstructor()
		{
			//LongEventHandler is called after all defs are loaded, which is handy.
			LongEventHandler.QueueLongEvent(InjectDefs, "MiniaturisationInjector", false, null);
		}

		//I used this list to get a bunch of general types I didn't want to touch, hopefully the reflection is done correctly.
		private static List<Type> unwantedBuildingTypes = new List<Type>()
		{
			typeof(Building_AncientCryptosleepCasket),
			typeof(Building_Battery),
			Assembly.GetAssembly(typeof(Building_Battery)).GetType("RimWorld.Building_CrashedShipPart"),
			typeof(Building_CryptosleepCasket),
			typeof(Building_Door),
			typeof(Building_Grave),
			typeof(Building_MarriageSpot),
			typeof(Building_PlantGrower),
			Assembly.GetAssembly(typeof(Building_Battery)).GetType("RimWorld.Building_PoisonShipPart"),
			Assembly.GetAssembly(typeof(Building_Battery)).GetType("RimWorld.Building_PsychicEmanator"),
			typeof(Building_SteamGeyser),
			typeof(Building_Vent),
			typeof(Mineable)
		};

		private static void InjectDefs()
		{
			/* Generates a list of buildings from the def database (DefDatabase is generated upon loading the game including mods).
			The parameters I placed are from trial and error, just a proof-of-concept. You can mess with them of course.
			This method requires much more effort on the coder's part, but once the parameters are tweaked it's very autonomous.
			Since this will run on modded ThingDefs as well, I think it's ideal as it requires 0 modder/user intervention.
			Another thing I thought of but didn't implement is having a Def which modders can use to determine which defNames NOT to touch.
			Maybe with an ExceptionDef like that you can reduce the amount of parameters in this list, but it's less autonomous. */

			var buildings = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(
				def => (def.thingClass.Name.Contains("Building") || def.category == ThingCategory.Building)
				&& def.designationCategory != "Structure"
				&& !def.Minifiable
				&& !unwantedBuildingTypes.Contains(def.thingClass)
				&& !def.IsFrame && !def.IsBlueprint && !def.IsDoor && !def.IsFilth
				&& def.building.claimable
				&& (def.costList != null || def.costStuffCount > 0)
				&& def.graphicData.linkType == LinkDrawerType.None
				&& !def.defName.Contains("Campfire") && !def.defName.Contains("Vent") && !def.defName.Contains("Duct"));

#if DEBUG
			Log.Message("Miniaturisation :: Found " + buildings.Count + " buildings that have no minifiedDef");
#endif

			//Count the number of injected ThingDefs
			var injectedDefs = 0;

			foreach (var thing in buildings)
			{
				thing.minifiedDef = MinifiedDefOf.MinifiedFurniture;

				//Increase modified ThingDefs total count
				injectedDefs++;

#if DEBUG
				Log.Message("Miniaturisation :: Defined MinifiedDef for " + thing.label + " (" + thing.defName + ")");
#endif
			}

			//Report the total number of ThingDefs changed
			Log.Message("Miniaturisation :: Defined MinifiedDef for " + injectedDefs + " items");
		}
	}
}