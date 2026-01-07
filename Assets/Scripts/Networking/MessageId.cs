using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking
{
    public enum MessageId : short
    {
        //client,
        CLIENT_LOGIN,
        CLIENT_MOVE,
        CLIENT_ENTER_MAP,
        CLIENT_SHOW_MENU,
        CLIENT_SELECT_MENU_ITEM,

        //server
        SERVER_LOGIN,
        SERVER_ENTER_MAP,
        SERVER_OTHERPLAYERS_IN_MAP,
        SERVER_MONSTERS_IN_MAP,
        SERVER_NPCS_IN_MAP,
        SERVER_SHOW_MENU,

        //other players
        SERVER_OTHER_PLAYER_MOVE,
        SERVER_OTHER_PLAYER_ENTER_MAP,
        SERVER_OTHER_PLAYER_EXIT_MAP,
    }
}
