
/// <summary>能进行事件处理的Actor基类</summary>
public abstract class EventActor
{
	//==============================================================================================
	// 公有方法

	/// <summary>生成Actor刚开始时执行的方法</summary>
	/// 相当于UnityEngine.MonoBehaviour.Start()
	public virtual void start( EventManager evman )
	{
	}

	/// <summary>在Actor被销毁前每帧执行的方法</summary>
	/// 相当于UnityEngine.MonoBehaviour.Update() 
	public virtual void execute( EventManager evman )
	{
	}

	// 相当于使用uGUI时的OnGUI()
	/// <summary>在Actor被销毁前绘制GUI时执行的方法</summary>
	/// 替代UnityEngine.MonoBehaviour.OnGUI()
	//public virtual void onGUI( EventManager evman )
	//{
	//}

	/// <summary>判断Actor必须执行的处理是否结束</summary>
	public virtual bool isDone()
	{
		// 默认很快结束
		return true;
	}

	/// <summary>判断执行结束后是否需要等待点击</summary>
	public virtual bool isWaitClick( EventManager evman )
	{
		// 默认需要等待
		return true;
	}
}
