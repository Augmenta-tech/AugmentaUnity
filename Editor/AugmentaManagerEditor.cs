using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Augmenta
{
    [CustomEditor(typeof(AugmentaManager))]
    public class AugmentaManagerEditor : Editor
    {

        SerializedProperty augmentaId;
        SerializedProperty inputPort;
        SerializedProperty protocolVersion;
        SerializedProperty pixelSize;
        SerializedProperty scaling;
        SerializedProperty flipX;
        SerializedProperty flipY;
        SerializedProperty augmentaObjectTimeOut;
        SerializedProperty desiredAugmentaObjectType;
        SerializedProperty desiredAugmentaObjectCount;
        SerializedProperty augmentaScenePrefab;
        SerializedProperty augmentaObjectPrefab;
        SerializedProperty mute;
        SerializedProperty showDebug;

        void OnEnable() {

            augmentaId = serializedObject.FindProperty("augmentaId");
            inputPort = serializedObject.FindProperty("_inputPort");
            protocolVersion = serializedObject.FindProperty("protocolVersion");
            pixelSize = serializedObject.FindProperty("pixelSize");
            scaling = serializedObject.FindProperty("scaling");
            flipX = serializedObject.FindProperty("flipX");
            flipY = serializedObject.FindProperty("flipY");
            augmentaObjectTimeOut = serializedObject.FindProperty("augmentaObjectTimeOut");
            desiredAugmentaObjectType = serializedObject.FindProperty("desiredAugmentaObjectType");
            desiredAugmentaObjectCount = serializedObject.FindProperty("desiredAugmentaObjectCount");
            augmentaScenePrefab = serializedObject.FindProperty("augmentaScenePrefab");
            augmentaObjectPrefab = serializedObject.FindProperty("augmentaObjectPrefab");
            mute = serializedObject.FindProperty("mute");
            showDebug = serializedObject.FindProperty("showDebug");
        }

        public override void OnInspectorGUI() {

            AugmentaManager augmentaManager = target as AugmentaManager;

            serializedObject.Update();

            EditorGUILayout.LabelField("AUGMENTA ID", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(augmentaId, new GUIContent("Augmenta ID"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("OSC SETTINGS", EditorStyles.boldLabel);
            //Input port change handling
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(inputPort, new GUIContent("Input Port"));
            if (EditorGUI.EndChangeCheck() && Application.isPlaying) {
                serializedObject.ApplyModifiedProperties(); 
                augmentaManager.CreateAugmentaOSCReceiver(); 
            }

            EditorGUILayout.PropertyField(protocolVersion, new GUIContent("Protocol Version"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("AUGMENTA SCENE SETTINGS", EditorStyles.boldLabel);
            if (protocolVersion.enumValueIndex == 0) {
                EditorGUILayout.PropertyField(pixelSize, new GUIContent("Pixel Size"));
            }
            EditorGUILayout.PropertyField(scaling, new GUIContent("Scaling"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("AUGMENTA OBJECTS SETTINGS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(flipX, new GUIContent("Flip X"));
            EditorGUILayout.PropertyField(flipY, new GUIContent("Flip Y"));
            EditorGUILayout.PropertyField(augmentaObjectTimeOut, new GUIContent("Augmenta Object TimeOut"));
            EditorGUILayout.PropertyField(desiredAugmentaObjectType, new GUIContent("Desired Augmenta Object Type"));
            if(desiredAugmentaObjectType.enumValueIndex > 0) {
                EditorGUILayout.PropertyField(desiredAugmentaObjectCount, new GUIContent("Desired Augmenta Object Count"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("AUGMENTA PREFABS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(augmentaScenePrefab, new GUIContent("Augmenta Scene Prefab"));
            EditorGUILayout.PropertyField(augmentaObjectPrefab, new GUIContent("Augmenta Object Prefab"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("DEBUG", EditorStyles.boldLabel);
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
