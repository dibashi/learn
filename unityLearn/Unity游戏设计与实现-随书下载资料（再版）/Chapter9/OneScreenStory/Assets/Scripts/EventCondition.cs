
/// <summary>事件的发生条件</summary>
public class EventCondition
{
	//==============================================================================================
	// 公开方法

	/// <summary>构造函数</summary>
	public EventCondition( BaseObject baseObject, string name, string compareValue )
	{
		m_object       = baseObject;
		m_name         = name;
		m_compareValue = compareValue;
	}

	/// <summary>对发生条件进行评价</summary>
	public bool evaluate()
	{
		string value = m_object.getVariable( m_name );
		if ( value == null ) {
			value = "0";
		}

		return m_compareValue == value;
	}


	//==============================================================================================
	// 非公开变量

	/// <summary>角色</summary>
	private BaseObject m_object;

	/// <summary>标记名称</summary>
	private string m_name;

	/// <summary>使事件发生的值</summary>
	private string m_compareValue;
}
