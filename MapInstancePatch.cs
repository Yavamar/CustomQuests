using System;
using HarmonyLib;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomQuest
{
    [HarmonyPatch(typeof(MapInstance), "Start")]
    internal class MapInstancePatch
    {
        public static void Postfix(MapInstance __instance)
        {
            foreach (ParsedQuestTrigger trigger in Plugin.parsedQuestTriggers)
            {
                if (trigger._mapInstance == __instance._mapName)
                {
                    Plugin.Logger.LogMessage("Loading QuestTrigger " + trigger._questTriggerTag);

                    GameObject[] triggers = Resources.LoadAll<GameObject>("_prefab/_entity/_trigger/");
                    
                    GameObject questTriggerPrefab = new();

                    foreach (GameObject a in triggers)
                    {
                        //Plugin.Logger.LogMessage(a.name);
                        if (a.GetComponent<QuestTrigger>() != null)
                        {
                            //Plugin.Logger.LogMessage("It's this one!");
                            questTriggerPrefab = a;
                            break;
                        }
                    }
                    
                    Plugin.Logger.LogInfo("Registering Quest Trigger prefab");

                    questTriggerPrefab.GetComponent<QuestTrigger>()._visualContainer = null;
                    questTriggerPrefab.GetComponent<QuestTrigger>()._questTriggerAnimator = null;
                    questTriggerPrefab.GetComponent<QuestTrigger>()._questTriggerParticles = null;
                    questTriggerPrefab.GetComponent<QuestTrigger>()._aSrcOnInteract = null;
                    questTriggerPrefab.GetComponent<QuestTrigger>()._disableVisualIfNotOnQuest = true;

                    NetworkClient.RegisterPrefab(questTriggerPrefab);
                    Plugin.Logger.LogInfo("Quest Trigger Prefab Registered");

                    GameObject questTriggerObject = UnityEngine.Object.Instantiate(questTriggerPrefab);
                    questTriggerObject.transform.SetPositionAndRotation(trigger.position, Quaternion.identity);

                    /*
                    NetworkIdentity netID = questTriggerObject.GetComponent<NetworkIdentity>();
                    netID._assetId = 0;
                    netID.serverOnly = false;
                    netID.visible = Visibility.Default;
                    netID.hasSpawned = false;
                    */

                    QuestTrigger questTrigger = questTriggerObject.GetComponent<QuestTrigger>();
                    questTrigger._scriptQuest = trigger._scriptQuest;

                    questTrigger._mapInstance = __instance;

                    questTrigger._questTriggerTag = trigger._questTriggerTag;
                    questTrigger._interactTrigger = trigger._interactTrigger;
                    questTrigger._completedRequirements = trigger._completedRequirements;
                    questTrigger._questTriggerOnlyOnce = trigger._questTriggerOnlyOnce;

                    questTrigger._netTriggerToInvoke = null; // Need to figure out how to set this
                    Enum.TryParse(trigger._netTriggerType, out questTrigger._netTriggerType);

                    questTrigger._triggerMessage = trigger._triggerMessage;

                    questTrigger._patternInstanceManager = null; //__instance, but only if it's _arenaSweepQuest I think?
                    Enum.TryParse("", out questTrigger._difficultyRequirement);
                    questTrigger._arenaSweepQuest = trigger._arenaSweepQuest;

                    /*
                    questTrigger._visualContainer = null;
                    questTrigger._questTriggerAnimator = null;
                    questTrigger._questTriggerParticles = null;
                    questTrigger._aSrcOnInteract = null;
                    questTrigger._disableVisualIfNotOnQuest = true;
                    */

                    // Collider
                    switch (trigger._triggerCollider.type)
                    {
                        case "Sphere":
                            questTrigger._triggerCollider = questTrigger.gameObject.AddComponent<SphereCollider>();
                            (questTrigger._triggerCollider as SphereCollider).center = trigger._triggerCollider.center;
                            (questTrigger._triggerCollider as SphereCollider).radius = trigger._triggerCollider.radius;
                            break;
                        case "Box":
                            questTrigger._triggerCollider = questTrigger.gameObject.AddComponent<BoxCollider>();
                            (questTrigger._triggerCollider as BoxCollider).center = trigger._triggerCollider.center;
                            (questTrigger._triggerCollider as BoxCollider).size = trigger._triggerCollider.size;
                            break;
                        case "Capsule":
                            questTrigger._triggerCollider = questTrigger.gameObject.AddComponent<CapsuleCollider>();
                            (questTrigger._triggerCollider as CapsuleCollider).center = trigger._triggerCollider.center;
                            (questTrigger._triggerCollider as CapsuleCollider).radius = trigger._triggerCollider.radius;
                            (questTrigger._triggerCollider as CapsuleCollider).height = trigger._triggerCollider.height;
                            (questTrigger._triggerCollider as CapsuleCollider).radius = trigger._triggerCollider.direction;
                            break;

                    }

                    //questTrigger._triggerCollider.enabled = true;
                    questTrigger._triggerCollider.isTrigger = true;
                    questTrigger._triggerCollider.material = null;

                    questTriggerObject.name = $"_QuestTrigger({questTrigger._scriptQuest._questName}, {questTrigger._questTriggerTag})";

                    SceneManager.MoveGameObjectToScene(questTriggerObject, __instance._loadedScene);
                    NetworkServer.Spawn(questTriggerObject, (NetworkConnection)null);
                    Plugin.Logger.LogMessage("Quest Trigger created!");
                }
            }
        }
    }
}
