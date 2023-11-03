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
        public const string PLUGIN_VERSION = "1.0.1";
        public static readonly Harmony HarmonyInstance = new Harmony(PLUGIN_GUID);
        internal static ManualLogSource log;
        internal static BepInEx.Configuration.KeyboardShortcut addLore = new(UnityEngine.KeyCode.F2, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut unlockAll = new(UnityEngine.KeyCode.F3, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut unlockHidden = new(UnityEngine.KeyCode.F4, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut buyAll = new(UnityEngine.KeyCode.F5, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut removeLore = new(UnityEngine.KeyCode.F6, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut sellAll = new(UnityEngine.KeyCode.F7, UnityEngine.KeyCode.LeftShift);
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
                Unlock();
                SaveGameHelper.SaveUser(user);
            }
            if (buyAll.IsDown()) {
                Buy();
                SaveGameHelper.SaveUser(user);
            }
            if (sellAll.IsDown()) {
                Sell();
                SaveGameHelper.SaveUser(user);
            }
            if (unlockHidden.IsDown()) {
                Unlock(true);
            }
        }
        private static void Unlock(bool onlyHidden = false) {
            foreach (var item in Env.Configs.LoreStore.Keys.ToList()) {
                if (onlyHidden) {
                    if (StatsHelper.GetStat(item, user.Stats) == -2) {
                        StatsHelper.SetStat(item, 0, user.Stats, false, true);
                    }
                } else {
                    if (StatsHelper.GetStat(item, user.Stats) < 0 && Env.Configs.LoreStore[item].DefaultState >= -2) {
                        StatsHelper.SetStat(item, 0, user.Stats, false, true);
                    }
                }
            }
        }
        private static void Buy() {
            foreach (var item in Env.Configs.LoreStore.Keys.ToList()) {
                if (!LoreStoreHelper.IsItemPurchased(item, user.Stats) && Env.Configs.LoreStore[item].DefaultState >= -2) {
                    StatsHelper.SetStat(item, 0, user.Stats, false, true);
                }
                while (StatsHelper.GetStat(item, user.Stats) < Env.Configs.LoreStore[item].MaxState) {
                    StatsHelper.AddStat("TOTAL_LORE", LoreStoreHelper.GetItemCost(item, user.Stats), user.Stats, false, true);
                    LoreStoreHelper.PurchaseItem(item, user.Stats);
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
    }
}