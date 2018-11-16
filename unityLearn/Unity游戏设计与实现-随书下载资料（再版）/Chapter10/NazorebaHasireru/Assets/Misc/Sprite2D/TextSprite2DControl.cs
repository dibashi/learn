using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathExtension;

// 2D 文本图片
public class TextSprite2DControl : Sprite2DControl {

	public	TextGenerationSettings	generator_settings;
	public	TextGenerator 			generator;

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Awake()
	{
	}

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	public void		create()
	{
		this.generator_settings = new TextGenerationSettings();

		this.generator_settings.color                = Color.white;

		this.generator_settings.font                 = null;
		this.generator_settings.fontSize             = 0;
		this.generator_settings.fontStyle            = FontStyle.Normal;
		this.generator_settings.richText             = true;

		this.generator_settings.horizontalOverflow   = HorizontalWrapMode.Wrap;
		this.generator_settings.verticalOverflow     = VerticalWrapMode.Overflow;
		this.generator_settings.textAnchor           = TextAnchor.MiddleCenter;

		this.generator_settings.generateOutOfBounds  = false;
		// 文本全体的尺寸
		//this.generator_settings.generationExtents    = Vector2.one*100.0f;
		this.generator_settings.generationExtents    = Vector2.zero;

		this.generator_settings.lineSpacing          = 1.0f;
		this.generator_settings.pivot                = new Vector2(0.0f, 0.0f);

		this.generator_settings.resizeTextForBestFit = false;
		this.generator_settings.resizeTextMaxSize    = 32; 
		this.generator_settings.resizeTextMinSize    = 16; 
		this.generator_settings.updateBounds         = true;


		this.generator = new TextGenerator();

		//

		this.gameObject.AddComponent<MeshRenderer>();
		this.gameObject.AddComponent<MeshFilter>();
	}

	protected bool	clip_by_rect_moji(Vector3[] dest_pos, Vector2[] dest_uvs, int index, Rect rect)
	{
		bool	ret = false;

		do {

			Vector3		lt = this.positions[index*4 + 0];
			Vector3		rt = this.positions[index*4 + 1];
			Vector3		lb = this.positions[index*4 + 3];
			Vector3		rb = this.positions[index*4 + 2];
		
			Vector2		uv_lt = this.uvs[index*4 + 0];
			Vector2		uv_rt = this.uvs[index*4 + 1];
			Vector2		uv_lb = this.uvs[index*4 + 3];
			Vector2		uv_rb = this.uvs[index*4 + 2];

			// ---------------------------------------------------------------- //
			// 完全位于 rect 外则结束（不显示）

			if(rt.x <= rect.xMin || rect.xMax <= lt.x) {

				break;
			}
			if(lt.y <= rect.yMin || rect.yMax <= lb.y) {

				break;
			}

			// ---------------------------------------------------------------- //
			// 左右裁剪

			if(rect.xMin <= lt.x && rt.x <= rect.xMax) {

			} else if(lt.x < rect.xMin) {

				uv_lt = Vector2.Lerp(uv_lt, uv_rt, Mathf.InverseLerp(lt.x, rt.x, rect.xMin));
				uv_lb = Vector2.Lerp(uv_lb, uv_rb, Mathf.InverseLerp(lb.x, rb.x, rect.xMin));

				lt.x = rect.xMin;
				lb.x = rect.xMin;

			} else if(rect.xMax < rt.x) {

				uv_rt = Vector2.Lerp(uv_lt, uv_rt, Mathf.InverseLerp(lt.x, rt.x, rect.xMax));
				uv_rb = Vector2.Lerp(uv_lb, uv_rb, Mathf.InverseLerp(lb.x, rb.x, rect.xMax));

				rt.x = rect.xMax;
				rb.x = rect.xMax;
			}
	
			// ---------------------------------------------------------------- //
			// 上下裁剪

			if(rect.yMin <= lb.y && lt.y <= rect.yMax) {

			} else if(lb.y < rect.yMin) {

				uv_lb = Vector2.Lerp(uv_lb, uv_lt, Mathf.InverseLerp(lb.y, lt.y, rect.yMin));
				uv_rb = Vector2.Lerp(uv_rb, uv_rt, Mathf.InverseLerp(rb.y, rt.y, rect.yMin));

				lb.y = rect.yMin;
				rb.y = rect.yMin;

			} else if(rect.yMax < lt.y) {

				uv_lt = Vector2.Lerp(uv_lb, uv_lt, Mathf.InverseLerp(lb.y, lt.y, rect.yMax));
				uv_rt = Vector2.Lerp(uv_rb, uv_rt, Mathf.InverseLerp(rb.y, rt.y, rect.yMax));

				lt.y = rect.yMax;
				rt.y = rect.yMax;
			}
	
			// ---------------------------------------------------------------- //

			dest_uvs[0] = uv_lt;
			dest_uvs[1] = uv_rt;
			dest_uvs[3] = uv_lb;
			dest_uvs[2] = uv_rb;
		
			dest_pos[0] = lt;
			dest_pos[1] = rt;
			dest_pos[3] = lb;
			dest_pos[2] = rb;

			ret = true;

		} while(false);

		return(ret);
	}

