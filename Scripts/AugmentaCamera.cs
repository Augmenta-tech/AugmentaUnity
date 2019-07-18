using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An AugmentaCamera is linked to an AugmentaArea and is designed to facilitate the rendering of the augmenta data. 
/// For example it can be set to always adapt its field of view to match exactly the AugmentaArea size through different camera types (orthographic, perpective or offcenter). 
/// You can still add and use your own cameras to the scene if the AugmentaCamera does not suit your needs.
/// </summary>

public class AugmentaCamera : AugmentaCameraAnchor
{
    public override void Awake()
    {
        updateCameraOnStart = false;
        updateTransformOnStart = false;
        updatePostProcessOnStart = false;
        alwaysUpdateCamera = false;
        alwaysUpdateTransform = false;
        alwaysUpdatePostProcess = false;
        disableAfterUpdate = false;

        //targetCamera = GetComponent<Camera>();

		base.Awake();
    }
}
