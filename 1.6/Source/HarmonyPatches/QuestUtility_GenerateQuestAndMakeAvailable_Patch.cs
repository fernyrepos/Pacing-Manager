using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;

namespace ProgressionPacing
{
    [HarmonyPatch(typeof(QuestUtility), nameof(QuestUtility.GenerateQuestAndMakeAvailable), typeof(QuestScriptDef), typeof(Slate))]
    public static class QuestUtility_GenerateQuestAndMakeAvailable_EraGate_Patch
    {
        public static bool Prefix(QuestScriptDef root, ref Quest __result)
        {
            if (root == null) return true;
            if (ProgressionPacingModSettings.IsQuestGeneratorAllowed(root.defName)) return true;
            __result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(QuestUtility), nameof(QuestUtility.SendLetterQuestAvailable))]
    public static class QuestUtility_SendLetterQuestAvailable_NullGuard_Patch
    {
        public static bool Prefix(Quest quest)
        {
            return quest != null;
        }
    }
}
