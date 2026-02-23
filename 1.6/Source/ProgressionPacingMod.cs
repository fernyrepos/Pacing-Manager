using HarmonyLib;
using Verse;
using UnityEngine;
using RimWorld;
using System;
using System.Linq;
using System.Collections.Generic;

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
            new Harmony("ProgressionPacingMod").PatchAll();
        }

        private static float scrollHeight = 999999f;
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Rect viewRect = new Rect(0, 0, inRect.width - 16f, scrollHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);
            
            var listing = new Listing_Standard();
            listing.Begin(viewRect);
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
            listing.curX += 200;
            if (ModsConfig.IsActive("vanillaexpanded.gravship"))
            {
                listing.CheckboxLabeled("PP_ExcludeGravdata".Translate(), ref ProgressionPacingModSettings.excludeGravdata);
            }
            listing.Gap();
            string powerOutputLabel = "PP_PowerOutputMultiplier".Translate() + ": " + ProgressionPacingModSettings.powerOutputMultiplier.ToStringPercent();
            ProgressionPacingModSettings.powerOutputMultiplier = listing.SliderLabeled(powerOutputLabel, ProgressionPacingModSettings.powerOutputMultiplier, 0.01f, 10f, labelPct: 0.30f);
            listing.Gap();
            Text.Anchor = TextAnchor.MiddleCenter;
            listing.Label("PP_PowerOutputRoundingMultiple".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            listing.curX -= 200;
            string powerOutputRoundingLabel = "";
            int powerOutputRoundingValue = ProgressionPacingModSettings.powerOutputRoundingMultiple;
            string powerOutputRoundingBuffer = powerOutputRoundingValue.ToString();
            Rect powerOutputRoundingRect = listing.GetRect(Text.LineHeight);
            if (!listing.BoundingRectCached.HasValue || powerOutputRoundingRect.Overlaps(listing.BoundingRectCached.Value))
            {
                Rect powerOutputRoundingRect2 = powerOutputRoundingRect.LeftHalf().Rounded();
                Rect powerOutputRoundingRect3 = powerOutputRoundingRect.RightHalf().Rounded();
                powerOutputRoundingRect3.height -= 6f;
                powerOutputRoundingRect3.y += 3f;
                TextAnchor powerOutputRoundingAnchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(powerOutputRoundingRect2, powerOutputRoundingLabel);
                Text.Anchor = powerOutputRoundingAnchor;
                Widgets.TextFieldNumeric(powerOutputRoundingRect3, ref powerOutputRoundingValue, ref powerOutputRoundingBuffer, 1, 10000);
            }
            listing.Gap(listing.verticalSpacing);
            ProgressionPacingModSettings.powerOutputRoundingMultiple = powerOutputRoundingValue;
            listing.curX += 200;
            scrollHeight = listing.CurHeight + 20f;
            listing.End();
            
            Widgets.EndScrollView();
        }
        
        private Vector2 scrollPosition = Vector2.zero;

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

    public class ProgressionPacingGameComponent : GameComponent
    {
        public Dictionary<TechLevel, float> savedMultipliers = new Dictionary<TechLevel, float>();
        public Dictionary<TechLevel, int> savedRoundingMultiples = new Dictionary<TechLevel, int>();

        public ProgressionPacingGameComponent(Game game)
        {
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref savedMultipliers, "savedMultipliers", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref savedRoundingMultiples, "savedRoundingMultiples", LookMode.Value, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (savedMultipliers == null) savedMultipliers = new Dictionary<TechLevel, float>();
                if (savedRoundingMultiples == null) savedRoundingMultiples = new Dictionary<TechLevel, int>();
                
                FixResearchProgress();
                UpdateSavedMultipliers();
            }
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            UpdateSavedMultipliers();
        }

        public void UpdateSavedMultipliers()
        {
            savedMultipliers.Clear();
            savedRoundingMultiples.Clear();
            foreach (TechLevel level in Enum.GetValues(typeof(TechLevel)))
            {
                if (level != TechLevel.Undefined)
                {
                    savedMultipliers[level] = ProgressionPacingModSettings.GetMultiplierForTechLevel(level);
                    savedRoundingMultiples[level] = ProgressionPacingModSettings.GetRoundingMultipleForTechLevel(level);
                }
            }
        }

        private void FixResearchProgress()
        {
            if (Find.ResearchManager == null) return;
            var progressDict = Find.ResearchManager.progress;
            if (progressDict == null) return;

            bool wasEmpty = savedMultipliers.Count == 0;

            foreach (var def in progressDict.Keys.ToList())
            {
                if (def.knowledgeCost > 0) continue;
                if (ProgressionPacingModSettings.excludeGravdata && ModsConfig.IsActive("vanillaexpanded.gravship") && def.tab?.defName == "VGE_Gravtech") continue;

                if (progressDict.TryGetValue(def, out float currentProgress) && currentProgress > 0)
                {
                    float oldMultiplier = wasEmpty ? 1f : (savedMultipliers.TryGetValue(def.techLevel, out float m) ? m : 1f);
                    int oldRounding = wasEmpty ? 1 : (savedRoundingMultiples.TryGetValue(def.techLevel, out int r) ? r : 1);

                    float originalVanillaCost = ProgressionPacingModSettings.originalResearchCosts != null && ProgressionPacingModSettings.originalResearchCosts.TryGetValue(def, out float orig) ? orig : def.baseCost;

                    float savedCost = originalVanillaCost * oldMultiplier;
                    if (oldRounding > 1)
                    {
                        savedCost = Mathf.RoundToInt(savedCost / oldRounding) * oldRounding;
                    }
                    else
                    {
                        savedCost = Mathf.RoundToInt(savedCost);
                    }
                    if (savedCost < oldRounding) savedCost = oldRounding;

                    float currentCost = def.baseCost;

                    if (savedCost > 0 && currentCost > 0 && Math.Abs(savedCost - currentCost) > 0.1f)
                    {
                        float ratio = currentCost / savedCost;
                        float newProgress = currentProgress * ratio;

                        if (currentProgress >= savedCost - 0.01f)
                        {
                            newProgress = Mathf.Max(newProgress, currentCost);
                        }

                        progressDict[def] = newProgress;
                    }
                }
            }
        }
    }
}
