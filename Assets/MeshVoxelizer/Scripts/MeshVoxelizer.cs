using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace MeshVoxelizer.Scripts {
	public class MeshVoxelizer {
		private int Count { get; set; }

		private int Width { get; }

		private int Height { get; }

		private int Depth { get; }

        public int[,,] Voxels { get; }

        public List<Box3> Bounds { get; private set; }

        public MeshVoxelizer(int width, int height, int depth) {
	        Width = width;
            Height = height;
            Depth = depth;
            Bounds = new List<Box3>();
            Voxels = new int[width, height, depth];
        }
        public void Voxelize(IList<Vector3> vertices, IList<int> indices, Box3 bounds) {
            Array.Clear(Voxels, 0, Voxels.Length);

            // build an aabb tree of the mesh
            var tree = new MeshRayTracer(vertices, indices);
            Bounds = tree.GetBounds();

            // parity count method, single pass
            var extents = bounds.Size;
	        var delta = new Vector3(extents.x/Width, extents.y/Height, extents.z/Depth);
	        var offset = new Vector3(0.5f/Width, 0.5f/Height, 0.5f/Depth);

            var eps = 1e-7f * extents.z;
			Profiler.BeginSample("Voxelization Tree Tracing");
            for (var x = 0; x < Width; ++x)
	        {
		        for (var y = 0; y < Height; ++y)
		        {
			        var inside = false;
			        var rayDir = new Vector3(0.0f, 0.0f, 1.0f);

                     // z-coord starts somewhat outside bounds 
			        var rayStart = bounds.Min + new Vector3(x*delta.x + offset.x, y*delta.y + offset.y, -0.0f*extents.z);

			        while(true)
			        {
                        var ray = tree.TraceRay(rayStart, rayDir);

                        if (ray.hit)
				        {
                            // calculate cell in which intersection occurred
                            var zPos = rayStart.z + ray.distance * rayDir.z;
                            var zHit = (zPos - bounds.Min.z) / delta.z;

                            var z = (int)((rayStart.z - bounds.Min.z) / delta.z);
                            var zEnd = (int)Math.Min(zHit, Depth - 1);

                            if (inside) {
                                for (int k = z; k <= zEnd; ++k) {
                                    Voxels[x, y, k] = 1;
                                    Count++;
                                }
                            }

                            inside = !inside;
					        rayStart += rayDir*(ray.distance + eps);
                        }
				        else
					        break;
			        }
		        }
	        }
			Profiler.EndSample();
            //end
        }

    }

}
