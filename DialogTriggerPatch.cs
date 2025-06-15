using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;

namespace CustomQuest
{
    [HarmonyPatch(typeof(DialogTrigger), "BeginDialog")]
    public static class DialogTriggerPatch
    {
        [HarmonyPrefix]
        private static void BeginDialogPatch(ref ScriptableDialogData _dialogData)
        {
            if (Plugin.questGiver != null && Plugin.questGiver.ContainsKey(_dialogData._nameTag))
            {
                _dialogData._scriptableQuests ??= [];
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

            if (_dialogData._scriptableQuests != null && _dialogData._scriptableQuests.Length > 0)
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
                        }]
                    }).ToArray();

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
}
