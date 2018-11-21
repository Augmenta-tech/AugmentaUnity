using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Augmenta;
using UnityOSC;

/// <summary>
/// The AugmentaArea handles the incoming Augmenta OSC messages and updates the AugmentaPersons list and AugmentaScene accordingly.
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
/// Zoom: Coefficient applied on the MeterPerPixel value in order to roughly correct miscalibrations. If the value of MeterPerPixel is accurate, the Zoom value should be 1.
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
    AllPersons,
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

public class AugmentaArea : MonoBehaviour  {

    public static AugmentaArea Instance;
    public static Camera AugmentaCam;

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
    [Range(0,1)]
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

    [Header("Augmenta settings")]
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
            CreateAugmentaOSCListener();
        }
    }
    public float MeterPerPixel= 0.01f;
    public float Zoom;

    [HideInInspector]
    public float AspectRatio;

    [Header("Augmenta persons settings")]
    public bool FlipX;
    public bool FlipY;
    // Number of seconds before a person who hasn't been updated is removed
    public float PersonTimeOut = 1.0f; // seconds
    public int NbAugmentaPersons;
    public AugmentaPersonType ActualPersonType;
    public int AskedPersons = 1;

    private float _oldPixelMeterCoeff, _oldZoom;

    /* Events */
    public delegate void PersonEntered(AugmentaPerson p);
    public static event PersonEntered personEntered;

    public delegate void PersonUpdated(AugmentaPerson p);
    public static event PersonUpdated personUpdated;

    public delegate void PersonLeaving(AugmentaPerson p);
    public static event PersonLeaving personLeaving;

    public delegate void SceneUpdated(AugmentaScene s);
    public static event SceneUpdated sceneUpdated;

	public static Dictionary<int, AugmentaPerson> AugmentaPersons = new Dictionary<int, AugmentaPerson>(); // Containing all current persons
    private static List<int> _orderedPids = new List<int>(); //Used to find oldest and newest

    private TestCards.TestOverlay[] overlays;


    public static AugmentaScene AugmentaScene;

	void Awake(){
        Instance = this;
        _orderedPids = new List<int>();
        AugmentaCam = transform.Find("AugmentaCamera").GetComponent<Camera>();
        AspectRatio = 1;

        Debug.Log("[Augmenta] Subscribing to OSC Message Receiver");

        CreateAugmentaOSCListener();

        AugmentaScene = new AugmentaScene();
        
        StopAllCoroutines();
		// Start the coroutine that check if everyone is alive
		StartCoroutine("checkAlive");

        overlays = FindObjectsOfType<TestCards.TestOverlay>();
        AugmentaDebugger.gameObject.SetActive(AugmentaDebug);
        AugmentaDebugger.Transparency = DebugTransparency;
    }

	public void OnDestroy(){
		Debug.Log("[Augmenta] Unsubscribing to OSC Message Receiver");

    }

    public void CreateAugmentaOSCListener()
    {
        if(OSCMaster.Receivers.ContainsKey("AugmentaInput"))
        {
            OSCMaster.Receivers["AugmentaInput"].messageReceived -= OSCMessageReceived;
            OSCMaster.RemoveReceiver("AugmentaInput");
        }
        OSCMaster.CreateReceiver("AugmentaInput", InputPort).messageReceived += OSCMessageReceived; // TODO : Remove link to OCF
    }

	public void OSCMessageReceived(OSCMessage message){

        if (Mute) return;

        if (message == null)
            return;

        string address = message.Address;
		ArrayList args = new ArrayList(message.Data); //message.Data.ToArray();

        //Debug.Log("OSC received with address : "+address);

        if (address == "/au/personEntered/" || address == "/au/personEntered")
        {
            int pid = (int)args[0];
            AugmentaPerson currentPerson = null;
            if (!AugmentaPersons.ContainsKey(pid))
            {
                currentPerson = addPerson(args);
                SendAugmentaEvent(AugmentaEventType.PersonEntered, currentPerson);
            }
            else
            {
                currentPerson = AugmentaPersons[pid];
                updatePerson(currentPerson, args);
                SendAugmentaEvent(AugmentaEventType.PersonUpdated, currentPerson);
            }

        }
        else if (address == "/au/personUpdated/" || address == "/au/personUpdated")
        {
            int pid = (int)args[0];
            AugmentaPerson currentPerson = null;
            if (!AugmentaPersons.ContainsKey(pid))
            {
                currentPerson = addPerson(args);
                SendAugmentaEvent(AugmentaEventType.PersonEntered, currentPerson);
            }
            else
            {
                currentPerson = AugmentaPersons[pid];
                updatePerson(currentPerson, args);
                SendAugmentaEvent(AugmentaEventType.PersonUpdated, currentPerson);
            }
        }
        else if (address == "/au/personWillLeave/" || address == "/au/personWillLeave")
        {
            int pid = (int)args[0];
            if (AugmentaPersons.ContainsKey(pid))
            {
                AugmentaPerson personToRemove = AugmentaPersons[pid];
                SendAugmentaEvent(AugmentaEventType.PersonWillLeave, personToRemove);
                _orderedPids.Remove(personToRemove.pid);
                _orderedPids.Sort(delegate (int x, int y)
                {
                    if (x == y) return 0;
                    else if (x < y) return -1;
                    else return 1;
                });
                AugmentaPersons.Remove(pid);
            }
        }
        else if (address == "/au/scene/" || address == "/au/scene")
        {
            AugmentaScene.Width = (int)args[5];
            AugmentaScene.Height = (int)args[6];

            AspectRatio = (AugmentaScene.Width / AugmentaScene.Height);
            transform.localScale = new Vector3(AugmentaScene.Width * (MeterPerPixel) * Zoom, AugmentaScene.Height *(MeterPerPixel) * Zoom, 1.0f);

            SendAugmentaEvent(AugmentaEventType.SceneUpdated);
        }
        else
        {
            print(address + " ");
        }
	}

    private void Update()
    {
        AugmentaDebugger.gameObject.SetActive(_augmentaDebug); //Because Unity doesn't support Properties in Inspector
        AugmentaDebugger.Transparency = _debugTransparency;//Because Unity doesn't support Properties in Inspector

        if (_oldPixelMeterCoeff != MeterPerPixel || _oldZoom != Zoom)
        {
            _oldZoom = Zoom;
            _oldPixelMeterCoeff = MeterPerPixel;
            SendAugmentaEvent(AugmentaEventType.SceneUpdated);
        }

        foreach(var overlay in overlays)
            overlay.enabled = Mire;
    }


    public void SendAugmentaEvent(AugmentaEventType type, AugmentaPerson person = null)
    {
        if (ActualPersonType == AugmentaPersonType.Oldest && type != AugmentaEventType.SceneUpdated)
        {
            var askedOldest = GetOldestPersons(AskedPersons);
            if (!askedOldest.Contains(person))
                type = AugmentaEventType.PersonWillLeave;
        }

        if (ActualPersonType == AugmentaPersonType.Newest && type != AugmentaEventType.SceneUpdated)
        {
            var askedNewest = GetNewestPersons(AskedPersons);
            if (!askedNewest.Contains(person))
                type = AugmentaEventType.PersonWillLeave;
        }

        switch (type)
        {
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

    public static bool HasObjects()
    {
        if (AugmentaPersons.Count >= 1)
            return true;
        else
            return false;
    }

    public int arrayPersonCount()
    {
        return AugmentaPersons.Count;
    }

    public static Dictionary<int, AugmentaPerson> getPeopleArray()
    {
        return AugmentaPersons;
    }


    void OnDrawGizmos()
    {
        

        if (!DrawGizmos) return;

        Gizmos.color = Color.red;
        DrawGizmoCube(transform.position, transform.rotation, transform.localScale);

        //Draw persons
        Gizmos.color = Color.green;
        foreach (var person in AugmentaPersons)
        {
            // Gizmos.DrawWireCube(person.Value.Position, new Vector3(person.Value.boundingRect.width * MeterPerPixel, person.Value.boundingRect.height * MeterPerPixel, person.Value.boundingRect.height * MeterPerPixel));
            DrawGizmoCube(person.Value.Position, Quaternion.identity, new Vector3(person.Value.boundingRect.width, person.Value.boundingRect.height, person.Value.boundingRect.height));
        }
    }

    public void DrawGizmoCube(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

        Gizmos.matrix *= cubeTransform;

        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = oldGizmosMatrix;
    }

    private AugmentaPerson addPerson(ArrayList args) {
		AugmentaPerson newPerson = new AugmentaPerson();
        newPerson.Init();
		updatePerson(newPerson, args);
        AugmentaPersons.Add(newPerson.pid, newPerson);
        _orderedPids.Add(newPerson.pid);
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
        if (FlipX)
        {
            centroid.x = 1 - centroid.x;
            velocity.x = -velocity.x;
            boudingRect.x = 1 - boudingRect.x;
            highest.x = 1 - highest.x;
        }
        if (FlipY)
        {
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

        NbAugmentaPersons = AugmentaPersons.Count;
        p.Position = transform.TransformPoint(new Vector3(p.centroid.x - 0.5f, -(p.centroid.y - 0.5f), p.centroid.z));

        // Inactive time reset to zero : the Person has just been updated
        p.inactiveTime = 0;

        _orderedPids.Sort(delegate (int x, int y)
        {
            if (x == y) return 0;
            else if (x < y) return -1;
            else return 1;
        });
    }

    public void clearAllPersons() {
        AugmentaPersons.Clear();
    }

    public static List<AugmentaPerson> GetOldestPersons(int count)
    {
        var oldestPersons = new List<AugmentaPerson>();

        if (count > _orderedPids.Count)
            count = _orderedPids.Count;

        if (count < 0)
            count = 0;

        var oidRange = _orderedPids.GetRange(0, count);
       // Debug.Log("Orderedoid size : " + _orderedPids.Count + "augmentaPersons size " + AugmentaPersons.Count + "oidRange size : " + oidRange.Count);
        for (var i=0; i < oidRange.Count; i++)
        {
            if (AugmentaPersons.ContainsKey(oidRange[i]))
                oldestPersons.Add(AugmentaPersons[oidRange[i]]);
        }
        
        //Debug.Log("Oldest count : " + oldestPersons.Count);
        return oldestPersons;
    }

    public static List<AugmentaPerson> GetNewestPersons(int count)
    {
        var newestPersons = new List<AugmentaPerson>();

        if (count > _orderedPids.Count)
            count = _orderedPids.Count;

        if (count < 0)
            count = 0;

        var oidRange = _orderedPids.GetRange(_orderedPids.Count - count, count);
        // Debug.Log("Orderedoid size : " + _orderedPids.Count + "augmentaPersons size " + AugmentaPersons.Count + "oidRange size : " + oidRange.Count);
        for (var i = 0; i < oidRange.Count; i++)
        {
            if(AugmentaPersons.ContainsKey(oidRange[i]))
                newestPersons.Add(AugmentaPersons[oidRange[i]]);
        }

        //Debug.Log("Oldest count : " + oldestPersons.Count);
        return newestPersons;
    }

    // Co-routine to check if person is alive or not
    IEnumerator checkAlive() {
		while(true) {
			ArrayList ids = new ArrayList();
			foreach(KeyValuePair<int, AugmentaPerson> p in AugmentaPersons) {
				ids.Add(p.Key);
			}
			foreach(int id in ids) {
				if(AugmentaPersons.ContainsKey(id)){

					AugmentaPerson p = AugmentaPersons[id];

					if(p.inactiveTime < PersonTimeOut) {
						//Debug.Log("***: IS ALIVE");
						// We add a frame to the inactiveTime count
						p.inactiveTime += Time.deltaTime;
					} else {
                        //Debug.Log("***: DESTROY");
                        // The Person hasn't been updated for a certain number of frames : remove
                        SendAugmentaEvent(AugmentaEventType.PersonWillLeave, p);
                        AugmentaPersons.Remove(id);
                    }
				}
			}
			ids.Clear();
			yield return 0;
		}
	}
}
