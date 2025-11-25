using HarmonyLib;
using Verse;
using UnityEngine;
using RimWorld;
using System;
using System.Linq;

namespace ProgressionPacing
{
    public class ProgressionPacingMod : Mod
    {
        public ProgressionPacingMod(ModContentPack pack) : base(pack)
        {
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.Label("PP_ResearchCostMultipliersByTechLevel".Translate());
            foreach (var techLevel in Enum.GetValues(typeof(TechLevel)).Cast<TechLevel>())
            {
                if (techLevel != TechLevel.Undefined)
                {
                    string label = techLevel.ToString() + ": " + ProgressionPacingModSettings.techLevelMultipliers[techLevel].ToStringPercent();
                    ProgressionPacingModSettings.techLevelMultipliers[techLevel] = listing.SliderLabeled(label, ProgressionPacingModSettings.techLevelMultipliers[techLevel], 0.01f, 10f);
                }
            }
            listing.End();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            ProgressionPacingModSettings.UpdateResearchProjectCosts();
        }


        public override string SettingsCategory()
        {
            return Content.Name;
        }
    }
}
