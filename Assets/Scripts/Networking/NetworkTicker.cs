using Assets.Scripts.Networking;
using UnityEngine;

public class NetworkTicker : MonoBehaviour
{
    void FixedUpdate()
    {
        Session.CheckReceiveMessage();
    }

    void OnApplicationQuit()
    {
        Session.Close();
    }
}
