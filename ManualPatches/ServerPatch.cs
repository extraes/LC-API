﻿using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using BepInEx;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using HarmonyLib;
using LC_API.ServerAPI;
using LC_API.Comp;
using LC_API.Data;
using System.Runtime.Remoting.Contexts;
using UnityEngine.InputSystem;
using LC_API.Extensions;

namespace LC_API.ManualPatches
{
    internal static class ServerPatch
    {
        internal static bool OnLobbyCreate(GameNetworkManager __instance, Result result, Lobby lobby)
        {
            if (result != Result.OK)
            {
                Debug.LogError(string.Format("Lobby could not be created! {0}", result), __instance);
            }
            __instance.lobbyHostSettings.lobbyName = "[MODDED]" + __instance.lobbyHostSettings.lobbyName.ToString();
            Plugin.Log.LogMessage("server pre-setup success");
            return (true);
        }

        internal static bool CacheMenuManager(MenuManager __instance)
        {
            SVAPI.MenuManager = __instance;
            return true;
        }

        internal static bool ChatInterpreter(HUDManager __instance, string chatMessage)
        {
            if (!chatMessage.Contains("NWE") || !chatMessage.Contains("<size=0>"))
                return true;
            

            string[] dataFragments = chatMessage.Split('/');

            if (dataFragments.Length < 5)
            {
                Plugin.Log.LogError($"Generic Network receive fail (expected 5+ data fragments, got {dataFragments.Length}). This is a failure of the API, and it should be reported as a bug.");
                return true;
            }

            if (!int.TryParse(dataFragments[4], out int parsedPlayer))
            {
                Plugin.Log.LogWarning($"Failed to parse player ID '{dataFragments[4]}'!!");
                return false;
            }

            if (parsedPlayer == (int)GameNetworkManager.Instance.localPlayerController.playerClientId & !SVAPI.netTester)
            {
                return false;
            }

            if (!Enum.TryParse(dataFragments[3], out NetworkBroadcastDataType type))
            {
                Plugin.Log.LogError($"Unknown datatype - unable to parse '{dataFragments[3]}' into a known data type!");
                return false;
            }

            switch (type)
            {
                case NetworkBroadcastDataType.BDstring:
                    Networking.GetString.InvokeActionSafe(dataFragments[1], dataFragments[2]);
                    break;

                case NetworkBroadcastDataType.BDint:
                    Networking.GetInt.InvokeActionSafe(int.Parse(dataFragments[1]), dataFragments[2]);
                    break;

                case NetworkBroadcastDataType.BDfloat:
                    Networking.GetFloat.InvokeActionSafe(float.Parse(dataFragments[1]), dataFragments[2]);
                    break;

                case NetworkBroadcastDataType.BDvector3:
                    // technically creating garbage by creating a 2-long char arr every time this executes
                    string vectorStr = dataFragments[1].Trim('(', ')');
                    string[] components = vectorStr.Split(',');
                    Vector3 convertedString = default; // could also use Vector3.zero, but that copies memory instead of just initializing it to all 0's

                    if (components.Length != 3)
                    {
                        Plugin.Log.LogError($"Vector3 Network receive fail (expected 3 numbers, got {components.Length} number(?)(s) instead). This is a failure of the API, and it should be reported as a bug. (passing an empty Vector3 in its place)");
                    }
                    else
                    {
                        float x, y, z;
                        if (float.TryParse(components[0], out x) && float.TryParse(components[1], out y) && float.TryParse(components[2], out z))
                        {
                            convertedString.x = x;
                            convertedString.y = y;
                            convertedString.z = z;
                        }
                        else
                        {
                            Plugin.Log.LogError($"Vector3 Network receive fail (failed to parse '{vectorStr}' as numbers). This is a failure of the API, and it should be reported as a bug.");
                        }
                    }

                    Networking.GetVector3.InvokeActionSafe(convertedString, dataFragments[2]);
                    break;
            }

            if (SVAPI.netTester)
            {
                //Plugin.Log.LogWarning("Success! Received data with no errors.");
            }
            return false;
            
        }

        internal static bool ChatCommands(HUDManager __instance, InputAction.CallbackContext context)
        {
            if (__instance.chatTextField.text.ToLower().Contains("/modcheck"))
            {
                CheatDatabase.OtherPlayerCheatDetector();
                return false;
            }
            return true;
        }
    }
}
