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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref techLevelMultipliers, "techLevelMultipliers", LookMode.Value, LookMode.Value);

            if (techLevelMultipliers == null)
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
        }

        public static void UpdateResearchProjectCosts()
        {
            foreach (var def in DefDatabase<ResearchProjectDef>.AllDefs)
            {
                float multiplier = GetMultiplierForTechLevel(def.techLevel);
                def.baseCost = Mathf.RoundToInt(def.baseCost * multiplier);
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
