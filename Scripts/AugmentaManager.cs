using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Augmenta;
using Augmenta.UnityOSC;

/// <summary>
/// The AugmentaArea handles the incoming Augmenta OSC messages and updates the AugmentaPeople list and AugmentaScene accordingly.
///  
/// It also sends the events personEntered, personUpdated, personLeaving and sceneUpdated when the corresponding events are happening in Augmenta.
///  
/// AugmentaArea parameters:
/// 
/// DEBUG:
/// Mute: If muted, the AugmentaArea will not process incoming OSC messages.
/// Mire: Enable the display of a mire in the AugmentaArea.
/// AugmentaDebugger: AugmentaDebugger instance that will be used for debug handling.
/// Augmenta Debug: Enable the Augmenta Debug, drawing each person with their information.
/// Debug Transparency: The transparency of the debug view.
/// Draw Gizmos: Enable the drawing of gizmos.
/// 
/// AUGMENTA CAMERA:
/// MeterPerPixel: Size of a pixel in meter. In order to have a coherent scale between Unity and reality, this value should be the size of a pixel on the projection surface.
/// Scaling: Coefficient applied on the MeterPerPixel value in order to roughly correct miscalibrations. If the value of MeterPerPixel is accurate, the scaling value should be 1.
/// 
/// AUGMENTA PERSONS SETTINGS:
/// FlipX: Flip the Augmenta persons positions and movements horizontally.
/// FlipY: Flip the Augmenta persons positions and movements vertically.
/// PersonTimeOut: Number of seconds before a person who hasn't been updated is removed.
/// NbAugmentaPersons: Number of persons detected.
/// ActualPersonType: Type of person displayed: All Persons = every person is displayed; Oldest = only the oldest person is displayed; Newest = only the newest person is displayed.
/// AskedPersons: Number of persons displayed in Oldest or Newest modes. 
/// 
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

	public enum DesiredPersonType
	{
		All,
		Oldest,
		Newest
	};

	public enum AugmentaEventType
	{
		PersonEntered,
		PersonUpdated,
		PersonLeft,
		SceneUpdated
	};

	public class AugmentaManager : MonoBehaviour
	{
		[Header("Augmenta ID")]
		public string id;

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

		[Header("Augmenta Person Settings")]
		public bool flipX;
		public bool flipY;
		// Number of seconds before a person who hasn't been updated is removed
		public float personTimeOut = 1.0f; // seconds
		public DesiredPersonType desiredPersonType = DesiredPersonType.All;
		public int desiredPersonCount = 1;

		[Header("Augmenta Prefabs")]
		public GameObject augmentaScenePrefab;
		public GameObject augmentaPersonPrefab;

		[Header("Debug")]
		public bool mute = false;
		public bool showDebug = true;

		/* Events */
		public delegate void PersonEntered(AugmentaPerson p);
		public event PersonEntered personEntered;

		public delegate void PersonUpdated(AugmentaPerson p);
		public event PersonUpdated personUpdated;

		public delegate void PersonLeft(AugmentaPerson p);
		public event PersonLeft personLeft;

		public delegate void SceneUpdated();
		public event SceneUpdated sceneUpdated;

		public Dictionary<int, AugmentaPerson> augmentaPersons;
		public AugmentaScene augmentaScene;

		private List<int> _expiredPids = new List<int>(); //Used to remove timed out persons

		#region MonoBehaviour Functions

		private void Awake() {

			//Initialize scene
			InitializeAugmentaScene();

			//Initialize persons array
			augmentaPersons = new Dictionary<int, AugmentaPerson>();

			//Create OSC Receiver
			CreateAugmentaOSCReceiver();
		}

		private void Update() {

			//Check if persons are alive
			CheckAlive();

			//for (int i = 0; i < _orderedPids.Count; i++)
			//	Debug.Log("_orderedPids[" + i + "] = " + _orderedPids[i]);
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
			sceneObject.name = "Scene " + id;

			augmentaScene = sceneObject.GetComponent<AugmentaScene>();

			augmentaScene.augmentaManager = this;
			augmentaScene.showDebug = showDebug;
			augmentaScene.ShowDebug(showDebug);

			augmentaScene.UpdateScene();
		}

		/// <summary>
		/// Send the Augmenta event of corresponding type, according to the desired person.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="person"></param>
		public void SendAugmentaEvent(AugmentaEventType type, AugmentaPerson person = null) {

			switch (type) {
				case AugmentaEventType.PersonEntered:
					personEntered?.Invoke(person);
					break;

				case AugmentaEventType.PersonUpdated:
					personUpdated?.Invoke(person);
					break;

				case AugmentaEventType.PersonLeft:
					personLeft?.Invoke(person);
					break;

				case AugmentaEventType.SceneUpdated:
					sceneUpdated?.Invoke();
					break;
			}
		}

		/// <summary>
		/// Add new Augmenta person.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		private AugmentaPerson AddPerson(ArrayList args) {

			GameObject newPersonObject = Instantiate(augmentaPersonPrefab, augmentaScene.gameObject.transform);
			newPersonObject.name = "Person " + args[0];

			AugmentaPerson newPerson = newPersonObject.GetComponent<AugmentaPerson>();
			newPerson.augmentaManager = this;
			newPerson.showDebug = showDebug;
			newPerson.ShowDebug(showDebug);

			UpdatePerson(newPerson, args);
			newPerson.UpdatePerson(newPerson);

			augmentaPersons.Add(newPerson.pid, newPerson);

			return newPerson;
		}

		/// <summary>
		/// Update an Augmenta person from incoming data.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="args"></param>
		private void UpdatePerson(AugmentaPerson p, ArrayList args) {

			p.pid = (int)args[0];
			p.oid = (int)args[1];
			p.age = (int)args[2];
			Vector2 centroid = new Vector2((float)args[3], (float)args[4]);
			Vector2 velocity = new Vector2((float)args[5], (float)args[6]);
			p.depth = (float)args[7];
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

			p.centroid = centroid;
			p.velocity = velocity;
			p.boundingRect = boundingRect;
			p.highest = highest;

			//Inactive time reset to zero : the Person has just been updated
			p.inactiveTime = 0;
		}

		/// <summary>
		/// Remove a person with its pid
		/// </summary>
		/// <param name="pid"></param>
		public void RemovePerson(int pid) {

			Destroy(augmentaPersons[pid].gameObject);
			augmentaPersons.Remove(pid);
		}

		/// <summary>
		/// Remove all persons
		/// </summary>
		public void RemoveAllPersons() {

			while (augmentaPersons.Count > 0) {
				RemovePerson(augmentaPersons.ElementAt(0).Key);
			}
		}

		/// <summary>
		/// Return true if the person is desired (i.e. should be added/updated).
		/// </summary>
		/// <param name="pid"></param>
		/// <returns></returns>
		public bool IsPersonDesired(int oid) {

			if (desiredPersonType == DesiredPersonType.Oldest) {
				return oid < desiredPersonCount;
			} else if (desiredPersonType == DesiredPersonType.Newest) {
				return oid >= (augmentaScene.personCount - desiredPersonCount);
			} else {
				return true;
			}
		}

		/// <summary>
		/// Check if persons are alive
		/// </summary>
		void CheckAlive() {

			_expiredPids.Clear();

			foreach (int key in augmentaPersons.Keys) {

				if (augmentaPersons[key].inactiveTime < personTimeOut) {
					// We add a frame to the inactiveTime count
					augmentaPersons[key].inactiveTime += Time.deltaTime;
				} else {
					// The Person hasn't been updated for a certain number of frames : mark for removal
					_expiredPids.Add(key);
				}
			}

			//Remove expired persons
			foreach (int pid in _expiredPids) {
				SendAugmentaEvent(AugmentaEventType.PersonLeft, augmentaPersons[pid]);
				RemovePerson(pid);
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

			if (OSCMaster.CreateReceiver("Augmenta-" + id, inputPort) != null) {
				OSCMaster.Receivers["Augmenta-" + id].messageReceived += OSCMessageReceived;
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

			if (OSCMaster.Receivers.ContainsKey("Augmenta-" + id)) {
				OSCMaster.Receivers["Augmenta-" + id].messageReceived -= OSCMessageReceived;
				OSCMaster.RemoveReceiver("Augmenta-" + id);
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

			int pid, oid;
			AugmentaPerson currentPerson = null;

			switch (address) {

				case "/au/personEntered/":
				case "/au/personEntered":

					pid = (int)args[0];
					oid = (int)args[1];

					if (IsPersonDesired(oid)) {
						if (!augmentaPersons.ContainsKey(pid)) {
							//New person
							currentPerson = AddPerson(args);
							SendAugmentaEvent(AugmentaEventType.PersonEntered, currentPerson);
						} else {
							//Person was already there
							currentPerson = augmentaPersons[pid];
							UpdatePerson(currentPerson, args);
							SendAugmentaEvent(AugmentaEventType.PersonUpdated, currentPerson);
						}
					}

					break;

				case "/au/personUpdated/":
				case "/au/personUpdated":

					pid = (int)args[0];
					oid = (int)args[1];

					if (IsPersonDesired(oid)) {
						if (!augmentaPersons.ContainsKey(pid)) {
							currentPerson = AddPerson(args);
							SendAugmentaEvent(AugmentaEventType.PersonEntered, currentPerson);
						} else {
							currentPerson = augmentaPersons[pid];
							UpdatePerson(currentPerson, args);
							SendAugmentaEvent(AugmentaEventType.PersonUpdated, currentPerson);
						}
					}

					break;

				case "/au/personWillLeave/":
				case "/au/personWillLeave":

					pid = (int)args[0];
					oid = (int)args[1];

					if (IsPersonDesired(oid)) {
						if (augmentaPersons.ContainsKey(pid)) {
							currentPerson = augmentaPersons[pid];
							SendAugmentaEvent(AugmentaEventType.PersonLeft, currentPerson);
							RemovePerson(pid);
						}
					}

					break;

				case "/au/scene/":
				case "/au/scene":

					augmentaScene.personCount = (int)args[2];
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

			foreach(var person in augmentaPersons) {
				person.Value.showDebug = show;
			}
		}

		#endregion
	}
}