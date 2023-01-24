using System;
using UnityEngine;


[Serializable]
public class ScaleModel{
    public GameObject prefab;
    public Vector3 scale = Vector3.one;
    public bool shouldRotate = true;
}
