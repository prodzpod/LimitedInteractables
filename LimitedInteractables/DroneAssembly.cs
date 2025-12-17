using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace LimitedInteractables
{
    public class DroneAssembly
    {
        public static void Patch()
        {
            var isc = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/DLC3/DroneAssemblyStation/iscDroneAssemblyStation.asset").WaitForCompletion();
            isc.directorCreditCost = (int)Main.DroneAssemblyCost.Value;
            isc.maxSpawnsPerStage = Main.DroneAssemblyMaxSpawns.Value;
            var dc = new DirectorCard() { spawnCard = isc, selectionWeight = (int)Main.DroneAssemblyFrequency.Value };
            var dch = new DirectorAPI.DirectorCardHolder() { Card = dc, InteractableCategory = DirectorAPI.InteractableCategory.Duplicator };
            foreach (var sname in Main.DroneAssemblyStages.Value.Split(',').ToList().ConvertAll(x => x.Trim()))
            {
                var found = false;
                foreach (var _s in Enum.GetValues(typeof(DirectorAPI.Stage)))
                {
                    DirectorAPI.Stage s = (DirectorAPI.Stage)_s;
                    if (s == DirectorAPI.Stage.Custom) continue;
                    if (sname == DirectorAPI.ToInternalStageName(s))
                    {
                        DirectorAPI.Helpers.AddNewInteractableToStage(dch, s);
                        found = true;
                        break;
                    }
                }
                if (!found) DirectorAPI.Helpers.AddNewInteractableToStage(dch, DirectorAPI.Stage.Custom, sname);
            }
            On.RoR2.PurchaseInteraction.Awake += (orig, self) =>
            {
                orig(self);
                if (!NetworkServer.active) return;
                if (self.name.Contains("DroneAssemblyStation")) // somewhat of a differentiating factor
                {
                    Main.InitUses(self.gameObject, Main.DroneAssemblyMaxUses.Value);
                    var t = self.GetComponent<ModelLocator>().modelTransform.gameObject;
                    t.transform.localPosition = t.transform.localPosition + (Vector3.up * .5f);
                    t.transform.eulerAngles = t.transform.eulerAngles + new Vector3(0, 0, 90);
                }
            };
            On.RoR2.PurchaseInteraction.OnInteractionBegin += (orig, self, activator) =>
            {
                if (self.CanBeAffordedByInteractor(activator) && self.name.Contains("DroneAssemblyStation")) Main.uses[self.gameObject]--;
                orig(self, activator);
            };
            On.EntityStates.DroneAssemblyStation.AssemblingDroneState.TransferItem += (orig, self) =>
            {
                orig(self);
                if (Main.uses.ContainsKey(self.gameObject) && Main.uses[self.gameObject] == 0)
                    self.outer.GetComponent<PurchaseInteraction>().SetAvailable(false);
            };
        }
    }
}