using UnityEngine;
using System.Collections;
using MathExtension;

// 2D 精灵图片
public class Sprite2DControl : MonoBehaviour {
	
	protected Vector2		size = Vector2.zero;
	protected float			angle = 0.0f;

	public string	depth_layer = "";
	public float	depth_offset = 0.0f;			// 同一层内的深度

	protected Vector2	position = Vector2.zero;
	public Vector2		rotation_center = Vector2.zero;

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	// 设置位置
	public void	setPosition(Vector2 position)
	{
		this.position = position;
		this.transform.localPosition = new Vector3(this.position.x, this.position.y, this.transform.localPosition.z);
	}

	// 获得位置
	public Vector2	getPosition()
	{
		//return(this.transform.localPosition.xy());
		return(this.position);
	}

	// 设置层级深度
	public void		setDepthLayer(string depth_layer)
	{
		this.depth_layer = depth_layer;

		if(!Sprite2DRoot.get().isHasLayer(this.depth_layer)) {

			Debug.LogError("Depth Layer \"" + depth_layer + "\" not exists.");
		}

		this.calc_depth();
	}

	// 设置深度的偏移
	public void		setDepthOffset(float offset)
	{
		offset = Mathf.Clamp(offset, 0.0f, 1.0f - float.Epsilon);

		this.depth_offset = offset;

		this.calc_depth();
	}

	// 获取深度偏移
	public float	getDepthOffset()
	{
		return(this.depth_offset);
	}

	// 设置深度值
	protected void		calc_depth()
	{
		float	depth = Sprite2DRoot.get().depthLayerToFloat(this.depth_layer, depth_offset);

		Vector3		position = this.transform.localPosition;

		position.z = depth;

		this.transform.localPosition = position;
	}

	// 获得深度值
	public float	getDepth()
	{
		return(this.transform.localPosition.z);
	}

	// [degree] 设置角度（围绕Z轴旋转）
	public void	setAngle(float angle)
	{
		this.angle = angle;
#if true
		Vector2		center = this.rotation_center;
		center.x *= this.transform.lossyScale.x;
		center.y *= this.transform.lossyScale.y;

		this.transform.localPosition = this.position.to_vector3(this.transform.localPosition.z);
		this.transform.localRotation = Quaternion.identity;
		this.transform.Translate( center, Space.Self);
		this.transform.Rotate(Vector3.forward, this.angle);
		this.transform.Translate(-center, Space.Self);
#else
		this.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
#endif
	}

	// [degree] 获取角度（围绕Z轴旋转）
	public float	getAngle()
	{
		return(this.angle);
	}

	// 设置旋转的中心

	public void	setRotationCenter(Vector2 center)
	{
		this.rotation_center = center;
	}

	// 设置缩放
	public void	setScale(Vector2 scale)
	{
		this.transform.localScale = new Vector3(scale.x, scale.y, 1.0f);
	}
	
	// 设置尺寸
	public void		setSize(Vector2 size)
	{
		Sprite2DRoot.get().setSizeToSprite(this, size);
	}
	// 获取尺寸
	public Vector2 getSize()
	{
		return(this.size);
	}
	
	// 设置顶点颜色
	public void		setVertexColor(Color color)
	{
		Sprite2DRoot.get().setVertexColorToSprite(this, color);
	}

	// 设置顶点颜色的透明值
	public void		setVertexAlpha(float alpha)
	{
		Sprite2DRoot.get().setVertexColorToSprite(this, new Color(1.0f, 1.0f, 1.0f, alpha));
	}

	// 设置纹理
	public void		setTexture(Texture texture)
	{
		this.GetComponent<MeshRenderer>().material.mainTexture = texture;
	}
	// 设置纹理（同时修改尺寸）
	public void		setTextureWithSize(Texture texture)
	{
		this.GetComponent<MeshRenderer>().material.mainTexture = texture;
		Sprite2DRoot.get().setSizeToSprite(this, new Vector2(texture.width, texture.height));
	}

