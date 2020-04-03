using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Augmenta;
using Augmenta.UnityOSC;

/// <summary>
///  Augmenta OSC Protocol :

///  /au/personWillLeave/ args0 arg1 ... argn
///  /au/personUpdated/   args0 arg1 ... argn
///  /au/personEntered/   args0 arg1 ... argn

///  where args are :

///  0: pid (int)
///  1: oid (int)
///  2: age (int)
///  3: centroid.x (float)
///  4: centroid.y (float)
///  5: velocity.x (float)
///  6: velocity.y (float)
///  7: depth (float)
///  8: boundingRect.x (float)
///  9: boundingRect.y (float)
///  10: boundingRect.width (float)
///  11: boundingRect.height (float)
///  12: highest.x (float)
///  13: highest.y (float)
///  14: highest.z (float)

///  /au/scene/   args0 arg1...argn

///  0: currentTime (int)
///  1: percentCovered (float)
///  2: numPeople (int)
///  3: averageMotion.x (float)
///  4: averageMotion.y (float)
///  5: scene.width (float)
///  6: scene.height (float)

/// </summary>

namespace Augmenta {

	public enum DesiredAugmentaObjectType
	{
		All,
		Oldest,
		Newest
	};

	public enum AugmentaEventType
	{
		AugmentaObjectEntered,
		AugmentaObjectUpdated,
		AugmentaObjectLeft,
		SceneUpdated
	};

	public class AugmentaManager : MonoBehaviour
	{
		[Header("Augmenta ID")]
		public string augmentaId;

		[Header("OSC Settings")]

		[SerializeField] private int _inputPort = 12000;
		public int inputPort {
			get { return _inputPort; }
			set {
				_inputPort = value;
				CreateAugmentaOSCReceiver();
			}
		}

		[Header("Augmenta Scene Settings")]
		//public float pixelSize = 0.005f;
		public float scaling = 1.0f;

		[Header("Augmenta Objects Settings")]
		public bool flipX;
		public bool flipY;
		// Number of seconds before an augmenta object who hasn't been updated is removed
		public float augmentaObjectTimeOut = 1.0f; // seconds
		public DesiredAugmentaObjectType desiredAugmentaObjectType = DesiredAugmentaObjectType.All;
		public int desiredAugmentaObjectCount = 1;

		[Header("Augmenta Prefabs")]
		public GameObject augmentaScenePrefab;
		public GameObject augmentaObjectPrefab;

		[Header("Debug")]
		public bool mute = false;
		public bool showDebug = true;

		/* Events */
		public delegate void AugmentaObjectEntered(AugmentaObject augmentaObject);
		public event AugmentaObjectEntered augmentaObjectEntered;

		public delegate void AugmentaObjectUpdated(AugmentaObject augmentaObject);
		public event AugmentaObjectUpdated augmentaObjectUpdated;

		public delegate void AugmentaObjectLeft(AugmentaObject augmentaObject);
		public event AugmentaObjectLeft augmentaObjectLeft;

		public delegate void SceneUpdated();
		public event SceneUpdated sceneUpdated;

		public Dictionary<int, AugmentaObject> augmentaObjects;
		public AugmentaScene augmentaScene;

		private List<int> _expiredIds = new List<int>(); //Used to remove timed out objects

		#region MonoBehaviour Functions

		private void Awake() {

			//Initialize scene
			InitializeAugmentaScene();

			//Initialize objects array
			augmentaObjects = new Dictionary<int, AugmentaObject>();

			//Create OSC Receiver
			CreateAugmentaOSCReceiver();
		}

		private void Update() {

			//Check if objects are alive
			CheckAlive();
		}

		private void OnDisable() {

			//Remove OSC Receiver
			RemoveAugmentaOSCReceiver();
		}

		#endregion

		#region Augmenta Functions

		/// <summary>
		/// Initialize the augmenta scene object
		/// </summary>
		void InitializeAugmentaScene() {

			GameObject sceneObject = Instantiate(augmentaScenePrefab, transform);
			sceneObject.name = "Augmenta Scene " + augmentaId;

			augmentaScene = sceneObject.GetComponent<AugmentaScene>();

			augmentaScene.augmentaManager = this;
			augmentaScene.showDebug = showDebug;
			augmentaScene.ShowDebug(showDebug);

			augmentaScene.UpdateScene();
		}

