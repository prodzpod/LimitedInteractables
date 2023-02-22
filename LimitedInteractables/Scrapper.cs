using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace LimitedInteractables
{
    public class Scrapper
    {
        public static void Patch()
        {
            Main.onGenerateInteractableCardSelection += (dir, sel) => Main.TweakFrequencyAndCost(sel, "iscScrapper", "SpawnCards/InteractableSpawnCard/iscScrapper", Main.ScrapperFrequency.Value, Main.ScrapperCost.Value);
            if (Main.ScrapperStackAtOnce.Value > 0) On.RoR2.ScrapperController.Start += (orig, self) => { self.maxItemsToScrapAtATime = Main.ScrapperStackAtOnce.Value; orig(self); };
            On.EntityStates.Scrapper.ScrapperBaseState.OnEnter += (orig, self) =>
            {
                GameObject scrapper = self.outer.gameObject;
                if (!Main.uses.ContainsKey(scrapper)) Main.uses.Add(scrapper, Main.ScrapperMaxUses.Value);
                orig(self);
                if (Main.uses[scrapper] <= 0) self.outer.GetComponent<PickupPickerController>().SetAvailable(false);
            };
            On.EntityStates.Scrapper.Scrapping.OnEnter += (orig, self) =>
            {
                GameObject scrapper = self.outer.gameObject;
                if (scrapper != null && Main.uses.ContainsKey(scrapper)) Main.uses[scrapper]--;
                orig(self);
            };
        }
    }
}