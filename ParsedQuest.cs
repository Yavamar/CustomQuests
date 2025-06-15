using System.Collections.Generic;
using UnityEngine;

namespace CustomQuest
{
    public class ParsedQuest
    {
        public string questGiver { get; set; } // NPC who gives you the quest.
        public string _questName { get; set; } // The name of the quest.
        public string _questType { get; set; } // Currently can only be "SINGLE" or "REPEATABLE"
        public string _questSubType { get; set; } // "NONE", "MAIN_QUEST", "CLASS", or "MASTERY"
        public string _questIco { get; set; } // Sprite name of the icon that appears next to the quest.
        public string _questDescription { get; set; } // Quest description. You can use color tags here.
        public string _questCompleteReturnMessage { get; set; } // Message that appears in the quest log once all objectives are complete. Usually tells the player which NPC to return to and where they are.
        public int _questLevel { get; set; } // Minimum level to accept the quest.
        public string _raceRequirement { get; set; } // Lets you restrict the quest to a single race
        public string _baseClassRequirement { get; set; } // Lets you restrict the quest to a single class
        public string _skillToHide { get; set; } // ???
        public bool _requireNoviceClass { get; set; } // Only appears for Novice class. The quests for choosing your class at Lv10 use this.
        public bool _autoFinishQuest { get; set; } // Quest auto-completes once objectives are met. The class choice and weapon mastery quests use this.
        public string[] _preQuestRequirements { get; set; } // Pre-requisite quests. Quests listed here must be completed before you can see this quest.
        public Dictionary<string, int> _questObjectiveItem { get; set; } // Item given to you at the start of the quest. This is the skillbook given to you for class and mastery quests.
        public string _scenePath { get; set; } // ???
        public Dictionary<string, int> _questItemRequirements { get; set; } // Items you are required to have in order to complete the quest.
        public Dictionary<string, int> _questCreepRequirements { get; set; } // Enemies you are required to kill in order to complete the quest.
        public QuestTriggerRequirement[] _questTriggerRequirements { get; set; } // Quest triggers that must be activated in order to complete the quest. You will need to create new quest triggers for this.
            // string _questTriggerTag: Essentially the name of the required quest trigger.
            // string _prefix: First part of requirement that appears in your quest log.
            // string _suffix: Second part of the requirement that appears in your quest log.
            // int _triggerEmitsNeeded: Number of times the trigger must be activated.
        public List<ParsedQuestTrigger> questTriggers { get; set; } // See the ParsedQuestTrigger class below.
        public float _questExperiencePercentage { get; set; } // XP you recieve upon completing the quest. I'm not entirely sure how it is calculated.
        public int _questCurrencyReward { get; set; } // The number of Crowns you receive upon completing the quest
        public Dictionary<string, int> _questItemRewards { get; set; } // Items you receive upon completing the quest
        public bool _displayEndDemoPrompt { get; set; } // ???
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
        public ScriptableQuest _scriptQuest { get; set; } // The quest that the trigger applies to. You do not need to manually set this.

    }
    public class ParsedCollider
    {
        public string type { get; set; } // Shape of the collider. Currently supports "Sphere", "Box", and "Capsule"
        public Vector3 center { get; set; } // The center of the collider relative to the quest trigger GameObject. You should probably leave it as {"x": 0, "y": 0, "z": 0}
        public float radius { get; set; } // Used by Sphere and Capsule colliders. Bigger radius = bigger sphere.
        public Vector3 size { get; set; } // Used by Box colliders. Sets the length, height, and width of the box using {"x": 123, "y": 456, "z": 789}
        public float height { get; set; } // Used by Capsule colliders.
        public int direction { get; set; } // Used by Capsule colliders.

    }
}
