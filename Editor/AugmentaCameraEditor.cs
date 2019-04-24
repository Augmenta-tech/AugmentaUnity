using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AugmentaCamera))]
public class AugmentaCameraEditor : Editor
{
	SerializedProperty _cameraType;
	SerializedProperty _centerOnAugmentaArea;
	SerializedProperty _lookTarget;

	private void OnEnable() {

		_cameraType = serializedObject.FindProperty("cameraType");
		_centerOnAugmentaArea = serializedObject.FindProperty("centerOnAugmentaArea");
		_lookTarget = serializedObject.FindProperty("lookTarget");
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(_cameraType, new GUIContent("Camera Type"));

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Augmenta Camera Settings", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(_centerOnAugmentaArea, new GUIContent("Center On Augmenta Area"));
		EditorGUILayout.PropertyField(_lookTarget, new GUIContent("Look Target"));

		serializedObject.ApplyModifiedProperties();
	}
}
