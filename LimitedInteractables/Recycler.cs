using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;

namespace LimitedInteractables
{
    public class Recycler
    {
        public static Dictionary<GenericPickupController, int> uses;
        public static void Patch()
        {
            uses = new();
            Stage.onServerStageComplete += stage => uses.Clear();
            On.RoR2.GenericPickupController.Start += (orig, self) =>
            {
                if (!uses.ContainsKey(self)) uses.Add(self, Main.RecyclerMaxUses.Value);
                else uses[self] = Main.RecyclerMaxUses.Value;
                orig(self);
            };
            IL.RoR2.EquipmentSlot.FireRecycle += (il) =>
            {
                ILCursor c = new(il);
                c.GotoNext(x => x.MatchLdcI4(1), x => x.MatchCallOrCallvirt<GenericPickupController>("set_" + nameof(GenericPickupController.NetworkRecycled)));
                c.Remove();
                c.Emit(OpCodes.Ldloc_0);
                c.EmitDelegate<Func<GenericPickupController, bool>>(self =>
                {
                    uses[self]--;
                    if (uses[self] == 0) return true;
                    return false;
                });
            };
            On.RoR2.Language.GetLocalizedStringByToken += (orig, self, token) =>
            {
                if (token == "EQUIPMENT_RECYCLER_PICKUP")
                {
                    string ret = "Transform an Item or Equipment into a different one.";
                    if (Main.RecyclerMaxUses.Value > 0) ret += " Can only recycle up to " + Main.RecyclerMaxUses.Value + " times.";
                    return ret;
                }
                if (token == "EQUIPMENT_RECYCLER_DESC")
                {
                    string ret = "<style=cIsUtility>Transform</style> an Item or Equipment into a different one.";
                    if (Main.RecyclerMaxUses.Value > 0) ret += " <style=cIsUtility>Can only be converted into the same tier " + Main.RecyclerMaxUses.Value + " times</style>.";
                    return ret;
                }
                return orig(self, token);
            };
        }
    }
}