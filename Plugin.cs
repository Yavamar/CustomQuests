using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
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
            CheckDirectory(Paths.PluginPath);
        }

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(Assembly.GetExecutingAssembly());

    }
    public static void CheckDirectory(string dirPath)
    {
        if(dirPath.ToUpper().EndsWith("QUEST"))
        {
            string[] files = Directory.GetFiles(dirPath);
            foreach (string filePath in files)
            {
                CheckFile(filePath);
            }
        }
        string[] directories = Directory.GetDirectories(dirPath);
        string[] array2 = directories;
        foreach (string dirPath2 in array2)
        {
            CheckDirectory(dirPath2);
        }
    }
    public static void CheckFile(string filePath)
    {
        if (filePath.ToUpper().EndsWith(".JSON"))
        {
            Logger.LogInfo("Adding to list of quests to be added");
            jsonFilePaths.Add(filePath);
            Logger.LogInfo("Successfully loaded JSON Quest file!");
        }
    }
}

