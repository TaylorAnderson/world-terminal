using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
public class TextInput : MonoBehaviour {

  public InputField inputField;
  GameController controller;
  private void Awake() {
    controller = GetComponent<GameController>();
    inputField.caretWidth = 10;
  }

  private void Update() {
    inputField.Select();
    if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) {
      this.AcceptStringInput(this.inputField.text);
    }
  }
  private void AcceptStringInput(string userInput) {
    inputField.Select();
    if (controller.currentCutScene != null) {
      this.controller.PlayLineFromCutScene();
    }
    else {
      userInput = userInput.ToLower();
      controller.AddLine(userInput);

      char[] delimChars = { ' ' };
      string[] words = userInput.Split(delimChars);

      foreach (KeyValuePair<string[], Action<string[]>> pair in controller.inputActions) {
        string[] actionKeys = pair.Key;
        for (int j = 0; j < actionKeys.Length; j++) {
          if (actionKeys[j] == words[0]) {
            pair.Value(words);
          }
        }
      }
    }


    InputComplete();

  }



  private void InputComplete() {
    inputField.ActivateInputField();
    inputField.text = null;
  }
}
