using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Augmenta;
using UnityOSC;

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
///  15:
///  16:
///  17:
///  18:
///  19:
///  20+ : contours (if enabled)

///  /au/scene/   args0 arg1...argn

///  0: currentTime (int)
///  1: percentCovered (float)
///  2: numPeople (int)
///  3: averageMotion.x (float)
///  4: averageMotion.y (float)
///  5: scene.width (int)
///  6: scene.height (int)

/// </summary>


public struct AugmentaScene
{
    public float Width;
    public float Height;
}

public enum AugmentaPersonType
{
    AllPeople,
    Oldest,
    Newest
};

public enum AugmentaEventType
{
    None,
    PersonEntered,
    PersonUpdated,
    PersonWillLeave,
    SceneUpdated
};

public enum ProtocolVersion
{
	v1,
	v2
};

public class AugmentaArea : MonoBehaviour  {

    [HideInInspector]
    public AugmentaCamera augmentaCamera;

    [Header("Augmenta settings")]
    public string augmentaAreaId;
    public static Dictionary<string, AugmentaArea> augmentaAreas;

    private bool _enableCameraRendering;
    public bool cameraRendering
    {
        get
        {
            return _enableCameraRendering;
        }
        set
        {
            _enableCameraRendering = value;
            augmentaCamera.GetComponent<Camera>().enabled = value;
        }
    }
    public int defaultInputPort;
    public bool connected;

    private int _inputPort = 12000;
    public int InputPort
    {
        get
        {
            return _inputPort;
        }
        set
        {
            _inputPort = value;
            connected = CreateAugmentaOSCListener();
        }
    }

	public ProtocolVersion protocolVersion = ProtocolVersion.v2;
    public float scaling = 1.0f;

	[Header("For protocol V1 only")]
	public float meterPerPixel = 0.005f;

	[HideInInspector]
    public float AspectRatio;

    [Header("Augmenta people settings")]
    public bool FlipX;
    public bool FlipY;
    // Number of seconds before a person who hasn't been updated is removed
    public float PersonTimeOut = 1.0f; // seconds
    public int NbAugmentaPeople;
    public AugmentaPersonType ActualPersonType;
    public int AskedPeople = 1;

    private float _oldMeterPerPixelCoeff, _oldScaling;

    [Header("Debug")]
    public bool Mute;
    public bool Mire;
    public AugmentaDebuggerManager AugmentaDebugger;

    [SerializeField]
    private bool _augmentaDebug;
    public bool AugmentaDebug
    {
        get
        {
            return _augmentaDebug;
        }
        set
        {
            _augmentaDebug = value;
            AugmentaDebugger.gameObject.SetActive(_augmentaDebug);
            AugmentaDebugger.Transparency = _debugTransparency;
        }
    }
    [SerializeField]
    [Range(0, 1)]
    private float _debugTransparency;
    public float DebugTransparency
    {
        get
        {
            return _debugTransparency;
        }
        set
        {
            _debugTransparency = value;
            AugmentaDebugger.Transparency = _debugTransparency;
        }
    }
    public bool DrawGizmos;
    private bool _connectedToAnchor = false;
    public bool connectedToAnchor
    {
        get {
            return _connectedToAnchor;
        }
        set
        {
            _connectedToAnchor = value;
            cameraRendering = value;
			if(spoutCamera != null)
				spoutCamera.enabled = value;
            
        }
    }

    public Camera spoutCamera;

	public List<TestCards.TestOverlay> overlays;

	public AugmentaScene AugmentaScene;

	/* Events */
	public delegate void PersonEntered(AugmentaPerson p);
    public event PersonEntered personEntered;

    public delegate void PersonUpdated(AugmentaPerson p);
    public event PersonUpdated personUpdated;

    public delegate void PersonLeaving(AugmentaPerson p);
    public event PersonLeaving personLeaving;

    public delegate void SceneUpdated(AugmentaScene s);
    public event SceneUpdated sceneUpdated;

	public Dictionary<int, AugmentaPerson> AugmentaPeople = new Dictionary<int, AugmentaPerson>(); // Containing all current persons
    private List<int> _orderedPids = new List<int>(); //Used to find oldest and newest



	#region MonoBehaviour Functions

