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
            Main.Harmony.PatchAll(typeof(PatchStart));
            Main.Harmony.PatchAll(typeof(PatchSelection));
            Main.Harmony.PatchAll(typeof(PatchInteraction));
        }

        [HarmonyPatch(typeof(ShrineOfRepair.Modules.Interactables.ShrineOfRepairPicker.ShrineRepairManager), "<Start>b__8_0")]
        public class PatchStart
        {
            public static void ILManipulator(ILContext il)
            {
                ILCursor c = new(il);
                c.GotoNext(x => x.MatchStloc(3));
                c.Index++;
                c.Emit(OpCodes.Ldloc_0); // pickupdef
                c.Emit(OpCodes.Ldloc_3); // itemcount
                c.EmitDelegate<Func<PickupDef, int, int>>((pickupDef, count) =>
                {
                    if ((pickupDef?.itemIndex ?? ItemIndex.None) == ItemIndex.None) return count;
                    if (repairList.Contains(ItemCatalog.GetItemDef(pickupDef.itemIndex).name)) return Mathf.Min(count, Main.RepairStackAtOnce.Value);
                    return count;
                });
                c.Emit(OpCodes.Stloc_3);
            }
        }

        [HarmonyPatch(typeof(ShrineOfRepair.Modules.Interactables.ShrineOfRepairPicker.ShrineRepairManager), nameof(ShrineOfRepair.Modules.Interactables.ShrineOfRepairPicker.ShrineRepairManager.HandleSelection), typeof(int))]
        public class PatchSelection
        {
            public static void ILManipulator(ILContext il)
            {
                ILCursor c = new(il);
                c.GotoNext(x => x.MatchStloc(2));
                c.Index++;
                c.Emit(OpCodes.Ldloc_0); // pickupdef
                c.Emit(OpCodes.Ldloc_2); // itemcount
                c.EmitDelegate<Func<PickupDef, int, int>>((pickupDef, count) =>
                {
                    if ((pickupDef?.itemIndex ?? ItemIndex.None) == ItemIndex.None) return count;
                    if (repairList.Contains(ItemCatalog.GetItemDef(pickupDef.itemIndex).name)) return Mathf.Min(count, Main.RepairStackAtOnce.Value);
                    return count;
                });
                c.Emit(OpCodes.Stloc_2);
            }
        }

        [HarmonyPatch(typeof(ShrineOfRepair.Modules.Interactables.ShrineOfRepairPicker.ShrineRepairManager), nameof(ShrineOfRepair.Modules.Interactables.ShrineOfRepairPicker.ShrineRepairManager.HandleInteraction), typeof(Interactor))]
        public class PatchInteraction
        {
            public static void ILManipulator(ILContext il)
            {
                ILCursor c = new(il);
                c.GotoNext(x => x.MatchStloc(3));
                c.Index++;
                c.Emit(OpCodes.Ldloc, 4); // repairItems
                c.Emit(OpCodes.Ldloc, 5); // itemcount
                c.EmitDelegate<Func<KeyValuePair<ItemIndex, ItemIndex>, int, int>>((repairItems, count) =>
                {
                    if (repairList.Contains(ItemCatalog.GetItemDef(repairItems.Key).name)) return Mathf.Min(count, Main.RepairStackAtOnce.Value);
                    return count;
                });
                c.Emit(OpCodes.Stloc, 5);
            }
        }
    }
}