using UnityEngine;
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

		private List<List<string>> table;

		private Vector2 scrollPosition;

		[MenuItem("Window/HotKeys Viewer")]
		private static void Open()
		{
			EditorWindow.GetWindow<HotKeysViewer>("HotKeys Viewer");
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
				this.AcquireWWW();
			}

			this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
			foreach(var t in this.table)
			{
				EditorGUILayout.BeginHorizontal(GUI.skin.box);
				foreach(var s in t)
				{
					EditorGUILayout.LabelField(s);
				}
				EditorGUILayout.EndHorizontal();
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
				this.table = this.Parse(www.text);
				this.www = null;
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

			public Table(string header, List<TableElement> elements)
			{
				this.Header = header;
				this.Elements = elements;
			}
		}

		private class TableElement
		{
			public string Keystroke{ private set; get; }

			public string Command{ private set; get; }

			public TableElement(string keystroke, string command)
			{
				this.Keystroke = keystroke;
				this.Command = command;
			}
		}
	}
}