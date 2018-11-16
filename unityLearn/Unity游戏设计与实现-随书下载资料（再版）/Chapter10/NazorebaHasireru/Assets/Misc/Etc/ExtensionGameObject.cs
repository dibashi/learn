using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 扩充方法
namespace GameObjectExtension {

	static class _List {
		
		// 头部节点
		public static T 		front<T>(this List<T> list)
		{
			return(list[0]);
		}

		// 最后一个节点
		public static T 		back<T>(this List<T> list)
		{
			return(list[list.Count - 1]);
		}
	}

	static class _MonoBehaviour {

		// 设置位置
		public static void 		setPosition(this MonoBehaviour mono, Vector3 position)
		{
			mono.gameObject.transform.position = position;
		}

		// 获取位置
		public static Vector3	getPosition(this MonoBehaviour mono)
		{
			return(mono.gameObject.transform.position);
		}

		// 设置本地坐标位置
		public static void setLocalPosition(this MonoBehaviour mono, Vector3 local_position)
		{
			mono.gameObject.transform.localPosition = local_position;
		}

		// 获取本地坐标位置
		public static void setLocalScale(this MonoBehaviour mono, Vector3 local_scale)
		{
			mono.gameObject.transform.localScale = local_scale;
		}

		// ================================================================ //

		// 设置父级对象
		public static void setParent(this MonoBehaviour mono, GameObject parent)
		{
			if(parent != null) {

				mono.gameObject.transform.parent = parent.transform;

			} else {

				mono.gameObject.transform.parent = null;
			}
		}

		// ================================================================ //
	};

	static class _GameObject {

		// 通过Prefab生成实例
		public static GameObject	instantiate(this GameObject prefab)
		{
			return(GameObject.Instantiate(prefab) as GameObject);
		}

		// 销毁自身
		public static void		destroy(this GameObject go)
		{
			GameObject.Destroy(go);
		}

		// ================================================================ //

		// 设置显示/隐藏
		public static void	setVisible(this GameObject go, bool is_visible)
		{
			Renderer[]		renders = go.GetComponentsInChildren<Renderer>();
			
			foreach(var render in renders) {
			
				render.enabled = is_visible;
			}
		}

		// ================================================================ //

		// 设置位置
		public static void 		setPosition(this GameObject go, Vector3 position)
		{
			go.transform.position = position;
		}

		// 获取位置
		public static Vector3	getPosition(this GameObject go)
		{
			return(go.transform.position);
		}

		// 设置角度
		public static void setRotation(this GameObject go, Quaternion rotation)
		{
			go.transform.rotation = rotation;
		}

		// 设置本地坐标
		public static void setLocalPosition(this GameObject go, Vector3 local_position)
		{
			go.transform.localPosition = local_position;
		}

		// 设置本地缩放率
		public static void setLocalScale(this GameObject go, Vector3 local_scale)
		{
			go.transform.localScale = local_scale;
		}

		// ================================================================ //

		// 设置父级对象
		public static void setParent(this GameObject go, GameObject parent)
		{
			if(parent != null) {

				go.transform.parent = parent.transform;

			} else {

				go.transform.parent = null;
			}
		}

		// 查找子游戏对象（非遍历）
		public static GameObject findChildGameObject(this GameObject go, string child_name)
		{
			GameObject	child_go = null;
			Transform	child    = go.transform.FindChild(child_name);

			if(child != null) {

				child_go = child.gameObject;
			}

			return(child_go);
		}

		// 遍历查找子对象
		public static GameObject	findDescendant(this GameObject go, string name)
		{
			GameObject	descendant = null;
	
			descendant = go.findChildGameObject(name);
	
			if(descendant == null) {
	
				foreach(Transform child in go.transform) {
	
					descendant = child.gameObject.findDescendant(name);
	
					if(descendant != null) {
	
						break;
					}
				}
			}
	
			return(descendant);
		}

		// ================================================================ //

		// 改变材质的属性（float）
		public static void	setMaterialProperty(this GameObject go, string name, float value)
		{
			SkinnedMeshRenderer[]		renders = go.GetComponentsInChildren<SkinnedMeshRenderer>();
			
			foreach(var render in renders) {
			
				foreach(var material in render.materials) {

					material.SetFloat(name, value);
				}
			}
		}

		// 改变材质的属性（Color）
		public static void	setMaterialProperty(this GameObject go, string name, Color color)
		{
			SkinnedMeshRenderer[]		renders = go.GetComponentsInChildren<SkinnedMeshRenderer>();
			
			foreach(var render in renders) {
			
				foreach(var material in render.materials) {

					material.SetColor(name, color);
				}
			}
		}

	}
};
