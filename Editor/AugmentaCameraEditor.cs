using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AugmentaCamera))]
public class AugmentaCameraEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AugmentaCamera augmentaMainCamera = (AugmentaCamera)target;

        EditorGUILayout.LabelField("Augmenta Camera Settings", EditorStyles.boldLabel);

        //augmentaMainCamera.augmentaAreaAnchor = (AugmentaAreaAnchor)EditorGUILayout.ObjectField("Augmenta area anchor", augmentaMainCamera.augmentaAreaAnchor, typeof(AugmentaAreaAnchor), false);
        augmentaMainCamera.linkedAugmentaArea = (AugmentaArea)EditorGUILayout.ObjectField("Augmenta Area", augmentaMainCamera.linkedAugmentaArea, typeof(AugmentaArea), true);
        augmentaMainCamera.Zoom = EditorGUILayout.FloatField("Zoom", augmentaMainCamera.Zoom);
        //augmentaMainCamera.NearFrustrum = EditorGUILayout.FloatField("Near Frustrum", augmentaMainCamera.NearFrustrum);
        augmentaMainCamera.drawNearCone = EditorGUILayout.Toggle("Draw Near Cone", augmentaMainCamera.drawNearCone);
        augmentaMainCamera.drawFrustum = EditorGUILayout.Toggle("Draw Frustum", augmentaMainCamera.drawFrustum);
        augmentaMainCamera.centerOnAugmentaArea = EditorGUILayout.Toggle("Center On Augmenta Area", augmentaMainCamera.centerOnAugmentaArea);
        augmentaMainCamera.lookTarget = (Transform)EditorGUILayout.ObjectField("Look Target", augmentaMainCamera.lookTarget, typeof(Transform), true);

        Undo.RecordObject(target, "Changed augmentaCamera");
    }
}
