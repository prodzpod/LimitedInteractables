using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using RoR2;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LimitedInteractables
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("bubbet.bubbetsitems", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Viliger.ShrineOfRepair", BepInDependency.DependencyFlags.SoftDependency)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "prodzpod";
        public const string PluginName = "LimitedInteractables";
        public const string PluginVersion = "1.0.0";
        public static ManualLogSource Log;
        public static Harmony Harmony;
        public static PluginInfo pluginInfo;
        public static ConfigFile Config;
        public static ConfigEntry<int> ScrapperStackAtOnce;
        public static ConfigEntry<float> ScrapperFrequency;
        public static ConfigEntry<float> ScrapperCost;
        public static ConfigEntry<int> ScrapperMaxUses;
        public static ConfigEntry<float> DuplicatorFrequencyWhite;
        public static ConfigEntry<float> DuplicatorFrequencyGreen;
        public static ConfigEntry<float> DuplicatorFrequencyRed;
        public static ConfigEntry<float> DuplicatorFrequencyYellow;
        public static ConfigEntry<float> DuplicatorCostWhite;
        public static ConfigEntry<float> DuplicatorCostGreen;
        public static ConfigEntry<float> DuplicatorCostRed;
        public static ConfigEntry<float> DuplicatorCostYellow;
        public static ConfigEntry<int> DuplicatorUsesWhite;
        public static ConfigEntry<int> DuplicatorUsesGreen;
        public static ConfigEntry<int> DuplicatorUsesRed;
        public static ConfigEntry<int> DuplicatorUsesYellow;
        public static ConfigEntry<string> RepairRepairList;
        public static ConfigEntry<int> RepairStackAtOnce;
        public static ConfigEntry<int> LunarTabletCost;
        public static ConfigEntry<int> LunarTabletUses;
        public static ConfigEntry<bool> CleansingPoolTakesEquipments;
        public static ConfigEntry<bool> CleansingPoolVoidLunar;
        public static ConfigEntry<float> CleansingPoolFrequency;
        public static ConfigEntry<float> CleansingPoolCost;
        public static ConfigEntry<int> CleansingPoolUses;
        public static ConfigEntry<int> RecyclerMaxUses;

        public static Dictionary<GameObject, int> uses;
        public static event Action<SceneDirector, DirectorCardCategorySelection> onGenerateInteractableCardSelection;
        public void Awake()
        {
            pluginInfo = Info;
            Harmony = new Harmony(PluginGUID); // uh oh!
            Log = Logger;
            Config = new ConfigFile(System.IO.Path.Combine(Paths.ConfigPath, PluginGUID + ".cfg"), true);

            uses = new();
            Stage.onServerStageComplete += stage => uses.Clear();

            DuplicatorFrequencyWhite = Config.Bind("3D Printer Tweaks", "Common Printer Frequency", 1f, "Multiplier for Scrapper spawn frequency.");
            DuplicatorFrequencyGreen = Config.Bind("3D Printer Tweaks", "Uncommon Printer Frequency", 1f, "Multiplier for Scrapper spawn frequency.");
            DuplicatorFrequencyRed = Config.Bind("3D Printer Tweaks", "Legendary Printer Frequency", 1f, "Multiplier for Scrapper spawn frequency.");
            DuplicatorFrequencyYellow = Config.Bind("3D Printer Tweaks", "Boss Printer Frequency", 1f, "Multiplier for Scrapper spawn frequency.");
            DuplicatorCostWhite = Config.Bind("3D Printer Tweaks", "Common Printer Cost", 1f, "Multiplier for Scrapper spawn cost.");
            DuplicatorCostGreen = Config.Bind("3D Printer Tweaks", "Uncommon Printer Cost", 1f, "Multiplier for Scrapper spawn cost.");
            DuplicatorCostRed = Config.Bind("3D Printer Tweaks", "Legendary Printer Cost", 1f, "Multiplier for Scrapper spawn cost.");
            DuplicatorCostYellow = Config.Bind("3D Printer Tweaks", "Boss Printer Cost", 0.8f, "Multiplier for Scrapper spawn cost.");
            DuplicatorUsesWhite = Config.Bind("3D Printer Tweaks", "Common Printer Max Uses", 0, "Max number of items to duplicate. Set to 0 to disable.");
            DuplicatorUsesGreen = Config.Bind("3D Printer Tweaks", "Uncommon Printer Max Uses", 0, "Max number of items to duplicate. Set to 0 to disable.");
            DuplicatorUsesRed = Config.Bind("3D Printer Tweaks", "Legendary Printer Max Uses", 0, "Max number of items to duplicate. Set to 0 to disable.");
            DuplicatorUsesYellow = Config.Bind("3D Printer Tweaks", "Boss Printer Max Uses", 1, "Max number of items to duplicate. Set to 0 to disable.");
            Duplicator.Patch();

            ScrapperStackAtOnce = Config.Bind("Scrapper Tweaks", "Scrapper Stacks at Once", 1, "Max number of items to scrap at once. Set to 0 to disable.");
            ScrapperFrequency = Config.Bind("Scrapper Tweaks", "Scrapper Frequency", 1f, "Multiplier for Scrapper spawn frequency.");
            ScrapperCost = Config.Bind("Scrapper Tweaks", "Scrapper Cost", 1f, "Multiplier for Scrapper spawn cost.");
            ScrapperMaxUses = Config.Bind("Scrapper Tweaks", "Scrapper Max Uses", 3, "Max number of items to scrap per scrapper. Set to 0 to disable.");
            Scrapper.Patch();

            RepairRepairList = Config.Bind("Shrine of Repair Tweaks", "Shrine of Repair True Repair List", "ExtraLifeConsumed, ExtraLifeVoidConsumed, FragileDamageBonusConsumed, HealingPotionConsumed, RegeneratingScrapConsumed, BossHunterConsumed", "List of repairs to count in the following configs.");
            RepairStackAtOnce = Config.Bind("Shrine of Repair Tweaks", "Shrine of Repair Stacks at Once", 0, "Max number of items to repair at once. ONLY AFFECTS ONES ON TRUE REPAIR LIST. Set to 0 to disable.");
            if (Chainloader.PluginInfos.ContainsKey("com.Viliger.ShrineOfRepair") && RepairStackAtOnce.Value > 0) ShrineRepair.Patch();

            LunarTabletCost = Config.Bind("Lunar Tablet Tweaks", "Lunar Tablet Start Cost", 5, "Set to 0 to remove it completely");
            LunarTabletUses = Config.Bind("Lunar Tablet Tweaks", "Lunar Tablet Max Uses", 0, "Set to 0 for vanilla behaviour");
            LunarTablet.Patch();

            CleansingPoolTakesEquipments = Config.Bind("Cleansing Pool Tweaks", "Cleansing Pool Accepts Lunar Equipments", true, "yeah");
            CleansingPoolVoidLunar = Config.Bind("Cleansing Pool Tweaks", "Cleansing Pool Accepts Void Lunar", true, "Set to false to not accept void lunar at in Cleansing Pools.");
            CleansingPoolFrequency = Config.Bind("Cleansing Pool Tweaks", "Cleansing Pool Frequency", 1f, "Multiplier for Cleansing Pool spawn frequency.");
            CleansingPoolCost = Config.Bind("Cleansing Pool Tweaks", "Cleansing Pool Cost", 1f, "Multiplier for Cleansing Pool spawn cost.");
            CleansingPoolUses = Config.Bind("Cleansing Pool Tweaks", "Cleansing Pool Max Uses", 0, "Set to 0 for vanilla behaviour");
            CleansingPool.Patch();

            RecyclerMaxUses = Config.Bind("Recycler Tweaks", "Recycler Max Uses", 1, "Vanilla: 1, set to 0 to disable.");
            Recycler.Patch();

            SceneDirector.onGenerateInteractableCardSelection += (director, selection) =>
            {
                if (!NetworkServer.active) return;
                if (onGenerateInteractableCardSelection != null) onGenerateInteractableCardSelection(director, selection);
            };
        }
        public static void InitUses(GameObject self, int use)
        {
            if (!NetworkServer.active) return;
            if (!uses.ContainsKey(self)) uses.Add(self, use);
            else uses[self] = use;
        }
        public static void TweakFrequencyAndCost(DirectorCardCategorySelection dccs, string name, string cscName, float frequency, float cost)
        {
            int index = GetCategoryIndex(dccs, name);
            if (index < 0) return;
            DirectorCard orig = GetCard(dccs, index, name);
            if (orig == null) return;
            List<DirectorCard> cards = dccs.categories[index].cards.ToList();
            cards.Remove(orig);
            dccs.categories[index].cards = cards.ToArray();
            if (orig.selectionWeight * frequency <= 0) return; 
            SpawnCard csc = LegacyResourcesAPI.Load<SpawnCard>(cscName);
            csc.directorCreditCost = Mathf.Max(1, (int)(csc.directorCreditCost * cost));
            DirectorCard card = new()
            {
                spawnCard = csc,
                selectionWeight = Mathf.Max(0, (int)(orig.selectionWeight * frequency))
            };
            dccs.AddCard(index, card);
        }

        public static int GetCategoryIndex(DirectorCardCategorySelection dccs, string name)
        {
            for (int i = 0; i < dccs.categories.Length; ++i) foreach (DirectorCard card in dccs.categories[i].cards) if (card.spawnCard.name.StartsWith(name)) return i;
            return -1;
        }
        public static DirectorCard GetCard(DirectorCardCategorySelection dccs, int category, string name)
        {
            foreach (DirectorCard card in dccs.categories[category].cards)
                if (card.spawnCard.name.StartsWith(name)) return card;
            return null;
        }
    }
}
