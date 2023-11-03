using BepInEx;
using BepInEx.Logging;
using ftk2lorestore;
using HarmonyLib;
using System.Linq;
using System.Reflection;

namespace ftk2lorestore {
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class FTK2lorestore : BaseUnityPlugin {
        public const string PLUGIN_GUID = "ftk2lorestore";
        public const string PLUGIN_NAME = "FTK 2 Lore Store Cheats";
        public const string PLUGIN_VERSION = "1.0.2";
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
        private void Awake() {
            // Plugin startup logic
            log = Logger;
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
        }
        private void Update() {
            if (user == null) return;
            if (addLore.IsDown()) {
                StatsHelper.AddStat("TOTAL_LORE", 50, user.Stats, false, true);
                SaveGameHelper.SaveUser(user);
            }
            if (removeLore.IsDown()) {
                var current = StatsHelper.GetStat("TOTAL_LORE", user.Stats);
                var dec = (current >= 50) ? 50 : current;
                StatsHelper.AddStat("TOTAL_LORE", -dec, user.Stats, false, true);
                SaveGameHelper.SaveUser(user);
            }
            if (unlockAll.IsDown()) {
                Unlock(true, true);
                SaveGameHelper.SaveUser(user);
            }
            if (buyAll.IsDown()) {
                Buy(true);
                SaveGameHelper.SaveUser(user);
            }
            if (buyUnlocked.IsDown()) {
                Buy(false);
                SaveGameHelper.SaveUser(user);
            }
            if (sellAll.IsDown()) {
                Sell();
                SaveGameHelper.SaveUser(user);
            }
            if (unlockHidden.IsDown()) {
                Unlock(false, true);
                SaveGameHelper.SaveUser(user);
            }
            if (unlockNonHidden.IsDown()) {
                Unlock(true, false);
                SaveGameHelper.SaveUser(user);
            }
            if (resetAll.IsDown()) {
                Reset();
                SaveGameHelper.SaveUser(user);
            }
        }
        private static void Unlock(bool includeNonHidden, bool includeHidden) {
            foreach (var item in Env.Configs.LoreStore.Keys.ToList()) {
                if (includeHidden) {
                    if (StatsHelper.GetStat(item, user.Stats) == -2) {
                        StatsHelper.SetStat(item, 0, user.Stats, false, true);
                    }
                }
                if (includeNonHidden) {
                    if (StatsHelper.GetStat(item, user.Stats) < 0 && Env.Configs.LoreStore[item].DefaultState == -1) {
                        StatsHelper.SetStat(item, 0, user.Stats, false, true);
                    }
                }
            }
        }
        private static void Buy(bool forceUnlock) {
            if (forceUnlock) Unlock(true, true);
            foreach (var item in Env.Configs.LoreStore.Keys.ToList()) {
                if (StatsHelper.GetStat(item, user.Stats) >= 0) {
                    while (StatsHelper.GetStat(item, user.Stats) < Env.Configs.LoreStore[item].MaxState) {
                        StatsHelper.AddStat("TOTAL_LORE", LoreStoreHelper.GetItemCost(item, user.Stats), user.Stats, false, true);
                        LoreStoreHelper.PurchaseItem(item, user.Stats);
                    }
                }
            }
        }
        private static void Sell() {
            foreach (var item in Env.Configs.LoreStore.Keys.ToList()) {
                while (StatsHelper.GetStat(item, user.Stats) > 0) {
                    StatsHelper.AddStat(item, -1, user.Stats, pPerRun: false, pCheckLoreUnlocks: true);
                    StatsHelper.AddStat(eUserStats.LORE_STORE_PURCHASES, -1, user.Stats, pPerRun: false, pCheckLoreUnlocks: true);
                    StatsHelper.AddStat(eUserStats.LORE_POINTS_SPENT, -Env.Configs.LoreStore[item].Cost, user.Stats, pPerRun: false, pCheckLoreUnlocks: true);
                    StatsHelper.AddStat("TOTAL_LORE", Env.Configs.LoreStore[item].Cost, user.Stats, pPerRun: false, pCheckLoreUnlocks: true);
                }
            }
        }
        private static void Reset() {
            foreach (var item in Env.Configs.LoreStore.Keys.ToList()) {
                StatsHelper.SetStat(item, Env.Configs.LoreStore[item].DefaultState, user.Stats, false, true);
            }
        }
    }
}