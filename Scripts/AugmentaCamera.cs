using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        targetCameraObject = gameObject;

		base.Awake();
    }
}
