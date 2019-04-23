using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AugmentaCamera))]
public class AugmentaCameraEditor : Editor
{
	SerializedProperty _cameraType;
	SerializedProperty _zoom;
	SerializedProperty _drawNearCone;
	SerializedProperty _drawFrustum;
	SerializedProperty _centerOnAugmentaArea;
	SerializedProperty _lookTarget;

	private void OnEnable() {

		_cameraType = serializedObject.FindProperty("cameraType");
		_zoom = serializedObject.FindProperty("zoom");
		_drawNearCone = serializedObject.FindProperty("drawNearCone");
		_drawFrustum = serializedObject.FindProperty("drawFrustum");
		_centerOnAugmentaArea = serializedObject.FindProperty("centerOnAugmentaArea");
		_lookTarget = serializedObject.FindProperty("lookTarget");
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(_cameraType, new GUIContent("Camera Type"));

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Augmenta Camera Settings", EditorStyles.boldLabel);

		EditorGUILayout.PropertyField(_zoom, new GUIContent("Zoom"));
		EditorGUILayout.PropertyField(_drawNearCone, new GUIContent("Draw Near Cone"));
		EditorGUILayout.PropertyField(_drawFrustum, new GUIContent("Draw Frustum"));
		EditorGUILayout.PropertyField(_centerOnAugmentaArea, new GUIContent("Center On Augmenta Area"));
		EditorGUILayout.PropertyField(_lookTarget, new GUIContent("Look Target"));

		serializedObject.ApplyModifiedProperties();
	}
}
