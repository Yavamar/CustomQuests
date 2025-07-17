using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mirror;

namespace CustomQuest
{
    [HarmonyPatch]
    public static class PlayerQuestingPatch
    {
        // This function executes as soon as the game loads the player's quest progress, which should be as soon as your character loads into the world.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerQuesting), "OnStartAuthority")]
        private static void LoadQuests()
        {
            if (Plugin.parsedQuests != null) // If this is null, either all quests have already been loaded or there were no quests to load.
            {
                foreach (ParsedQuest parsedQuest in Plugin.parsedQuests)
                {
                    // Save a dictionary of quest givers with a list of quest names for later. Those will be handled in DialogTriggerPatch.cs
                    if (Plugin.questGiver.ContainsKey(parsedQuest.questGiver))
                    {
                        Plugin.questGiver[parsedQuest.questGiver].Add(parsedQuest._questName);
                    }
                    else
                    {
                        Plugin.questGiver.Add(parsedQuest.questGiver, [parsedQuest._questName]);
                    }

                    // Load the rest of the quest.
                    try
                    {
                        LoadQuestDetails(parsedQuest);
                    }
                    catch (Exception e)
                    {
                        Plugin.Logger.LogError($"Error while loading quest \"{parsedQuest._questName}\" - Quest NOT loaded!");
                        Plugin.Logger.LogError(e);

                        // Unload the quest that threw the exception.
                        Plugin.questGiver[parsedQuest.questGiver].Remove(parsedQuest._questName);
                        Plugin.gameManager._cachedScriptableQuests.Remove(parsedQuest._questName);
                    }
                }

                // Clear the parsedQuests list once the quests have all been loaded. This will prevent the quests from accidentally being loaded again.
                Plugin.parsedQuests.Clear();
            }
        }

