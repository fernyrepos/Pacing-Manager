using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace ProgressionPacing
{
    [HarmonyPatch(typeof(CompPowerTrader), nameof(CompPowerTrader.PowerOutput), MethodType.Setter)]
    public static class CompPowerTrader_PowerOutput_Patch
    {
        public static void Prefix(ref float value)
        {
            if (value > 0f && ProgressionPacingModSettings.powerOutputMultiplier != 1f)
            {
                value *= ProgressionPacingModSettings.powerOutputMultiplier;
                if (ProgressionPacingModSettings.powerOutputRoundingMultiple > 1)
                {
                    value = Mathf.RoundToInt(value / ProgressionPacingModSettings.powerOutputRoundingMultiple) * ProgressionPacingModSettings.powerOutputRoundingMultiple;
                }
                else
                {
                    value = Mathf.RoundToInt(value);
                }
                if (value < ProgressionPacingModSettings.powerOutputRoundingMultiple)
                {
                    value = ProgressionPacingModSettings.powerOutputRoundingMultiple;
                }
            }
        }
    }
}
