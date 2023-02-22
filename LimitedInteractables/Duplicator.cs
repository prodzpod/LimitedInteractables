using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace LimitedInteractables
{
    public class Duplicator
    {
        public static void Patch()
        {
            Main.onGenerateInteractableCardSelection += (dir, sel) => Main.TweakFrequencyAndCost(sel, "iscDuplicator", "SpawnCards/InteractableSpawnCard/iscDuplicator", Main.DuplicatorFrequencyWhite.Value, Main.DuplicatorCostWhite.Value);
            Main.onGenerateInteractableCardSelection += (dir, sel) => Main.TweakFrequencyAndCost(sel, "iscDuplicatorLarge", "SpawnCards/InteractableSpawnCard/iscDuplicatorLarge", Main.DuplicatorFrequencyGreen.Value, Main.DuplicatorCostGreen.Value);
            Main.onGenerateInteractableCardSelection += (dir, sel) => Main.TweakFrequencyAndCost(sel, "iscDuplicatorMilitary", "SpawnCards/InteractableSpawnCard/iscDuplicatorMilitary", Main.DuplicatorFrequencyRed.Value, Main.DuplicatorCostRed.Value);
            Main.onGenerateInteractableCardSelection += (dir, sel) => Main.TweakFrequencyAndCost(sel, "iscDuplicatorWild", "SpawnCards/InteractableSpawnCard/iscDuplicatorWild", Main.DuplicatorFrequencyYellow.Value, Main.DuplicatorCostYellow.Value);
            On.RoR2.PurchaseInteraction.Awake += (orig, self) =>
            {
                orig(self);
                if (!NetworkServer.active) return;
                if (self.name.Contains("DuplicatorWild")) Main.InitUses(self.gameObject, Main.DuplicatorUsesYellow.Value);
                else if (self.name.Contains("DuplicatorMilitary")) Main.InitUses(self.gameObject, Main.DuplicatorUsesRed.Value);
                else if (self.name.Contains("DuplicatorLarge")) Main.InitUses(self.gameObject, Main.DuplicatorUsesGreen.Value);
                else if (self.name.Contains("Duplicator")) Main.InitUses(self.gameObject, Main.DuplicatorUsesWhite.Value);
            };
            On.RoR2.PurchaseInteraction.OnInteractionBegin += (orig, self, activator) =>
            {
                if (self.CanBeAffordedByInteractor(activator) && self.name.Contains("Duplicator")) Main.uses[self.gameObject]--;
                orig(self, activator);
            };
            On.EntityStates.Duplicator.Duplicating.DropDroplet += (orig, self) =>
            {
                orig(self);
                if (Main.uses.ContainsKey(self.gameObject) && Main.uses[self.gameObject] == 0)
                {
                    self.outer.GetComponent<ShopTerminalBehavior>().SetHasBeenPurchased(true);
                    self.outer.GetComponent<ShopTerminalBehavior>().SetNoPickup();
                    self.outer.GetComponent<PurchaseInteraction>().Networkavailable = false;
                }
            };
        }
    }
}