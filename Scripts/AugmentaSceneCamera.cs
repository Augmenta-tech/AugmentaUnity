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

namespace Augmenta
{
    public class AugmentaSceneCamera : AugmentaCamera
    {
        public AugmentaManager augmentaManager;

        private Vector3 _botLeftCorner;
        private Vector3 _botRightCorner;
        private Vector3 _topLeftCorner;
        private Vector3 _topRightCorner;

        // Update is called once per frame
        void Update() {
            //Don't update if there is no Augmenta scene
            if (!augmentaManager.augmentaScene)
                return;

            //Don't update if Augmenta scene size is 0
            if (augmentaManager.augmentaScene.width <= 0 || augmentaManager.augmentaScene.height <= 0)
                return;

            UpdateAugmentaSceneCorners();

            switch (cameraType) {

                case CameraType.Orthographic:
                    CenterCamera();
                    ComputeOrthoCamera(augmentaManager.augmentaScene.width * augmentaManager.scaling, augmentaManager.augmentaScene.height * augmentaManager.scaling);
                    break;

                case CameraType.Perspective:
                    CenterCamera();
                    ComputePerspectiveCamera(augmentaManager.augmentaScene.width * augmentaManager.scaling, augmentaManager.augmentaScene.height * augmentaManager.scaling);
                    break;

                case CameraType.OffCenter:
                    ComputeOffCenterCamera(_botLeftCorner, _botRightCorner, _topLeftCorner, _topRightCorner);
                    break;

            }
        }

        /// <summary>
        /// Update the positions of the Augmenta scene corners
        /// </summary>
        void UpdateAugmentaSceneCorners() {
            _botLeftCorner = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0));
            _botRightCorner = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(new Vector3(0.5f, -0.5f, 0));
            _topLeftCorner = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0));
            _topRightCorner = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(new Vector3(0.5f, 0.5f, 0));
        }

        void CenterCamera() {

            camera.transform.position = new Vector3(_botLeftCorner.x + (_botRightCorner.x - _botLeftCorner.x) * 0.5f, camera.transform.position.y, _botLeftCorner.z + (_topLeftCorner.z - _botLeftCorner.z) * 0.5f);
        }
    }
}