	void Awake() {

		//Get the AugmentaCamera
		augmentaCamera = transform.GetComponentInChildren<AugmentaCamera>();

		cameraRendering = false;

		RegisterArea();

		_orderedPids = new List<int>();
		

		AspectRatio = 1;

		InputPort = defaultInputPort;
		connected = CreateAugmentaOSCListener();

		AugmentaScene = new AugmentaScene();

		StopAllCoroutines();
		// Start the coroutine that check if everyone is alive
		StartCoroutine("checkAlive");
		AugmentaDebugger.gameObject.SetActive(AugmentaDebug);
		AugmentaDebugger.Transparency = DebugTransparency;
	}

	private void Update() {
		AugmentaDebugger.gameObject.SetActive(_augmentaDebug); //Because Unity doesn't support Properties in Inspector
		AugmentaDebugger.Transparency = _debugTransparency;//Because Unity doesn't support Properties in Inspector

		if (_oldMeterPerPixelCoeff != meterPerPixel || _oldScaling != scaling) {
			_oldScaling = scaling;
			_oldMeterPerPixelCoeff = meterPerPixel;
			SendAugmentaEvent(AugmentaEventType.SceneUpdated);
		}

		foreach (var overlay in overlays)
			overlay.enabled = Mire;
	}

	public void OnDestroy() {
		Debug.Log("[Augmenta" + augmentaAreaId + "] Unsubscribing to OSC Message on " + InputPort);

		if (OSCMaster.Receivers.ContainsKey("AugmentaInput-" + augmentaAreaId)) {
			OSCMaster.Receivers["AugmentaInput-" + augmentaAreaId].messageReceived -= OSCMessageReceived;
			OSCMaster.RemoveReceiver("AugmentaInput-" + augmentaAreaId);
		}
	}

	void OnDrawGizmos() {
		if (!DrawGizmos) return;

		Gizmos.color = Color.red;
		DrawGizmoCube(transform.position, transform.rotation, transform.localScale);

		//Draw persons
		Gizmos.color = Color.green;
		foreach (var person in AugmentaPeople) {
			// Gizmos.DrawWireCube(person.Value.Position, new Vector3(person.Value.boundingRect.width * MeterPerPixel, person.Value.boundingRect.height * MeterPerPixel, person.Value.boundingRect.height * MeterPerPixel));
			DrawGizmoCube(person.Value.Position, Quaternion.identity, new Vector3(person.Value.boundingRect.width, person.Value.boundingRect.height, person.Value.boundingRect.height));
		}
	}

	#endregion

	#region Augmenta Functions

	void RegisterArea() {
		if (augmentaAreas == null)
			augmentaAreas = new Dictionary<string, AugmentaArea>();

		if (string.IsNullOrEmpty(augmentaAreaId))
			Debug.LogWarning("Augmenta area doesn't have an ID !");

		augmentaAreas.Add(augmentaAreaId, this);
	}

	public void ConnectToAnchor() {
		connectedToAnchor = true;
	}

	public void DisconnectFromAnchor() {
		connectedToAnchor = false;
	}

	public void SendAugmentaEvent(AugmentaEventType type, AugmentaPerson person = null) {
		if (ActualPersonType == AugmentaPersonType.Oldest && type != AugmentaEventType.SceneUpdated) {
			var askedOldest = GetOldestPersons(AskedPeople);
			if (!askedOldest.Contains(person))
				type = AugmentaEventType.PersonWillLeave;
		}

		if (ActualPersonType == AugmentaPersonType.Newest && type != AugmentaEventType.SceneUpdated) {
			var askedNewest = GetNewestPersons(AskedPeople);
			if (!askedNewest.Contains(person))
				type = AugmentaEventType.PersonWillLeave;
		}

		switch (type) {
			case AugmentaEventType.PersonEntered:
				if (personEntered != null)
					personEntered(person);
				break;

			case AugmentaEventType.PersonUpdated:
				if (personUpdated != null)
					personUpdated(person);
				break;

			case AugmentaEventType.PersonWillLeave:
				if (personLeaving != null)
					personLeaving(person);
				break;

			case AugmentaEventType.SceneUpdated:
				if (sceneUpdated != null)
					sceneUpdated(AugmentaScene);
				break;
		}
	}

	public bool HasObjects() {
		if (AugmentaPeople.Count >= 1)
			return true;
		else
			return false;
	}

	public int arrayPersonCount() {
		return AugmentaPeople.Count;
	}

