using RoR2;
using UnityEngine;

namespace LimitedInteractables
{
    public class DroneScrapper
    {
        public static void Patch()
        {
            Main.onGenerateInteractableCardSelection += (dir, sel) => Main.TweakFrequencyAndCost(sel, "iscDroneScrapper", "RoR2/DLC3/DroneScrapper/iscDroneScrapper.asset", Main.DroneScrapperFrequency.Value, Main.DroneScrapperCost.Value);            
            On.EntityStates.DroneScrapper.DroneScrapperBaseState.OnEnter += (orig, self) =>
            {
                GameObject scrapper = self.outer.gameObject;
                if (!Main.uses.ContainsKey(scrapper)) Main.uses.Add(scrapper, Main.DroneScrapperMaxUses.Value);
                orig(self);
                if (Main.uses[scrapper] <= 0) self.outer.GetComponent<DroneScrapperPickerController>().SetAvailable(false);
            };
            On.EntityStates.DroneScrapper.DroneScrapping.OnEnter += (orig, self) =>
            {
                GameObject scrapper = self.outer.gameObject;
                if (scrapper != null && Main.uses.ContainsKey(scrapper)) Main.uses[scrapper]--;
                orig(self);
            };
        }
    }
}