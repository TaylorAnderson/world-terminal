using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[System.Serializable]

public class Room : MonoBehaviour {
  [HideInInspector]
  public string roomName;
  public string[] roomPrompts;
  [TextArea(15, 20)]
  public string description;

  public CutScene cutScene;

  public bool isObject;
  public bool isGoal;
  public bool accessible = true;
  public Sound sound;
}

[System.Serializable]
public class Sound {
  public AudioClip clip;
  [Range(0, 1)]
  public float volume = 1;
}
