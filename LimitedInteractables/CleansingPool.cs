using BepInEx.Bootstrap;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace LimitedInteractables
{
    public class CleansingPool
    {
        public static void Patch()
        {
            Main.onGenerateInteractableCardSelection += (dir, sel) => Main.TweakFrequencyAndCost(sel, "iscShrineCleanse", "SpawnCards/InteractableSpawnCard/iscShrineCleanse", Main.CleansingPoolFrequency.Value, Main.CleansingPoolCost.Value);
            RoR2Application.onLoad += () =>
            {
                if (!Main.CleansingPoolTakesEquipments.Value)
                {
                    ShrineCleanseBehavior.cleansableEquipments = Array.Empty<EquipmentIndex>();
                    CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper.lunarEquipmentIndices = Array.Empty<EquipmentIndex>();
                }
                else if (Main.CleansingPoolVoidLunar.Value && Chainloader.PluginInfos.ContainsKey("bubbet.bubbetsitems"))
                {
                    List<ItemIndex> itemIndexList = new(ShrineCleanseBehavior.cleansableItems);
                    ItemIndex itemIndex = ~ItemIndex.None;
                    for (ItemIndex itemCount = (ItemIndex)ItemCatalog.itemCount; itemIndex < itemCount; ++itemIndex)
                    {
                        ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
                        if (isVoidLunar(itemDef.tier)) itemIndexList.Add(itemIndex);
                    }
                    ShrineCleanseBehavior.cleansableItems = itemIndexList.ToArray();

                    List<ItemIndex> indicies = new();
                    indicies.AddRange(ItemCatalog.lunarItemList);
                    ItemIndex itemIndex2 = ~ItemIndex.None;
                    for (ItemIndex itemCount = (ItemIndex)ItemCatalog.itemCount; itemIndex2 < itemCount; ++itemIndex2)
                    {
                        ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex2);
                        if (isVoidLunar(itemDef.tier)) indicies.Add(itemDef.itemIndex);
                    }
                    CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper.lunarItemIndices = indicies.ToArray();
                }
            };
            On.RoR2.ShopTerminalBehavior.Start += (orig, self) =>
            {
                bool isCleanse = self.gameObject.name.Contains("ShrineCleanse");
                if (isCleanse) Main.InitUses(self.gameObject, Main.CleansingPoolUses.Value);
                orig(self);
            };
            On.RoR2.PurchaseInteraction.OnInteractionBegin += (orig, self, activator) =>
            {
                bool isCleanse = self.gameObject.name.Contains("ShrineCleanse");
                if (isCleanse && Main.uses.ContainsKey(self.gameObject))
                {
                    Main.uses[self.gameObject]--;
                    if (Main.uses[self.gameObject] <= 0) self.GetComponent<PurchaseInteraction>().enabled = false;
                }
                orig(self, activator);
            };
        }
        public static bool isVoidLunar(ItemTier tier)
        {
            return tier == BubbetsItems.BubbetsItemsPlugin.VoidLunarTier.tier;
        }
    }
}