        public static void LoadQuestDetails(ParsedQuest parsedQuest)
        {
            GameManager gameManager = Plugin.gameManager;

            gameManager._cachedScriptableQuests.TryGetValue(parsedQuest._questName, out ScriptableQuest quest);

            // Create Dictionary<string,int> for ScriptableStatModifiers
            Dictionary<string, ScriptableStatModifier> scriptableStatModifierNames = new();
            foreach ((int _, ScriptableStatModifier a) in gameManager._cachedScriptableStatModifiers)
            {
                scriptableStatModifierNames.Add(a._modifierTag, a);
            }



            // Quest Type
            Plugin.Logger.LogInfo(quest._questName + ": Setting Quest Type.");
            if (!Enum.TryParse(parsedQuest._questType, out quest._questType))
            {
                Plugin.Logger.LogWarning($"_questType: {parsedQuest._questType} is not valid quest type.");
            }



            // Quest Sub-Type
            Plugin.Logger.LogInfo(quest._questName + ": Setting Quest SubType.");
            if (!Enum.TryParse(parsedQuest._questSubType, out quest._questSubType))
            {
                Plugin.Logger.LogWarning($"_questSubType: {parsedQuest._questSubType} is not a valid quest subtype.");
            }



            // Quest Icon
            if (parsedQuest._questIco != null)
            {
                Plugin.Logger.LogInfo(quest._questName + ": Setting Quest Icon.");
                if (!Plugin._cachedSprite.TryGetValue(parsedQuest._questIco, out quest._questIco))
                {
                    Plugin.Logger.LogWarning($"Sprite \"{parsedQuest._questIco}\" not found!");
                }
            }



            // Skill to Hide
            if (parsedQuest._skillToHide != null)
            {
                Plugin.Logger.LogInfo(quest._questName + ": Setting Skill To Hide.");
                if (!gameManager._cachedScriptableSkills.TryGetValue(parsedQuest._skillToHide, out quest._skillToHide))
                {
                    Plugin.Logger.LogWarning($"Skill \"{parsedQuest._skillToHide}\" not found! Defaulting to not hiding any skills.");
                }
            }



            // Race Requirement
            if (parsedQuest._raceRequirement != null)
            {
                Plugin.Logger.LogInfo(quest._questName + ": Setting Race Requirement.");
                if (!gameManager._cachedScriptableRaces.TryGetValue(parsedQuest._raceRequirement, out quest._raceRequirement))
                {
                    Plugin.Logger.LogWarning($"Race \"{parsedQuest._raceRequirement}\" not found! Defaulting to all races.");
                }
            }



            // Base Class Requirement
            if (parsedQuest._baseClassRequirement != null)
            {
                Plugin.Logger.LogInfo(quest._questName + ": Setting Base Class Requirement.");
                if (!gameManager._cachedScriptablePlayerClasses.TryGetValue(parsedQuest._baseClassRequirement, out quest._baseClassRequirement))
                {
                    Plugin.Logger.LogWarning($"Base Class \"{parsedQuest._baseClassRequirement}\" not found! Defaulting to all base classes.");
                }
            }



            // Pre-Quest Requirements
            if (parsedQuest._preQuestRequirements != null)
            {
                Plugin.Logger.LogInfo(quest._questName + ": Setting Pre-Quest Requirements.");

                List<ScriptableQuest> list = [];

                foreach (string questName in parsedQuest._preQuestRequirements)
                {
                    if (gameManager._cachedScriptableQuests.TryGetValue(questName, out ScriptableQuest q))
                    {
                        list.Add(q);
                    }
                    else
                    {
                        throw new Exception($"Quest \"{questName}\" not found!");
                    }
                }

                quest._preQuestRequirements = list.ToArray();
            }



            // Quest Creep Requirements
            if (parsedQuest._questCreepRequirements != null)
            {
                Plugin.Logger.LogInfo(quest._questName + ": Setting Quest Creep Requirements.");

                List<QuestCreepRequirement> list = [];

                foreach (var (creepName, amount) in parsedQuest._questCreepRequirements)
                {
                    if (amount > 0)
                    {
                        if (!gameManager._cachedScriptableCreeps.TryGetValue(creepName, out ScriptableCreep creep))
                        {
                            //Try partial creep name match.
                            IEnumerable<ScriptableCreep> partialMatches = DictionaryExt.PartialMatch(gameManager._cachedScriptableCreeps, creepName);
                            if(partialMatches.Count() > 0)
                            {
                                creep = partialMatches.First();
                            }
                            else
                            {
                                throw new Exception($"Creep \"{creepName}\" not found!");
                            }
                        }

                        list.Add(new()
                        {
                            _questCreep = creep,
                            _creepsKilled = amount
                        });
                    }
                    else 
                    {
                        Plugin.Logger.LogWarning($"Creep \"{creepName}\" is set to require 0 or fewer kills. Ignoring this requirement.");
                    }
                }

                quest._questObjective._questCreepRequirements = list.ToArray();
            }



            // Quest Item Requirements
            if (parsedQuest._questItemRequirements != null)
            {
                Plugin.Logger.LogInfo(quest._questName + ": Setting Quest Item Requirements.");

                List<QuestItemRequirement> list = [];

                foreach (var (itemName, amount) in parsedQuest._questItemRequirements)
                {
                    if (amount > 0)
                    {
                        if (!gameManager._cachedScriptableItems.TryGetValue(itemName, out ScriptableItem item))
                        {
                            //Try partial item name match for Homebrewery items.
                            IEnumerable<ScriptableItem> partialMatches = DictionaryExt.PartialMatch(gameManager._cachedScriptableItems, itemName);
                            if (partialMatches.Count() > 0)
                            {
                                item = partialMatches.First();
                            }
                            else
                            {
                                throw new Exception($"Item \"{itemName}\" not found!");
                            }
                        }

                        list.Add(new()
                        {
                            _questItem = item,
                            _itemsNeeded = amount
                        });
                    }
                    else
                    {
                        Plugin.Logger.LogWarning($"Item \"{itemName}\" is set to require 0 or fewer quantity. Ignoring this requirement.");
                    }
                }

                quest._questObjective._questItemRequirements = list.ToArray();
            }



            // Quest Trigger Requirements
            if (parsedQuest._questTriggerRequirements != null)
            {
                Plugin.Logger.LogInfo(quest._questName + ": Setting Quest Trigger Requirements.");
                quest._questObjective._questTriggerRequirements = parsedQuest._questTriggerRequirements;
            }



            // Quest Triggers
            if (parsedQuest.questTriggers != null)
            {
                foreach (ParsedQuestTrigger trigger in parsedQuest.questTriggers)
                {
                    trigger._scriptQuest = quest;
                    Plugin.parsedQuestTriggers.Add(trigger);
                }
            }



            // Quest Objective Item
            if (parsedQuest._questObjectiveItem != null)
            {
                Plugin.Logger.LogInfo(quest._questName + ": Setting Quest Objective Item.");
                if (parsedQuest._questObjectiveItem._itemQuantity > 0)
                {
                    QuestItemReward objectiveItem = new();

                    if (!gameManager._cachedScriptableItems.TryGetValue(parsedQuest._questObjectiveItem._scriptItem, out objectiveItem._scriptItem))
                    {
                        //Try partial item name match for Homebrewery items.
                        IEnumerable<ScriptableItem> partialMatches = DictionaryExt.PartialMatch(gameManager._cachedScriptableItems, parsedQuest._questObjectiveItem._scriptItem);
                        if (partialMatches.Count() > 0)
                        {
                            objectiveItem._scriptItem = partialMatches.First();
                        }
                        else
                        {
                            throw new Exception($"Item \"{parsedQuest._questObjectiveItem._scriptItem}\" not found!");
                        }
                    }

                    if (parsedQuest._questObjectiveItem._scriptableStatModifier != 0 && !gameManager._cachedScriptableStatModifiers.TryGetValue(parsedQuest._questObjectiveItem._scriptableStatModifier, out objectiveItem._scriptableStatModifier))
                    {
                        //Not Found
                        Plugin.Logger.LogWarning($"Modifier index {parsedQuest._questObjectiveItem._scriptableStatModifier} is not valid. Valid range is between 0 and {gameManager._cachedScriptableStatModifiers.Count - 1}");
                    }

                    if (parsedQuest._questObjectiveItem.scriptableStatModifierName != null && !scriptableStatModifierNames.TryGetValue(parsedQuest._questObjectiveItem.scriptableStatModifierName, out objectiveItem._scriptableStatModifier))
                    {
                        //Not Found
                        throw new Exception($"Modifier \"{parsedQuest._questObjectiveItem.scriptableStatModifierName}\" not found!");
                    }

                    objectiveItem._itemQuantity = parsedQuest._questObjectiveItem._itemQuantity;

                    quest._questObjectiveItem = objectiveItem;
                }
                else
                {
                    Plugin.Logger.LogWarning($"Item \"{parsedQuest._questObjectiveItem._scriptItem}\" is set to give 0 or fewer quantity. This item will not be provided.");
                }
            }



            // Quest Item Rewards
            if (parsedQuest._questItemRewards != null)
            {
                Plugin.Logger.LogInfo(quest._questName + ": Setting Quest Item Rewards.");

                List<QuestItemReward> list = [];

                foreach (ParsedQuestItemReward parsedReward in parsedQuest._questItemRewards)
                {
                    if (parsedReward._itemQuantity > 0)
                    {
                        QuestItemReward reward = new();

                        if (!gameManager._cachedScriptableItems.TryGetValue(parsedReward._scriptItem, out reward._scriptItem))
                        {
                            //Try partial item name match for Homebrewery items.
                            IEnumerable<ScriptableItem> partialMatches = DictionaryExt.PartialMatch(gameManager._cachedScriptableItems, parsedReward._scriptItem);
                            if (partialMatches.Count() > 0)
                            {
                                reward._scriptItem = partialMatches.First();
                            }
                            else
                            {
                                throw new Exception($"Item \"{parsedReward._scriptItem}\" not found!");
                            }
                        }

                        if (parsedReward._scriptableStatModifier != 0 && !gameManager._cachedScriptableStatModifiers.TryGetValue(parsedReward._scriptableStatModifier, out reward._scriptableStatModifier))
                        {
                            //Not Found
                            Plugin.Logger.LogWarning($"Modifier index {parsedReward._scriptableStatModifier} is not valid. Valid range is between 0 and {gameManager._cachedScriptableStatModifiers.Count-1}");
                        }

                        if (parsedReward.scriptableStatModifierName != null && !scriptableStatModifierNames.TryGetValue(parsedReward.scriptableStatModifierName, out reward._scriptableStatModifier))
                        {
                            //Not Found
                            Plugin.Logger.LogWarning($"Modifier \"{parsedReward.scriptableStatModifierName}\" not found! No modifier will be applied to the item.");
                        }

                        reward._itemQuantity = parsedReward._itemQuantity;

                        list.Add(reward);
                    }
                    else
                    {
                        Plugin.Logger.LogWarning($"Item \"{parsedReward._scriptItem}\" is set to give 0 or fewer quantity. Ignoring this reward.");
                    }
                }
                quest._questItemRewards = list.ToArray();
            }
        }

