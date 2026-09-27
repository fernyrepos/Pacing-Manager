using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ProgressionPacing
{
    public static class RealRuinsCompat
    {
        public const string PackageId = "Woolstrand.RealRuins";

        private static FieldInfo costCapField;
        private static bool insideStandardScatter;

        public static bool Active => ModsConfig.IsActive(PackageId);

        public static void ApplyPatch(Harmony harmony)
        {
            if (!Active) return;

            Type genStepType = AccessTools.TypeByName("RealRuins.GenStep_ScatterRealRuins");
            Type scattererType = AccessTools.TypeByName("RealRuins.RuinsScatterer");
            Type optionsType = AccessTools.TypeByName("RealRuins.ScatterOptions");
            if (genStepType == null || scattererType == null || optionsType == null)
            {
                Log.Warning("[Pacing Manager] Real Ruins is active but its types could not be found; ruin wealth cap disabled.");
                return;
            }

            MethodInfo generate = AccessTools.DeclaredMethod(genStepType, "Generate");
            MethodInfo scatter = AccessTools.DeclaredMethod(scattererType, "Scatter");
            costCapField = AccessTools.Field(optionsType, "costCap");
            if (generate == null || scatter == null || costCapField == null)
            {
                Log.Warning("[Pacing Manager] Real Ruins is active but its scatter methods could not be found; ruin wealth cap disabled.");
                return;
            }

            harmony.Patch(generate,
                prefix: new HarmonyMethod(typeof(RealRuinsCompat), nameof(GeneratePrefix)),
                finalizer: new HarmonyMethod(typeof(RealRuinsCompat), nameof(GenerateFinalizer)));
            harmony.Patch(scatter, prefix: new HarmonyMethod(typeof(RealRuinsCompat), nameof(ScatterPrefix)));
        }

        public static void GeneratePrefix()
        {
            insideStandardScatter = true;
        }

        public static Exception GenerateFinalizer(Exception __exception)
        {
            insideStandardScatter = false;
            return __exception;
        }

        public static void ScatterPrefix(object[] __args)
        {
            if (!insideStandardScatter) return;

            IntRange range = ProgressionPacingModSettings.ruinsWealthCapRange;
            if (range.max <= 0) return;

            object options = __args[1];
            if (options == null) return;

            int cap = Mathf.Max(1, Rand.RangeInclusive(Mathf.Max(0, range.min), range.max));
            costCapField.SetValue(options, cap);
        }
    }
}