		/// <summary>
		/// Send the Augmenta event of corresponding type, according to the desired object.
		/// </summary>
		/// <param name="eventType"></param>
		/// <param name="augmentaObject"></param>
		public void SendAugmentaEvent(AugmentaEventType eventType, AugmentaObject augmentaObject = null) {

			switch (eventType) {
				case AugmentaEventType.AugmentaObjectEntered:
					augmentaObjectEntered?.Invoke(augmentaObject);
					break;

				case AugmentaEventType.AugmentaObjectUpdated:
					augmentaObjectUpdated?.Invoke(augmentaObject);
					break;

				case AugmentaEventType.AugmentaObjectLeft:
					augmentaObjectLeft?.Invoke(augmentaObject);
					break;

				case AugmentaEventType.SceneUpdated:
					sceneUpdated?.Invoke();
					break;
			}
		}

		/// <summary>
		/// Add new Augmenta object.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		private AugmentaObject AddAugmentaObject(ArrayList args) {

			GameObject newAugmentaObjectObject = Instantiate(augmentaObjectPrefab, augmentaScene.gameObject.transform);
			newAugmentaObjectObject.name = "Augmenta Object " + args[0];

			AugmentaObject newAugmentaObject = newAugmentaObjectObject.GetComponent<AugmentaObject>();
			newAugmentaObject.augmentaManager = this;
			newAugmentaObject.showDebug = showDebug;
			newAugmentaObject.ShowDebug(showDebug);

			UpdateAugmentaObject(newAugmentaObject, args);
			newAugmentaObject.UpdateAugmentaObject(newAugmentaObject);

			augmentaObjects.Add(newAugmentaObject.id, newAugmentaObject);

			return newAugmentaObject;
		}

		/// <summary>
		/// Update an Augmenta object from incoming data.
		/// </summary>
		/// <param name="augmentaObject"></param>
		/// <param name="args"></param>
		private void UpdateAugmentaObject(AugmentaObject augmentaObject, ArrayList args) {

			augmentaObject.id = (int)args[0];
			augmentaObject.oid = (int)args[1];
			augmentaObject.age = (int)args[2];
			Vector2 centroid = new Vector2((float)args[3], (float)args[4]);
			Vector2 velocity = new Vector2((float)args[5], (float)args[6]);
			augmentaObject.depth = (float)args[7];
			Rect boundingRect = new Rect((float)args[8], (float)args[9], (float)args[10], (float)args[11]);
			Vector3 highest = new Vector3((float)args[12], (float)args[13], (float)args[14]);

			if (flipX) {
				centroid.x = 1 - centroid.x;
				velocity.x = -velocity.x;
				boundingRect.x = 1 - boundingRect.x;
				highest.x = 1 - highest.x;
			}

			if (flipY) {
				centroid.y = 1 - centroid.y;
				velocity.y = -velocity.y;
				boundingRect.y = 1 - boundingRect.y;
				highest.y = 1 - highest.y;
			}

			augmentaObject.centroid = centroid;
			augmentaObject.velocity = velocity;
			augmentaObject.boundingRect = boundingRect;
			augmentaObject.highest = highest;

			//Inactive time reset to zero : the object has just been updated
			augmentaObject.inactiveTime = 0;
		}

		/// <summary>
		/// Remove an object with its id
		/// </summary>
		/// <param name="id"></param>
		public void RemoveAugmentaObject(int id) {

			Destroy(augmentaObjects[id].gameObject);
			augmentaObjects.Remove(id);
		}

		/// <summary>
		/// Remove all augmenta objects
		/// </summary>
		public void RemoveAllAugmentaObjects() {

			while (augmentaObjects.Count > 0) {
				RemoveAugmentaObject(augmentaObjects.ElementAt(0).Key);
			}
		}

		/// <summary>
		/// Return true if the object is desired (i.e. should be added/updated).
		/// </summary>
		/// <param name="oid"></param>
		/// <returns></returns>
		public bool IsAugmentaObjectDesired(int oid) {

			if (desiredAugmentaObjectType == DesiredAugmentaObjectType.Oldest) {
				return oid < desiredAugmentaObjectCount;
			} else if (desiredAugmentaObjectType == DesiredAugmentaObjectType.Newest) {
				return oid >= (augmentaScene.augmentaObjectCount - desiredAugmentaObjectCount);
			} else {
				return true;
			}
		}

