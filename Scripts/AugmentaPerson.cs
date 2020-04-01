using System;
using System.Collections.Generic;
using UnityEngine;

namespace Augmenta
{
	public class AugmentaPerson : MonoBehaviour
	{
        [Header("Person Settings")]
        public AugmentaManager augmentaManager;
        public bool showDebug = false;
        public GameObject debugObject;

        [Header("Augmenta Person Values")]
        public int pid;
		public int oid;
		public int age;
		public Vector2 centroid;
        public Vector2 velocity;
		public float depth;
		public Rect boundingRect;
		public Vector3 highest;

        public float inactiveTime;

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
            DrawGizmoCube(GetPersonWorldPosition(true),
                          transform.rotation, 
                          GetPersonWorldScale());
        }

        void OnDisable() {

            //Disconnect from person updated event
            if (_initialized) {
                augmentaManager.personUpdated -= UpdatePerson;
            }
        }

        #endregion

        #region Scene Handling Functions

        /// <summary>
        /// Initialize the person
        /// </summary>
        void Initialize() {

            if (!augmentaManager)
                return;

            //Connect to Augmenta events
            augmentaManager.personUpdated += UpdatePerson;

            _initialized = true;
        }

        /// <summary>
        /// Response to person updated event
        /// </summary>
        /// <param name="person"></param>
        public void UpdatePerson(AugmentaPerson person) {

            if (person.pid != pid)
                return;

            //Update debug object size
            debugObject.transform.position = GetPersonWorldPosition(true);
            debugObject.transform.localScale = GetPersonWorldScale();
        }

        /// <summary>
        /// Return the person world position from the Augmenta scene position, offsetted by half the person height or not.
        /// </summary>
        /// <returns></returns>
        Vector3 GetPersonWorldPosition(bool offset) {

            return augmentaManager.augmentaScene.transform.TransformPoint((centroid.x - 0.5f) * augmentaManager.augmentaScene.width * augmentaManager.scaling,
                                                                          offset ? highest.z * 0.5f * augmentaManager.scaling : 0,
                                                                          -(centroid.y - 0.5f) * augmentaManager.augmentaScene.height * augmentaManager.scaling);
        }

        /// <summary>
        /// Return the person scale
        /// </summary>
        /// <returns></returns>
        Vector3 GetPersonWorldScale() {

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