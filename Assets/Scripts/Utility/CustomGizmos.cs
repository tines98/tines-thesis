using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGizmos
{
    public static void DrawRotatableCubeGizmo(SerializedMatrix matrix, Vector3 size) => 
        DrawRotatableCubeGizmo(matrix.GetMatrix(),matrix.position,size);

    public static void DrawRotatableCubeGizmo(Matrix4x4 matrix, Vector3 position, Vector3 size) {
        var rotatedForward = matrix.MultiplyVector(Vector3.forward * size.z);
        var rotatedUp = matrix.MultiplyVector(Vector3.up * size.y);
        var rotatedRight = matrix.MultiplyVector(Vector3.right * size.x);

        var centering = rotatedForward + rotatedUp + rotatedRight;
        var minMinMinPos = position - centering * 0.5f;
        var minMinMaxPos = minMinMinPos + rotatedRight;
        var minMaxMinPos = minMinMinPos + rotatedUp;
        var minMaxMaxPos = minMinMaxPos + rotatedUp;
        var maxMinMinPos = minMinMinPos + rotatedForward;
        var maxMinMaxPos = maxMinMinPos + rotatedRight;
        var maxMaxMinPos = maxMinMinPos + rotatedUp;
        var maxMaxMaxPos = maxMinMaxPos + rotatedUp;
        //DRAW LOWER SQUARE
        DrawRotatableSquareGizmo(minMaxMinPos,minMaxMaxPos, maxMaxMinPos, maxMaxMaxPos);
        //DRAW TOP SQUARE
        DrawRotatableSquareGizmo(minMinMinPos,minMinMaxPos, maxMinMinPos, maxMinMaxPos);
        //DRAW NEAR SQUARE
        DrawRotatableSquareGizmo(minMinMinPos,minMinMaxPos, minMaxMinPos, minMaxMaxPos);
        //DRAW FAR SQUARE
        DrawRotatableSquareGizmo(maxMinMinPos,maxMinMaxPos, maxMaxMinPos, maxMaxMaxPos);
        //DRAW LEFT SQUARE
        DrawRotatableSquareGizmo(minMinMinPos,minMaxMinPos, maxMinMinPos, maxMaxMinPos);
        //DRAW RIGHT SQARE
        DrawRotatableSquareGizmo(minMinMaxPos,minMaxMaxPos, maxMinMaxPos, maxMaxMaxPos);
    }

    public static void DrawRotatableSquareGizmo(Vector3 minMin, Vector3 minMax, Vector3 maxMin, Vector3 maxMax) {
        Gizmos.DrawLine(minMin,minMax);
        Gizmos.DrawLine(minMin,maxMin);
        Gizmos.DrawLine(maxMin, maxMax);
        Gizmos.DrawLine(minMax,maxMax);
    }
}
