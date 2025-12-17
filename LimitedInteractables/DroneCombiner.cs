using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace LimitedInteractables
{
    public class DroneCombiner
    {
        public static void Patch()
        {
            Main.onGenerateInteractableCardSelection += (dir, sel) => Main.TweakFrequencyAndCost(sel, "iscDroneCombinerStation", "RoR2/DLC3/DroneCombinerStation/iscDroneCombinerStation.asset", Main.DroneCombinerFrequency.Value, Main.DroneCombinerCost.Value, Main.DroneAssemblyMaxSpawns.Value);
            On.RoR2.PurchaseInteraction.Awake += (orig, self) =>
            {
                orig(self);
                if (!NetworkServer.active) return;
                if (self.name.Contains("DroneCombinerStation")) Main.InitUses(self.gameObject, Main.DroneCombinerMaxUses.Value);
            };
            On.RoR2.PurchaseInteraction.OnInteractionBegin += (orig, self, activator) =>
            {
                if (self.CanBeAffordedByInteractor(activator) && self.name.Contains("DroneCombinerStation")) Main.uses[self.gameObject]--;
                orig(self, activator);
            };
            On.EntityStates.DroneCombiner.DroneCombinerCombining.OnEnter += (orig, self) =>
            {
                orig(self);
                if (Main.uses.ContainsKey(self.gameObject) && Main.uses[self.gameObject] == 0)
                    self.outer.GetComponent<PurchaseInteraction>().SetAvailable(false);
            };
        }
    }
}