using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Augmenta
{
	public class AugmentaScene : MonoBehaviour
	{

		[Header("Scene Settings")]
		public AugmentaManager augmentaManager;
		public bool showDebug = false;
		public GameObject debugObject;

		[Header("Augmenta Scene Values")]
		public float width;
		public float height;
		public int personCount; //Person count from the scene updated message  /!\ Because of personTimeOut, it can be different from the instantiated person count /!\

		private Material debugMaterial;

		private bool _initialized = false;

		#region MonoBehaviour Functions

		private void OnEnable() {

			_initialized = false;
		}

		private void Update() {

			//Initialization
			if (!_initialized)
				Initialize();

			//Update debug state if incoherent
			if (showDebug != debugObject.activeSelf)
				ShowDebug(showDebug);
		}

		private void OnDisable() {

			//Disconnect from Augmenta SceneUpdated event
			if(_initialized)
				augmentaManager.sceneUpdated -= UpdateScene;
		}

		void OnDrawGizmos() {

			Gizmos.color = Color.blue;
			DrawGizmoCube(transform.position, transform.rotation, new Vector3(width * augmentaManager.scaling, 0, height * augmentaManager.scaling));
		}

		#endregion

		#region Scene Handling Functions

		/// <summary>
		/// Initialize the scene
		/// </summary>
		void Initialize() {

			if (!augmentaManager)
				return;

			//Connect to Augmenta SceneUpdated event
			augmentaManager.sceneUpdated += UpdateScene;

			//Get the debug material
			debugMaterial = debugObject.GetComponent<Renderer>().material;

			_initialized = true;
		}

		/// <summary>
		/// Update the scene object.
		/// </summary>
		public void UpdateScene() {

			//Initialization
			if (!_initialized)
				Initialize();

			//Update debug object size
			debugObject.transform.localScale = new Vector3(width * augmentaManager.scaling, height * augmentaManager.scaling, 0);

			//Update debug material tiling
			debugMaterial.mainTextureScale = debugObject.transform.localScale * 0.5f;
		}

		#endregion

		#region Gizmos Functions

		public void DrawGizmoCube(Vector3 position, Quaternion rotation, Vector3 scale) {
			Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
			Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

			Gizmos.matrix *= cubeTransform;

			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

			Gizmos.matrix = oldGizmosMatrix;
		}

		#endregion

		#region Debug Functions

		/// <summary>
		/// Activate/desactivate debug object
		/// </summary>
		/// <param name="show"></param>
		public void ShowDebug(bool show) {

			debugObject.SetActive(show);
		}

		#endregion

	}
}
