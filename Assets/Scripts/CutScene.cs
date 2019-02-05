using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CutSceneTrigger {
  ON_START,
  ON_END,
  ON_OBJECT_TAKEN,
  NEVER
}

[System.Serializable]
public class CutScene {
  [TextArea]
  public string[] messages;
  public CutSceneTrigger trigger;
}