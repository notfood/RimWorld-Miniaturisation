using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using UnityEngine;
using Verse;
using RimWorld;

namespace Miniaturisation
{
	public class MiniaturisationDef : Def
	{
		private static FieldInfo thingDef_minifiedDef = typeof(ThingDef).GetField("minifiedDef");

		#region XML Data
		public string requiredMod;
		public List<string> targetsDefNames = new List<string>();
		#endregion

		public override void PostLoad ()
		{
			ModContentPack mod = null;
			foreach (ModContentPack current in LoadedModManager.RunningMods) {
				if (current.Name == requiredMod) {
					Log.Message ("Miniaturisation :: Found " + requiredMod + " (" + targetsDefNames.Count + ")");
					mod = current;
					break;
				}
			}

			if (mod == null) {
#if DEBUG
				Log.Message ("Miniaturisation :: Skipping " + requiredMod);
#endif
				return;
			}

			foreach( Def thing in mod.AllDefs.Where ( def => targetsDefNames.Contains(def.defName) && def.GetType() == typeof(ThingDef) ) ) {
#if DEBUG
				Log.Message ("> " + thing.defName);
#endif
				CrossRefLoader.RegisterObjectWantsCrossRef (thing, thingDef_minifiedDef, "MinifiedFurniture");
			}

			// don't waste hashes
			//base.PostLoad ();
		}

	}
}

