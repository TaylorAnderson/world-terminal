using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public struct Line {
  public string txt;
  public float delay;

  public Line(string txt, float delay = 0) {
    this.txt = txt;
    this.delay = delay;
  }
}
public class GameController : MonoBehaviour {
  [HideInInspector] public List<string> interactivesInRoom = new List<string>();
  [HideInInspector] public CutScene currentCutScene = null;
  [HideInInspector] public int currentCutSceneIndex = 0;
  private List<Line> lines = new List<Line>();


  public SuperTextMesh displayText;

  private float currentDelay = 1f;
  private float lineCounter = 0;

  public Dictionary<string[], Action<string[]>> inputActions = new Dictionary<string[], Action<string[]>>();

  public Room startRoom;

  [HideInInspector] public TreeNode<Room> currentRoom;

  void Awake() {

    lines.Clear();
    inputActions.Add(new string[] { "go", ">", "enter" },
      (string[] args) => {
        EnterRoom(args[1]);
      }
    );

    inputActions.Add(new string[] { "take", "pick up", "grab" },
        (string[] args) => {
          this.TakeObject(args[1]);
        }
    );
    Room startRoomClone = Instantiate(startRoom);
    this.currentRoom = new TreeNode<Room>(startRoomClone);
    ConstructRoomTree(startRoomClone, this.currentRoom);
    this.EnterRoom(startRoomClone.children[0].roomName);
  }

  void Start() {
  }

  public void Update() {

    lineCounter += Time.deltaTime;
    if (lines.Count > 0 && !displayText.reading && lineCounter > currentDelay) {
      currentDelay = lines[0].delay;
      lineCounter = 0;
      string line = lines[0].txt;
      lines.RemoveAt(0);
      this.displayText.Append(line + "\n");
    }
  }

  public void AddLine(string stringToAdd, float delay = 0) {
    stringToAdd = stringToAdd + "\n";
    char[] newLine = { '\n' };
    string[] lines = stringToAdd.Split(newLine);
    for (int i = 0; i < lines.Length; i++) {
      if (lines[i].Length > 0) {
        this.lines.Add(new Line(lines[i], delay));
      }
    }
  }

  public void AddRoomText() {
    AddLine(currentRoom.Value.description, 1.75f);
    bool foundPart = false;
    for (int i = 0; i < currentRoom.Children.Count; i++) {
      this.AddLine("IDENTIFIED: [" + currentRoom.Children[i].Value.prompt + "]");
      if (currentRoom.Children[i].Value.isGoal) {
        foundPart = true;
      }
    }
    if (foundPart) {
      AddLine("PART FOUND; ENTERING EDIT MODE");
    }
  }

  public void PlayLineFromCutScene() {
    this.AddLine(this.currentCutScene.messages[this.currentCutSceneIndex]);
    this.currentCutSceneIndex++;
    if (this.currentCutSceneIndex >= this.currentCutScene.messages.Count) {
      this.currentCutScene = null;
      this.AddRoomText();
    }
  }

  public void EnterCutScene(CutScene cutScene) {
    this.currentCutScene = cutScene;
    this.currentCutSceneIndex = 0;
  }

  public void TakeObject(string obj) {
    for (int i = 0; i < this.currentRoom.Children.Count; i++) {
      Room room = currentRoom.Children[i].Value;
      if (room.roomName.ToLower() == obj.ToLower() && room.isObject) {
        AddLine("[" + room.roomName + "] OBTAINED. Exiting snapshot...");
        ReturnToRoot();
      }
    }

  }

  public void EnterRoom(string roomName) {
    for (int i = 0; i < currentRoom.Children.Count; i++) {
      TreeNode<Room> room = currentRoom.Children[i];
      if (room.Value.roomName.ToLower() == roomName.ToLower()) {
        if (room.Value.isObject) {
          AddLine(room.Value.roomName + " cannot be entered.");
          return;
        }
        this.currentRoom = room;
        if (this.currentRoom.Value.cutScene && this.currentRoom.Value.cutScene.trigger == CutSceneTrigger.ON_START) {
          EnterCutScene(this.currentRoom.Value.cutScene);
          this.currentRoom.Value.cutScene = null;
        }
        if (this.currentRoom.Parent.Parent != null) {
          AddLine(room.Value.roomName + " entered.", 1.5f);
          AddRoomText();
        }
        return;
      }
    }

    AddLine(roomName + " does not exist.");
  }

  public void ExitRoom() {
    AddLine(this.currentRoom.Value.roomName + " exited");
    this.currentRoom = this.currentRoom.Parent;
  }

  public void ReturnToRoot() {
    while (currentRoom.Parent != null) {
      ExitRoom();  
    }
    EnterRoom(currentRoom.Children[0].Value.roomName);
  }

  private void ConstructRoomTree(Room room, TreeNode<Room> node) {
    for (int i = 0; i < room.children.Length; i++) {
      Room childClone = Instantiate(room.children[i]);
      TreeNode<Room> child = node.AddChild(childClone);
      ConstructRoomTree(childClone, child);
    }
  }
}
