using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class AugmentaCamera : CopyCameraToTargetCamera {

    [Header("Augmenta Area Anchor")]
    public bool useAnchor = false;
    public AugmentaAreaAnchor augmentaAreaAnchor;

    [Header("Augmenta Settings")]
    [Tooltip("Should this camera Augmenta settings be copied to the augmenta camera on start ?")]
    public bool updateAugmentaOnStart = true;

    [Tooltip("Should this camera Augmenta settings be copied to the augmenta camera at each frame ?")]
    public bool alwaysUpdateAugmenta = false;

    public float Zoom = 1;

    public float NearFrustrum = 0.01f;
    public bool drawNearCone, drawFrustum;
    public bool centerOnAugmentaArea;

    private Vector3 BottomLeftCorner;
    private Vector3 BottomRightCorner;
    private Vector3 TopLeftCorner;
    private Vector3 TopRightCorner;
    public Transform lookTarget;

    // Use this for initialization
    public virtual void Start () {

        UpdateTargetCamera(updateTransformOnStart, updateCameraOnStart, updatePostProcessOnStart && hasPostProcessLayer);

        if (updateAugmentaOnStart)
            CopyAugmentaSettings();
    }

    public void ForceCoreCameraUpdate()
    {
        UpdateTargetCamera(false, true, true);
        CopyAugmentaSettings();
    }

    void Update()
    {
        base.UpdateTargetCamera(alwaysUpdateTransform, alwaysUpdateCamera, alwaysUpdatePostProcess && hasPostProcessLayer);

        UpdateAugmentaAreaCorners();

        if (centerOnAugmentaArea)
        {
            sourceCamera.transform.localPosition = new Vector3(0, 0, transform.localPosition.z);
        }
        else
        {
            sourceCamera.transform.localPosition = new Vector3(sourceCamera.transform.localPosition.x, sourceCamera.transform.localPosition.y, transform.localPosition.z);
        }

        //Don't update camera with a 0 sized anchor
        if (useAnchor && (augmentaAreaAnchor.Width == 0 || augmentaAreaAnchor.Height == 0))
            return;

        //Don't update camera with a 0 sized AugmentaArea
        if (!useAnchor && (AugmentaArea.AugmentaScene.Width == 0 || AugmentaArea.AugmentaScene.Height == 0))
            return;

        if (sourceCamera.orthographic)
        {
            ComputeOrthoCamera();
        }
        else
        {
            ComputePerspectiveCamera();
        }
    }

    void UpdateAugmentaAreaCorners()
    {
        if (useAnchor)
        {
            BottomLeftCorner = augmentaAreaAnchor.transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0));
            BottomRightCorner = augmentaAreaAnchor.transform.TransformPoint(new Vector3(0.5f, 0.5f, 0));
            TopLeftCorner = augmentaAreaAnchor.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0));
            TopRightCorner = augmentaAreaAnchor.transform.TransformPoint(new Vector3(0.5f, -0.5f, 0));
        }
        else
        {
            BottomLeftCorner = AugmentaArea.Instance.transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0));
            BottomRightCorner = AugmentaArea.Instance.transform.TransformPoint(new Vector3(0.5f, 0.5f, 0));
            TopLeftCorner = AugmentaArea.Instance.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0));
            TopRightCorner = AugmentaArea.Instance.transform.TransformPoint(new Vector3(0.5f, -0.5f, 0));
        }
    }

    
    public override void UpdateTargetCamera(bool updateTransform, bool updateCamera, bool updatePostProcess)
    {
        base.UpdateTargetCamera(updateTransform, updateCamera, updatePostProcess && hasPostProcessLayer);

        if(alwaysUpdateAugmenta)
            CopyAugmentaSettings();
    }

    private void CopyAugmentaSettings()
    {
        if(AugmentaMainCamera.Instance)
            AugmentaMainCamera.Instance.UpdateCameraSettings(this);
    }

    void ComputeOrthoCamera()
    {
        if (useAnchor)
        {
            sourceCamera.aspect = augmentaAreaAnchor.Width / augmentaAreaAnchor.Height;
            sourceCamera.orthographicSize = augmentaAreaAnchor.transform.localScale.y / 2;
        } else
        {
            sourceCamera.aspect = AugmentaArea.Instance.AspectRatio;
            sourceCamera.orthographicSize = AugmentaArea.Instance.transform.localScale.y / 2;
        }
        sourceCamera.ResetProjectionMatrix();
    }

    void ComputePerspectiveCamera()
    {
        if (centerOnAugmentaArea) {
            sourceCamera.transform.localPosition = new Vector3(0.0f, 0.0f, transform.localPosition.z);
        }
        
        if (useAnchor)
        {
            sourceCamera.fieldOfView = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(augmentaAreaAnchor.Height * 0.5f * augmentaAreaAnchor.MeterPerPixel * Zoom, transform.localPosition.z);
            sourceCamera.aspect = augmentaAreaAnchor.Width / augmentaAreaAnchor.Height;
        } else
        {
            sourceCamera.fieldOfView = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(AugmentaArea.AugmentaScene.Height * 0.5f * AugmentaArea.Instance.MeterPerPixel * Zoom, transform.localPosition.z);
            sourceCamera.aspect = AugmentaArea.AugmentaScene.Width / AugmentaArea.AugmentaScene.Height;
        }
    }

    void ComputeOffCenterCamera()
    {
        sourceCamera.ResetAspect();

        Vector3 pa, pb, pc, pd;
        pa = BottomLeftCorner; //Bottom-Left
        pb = BottomRightCorner; //Bottom-Right
        pc = TopLeftCorner; //Top-Left
        pd = TopRightCorner; //Top-Right

        Vector3 pe = sourceCamera.transform.position;// eye position

        Vector3 vr = (pb - pa).normalized; // right axis of screen
        Vector3 vu = (pc - pa).normalized; // up axis of screen
        Vector3 vn = Vector3.Cross(vr, vu).normalized; // normal vector of screen

        Vector3 va = pa - pe; // from pe to pa
        Vector3 vb = pb - pe; // from pe to pb
        Vector3 vc = pc - pe; // from pe to pc
        Vector3 vd = pd - pe; // from pe to pd

        float n = lookTarget.InverseTransformPoint(sourceCamera.transform.position).z; // distance to the near clip plane (screen)
        float f = sourceCamera.farClipPlane; // distance of far clipping plane
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
        p[2, 3] = 2.0f * f * n / (n - f) * NearFrustrum;
        p[3, 2] = -1.0f;

        if (centerOnAugmentaArea)
        {
            p[0, 2] = 0.0f;
            p[1, 2] = 0.0f;
        }

        try
        {
            sourceCamera.projectionMatrix = p; // Assign matrix to camera
        }
        catch (Exception e)
        {
            Debug.LogWarning("Frustrum error, matrix invalid : " + e.Message);
        }

        if (drawNearCone)
        { //Draw lines from the camera to the corners f the screen
            Debug.DrawRay(sourceCamera.transform.position, va, Color.blue);
            Debug.DrawRay(sourceCamera.transform.position, vb, Color.blue);
            Debug.DrawRay(sourceCamera.transform.position, vc, Color.blue);
            Debug.DrawRay(sourceCamera.transform.position, vd, Color.blue);
        }

        if (drawFrustum) DrawFrustum(sourceCamera); //Draw actual camera frustum
    }

    Vector3 ThreePlaneIntersection(Plane p1, Plane p2, Plane p3)
    { //get the intersection point of 3 planes
        return ((-p1.distance * Vector3.Cross(p2.normal, p3.normal)) +
                (-p2.distance * Vector3.Cross(p3.normal, p1.normal)) +
                (-p3.distance * Vector3.Cross(p1.normal, p2.normal))) /
            (Vector3.Dot(p1.normal, Vector3.Cross(p2.normal, p3.normal)));
    }

    void DrawFrustum(Camera cam)
    {
        Vector3[] nearCorners = new Vector3[4]; //Approx'd nearplane corners
        Vector3[] farCorners = new Vector3[4]; //Approx'd farplane corners
        Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes(cam); //get planes from matrix
        Plane temp = camPlanes[1]; camPlanes[1] = camPlanes[2]; camPlanes[2] = temp; //swap [1] and [2] so the order is better for the loop

        for (int i = 0; i < 4; i++)
        {
            nearCorners[i] = ThreePlaneIntersection(camPlanes[4], camPlanes[i], camPlanes[(i + 1) % 4]); //near corners on the created projection matrix
            farCorners[i] = ThreePlaneIntersection(camPlanes[5], camPlanes[i], camPlanes[(i + 1) % 4]); //far corners on the created projection matrix
        }

        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine(nearCorners[i], nearCorners[(i + 1) % 4], Color.red, Time.deltaTime, false); //near corners on the created projection matrix
            Debug.DrawLine(farCorners[i], farCorners[(i + 1) % 4], Color.red, Time.deltaTime, false); //far corners on the created projection matrix
            Debug.DrawLine(nearCorners[i], farCorners[i], Color.red, Time.deltaTime, false); //sides of the created projection matrix
        }
    }
}