        // Breaking the netcode so I can turn in quests online.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerQuesting), "Client_CompleteQuest")]
        private static bool Client_CompleteQuest(PlayerQuesting __instance, int _index)
        {
            if (!NetworkClient.active)
            {
                return false;
            }

            if (__instance._questProgressData.Count <= 0 || !__instance._questProgressData[_index]._questComplete)
            {
                return false;
            }
            ScriptableQuest scriptableQuest = GameManager._current.Locate_Quest(__instance._questProgressData[_index]._questTag);
            if ((bool)scriptableQuest)
            {
                int expGain = (int)((float)(int)GameManager._current._statLogics._experienceCurve.Evaluate(scriptableQuest._questLevel) * scriptableQuest._questExperiencePercentage);
                
                //__instance._pStats.GainExp(expGain, __instance._pStats._currentLevel);
                if (expGain > 0)
                {
                    string message = $"Gained experience. (+{expGain})";
                    __instance._pStats._chatBehaviour.Init_GameLogicMessage(message);
                    __instance._pStats.UserCode_Target_DisplayExpFloatText__Int32(expGain);
                    __instance._pStats.Network_currentExp = __instance._pStats._currentExp + expGain;
                }

                //__instance._pInventory.Add_Currency(scriptableQuest._questCurrencyReward);
                if (scriptableQuest._questCurrencyReward > 0 && __instance._pInventory._heldCurrency < GameManager._current._statLogics._maxCurrency)
                {
                    int num = __instance._pInventory._heldCurrency + scriptableQuest._questCurrencyReward;
                    if (num >= GameManager._current._statLogics._maxCurrency)
                    {
                        num = GameManager._current._statLogics._maxCurrency;
                    }

                    string message = $"Picked up {GameManager._current._statLogics._currencyName}. (+{scriptableQuest._questCurrencyReward})";
                    __instance._pInventory._chatBehaviour.Init_GameLogicMessage(message);
                    __instance._pInventory.Network_heldCurrency = num;
                }
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerQuesting), "UserCode_Cmd_InitServersideQuestRewards__String")]
        private static bool UserCode_Cmd_InitServersideQuestRewards__String()
        {
            return false;
        }
    }
}
