using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;
using RimWorld;

namespace Miniaturisation
{
	public class MiniaturisationDef : Def
	{
		private static Color BlueprintColor = new Color (0.5f, 0.5f, 1f, 0.35f);

		#region XML Data
		public string requiredMod;
		public List<string> targetsDefNames = new List<string>();
		#endregion

		public override void ResolveReferences ()
		{
			base.ResolveReferences ();

			if (!IsModActive(requiredMod)) {
#if DEBUG
				Log.Message ("Miniaturisation :: Skipping " + requiredMod);
#endif
				return;
			}

			List<string> filter = targetsDefNames;
			Log.Message ("Miniaturisation :: Found " + requiredMod + " (" + filter.Count + ")");

			ThingDef minifiedFurniture = DefDatabase< ThingDef >.GetNamed ("MinifiedFurniture");

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

		private static bool IsModActive( string name )
		{
			IEnumerable<ModMetaData> activeMods = ModsConfig.ActiveModsInLoadOrder;
			foreach (ModMetaData mod in activeMods) {
				if (mod.Name == name) {
					return true;
				}
			}

			return false;
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

