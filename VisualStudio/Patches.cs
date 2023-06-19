﻿using Il2Cpp;
using Il2CppTLD.Gear;
using HarmonyLib;
using UnityEngine;

namespace FirePack
{
    internal class Patches
    {
        [HarmonyPatch(typeof(Panel_FeedFire), "CanTakeTorch")]
        [HarmonyPriority(Priority.Last)]
        internal class Panel_FeedFire_CanTakeTorch
        {
            private static void Postfix(ref bool __result)
            {
                if (!Settings.instance.pullTorches) __result = false;
            }
        }

        [HarmonyPatch(typeof(Fire), "CanTakeTorch")]
        [HarmonyPriority(Priority.Last)]
        internal class Fire_CanTakeTorch
        {
            private static void Postfix(ref bool __result)
            {
                if (!Settings.instance.pullTorches) __result = false;
            }
        }

        [HarmonyPatch(typeof(FireManager), "PlayerStartFire")]
        internal class FireManager_PlayerStartFire
        {
            private static void Prefix(FireStarterItem starter)
            {
                if (Settings.instance.consumeTorchOnFirestart && starter.name.StartsWith("GEAR_Torch"))
                {
                    starter.m_ConditionDegradeOnUse = 100;
                    starter.m_ConsumeOnUse = true;
                }
            }
        }

        [HarmonyPatch(typeof(StartGear), "AddAllToInventory")]
        internal class StartGear_AddAllToInventory
        {
            private static void Postfix()
            {
                if (Settings.instance.noWoodMatches) GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(FireUtils.matches, 20);
            }
        }

        [HarmonyPatch(typeof(GearItem), "Awake")]
        internal class DestroyWoodMatches
        {
            private static void Postfix(GearItem __instance)
            {
                if (Settings.instance.noWoodMatches && __instance.name.Replace("(Clone)", "") == "GEAR_WoodMatches")
                {
                    UnityEngine.Object.Destroy(__instance.gameObject);
                }
            }
        }
        [HarmonyPatch(typeof(PlayerManager), "InteractiveObjectsProcessInteraction")]
        internal class PlayerManager_InteractiveObjectsProcessInteraction
        {
            private static void Postfix(PlayerManager __instance)
            {
                float maxRange = __instance.ComputeModifiedPickupRange(GameManager.GetGlobalParameters().m_MaxPickupRange);
                if (GameManager.GetPlayerManagerComponent().GetControlMode() == PlayerControlMode.InFPCinematic)
                {
                    maxRange = 50f;
                }
                GameObject gameObject = __instance.GetInteractiveObjectNearCrosshairs(maxRange);
                if (gameObject != null)
                {
                    Fire thisFire = gameObject.GetComponent<Fire>();
                    if (thisFire != null)
                    {
                        TakeEmbersButton.lastInteractedFire = thisFire;
                        return;
                    }
                    else
                    {
                        Campfire thisCampFire = gameObject.GetComponent<Campfire>();
                        if (thisCampFire != null)
                        {
                            TakeEmbersButton.lastInteractedFire = thisCampFire.Fire;
                            return;
                        }
                        WoodStove thisWoodStove = gameObject.GetComponent<WoodStove>();
                        if (thisWoodStove != null)
                        {
                            TakeEmbersButton.lastInteractedFire = thisWoodStove.Fire;
                            return;
                        }
                    }
                }
            }
        }
    }
}
