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
                    Player player = Player.Create(playerId, name); 
                    GameHandler.Player = player;
                    CameraFollow.GI().target = player.transform;
                    MiniMapCamera.instance.player = player.transform;
                    MiniMapCamera.instance.minimapWindow.SetActive(true);
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
                default:
                    break;
            }
        }
    }
}
