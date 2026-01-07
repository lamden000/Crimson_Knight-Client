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
            Message msg = new Message(MessageId.CLIENT_MOVE);
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
    }
}
