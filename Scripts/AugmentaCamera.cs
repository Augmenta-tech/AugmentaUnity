using Augmenta;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A camera that can will always adapt its field of view to perfectly match the Augmenta scene.
/// It can be orthographic, perspective or offCenter.
/// If it is orthographic or perspective, it should be centered on the Augmenta scene for the field of view to match the scene perfectly.
/// </summary>
[RequireComponent(typeof(Camera))]
public class AugmentaCamera : MonoBehaviour
{
    public AugmentaManager augmentaManager;

    public enum CameraType { Orthographic, Perspective, OffCenter };
    public CameraType cameraType = CameraType.Perspective;

    private Vector3 botLeftCorner;
    private Vector3 botRightCorner;
    private Vector3 topLeftCorner;
    private Vector3 topRightCorner;

    private new Camera camera;

    private void Start() {

        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        //Don't update if there is no Augmenta scene
        if (!augmentaManager.augmentaScene)
            return;

        //Don't update if Augmenta scene size is 0
        if (augmentaManager.augmentaScene.width <= 0 || augmentaManager.augmentaScene.height <= 0)
            return;

        UpdateAugmentaSceneCorners();

        switch (cameraType) {

            case CameraType.Orthographic:
                ComputeOrthoCamera();
                break;

            case CameraType.Perspective:
                ComputePerspectiveCamera();
                break;

            case CameraType.OffCenter:
                ComputeOffCenterCamera();
                break;

        }
    }

    /// <summary>
    /// Update the positions of the Augmenta scene corners
    /// </summary>
    void UpdateAugmentaSceneCorners() {
        botLeftCorner = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0));
        botRightCorner = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(new Vector3(0.5f, -0.5f, 0));
        topLeftCorner = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0));
        topRightCorner = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(new Vector3(0.5f, 0.5f, 0));
    }

    #region Camera Update Functions

    void ComputeOrthoCamera() {
        camera.orthographic = true;
        camera.aspect = augmentaManager.augmentaScene.width / augmentaManager.augmentaScene.height;
        camera.orthographicSize = augmentaManager.augmentaScene.debugObject.transform.localScale.y * 0.5f;

        camera.ResetProjectionMatrix();
    }

    void ComputePerspectiveCamera() {

        camera.orthographic = false;
        camera.ResetProjectionMatrix();

        if (augmentaManager.protocolVersion == AugmentaProtocolVersion.V1) {
            camera.fieldOfView = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(augmentaManager.augmentaScene.height * 0.5f * augmentaManager.pixelSize * augmentaManager.scaling, transform.localPosition.y);
        } else if (augmentaManager.protocolVersion == AugmentaProtocolVersion.V2) {
            camera.fieldOfView = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(augmentaManager.augmentaScene.height * 0.5f * augmentaManager.scaling, transform.localPosition.y);
        }

        camera.aspect = augmentaManager.augmentaScene.width / augmentaManager.augmentaScene.height;

    }

    void ComputeOffCenterCamera() {
        camera.orthographic = false;
        camera.ResetAspect();

        Vector3 pa, pb, pc, pd;
        pa = botLeftCorner; //Bottom-Left
        pb = botRightCorner; //Bottom-Right
        pc = topLeftCorner; //Top-Left
        pd = topRightCorner; //Top-Right

        Vector3 pe = camera.transform.position;// eye position

        Vector3 vr = (pb - pa).normalized; // right axis of screen
        Vector3 vu = (pc - pa).normalized; // up axis of screen
        Vector3 vn = Vector3.Cross(vr, vu).normalized; // normal vector of screen

        Vector3 va = pa - pe; // from pe to pa
        Vector3 vb = pb - pe; // from pe to pb
        Vector3 vc = pc - pe; // from pe to pc
        Vector3 vd = pd - pe; // from pe to pd

        float n = camera.nearClipPlane; // distance to the near clip plane (screen)
        float f = camera.farClipPlane; // distance of far clipping plane
        float d = Vector3.Dot(va, vn); // distance from eye to screen
        float l = Vector3.Dot(vr, va) * n / d; // distance to left screen edge from the 'center'
        float r = Vector3.Dot(vr, vb) * n / d; // distance to right screen edge from 'center'
        float b = Vector3.Dot(vu, va) * n / d; // distance to bottom screen edge from 'center'
        float t = Vector3.Dot(vu, vc) * n / d; // distance to top screen edge from 'center'

        Matrix4x4 p = new Matrix4x4(); // Projection matrix
        p[0, 0] = 2.0f * n / (r - l);
        p[0, 2] = (r + l) / (r - l);
        p[1, 1] = 2.0f * n / (t - b);
        p[1, 2] = (t + b) / (t - b);
        p[2, 2] = (f + n) / (n - f);
        p[2, 3] = 2.0f * f * n / (n - f);
        p[3, 2] = -1.0f;

        try {
            camera.projectionMatrix = p; // Assign matrix to camera
        } catch (Exception e) {
            Debug.LogWarning("Frustrum error, matrix invalid : " + e.Message);
        }
    }

    #endregion
}
