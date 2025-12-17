using RoR2;
using UnityEngine;

namespace LimitedInteractables
{
    public class MealPrep
    {
        public static void Patch()
        {
            On.EntityStates.MealPrep.MealPrepBaseState.OnEnter += (orig, self) =>
            {
                GameObject scrapper = self.outer.gameObject;
                if (!Main.uses.ContainsKey(scrapper)) Main.uses.Add(scrapper, Main.CHEFMaxUses.Value);
                orig(self);
                if (Main.uses[scrapper] <= 0) self.outer.GetComponent<CraftingController>().SetAvailable(false);
            };
            On.EntityStates.MealPrep.Cooking.OnEnter += (orig, self) =>
            {
                GameObject scrapper = self.outer.gameObject;
                if (scrapper != null && Main.uses.ContainsKey(scrapper)) Main.uses[scrapper]--;
                if (Run.instance.runRNG.RangeFloat(0, 1) > Main.CHEFChance.Value) self.pickupToDrop = PickupIndex.none;
                orig(self);
            };
        }
    }
}