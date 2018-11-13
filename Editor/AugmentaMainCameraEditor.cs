using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AugmentaMainCamera))]
public class AugmentaMainCameraEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AugmentaMainCamera augmentaMainCamera = (AugmentaMainCamera)target;

        EditorGUILayout.LabelField("Augmenta Camera Settings", EditorStyles.boldLabel);

        augmentaMainCamera.Zoom = EditorGUILayout.FloatField("Zoom", augmentaMainCamera.Zoom);
        augmentaMainCamera.NearFrustrum = EditorGUILayout.FloatField("Near Frustrum", augmentaMainCamera.NearFrustrum);
        augmentaMainCamera.drawNearCone = EditorGUILayout.Toggle("Draw Near Cone", augmentaMainCamera.drawNearCone);
        augmentaMainCamera.drawFrustum = EditorGUILayout.Toggle("Draw Frustum", augmentaMainCamera.drawFrustum);
        augmentaMainCamera.centerOnAugmentaArea = EditorGUILayout.Toggle("Center On Augmenta Area", augmentaMainCamera.centerOnAugmentaArea);
        augmentaMainCamera.lookTarget = (Transform)EditorGUILayout.ObjectField("Look Target", augmentaMainCamera.lookTarget, typeof(Transform), true);
    }
}
