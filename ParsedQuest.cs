using System.Collections.Generic;

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
        public float _questExperiencePercentage { get; set; }
        public int _questCurrencyReward { get; set; }
        public Dictionary<string, int> _questItemRewards { get; set; }
        public bool _displayEndDemoPrompt { get; set; }
    }
}
