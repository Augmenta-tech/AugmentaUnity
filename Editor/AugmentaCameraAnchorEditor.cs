using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AugmentaCameraAnchor))]
public class AugmentaCameraAnchorEditor : Editor
{
	SerializedProperty _updateCameraOnStart;
	SerializedProperty _updateTransformOnStart;
	SerializedProperty _updatePostProcessOnStart;

	SerializedProperty _alwaysUpdateCamera;
	SerializedProperty _alwaysUpdateTransform;
	SerializedProperty _alwaysUpdatePostProcess;

	SerializedProperty _disableAfterUpdate;

	SerializedProperty _cameraType;
	SerializedProperty _centerOnAugmentaArea;
	SerializedProperty _lookTarget;

	private void OnEnable() {

		_updateCameraOnStart = serializedObject.FindProperty("updateCameraOnStart");
		_updateTransformOnStart = serializedObject.FindProperty("updateTransformOnStart");
		_updatePostProcessOnStart = serializedObject.FindProperty("updatePostProcessOnStart");

		_alwaysUpdateCamera = serializedObject.FindProperty("alwaysUpdateCamera");
		_alwaysUpdateTransform = serializedObject.FindProperty("alwaysUpdateTransform");
		_alwaysUpdatePostProcess = serializedObject.FindProperty("alwaysUpdatePostProcess");

		_disableAfterUpdate = serializedObject.FindProperty("disableAfterUpdate");

		_cameraType = serializedObject.FindProperty("cameraType");
		_centerOnAugmentaArea = serializedObject.FindProperty("centerOnAugmentaArea");
		_lookTarget = serializedObject.FindProperty("lookTarget");
	}

	public override void OnInspectorGUI()
    {
		serializedObject.Update();

		EditorGUILayout.Space();
        EditorGUILayout.LabelField("What to update on start ?", EditorStyles.boldLabel);

		EditorGUILayout.PropertyField(_updateCameraOnStart, new GUIContent("Camera", "Copy the camera parameters on start."));
		EditorGUILayout.PropertyField(_updateTransformOnStart, new GUIContent("Transform", "Copy the transform parameters on start."));
		EditorGUILayout.PropertyField(_updatePostProcessOnStart, new GUIContent("PostProcess", "Copy the post process layer parameters on start."));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("What to update every frame ?", EditorStyles.boldLabel);

		EditorGUILayout.PropertyField(_alwaysUpdateCamera, new GUIContent("Camera", "Copy the camera parameters every frame."));
		EditorGUILayout.PropertyField(_alwaysUpdateTransform, new GUIContent("Transform", "Copy the transform parameters every frame."));
		EditorGUILayout.PropertyField(_alwaysUpdatePostProcess, new GUIContent("PostProcess", "Copy the post process layers parameters every frame."));

        EditorGUILayout.Space();
		EditorGUILayout.PropertyField(_disableAfterUpdate, new GUIContent("Disable After Update", "Disable the anchor camera after every update."));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Augmenta Camera Settings", EditorStyles.boldLabel);

		EditorGUILayout.PropertyField(_cameraType, new GUIContent("Camera Type"));
		EditorGUILayout.PropertyField(_centerOnAugmentaArea, new GUIContent("Center On Augmenta Area"));
		EditorGUILayout.PropertyField(_lookTarget, new GUIContent("Look Target"));

		serializedObject.ApplyModifiedProperties();
	}
}
