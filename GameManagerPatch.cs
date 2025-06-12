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
            Plugin.Logger.LogInfo($"Cached Sprite for {array[i].name}");
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

[HarmonyPatch(typeof(DialogTrigger), "BeginDialog")]
public static class DialogTriggerPatch
{
    [HarmonyPrefix]
    private static void BeginDialogPatch(ref ScriptableDialogData _dialogData)
    {
        if (Plugin.questGiver != null && Plugin.questGiver.ContainsKey(_dialogData._nameTag))
        {
            _dialogData._scriptableQuests ??= [];
            foreach (string questName in Plugin.questGiver[_dialogData._nameTag])
            {
                if (!_dialogData._scriptableQuests.Contains(Plugin.gameManager._cachedScriptableQuests[questName]))
                {
                    _dialogData._scriptableQuests = _dialogData._scriptableQuests.Append(Plugin.gameManager._cachedScriptableQuests[questName]).ToArray();
                    Plugin.Logger.LogInfo($"{questName}: Added ScriptableQuest {_dialogData._scriptableQuests.Last()._questName} to {_dialogData._nameTag}'s ScriptableDialogData");
                }
            }
        }

        if (_dialogData._scriptableQuests.Length > 0)
        {
            bool hasQuestDialog = false;
            foreach (DialogBranch d in _dialogData._dialogBranches)
            {
                if (d.dialogs.First()._dialogUI == DialogUIPrompt.QUEST)
                {
                    hasQuestDialog = true;
                    break;
                }
            }
            if (!hasQuestDialog)
            {
                Dialog[] firstBranchDialogs = _dialogData._dialogBranches.First().dialogs;
                _dialogData._dialogBranches.First().dialogs.Last()._dialogSelections = 
                    _dialogData._dialogBranches.First().dialogs.Last()._dialogSelections.Append(new()
                    {
                        _selectionCaption = "Got Quests?",
                        _selectionIcon = Plugin._cachedSprite["_lexiconIco02"],
                        _setDialogIndex = _dialogData._dialogBranches.Length
                    }).ToArray();

                _dialogData._dialogBranches = _dialogData._dialogBranches.Append(new()
                {
                    dialogs = [new Dialog()
                {
                    _dialogKey = _dialogData._nameTag + "Quest",
                    _dialogUI = DialogUIPrompt.QUEST,
                    _altInputs = ["Here are my quests."],
                    _dialogSelections = [new()
                    {
                        _selectionCaption = "See ya!",
                        _setDialogIndex = -1
                    }, new()
                    {
                        _selectionCaption = "Something else...",
                        _setDialogIndex = 0
                    }]
                }]}).ToArray();

                _dialogData._questAcceptResponses = ["Quest Accepted."];
                _dialogData._questCompleteResponses = ["Quest Complete!"];
            }
        }
        /*
        Plugin.Logger.LogInfo($"Hopefully forced the updated ScriptableDialogData onto {_dialogData._nameTag}");

        foreach(var (questName, quest) in Patch._cachedScriptableQuests)
        {
            Plugin.Logger.LogInfo($"Quest '{questName}' still exists");
        }

        foreach (var quest in _dialogData._scriptableQuests)
        {
            Plugin.Logger.LogInfo($"Quest '{quest._questName}' still exists for {_dialogData._nameTag}");
        }
        */
    }
}