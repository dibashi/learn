using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 生成锁定激光碰撞检测用的MeshCollider
// ----------------------------------------------------------------------------
public class ScoutingLaserMeshController : MonoBehaviour {
	
	private Mesh			mesh;
	private MeshFilter		meshFilter;
	private MeshCollider	meshCollider;
	private ScoutingLaser	scoutingLaser;
	
	private const float 	PIECE_ANGLE = 5.0f;		// 1个多边形的角度（圆的光滑度）
	private const float 	FAN_RADIUS = 10.0f;		// 圆的半径
	
	void Start () {

		mesh = new Mesh();
		meshFilter = GetComponent<MeshFilter>();
		meshCollider = GetComponent<MeshCollider>();
		meshCollider.sharedMesh = mesh;
		// .
		scoutingLaser = GameObject.FindGameObjectWithTag("ScoutingLaser").GetComponent<ScoutingLaser>();
		
	}
	
	void Update () {
	}
	
	public void clearShape()
	{
		mesh.Clear();
		meshFilter.mesh = mesh;
		// mesh 发生变化后如果不执行 false -> true 则不会得到反映
		meshCollider.enabled = false;
		meshCollider.enabled = true;
	}

	public void makeFanShape( float[] angle )
	{
		float startAngle;					// 圆的开始角度
		float endAngle;						// 圆的结束角度
		float pieceAngle = PIECE_ANGLE;		// 1个多边形的角度（圆的光滑度）
		float radius     = FAN_RADIUS;		// 圆的半径
		
		startAngle = angle[0];
		endAngle   = angle[1];

		// --------------------------------------------------------------------
		// 准备
		// --------------------------------------------------------------------

		if ( Mathf.Abs ( startAngle - endAngle ) > 180f )
		{
			// 如果跨越了0度 <-> 359度 ，则+360度
			if ( startAngle < 180f )
			{
				startAngle += 360f;
			}
			if ( endAngle < 180f )
			{
				endAngle += 360f;
			}
		}
		
		Vector3[]	circleVertices;			// 构成圆的各个多边形顶点坐标
		int[]		circleTriangles;		// 多边形面的信息（顶点连接信息）

		// 对角度而言，开始 < 结束
		if ( startAngle > endAngle )
		{
			float tmp = startAngle;
			startAngle = endAngle;
			endAngle = tmp;
		}

		// 三角形的数量
		int	triangleNum = (int)Mathf.Ceil(( endAngle - startAngle ) / pieceAngle );

		// 分配数组空间
		circleVertices = new Vector3[triangleNum + 1 + 1];
		circleTriangles = new int[triangleNum*3];

		// --------------------------------------------------------------------
		// 生成多边形
		// --------------------------------------------------------------------

		// 计算顶点坐标

		circleVertices[0] = Vector3.zero;


		for( int i = 0; i < triangleNum + 1; i++ )
		{

			float currentAngle = startAngle + (float)i*pieceAngle;

			// 确保不超过终点值
			currentAngle = Mathf.Min( currentAngle, endAngle );

			circleVertices[1 + i] = Quaternion.AngleAxis( currentAngle, Vector3.up ) * Vector3.forward * radius;
		}

		// 索引

		for( int i = 0; i < triangleNum; i++ )
		{
			circleTriangles[i*3 + 0] = 0;
			circleTriangles[i*3 + 1] = i + 1;
			circleTriangles[i*3 + 2] = i + 2;
		}

		// --------------------------------------------------------------------
		// 生成网格
		// --------------------------------------------------------------------

		mesh.Clear();
		
		mesh.vertices = circleVertices;
		mesh.triangles = circleTriangles;

		mesh.Optimize();
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		
		meshFilter.mesh = mesh;

		// mesh 发生变化后如果不执行 false -> true 则不会得到反映
		meshCollider.enabled = false;
		meshCollider.enabled = true;
	}

	void OnTriggerEnter( Collider collider )
	{
		scoutingLaser.Lockon( collider );
	}

	void OnTriggerExit()
	{
	}
	
	void OnTriggerStay( Collider collider )
	{
	}

}
