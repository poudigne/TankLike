using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ChatController : MonoBehaviour
{
  public float chatWidth = 500.0f;
  public float chatHeight = 300.0f;

  private Vector2 scrollPosition = Vector2.zero;
  private List<string> saidStuff;

  #region Unity Engine

  void Awake()
  {
    saidStuff = new List<string>();
  }

  void Start()
  {
    AddLineToChat("", "Welcome to my chat test!");
    AddLineToChat("", "type /Help for some basic commands");
  }

  void OnGUI()
  {
    var ViewPort = (25 * (saidStuff.Count - 4));
    if (ViewPort < 0)
    {
      ViewPort = 0;
    }
    scrollPosition = GUI.BeginScrollView(new Rect(10, 10, 420, 100), scrollPosition, new Rect(0, 0, 400, 100 + ViewPort), false, true);
    var HeightOfBox = (25 * (saidStuff.Count - 4));
    if (HeightOfBox < 0)
    {
      HeightOfBox = 0;
    }
    GUI.Box(new Rect(0, 0, 400, 100 + HeightOfBox), " ");
    for (var i = 0; i < saidStuff.Count; i++)
    {
      GUI.Label(new Rect(0, (25 * i), 400, 23), saidStuff[i]);
    }
    GUI.EndScrollView();
  }

  #endregion
  void AddLineToChat(string name, string text)
  {

    saidStuff.Add(text);
    if (text == "/Help")
    {
      DisplayHelp();
    }
    if (text == "/Top")
    {
      scrollPosition = Vector2.zero;
    }
    else
    {
      Vector2 LowestSpot = new Vector2(0, 25 * (saidStuff.Count - 4));
      if (LowestSpot.y < 0)
      {
        LowestSpot.y = 0;
      }
      scrollPosition = LowestSpot;
    }
    if (text == "/Clear")
    {
      saidStuff = new List<string>();
      AddLineToChat("", "Cleared chat");
    }
  }
  void DisplayHelp()
  {
    AddLineToChat("", "  ");
    AddLineToChat("", "Here are some commands!");
    AddLineToChat("", "/Clear to clear your chat history");
    AddLineToChat("", "/Top to go back to the top");
  }

  public void AddNewLine(string name, string text)
  {
    AddLineToChat(name, text);
  }

  // Draw the Chat UI
  private void DrawChatUI()
  {
    GUI.Box(new Rect(5, 5, chatWidth, chatHeight), string.Empty);
  }
}
