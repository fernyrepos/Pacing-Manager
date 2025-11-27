using UnityEngine;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ProgressionPacing
{
    public class ProgressionPacingModSettings : ModSettings
    {
        public static Dictionary<TechLevel, float> techLevelMultipliers = new Dictionary<TechLevel, float>
        {
            { TechLevel.Animal, 1f },
            { TechLevel.Neolithic, 1f },
            { TechLevel.Medieval, 1f },
            { TechLevel.Industrial, 1f },
            { TechLevel.Spacer, 1f },
            { TechLevel.Ultra, 1f },
            { TechLevel.Archotech, 1f }
        };
        public static int roundingMultiple = 1;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref techLevelMultipliers, "techLevelMultipliers", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref roundingMultiple, "roundingMultiple", 1);
        }

        public static void ResetTechLevelMultipliers()
        {
            techLevelMultipliers = new Dictionary<TechLevel, float>
                {
                    { TechLevel.Animal, 1f },
                    { TechLevel.Neolithic, 1f },
                    { TechLevel.Medieval, 1f },
                    { TechLevel.Industrial, 1f },
                    { TechLevel.Spacer, 1f },
                    { TechLevel.Ultra, 1f },
                    { TechLevel.Archotech, 1f }
                };
        }
        
        

        public static void UpdateResearchProjectCosts()
        {
            if (techLevelMultipliers == null)
            {
                ResetTechLevelMultipliers();
            }
            foreach (var def in DefDatabase<ResearchProjectDef>.AllDefs)
            {
                float multiplier = GetMultiplierForTechLevel(def.techLevel);
                float newCost = def.baseCost * multiplier;
                if (roundingMultiple > 1)
                {
                    def.baseCost = Mathf.RoundToInt(newCost / roundingMultiple) * roundingMultiple;
                }
                else
                {
                    def.baseCost = Mathf.RoundToInt(newCost);
                }
                if (def.baseCost < roundingMultiple)
                {
                    def.baseCost = roundingMultiple;
                }
            }
        }

        public static float GetMultiplierForTechLevel(TechLevel techLevel)
        {
            if (techLevelMultipliers.TryGetValue(techLevel, out float multiplier))
            {
                return multiplier;
            }
            return 1f;
        }
    }
}
