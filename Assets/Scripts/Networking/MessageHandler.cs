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
                case MessageId.OK:
                    Debug.Log("Server gửi: OK");
                    break;
                default:
                    break;
            }
        }
    }
}
