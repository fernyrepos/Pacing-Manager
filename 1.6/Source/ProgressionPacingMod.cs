using HarmonyLib;
using Verse;
using UnityEngine;
using RimWorld;
using System;
using System.Linq;

namespace ProgressionPacing
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
    [HotSwappable]
    public class ProgressionPacingMod : Mod
    {
        public ProgressionPacingMod(ModContentPack pack) : base(pack)
        {
            GetSettings<ProgressionPacingModSettings>();
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
            Text.Font = GameFont.Tiny;
            foreach (var techLevel in Enum.GetValues(typeof(TechLevel)).Cast<TechLevel>())
            {
                if (techLevel != TechLevel.Undefined)
                {
                    string label = techLevel.ToString() + ": " + ProgressionPacingModSettings.techLevelMultipliers[techLevel].ToStringPercent();
                    ProgressionPacingModSettings.techLevelMultipliers[techLevel] = listing.SliderLabeled(label, ProgressionPacingModSettings.techLevelMultipliers[techLevel], 0.01f, 10f, labelPct: 0.10f);
                }
            }
            Text.Font = GameFont.Small;
            
            listing.Gap();
            Text.Anchor = TextAnchor.MiddleCenter;
            listing.Label("PP_RoundingMultiple".Translate());
            Text.Anchor = TextAnchor.UpperLeft;

            listing.curX -= 200;
            foreach (var techLevel in Enum.GetValues(typeof(TechLevel)).Cast<TechLevel>())
            {
                if (techLevel != TechLevel.Undefined)
                {
                    string label = techLevel.ToString() + ": ";
                    int roundingValue = ProgressionPacingModSettings.techLevelRoundingMultiples[techLevel];
                    string buffer = roundingValue.ToString();
                    Rect rect = listing.GetRect(Text.LineHeight);
                    if (!listing.BoundingRectCached.HasValue || rect.Overlaps(listing.BoundingRectCached.Value))
                    {
                        Rect rect2 = rect.LeftHalf().Rounded();
                        Rect rect3 = rect.RightHalf().Rounded();
                        rect3.height -= 6f;
                        rect3.y += 3f;
                        TextAnchor anchor = Text.Anchor;
                        Text.Anchor = TextAnchor.MiddleRight;
                        Widgets.Label(rect2, label);
                        Text.Anchor = anchor;
                        Widgets.TextFieldNumeric(rect3, ref roundingValue, ref buffer, 1, 10000);
                    }
                    listing.Gap(listing.verticalSpacing);
                    ProgressionPacingModSettings.techLevelRoundingMultiples[techLevel] = roundingValue;
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
