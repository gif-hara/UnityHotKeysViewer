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

		private List<Table> table;

		private Vector2 scrollPosition;

		[MenuItem("Window/Unity HotKeys")]
		private static void Open()
		{
			EditorWindow.GetWindow<HotKeysViewer>(true, "Unity HotKeys");
		}

		void OnEnable()
		{
			this.AcquireWWW(this.DefaultCulture);
		}

		void OnGUI()
		{
			if(this.www == null && this.table == null)
			{
				EditorGUILayout.LabelField("Push Acquire data Button");
				GUILayout.FlexibleSpace();
			}
			else if(this.table == null)
			{
				EditorGUILayout.LabelField("Acquiring data...");
				GUILayout.FlexibleSpace();
			}
			else
			{
				this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
				foreach(var t in this.table)
				{
					EditorGUILayout.BeginVertical(GUI.skin.box);
					EditorGUILayout.LabelField(t.Header, EditorStyles.boldLabel);
					foreach(var s in t.Elements)
					{
						EditorGUILayout.BeginHorizontal(GUI.skin.box);
						EditorGUILayout.LabelField(s.HotKey, GUILayout.Width(200));
						EditorGUILayout.LabelField(s.Command);
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndScrollView();
			}

			EditorGUILayout.BeginHorizontal(GUI.skin.box);
			EditorGUILayout.LabelField("Acquire data:", GUILayout.Width(75));
			if(GUILayout.Button("English"))
			{
				this.AcquireWWW("en");
			}
			if(GUILayout.Button("日本語"))
			{
				this.AcquireWWW("ja");
			}
			if(GUILayout.Button("Espanol"))
			{
				this.AcquireWWW("es");
			}
			if(GUILayout.Button("한국어"))
			{
				this.AcquireWWW("kr");
			}
			EditorGUILayout.EndHorizontal();

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

		private void AcquireWWW(string culture)
		{
			this.table = null;
			var url = culture == "en"
				? "https://docs.unity3d.com/Manual/UnityHotkeys.html"
				: string.Format("https://docs.unity3d.com/{0}/current/Manual/UnityHotkeys.html", culture);
			this.www = new WWW(url);			
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

		private string DefaultCulture
		{
			get
			{
				return Application.systemLanguage == SystemLanguage.Japanese
				? "ja"
				: Application.systemLanguage == SystemLanguage.Korean
				? "kr"
				: "en";
			}
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