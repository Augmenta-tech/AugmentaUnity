using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(Camera))]
public class CopyCameraToTargetCamera : MonoBehaviour {

    /******************************************
	 * Copy the transform, camera settings and post process layer from this camera to the target camera at scene start up.
	 * 
	 * If the camera is animated, you can enable continuous update of the target camera with the toggles AlwaysUdate* below.
	 * 
	 * If you change your camera properties at runtime by script, you should call the updateTargetCamera function to update your changes to the target camera
	 * 
	 * ****************************************/
    
    [Header("Target Camera")]
    [SerializeField]
    public string TargetCameraName;

    [Header("Target Camera Update Settings")]
    //Whether the camera properties should be updated at each frame or not
    [Tooltip("Should the transform be copied to the target camera on start ?")]
    public bool updateTransformOnStart = true;

    [Tooltip("Should the camera settings be copied to the target camera on start ?")]
    public bool updateCameraOnStart = true;

    [Tooltip("Should the postprocess be copied to the target camera on start ?")]
    public bool updatePostProcessOnStart = true;

    [Tooltip("Should the camera be disabled once the target camera is updated ?")]
    public bool disableAfterUpdate = false;

    //Whether the camera properties should be updated at each frame or not
    [Tooltip("Should the transform be copied to the target camera at each frame ?")]
	public bool alwaysUpdateTransform = false;

	[Tooltip("Should the camera settings be copied to the target camera at each frame ?")]
	public bool alwaysUpdateCamera = false;

	[Tooltip("Should the postprocess be copied to the target camera at each frame ?")]
	public bool alwaysUpdatePostProcess = false;

    protected Camera sourceCamera;

    //Target camera attributes
    protected GameObject targetCameraObject;
    protected Camera targetCamera;

    protected PostProcessLayer targetPostProcessLayer;

    private Vector3 tmpPosition;
    private Quaternion tmpRotation;

	//This camera attributes
	protected PostProcessLayer postProcessLayer;
    protected bool hasPostProcessLayer;

    private void OnEnable()
    {
        sourceCamera = GetComponent<Camera>();

        //Check if this camera has post process
        postProcessLayer = GetComponent<PostProcessLayer>();

		if (postProcessLayer) {
			hasPostProcessLayer = true;
		} else {
			hasPostProcessLayer = false;
		}

		//Get target camera components
		GetTargetCameraComponents();

        //Update target camera
        UpdateTargetCamera(updateTransformOnStart, updateCameraOnStart, updatePostProcessOnStart && hasPostProcessLayer);

	}

    
	private void Update() {

		UpdateTargetCamera(alwaysUpdateTransform, alwaysUpdateCamera, alwaysUpdatePostProcess && hasPostProcessLayer);

	}

	public void GetTargetCameraComponents() {

		//Look for TargetCamera object
		targetCameraObject = GameObject.Find(TargetCameraName);

		if (!targetCameraObject) {
			Debug.LogWarning("Could not find TargetCamera " + TargetCameraName + " object to copy settings to.");
			return;
		}

		//Look for Camera Component
		targetCamera = targetCameraObject.GetComponent<Camera>();

		if (!targetCamera) {
			Debug.LogWarning("TargetCamera " + TargetCameraName + " does not contain a camera component.");
			return;
		}

		if (hasPostProcessLayer && updatePostProcessOnStart) {
			//Look for PostProcessLayer Component
			targetPostProcessLayer = targetCameraObject.GetComponent<PostProcessLayer>();

			if (!targetPostProcessLayer) {
				Debug.Log("TargetCamera " + TargetCameraName + " do not have a post process layer, adding one.");
				targetPostProcessLayer = targetCameraObject.AddComponent<PostProcessLayer>();
			}
		}

        if(disableAfterUpdate)
            sourceCamera.enabled = false;
    }

	public virtual void UpdateTargetCamera(bool updateTransform, bool updateCamera, bool updatePostProcess) {

        //Don't update if no target camera 
        if (!targetCameraObject)
        {
            Debug.LogWarning("No target camera object.");
            return;
        }

        //Don't update self
        if (targetCameraObject == gameObject)
        {
            return;
        }

        //Copy layer to TargetCamera (for postprocess mainly)
        targetCameraObject.layer = gameObject.layer;

        if (updateCamera) {
			//Copy camera settings to TargetCamera
			CopyCameraComponent(sourceCamera, targetCamera);
		}

		if (updateTransform) {
			//Copy transform to TargetCamera
			CopyTransformComponent(transform, targetCameraObject.transform);
		}

		if (updatePostProcess) {
			//If post processings, copy post processing settings to TargetCamera
			CopyPostProcessLayerComponent(postProcessLayer, targetPostProcessLayer);
		}

        if (disableAfterUpdate)
            sourceCamera.enabled = false;
    }

	private void CopyCameraComponent(Camera source, Camera destination) {

        //Camera properties
        /*
        destination.clearFlags = source.clearFlags;
		destination.backgroundColor = source.backgroundColor;
		destination.cullingMask = source.cullingMask;
		destination.orthographic = source.orthographic;
		destination.orthographicSize = source.orthographicSize;
		destination.fieldOfView = source.fieldOfView;
		destination.farClipPlane = source.farClipPlane;
		destination.nearClipPlane = source.nearClipPlane;
		destination.rect = source.rect;
		destination.depth = source.depth;
		destination.renderingPath = source.renderingPath;
		destination.targetTexture = source.targetTexture;
		destination.targetDisplay = source.targetDisplay;
		destination.allowHDR = source.allowHDR;
		destination.allowMSAA = source.allowMSAA;
		destination.allowDynamicResolution = source.allowDynamicResolution;
		destination.clearStencilAfterLightingPass = source.clearStencilAfterLightingPass;
		destination.depthTextureMode = source.depthTextureMode;
		destination.eventMask = source.eventMask;
		destination.opaqueSortMode = source.opaqueSortMode;
        */

        //Keep and reapply current transform since copyFrom also copy the transform
        tmpPosition = targetCamera.transform.position;
        tmpRotation = targetCamera.transform.rotation;

        targetCamera.CopyFrom(sourceCamera);

        targetCamera.transform.position = tmpPosition;
        targetCamera.transform.rotation = tmpRotation;

    }

	private void CopyTransformComponent(Transform source, Transform destination) {

		//Transform properties
		destination.position = source.position;
		destination.rotation = source.rotation;
		destination.localScale = source.localScale;

	}

	private void CopyPostProcessLayerComponent(PostProcessLayer source, PostProcessLayer destination) {

		destination.volumeLayer = source.volumeLayer;
		//destination.volumeTrigger = source.volumeTrigger;
		destination.antialiasingMode = source.antialiasingMode;
		destination.fastApproximateAntialiasing = source.fastApproximateAntialiasing;
		destination.subpixelMorphologicalAntialiasing = source.subpixelMorphologicalAntialiasing;
		destination.temporalAntialiasing = source.temporalAntialiasing;
		destination.fog = source.fog;
		destination.dithering = source.dithering;
		destination.stopNaNPropagation = source.stopNaNPropagation;

	}
}
