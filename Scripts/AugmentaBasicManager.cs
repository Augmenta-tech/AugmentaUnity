using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Augmenta;

public class AugmentaBasicManager : MonoBehaviour {

    /// <summary>
    /// The AugmentaBasicManager is an example of Augmenta manager that instantiate and move prefabs according to the AugmentaArea events and AugmentaPersons list.
    /// 
    /// A prefab is intantiated when the personEntered event is received.
    /// When a personLeaving event is received, the AugmentaPersonBehaviour's Disappear animation of the prefab is played. If the prefab has no AugmentaPersonBehaviour, it is destroyed instantly.
    /// The instantiated objects are moved each frame according to follow the AugmentaPersons.
    /// 
    /// 
    /// </summary>
    public GameObject PrefabToInstantiate;

    public Dictionary<int, GameObject> InstantiatedObjects;

    [Tooltip("In seconds")]
    private float _personTimeOut = 1;
    public float PersonTimeOut {
        get
        {
            return _personTimeOut;
        }
        set
        {
            _personTimeOut = value;
            AugmentaArea.Instance.PersonTimeOut = _personTimeOut;
        }
    }

    [Range(1, 20)]
    public float PositionFollowTightness = 10;

    [Range(1, 20)]
    public int VelocityAverageValueCount = 1;

    public virtual void Update()
    {
        //for object to always face AugmentaCamera
        if(AugmentaArea.Instance)
            transform.rotation = AugmentaArea.Instance.transform.rotation;

        foreach (var element in InstantiatedObjects)
        {
            if (!AugmentaArea.AugmentaPersons.ContainsKey(element.Key)) continue;

            element.Value.transform.position = Vector3.Lerp(element.Value.transform.position, AugmentaArea.AugmentaPersons[element.Key].Position, Time.deltaTime * PositionFollowTightness);
        }
    }

	// Use this for initialization
	public virtual void OnEnable () {
        InstantiatedObjects = new Dictionary<int, GameObject>();

        AugmentaArea.personEntered += PersonEntered;
        AugmentaArea.personUpdated += PersonUpdated;
        AugmentaArea.personLeaving += PersonLeft;
        AugmentaArea.sceneUpdated += SceneUpdated;
    }

    // Use this for initialization
    public virtual void OnDisable()
    {
        foreach (var element in InstantiatedObjects.Values)
            Destroy(element);

        InstantiatedObjects.Clear();

        AugmentaArea.personEntered -= PersonEntered;
        AugmentaArea.personUpdated -= PersonUpdated;
        AugmentaArea.personLeaving -= PersonLeft;
        AugmentaArea.sceneUpdated -= SceneUpdated;
    }

    public virtual void SceneUpdated(AugmentaScene s)
    { }

    public virtual void PersonEntered(AugmentaPerson p)
    {
        if(!InstantiatedObjects.ContainsKey(p.pid))
        {
            var newObject = Instantiate(PrefabToInstantiate, p.Position, Quaternion.identity, this.transform);
            newObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
            InstantiatedObjects.Add(p.pid, newObject);

            var augBehaviour = newObject.GetComponent<AugmentaPersonBehaviour>();
            if (augBehaviour != null)
            {
                augBehaviour.pid = p.pid;
                augBehaviour.disappearAnimationCompleted += HandleDisappearedObject;
                augBehaviour.Appear();
            }
        }
    }

    public virtual void PersonUpdated(AugmentaPerson p)
    {
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
}
