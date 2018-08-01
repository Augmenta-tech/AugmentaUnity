using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaPersonDebugger : MonoBehaviour {

    public Augmenta.AugmentaPerson MyAugmentaPerson;

    public Color BorderColor;
    public TextMesh PointInfoText;
    public Transform Point;
    [HideInInspector]
    public float Magic = 1.9f;
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

        //Update centroid

        //udpate text
        PointInfoText.text = "PID : " + MyAugmentaPerson.pid + '\n' + '\n' + '\n' + "OID : " + MyAugmentaPerson.oid;
        //if (Point.transform.localScale.x > Point.transform.localScale.y)
        //{
        //    //fit text to point height
        //    PointInfoText.transform.localScale = (new Vector3(Point.transform.localScale.y, Point.transform.localScale.y, Point.transform.localScale.y) / PointInfoText.fontSize) * Magic;
        //}
        //else
        //{
        //    //fit text to point width
        //    PointInfoText.transform.localScale = (new Vector3(Point.transform.localScale.x, Point.transform.localScale.x, Point.transform.localScale.x) / PointInfoText.fontSize) * Magic;
        //}

        //Update velocity
        float angle = Mathf.Atan2(MyAugmentaPerson.GetSmoothedVelocity().y *100, MyAugmentaPerson.GetSmoothedVelocity().x * 100 ) *180 / Mathf.PI;
        if (float.IsNaN(angle))
            return;
        VelocityVisualizer.localRotation = Quaternion.Euler(new Vector3(0, 0, -angle  +90));
        VelocityVisualizer.localScale = new Vector3(VelocityThickness, MyAugmentaPerson.GetSmoothedVelocity().magnitude * 100, VelocityThickness);
    }
}
