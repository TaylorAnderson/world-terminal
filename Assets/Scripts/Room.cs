using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TextAdventure/Room")]


public class Room : ScriptableObject {
  [TextArea(15, 20)]
  public string description;

  [TextArea]
  public string prompt;

  public string roomName;

  public CutScene cutScene;

  public bool isObject;
  public bool isGoal;

  public Room[] children;


}
