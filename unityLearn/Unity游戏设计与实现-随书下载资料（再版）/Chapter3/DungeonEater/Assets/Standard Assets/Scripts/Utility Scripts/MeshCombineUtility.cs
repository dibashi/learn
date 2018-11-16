using UnityEngine;
using System.Collections;

public class MeshCombineUtility {
	
	public struct MeshInstance
	{
		public Mesh      mesh;
		public int       subMeshIndex;            
		public Matrix4x4 transform;
	}
	
	public static Mesh Combine (MeshInstance[] combines, bool generateStrips)
	{
		int 	vertex_count = 0;
		int 	triangle_count = 0;

		foreach(var combine in combines) {

			if(combine.mesh == null) {

				continue;
			}
			vertex_count   += combine.mesh.vertexCount;
			triangle_count += combine.mesh.GetTriangles(combine.subMeshIndex).Length;
		}

		// 頂点座標等をまとめる.

		Vector3[]	vertices = new Vector3[vertex_count];
		Vector3[]	normals  = new Vector3[vertex_count];
		Vector4[]	tangents = new Vector4[vertex_count];
		Vector2[]	uv       = new Vector2[vertex_count];
		Vector2[]	uv1      = new Vector2[vertex_count];
		Color[]		colors   = new Color[vertex_count];
		
		int[] 	triangles = new int[triangle_count];


		int 	offset = 0;
		
		foreach(var combine in combines) {
			
			if(combine.mesh == null) {
				
				continue;
			}

			Copy(combine.mesh.vertexCount,         combine.mesh.vertices, vertices, offset, combine.transform);
			CopyNormal(combine.mesh.vertexCount,   combine.mesh.normals,  normals,  offset, combine.transform);
			CopyTangents(combine.mesh.vertexCount, combine.mesh.tangents, tangents, offset, combine.transform);
			CopyColors(combine.mesh.vertexCount,   combine.mesh.colors,   colors, offset);
			Copy(combine.mesh.vertexCount, combine.mesh.uv,  uv,  offset);
			Copy(combine.mesh.vertexCount, combine.mesh.uv2, uv1, offset);
			offset += combine.mesh.vertexCount;
		}

		// 頂点の結線情報（頂点のインデックスのならび）をまとめる.

		int		vertex_offset = 0;
		offset = 0;
		
		foreach(var combine in combines) {
			
			if(combine.mesh == null) {
				
				continue;
			}

			for(int i = 0;i < combine.mesh.GetTriangles(combine.subMeshIndex).Length;i++) {

				triangles[offset + i] = combine.mesh.GetTriangles(combine.subMeshIndex)[i] + vertex_offset;
			}
			offset += combine.mesh.GetTriangles(combine.subMeshIndex).Length;
			vertex_offset += combine.mesh.vertexCount;
		}

		// ---------------------------------------------------------------- //

		Mesh mesh = new Mesh();

		mesh.name      = "Combined Mesh";
		mesh.vertices  = vertices;
		mesh.normals   = normals;
		mesh.colors    = colors;
		mesh.uv        = uv;
		mesh.uv2       = uv1;
		mesh.tangents  = tangents;
		mesh.triangles = triangles;
		
		return(mesh);
	}
	
	static void		Copy(int vertexcount, Vector3[] src, Vector3[] dst, int offset, Matrix4x4 transform)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = transform.MultiplyPoint(src[i]);
	}

	static void		CopyNormal(int vertexcount, Vector3[] src, Vector3[] dst, int offset, Matrix4x4 transform)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = transform.MultiplyVector(src[i]).normalized;
	}

	static void Copy (int vertexcount, Vector2[] src, Vector2[] dst, int offset)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = src[i];
	}

	static void CopyColors (int vertexcount, Color[] src, Color[] dst, int offset)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = src[i];
	}
	
	static void CopyTangents (int vertexcount, Vector4[] src, Vector4[] dst, int offset, Matrix4x4 transform)
	{
		for (int i=0;i<src.Length;i++)
		{
			Vector4 p4 = src[i];
			Vector3 p = new Vector3(p4.x, p4.y, p4.z);
			p = transform.MultiplyVector(p).normalized;
			dst[i+offset] = new Vector4(p.x, p.y, p.z, p4.w);
		}		
	}
}
