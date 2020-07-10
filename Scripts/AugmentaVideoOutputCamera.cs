using Augmenta;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A camera that can will always adapt its field of view to perfectly match the Augmenta video output from Fusion.
/// It can be orthographic, perspective or offCenter.
/// If it is orthographic or perspective, it should be centered on the Augmenta scene for the field of view to match the output perfectly.
/// </summary>

namespace Augmenta
{
    public class AugmentaVideoOutputCamera : AugmentaCamera
    {
        public AugmentaVideoOutput augmentaVideoOutput;

		// Update is called once per frame
		void Update() {

			//Don't update if Augmenta scene size is 0
			if (augmentaVideoOutput.videoOutputSizeInMeters.x <= 0 || augmentaVideoOutput.videoOutputSizeInMeters.y <= 0)
				return;

			switch (cameraType) {

				case CameraType.Orthographic:
					CenterCamera();
					ComputeOrthoCamera(augmentaVideoOutput.videoOutputSizeInMeters.x, augmentaVideoOutput.videoOutputSizeInMeters.y);
					break;

				case CameraType.Perspective:
					CenterCamera();
					ComputePerspectiveCamera(augmentaVideoOutput.videoOutputSizeInMeters.x, augmentaVideoOutput.videoOutputSizeInMeters.y);
					break;

				case CameraType.OffCenter:
					ComputeOffCenterCamera(augmentaVideoOutput.botLeftCorner, augmentaVideoOutput.botRightCorner, augmentaVideoOutput.topLeftCorner, augmentaVideoOutput.topRightCorner);
					break;

			}
		}

		void CenterCamera() {

			camera.transform.position = new Vector3(augmentaVideoOutput.botLeftCorner.x + (augmentaVideoOutput.botRightCorner.x - augmentaVideoOutput.botLeftCorner.x) * 0.5f, camera.transform.position.y, augmentaVideoOutput.botLeftCorner.z + (augmentaVideoOutput.topLeftCorner.z - augmentaVideoOutput.botLeftCorner.z) * 0.5f);
		}
	}
}
