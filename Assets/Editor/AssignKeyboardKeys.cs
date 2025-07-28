using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace LightingMidiPiano.Editor
{
    public class AssignKeyboardKeys : EditorWindow
    {
        public KeyboardView targetController;
        public GameObject keyboardParent;
        public string keyNamePrefix = "Key.";

        [MenuItem("Tools/Assign Keyboard Keys")]
        public static void ShowWindow()
        {
            GetWindow<AssignKeyboardKeys>("Assign Keyboard Keys");
        }

        void OnGUI()
        {
            GUILayout.Label("Assign Keyboard Keys to Controller", EditorStyles.boldLabel);

            targetController = (KeyboardView)EditorGUILayout.ObjectField(
                "Target Controller", targetController, typeof(KeyboardView), true);

            keyboardParent = (GameObject)EditorGUILayout.ObjectField(
                "Keyboard Parent", keyboardParent, typeof(GameObject), true);

            keyNamePrefix = EditorGUILayout.TextField("Key Name Prefix", keyNamePrefix);

            if (GUILayout.Button("Assign Keys"))
            {
                AssignKeys();
            }
        }

        private void AssignKeys()
        {
            if (targetController == null)
            {
                Debug.LogError("Target VirtualKeyboardController is not assigned.");
                return;
            }

            if (keyboardParent == null)
            {
                Debug.LogError("Keyboard Parent object is not assigned. Please drag the parent of your keyboard keys here.");
                return;
            }

            List<GameObject> sortedKeys = keyboardParent.GetComponentsInChildren<Transform>(true)
                                             .Where(t => t.gameObject.name.StartsWith(keyNamePrefix))
                                             .Select(t => t.gameObject)
                                             .OrderBy(go =>
                                             {
                                                 string numPart = go.name.Substring(keyNamePrefix.Length);
                                                 if (int.TryParse(numPart, out int number))
                                                 {
                                                     return number;
                                                 }
                                                 return -1;
                                             })
                                             .ToList();

            if (sortedKeys.Count == 0)
            {
                Debug.LogWarning($"No keyboard keys found with prefix '{keyNamePrefix}' under the Keyboard Parent object. " +
                                 "Please check your Key Name Prefix and parent assignment.");
                return;
            }

            targetController.KeyboardKeys = new GameObject[sortedKeys.Count];

            for (int i = 0; i < sortedKeys.Count; i++)
            {
                targetController.KeyboardKeys[i] = sortedKeys[i];
            }

            Debug.Log($"Successfully assigned {sortedKeys.Count} keyboard keys to VirtualKeyboardController.");

            EditorUtility.SetDirty(targetController);
            AssetDatabase.SaveAssets();
        }
    }
}
