using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CutSceneTrigger {
    ON_START,
    ON_END
}

[CreateAssetMenu(menuName = "TextAdventure/CutScene")]
public class CutScene : ScriptableObject {
    [TextArea]
    
    public List<string> messages;
    public CutSceneTrigger trigger;
}