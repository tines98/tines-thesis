using System;
using UnityEngine;


[Serializable]
public class ScaleModel{
    public GameObject prefab;
    public Vector3 scale = Vector3.one;
    public bool shouldRotate = true;
    public bool forceRotate = false;
    public Vector3 up;
    public Vector3 forward;
    public Texture Texture;
}
