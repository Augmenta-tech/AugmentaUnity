using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Augmenta;

/// <summary>
/// The AugmentaAreaAnchor is an object used to pre-visualize an AugmentaArea in your scene. 
/// You can set its size to the future size of the AugmentaArea (if you know it in advance) to better prepare the rest of your environment. 
/// You should place the AugmentaAreaAnchor where you want the AugmentaArea to be in your scene as it will always update the AugmentaArea position and rotation to match its own position and rotation. 
/// The link between an AugmentaAreaAnchor and an AugmentaArea is done via the AugmentaAreaID and LinkedAugmentaAreaID strings (for the AugmentaArea and AugmentaAreaAnchor respectively). 
/// Each AugmentaAreaAnchor should have an ID matching the ID of an AugmentaArea, and different from every other AugmentaAreaAnchor IDs.
/// </summary>

public class AugmentaAreaAnchor : MonoBehaviour {

    public string linkedAugmentaAreaId;

    [Header("Augmenta points settings")]
    public GameObject PrefabToInstantiate;

    public Dictionary<int, GameObject> InstantiatedObjects;

    [Tooltip("In seconds")]
    private float _personTimeOut = 1;
    public float PersonTimeOut
    {
        get
        {
            return _personTimeOut;
        }
        set
        {
            _personTimeOut = value;
            linkedAugmentaArea.PersonTimeOut = _personTimeOut;
        }
    }

    [Range(0f, 1)]
    public float PositionFollowTightness = 0.9f;

    [Range(1, 20)]
    public int VelocityAverageValueCount = 1;

    [Header("Augmenta Area Visualization")]
    public float Width = 1280;
    public float Height = 800;
    public float meterPerPixel = 0.005f;
	public bool DrawGizmos;

    [Header("Augmenta Area scene settings")]
    public float scaling = 1.0f;

    [Header("Augmenta Camera")]
	public AugmentaCameraAnchor augmentaCameraAnchor;

	private float _distanceToArea = 5.0f;
    public float distanceToArea {
        get {
            return _distanceToArea;
        }
        set
        {
            _distanceToArea = value;

			if (!augmentaCameraAnchor)
				return;

			//augmentaCameraAnchor.transform.localPosition = new Vector3(augmentaCameraAnchor.transform.localPosition.x, augmentaCameraAnchor.transform.localPosition.y, _distanceToArea);
//			augmentaCameraAnchor.UpdateTargetCamera(true, false, false);
        }
    }
    
	[Tooltip("Copy the AugmentaCameraAnchor transform to the AugmentaCamera on scene change to correct potentiel camera movement introduced by the scene scaling.")]
	public bool preserveCameraTransformOnSceneChange = true;

	[HideInInspector]
	public AugmentaArea linkedAugmentaArea;

	#region MonoBehaviour Functions

	public virtual void Awake()
    {
        if (string.IsNullOrEmpty(linkedAugmentaAreaId))
            Debug.LogWarning("linkedAugmentaAreaId is empty !");

        if (augmentaCameraAnchor == null)
            augmentaCameraAnchor = transform.GetChild(0).GetComponent<AugmentaCameraAnchor>();

        linkedAugmentaArea = AugmentaArea.augmentaAreas[linkedAugmentaAreaId];
        linkedAugmentaArea.ConnectToAnchor();

		augmentaCameraAnchor.linkedAugmentaArea = linkedAugmentaArea;
		augmentaCameraAnchor.InitializeTargetCamera();
    }

	// Use this for initialization
	public virtual void OnEnable() {
		InstantiatedObjects = new Dictionary<int, GameObject>();

		linkedAugmentaArea.personEntered += PersonEntered;
		linkedAugmentaArea.personUpdated += PersonUpdated;
		linkedAugmentaArea.personLeaving += PersonLeft;
		linkedAugmentaArea.sceneUpdated += SceneUpdated;
	}