	// 剔除溢出Rect的部分
	public void		clipByRect(Rect	rect)
	{
		MeshRenderer	mesh_render = this.gameObject.GetComponent<MeshRenderer>();
		MeshFilter		mesh_filter = this.gameObject.GetComponent<MeshFilter>();
		Mesh			mesh        = mesh_filter.mesh;
	
		do {

			mesh_render.enabled = true;

			// ---------------------------------------------------------------- //

			List<Vector3>	new_pos = new List<Vector3>();
			List<Vector2>	new_uvs = new List<Vector2>();
			List<Color>		new_col = new List<Color>();
	
			Vector3[]	dest_pos = new Vector3[4];
			Vector2[]	dest_uvs = new Vector2[4];
	
			for(int i = 0;i < this.generator.characters.Count;i++) {
		
				if(this.clip_by_rect_moji(dest_pos, dest_uvs, i, rect)) {
	
					new_pos.AddRange(dest_pos);
					new_uvs.AddRange(dest_uvs);
	
					for(int j = 0;j < 4;j++) {
	
						new_col.Add(this.generator.verts[i*4 + j].color);
					}
				}
			}

			if(new_pos.Count == 0) {

				mesh_render.enabled = false;
				break;
			}

			// ---------------------------------------------------------------- //
	
			dest_pos = new Vector3[new_pos.Count];
			dest_uvs = new Vector2[new_uvs.Count];
	
			new_pos.CopyTo(dest_pos);
			new_uvs.CopyTo(dest_uvs);
	
			Color[]		dest_col = new Color[new_col.Count];
	
			new_col.CopyTo(dest_col);
	
			// ---------------------------------------------------------------- //
			// 索引

			int			moji_count = new_pos.Count/4;
			int[]		indices = new int[moji_count*3*2];
	
			for(int i = 0;i < moji_count;i++) {
	
				indices[i*6 + 0] = i*4 + 0;
				indices[i*6 + 1] = i*4 + 1;
				indices[i*6 + 2] = i*4 + 2;
				indices[i*6 + 3] = i*4 + 2;
				indices[i*6 + 4] = i*4 + 3;
				indices[i*6 + 5] = i*4 + 0;
			}
	
			// ---------------------------------------------------------------- //

			mesh.Clear();
			mesh.vertices        = dest_pos;
			mesh.uv              = dest_uvs;
			mesh.colors          = dest_col;
			mesh.triangles       = indices;
			mesh_filter.mesh     = mesh;

		} while(false);
	}

	protected	Vector3[]	positions;
	protected	Vector2[]	uvs;

	// 创建字符串图片
	public void		createText(string text)
	{
		this.generator.Populate(text, this.generator_settings);

		Color[]		colors;
		int[]		indices;

		this.positions = new Vector3[this.generator.verts.Count];
		this.uvs       = new Vector2[this.generator.verts.Count];

		colors    = new Color[this.generator.verts.Count];
		indices   = new int[this.generator.characters.Count*3*2];

		for(int i = 0;i < this.generator.verts.Count;i++) {

			this.positions[i] = this.generator.verts[i].position;
			this.uvs[i]       = this.generator.verts[i].uv0;
			colors[i]         = this.generator.verts[i].color;

			if(i%4 == 0) {

				//colors[i] = Color.blue;
			}
		}

		// 设置字符间的固定间距
		for(int i = 0;i < this.generator.characters.Count;i++) {

			Vector3		lt = this.positions[i*4 + 0];
			Vector3		rt = this.positions[i*4 + 1];
			Vector3		lb = this.positions[i*4 + 3];
			Vector3		rb = this.positions[i*4 + 2];
			Vector3		center = new Vector3((lt.x + rt.x)/2.0f, (lt.y + lb.y)/2.0f, 0.0f);

			lt -= center;
			rt -= center;
			lb -= center;
			rb -= center;

			center = Vector3.zero;
			center.x -= (this.generator.characters.Count - 1.0f)*this.generator_settings.fontSize/2.0f;
						// ↑为什么比字符数多1？
			center.x += this.generator_settings.fontSize/2.0f;
			center.x += i*this.generator_settings.fontSize;

			lt += center;
			rt += center;
			lb += center;
			rb += center;

			this.positions[i*4 + 0] = lt;
			this.positions[i*4 + 1] = rt;
			this.positions[i*4 + 3] = lb;
			this.positions[i*4 + 2] = rb;
		}

		for(int i = 0;i < this.generator.characters.Count;i++) {

			indices[i*6 + 0] = i*4 + 0;
			indices[i*6 + 1] = i*4 + 1;
			indices[i*6 + 2] = i*4 + 2;
			indices[i*6 + 3] = i*4 + 2;
			indices[i*6 + 4] = i*4 + 3;
			indices[i*6 + 5] = i*4 + 0;
		}

		MeshRenderer	mesh_render = this.gameObject.GetComponent<MeshRenderer>();
		MeshFilter		mesh_filter = this.gameObject.GetComponent<MeshFilter>();
		Mesh			mesh        = mesh_filter.mesh;

		mesh.Clear();
		mesh.vertices        = this.positions;
		mesh.uv              = this.uvs;
		mesh.colors          = colors;
		mesh.triangles       = indices;
		mesh_filter.mesh     = mesh;
		mesh_render.material = this.generator_settings.font.material;
	}
}
