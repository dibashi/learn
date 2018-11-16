using UnityEngine;
using System.Collections;

public class SimpleSprite : MonoBehaviour {
	
	public Vector3	size = Vector3.one;
	
	// Use this for initialization
	void Start () {
	
		//

		Mesh mesh = new Mesh();

		Vector3[]	vertices  = new Vector3[4];
		Vector2[]	uvs       = new Vector2[4];
		int[]		triangles = new int[6];

		float		width  = this.size.x;
		float		height = this.size.y;

		vertices[0] = new Vector3( width/2.0f,  height/2.0f, 0.0f);
		vertices[1] = new Vector3(-width/2.0f,  height/2.0f, 0.0f);
		vertices[2] = new Vector3( width/2.0f, -height/2.0f, 0.0f);
		vertices[3] = new Vector3(-width/2.0f, -height/2.0f, 0.0f);

		uvs[0] = new Vector2(1.0f, 1.0f);
		uvs[1] = new Vector2(0.0f, 1.0f);
		uvs[2] = new Vector2(1.0f, 0.0f);
		uvs[3] = new Vector2(0.0f, 0.0f);

		triangles[0] = 0;
		triangles[1] = 2;
 		triangles[2] = 1;

		triangles[3] = 2;
		triangles[4] = 3;
 		triangles[5] = 1;

		mesh.vertices  = vertices;
		mesh.triangles = triangles;
		mesh.uv        = uvs;

		this.gameObject.GetComponent<MeshFilter>().mesh = mesh;
	}

	// Update is called once per frame
	void Update () {
	
	}

	public void	SetTexture(Texture texture)
	{
		this.gameObject.GetComponent<Renderer>().material.mainTexture = texture;
	}

	public void	SetSize(float width, float height)
	{
		this.size.x = width;
		this.size.y = height;
	}

}