	public Dictionary<int, AugmentaPerson> getPeopleArray() {
		return AugmentaPeople;
	}

	private AugmentaPerson addPerson(ArrayList args) {
		AugmentaPerson newPerson = new AugmentaPerson();
		newPerson.Init();
		updatePerson(newPerson, args);
		AugmentaPeople.Add(newPerson.pid, newPerson);

		return newPerson;
	}

	private void updatePerson(AugmentaPerson p, ArrayList args) {
		p.pid = (int)args[0];
		p.oid = (int)args[1];
		p.age = (int)args[2];
		var centroid = new Vector3((float)args[3], (float)args[4]);
		var velocity = new Vector3((float)args[5], (float)args[6]);
		var boudingRect = new Vector3((float)args[8], (float)args[9]);
		var highest = new Vector3((float)args[12], (float)args[13]);
		if (FlipX) {
			centroid.x = 1 - centroid.x;
			velocity.x = -velocity.x;
			boudingRect.x = 1 - boudingRect.x;
			highest.x = 1 - highest.x;
		}
		if (FlipY) {
			centroid.y = 1 - centroid.y;
			velocity.y = -velocity.y;
			boudingRect.y = 1 - boudingRect.y;
			highest.y = 1 - highest.y;
		}

		p.centroid = centroid;
		p.AddVelocity(velocity);

		p.depth = (float)args[7];
		p.boundingRect.x = boudingRect.x;
		p.boundingRect.y = boudingRect.y;
		p.boundingRect.width = (float)args[10];
		p.boundingRect.height = (float)args[11];
		p.highest.x = highest.x;
		p.highest.y = highest.y;
		p.highest.z = (float)args[14];

		p.Position = transform.TransformPoint(new Vector3(-(p.centroid.x - 0.5f), -(p.centroid.y - 0.5f), p.centroid.z));

		// Inactive time reset to zero : the Person has just been updated
		p.inactiveTime = 0;

		if (!_orderedPids.Contains(p.pid) && p.oid <= _orderedPids.Count) {
			_orderedPids.Insert(p.oid, p.pid);
		}
		//_orderedPids.Sort(delegate (int x, int y)
		//{
		//    if (x == y) return 0;
		//    else if (x < y) return -1;
		//    else return 1;
		//});
	}

	public void clearAllPersons() {
		AugmentaPeople.Clear();
	}

	public List<AugmentaPerson> GetOldestPersons(int count) {
		var oldestPersons = new List<AugmentaPerson>();

		if (count > _orderedPids.Count)
			count = _orderedPids.Count;

		if (count < 0)
			count = 0;

		var oidRange = _orderedPids.GetRange(0, count);
		// Debug.Log("Orderedoid size : " + _orderedPids.Count + "augmentaPersons size " + AugmentaPeople.Count + "oidRange size : " + oidRange.Count);
		for (var i = 0; i < oidRange.Count; i++) {
			// if (AugmentaPeople.ContainsKey(oidRange[i]))
			oldestPersons.Add(AugmentaPeople[oidRange[i]]);
		}

		//Debug.Log("Oldest count : " + oldestPersons.Count);
		return oldestPersons;
	}

	public List<AugmentaPerson> GetNewestPersons(int count) {
		var newestPersons = new List<AugmentaPerson>();

		if (count > AugmentaPeople.Count)
			count = _orderedPids.Count;

		if (count < 0)
			count = 0;

		var oidRange = _orderedPids.GetRange(_orderedPids.Count - count, count);
		// Debug.Log("Orderedoid size : " + _orderedPids.Count + "augmentaPersons size " + AugmentaPeople.Count + "oidRange size : " + oidRange.Count);
		for (var i = 0; i < oidRange.Count; i++) {
			//if(AugmentaPeople.ContainsKey(oidRange[i]))
			newestPersons.Add(AugmentaPeople[oidRange[i]]);
		}

		//Debug.Log("newestPersons count : " + newestPersons.Count);
		return newestPersons;
	}

