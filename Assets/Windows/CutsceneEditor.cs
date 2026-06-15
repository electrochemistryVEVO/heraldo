using System;
using Heraldo.Cutscenes;
using UnityEditor;
using UnityEngine;

namespace Windows
{
    [CustomEditor(typeof(Cutscene))]
    public class CutsceneEditor : Editor
    {
        public Cutscene cutscene; 

        private void OnInspectorGUI()
        {
            cutscene = EditorGUILayout.ObjectField("Cutscene", cutscene, typeof(Cutscene), false) as Cutscene;
            if (cutscene == null) return;

            var cutsceneSo = new SerializedObject(cutscene);
            var partsList = cutsceneSo.FindProperty("parts");
            for (int i = 0; i < partsList.arraySize; i++)
            {
                var partsSo = partsList.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                //if (partsSo.objectReferenceValue == null) continue;
                EditorGUILayout.LabelField("Element "+i); 
                EditorGUILayout.PropertyField(partsSo.FindPropertyRelative("author"));
                EditorGUILayout.PropertyField(partsSo.FindPropertyRelative("subjectName"));
                EditorGUILayout.PropertyField(partsSo.FindPropertyRelative("subjectPos"));
                EditorGUILayout.PropertyField(partsSo.FindPropertyRelative("text"));
                EditorGUILayout.PropertyField(partsSo.FindPropertyRelative("soundbyteId"));
                EditorGUILayout.PropertyField(partsSo.FindPropertyRelative("time"));
                EditorGUILayout.EndHorizontal();
            }
            //EditorGUILayout.PropertyField(cutsceneSo.FindProperty("parts"));
            

            if (GUILayout.Button("Add Parts"))
            {
                cutscene.parts.Add(new CutscenePart());
                EditorUtility.SetDirty(cutscene);
            }
            
            if (GUILayout.Button("Save"))
            { 

                // Call SetDirty to ensure the change is saved
                EditorUtility.SetDirty(cutscene);
                // Optional: AssetDatabase.SaveAssets(); // To save immediately
            }
        }
    }
}