	public virtual void Update () {


        foreach (var element in InstantiatedObjects)
        {
            if (!linkedAugmentaArea.AugmentaPeople.ContainsKey(element.Key)) continue;

            element.Value.transform.position = Vector3.Lerp(element.Value.transform.position, linkedAugmentaArea.AugmentaPeople[element.Key].Position, PositionFollowTightness);
        }
        if (linkedAugmentaArea)
        {
            UpdateAugmentaArea();
        }
    }

    // Use this for initialization
    public virtual void OnDisable()
    {
        foreach (var element in InstantiatedObjects.Values)
            Destroy(element);

        InstantiatedObjects.Clear();

        linkedAugmentaArea.personEntered -= PersonEntered;
        linkedAugmentaArea.personUpdated -= PersonUpdated;
        linkedAugmentaArea.personLeaving -= PersonLeft;
        linkedAugmentaArea.sceneUpdated -= SceneUpdated;
    }

	public virtual void OnDestroy() {
		linkedAugmentaArea.DisconnectFromAnchor();
	}

	public virtual void OnDrawGizmos() {
		if (!DrawGizmos) return;

		Gizmos.color = Color.blue;

		//Draw area 
		DrawGizmoCube(transform.position, transform.rotation, new Vector3(Width * meterPerPixel * scaling, Height * meterPerPixel * scaling, 1.0f));
	}


    #endregion

    #region Augmenta Functions

    public void UpdateAugmentaArea() {
		linkedAugmentaArea.transform.position = transform.position;
		linkedAugmentaArea.transform.rotation = transform.rotation;
        linkedAugmentaArea.scaling = scaling;
	}

	public virtual void SceneUpdated(AugmentaScene s)
    {
		if (preserveCameraTransformOnSceneChange) {
			//Update the position of the augmentaCamera when the scene change as it may have moved the augmenta camera
			augmentaCameraAnchor.UpdateTargetCamera(true, false, false);
		}
	}

    public virtual void PersonEntered(AugmentaPerson p)
    {
        if (!InstantiatedObjects.ContainsKey(p.pid))
        {
            if (PrefabToInstantiate == null)
                return;

            var newObject = Instantiate(PrefabToInstantiate, p.Position, Quaternion.identity, this.transform);
            newObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
            InstantiatedObjects.Add(p.pid, newObject);

            var augBehaviour = newObject.GetComponent<AugmentaPersonBehaviour>();
            if (augBehaviour != null)
            {
                augBehaviour.augmentaAreaAnchor = this;
                augBehaviour.pid = p.pid;
                augBehaviour.disappearAnimationCompleted += HandleDisappearedObject;
                augBehaviour.Appear();
            }
        }
    }

    public virtual void PersonUpdated(AugmentaPerson p)
    {
        //Debug.Log("Person updated : " + p.pid);
        if (InstantiatedObjects.ContainsKey(p.pid))
        {
            p.VelocitySmooth = VelocityAverageValueCount;
        }
        else
        {
            PersonEntered(p);
        }
    }

    public virtual void PersonLeft(AugmentaPerson p)
    {
        if (InstantiatedObjects.ContainsKey(p.pid))
        {
            var augBehaviour = InstantiatedObjects[p.pid].GetComponent<AugmentaPersonBehaviour>();
            if (augBehaviour != null)
                augBehaviour.Disappear();
            else
                HandleDisappearedObject(p.pid);
        }
    }

    public virtual void HandleDisappearedObject(int pid)
    {
        if (!InstantiatedObjects.ContainsKey(pid)) //To investigate, shouldn't happen
            return;

        var objectToDestroy = InstantiatedObjects[pid];
        Destroy(objectToDestroy);
        InstantiatedObjects.Remove(pid);
    }

	#endregion

	#region Gizmos Functions

	public virtual void DrawGizmoCube(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

        Gizmos.matrix *= cubeTransform;

        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = oldGizmosMatrix;
    }

	#endregion
}
