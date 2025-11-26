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
            Rect lineRect = listing.GetRect(30f);
            Rect labelRect = lineRect;
            labelRect.width -= 104f;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, "PP_ResearchCostMultipliersByTechLevel".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Rect buttonRect = new Rect(lineRect.xMax - 100f, lineRect.y, 100f, lineRect.height);
            if (Widgets.ButtonText(buttonRect, "Reset".Translate()))
            {
                ProgressionPacingModSettings.ResetTechLevelMultipliers();
            }
            listing.Gap(listing.verticalSpacing);
            foreach (var techLevel in Enum.GetValues(typeof(TechLevel)).Cast<TechLevel>())
            {
                if (techLevel != TechLevel.Undefined)
                {
                    string label = techLevel.ToString() + ": " + ProgressionPacingModSettings.techLevelMultipliers[techLevel].ToStringPercent();
                    ProgressionPacingModSettings.techLevelMultipliers[techLevel] = listing.SliderLabeled(label, ProgressionPacingModSettings.techLevelMultipliers[techLevel], 0.01f, 10f, labelPct: 0.15f);
                }
            }

            listing.Gap();
            string buffer = ProgressionPacingModSettings.roundingMultiple.ToString();
            listing.curX -= 200;
            listing.TextFieldNumericLabeled("PP_RoundingMultiple".Translate() + " ", ref ProgressionPacingModSettings.roundingMultiple, ref buffer, 1, 10000);

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
