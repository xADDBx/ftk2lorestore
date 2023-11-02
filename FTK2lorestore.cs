using BepInEx;
using BepInEx.Logging;
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
        internal static BepInEx.Configuration.KeyboardShortcut hotkey = new(UnityEngine.KeyCode.F2, UnityEngine.KeyCode.LeftShift);
        internal static UserData user;
        private void Awake() {
            // Plugin startup logic
            log = Logger;
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
        }
        private void Update() {
            if (hotkey.IsDown()) {
                if (user != null) {
                    StatsHelper.AddStat("TOTAL_LORE", 50, user.Stats, false, true);
                    SaveGameHelper.SaveUser(user);
                }
            }
        }
    }
    [HarmonyPatch(typeof(LoreStoreDirector))]
    public static class LoreStoreDirector_Patch {
        [HarmonyPatch(nameof(LoreStoreDirector.Initialize))]
        [HarmonyPrefix]
        public static void Initialize(LoreStoreDirector __instance) {
            FTK2lorestore.user = __instance._env.User;
            foreach (var item in Env.Configs.LoreStore.Keys.ToList()) {
                if (!LoreStoreHelper.IsItemPurchased(item, __instance._env.User.Stats) && Env.Configs.LoreStore[item].DefaultState >= -2) {
                    StatsHelper.SetStat(item, 0, __instance._env.User.Stats, false, true);
                    StatsHelper.AddStat("TOTAL_LORE", LoreStoreHelper.GetItemCost(item, __instance._env.User.Stats), __instance._env.User.Stats, false, true);
                    LoreStoreHelper.PurchaseItem(item, __instance._env.User.Stats);
                }
            }
            SaveGameHelper.SaveUser(__instance._env.User);
        }
    }
}