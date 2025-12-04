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
                case MessageId.LOGIN:
                    break;
                case MessageId.PLAYER_ENTER_MAP:
                    //map
                    short mapId = msg.ReadShort();
                    string mapName = msg.ReadString();

                    //player
                    short x = msg.ReadShort();
                    short y = msg.ReadShort();
                    GameHandler.PlayerEnterMap(mapId, mapName, x, y);
                    break;


                //OTHER PLAYER MESSAGES
                case MessageId.OTHER_PLAYER_ENTER_MAP:
                    int otherPlayerId = msg.ReadInt();
                    string otherPlayerName = msg.ReadString();
                    short otherX = msg.ReadShort();
                    short otherY = msg.ReadShort();
                    Debug.Log($"Người chơi khác {otherPlayerName} (ID: {otherPlayerId}) đã vào bản đồ tại vị trí ({otherX}, {otherY})");
                    break;
                default:
                    break;
            }
        }
    }
}
