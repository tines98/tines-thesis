using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedMatrix
{
    public Vector3 position = Vector3.zero;
    public  Quaternion rotation = Quaternion.identity;
    public Vector3 scale = Vector3.one;

    public Matrix4x4 GetMatrix() => Matrix4x4.TRS(position, rotation, scale);

}
