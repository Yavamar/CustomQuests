using System.IO;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;


namespace CustomQuest;

[HarmonyPatch(typeof(GameManager), "Cache_ScriptableAssets")]
public static class GameManagerPatch
{
    [HarmonyPostfix]
    private static void Cache_ScriptableAssetsPatch()
    {
        Plugin.gameManager = GameManager._current;

        Sprite[] array = Resources.LoadAll<Sprite>("_GRAPHIC/_UI/");

        for (int i = 0; i < array.Length; i++)
        {
            Plugin._cachedSprite.Add(array[i].name, array[i]);
            //Plugin.Logger.LogInfo($"Cached Sprite for {array[i].name}");
        }
        foreach(string filePath in Plugin.jsonFilePaths)
        {
            ParseJsonQuest(filePath);
        }
        Plugin.jsonFilePaths.Clear();
    }

    //Add the quest to the dictionary early, then set up the inter-dependencies in PlayerQuetingPatch.cs later after all quests, creeps, items, etc. are loaded.
    public static void ParseJsonQuest(string filePath)
    {
        // Parse the JSON file into an object
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
        quest._questExperiencePercentage = parsedQuest._questExperiencePercentage;
        quest._questCurrencyReward = parsedQuest._questCurrencyReward;
        quest._displayEndDemoPrompt = parsedQuest._displayEndDemoPrompt;

        // Initialize all of the arrays
        quest._preQuestRequirements = [];
        quest._questObjectiveItem = new();
        quest._questItemRewards = [];
        quest._questObjective = new() //parsedQuest._questObjective;
        {
            _questCreepRequirements = [],
            _questItemRequirements = [],
            _questTriggerRequirements = []
        };

        // Add quest to the cache
        Plugin.gameManager._cachedScriptableQuests.Add(quest._questName, quest);
        Plugin.Logger.LogInfo(quest._questName + ": Cached.");

        // Save the rest of the quest setup for later
        Plugin.parsedQuests.Add(parsedQuest);
    }
}

