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
                    int playerId = msg.ReadInt();
                    string name = msg.ReadString();
                    GameHandler.Player.Id = playerId;
                    GameHandler.Player.Name = name;
                    break;
                case MessageId.PLAYER_ENTER_MAP:
                    
                    GameHandler.PlayerEnterMap(msg);
                    break;
                case MessageId.OTHER_PLAYER_MOVE:
                    GameHandler.OtherPlayerMove(msg);
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
