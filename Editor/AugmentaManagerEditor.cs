using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Augmenta
{
    [CustomEditor(typeof(AugmentaManager))]
    public class AugmentaManagerEditor : Editor
    {

        SerializedProperty id;
        SerializedProperty inputPort;
        SerializedProperty scaling;
        SerializedProperty flipX;
        SerializedProperty flipY;
        SerializedProperty personTimeOut;
        SerializedProperty desiredPersonType;
        SerializedProperty desiredPersonCount;
        SerializedProperty augmentaScenePrefab;
        SerializedProperty augmentaPersonPrefab;
        SerializedProperty mute;
        SerializedProperty showDebug;

        void OnEnable() {

            id = serializedObject.FindProperty("id");
            inputPort = serializedObject.FindProperty("_inputPort");
            scaling = serializedObject.FindProperty("scaling");
            flipX = serializedObject.FindProperty("flipX");
            flipY = serializedObject.FindProperty("flipY");
            personTimeOut = serializedObject.FindProperty("personTimeOut");
            desiredPersonType = serializedObject.FindProperty("desiredPersonType");
            desiredPersonCount = serializedObject.FindProperty("desiredPersonCount");
            augmentaScenePrefab = serializedObject.FindProperty("augmentaScenePrefab");
            augmentaPersonPrefab = serializedObject.FindProperty("augmentaPersonPrefab");
            mute = serializedObject.FindProperty("mute");
            showDebug = serializedObject.FindProperty("showDebug");
        }

        public override void OnInspectorGUI() {

            AugmentaManager augmentaManager = target as AugmentaManager;

            serializedObject.Update();

            EditorGUILayout.PropertyField(id, new GUIContent("ID"));

            //Input port change handling
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(inputPort, new GUIContent("Input Port"));
            if (EditorGUI.EndChangeCheck() && Application.isPlaying) {
                serializedObject.ApplyModifiedProperties(); 
                augmentaManager.CreateAugmentaOSCReceiver(); 
            }

            EditorGUILayout.PropertyField(scaling, new GUIContent("Scaling"));
            EditorGUILayout.PropertyField(flipX, new GUIContent("Flip X"));
            EditorGUILayout.PropertyField(flipY, new GUIContent("Flip Y"));
            EditorGUILayout.PropertyField(personTimeOut, new GUIContent("Person TimeOut"));
            EditorGUILayout.PropertyField(desiredPersonType, new GUIContent("Desired Person Type"));
            if(desiredPersonType.enumValueIndex > 0) {
                EditorGUILayout.PropertyField(desiredPersonCount, new GUIContent("Desired Person Count"));
            }

            EditorGUILayout.PropertyField(augmentaScenePrefab, new GUIContent("Augmenta Scene Prefab"));
            EditorGUILayout.PropertyField(augmentaPersonPrefab, new GUIContent("Augmenta Person Prefab"));

            EditorGUILayout.PropertyField(mute, new GUIContent("Mute"));

            //Show debug change handling
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(showDebug, new GUIContent("Show Debug"));
            if (EditorGUI.EndChangeCheck() && Application.isPlaying) {
                serializedObject.ApplyModifiedProperties();
                augmentaManager.ShowDebug(showDebug.boolValue);
            }

            serializedObject.ApplyModifiedProperties();

        }
    }
}
