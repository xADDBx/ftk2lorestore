using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ftk2lorestore {
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class FTK2lorestore : BaseUnityPlugin {
        public const string PLUGIN_GUID = "ftk2lorestore";
        public const string PLUGIN_NAME = "FTK 2 Lore Store Cheats";
        public const string PLUGIN_VERSION = "1.3.0";
        public static readonly Harmony HarmonyInstance = new Harmony(PLUGIN_GUID);
        internal static ManualLogSource log;
        internal static BepInEx.Configuration.KeyboardShortcut addLore = new(UnityEngine.KeyCode.F2, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut unlockAll = new(UnityEngine.KeyCode.F3, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut unlockHidden = new(UnityEngine.KeyCode.F4, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut unlockNonHidden = new(UnityEngine.KeyCode.F5, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut buyAll = new(UnityEngine.KeyCode.F6, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut buyUnlocked = new(UnityEngine.KeyCode.F7, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut removeLore = new(UnityEngine.KeyCode.F8, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut sellAll = new(UnityEngine.KeyCode.F9, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut resetAll = new(UnityEngine.KeyCode.F10, UnityEngine.KeyCode.LeftShift);
        internal static UserData user => RouterMono.GetEnv().User;
        internal void Awake() {
            // Plugin startup logic
            log = Logger;
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
        }
        internal void Update() {
            if (user == null) return;
            if (addLore.IsDown()) {
                StatsHelper.AddStat("TOTAL_LORE", 50);
                SaveGameHelper.SaveUserAsync(user);
            }
            if (removeLore.IsDown()) {
                var current = getStat("TOTAL_LORE", user.LocalStats);
                var dec = (current >= 50) ? 50 : current;
                StatsHelper.AddStat("TOTAL_LORE", -dec);
                SaveGameHelper.SaveUserAsync(user);
            }
            if (unlockAll.IsDown()) {
                Unlock(true, true);
                SaveGameHelper.SaveUserAsync(user);
            }
            if (buyAll.IsDown()) {
                Buy(true);
                SaveGameHelper.SaveUserAsync(user);
            }
            if (buyUnlocked.IsDown()) {
                Buy(false);
                SaveGameHelper.SaveUserAsync(user);
            }
            if (sellAll.IsDown()) {
                Sell();
                SaveGameHelper.SaveUserAsync(user);
            }
            if (unlockHidden.IsDown()) {
                Unlock(false, true);
                SaveGameHelper.SaveUserAsync(user);
            }
            if (unlockNonHidden.IsDown()) {
                Unlock(true, false);
                SaveGameHelper.SaveUserAsync(user);
            }
            if (resetAll.IsDown()) {
                Reset();
                SaveGameHelper.SaveUserAsync(user);
            }
        }
        private static int getStat(string pStat, Dictionary<string, int> pStats) {
            if (pStats.TryGetValue(pStat, out var value)) {
                return value;
            }

            return 0;
        }
        private static void setStat(string pStat, int pValue, Dictionary<string, int> pStats) {
            if (!pStats.ContainsKey(pStat)) {
                pStats.Add(pStat, 0);
            }

            pStats[pStat] = pValue;
        }
        private static void Unlock(bool includeNonHidden, bool includeHidden) {
            foreach (var item in Env.Configs.LoreStore.Keys.ToList()) {
                if (includeHidden) {
                    if (getStat(item, user.LocalStats) == -2 && Env.Configs.LoreStore[item].MaxState >= 0) {
                        setStat(item, 0, user.LocalStats);
                    }
                }
                if (includeNonHidden) {
                    if (getStat(item, user.LocalStats) < 0 && Env.Configs.LoreStore[item].DefaultState == -1) {
                        setStat(item, 0, user.LocalStats);
                    }
                }
            }
        }
        private static void Buy(bool forceUnlock) {
            if (forceUnlock) Unlock(true, true);
            foreach (var item in Env.Configs.LoreStore.Keys.ToList()) {
                if (getStat(item, user.LocalStats) >= 0) {
                    while (getStat(item, user.LocalStats) < Env.Configs.LoreStore[item].MaxState || (getStat(item, user.LocalStats) == 0 && Env.Configs.LoreStore[item].MaxState == -2)) {
                        StatsHelper.AddStat("TOTAL_LORE", LoreStoreHelper.GetItemCost(item));
                        LoreStoreHelper.PurchaseItem(item, LoreStoreHelper.GetItemCost(item));
                    }
                }
            }
        }
        private static void Sell() {
            foreach (var item in Env.Configs.LoreStore.Keys.ToList()) {
                while (getStat(item, user.LocalStats) > 0 && getStat(item, user.LocalStats) > Env.Configs.LoreStore[item].DefaultState) {
                    StatsHelper.AddStat(item, -1);
                    StatsHelper.AddStat(eUserStats.LORE_STORE_PURCHASES, -1);
                    StatsHelper.AddStat(eUserStats.LORE_POINTS_SPENT, -Env.Configs.LoreStore[item].Cost);
                    StatsHelper.AddStat("TOTAL_LORE", Env.Configs.LoreStore[item].Cost);
                }
            }
        }
        private static void Reset() {
            foreach (var item in Env.Configs.LoreStore.Keys.ToList()) {
                setStat(item, Env.Configs.LoreStore[item].DefaultState, user.LocalStats);
            }
        }
    }
}