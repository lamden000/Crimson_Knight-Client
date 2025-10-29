using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueEntry
{
    public string key;                 
    [TextArea(2, 5)] public List<string> lines; 
    public AudioClip[] audioClips;
}
