using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace CustomQuest;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    public static GameManager gameManager;
    public static Dictionary<string, Sprite> _cachedSprite = [];

    public static List<string> jsonFilePaths = [];
    public static List<ParsedQuest> parsedQuests = [];
    public static Dictionary<string, List<string>> questGiver = [];
    public static List<ParsedQuestTrigger> parsedQuestTriggers = [];

    private void Awake()
    {
        Logger = base.Logger;
        // Plugin startup logic\
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        if (Directory.Exists(Paths.PluginPath))
        {
            FindCustomQuestsFolder(Paths.PluginPath);
        }

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(Assembly.GetExecutingAssembly());

    }
    public static void FindCustomQuestsFolder(string dirPath)
    {
        if(dirPath.ToUpper().EndsWith("CUSTOMQUESTS-FILES"))
        {
            CheckFiles(dirPath);
        }
        string[] directories = Directory.GetDirectories(dirPath);
        string[] array2 = directories;
        foreach (string dirPath2 in array2)
        {
            FindCustomQuestsFolder(dirPath2);
        }
    }
    public static void CheckFiles(string path)
    {
        foreach (string filePath in Directory.GetFiles(path))
        {
            if (filePath.ToUpper().EndsWith(".JSON"))
            {
                Logger.LogInfo($"Adding {filePath} to list of quest files to be read.");
                jsonFilePaths.Add(filePath);
            }
        }
        foreach (string dirPath in Directory.GetDirectories(path))
        { 
        CheckFiles(dirPath);
        }

    }
}

