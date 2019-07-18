using System;
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
	[Tooltip("If you don't specify a targetCamera, the targetCameraName will be used to find the targetCamera.")]
    [SerializeField] public String targetCameraName;
	[SerializeField] public Camera targetCamera;

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

		if (!sourceCamera) {
			Debug.LogError("Could not find a Camera component on " + gameObject.name);
			return;
		}

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
		if (!targetCamera)
			targetCamera = GameObject.Find(targetCameraName).GetComponent<Camera>();

		if (!targetCamera) {
			Debug.LogWarning("Could not find the target camera to copy to from " + gameObject.name);
			return;
		}

		if (hasPostProcessLayer && updatePostProcessOnStart) {
			//Look for PostProcessLayer Component
			targetPostProcessLayer = targetCamera.gameObject.GetComponent<PostProcessLayer>();

			if (!targetPostProcessLayer) {
				Debug.Log("TargetCamera " + targetCamera + " does not have a post process layer, adding one.");
				targetPostProcessLayer = targetCamera.gameObject.AddComponent<PostProcessLayer>();
			}
		}

        if (disableAfterUpdate)
            sourceCamera.enabled = false;

		targetInitialized = true;
    }

	public virtual void UpdateTargetCamera(bool updateTransform, bool updateCamera, bool updatePostProcess) {

		//Ensure the source camera components are initialized
		if (!sourceInitialized) {
			GetSourceCameraComponents();
		}

		//Ensure the target camera components are initialized
		if (!targetInitialized) {
			GetTargetCameraComponents();
		}

		//Don't update self
		if (targetCamera == sourceCamera)
        {
            return;
        }

		//Copy layer to TargetCamera (for postprocess mainly)
		targetCamera.gameObject.layer = gameObject.layer;


		if (updateTransform) {
			//Copy transform to TargetCamera
            CopyTransformComponent(transform, targetCamera.transform);
		}


        if (updateCamera)
        {
            //Copy camera settings to TargetCamera
            CopyCameraComponent();
        }

        if (updatePostProcess) {
			//If post processings, copy post processing settings to TargetCamera
			CopyPostProcessLayerComponent(postProcessLayer, targetPostProcessLayer);
		}

        if (disableAfterUpdate)
            sourceCamera.enabled = false;
    }

	private void CopyCameraComponent() {

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
		destination.volumeTrigger = destination.transform;
		destination.antialiasingMode = source.antialiasingMode;
		destination.fastApproximateAntialiasing = source.fastApproximateAntialiasing;
		destination.subpixelMorphologicalAntialiasing = source.subpixelMorphologicalAntialiasing;
		destination.temporalAntialiasing = source.temporalAntialiasing;
		destination.fog = source.fog;
		destination.stopNaNPropagation = source.stopNaNPropagation;
		destination.finalBlitToCameraTarget = source.finalBlitToCameraTarget;

	}
}
