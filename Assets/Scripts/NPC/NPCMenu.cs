using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCMenu", menuName = "Scriptable Objects/NPCMenu")]
public class NPCMenu : ScriptableObject
{
    public string menuName;
    public List<NPCMenuOption> options;
}
