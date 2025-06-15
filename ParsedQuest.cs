using System.Collections.Generic;
using UnityEngine;

namespace CustomQuest
{
    public class ParsedQuest
    {
        public string questGiver { get; set; }
        public string _questName { get; set; }
        public string _questType { get; set; }
        public string _questSubType { get; set; }
        public string _questIco { get; set; }
        public string _questDescription { get; set; }
        public string _questCompleteReturnMessage { get; set; }
        public int _questLevel { get; set; }
        public string _raceRequirement { get; set; }
        public string _baseClassRequirement { get; set; }
        public string _skillToHide { get; set; }
        public bool _requireNoviceClass { get; set; }
        public bool _autoFinishQuest { get; set; }
        public string[] _preQuestRequirements { get; set; }
        public Dictionary<string, int> _questObjectiveItem { get; set; } // This is the skillbook given to you for class and mastery quests.
        public string _scenePath { get; set; }
        public Dictionary<string, int> _questItemRequirements { get; set; }
        public Dictionary<string, int> _questCreepRequirements { get; set; }
        public QuestTriggerRequirement[] _questTriggerRequirements { get; set; }
        //public QuestObjective _questObjective { get; set; }
        public List<ParsedQuestTrigger> questTriggers { get; set; }
        public float _questExperiencePercentage { get; set; }
        public int _questCurrencyReward { get; set; }
        public Dictionary<string, int> _questItemRewards { get; set; }
        public bool _displayEndDemoPrompt { get; set; }
    }

    public class ParsedQuestTrigger
    {
        public string _questTriggerTag { get; set; }
        public string _mapInstance { get; set; }
        public Vector3 position { get; set; }
        public string _difficultyRequirement { get; set; }
        public bool _interactTrigger { get; set; }
        public bool _completedRequirements { get; set; }
        public bool _questTriggerOnlyOnce { get; set; }
        public NetTrigger _netTriggerToInvoke { get; set; }
        public string _netTriggerType { get; set; }
        public TriggerMessage _triggerMessage { get; set; }
        public bool _arenaSweepQuest { get; set; }
        public ParsedCollider _triggerCollider { get; set; }

        //This will be set by the code, not by the JSON
        public ScriptableQuest _scriptQuest { get; set; }

    }
    public class ParsedCollider
    {
        public string type { get; set; }
        public Vector3 center { get; set; }
        public float radius { get; set; }
        public Vector3 size { get; set; }
        public float height { get; set; }
        public int direction { get; set; }

    }
}
