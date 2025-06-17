using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CustomQuest
{
    /// <summary>
    /// When a quest file is read, it will first be put into a ParsedQuest object. The JSON file containing the quest can use any of the fields listed below.
    /// 
    /// For anyone reading this who isn't familar with code or variable types, I'll give a brief explanation:
    /// string = Text. You can put normal words in any field labeled "string".
    /// int = Integer. Whole numbers, no decimals.
    /// float = Floating Point. These are numbers that can contain decimals.
    /// bool = Boolean. The only valid values are true (On, Yes) or false (Off, No).
    /// Dictionary<string, int> = A series of key:value pairs where no two keys can be the same.
    /// 
    /// Any type that ends with brackets [] is an array, which means it can contain multiple values of the same type. In the JSON file, this will be represented by a comma-separated list between brackets [].
    /// 
    /// More complex types such as QuestTriggerRequirement or ParsedQuestTrigger can be broken down into simpler types. Explanations for the fields contained within, and their types, are below.
    /// </summary>
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
        public string _raceRequirement { get; set; } // "IMP", "POON", "KUBOLD", "BYRDLE", "CHANG". Only characters of the specified race can do this quest. Leave this field out to allow all races.
        public string _baseClassRequirement { get; set; } // "FIGHTER", "BANDIT", "MYSTIC". Only characters of the specified class can do this quest. Leave this field out to allow all classes.
        public string _skillToHide { get; set; } // ???
        public bool _requireNoviceClass { get; set; } // Only appears for Novice class. The quests for choosing your class at Lv10 use this.
        public bool _autoFinishQuest { get; set; } // Quest auto-completes once objectives are met. The class choice and weapon mastery quests use this.
        public string[] _preQuestRequirements { get; set; } // Pre-requisite quests. Quests listed here must be completed before you can see this quest.
        public Dictionary<string, int> _questObjectiveItem { get; set; } // Item given to you at the start of the quest. This is the skillbook given to you for class and mastery quests.
        public string _scenePath { get; set; } // ???
        public Dictionary<string, int> _questItemRequirements { get; set; } // Items (and the amount of each item) you are required to have in order to complete the quest.
        public Dictionary<string, int> _questCreepRequirements { get; set; } // Enemies (and the amount of each enemy) you are required to kill in order to complete the quest.
        public QuestTriggerRequirement[] _questTriggerRequirements { get; set; } // Quest triggers are special conditions that must be met in order to complete the quest. You must create quest triggers specifically for this quest. (see questTriggers field)
            // string _questTriggerTag: Essentially the name of the required quest trigger.
            // string _prefix: First part of requirement that appears in your quest log.
            // string _suffix: Second part of the requirement that appears in your quest log.
            // int _triggerEmitsNeeded: Number of times the trigger must be activated.
        public List<ParsedQuestTrigger> questTriggers { get; set; } // Creates new quest triggers for this quest. See the ParsedQuestTrigger class below.
        public float _questExperiencePercentage { get; set; } // XP you recieve upon completing the quest, which is then multiplied by the XP gain curve to determine the actual amount of XP rewarded.
        public int _questExperienceReward { get; set; } // XP you recieve upon completing the quest. This will override _questExperiencePercentage if provided. Might be slightly inaccurate due to type conversions.
        public int _questCurrencyReward { get; set; } // The number of Crowns you receive upon completing the quest.
        public Dictionary<string, int> _questItemRewards { get; set; } // Items (and the amount of each item) you receive upon completing the quest.
        public bool _displayEndDemoPrompt { get; set; } // ???

        // I spent a lot of time writing this function and now I'm not even sure if I need it. Commenting for now just in case I change my mind.
        /*
        public object GetObjectFromCache(string field, Dictionary<string, object> cache, string log)
        {
            FieldInfo field2 = typeof(ParsedQuest).GetField(field);
            if (field2 != null)
            {
                if (cache.TryGetValue(field2.GetValue(this).ToString(), out object outValue))
                {
                    return outValue;
                }
                else
                {
                    Plugin.Logger.LogWarning($"{field2.GetType()} {field2.GetValue(this)} not found!");
                }
            }
            return null;
        }
        */
    }

    /// <summary>
    /// Quest Triggers come in many different ways. However currently CustomQuests only supports map-based quest triggers.
    /// 
    /// This will allow you to create quest triggers that require exploring a specific area (the first tutorial quest, dungeon sweep quests) or interacting with a specific object (attuning sigils)
    /// </summary>
    public class ParsedQuestTrigger
    {
        public string _questTriggerTag { get; set; } // Essentially the name of the quest trigger. It must match a _questTriggerTag from a quest's _questTriggerRequirements
        public string _mapInstance { get; set; } // The map you want the trigger to appear in.
        public Vector3 position { get; set; } // The 3D coordinates of the trigger. {"x": 123, "y": 456, "z": 789}
        public string _difficultyRequirement { get; set; } // Dungeon difficulty requirement. Irrelevant for open-world quest triggers. "EASY", "NORMAL", or "HARD"
        public bool _interactTrigger { get; set; } // If true, the player must press the Interact key to activate the trigger. If false, they only have to get close to it.
        public bool _completedRequirements { get; set; } // ??? (This seems to be true in every quest I've seen)
        public bool _questTriggerOnlyOnce { get; set; } // ??? (I haven't encountered any quest triggers that require multiple activations)
        public NetTrigger _netTriggerToInvoke { get; set; } // Not yet implemented in CustomQuests. Used to open the door to Crescent Grove.
        public string _netTriggerType { get; set; } // Determines when the NetTrigger is enabled.
        public TriggerMessage _triggerMessage { get; set; } // Messages that appear when the quest trigger is activated.
            // string _singleMessage: Message that appears when the quest trigger is successfully activated
            // string _incompleteMessage: Message that appears when the quest trigger is activated, but the objective hasn't been met (used for dungeon sweeps where enemies are still alive).
            // string[] _triggerMessageArray: ??? (I'm assuming it lets you set different messages for multiple trigger activations)
            // bool _sentMessage: ??? (Always seems to be false)
        public bool _arenaSweepQuest { get; set; } // All enemies in the dungeon must be killed to activate the quest trigger.
        public ParsedCollider _triggerCollider { get; set; } // An object representing the area where the trigger activates. See ParsedCollider class below.

        //This will be set by the code, not by the JSON
        public ScriptableQuest _scriptQuest { get; set; } // The quest that the trigger applies to. You do not need to manually set this.
    }

    /// <summary>
    /// The collider is the part of the quest trigger that determines if a player is touching it. Think of it like a hitbox for the trigger.
    /// </summary>
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