	// Co-routine to check if person is alive or not
	IEnumerator checkAlive() {
		while (true) {
			ArrayList ids = new ArrayList();
			foreach (KeyValuePair<int, AugmentaPerson> p in AugmentaPeople) {
				ids.Add(p.Key);
			}
			foreach (int id in ids) {
				if (AugmentaPeople.ContainsKey(id)) {

					AugmentaPerson p = AugmentaPeople[id];

					if (p.inactiveTime < PersonTimeOut) {
						//Debug.Log("***: IS ALIVE");
						// We add a frame to the inactiveTime count
						p.inactiveTime += Time.deltaTime;
					} else {
						//Debug.Log("***: DESTROY");
						// The Person hasn't been updated for a certain number of frames : remove
						SendAugmentaEvent(AugmentaEventType.PersonWillLeave, p);
						AugmentaPeople.Remove(id);
					}
				}
			}
			ids.Clear();
			yield return 0;
		}
	}

	#endregion

	#region OSC Functions 

	public bool CreateAugmentaOSCListener() {
		Debug.Log("[Augmenta" + augmentaAreaId + "] Subscribing to OSC Message on " + InputPort);
		if (OSCMaster.Receivers.ContainsKey("AugmentaInput-" + augmentaAreaId)) {
			OSCMaster.Receivers["AugmentaInput-" + augmentaAreaId].messageReceived -= OSCMessageReceived;
			OSCMaster.RemoveReceiver("AugmentaInput-" + augmentaAreaId);
		}
		if (OSCMaster.CreateReceiver("AugmentaInput-" + augmentaAreaId, InputPort) != null) {
			OSCMaster.Receivers["AugmentaInput-" + augmentaAreaId].messageReceived += OSCMessageReceived;
			return true;
		} else {
			return false;
		}
	}

	public void OSCMessageReceived(OSCMessage message) {

		if (Mute) return;

		string address = message.Address;
		ArrayList args = new ArrayList(message.Data); //message.Data.ToArray();

		//Debug.Log("OSC received with address : "+address);

		if (address == "/au/personEntered/" || address == "/au/personEntered") {
			int pid = (int)args[0];
			AugmentaPerson currentPerson = null;
			if (!AugmentaPeople.ContainsKey(pid)) {
				currentPerson = addPerson(args);
				SendAugmentaEvent(AugmentaEventType.PersonEntered, currentPerson);
			} else {
				currentPerson = AugmentaPeople[pid];
				updatePerson(currentPerson, args);
				SendAugmentaEvent(AugmentaEventType.PersonUpdated, currentPerson);
			}

		} else if (address == "/au/personUpdated/" || address == "/au/personUpdated") {
			int pid = (int)args[0];
			AugmentaPerson currentPerson = null;
			if (!AugmentaPeople.ContainsKey(pid)) {
				currentPerson = addPerson(args);
				SendAugmentaEvent(AugmentaEventType.PersonEntered, currentPerson);
			} else {
				currentPerson = AugmentaPeople[pid];
				updatePerson(currentPerson, args);
				SendAugmentaEvent(AugmentaEventType.PersonUpdated, currentPerson);
			}
		} else if (address == "/au/personWillLeave/" || address == "/au/personWillLeave") {
			int pid = (int)args[0];
			if (AugmentaPeople.ContainsKey(pid)) {
				AugmentaPerson personToRemove = AugmentaPeople[pid];
				SendAugmentaEvent(AugmentaEventType.PersonWillLeave, personToRemove);
				_orderedPids.Remove(personToRemove.pid);
				//_orderedPids.Sort(delegate (int x, int y)
				//{
				//    if (x == y) return 0;
				//    else if (x < y) return -1;
				//    else return 1;
				//});
				AugmentaPeople.Remove(pid);
			}
		} else if (address == "/au/scene/" || address == "/au/scene") {

			if (protocolVersion == ProtocolVersion.v1) {
				AugmentaScene.Width = (int)args[5];
				AugmentaScene.Height = (int)args[6];

				transform.localScale = new Vector3(AugmentaScene.Width * meterPerPixel * scaling, AugmentaScene.Height * meterPerPixel * scaling, 1.0f);

			} else if (protocolVersion == ProtocolVersion.v2) {
				AugmentaScene.Width = (float)args[5];
				AugmentaScene.Height = (float)args[6];

				transform.localScale = new Vector3(AugmentaScene.Width * scaling, AugmentaScene.Height * scaling, 1.0f);
			}

			AspectRatio = (AugmentaScene.Width / AugmentaScene.Height);

			SendAugmentaEvent(AugmentaEventType.SceneUpdated);
		} else {
			print(address + " ");
		}

		NbAugmentaPeople = AugmentaPeople.Count;
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

}
