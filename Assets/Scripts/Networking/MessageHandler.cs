using Assets.Scripts.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public static class MessageHandler
    {
        public static void HandleMessage(Message msg)
        {
            switch (msg.Id)
            {
                case MessageId.SERVER_LOGIN:
                    int playerId = msg.ReadInt();
                    string name = msg.ReadString();
                    ClassType classType = (ClassType)msg.ReadByte();
                    Player player = Player.Create(playerId, name);
                    GameHandler.Player = player;
                    GameHandler.Player.ClassType = classType;
                    CameraFollow.GI().target = player.transform;
                    MiniMapCamera.instance.player = player.transform;
                    MiniMapCamera.instance.minimapWindow.SetActive(true);
                    break;
                case MessageId.SERVER_PLAYER_BASE_INFO:
                    GameHandler.Player.LoadBaseInfoFromServer(msg);
                    break;
                case MessageId.SERVER_PLAYER_SKILL_INFO:
                    GameHandler.Player.LoadPlayerSkillInfoFromServer(msg);
                    break;
                case MessageId.SERVER_ENTER_MAP:
                    GameHandler.PlayerEnterMap(msg);
                    break;
                case MessageId.SERVER_OTHERPLAYERS_IN_MAP:
                    GameHandler.LoadOtherPlayersInMap(msg);
                    break;
                case MessageId.SERVER_MONSTERS_IN_MAP:
                    GameHandler.LoadMonstersInMap(msg);
                    break;
                case MessageId.SERVER_NPCS_IN_MAP:
                    GameHandler.LoadNpcsInMap(msg);
                    break;
                case MessageId.SERVER_OTHER_PLAYER_MOVE:
                    GameHandler.OtherPlayerMove(msg);
                    break;
                case MessageId.SERVER_SHOW_MENU:
                    GameHandler.ShowMenu(msg);
                    break;

                //OTHER PLAYER MESSAGES
                case MessageId.SERVER_OTHER_PLAYER_ENTER_MAP:
                    int otherPlayerId = msg.ReadInt();
                    string otherPlayerName = msg.ReadString();
                    short otherX = msg.ReadShort();
                    short otherY = msg.ReadShort();
                    GameHandler.OtherPlayerEnterMap(otherPlayerId, otherPlayerName, otherX, otherY);
                    break;
                case MessageId.SERVER_OTHER_PLAYER_EXIT_MAP:
                    otherPlayerId = msg.ReadInt();
                    GameHandler.OtherPlayerExitMap(otherPlayerId);
                    break;
                case MessageId.SERVER_OTHER_PLAYER_BASE_INFO:
                    otherPlayerId = msg.ReadInt();
                    if (GameHandler.OtherPlayers.TryGetValue(otherPlayerId, out OtherPlayer otherPlayer))
                    {
                        otherPlayer.LoadBaseInfoFromServer(msg);
                    }
                    break;
                case MessageId.SERVER_MONSTER_BASE_INFO:
                    int monsterId = msg.ReadInt();
                    if (GameHandler.Monsters.TryGetValue(monsterId, out Monster monster))
                    {
                        monster.LoadBaseInfoFromServer(msg);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
