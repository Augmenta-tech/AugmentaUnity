using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaAreaAnchor : MonoBehaviour {

    [Header("Augmenta Area Visualization")]
    public float Width;
    public float Height;
    public float MeterPerPixel;
    public bool DrawGizmos;

    [Header("Augmenta Camera")]
    public AugmentaCamera augmentaCamera;

    // Update is called once per frame
    void Update () {
        AugmentaArea.Instance.gameObject.transform.position = transform.position;
        AugmentaArea.Instance.gameObject.transform.rotation = transform.rotation;
        UpdateScale();
    }

    void UpdateScale()
    {
        transform.localScale = new Vector3(Width * MeterPerPixel * augmentaCamera.Zoom, Height * MeterPerPixel * augmentaCamera.Zoom, 1.0f);
    }   

    private void OnDrawGizmos()
    {
        if (!DrawGizmos) return;

        Gizmos.color = Color.blue;

        //Draw area 
        UpdateScale();
        DrawGizmoCube(transform.position, transform.rotation, transform.localScale);
    }

    public void DrawGizmoCube(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

        Gizmos.matrix *= cubeTransform;

        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = oldGizmosMatrix;
    }
}
