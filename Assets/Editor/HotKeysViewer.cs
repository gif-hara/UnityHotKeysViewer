﻿using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace HK.Framework.Editor.HotKeyViewer
{
	/// <summary>
	/// .
	/// </summary>
	public class HotKeysViewer : EditorWindow
	{
		private WWW www;

		private List<Table> table;

		private Vector2 scrollPosition;

		[MenuItem("Window/HotKeys Viewer")]
		private static void Open()
		{
			EditorWindow.GetWindow<HotKeysViewer>(true, "HotKeys Viewer");
		}

		void OnEnable()
		{
			if(this.table == null)
			{
				this.AcquireWWW();
			}
		}

		void OnGUI()
		{
			if(GUILayout.Button("WWW"))
			{
				this.table = null;
				this.AcquireWWW();
			}

			if(this.table == null)
			{
				EditorGUILayout.LabelField("Acquiring data...");
				return;
			}

			this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
			foreach(var t in this.table)
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				EditorGUILayout.LabelField(t.Header, EditorStyles.boldLabel);
				foreach(var s in t.Elements)
				{
					EditorGUILayout.BeginHorizontal(GUI.skin.box);
					EditorGUILayout.LabelField(s.HotKey);
					EditorGUILayout.LabelField(s.Command);
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndScrollView();
		}

		void OnInspectorUpdate()
		{
			if(this.www == null)
			{
				return;
			}

			if(this.www.isDone)
			{
				var parsedTable = this.Parse(www.text);
				this.table = new List<Table>();
				Table createTable = null;
				parsedTable.ForEach(t =>
				{
					if(t.Count == 1)
					{
						if(createTable != null)
						{
							this.table.Add(createTable);
						}
						createTable = new Table(t[0]);
					}
					else
					{
						createTable.Add(t[0], t[1]);
					}
				});
				this.table.Add(createTable);

				this.www = null;
				this.Repaint();
			}
		}

		private void AcquireWWW()
		{
			this.www = new WWW("https://docs.unity3d.com/ja/current/Manual/UnityHotkeys.html");			
		}

		private List<List<string>> Parse(string text)
		{
			return text
				.Split(new string[]{"<tr>"}, StringSplitOptions.RemoveEmptyEntries)
				.Where(t => t.IndexOf("<td") != -1)
				.Select(t => this.GetElements(t))
				.ToList();
		}

		private List<string> GetElements(string elementText)
		{
			return elementText.Split('\n')
				.Where(t => t.IndexOf("<td") != -1)
				.Select(t => this.GetElement(t))
				.ToList();
		}

		private string GetElement(string elementText)
		{
			var startIndex = 0;
			var endIndex = elementText.IndexOf("</");
			for(var i=endIndex; i>0; i--)
			{
				if(elementText[i] == '>')
				{
					startIndex = i + 1;
					break;
				}
			}

			return elementText.Substring(startIndex, endIndex - startIndex);
		}

		private class Table
		{
			public string Header{ private set; get; }

			public List<TableElement> Elements{ private set; get; }

			public Table(string header)
			{
				this.Header = header;
				this.Elements = new List<TableElement>();
			}

			public void Add(string hotKey, string command)
			{
				this.Elements.Add(new TableElement(hotKey, command));
			}
		}

		private class TableElement
		{
			public string HotKey{ private set; get; }

			public string Command{ private set; get; }

			public TableElement(string hotKey, string command)
			{
				hotKey = hotKey.Replace("CTRL/CMD", this.PlatformKey);
				this.HotKey = hotKey;
				this.Command = command;
			}

			private string PlatformKey
			{
				get
				{
					return Application.platform == RuntimePlatform.OSXEditor
					? "CMD"
					: "CTRL";
				}
			}
		}
	}
}