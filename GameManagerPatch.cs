using System;
using System.IO;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;


namespace CustomQuest;

[HarmonyPatch(typeof(GameManager), "Cache_ScriptableAssets")]
public static class GameManagerPatch
{
    // This function executes as soon as the game is launched.
    [HarmonyPostfix]
    private static void Cache_ScriptableAssetsPatch()
    {
        // Grab the GameManager and save it for use elsewhere.
        Plugin.gameManager = GameManager._current;

        // Create a cached dictionary of sprite names and their related sprites so we can find them later.
        Sprite[] array = Resources.LoadAll<Sprite>("_GRAPHIC/_UI/");

        for (int i = 0; i < array.Length; i++)
        {
            Plugin._cachedSprite.Add(array[i].name, array[i]);
        }

        // Parse all of the JSON quest files.
        foreach(string filePath in Plugin.jsonFilePaths)
        {
            ParseJsonQuest(filePath);
        }

        // Clear the list of JSON files after they've been loaded. We don't need them any more and we don't want to accidentally parse them again.
        Plugin.jsonFilePaths.Clear();
    }

    //Add the quest to the dictionary early, then set up the inter-dependencies in PlayerQuetingPatch.cs later after all quests, creeps, items, etc. are loaded.
    public static void ParseJsonQuest(string filePath)
    {
        // Parse the JSON file into an object.
        try
        {
            ParsedQuest parsedQuest = new JsonSerializer().Deserialize<ParsedQuest>(new JsonTextReader(new StreamReader(filePath)));

            ScriptableQuest quest = ScriptableObject.CreateInstance<ScriptableQuest>();

            // If they have the same data type, we can just copy them over.
            quest._questName = parsedQuest._questName;
            quest._questDescription = parsedQuest._questDescription;
            quest._questCompleteReturnMessage = parsedQuest._questCompleteReturnMessage;
            quest._questLevel = parsedQuest._questLevel;
            quest._requireNoviceClass = parsedQuest._requireNoviceClass;
            quest._autoFinishQuest = parsedQuest._autoFinishQuest;
            quest._scenePath = parsedQuest._scenePath;
            if (parsedQuest._questExperienceReward != 0)
            {
                // Attempt to deal with precision loss.
                quest._questExperiencePercentage = (float)((parsedQuest._questExperienceReward) / GameManager._current._statLogics._experienceCurve.Evaluate(quest._questLevel));
                int actualXp = (int)((float)(int)GameManager._current._statLogics._experienceCurve.Evaluate(quest._questLevel) * quest._questExperiencePercentage);
                int difference = parsedQuest._questExperienceReward - actualXp;
                quest._questExperiencePercentage = (float)((parsedQuest._questExperienceReward + difference) / GameManager._current._statLogics._experienceCurve.Evaluate(quest._questLevel));
            }
            else
            {
                quest._questExperiencePercentage = parsedQuest._questExperiencePercentage;
            }
            quest._questCurrencyReward = parsedQuest._questCurrencyReward;

            // Initialize all of the arrays.
            quest._preQuestRequirements = [];
            quest._questObjectiveItem = new();
            quest._questItemRewards = [];
            quest._questObjective = new()
            {
                _questCreepRequirements = [],
                _questItemRequirements = [],
                _questTriggerRequirements = []
            };

            // Add quest to the cache.
            Plugin.gameManager._cachedScriptableQuests.Add(quest._questName, quest);
            Plugin.Logger.LogInfo(quest._questName + ": Cached.");

            // Save the rest of the quest setup for later.
            Plugin.parsedQuests.Add(parsedQuest);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Error while loading {filePath} - Quest NOT cached!");
            Plugin.Logger.LogError(e);
        }
    }
}

