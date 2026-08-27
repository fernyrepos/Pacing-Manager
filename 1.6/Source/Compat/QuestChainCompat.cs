using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ProgressionPacing
{
    public static class QuestChainCompat
    {
        public const string PackageId = "OskarPotocki.VanillaFactionsExpanded.Core";
        public const string KeyPrefix = "VEFChain:";

        private static Type chainDefType;
        private static Type gameComponentType;
        private static Type chainExtType;
        private static Type questUtilsType;
        private static PropertyInfo allChainDefsProperty;
        private static FieldInfo questChainDefField;
        private static FieldInfo conditionSucceedQuestsField;
        private static FieldInfo conditionFailQuestsField;
        private static FieldInfo conditionEitherField;
        private static FieldInfo conditionSucceedQuestsCountField;
        private static FieldInfo instanceField;
        private static MethodInfo tryScheduleQuestsMethod;
        private static MethodInfo scheduleQuestInTicksMethod;
        private static Harmony harmonyInstance;
        private static bool patchApplied;

        public static bool Active => ModsConfig.IsActive(PackageId) && ResolveTypes();

        private static bool ResolveTypes()
        {
            if (chainDefType != null) return true;

            Type resolvedChainDefType = AccessTools.TypeByName("VEF.Storyteller.QuestChainDef");
            Type resolvedGameComponentType = AccessTools.TypeByName("VEF.Storyteller.GameComponent_QuestChains");
            Type resolvedChainExtType = AccessTools.TypeByName("VEF.Storyteller.QuestChainExtension");
            Type resolvedQuestUtilsType = AccessTools.TypeByName("VEF.Storyteller.QuestUtils");
            if (resolvedChainDefType == null || resolvedGameComponentType == null || resolvedChainExtType == null || resolvedQuestUtilsType == null)
            {
                return false;
            }

            Type dbType = typeof(DefDatabase<>).MakeGenericType(resolvedChainDefType);
            PropertyInfo resolvedAllDefsProperty = dbType.GetProperty("AllDefsListForReading", BindingFlags.Public | BindingFlags.Static);
            if (resolvedAllDefsProperty == null) return false;

            gameComponentType = resolvedGameComponentType;
            chainExtType = resolvedChainExtType;
            allChainDefsProperty = resolvedAllDefsProperty;
            questChainDefField = AccessTools.Field(chainExtType, "questChainDef");
            conditionSucceedQuestsField = AccessTools.Field(chainExtType, "conditionSucceedQuests");
            conditionFailQuestsField = AccessTools.Field(chainExtType, "conditionFailQuests");
            conditionEitherField = AccessTools.Field(chainExtType, "conditionEither");
            conditionSucceedQuestsCountField = AccessTools.Field(chainExtType, "conditionSucceedQuestsCount");
            instanceField = AccessTools.Field(gameComponentType, "Instance");
            tryScheduleQuestsMethod = AccessTools.Method(gameComponentType, "TryScheduleQuests", Type.EmptyTypes);
            scheduleQuestInTicksMethod = AccessTools.Method(gameComponentType, "ScheduleQuestInTicks", new[] { typeof(QuestScriptDef), typeof(int) });
            questUtilsType = resolvedQuestUtilsType;
            chainDefType = resolvedChainDefType;
            return true;
        }

        public static void ApplyPatch(Harmony harmony)
        {
            harmonyInstance = harmony;
            TryApplyPatch();
        }

        private static void TryApplyPatch()
        {
            if (patchApplied || harmonyInstance == null || !Active) return;
            MethodInfo scheduleMethod = AccessTools.Method(gameComponentType, "TryScheduleQuest", new[] { typeof(QuestScriptDef) });
            MethodInfo createQuestMethod = AccessTools.Method(questUtilsType, "CreateQuest", new[] { typeof(QuestScriptDef) });
            if (scheduleMethod == null || createQuestMethod == null) return;
            harmonyInstance.Patch(scheduleMethod, prefix: new HarmonyMethod(typeof(QuestChainCompat), nameof(TryScheduleQuestPrefix)));
            harmonyInstance.Patch(createQuestMethod, prefix: new HarmonyMethod(typeof(QuestChainCompat), nameof(CreateQuestPrefix)));
            patchApplied = true;
        }

        private static List<Def> AllChainDefs()
        {
            var result = new List<Def>();
            if (!Active) return result;
            if (allChainDefsProperty.GetValue(null) is IEnumerable list)
            {
                foreach (object item in list)
                {
                    if (item is Def def) result.Add(def);
                }
            }
            return result;
        }

        public static IEnumerable<(string key, string label)> ChainEntries()
        {
            foreach (Def def in AllChainDefs())
            {
                yield return (KeyPrefix + def.defName, def.LabelCap);
            }
        }

        public static void EnsureEraRangeEntries(Dictionary<string, QuestGeneratorEraRange> dict)
        {
            TryApplyPatch();
            if (!Active) return;
            foreach (Def def in AllChainDefs())
            {
                string key = KeyPrefix + def.defName;
                if (!dict.ContainsKey(key)) dict[key] = new QuestGeneratorEraRange();
            }
        }

        public static void EnsureDelayRangeEntries(Dictionary<string, IntRange> dict)
        {
            if (!Active) return;
            foreach (Def def in AllChainDefs())
            {
                string key = KeyPrefix + def.defName;
                if (!dict.ContainsKey(key)) dict[key] = new IntRange(0, 0);
            }
        }

        private static bool IsEntryQuest(QuestScriptDef quest, out Def chainDef)
        {
            chainDef = null;
            if (quest?.modExtensions == null) return false;
            foreach (DefModExtension ext in quest.modExtensions)
            {
                if (ext.GetType() != chainExtType) continue;

                chainDef = questChainDefField.GetValue(ext) as Def;
                bool hasSucceed = conditionSucceedQuestsField.GetValue(ext) is ICollection succeed && succeed.Count > 0;
                bool hasFail = conditionFailQuestsField.GetValue(ext) is ICollection fail && fail.Count > 0;
                bool hasEither = conditionEitherField.GetValue(ext) != null;
                bool hasCount = conditionSucceedQuestsCountField.GetValue(ext) is ICollection count && count.Count > 0;
                return !hasSucceed && !hasFail && !hasEither && !hasCount;
            }
            return false;
        }

        public static bool TryScheduleQuestPrefix(QuestScriptDef quest)
        {
            if (!IsEntryQuest(quest, out Def chainDef) || chainDef == null) return true;
            return ProgressionPacingModSettings.IsQuestGeneratorAllowed(KeyPrefix + chainDef.defName);
        }

        public static bool CreateQuestPrefix(QuestScriptDef questDef)
        {
            if (!IsEntryQuest(questDef, out Def chainDef) || chainDef == null) return true;
            string key = KeyPrefix + chainDef.defName;

            var comp = Current.Game?.GetComponent<ProgressionPacingGameComponent>();
            if (comp == null || comp.HasAppliedChainDelay(key)) return true;
            comp.MarkChainDelayApplied(key);

            IntRange range = ProgressionPacingModSettings.GetChainDelayRange(key);
            int minDays = Mathf.Clamp(range.min, 0, 60);
            int maxDays = Mathf.Clamp(range.max, minDays, 60);
            if (maxDays <= 0) return true;

            int delayTicks = Rand.RangeInclusive(minDays, maxDays) * GenDate.TicksPerDay;

            ScheduleQuestInTicks(questDef, delayTicks);
            return false;
        }

        private static void ScheduleQuestInTicks(QuestScriptDef quest, int ticksInFuture)
        {
            if (!Active || scheduleQuestInTicksMethod == null || instanceField == null) return;
            object instance = instanceField.GetValue(null);
            if (instance == null) return;
            scheduleQuestInTicksMethod.Invoke(instance, new object[] { quest, ticksInFuture });
        }

        public static void RescanBlockedChains()
        {
            if (!Active || tryScheduleQuestsMethod == null || instanceField == null) return;
            object instance = instanceField.GetValue(null);
            if (instance == null) return;
            tryScheduleQuestsMethod.Invoke(instance, null);
        }
    }
}
