﻿using LC_API.GameInterfaceAPI;
using LC_API.ServerAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#pragma warning disable CS0618 // Member is obsolete
namespace LC_API.Comp
{
    internal class SVAPI : MonoBehaviour
    {
        public static MenuManager MenuManager;
        public static bool netTester = false;
        private static int playerCount;
        private static bool wanttoCheckMods;
        private static float lobbychecktimer;
        public void Update()
        {
            GameState.GSUpdate();
            if (HUDManager.Instance != null & netTester)
            {
                if (GameNetworkManager.Instance.localPlayerController  != null)
                {
                    Networking.Broadcast("testerData", "testerSignature");
                }
            }
            if (!ModdedServer.setModdedOnly)
            {
                ModdedServer.OnSceneLoaded();
            }
            else if (ModdedServer.ModdedOnly)
            {
                if (MenuManager != null)
                {
                    if (MenuManager.versionNumberText)
                    {
                        MenuManager.versionNumberText.text = $"v{GameNetworkManager.Instance.gameVersionNum - 16440}\nMOD";
                    }
                }
            }

            if (GameNetworkManager.Instance != null)
            {
                if (playerCount < GameNetworkManager.Instance.connectedPlayers)
                {
                    lobbychecktimer = -8f;
                    wanttoCheckMods = true;
                }
                playerCount = GameNetworkManager.Instance.connectedPlayers;
            }
            if (lobbychecktimer < 0)
            {
                lobbychecktimer += Time.deltaTime;
            }
            else if (wanttoCheckMods)
            {
                wanttoCheckMods = false;
                CD();
            }
        }

        private void CD()
        {
            CheatDatabase.OtherPlayerCheatDetector();
        }
    }
}
#pragma warning restore CS0618 // Member is obsolete
