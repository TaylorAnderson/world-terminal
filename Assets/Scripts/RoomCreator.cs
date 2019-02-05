using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class RoomCreator : MonoBehaviour {
#if UNITY_EDITOR
  [MenuItem("GameObject/Room", false, 10)]
  static void CreateCustomGameObject(MenuCommand menuCommand) {
    // Create a custom game object
    GameObject go = new GameObject("Room");

    go.AddComponent<Room>();
    // Ensure it gets reparented if this was a context click (otherwise does nothing)
    GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
    // Register the creation in the undo system
    Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
    Selection.activeObject = go;
  }
#endif
}
