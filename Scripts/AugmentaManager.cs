using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Augmenta;
using Augmenta.UnityOSC;

/// <summary>
///  Augmenta OSC Protocol V1 :

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

/* Augmenta OSC Protocol v2.0

/object/enter arg0 arg1 ... argN
/object/leave arg0 arg1 ... argN
/object/update arg0 arg1 ... argN

where args are : 
0: frame(int)     // Frame number
1: id(int)                        // id ex : 42th object to enter stage has pid=42
2: oid(int)                        // Ordered id ex : if 3 objects on stage, 43th object might have oid=2 
3: age(float)                      // Alive time (in s)
4: centroid.x(float 0:1)           // Position projected to the ground (normalised)
5: centroid.y(float 0:1)               
6: velocity.x(float -1:1)           // Speed and direction vector (in unit.s-1) (normalised)
7: velocity.y(float -1:1)
8: orientation(float 0:360) // With respect to horizontal axis right (0° = (1,0)), rotate counterclockwise
							// Estimation of the object orientation from its rotation and velocity
9: boundingRect.x(float 0:1)       // Bounding box center coord (normalised)	
10: boundingRect.y(float 0:1)       
11: boundingRect.width(float 0:1) // Bounding box width (normalised)
12: boundingRect.height(float 0:1)
13: boundingRect.rotation(float 0:360) // With respect to horizontal axis right counterclockwise
14: height(float)           // Height of the object (in m) (absolute)

/scene   arg0 arg1 ... argN
0: frame (int)                // Frame number
1: objectCount (int)                  // Number of objects
2: scene.width (float)             // Scene width in 
3: scene.height (float)

/fusion arg0 arg1 ... argN

0: videoOut.PixelWidth (int)      // VideoOut width in fusion
1: videoOut.PixelHeight (int)
2: videoOut.coord.x (int)          // top left coord in fusion
3: videoOut.coord.y (int)
4: scene.coord.x (float)          // Scene top left coord (0 for node by default)
5: scene.coord.y (float)

*/

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
		AugmentaObjectEnter,
		AugmentaObjectUpdate,
		AugmentaObjectLeave,
		SceneUpdated
	};

	public enum AugmentaDataType
	{
		Main,
		Extra,
		Shape	//Not implemented in Augmenta protocol yet.
	}

	public enum AugmentaProtocolVersion
	{
		V1,
		V2
	}

	public class AugmentaManager : MonoBehaviour
	{
		//Augmenta ID
		public string augmentaId;

		//OSC Settings

		[SerializeField] private int _inputPort = 12000;
		public int inputPort {
			get { return _inputPort; }
			set {
				_inputPort = value;
				CreateAugmentaOSCReceiver();
			}
		}

		public AugmentaProtocolVersion protocolVersion = AugmentaProtocolVersion.V2;

		//Augmenta Scene Settings
		public float pixelSize = 0.005f;
		public float scaling = 1.0f;

		//Augmenta Objects Settings
		public bool flipX;
		public bool flipY;
		// Number of seconds before an augmenta object who hasn't been updated is removed
		public float augmentaObjectTimeOut = 1.0f; // seconds
		public DesiredAugmentaObjectType desiredAugmentaObjectType = DesiredAugmentaObjectType.All;
		public int desiredAugmentaObjectCount = 1;

		//Augmenta Prefabs
		public GameObject augmentaScenePrefab;
		public GameObject augmentaObjectPrefab;

		//Debug
		public bool mute = false;
		public bool showDebug = true;

		//Events
		public delegate void AugmentaObjectEnter(AugmentaObject augmentaObject, AugmentaDataType augmentaDataType);
		public event AugmentaObjectEnter augmentaObjectEnter;

		public delegate void AugmentaObjectUpdate(AugmentaObject augmentaObject, AugmentaDataType augmentaDataType);
		public event AugmentaObjectUpdate augmentaObjectUpdate;

		public delegate void AugmentaObjectLeave(AugmentaObject augmentaObject, AugmentaDataType augmentaDataType);
		public event AugmentaObjectLeave augmentaObjectLeave;

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
		public void SendAugmentaEvent(AugmentaEventType eventType, AugmentaObject augmentaObject = null, AugmentaDataType augmentaDataType = AugmentaDataType.Main) {

			switch (eventType) {
				case AugmentaEventType.AugmentaObjectEnter:
					augmentaObjectEnter?.Invoke(augmentaObject, augmentaDataType);
					break;

				case AugmentaEventType.AugmentaObjectUpdate:
					augmentaObjectUpdate?.Invoke(augmentaObject, augmentaDataType);
					break;

				case AugmentaEventType.AugmentaObjectLeave:
					augmentaObjectLeave?.Invoke(augmentaObject, augmentaDataType);
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
		private AugmentaObject AddAugmentaObject(ArrayList args, AugmentaDataType objectDataType = AugmentaDataType.Main) {

			GameObject newAugmentaObjectObject = Instantiate(augmentaObjectPrefab, augmentaScene.gameObject.transform);

			AugmentaObject newAugmentaObject = newAugmentaObjectObject.GetComponent<AugmentaObject>();
			newAugmentaObject.augmentaManager = this;
			newAugmentaObject.showDebug = showDebug;
			newAugmentaObject.ShowDebug(showDebug);

			UpdateAugmentaObject(newAugmentaObject, args, objectDataType);

			newAugmentaObject.UpdateAugmentaObject(newAugmentaObject, objectDataType);

			augmentaObjects.Add(newAugmentaObject.id, newAugmentaObject);

			newAugmentaObjectObject.name = "Augmenta Object " + newAugmentaObject.id;

			return newAugmentaObject;
		}

		/// <summary>
		/// Update an Augmenta object data from incoming data
		/// </summary>
		/// <param name="augmentaObject"></param>
		/// <param name="args"></param>
		/// <param name="augmentaDataType"></param>
		private void UpdateAugmentaObject(AugmentaObject augmentaObject, ArrayList args, AugmentaDataType augmentaDataType = AugmentaDataType.Main) {

			switch (augmentaDataType) {
				case AugmentaDataType.Main:
					UpdateAugmentaObjectMain(augmentaObject, args);
					break;
				case AugmentaDataType.Extra:
					UpdateAugmentaObjectExtra(augmentaObject, args);
					break;
			}

		}

		/// <summary>
		/// Update an Augmenta object main data from incoming data.
		/// </summary>
		/// <param name="augmentaObject"></param>
		/// <param name="args"></param>
		private void UpdateAugmentaObjectMain(AugmentaObject augmentaObject, ArrayList args) {

			Vector2 centroid = Vector2.zero;
			Vector2 velocity = Vector2.zero;
			Vector3 highest = Vector3.zero;
			Rect boundingRect = new Rect();
			float orientation = 0;
			float rotation = 0;

			switch (protocolVersion) {

				case AugmentaProtocolVersion.V1:

					augmentaObject.id = (int)args[0];
					augmentaObject.oid = (int)args[1];
					augmentaObject.ageInFrames = (int)args[2];
					centroid = new Vector2((float)args[3], (float)args[4]);
					velocity = new Vector2((float)args[5], (float)args[6]);
					augmentaObject.depth = (float)args[7];
					boundingRect = new Rect((float)args[8], (float)args[9], (float)args[10], (float)args[11]);
					highest = new Vector3((float)args[12], (float)args[13], (float)args[14]);
					break;

				case AugmentaProtocolVersion.V2:

					augmentaObject.id = (int)args[1];
					augmentaObject.oid = (int)args[2];
					augmentaObject.ageInSeconds = (float)args[3];
					centroid = new Vector2((float)args[4], (float)args[5]);
					velocity = new Vector2((float)args[6], (float)args[7]);
					orientation = (float)args[8];
					boundingRect = new Rect((float)args[9], (float)args[10], (float)args[11], (float)args[12]);
					rotation = (float)args[13];
					highest = new Vector3(augmentaObject.highest.x, augmentaObject.highest.y, (float)args[14]);
					break;
			}

			if (flipX) {
				centroid.x = 1 - centroid.x;
				velocity.x = -velocity.x;
				orientation = orientation > 180 ? 360.0f - orientation : 180.0f - orientation;
				boundingRect.x = 1 - boundingRect.x;
				rotation = rotation > 180 ? 360.0f - rotation : 180.0f - rotation;
				highest.x = 1 - highest.x;
			}

			if (flipY) {
				centroid.y = 1 - centroid.y;
				velocity.y = -velocity.y;
				orientation = 360.0f - orientation;
				boundingRect.y = 1 - boundingRect.y;
				rotation = 360.0f - rotation;
				highest.y = 1 - highest.y;
			}

			augmentaObject.centroid = centroid;
			augmentaObject.velocity = velocity;
			augmentaObject.orientation = orientation;
			augmentaObject.boundingRect = boundingRect;
			augmentaObject.boundingRectRotation = rotation;
			augmentaObject.highest = highest;

			//Inactive time reset to zero : the object has just been updated
			augmentaObject.inactiveTime = 0;
		}

		/// <summary>
		/// Update an Augmenta object extra data from incoming data.
		/// </summary>
		/// <param name="augmentaObject"></param>
		/// <param name="args"></param>
		private void UpdateAugmentaObjectExtra(AugmentaObject augmentaObject, ArrayList args) {

			Vector3 highest = new Vector3((float)args[3], (float)args[4], augmentaObject.highest.z);

			if (flipX) {
				highest.x = 1 - highest.x;
			}

			if (flipY) {
				highest.y = 1 - highest.y;
			}

			augmentaObject.id = (int)args[1];
			augmentaObject.oid = (int)args[2];
			augmentaObject.highest = highest;
			augmentaObject.distanceToSensor = (float)args[5];
			augmentaObject.reflectivity = (float)args[6];

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
				SendAugmentaEvent(AugmentaEventType.AugmentaObjectLeave, augmentaObjects[id]);
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

			switch (protocolVersion) {
				case AugmentaProtocolVersion.V1:
					ParseAugmentaProtocolV1(message);
					break;

				case AugmentaProtocolVersion.V2:
					ParseAugmentaProtocolV2(message);
					break;
			}
		}

		/// <summary>
		/// Parse the OSC message using Augmenta protocol V1
		/// </summary>
		/// /// <param name="message"></param>
		private void ParseAugmentaProtocolV1(OSCMessage message) {

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
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectEnter, augmentaObject);
						} else {
							//Object was already there
							augmentaObject = augmentaObjects[id];
							UpdateAugmentaObject(augmentaObject, args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectUpdate, augmentaObject);
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
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectEnter, augmentaObject);
						} else {
							augmentaObject = augmentaObjects[id];
							UpdateAugmentaObject(augmentaObject, args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectUpdate, augmentaObject);
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
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectLeave, augmentaObject);
							RemoveAugmentaObject(id);
						}
					}

					break;

				case "/au/scene/":
				case "/au/scene":

					augmentaScene.augmentaObjectCount = (int)args[2];
					augmentaScene.width = (int)args[5] * pixelSize;
					augmentaScene.height = (int)args[6] * pixelSize;

					SendAugmentaEvent(AugmentaEventType.SceneUpdated);

					break;
			}
		}

		/// <summary>
		/// Parse the OSC message using Augmenta protocol V2
		/// </summary>
		/// <param name="message"></param>
		private void ParseAugmentaProtocolV2(OSCMessage message) {

			string address = message.Address;
			ArrayList args = new ArrayList(message.Data);

			int id, oid;
			AugmentaObject augmentaObject = null;

			switch (address) {

				case "/object/enter/":
				case "/object/enter":

					id = (int)args[1];
					oid = (int)args[2];

					if (IsAugmentaObjectDesired(oid)) {
						if (!augmentaObjects.ContainsKey(id)) {
							//New object
							augmentaObject = AddAugmentaObject(args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectEnter, augmentaObject);
						} else {
							//Object was already there
							augmentaObject = augmentaObjects[id];
							UpdateAugmentaObject(augmentaObject, args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectUpdate, augmentaObject);
						}
					}

					break;

				case "/object/update/":
				case "/object/update":

					id = (int)args[1];
					oid = (int)args[2];

					if (IsAugmentaObjectDesired(oid)) {
						if (!augmentaObjects.ContainsKey(id)) {
							augmentaObject = AddAugmentaObject(args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectEnter, augmentaObject);
						} else {
							augmentaObject = augmentaObjects[id];
							UpdateAugmentaObject(augmentaObject, args);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectUpdate, augmentaObject);
						}
					}

					break;

				case "/object/leave/":
				case "/object/leave":

					id = (int)args[1];
					oid = (int)args[2];

					if (IsAugmentaObjectDesired(oid)) {
						if (augmentaObjects.ContainsKey(id)) {
							augmentaObject = augmentaObjects[id];
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectLeave, augmentaObject);
							RemoveAugmentaObject(id);
						}
					}

					break;

				case "/object/enter/extra/":
				case "/object/enter/extra":

					id = (int)args[1];
					oid = (int)args[2];

					if (IsAugmentaObjectDesired(oid)) {
						if (!augmentaObjects.ContainsKey(id)) {
							//New object
							augmentaObject = AddAugmentaObject(args, AugmentaDataType.Extra);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectEnter, augmentaObject, AugmentaDataType.Extra);
						} else {
							//Object was already there
							augmentaObject = augmentaObjects[id];
							UpdateAugmentaObject(augmentaObject, args, AugmentaDataType.Extra);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectUpdate, augmentaObject, AugmentaDataType.Extra);
						}
					}

					break;

				case "/object/update/extra/":
				case "/object/update/extra":

					id = (int)args[1];
					oid = (int)args[2];

					if (IsAugmentaObjectDesired(oid)) {
						if (!augmentaObjects.ContainsKey(id)) {
							augmentaObject = AddAugmentaObject(args, AugmentaDataType.Extra);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectEnter, augmentaObject, AugmentaDataType.Extra);
						} else {
							augmentaObject = augmentaObjects[id];
							UpdateAugmentaObject(augmentaObject, args, AugmentaDataType.Extra);
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectUpdate, augmentaObject, AugmentaDataType.Extra);
						}
					}

					break;

				case "/object/leave/extra/":
				case "/object/leave/extra":

					id = (int)args[1];
					oid = (int)args[2];

					if (IsAugmentaObjectDesired(oid)) {
						if (augmentaObjects.ContainsKey(id)) {
							augmentaObject = augmentaObjects[id];
							SendAugmentaEvent(AugmentaEventType.AugmentaObjectLeave, augmentaObject, AugmentaDataType.Extra);
							RemoveAugmentaObject(id);
						}
					}

					break;

				case "/scene/":
				case "/scene":

					augmentaScene.augmentaObjectCount = (int)args[1];
					augmentaScene.width = (float)args[2];
					augmentaScene.height = (float)args[3];

					SendAugmentaEvent(AugmentaEventType.SceneUpdated);

					break;
			}
		}

		#endregion

		/// <summary>
		/// Clamp angle between 0 and 360 degrees
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		private float ClampAngle(float angle) {

			while (angle < 0)
				angle += 360.0f;

			return angle % 360.0f;
		}

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