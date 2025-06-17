using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace CustomQuest
{
    [HarmonyPatch(typeof(PlayerQuesting), "Start")]
    public static class PlayerQuestingPatch
    {
        // This function executes as soon as the game loads the player's quest progress, which should be as soon as your character loads into the world.
        [HarmonyPrefix]
        private static void Prefix()
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
                    LoadQuestDetails(parsedQuest);
                }

                // Clear the parsedQuests list once the quests have all been loaded. This will prevent the quests from accidentally being loaded again.
                Plugin.parsedQuests.Clear();
            }
        }

        public static void LoadQuestDetails(ParsedQuest parsedQuest)
        {
            GameManager gameManager = Plugin.gameManager;

            gameManager._cachedScriptableQuests.TryGetValue(parsedQuest._questName, out ScriptableQuest quest);



            // Quest Type
            Plugin.Logger.LogInfo(quest._questName + ": Setting Quest Type.");
            if (!Enum.TryParse(parsedQuest._questType, out quest._questType))
            {
                Plugin.Logger.LogWarning($"_questType {parsedQuest._questType} is not valid.");
            }



            // Quest Sub-Type
            Plugin.Logger.LogInfo(quest._questName + ": Setting Quest SubType.");
            if (!Enum.TryParse(parsedQuest._questSubType, out quest._questSubType))
            {
                Plugin.Logger.LogWarning($"_questSubType {parsedQuest._questSubType} is not valid.");
            }



            // Quest Icon
            Plugin.Logger.LogInfo(quest._questName + ": Setting Quest Icon.");
            quest._questIco = Plugin._cachedSprite[parsedQuest._questIco];



            // Skill to Hide
            if (parsedQuest._skillToHide != null)
            {
                Plugin.Logger.LogInfo(quest._questName + ": Setting Skill To Hide.");
                if (!gameManager._cachedScriptableSkills.TryGetValue(parsedQuest._skillToHide, out quest._skillToHide))
                {
                    //quest._skillToHide = DictionaryExt.PartialMatch(gameManager._cachedScriptableSkills, parsedQuest._skillToHide).First();
                    Plugin.Logger.LogWarning(quest._questName + ": Skill " + parsedQuest._skillToHide + " not found!");
                }
            }



            // Race Requirement
            if (parsedQuest._raceRequirement != null)
            {
                Plugin.Logger.LogInfo(quest._questName + ": Setting Race Requirement.");
                if (!gameManager._cachedScriptableRaces.TryGetValue(parsedQuest._raceRequirement, out quest._raceRequirement))
                {
                    Plugin.Logger.LogWarning(quest._questName + ": Race " + parsedQuest._raceRequirement + " not found!");
                }
            }



            // Base Class Requirement
            if (parsedQuest._baseClassRequirement != null)
            {
                Plugin.Logger.LogInfo(quest._questName + ": Setting Base Class Requirement.");
                if (!gameManager._cachedScriptablePlayerClasses.TryGetValue(parsedQuest._baseClassRequirement, out quest._baseClassRequirement))
                {
                    Plugin.Logger.LogWarning(quest._questName + ": Class " + parsedQuest._baseClassRequirement + " not found!");
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
                        Plugin.Logger.LogWarning(quest._questName + ": Quest " + questName + " not found!");
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
                    if (!gameManager._cachedScriptableCreeps.TryGetValue(creepName, out ScriptableCreep creep))
                    {
                        //Try partial creep name match.
                        creep = DictionaryExt.PartialMatch(gameManager._cachedScriptableCreeps, creepName).First();
                    }

                    list.Add(new()
                    {
                        _questCreep = creep,
                        _creepsKilled = amount
                    });
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
                    if (!gameManager._cachedScriptableItems.TryGetValue(itemName, out ScriptableItem item))
                    {
                        //Try partial item name match for Homebrewery items.
                        item = DictionaryExt.PartialMatch(gameManager._cachedScriptableItems, itemName).First();
                    }

                    list.Add(new()
                    {
                        _questItem = item,
                        _itemsNeeded = amount
                    });
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

                foreach (var (itemName, amount) in parsedQuest._questObjectiveItem)
                {
                    if (gameManager._cachedScriptableItems.TryGetValue(itemName, out ScriptableItem item))
                    {
                        //Try partial item name match for Homebrewery items.
                        item = DictionaryExt.PartialMatch(gameManager._cachedScriptableItems, itemName).First();
                    }

                    quest._questObjectiveItem = new()
                    {
                        _scriptItem = item,
                        _setItemData = new()
                        {
                            _itemName = itemName,
                            _quantity = amount
                        }
                    };

                }
            }



            // Quest Item Rewards
            if (parsedQuest._questItemRewards != null)
            {
                Plugin.Logger.LogInfo(quest._questName + ": Setting Quest Item Rewards.");

                List<QuestItemReward> list = [];

                foreach (var (itemName, amount) in parsedQuest._questItemRewards)
                {
                    if (gameManager._cachedScriptableItems.TryGetValue(itemName, out ScriptableItem item))
                    {
                        //Try partial item name match for Homebrewery items.
                        item = DictionaryExt.PartialMatch(gameManager._cachedScriptableItems, itemName).First();
                    }

                    list.Add(new()
                    {
                        _scriptItem = item,
                        _setItemData = new()
                        {
                            _itemName = itemName,
                            _quantity = amount,
                            _maxQuantity = amount
                        }
                    });
                }
                quest._questItemRewards = list.ToArray();
            }
        }
    }
}
