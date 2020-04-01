using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// The AugmentaCameraAnchor is to the AugmentaCamera what the AugmentaAreaAnchor is to the AugmentaArea. 
/// It is linked to an AugmentaAreaAnchor and allow to render it using various ways. It can also update the corresponding AugmentaCamera (i.e. the AugmentaCamera belonging to the AugmentaArea that the AugmentaAreaAnchor is linked to) with its camera, position, rotation, post-process and Augmenta parameters.
/// </summary>

public class AugmentaCameraAnchor : CopyCameraToTargetCamera {

	//[Header("Augmenta Area Anchor")]
	[SerializeField]
	public AugmentaArea linkedAugmentaArea;

	public bool centerOnAugmentaArea;

	public enum CameraType {Orthographic, Perspective, OffCenter };
	public CameraType cameraType = CameraType.Perspective;

    private Vector3 BottomLeftCorner;
    private Vector3 BottomRightCorner;
    private Vector3 TopLeftCorner;
    private Vector3 TopRightCorner;
    public Transform lookTarget;

	#region MonoBehaviour Functions

	public override void Awake() {
		base.Awake();
	}

	// Use this for initialization
	public virtual void Start () {

		UpdateTargetCamera(updateTransformOnStart, updateCameraOnStart, updatePostProcessOnStart && hasPostProcessLayer);

    }

	void Update() {

		UpdateAugmentaAreaCorners();

		if (centerOnAugmentaArea) {
			sourceCamera.transform.localPosition = new Vector3(0, 0, transform.localPosition.z);
		} else {
			sourceCamera.transform.localPosition = new Vector3(sourceCamera.transform.localPosition.x, sourceCamera.transform.localPosition.y, transform.localPosition.z);
		}

		//Don't update camera with a 0 sized AugmentaArea
		if ((linkedAugmentaArea.AugmentaScene.Width == 0 || linkedAugmentaArea.AugmentaScene.Height == 0))
			return;

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

		UpdateTargetCamera(alwaysUpdateTransform, alwaysUpdateCamera, alwaysUpdatePostProcess && hasPostProcessLayer);
	}

	#endregion

	#region Augmenta Functions

	public void InitializeTargetCamera() {
        targetCamera = linkedAugmentaArea.augmentaCamera.gameObject.GetComponent<Camera>();
    }

	void UpdateAugmentaAreaCorners() {
		BottomLeftCorner = linkedAugmentaArea.transform.TransformPoint(new Vector3(0.5f, -0.5f, 0));
		BottomRightCorner = linkedAugmentaArea.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0));
		TopLeftCorner = linkedAugmentaArea.transform.TransformPoint(new Vector3(0.5f, 0.5f, 0));
		TopRightCorner = linkedAugmentaArea.transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0));
	}

	private void CopyAugmentaCameraSettings() {
		if (linkedAugmentaArea == null)
			return;

		if (!linkedAugmentaArea.augmentaCamera)
			return;

		linkedAugmentaArea.augmentaCamera.cameraType = cameraType;
		linkedAugmentaArea.augmentaCamera.centerOnAugmentaArea = centerOnAugmentaArea;
	}

	#endregion

	#region Camera Update Functions
    
    public override void UpdateTargetCamera(bool updateTransform, bool updateCamera, bool updatePostProcess)
    {

        base.UpdateTargetCamera(updateTransform, updateCamera, updatePostProcess && hasPostProcessLayer);

		if (updateCamera)
			CopyAugmentaCameraSettings();
    }

    void ComputeOrthoCamera()
    {
        sourceCamera.orthographic = true;
        sourceCamera.aspect = linkedAugmentaArea.AspectRatio;
        sourceCamera.orthographicSize = linkedAugmentaArea.transform.localScale.y / 2;
        
        sourceCamera.ResetProjectionMatrix();
    }

    void ComputePerspectiveCamera()
    {
        sourceCamera.orthographic = false;

        if (centerOnAugmentaArea) {
            sourceCamera.transform.localPosition = new Vector3(0.0f, 0.0f, transform.localPosition.z);
        }

		sourceCamera.ResetProjectionMatrix();

        if (linkedAugmentaArea.protocolVersion == ProtocolVersion.v1) {
            sourceCamera.fieldOfView = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(linkedAugmentaArea.AugmentaScene.Height * 0.5f * linkedAugmentaArea.meterPerPixel * linkedAugmentaArea.scaling, transform.localPosition.z);
        } else if (linkedAugmentaArea.protocolVersion == ProtocolVersion.v2) {
            sourceCamera.fieldOfView = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(linkedAugmentaArea.AugmentaScene.Height * 0.5f * linkedAugmentaArea.scaling, transform.localPosition.z);
        }
        sourceCamera.aspect = linkedAugmentaArea.AugmentaScene.Width / linkedAugmentaArea.AugmentaScene.Height;
        
    }

    void ComputeOffCenterCamera()
    {
        sourceCamera.orthographic = false;
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

		// rotate camera to align it with the projection surface normal
		//Quaternion camRotation = new Quaternion();
		//camRotation.SetLookRotation(vn, vu);
		//sourceCamera.transform.rotation = camRotation;

		float n = sourceCamera.nearClipPlane; // distance to the near clip plane (screen)
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
		p[2, 3] = 2.0f * f * n / (n - f);
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

        //if (drawNearCone)
        //{ //Draw lines from the camera to the corners f the screen
        //    Debug.DrawRay(sourceCamera.transform.position, va, Color.blue);
        //    Debug.DrawRay(sourceCamera.transform.position, vb, Color.blue);
        //    Debug.DrawRay(sourceCamera.transform.position, vc, Color.blue);
        //    Debug.DrawRay(sourceCamera.transform.position, vd, Color.blue);
        //}

        //if (drawFrustum) DrawFrustum(sourceCamera); //Draw actual camera frustum
    }

	#endregion

	#region Drawing Functions

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

	#endregion
}
