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
                    Gender gender = (Gender)msg.ReadByte();
                    Player player = Player.Create(playerId, name,classType, gender);
                    ClientReceiveMessageHandler.Player = player;
                    ClientReceiveMessageHandler.Player.ClassType = classType;
                    CameraFollow.GI().target = player.transform;
                    MiniMapCamera.instance.player = player.transform;
                    MiniMapCamera.instance.minimapWindow.SetActive(true);
                    break;
                case MessageId.SERVER_PLAYER_MOVE:
                    ClientReceiveMessageHandler.PlayerMove(msg);
                    break;
                case MessageId.SERVER_ENTER_MAP:
                    ClientReceiveMessageHandler.EnterMap(msg);
                    break;
                case MessageId.SERVER_SHOW_MENU:
                    ClientReceiveMessageHandler.ShowMenu(msg);
                    break;
                case MessageId.SERVER_PLAYER_EXIT_MAP:
                    int otherPlayerId = msg.ReadInt();
                    ClientReceiveMessageHandler.OtherPlayerExitMap(otherPlayerId);
                    break;
                case MessageId.SERVER_MONSTERS_IN_MAP:
                    ClientReceiveMessageHandler.LoadMonstersInMap(msg);
                    break;
                case MessageId.SERVER_NPCS_IN_MAP:
                    ClientReceiveMessageHandler.LoadNpcsInMap(msg);
                    break;
                case MessageId.SERVER_OTHERPLAYERS_IN_MAP:
                    ClientReceiveMessageHandler.LoadOtherPlayersInMap(msg);
                    break;
                case MessageId.SERVER_PLAYER_BASE_INFO:
                    ClientReceiveMessageHandler.PlayerBaseInfo(msg);
                    break;

                case MessageId.SERVER_PLAYER_SKILL_INFO:
                    ClientReceiveMessageHandler.Player.LoadPlayerSkillInfoFromServer(msg);
                    break;

                case MessageId.SERVER_MONSTER_BASE_INFO:
                    int monsterId = msg.ReadInt();
                    if (ClientReceiveMessageHandler.Monsters.TryGetValue(monsterId, out Monster monster))
                    {
                        monster.LoadBaseInfoFromServer(msg);
                    }
                    break;
                case MessageId.SERVER_PLAYER_ATTACK:
                    ClientReceiveMessageHandler.PlayerAttack(msg);
                    break;
                case MessageId.SERVER_PLAYER_PKTYPE_INFO:
                    ClientReceiveMessageHandler.PlayerPktypeInfo(msg);
                    break;
                case MessageId.SERVER_MONSTER_ATTACK:
                    ClientReceiveMessageHandler.MonsterAttackInfo(msg);
                    break;
                case MessageId.SERVER_PLAYER_WEARING_ITEMS_INFO:
                    ClientReceiveMessageHandler.PlayerWearingItems(msg);
                    break;
                case MessageId.SERVER_PLAYER_INVENTORY_ITEMS_INFO:
                    ClientReceiveMessageHandler.InventoryItems(msg);
                    break;
                case MessageId.SERVER_ITEM_DROP:
                    ClientReceiveMessageHandler.DropItem(msg);
                    break;
                case MessageId.SERVER_PLAYER_PICK_ITEM:
                    ClientReceiveMessageHandler.PlayerPickItem(msg);
                    break;
                case MessageId.SERVER_REMOVE_ITEM_PICK:
                    ClientReceiveMessageHandler.RemoveItemPick(msg);
                    break;
                case MessageId.SERVER_CENTER_NOTIFICATION_VIEW:
                    ClientReceiveMessageHandler.CenterNotification(msg);
                    break;
                case MessageId.SERVER_PLAYER_GOLD_INFO:
                    long gold = msg.ReadLong();
                    ClientReceiveMessageHandler.Player.Gold = gold;
                    break;
                default:
                    break;
            }
        }
    }
}
