using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Mirage;
using CommandMod.CommandHandler;

namespace WhisperCommand;

[BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
[BepInDependency("me.muj.commandmod")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
    }
}

static class PluginInfo
{
    public const string GUID = "net.downloadpizza.WhisperCommand";
    public const string NAME = "Whisper Command";
    public const string VERSION = "1.0.0";
}

public static class Commands
{
    [ConsoleCommand("whisper")]
    public static void Whisper(string[] args, CommandObjects co)
    {
        var targetName = args[0];
        var rest = string.Join(" ", args.Skip(1));
        var target = UnitRegistry.playerLookup.First(pl => MatchGlob(targetName, pl.Value.PlayerName));
        Wrapper.TargetReceiveMessage(target.Value.Owner, $"<whispers to you> {rest}", co.Player, true);
    }

    private static bool MatchGlob(string glob, string searchSpace)
    {
        // Escape special regex characters in glob
        string regexPattern = "^" + Regex.Escape(glob)
            .Replace(@"\*", ".*")
            .Replace(@"\?", ".") + "$";

        return Regex.IsMatch(searchSpace, regexPattern);
    }
}

public class Wrapper
{
    private static readonly FieldInfo _chatManager = AccessTools.Field(typeof(ChatManager), "i");
    public static ChatManager ChatManager
    {
        get => (ChatManager)_chatManager.GetValue(null);
        set
        {
            if (value != null)
                _chatManager.SetValue(null, value);
        }
    }
    
    private static FieldInfo _blockList = AccessTools.Field(typeof(BlockList), "playerId");
    private static MethodInfo _targetReceiveMsg = AccessTools.Method(typeof(ChatManager), nameof(ChatManager.TargetReceiveMessage), [typeof(INetworkPlayer), typeof(string), typeof(Player), typeof(bool)]);
    

    public static List<ulong> BlockList
    {
        get => _blockList.GetValue(null) as List<ulong>;
        set
        {
            if (value != null)
                _blockList.SetValue(null, value);
        }
    }

    public static void TargetReceiveMessage(INetworkPlayer receiver, string msg, Player from, bool allChat) {
        _targetReceiveMsg.Invoke(ChatManager, [receiver, msg, from, allChat]);
    }
}