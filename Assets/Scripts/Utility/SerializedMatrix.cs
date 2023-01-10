using System;
using UnityEngine;

namespace Utility{
    [Serializable]
    public class SerializedMatrix
    {
        public Vector3 position = Vector3.zero;
        public  Quaternion rotation = Quaternion.identity;
        public Vector3 scale = Vector3.one;

        public Matrix4x4 GetMatrix() => Matrix4x4.TRS(position, rotation.normalized, scale);

    }
}
