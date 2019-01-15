using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaCamera : AugmentaCameraAnchor
{
    public delegate void CameraUpdated(AugmentaCameraAnchor settings);
    public event CameraUpdated cameraUpdated;

    void Awake()
    {
        updateCameraOnStart = false;
        updateTransformOnStart = false;
        updatePostProcessOnStart = false;
        alwaysUpdateCamera = false;
        alwaysUpdateTransform = false;
        alwaysUpdatePostProcess = false;
        disableAfterUpdate = false;

        TargetCameraName = gameObject.name;
    }

    public void UpdateCameraSettings(AugmentaCameraAnchor augmentaCamera)
    {

        //Don't update self
        if (augmentaCamera.gameObject == gameObject)
            return;

        linkedAugmentaArea.Zoom = augmentaCamera.Zoom;

        //sourceCamera.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, augmentaCamera.transform.localPosition.z);

        augmentaCamera.gameObject.GetComponent<Camera>().enabled = false;

        if (cameraUpdated != null)
            cameraUpdated(augmentaCamera);
    }

    /*
    //The width of the screen
    public float Width
    {
        get
        {
            return (BottomRightCorner - BottomLeftCorner).magnitude;
        }
        set
        {
            Vector3 vecWidth = BottomRightCorner - BottomLeftCorner;
            float scale = value / vecWidth.magnitude;
            vecWidth *= (1 - scale);

            TopLeftCorner += vecWidth / 2;
            BottomLeftCorner += vecWidth / 2;
            BottomRightCorner -= vecWidth / 2;
        }
    }

    //The height of the screen
    public float Height
    {
        get
        {
            return (TopLeftCorner - BottomLeftCorner).magnitude;
        }
        set
        {
            Vector3 vecHeight = TopLeftCorner - BottomLeftCorner;
            float scale = value / vecHeight.magnitude;
            vecHeight *= (1 - scale);

            TopLeftCorner -= vecHeight / 2;
            BottomLeftCorner += vecHeight / 2;
            BottomRightCorner += vecHeight / 2;
        }
    }
    */

}
