using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaAreaAnchor : MonoBehaviour {

    /// <summary>
    /// The AugmentaAreaAnchor will update the AugmentaArea position, rotation and scale according to the AugmentaAreaAnchor parameters at every frame.
    /// 
    /// The position and rotation are copied from this object's transform position and rotation in world space.
    /// The scale is updated according to the Width, Height, MeterPerPixel and Zoom values.
    /// 
    /// The AugmentaAreaAnchor is expected to have an AugmentaCamera as children and will use this AugmentaCamera Zoom value to compute its scale.
    /// </summary>
    [Header("Augmenta Area Visualization")]
    public float Width;
    public float Height;
    public float MeterPerPixel;
    public bool DrawGizmos;

    [Header("Augmenta Camera")]
    public AugmentaCamera augmentaCamera;

    // Update is called once per frame
    void Update () {
        if (AugmentaArea.Instance)
        {
            AugmentaArea.Instance.gameObject.transform.position = transform.position;
            AugmentaArea.Instance.gameObject.transform.rotation = transform.rotation;
        }
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
