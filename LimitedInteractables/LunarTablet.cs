using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace LimitedInteractables
{
    public class LunarTablet
    {
        public static PurchaseInteraction slab;
        public static int uses = 0;
        public static void Patch()
        {
            Stage.onStageStartGlobal += (self) =>
            {
                if (self.sceneDef.cachedName != "bazaar") return;
                PurchaseInteraction pi = GameObject.Find("HOLDER: Store")?.transform?.Find("LunarShop")?.Find("LunarRecycler")?.GetComponent<PurchaseInteraction>();
                if (pi != null)
                {
                    if (Main.LunarTabletCost.Value == 0) pi.gameObject.SetActive(false);
                    else
                    {
                        slab = pi;
                        pi.cost = Main.LunarTabletCost.Value;
                        uses = Main.LunarTabletUses.Value;
                    }
                }
            };
            On.RoR2.PurchaseInteraction.OnInteractionBegin += (orig, self, activator) =>
            {
                bool yes = self.CanBeAffordedByInteractor(activator);
                orig(self, activator);
                if (self == slab && yes)
                {
                    uses--;
                    if (uses == 0) self.enabled = false;
                }
            };
        }
    }
}