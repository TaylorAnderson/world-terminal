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

  public bool skipCutscenes = false;
  public SuperTextMesh displayText;

  private float currentDelay = 1f;
  private float lineCounter = 0;

  public bool connectionLost = false;

  public Dictionary<string[], Action<string[]>> inputActions = new Dictionary<string[], Action<string[]>>();

  public List<CutScene> cutScenesSeen = new List<CutScene>();

  public GameObject roomTree;

  public int partsCollected = 0;

  private TreeNode<Room> startRoom;

  private float defaultCutoffFrequency = 5000;

  private string systemManual;

  [HideInInspector] public TreeNode<Room> currentRoom;

  void Awake() {
    Screen.fullScreen = false;
    Screen.SetResolution(1280, 720, false);
    lines.Clear();
    inputActions.Add(new string[] { "go", ">", "enter" },
      (string[] args) => {
        EnterRoom(args[1]);
      }
    );

    inputActions.Add(new string[] { "take", "pick up", "grab" },
        (string[] args) => {
          if (args[1] != null) {
            this.TakeObject(args[1]);
          }

        }
    );

    inputActions.Add(new string[] { "leave", "exit" },
      (string[] args) => {
        this.ExitRoom(true);
      }
    );

    inputActions.Add(new string[] { "describe", "show" },
     (string[] args) => {
       this.AddRoomText();
     }
   );
    inputActions.Add(new string[] { "?", "help" },
      (string[] args) => {
        this.AddManual();
      }
    );
    this.SetRoom(BuildRoom(roomTree));
    this.startRoom = currentRoom.Children[0];
    this.systemManual = startRoom.Value.cutScene.messages[startRoom.Value.cutScene.messages.Length - 1];
    this.EnterRoom(currentRoom.Children[0].Value.roomName);
  }

  void Start() {

  }

  public bool CutSceneCanBePlayed() {
    return !this.skipCutscenes && this.currentRoom.Value.cutScene.messages.Length > 0 && currentRoom.Value.cutScene.trigger != CutSceneTrigger.NEVER && this.cutScenesSeen.IndexOf(this.currentRoom.Value.cutScene) == -1;
  }

  public void Update() {
    if (!displayText.reading) {
      lineCounter += Time.deltaTime;
    }

    if (lines.Count > 0 && !displayText.reading && lineCounter > currentDelay) {
      currentDelay = lines[0].delay;
      lineCounter = 0;
      string line = lines[0].txt;
      lines.RemoveAt(0);


      if (this.displayText._text.Length > 2500) {
        this.displayText._text = this.displayText._text.Substring(1000, this.displayText._text.Length - 1000);
        this.displayText.Rebuild(-1);
        print("rebuilding");
      }
      this.displayText.Append(line + "\n");


    }

  }

  public void AddLine(string stringToAdd, float delay = 0, bool isSystemMessage = false, bool isScientistMessage = false) {
    stringToAdd = stringToAdd + "\n";
    char[] newLine = { '\n' };
    string[] lines = stringToAdd.Split(newLine);
    for (int i = 0; i < lines.Length; i++) {
      if (lines[i].Length > 0) {
        string line = lines[i];
        if (isSystemMessage) {
          line = "<c=computer>" + line + "</c>";
        }
        if (isScientistMessage) {
          line = "<c=scientist>" + line + "</c>";
        }
        this.lines.Add(new Line(line, delay));
      }
    }
  }

  public void AddRoomText(bool showDescription = true) {
    if (showDescription) {
      AddLine(currentRoom.Value.description, 1.75f);
    }

    bool foundPart = false;
    bool foundAnything = false;
    for (int i = 0; i < currentRoom.Children.Count; i++) {
      if (currentRoom.Children[i].Value.accessible) {
        foundAnything = true;
        this.AddLine("IDENTIFIED: [" + currentRoom.Children[i].Value.roomName + "]", 0, true);
        if (currentRoom.Children[i].Value.isGoal) {
          foundPart = true;
          currentRoom.Children[i].Value.accessible = false;
        }
      }
      print("found anything: " + foundAnything);
      if (!foundAnything) {
        AddLine("[NOTHING IDENTIFIED]", 0, true);
      }


    }
    if (foundPart) {
      AddLine("PART FOUND; ENTERING EDIT MODE\nUse the 'take' command to grab things from the scene.");
    }
  }

  public void AddManual() {
    AddLine(this.systemManual);
  }

  public void PlayLineFromCutScene() {
    this.AddLine(this.currentCutScene.messages[this.currentCutSceneIndex], 0, false, true);
    this.currentCutSceneIndex++;
    if (this.currentCutSceneIndex >= this.currentCutScene.messages.Length) {
      this.currentCutScene.messages = null;
      if (this.partsCollected < 3) {
        this.AddRoomText();
      }
      else {
        this.connectionLost = true;
        AddLine("[CONNECTION LOST]");
      }

    }
  }

  public void EnterCutScene(CutScene cutScene) {
    this.currentCutScene = cutScene;
    this.currentCutSceneIndex = 0;
    this.cutScenesSeen.Add(cutScene);

  }

  public void TakeObject(string obj) {
    for (int i = 0; i < this.currentRoom.Children.Count; i++) {
      Room room = currentRoom.Children[i].Value;

      if (room.roomName.ToLower() == obj.ToLower() && room.isObject) {
        bool playExit = false;
        CutScene exitCutscene = currentRoom.Value.cutScene;
        if (CutSceneCanBePlayed() && this.currentRoom.Value.cutScene.trigger == CutSceneTrigger.ON_OBJECT_TAKEN) {
          playExit = true;
        }
        AddLine("[" + room.roomName + "] OBTAINED. Exiting snapshot...");

        this.partsCollected++;
        if (partsCollected == 1) this.startRoom.Children[1].Value.accessible = true;
        if (partsCollected == 2) this.startRoom.Children[2].Value.accessible = true;
        ReturnToRoot();

        if (playExit) {
          EnterCutScene(exitCutscene);
        }
      }
    }
  }

  public void SetRoom(TreeNode<Room> room) {
    if (this.currentRoom != null) {
      if (!currentRoom.Children.Contains(room)) {
        AudioSource source = currentRoom.Value.GetComponent<AudioSource>();
        if (source != null) {
          source.Stop();
        }
      }
    }

    this.currentRoom = room;
    UpdateRoomAudio();
  }

  public void UpdateRoomAudio() {

    AudioSource source = currentRoom.Value.GetComponent<AudioSource>();
    AudioLowPassFilter filter = currentRoom.Value.GetComponent<AudioLowPassFilter>();
    if (source != null) {
      StartCoroutine(TweenVolume(source, currentRoom.Value.sound.volume));
      StartCoroutine(TweenLowPassFilter(filter, this.defaultCutoffFrequency));
      source.Play();
      AudioSource overlaidSound = this.currentRoom.Parent.Value.GetComponent<AudioSource>();
      if (overlaidSound) {
        source.time = overlaidSound.time % source.clip.length;
      }
    }

    float currentVolMultplier = 0.6f;
    float currentCutoffFrequencyMultiplier = 0.5f;

    TreeNode<Room> roomNode = currentRoom;
    while (roomNode.Parent != null) {
      roomNode = roomNode.Parent;
      currentVolMultplier *= 0.7f;
      if (currentVolMultplier <= 0.1) currentVolMultplier = 0;
      currentCutoffFrequencyMultiplier /= 2;
      AudioSource parentSource = roomNode.Value.GetComponent<AudioSource>();
      AudioLowPassFilter parentFilter = roomNode.Value.GetComponent<AudioLowPassFilter>();
      if (parentSource) {

        StartCoroutine(TweenVolume(parentSource, roomNode.Value.sound.volume * currentVolMultplier));
        StartCoroutine(TweenLowPassFilter(parentFilter, this.defaultCutoffFrequency * currentCutoffFrequencyMultiplier));
      }
    }
  }

  public IEnumerator TweenVolume(AudioSource src, float desiredVolume) {
    while (src.volume != desiredVolume) {
      src.volume += Math.Sign(desiredVolume - src.volume) * 0.02f;
      yield return new WaitForSeconds(0.1f);
    }
  }
  public IEnumerator TweenLowPassFilter(AudioLowPassFilter filter, float desiredFilterValue) {
    while (filter.cutoffFrequency != desiredFilterValue) {
      filter.cutoffFrequency += Math.Sign(desiredFilterValue - filter.cutoffFrequency) * 0.02f;
      yield return new WaitForSeconds(0.1f);
    }
  }

  public void EnterRoom(string roomName) {
    for (int i = 0; i < currentRoom.Children.Count; i++) {
      TreeNode<Room> room = currentRoom.Children[i];

      string actualRoomName = "";
      if (room.Value.roomPrompts.Length > 0) {
        for (int j = 0; j < room.Value.roomPrompts.Length; j++) {
          if (roomName == room.Value.roomPrompts[j]) {
            actualRoomName = room.Value.roomPrompts[j];
          }
        }
      }
      else {
        actualRoomName = room.Value.roomName;
      }
      if (actualRoomName.ToLower() == roomName.ToLower()) {
        if (room.Value.isObject) {
          AddLine(room.Value.roomName + " cannot be entered.");
          return;
        }
        SetRoom(room);
        if (this.currentRoom.Parent != null && this.currentRoom.Parent.Parent != null) AddLine(room.Value.roomName + " entered.", 1.5f);
        if (CutSceneCanBePlayed() && this.currentRoom.Value.cutScene.trigger == CutSceneTrigger.ON_START) {
          EnterCutScene(this.currentRoom.Value.cutScene);
        }
        else {
          AddRoomText();
        }
        return;
      }
    }
    AddLine(roomName + " does not exist.");
  }

  public void ExitRoom(bool addRoomText, bool overrideExitLock = false) {
    if (currentRoom.Children.Count > 0 && currentRoom.Children[0].Value.isGoal && currentRoom.Children[0].Value.accessible && !overrideExitLock) {
      AddLine("Exiting edit mode.", 0, true);
    }
    if (CutSceneCanBePlayed() && currentRoom.Value.cutScene.trigger == CutSceneTrigger.ON_END) {
      for (int i = 0; i < this.currentRoom.Value.cutScene.messages.Length; i++) {
        AddLine(this.currentRoom.Value.cutScene.messages[i], 2f, false, true);
      }
    }
    if (this.currentRoom.Parent != null && this.currentRoom.Parent.Parent != null) {
      AddLine(this.currentRoom.Value.roomName + " exited");
    }
    SetRoom(this.currentRoom.Parent);

    if (addRoomText) {
      if (this.currentRoom.Parent != null) {
        AddLine("Current room: " + this.currentRoom.Value.roomName);
      }
      this.AddRoomText(false);
    }
    if (partsCollected == 3 && currentRoom.Parent.Parent == null) {
      EnterCutScene(((StartRoom)currentRoom.Value).finalCutscene);
    }
  }

  public void ReturnToRoot() {
    int loopSafety = 50;
    while (currentRoom.Parent != null && currentRoom.Parent.Parent != null && loopSafety >= 0) {
      //basically, we do wanna see the room text if we're done returning.
      if (currentRoom.Parent.Parent.Parent == null) {
        ExitRoom(true, true);
      }
      else {
        ExitRoom(false, true);
      }


      loopSafety--;
    }
  }

  public TreeNode<Room> BuildRoom(GameObject go) {
    Room roomVal = go.GetComponent<Room>();
    if (go.GetComponent<Room>().sound.clip != null) {
      AudioSource source = go.AddComponent<AudioSource>();
      source.playOnAwake = false;
      source.volume = 0;
      source.clip = roomVal.sound.clip;
      source.loop = true;
      source.priority = 0;
      AudioLowPassFilter lowPassFilter = go.AddComponent<AudioLowPassFilter>();
      lowPassFilter.cutoffFrequency = this.defaultCutoffFrequency;
      lowPassFilter.lowpassResonanceQ = 0.5f;

    }
    TreeNode<Room> room = new TreeNode<Room>(roomVal);
    room.Value.roomName = go.name;
    for (int i = 0; i < go.transform.childCount; i++) {
      room.AddTreeNodeChild(BuildRoom(go.transform.GetChild(i).gameObject));
    }
    return room;
  }
}
