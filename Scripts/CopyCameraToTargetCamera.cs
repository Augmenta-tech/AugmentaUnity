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
    
    //[Header("Target Camera Object")]
    [SerializeField] public GameObject targetCameraObject;

	//Whether the camera properties should be updated at each frame or not
	//[Header("Target Camera Update Settings")]

    [Tooltip("Should the transform be copied to the target camera on start ?")]
	[SerializeField] public bool updateTransformOnStart = true;

    [Tooltip("Should the camera settings be copied to the target camera on start ?")]
	[SerializeField] public bool updateCameraOnStart = true;

    [Tooltip("Should the postprocess be copied to the target camera on start ?")]
	[SerializeField] public bool updatePostProcessOnStart = true;

    [Tooltip("Should the camera be disabled once the target camera is updated ?")]
	[SerializeField] public bool disableAfterUpdate = false;

    //Whether the camera properties should be updated at each frame or not
    [Tooltip("Should the transform be copied to the target camera at each frame ?")]
	[SerializeField] public bool alwaysUpdateTransform = false;

	[Tooltip("Should the camera settings be copied to the target camera at each frame ?")]
	[SerializeField] public bool alwaysUpdateCamera = false;

	[Tooltip("Should the postprocess be copied to the target camera at each frame ?")]
	[SerializeField] public bool alwaysUpdatePostProcess = false;

    protected Camera sourceCamera;

    //Target camera attributes
    protected Camera targetCamera;

    protected PostProcessLayer targetPostProcessLayer;

    private Vector3 tmpPosition;
    private Quaternion tmpRotation;

	//This camera attributes
	protected PostProcessLayer postProcessLayer;
    protected bool hasPostProcessLayer;

	protected bool targetInitialized = false;
	protected bool sourceInitialized = false;

	#region MonoBehaviour Functions

	public virtual void Awake()
    {
		Debug.Log("Awake " + gameObject.name + " : " + transform.position);
		if (!sourceInitialized) {
			GetSourceCameraComponents();
		}

		if (!targetInitialized) {
			GetTargetCameraComponents();
		}

	}

	private void Start() {
		//Update target camera
		UpdateTargetCamera(updateTransformOnStart, updateCameraOnStart, updatePostProcessOnStart && hasPostProcessLayer);
	}


	private void Update() {

		UpdateTargetCamera(alwaysUpdateTransform, alwaysUpdateCamera, alwaysUpdatePostProcess && hasPostProcessLayer);

	}

	#endregion

	public void GetSourceCameraComponents() {
		sourceCamera = GetComponent<Camera>();

		//Check if this camera has post process
		postProcessLayer = GetComponent<PostProcessLayer>();

		if (postProcessLayer) {
			hasPostProcessLayer = true;
		} else {
			hasPostProcessLayer = false;
		}

		sourceInitialized = true;
	}

	public void GetTargetCameraComponents() {

		//Look for Camera Component
		targetCamera = targetCameraObject.GetComponent<Camera>();

		if (!targetCamera) {
			Debug.LogWarning("The target camera object " + targetCameraObject + " does not contain a camera component.");
			return;
		}

		if (hasPostProcessLayer && updatePostProcessOnStart) {
			//Look for PostProcessLayer Component
			targetPostProcessLayer = targetCameraObject.GetComponent<PostProcessLayer>();

			if (!targetPostProcessLayer) {
				Debug.Log("TargetCamera " + targetCameraObject + " does not have a post process layer, adding one.");
				targetPostProcessLayer = targetCameraObject.AddComponent<PostProcessLayer>();
			}
		}

        if (disableAfterUpdate)
            sourceCamera.enabled = false;

		targetInitialized = true;
    }

	public virtual void UpdateTargetCamera(bool updateTransform, bool updateCamera, bool updatePostProcess) {

        //Don't update if no target camera 
        if (!targetCameraObject)
        {
            Debug.LogWarning("No target camera object.");
            return;
        }

		//Ensure the source camera components are initialized
		if (!sourceInitialized) {
			GetTargetCameraComponents();
		}

		//Ensure the target camera components are initialized
		if (!targetInitialized) {
			GetTargetCameraComponents();
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
		//targetCamera.projectionMatrix = sourceCamera.projectionMatrix;

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
		//destination.dithering = source.dithering;
		destination.stopNaNPropagation = source.stopNaNPropagation;

	}
}
