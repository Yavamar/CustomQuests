using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace CustomQuest
{
    [HarmonyPatch(typeof(DialogTrigger), "BeginDialog")]
    public static class DialogTriggerPatch
    {
        [HarmonyPrefix]
        private static void BeginDialogPatch(ref ScriptableDialogData _dialogData)
        {
            // If any new quests have this NPC as their questgiver...
            if (Plugin.questGiver != null && Plugin.questGiver.ContainsKey(_dialogData._nameTag))
            {
                // Initialize their quest list if they don't already have one.
                _dialogData._scriptableQuests ??= [];

                // Find all of the new quests for this NPC and add them to the NPC's quest list.
                List<ScriptableQuest> list = _dialogData._scriptableQuests.ToList();
                foreach (string questName in Plugin.questGiver[_dialogData._nameTag])
                {
                    if (!_dialogData._scriptableQuests.Contains(Plugin.gameManager._cachedScriptableQuests[questName]))
                    {
                        list.Add(Plugin.gameManager._cachedScriptableQuests[questName]);

                        Plugin.Logger.LogInfo($"{questName}: Added to {_dialogData._nameTag}'s ScriptableDialogData");
                    }
                }
                _dialogData._scriptableQuests = list.ToArray();
            }

            // If this NPC has quests...
            if (_dialogData._scriptableQuests != null && _dialogData._scriptableQuests.Length > 0)
            {
                // Find out if they have a quest menu.
                bool hasQuestDialog = false;
                foreach (DialogBranch d in _dialogData._dialogBranches)
                {
                    if (d.dialogs.First()._dialogUI == DialogUIPrompt.QUEST)
                    {
                        hasQuestDialog = true;
                        break;
                    }
                }

                // If they don't have a quest menu...
                if (!hasQuestDialog)
                {
                    // First add a button to access the quest menu.
                    _dialogData._dialogBranches.First().dialogs.Last()._dialogSelections =                // This is messy, but we want to add our button to the first branch's
                        _dialogData._dialogBranches.First().dialogs.Last()._dialogSelections.Append(new() // last dialog. This is the "main menu" of the NPC's dialog tree.
                        {
                            _selectionCaption = "Got Quests?",
                            _selectionIcon = Plugin._cachedSprite["_lexiconIco02"],
                            _setDialogIndex = _dialogData._dialogBranches.Length // This specifies the DialogBranch index to go to when we press the button.
                                                                                 // The quest menu we add next will be the last DialogBranch on the list.
                        }).ToArray();

                    // Then add the quest menu.
                    _dialogData._dialogBranches = _dialogData._dialogBranches.Append(new()
                    {
                        dialogs = [new Dialog()
                        {
                            _dialogKey = _dialogData._nameTag + "Quest",
                            _dialogUI = DialogUIPrompt.QUEST, // This is what makes it a quest menu.
                            _altInputs = ["Here are my quests."],
                            _dialogSelections = [new() // Add the two standard buttons every quest menu has.
                            {
                                _selectionCaption = "See ya!",
                                _setDialogIndex = -1 // This is a special index that exit's the NPC's dialog.
                            }, new()
                            {
                                _selectionCaption = "Something else...",
                                _setDialogIndex = 0 // Return to the NPC's "main menu"
                            }]
                        }]
                    }).ToArray();

                    // Finally add messages that appear when accepting and completing quests.
                    _dialogData._questAcceptResponses = ["Quest Accepted."];
                    _dialogData._questCompleteResponses = ["Quest Complete!"];
                }
            }
        }
    }
}