		/// <summary>
		/// Check if augmenta objects are alive
		/// </summary>
		void CheckAlive() {

			_expiredIds.Clear();

			foreach (int key in augmentaObjects.Keys) {

				if (augmentaObjects[key].inactiveTime < augmentaObjectTimeOut) {
					// We add a frame to the inactiveTime count
					augmentaObjects[key].inactiveTime += Time.deltaTime;
				} else {
					// The object hasn't been updated for a certain number of frames : mark for removal
					_expiredIds.Add(key);
				}
			}

			//Remove expired objects
			foreach (int id in _expiredIds) {
				SendAugmentaEvent(AugmentaEventType.AugmentaObjectLeft, augmentaObjects[id]);
				RemoveAugmentaObject(id);
			}
		}

		#endregion

		#region OSC Functions 

		/// <summary>
		/// Create an OSC receiver for Augmenta at the inputPort. Return true if success, false otherwise.
		/// </summary>
		/// <returns>
		/// </returns>
		public void CreateAugmentaOSCReceiver() {

			RemoveAugmentaOSCReceiver();

			if (OSCMaster.CreateReceiver("Augmenta-" + augmentaId, inputPort) != null) {
				OSCMaster.Receivers["Augmenta-" + augmentaId].messageReceived += OSCMessageReceived;
			} else {
				Debug.LogError("Could not create OSC receiver at port " + inputPort + ".");
			}
		}

		/// <summary>
		/// Remove the Augmenta OSC receiver.
		/// </summary>
		public void RemoveAugmentaOSCReceiver() {

			//To avoid errors if OSCMaster was destroyed before this
			if (OSCMaster.Instance == null)
				return;

			if (OSCMaster.Receivers.ContainsKey("Augmenta-" + augmentaId)) {
				OSCMaster.Receivers["Augmenta-" + augmentaId].messageReceived -= OSCMessageReceived;
				OSCMaster.RemoveReceiver("Augmenta-" + augmentaId);
			}
		}

		/// <summary>
		/// Parse incoming Augmenta messages.
		/// </summary>
		/// <param name="message"></param>
		public void OSCMessageReceived(OSCMessage message) {

			if (mute) return;

			string address = message.Address;
			ArrayList args = new ArrayList(message.Data);

			int id, oid;
			AugmentaObject augmentaObject = null;

			switch (address) {

				case "/au/personEntered/":
				case "/au/personEntered":

					id = (int)args[0];
					oid = (int)args[1];

					if (IsAugmentaObjectDesired(oid)) {
						if (!augmentaObjects.ContainsKey(id)) {
							//New object
							augmentaObject = AddAugmentaObject(args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectEntered, augmentaObject);
						} else {
							//Object was already there
							augmentaObject = augmentaObjects[id];
							UpdateAugmentaObject(augmentaObject, args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectUpdated, augmentaObject);
						}
					}

					break;

				case "/au/personUpdated/":
				case "/au/personUpdated":

					id = (int)args[0];
					oid = (int)args[1];

					if (IsAugmentaObjectDesired(oid)) {
						if (!augmentaObjects.ContainsKey(id)) {
							augmentaObject = AddAugmentaObject(args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectEntered, augmentaObject);
						} else {
							augmentaObject = augmentaObjects[id];
							UpdateAugmentaObject(augmentaObject, args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectUpdated, augmentaObject);
						}
					}

					break;

				case "/au/personWillLeave/":
				case "/au/personWillLeave":

					id = (int)args[0];
					oid = (int)args[1];

					if (IsAugmentaObjectDesired(oid)) {
						if (augmentaObjects.ContainsKey(id)) {
							augmentaObject = augmentaObjects[id];
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectLeft, augmentaObject);
							RemoveAugmentaObject(id);
						}
					}

					break;

				case "/au/scene/":
				case "/au/scene":

					augmentaScene.augmentaObjectCount = (int)args[2];
					augmentaScene.width = (float)args[5];
					augmentaScene.height = (float)args[6];

					SendAugmentaEvent(AugmentaEventType.SceneUpdated);

					break;
			}
		}

		#endregion

		#region Debug Functions

		/// <summary>
		/// Show debug of scene and persons
		/// </summary>
		/// <param name="show"></param>
		public void ShowDebug(bool show) {

			augmentaScene.showDebug = show;

			foreach(var augmentaObject in augmentaObjects) {
				augmentaObject.Value.showDebug = show;
			}
		}

		#endregion
	}
}