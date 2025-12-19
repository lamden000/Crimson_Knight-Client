using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking
{
    public enum MessageId : short
    {
        LOGIN,
        PLAYER_MOVE,
        PLAYER_ENTER_MAP,
        PLAYER_OTHERPLAYERS_IN_MAP,
        PLAYER_MONSTERS_IN_MAP,

        //send other
        OTHER_PLAYER_MOVE,
        OTHER_PLAYER_ENTER_MAP,
        OTHER_PLAYER_EXIT_MAP
    }
}