	// 获取纹理
	public Texture	getTexture()
	{
		return(this.GetComponent<MeshRenderer>().material.mainTexture);
	}

	// 设置材质
	public void		setMaterial(Material material)
	{
		this.GetComponent<MeshRenderer>().material = material;
	}
	// 获取材质
	public Material		getMaterial()
	{
		return(this.GetComponent<MeshRenderer>().material);
	}

	// 左右/上下翻转
	public void		setFlip(bool horizontal, bool vertical)
	{
		Vector2		scale  = Vector2.one;
		Vector2		offset = Vector2.zero;

		if(horizontal) {

			scale.x  = -1.0f;
			offset.x = 1.0f;
		}
		if(vertical) {

			scale.y = -1.0f;
			offset.y = 1.0f;
		}

		this.GetComponent<MeshRenderer>().material.mainTextureScale  = scale;
		this.GetComponent<MeshRenderer>().material.mainTextureOffset = offset;
	}

	// 判断该点是否位于精灵图片上？
	public bool		isContainPoint(Vector2 point)
	{
		bool	ret = false;

		Vector2		position = this.transform.localPosition.xy();
		Vector2		scale    = this.transform.localScale.xy();

		do {

			if(point.x < position.x - this.size.x/2.0f*scale.x || position.x + this.size.x/2.0f*scale.x < point.x) {

				break;
			}
			if(point.y < position.y - this.size.y/2.0f*scale.y || position.y + this.size.y/2.0f*scale.y < point.y) {

				break;
			}

			ret = true;

		} while(false);

		return(ret);
	}

	// 设置显示/隐藏
	public void		setVisible(bool is_visible)
	{
		this.GetComponent<MeshRenderer>().enabled = is_visible;
	}
	// 是否显示
	public bool		isVisible()
	{
		return(this.GetComponent<MeshRenderer>().enabled);
	}

	// 获取顶点的位置
	public Vector3[]	getVertexPositions()
	{
		return(Sprite2DRoot.get().getVertexPositionsFromSprite(this));
	}

	// 设置顶点的位置（2D）
	public void		setVertexPositions(Vector2[] positions)
	{
		Vector3[]		positions_3d = new Vector3[positions.Length];

		for(int i = 0;i < positions.Length;i++) {

			positions_3d[i] = positions[i];
		}
		Sprite2DRoot.get().setVertexPositionsToSprite(this, positions_3d);
	}

	// 设置顶点的位置（3D）
	public void		setVertexPositions(Vector3[] positions)
	{
		Sprite2DRoot.get().setVertexPositionsToSprite(this, positions);
	}

	// 设置UV
	public void		setVertexUVs(Vector2[] uvs)
	{
		Sprite2DRoot.get().setVertexUVsToSprite(this, uvs);
	}

	// 获取网格的顶点数
	public int		getDivCount()
	{
		return(this.div_count);
	}

	// 销毁
	public void		destroy()
	{
		GameObject.Destroy(this.gameObject);
	}

	// 设置父对象
	public void		setParent(Sprite2DControl parent)
	{
		this.transform.parent = parent.transform;
	}

	// 添加网格碰撞体
	public void		createCollider()
	{
		this.gameObject.AddComponent<MeshCollider>();
	}

	// 是否位于画面内？
	public bool		isInScreen()
	{
		bool		ret = false;
		Vector2		viewport_size = Sprite2DRoot.get().viewport_size;
		Vector2		position = this.transform.localPosition.xy();

		do {

			float	w = viewport_size.x/2.0f + this.size.x/2.0f;
			float	h = viewport_size.y/2.0f + this.size.y/2.0f;

			if(position.x < -w || w < position.x) {

				break;
			}
			if(position.y < -h || h < position.y) {

				break;
			}

			ret = true;

		} while(false);

		return(ret);
	}

	// ================================================================ //
	// Sprite2DRoot

	// 设置尺寸
	public void	internalSetSize(Vector2 size)
	{
		this.size = size;
	}

	protected int	div_count = 2;

	public void	internalSetDivCount(int div_count)
	{
		this.div_count = div_count;
	}

}
