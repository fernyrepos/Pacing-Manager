using HarmonyLib;
using RimWorld;

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
            }
        }
    }
}
