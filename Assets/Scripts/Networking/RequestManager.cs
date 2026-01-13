using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking
{
    public static class RequestManager
    {
        public static void PlayerMove(int x, int y)
        {
            Message msg = new Message(MessageId.CLIENT_PLAYER_MOVE);
            msg.WriteInt(x);
            msg.WriteInt(y);
            Session.AddMessage(msg);
        }

        public static void EnterMap(short departId)
        {
            Message msg = new Message(MessageId.CLIENT_ENTER_MAP);
            msg.WriteShort(departId);
            Session.AddMessage(msg);
        }

        public static void RequestShowMenu(int id)
        {
            Message msg = new Message(MessageId.CLIENT_SHOW_MENU);
            msg.WriteInt(id);
            Session.AddMessage(msg);
        }

        public static void RequestSelectMenuItem(int npcId, byte menuItemId)
        {
            Message msg = new Message(MessageId.CLIENT_SELECT_MENU_ITEM);
            msg.WriteInt(npcId);
            msg.WriteByte(menuItemId);
            Session.AddMessage(msg);
        }

        public static void RequestAttack(int skillUseId, bool[] isPlayers, int[] targetIds)
        {
            Message msg = new Message(MessageId.CLIENT_PLAYER_ATTACK);
            msg.WriteInt(skillUseId);
            msg.WriteByte((byte)isPlayers.Length);
            foreach (bool isPlayer in isPlayers)
            {
                msg.WriteBool(isPlayer);
            }
            foreach (int targetId in targetIds)
            {
                msg.WriteInt(targetId);
            }
            Session.AddMessage(msg);
        }

        public static void ChangePkType(PkType pkType)
        {
            Message msg = new Message(MessageId.CLIENT_PLAYER_CHANGE_PKTYPE);
            msg.WriteByte((byte)pkType);
            Session.AddMessage(msg);
        }

        public static void PickItem(string id)
        {
            Message msg = new Message(MessageId.CLIENT_PICK_ITEM);
            msg.WriteString(id);
            Session.AddMessage(msg);
        }

        public static void UseItem(string id, ItemType type)
        {
            Message msg = new Message(MessageId.CLIENT_USE_ITEM);
            msg.WriteString(id);
            msg.WriteByte((byte)type);
            Session.AddMessage(msg);
        }
    }
}
