using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaPersonDebugger : MonoBehaviour {

    public Augmenta.AugmentaPerson MyAugmentaPerson;

    public Color BorderColor;
    public TextMesh PointInfoText;
    public Transform Point;
    public Transform VelocityVisualizer;
    public float VelocityThickness;

    private void Start()
    {
        transform.Find("Cube").GetComponent<Renderer>().material.SetColor("_BorderColor", BorderColor);
    }

    // Update is called once per frame
    void Update () {
        if (MyAugmentaPerson == null)
            return;
        else
            transform.localScale = Vector3.one;

        //Update bouding box
        Point.transform.localScale = new Vector3(MyAugmentaPerson.boundingRect.width * AugmentaArea.Instance.transform.localScale.x, MyAugmentaPerson.boundingRect.height * AugmentaArea.Instance.transform.localScale.y, 0.1f);

        //udpate text
        PointInfoText.text = "PID : " + MyAugmentaPerson.pid + '\n' + "OID : " + MyAugmentaPerson.oid;

        //Update velocity
        float angle = Mathf.Atan2(MyAugmentaPerson.GetSmoothedVelocity().y, MyAugmentaPerson.GetSmoothedVelocity().x)*180 / Mathf.PI;
        if (float.IsNaN(angle))
            return;
        VelocityVisualizer.localRotation = Quaternion.Euler(new Vector3(0, 0, -angle  +90));
        VelocityVisualizer.localScale = new Vector3(VelocityThickness, MyAugmentaPerson.GetSmoothedVelocity().magnitude * 100, VelocityThickness);
    }
}
