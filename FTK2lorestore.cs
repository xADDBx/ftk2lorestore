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
        public const string PLUGIN_VERSION = "1.0.0";
        public static readonly Harmony HarmonyInstance = new Harmony(PLUGIN_GUID);
        internal static ManualLogSource log;
        internal static BepInEx.Configuration.KeyboardShortcut addLore = new(UnityEngine.KeyCode.F2, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut unlockAll = new(UnityEngine.KeyCode.F3, UnityEngine.KeyCode.LeftShift);
        internal static BepInEx.Configuration.KeyboardShortcut buyAll = new(UnityEngine.KeyCode.F4, UnityEngine.KeyCode.LeftShift);
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
            if (unlockAll.IsDown()) {
                unlock();
                SaveGameHelper.SaveUser(user);
            }
            if (buyAll.IsDown()) {
                buy();
                SaveGameHelper.SaveUser(user);
            }
        }
        private static void unlock() {
            foreach (var item in Env.Configs.LoreStore.Keys.ToList()) {
                if (StatsHelper.GetStat(item, user.Stats) < 0 && Env.Configs.LoreStore[item].DefaultState >= -2) {
                    StatsHelper.SetStat(item, 0, user.Stats, false, true);
                }
            }
        }
        private static void buy() {
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
    }
}