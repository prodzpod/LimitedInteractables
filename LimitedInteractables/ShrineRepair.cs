using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LimitedInteractables
{
    public class ShrineRepair
    {
        public static List<string> repairList = new();
        public static void Patch()
        {
            foreach (var entry in Main.RepairRepairList.Value.Split(',')) repairList.Add(entry.Trim());
            Main.Log.LogDebug("Repair Repair List: " + repairList.Join());
            Main.Harmony.PatchAll(typeof(PatchSelection));
            Main.Harmony.PatchAll(typeof(PatchInteraction));
        }

        [HarmonyPatch(typeof(ShrineOfRepair.Behaviours.ShrineOfRepairPickerManager), nameof(ShrineOfRepair.Behaviours.ShrineOfRepairPickerManager.HandleSelection), typeof(int))]
        public class PatchSelection
        {
            public static void ILManipulator(ILContext il)
            {
                ILCursor c = new(il);
                c.GotoNext(x => x.MatchStloc(3));
                c.Emit(OpCodes.Ldloc_0); // pickupdef
                c.EmitDelegate<Func<int, PickupDef, int>>((count, pickupDef) =>
                {
                    if ((pickupDef?.itemIndex ?? ItemIndex.None) == ItemIndex.None) return count;
                    if (repairList.Contains(ItemCatalog.GetItemDef(pickupDef.itemIndex).name)) return Mathf.Min(count, Main.RepairStackAtOnce.Value);
                    return count;
                });
            }
        }

        [HarmonyPatch(typeof(ShrineOfRepair.Behaviours.ShrineOfRepairPickerManager), nameof(ShrineOfRepair.Behaviours.ShrineOfRepairPickerManager.HandleInteraction), typeof(Interactor))]
        public class PatchInteraction
        {
            public static void ILManipulator(ILContext il)
            {
                ILCursor c = new(il);
                c.GotoNext(x => x.MatchStloc(6));
                c.Emit(OpCodes.Ldloc, 5); // repairItems
                c.EmitDelegate<Func<int, KeyValuePair<ItemIndex, ItemIndex>, int>>((count, repairItems) =>
                {
                    if (repairList.Contains(ItemCatalog.GetItemDef(repairItems.Key).name)) return Mathf.Min(count, Main.RepairStackAtOnce.Value);
                    return count;
                });
            }
        }
    }
}