using System.Collections;
using System.Collections.Generic;
using System;
using Augmenta;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class AugmentaDebuggerManager : AugmentaAreaAnchor {

    public GameObject Background;
    private float _transparency;
    public float Transparency
    {
        get
        {
            return _transparency;
        }
        set
        {
            _transparency = value;
            var renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.material.SetFloat("_Transparency", _transparency);
                if(renderer.gameObject.name == "Text")//Font shader are different, if I could put a Transparency variable we could remove this
                {
                    var textColor = renderer.material.GetColor("_TextColor"); 
                    renderer.material.SetColor("_TextColor", new Vector4(textColor.r, textColor.g, textColor.b, _transparency));
                }
            }
        }
    }

	public override void OnEnable() {
		base.OnEnable();

		//Disable the post process of the cameras
		augmentaCameraAnchor.GetComponent<PostProcessLayer>().enabled = false;
        augmentaCameraAnchor.linkedAugmentaArea.spoutCamera.GetComponent<PostProcessLayer>().enabled = false;
    }

	public override void OnDisable() {
		base.OnDisable();

		//Enable the post process of the cameras
		augmentaCameraAnchor.GetComponent<PostProcessLayer>().enabled = true;
        augmentaCameraAnchor.linkedAugmentaArea.spoutCamera.GetComponent<PostProcessLayer>().enabled = true;
    }

	public override void Update()
    {
        PositionFollowTightness = 20; //To prevent people from touching it

        //because it is under aAugmnetaArea and is scaled by it, to keep correct aspect ratio
        transform.localScale = new Vector3(1 / linkedAugmentaArea.transform.localScale.x, 1 / linkedAugmentaArea.transform.localScale.y, 1);

        Background.transform.localScale = new Vector3(linkedAugmentaArea.transform.localScale.x, linkedAugmentaArea.transform.localScale.y, 0.01f); ;
        Background.GetComponent<Renderer>().material.mainTextureScale = linkedAugmentaArea.transform.localScale * 0.5f; //because texture is made of 4 same size squares ;
        base.Update();
    }

    public override void PersonEntered(AugmentaPerson p)
    {
        base.PersonEntered(p);
        InstantiatedObjects[p.pid].GetComponent<AugmentaPersonDebugger>().BorderColor = Color.HSVToRGB(UnityEngine.Random.value, 0.85f, 0.75f);
        InstantiatedObjects[p.pid].GetComponent<AugmentaPersonDebugger>().AugmentaAreaAnchor = this;
    }

    public override void PersonUpdated(AugmentaPerson p)
    {

        if (InstantiatedObjects.ContainsKey(p.pid))
        {
            InstantiatedObjects[p.pid].GetComponent<AugmentaPersonDebugger>().MyAugmentaPerson = p;
            p.VelocitySmooth = VelocityAverageValueCount;
        }
        else
        {
            PersonEntered(p);
        }
    }
}
