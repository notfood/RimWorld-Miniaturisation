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
			var mod = (
				from m in LoadedModManager.RunningMods
				where m.Name == requiredMod
				select m
			).FirstOrDefault();

			if (mod == null) {
#if DEBUG
				Log.Message ("Miniaturisation :: Skipping " + requiredMod);
#endif
				return;
			}

			Log.Message ("Miniaturisation :: Found " + requiredMod + " (" + targetsDefNames.Count + ")");

			var things = (
				from m in LoadedModManager.RunningMods
				from def in m.AllDefs
				where targetsDefNames.Contains(def.defName) && def.GetType() == typeof(ThingDef)
				select def
			);

			foreach (Def thing in things) {
#if DEBUG
				Log.Message ("> " + thing.defName);
#endif
				DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef (thing, thingDef_minifiedDef, "MinifiedFurniture");

			}
		}

	}
}

