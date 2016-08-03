using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using RimWorld;
using Verse;
using CommunityCoreLibrary;

namespace Miniaturisation
{
	public class DefInjector : SpecialInjector
	{
		private static Color BlueprintColor = new Color (0.5f, 0.5f, 1f, 0.35f);

		public override bool Inject ()
		{
			try {
				InjectVarious ();
			} catch (Exception e) {
				Log.Error ("Miniaturisation :: Met error while injecting. \n" + e);
				return false;
			}

			Log.Message ("Miniaturisation :: Injected");

			return true;
		}

		private void InjectVarious()
		{
			// Get the generic for furniture
			ThingDef minifiedFurniture = DefDatabase< ThingDef >.GetNamed ("MinifiedFurniture");

			// traverse through our filters
			foreach (MiniaturisationDef miniaturisationDef in DefDatabase< MiniaturisationDef >.AllDefsListForReading) {

				// check if the mod exists or ignore
				if (Find_Extensions.ModByName (miniaturisationDef.requiredMod) == null) {
#if DEBUG
					Log.Message ("Miniaturisation :: Skipping " + miniaturisationDef.requiredMod);
#endif
					continue;
				}

				// This list contains the defNames of the mod
				List<string> filter = miniaturisationDef.targetsDefNames;
				Log.Message ("Miniaturisation :: Found " + miniaturisationDef.requiredMod + " (" + filter.Count + ")");

				IEnumerable<ThingDef> list = DefDatabase< ThingDef >.AllDefs.Where ( def => filter.Contains(def.defName) );
				for (int i = 0; i < list.Count(); i++) {
					ThingDef thing = list.ElementAt (i);
#if DEBUG
					Log.Message ("> " + thing.defName);
#endif
					// add the minifiedDef is it isn't there
					if (thing.minifiedDef == null) {
						thing.minifiedDef = minifiedFurniture;
						thing.minifiedDef.installBlueprintDef = NewBlueprintDef_Thing (thing);

						DefDatabase<ThingDef>.Add (thing.minifiedDef.installBlueprintDef);
					}
				}
			}
		}

		// copied as it is from ThingDefGenerator_Buildings.BaseBlueprintDef
		private static ThingDef BaseBlueprintDef ()
		{
			return new ThingDef {
				category = ThingCategory.Ethereal,
				label = "Unspecified blueprint",
				altitudeLayer = AltitudeLayer.Blueprint,
				useHitPoints = false,
				selectable = true,
				seeThroughFog = true,
				comps =  {
					new CompProperties_Forbiddable ()
				},
				drawerType = DrawerType.MapMeshAndRealTime
			};
		}

		// modified from ThingDefGenerator_Buildings.NewBlueprintDef_Thing to handle loading images out of main thread
		private static ThingDef NewBlueprintDef_Thing (ThingDef def)
		{
			ThingDef thingDef = BaseBlueprintDef ();
			thingDef.defName = def.defName + ThingDefGenerator_Buildings.BlueprintDefNameSuffix;
			thingDef.label = def.label + "BlueprintLabelExtra".Translate ();
			thingDef.size = def.size;
			thingDef.defName += ThingDefGenerator_Buildings.InstallBlueprintDefNameSuffix;

			thingDef.graphicData = new GraphicData ();
			thingDef.graphicData.CopyFrom (def.graphicData);
			thingDef.graphicData.shaderType = ShaderType.Transparent;
			thingDef.graphicData.color = BlueprintColor;
			thingDef.graphicData.colorTwo = Color.white;
			thingDef.graphicData.shadowData = null;

			LongEventHandler.ExecuteWhenFinished (delegate {
				thingDef.graphic = thingDef.graphicData.Graphic;
			});

			if (thingDef.graphicData.shadowData != null) {
				Log.Error ("Blueprint has shadow: " + def);
			}
			thingDef.thingClass = typeof(Blueprint_Install);

			if (def.thingClass == typeof(Building_Door)) {
				thingDef.drawerType = DrawerType.RealtimeOnly;
			}
			else {
				thingDef.drawerType = DrawerType.MapMeshAndRealTime;
			}
			thingDef.entityDefToBuild = def;

			def.installBlueprintDef = thingDef;

			ShortHashGiver.GiveShortHash (thingDef);

			return thingDef;
		}
	}
}

