using UnityEngine;

public class DeathPlaneFactory
{
    public static DeathPlane CreateDeathPlane(Transform parent, Bounds meshBounds, Bounds barChartBounds, float particleDiameter){
        var deathPlaneGameObject = new GameObject("DeathPlane"){
            transform ={
                parent = parent,
                position = PlaceDeathPlane(barChartBounds.center,
                                           meshBounds,
                                           particleDiameter)
            }
        };
        var deathPlane = deathPlaneGameObject.AddComponent<DeathPlane>();
        deathPlane.size = ResizeDeathPlane(meshBounds.size, particleDiameter);
        return deathPlane;
    }

    /// <summary>
    /// Places the DeathPlane based on the barchart and the mesh
    /// </summary>
    /// <param name="barChartPos">Center of a Bar in the barchart</param>
    /// <param name="meshBounds">Bounds of the mesh</param>
    /// <param name="particleDiameter">diameter of a particle in the fluid simulation</param>
    private static Vector3 PlaceDeathPlane(Vector3 barChartPos, Bounds meshBounds, float particleDiameter) => 
        new Vector3(barChartPos.x,
                    meshBounds.min.y-particleDiameter,
                    barChartPos.z);

    /// <summary>
    /// Resizes the DeathPlane based on the given bounds
    /// </summary>
    /// <param name="meshSize">Bounds of the voxelized mesh</param>
    /// <param name="particleDiameter">diameter of a particle in the fluid simulation</param>
    private static Vector3 ResizeDeathPlane(Vector3 meshSize, float particleDiameter) => 
        new Vector3(meshSize.x + particleDiameter * 1.1f,
                    0,
                    meshSize.z + particleDiameter * 1.1f);

}
