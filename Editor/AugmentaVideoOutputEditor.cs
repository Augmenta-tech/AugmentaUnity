using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Augmenta
{
    [CustomEditor(typeof(AugmentaVideoOutput))]
    public class AugmentaVideoOutputEditor : Editor
    {
        SerializedProperty augmentaManager;
        SerializedProperty augmentaVideoOutputCamera;

        SerializedProperty autoOutputSizeInPixels;
        SerializedProperty autoOutputSizeInMeters;
        SerializedProperty autoOutputOffset;

        SerializedProperty videoOutputSizeInPixels;
        SerializedProperty videoOutputSizeInMeters;
        SerializedProperty videoOutputOffset;

        SerializedProperty videoOutputTexture;

        void OnEnable() {

            augmentaManager = serializedObject.FindProperty("augmentaManager");
            augmentaVideoOutputCamera = serializedObject.FindProperty("augmentaVideoOutputCamera");

            autoOutputSizeInPixels = serializedObject.FindProperty("autoOutputSizeInPixels");
            autoOutputSizeInMeters = serializedObject.FindProperty("autoOutputSizeInMeters");
            autoOutputOffset = serializedObject.FindProperty("autoOutputOffset");

            videoOutputSizeInPixels = serializedObject.FindProperty("_videoOutputSizeInPixels");
            videoOutputSizeInMeters = serializedObject.FindProperty("_videoOutputSizeInMeters");
            videoOutputOffset = serializedObject.FindProperty("_videoOutputOffset");

            videoOutputTexture = serializedObject.FindProperty("videoOutputTexture");
        }

        public override void OnInspectorGUI() {

            AugmentaVideoOutput augmentaVideoOutput = target as AugmentaVideoOutput;

            serializedObject.Update();

            EditorGUILayout.LabelField("AUGMENTA COMPONENTS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(augmentaManager, new GUIContent("Augmenta Manager"));
            EditorGUILayout.PropertyField(augmentaVideoOutputCamera, new GUIContent("Augmenta Video Output Camera"));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("VIDEO OUTPUT SETTINGS", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(autoOutputSizeInPixels, new GUIContent("Auto Size Output in Pixels", "Use data from Fusion to determine the output size in pixels."));

            if (!autoOutputSizeInPixels.boolValue) {
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(videoOutputSizeInPixels, new GUIContent("Output Size in Pixels"));
				if (EditorGUI.EndChangeCheck() && Application.isPlaying) {
                    serializedObject.ApplyModifiedProperties();
                    augmentaVideoOutput.RefreshVideoTexture();
				}

				EditorGUILayout.Space();
            }

            EditorGUILayout.PropertyField(autoOutputSizeInMeters, new GUIContent("Auto Size Output in Meters", "Use data from Fusion to determine the output size in meters."));

            if (!autoOutputSizeInMeters.boolValue) {
                EditorGUILayout.PropertyField(videoOutputSizeInMeters, new GUIContent("Output Size in Meters"));

                EditorGUILayout.Space();
            }

            EditorGUILayout.PropertyField(autoOutputOffset, new GUIContent("Auto Output Offset", "Use data from Fusion to determine the output offset."));

            if (!autoOutputOffset.boolValue) {
                EditorGUILayout.PropertyField(videoOutputOffset, new GUIContent("Output Offset"));
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.LabelField("DEBUG (Read only)", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(videoOutputTexture, new GUIContent("Video Output Texture"));

        }
    }
}
