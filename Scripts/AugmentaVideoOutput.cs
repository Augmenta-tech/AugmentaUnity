using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;

namespace Augmenta
{
    /// <summary>
    /// Handle a video output to a texture according to the data from Fusion.
    /// </summary>
    public class AugmentaVideoOutput : MonoBehaviour
    {
        public AugmentaManager augmentaManager;
        public AugmentaVideoOutputCamera augmentaVideoOutputCamera;

        public RenderTexture videoOutputTexture;

        [Tooltip("Use data from Fusion to determine the output size in pixels.")]
        public bool autoOutputSizeInPixels = true;
        [Tooltip("Use data from Fusion to determine the output size in meters.")]
        public bool autoOutputSizeInMeters = true;
        [Tooltip("Use data from Fusion to determine the output offset.")]
        public bool autoOutputOffset = true;

        public Vector2Int videoOutputSizeInPixels {
			get { return autoOutputSizeInPixels ? augmentaManager.videoOutputSizeInPixels : _videoOutputSizeInPixels; }
            set { _videoOutputSizeInPixels = value; RefreshVideoTexture(); }
        }
        
        public Vector2 videoOutputSizeInMeters {
            get { return autoOutputSizeInMeters ? augmentaManager.videoOutputSizeInMeters : _videoOutputSizeInMeters; }
            set { _videoOutputSizeInMeters = value; }
		}

        public Vector2 videoOutputOffset {
			get { return autoOutputOffset ? augmentaManager.videoOutputOffset : _videoOutputOffset; }
            set { _videoOutputOffset = value; }
		}

        public Vector3 botLeftCorner = Vector3.zero;
        public Vector3 botRightCorner = Vector3.zero;
        public Vector3 topLeftCorner = Vector3.zero;
        public Vector3 topRightCorner = Vector3.zero;

        public delegate void VideoOutputTextureUpdated();
        public event VideoOutputTextureUpdated videoOutputTextureUpdated;

        [SerializeField] private Vector2Int _videoOutputSizeInPixels = new Vector2Int();
        [SerializeField] private Vector2 _videoOutputSizeInMeters = new Vector2();
        [SerializeField] private Vector2 _videoOutputOffset = new Vector2();

        private bool _initialized = false;

		#region MonoBehavious Functions

		private void OnEnable() {

            if (!_initialized)
                Initialize();
		}

		private void Update() {

            UpdateVideoOutputCorners();
		}

		private void OnDisable() {

            if (_initialized)
                CleanUp();
		}

		private void OnDrawGizmos() {


            Gizmos.color = Color.magenta;

            Gizmos.DrawLine(botLeftCorner, botRightCorner);
            Gizmos.DrawLine(botRightCorner, topRightCorner);
            Gizmos.DrawLine(topRightCorner, topLeftCorner);
            Gizmos.DrawLine(topLeftCorner, botLeftCorner);
        }

		#endregion

		void Initialize() {

			if (!augmentaManager) {
                Debug.LogError("AugmentaManager is not specified in AugmentaVideoOutput " + name+".");
                return;
			}

            augmentaManager.fusionUpdated += OnFusionUpdated;

			if (!videoOutputTexture) {
                //Initialize videoOutputTexture
                RefreshVideoTexture();
			}

            _initialized = true;
		}

        void CleanUp() {

            augmentaManager.fusionUpdated -= OnFusionUpdated;
        }

        public void RefreshVideoTexture() {

            if (videoOutputSizeInPixels.x == 0 || videoOutputSizeInPixels.y == 0)
                return;

            if (videoOutputTexture) {
                if (videoOutputSizeInPixels.x != videoOutputTexture.width || videoOutputSizeInPixels.y != videoOutputTexture.height) {
                    videoOutputTexture.Release();
				} else {
                    return;
				}
            }

            //Create texture
            videoOutputTexture = new RenderTexture(videoOutputSizeInPixels.x, videoOutputSizeInPixels.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            videoOutputTexture.Create();

            //Assign texture as render target of video output camera
            augmentaVideoOutputCamera.camera.targetTexture = videoOutputTexture;

            //Send texture updated event
            videoOutputTextureUpdated?.Invoke();
		}

        void OnFusionUpdated() {

            if (!videoOutputTexture)
                RefreshVideoTexture();

            //Check video texture size
            if (videoOutputSizeInPixels.x != videoOutputTexture.width || videoOutputSizeInPixels.y != videoOutputTexture.height) {
                RefreshVideoTexture();
            }

		}

        void UpdateVideoOutputCorners() {

            if (!augmentaManager.augmentaScene)
                return;

            topLeftCorner = augmentaManager.augmentaScene.debugObject.transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0)) + new Vector3(videoOutputOffset.x, 0, videoOutputOffset.y);
            botLeftCorner = topLeftCorner - Vector3.forward * videoOutputSizeInMeters.y;
            botRightCorner = botLeftCorner + Vector3.right * videoOutputSizeInMeters.x;
            topRightCorner = topLeftCorner + Vector3.right * videoOutputSizeInMeters.x;
        }
    }
}
