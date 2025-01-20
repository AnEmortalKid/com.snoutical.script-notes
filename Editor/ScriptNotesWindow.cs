using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// TODO make the names pretty like in the editor
// TODO make the right pane be a like wrap box or some shit
// TODO decorate the labels nicely
// TODO maybe do this with UIToolkit builder
namespace Snoutical.ScriptNotes
{
    public class ScriptNotesWindow : EditorWindow
    {
        [MenuItem("Snoutical/Script Notes")]
        public static void CreateWindow()
        {
            ScriptNotesWindow wnd = GetWindow<ScriptNotesWindow>();
            wnd.titleContent = new GUIContent("Script Notes");
        }

        private GameObject previous;
        private bool initialized;
        private VisualElement rightPane;
        private ListView leftPane;

        private List<ScriptNote> loadedNotes = new();
        private HashSet<string> processedScripts = new();

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            // Create a two-pane view with the left pane being fixed.
            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);

            leftPane = new ListView();
            splitView.Add(leftPane);
            leftPane.makeItem = () => new Label()
            {
                style = { fontSize = 14 }
            };
            leftPane.bindItem = (item, index) =>
            {
                (item as Label).text = PascalCaseToSpaced(loadedNotes[index].ScriptName);
            };
            leftPane.itemsSource = loadedNotes;
            leftPane.selectionChanged += OnMonoSelectionChange;

            rightPane = new VisualElement();
            splitView.Add(rightPane);

            initialized = true;
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            var selected = Selection.activeGameObject;
            // nothing selected should we clear it
            if (selected == null)
            {
                // remove state
                if (previous != null)
                {
                    ClearItems();
                    previous = null;
                }
            }

            // no work to do
            if (previous == selected)
            {
                return;
            }

            RegenerateItems(selected);
            previous = selected;
        }

        private void ClearItems()
        {
            ClearPanesAndData();

            ScriptNotesWindow wnd = GetWindow<ScriptNotesWindow>();
            wnd.titleContent.text = "Script Notes";
            leftPane.RefreshItems();
        }

        private void ClearPanesAndData()
        {
            loadedNotes.Clear();
            rightPane.Clear();
            leftPane.ClearSelection();
            processedScripts.Clear();
        }

        private void RegenerateItems(GameObject selectedObj)
        {
            ClearPanesAndData();

            // Try to read the notes, cache behaviour copies
            var monoBehaviours = selectedObj.GetComponents<MonoBehaviour>();
            foreach (var mono in monoBehaviours)
            {
                var monoType = mono.GetType();
                if (processedScripts.Contains(monoType.Name))
                {
                    continue;
                }

                var scriptNoteAttribute = GetScriptNoteAttribute(monoType);
                if (scriptNoteAttribute == null)
                {
                    continue;
                }

                var scriptNote = new ScriptNote();
                scriptNote.ScriptName = monoType.Name;
                scriptNote.Note = scriptNoteAttribute.Note;
                loadedNotes.Add(scriptNote);

                processedScripts.Add(monoType.Name);
            }

            ScriptNotesWindow wnd = GetWindow<ScriptNotesWindow>();
            wnd.titleContent.text = selectedObj.name + " Notes";
            leftPane.RefreshItems();
        }

        private void OnMonoSelectionChange(IEnumerable<object> selectedItems)
        {
            rightPane.Clear();

            using var enumerator = selectedItems.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return;
            }

            if (enumerator.Current is not ScriptNote selected)
            {
                return;
            }

            var label = new Label()
            {
                style =
                {
                    fontSize = 14,
                    // autowrap
                    whiteSpace = WhiteSpace.Normal
                }
            };
            label.text = selected.Note;
            rightPane.Add(label);
        }

        private ScriptNoteAttribute GetScriptNoteAttribute(Type type)
        {
            Attribute[] attrs = Attribute.GetCustomAttributes(type);
            foreach (Attribute attr in attrs)
            {
                if (attr is ScriptNoteAttribute a)
                {
                    return a;
                }
            }

            return null;
        }

        private static string PascalCaseToSpaced(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // insert spaces before uppercase letters
            return Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
        }
    }
}