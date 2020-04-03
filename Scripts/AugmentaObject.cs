//using System;
using System.Collections.Generic;
using UnityEngine;

namespace Augmenta
{
	public class AugmentaObject : MonoBehaviour
	{
        [Header("Object Settings")]
        public AugmentaManager augmentaManager;
        public bool showDebug = false;
        public GameObject debugObject;

        [Header("Augmenta Object Values")]
        public int id;
		public int oid;
		public int age;
		public Vector2 centroid;
        public Vector2 velocity;
		public float depth;
		public Rect boundingRect;
		public Vector3 highest;

        public float inactiveTime;

        private Material augmentaObjectMaterialInstance;

        private bool _initialized = false;

        #region MonoBehaviour Functions

        private void OnEnable() {

            _initialized = false;
        }

        void Update() {

            //Initialization
            if (!_initialized)
                Initialize();

            //Update debug state if incoherent
            if (showDebug != debugObject.activeSelf)
                ShowDebug(showDebug);
        }

        void OnDrawGizmos() {

            Gizmos.color = Color.red;
            DrawGizmoCube(GetAugmentaObjectWorldPosition(true),
                          transform.rotation, 
                          GetAugmentaObjectWorldScale());
        }

        void OnDisable() {

            //Disconnect from person updated event
            if (_initialized) {
                augmentaManager.augmentaObjectUpdated -= UpdateAugmentaObject;
            }
        }

        #endregion

        #region Scene Handling Functions

        /// <summary>
        /// Initialize the augmenta object
        /// </summary>
        void Initialize() {

            if (!augmentaManager)
                return;

            //Connect to Augmenta events
            augmentaManager.augmentaObjectUpdated += UpdateAugmentaObject;

            //Get an instance of the debug material
            augmentaObjectMaterialInstance = debugObject.GetComponent<Renderer>().material;

            //Apply a random color to the material
            Random.InitState(id);
            augmentaObjectMaterialInstance.SetColor("_Color", Color.HSVToRGB(Random.value, 0.85f, 0.75f));

            _initialized = true;
        }

        /// <summary>
        /// Response to augmenta object updated event
        /// </summary>
        /// <param name="augmentaObject"></param>
        public void UpdateAugmentaObject(AugmentaObject augmentaObject) {

            if (augmentaObject.id != id)
                return;

            //Update debug object size
            debugObject.transform.position = GetAugmentaObjectWorldPosition(true);
            debugObject.transform.localScale = GetAugmentaObjectWorldScale();
        }

        /// <summary>
        /// Return the Augmenta object world position from the Augmenta scene position, offsetted by half the object height or not.
        /// </summary>
        /// <returns></returns>
        Vector3 GetAugmentaObjectWorldPosition(bool offset) {

            return augmentaManager.augmentaScene.transform.TransformPoint((centroid.x - 0.5f) * augmentaManager.augmentaScene.width * augmentaManager.scaling,
                                                                          offset ? highest.z * 0.5f * augmentaManager.scaling : 0,
                                                                          -(centroid.y - 0.5f) * augmentaManager.augmentaScene.height * augmentaManager.scaling);
        }

        /// <summary>
        /// Return the Augmenta object scale
        /// </summary>
        /// <returns></returns>
        Vector3 GetAugmentaObjectWorldScale() {

            return new Vector3(boundingRect.width * augmentaManager.augmentaScene.width * augmentaManager.scaling,
                               highest.z * augmentaManager.scaling,
                               boundingRect.height * augmentaManager.augmentaScene.height * augmentaManager.scaling);
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