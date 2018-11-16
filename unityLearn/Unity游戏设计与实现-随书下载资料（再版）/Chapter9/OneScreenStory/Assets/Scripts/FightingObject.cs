
using UnityEngine;


/// <summary>战斗对象专用类</summary>
public class FightingObject : DraggableObject
{
	//==============================================================================================
	// 内部数据类型

	/// <summary>战斗指令的执行状态</summary>
	private enum BattleStatus
	{
		BeforeBattle,
		ZoomingIn,
		Battle,
		AfterBattle,
		ZoomingOut,
		EndBattle
	}


	//==============================================================================================
	// MonoBehaviour 相关的成员变量／方法

	/// <summary>摄像机管理对象</summary>
	protected CameraManager m_cameraManager = null;

	/// <summary>启动方法</summary>
	private void Start()
	{
		m_cameraManager = CameraManager.get ();
		m_objectManager = m_eventManager.GetComponent< ObjectManager >();
		m_soundManager  = SoundManager.get();
	}


	//==============================================================================================
	// 公开方法

	/// <summary>收到Actor发来的消息后每帧的处理</summary>
	public override bool updateMessage( string message, string[] parameters )
	{
		switch ( message )
		{
		// 战斗指令
		case "battle":
			{
				switch ( m_battleStatus )
				{
				case BattleStatus.BeforeBattle:
					// 隐藏文本
					TextManager textman = m_objectManager.GetComponent< TextManager >();
					if ( textman != null ) textman.hide();

					// 求出从战斗发生场地向摄像机延伸的光线（Ray）
					BaseObject enemy = m_objectManager.find( parameters[ 0 ] );
					Vector3 myPosition    = transform.position + 0.5f * ( getYTop() + getYBottom() ) * Vector3.up;
					Vector3 enemyPosition = enemy.transform.position + 0.5f * ( enemy.getYTop() + enemy.getYBottom() ) * Vector3.up;
					Vector3 battlePosition = Vector3.Lerp( myPosition, enemyPosition, 0.5f );
					Quaternion qt = Quaternion.AngleAxis( 20.0f, Vector3.right );

					m_rayFromBattlePositionToCamera = new Ray( battlePosition, qt * -Vector3.forward );

					// 镜头转向战斗场地
					m_cameraManager.moveTo(
						m_rayFromBattlePositionToCamera.GetPoint( /*5100.0f*/600.0f / Mathf.Sin( 32.92f * Mathf.Deg2Rad ) ),
						22.0f,
						m_cameraManager.getOriginalSize() * 0.5f,
						0.5f );

					m_battleStatus = BattleStatus.ZoomingIn;
					return true;  // 继续执行

				case BattleStatus.ZoomingIn:
					// 摄像机移动过程中
					if ( !m_cameraManager.isMoving() )
					{
						m_battleStatus = BattleStatus.Battle;
						m_currentFrame = 0;
					}
					return true;  // 继续执行

				case BattleStatus.Battle:
					// 播放动画／声音
					if ( m_currentFrame == 0 )
					{
						// 为了让战斗特效遮盖住角色稍微使其显示在前面
						EffectManager.get().playFightingEffect(m_rayFromBattlePositionToCamera.GetPoint( 100.0f ));

						// 播放战斗音效
						m_soundManager.playSE( "rpg_system07", true );
					}

					if ( ++m_currentFrame >= m_animationFrames )
					{
						m_battleStatus = BattleStatus.AfterBattle;
					}
					return true;  // 继续执行

				case BattleStatus.AfterBattle:
					// 停止动画／音效
					EffectManager.get().stopFightingEffect();
					m_soundManager.stopSE();

					// 摄像机回到原来位置
					m_cameraManager.moveTo(
						m_cameraManager.getOriginalPosition(),
						m_cameraManager.getOriginalRotationX(),
						m_cameraManager.getOriginalSize(),
						0.5f );

					m_battleStatus = BattleStatus.ZoomingOut;
					return true;  // 继续执行

				case BattleStatus.ZoomingOut:
					// 移动摄像机
					if ( !m_cameraManager.isMoving() )
					{
						m_battleStatus = BattleStatus.EndBattle;
					}
					return true;  // 继续执行

				case BattleStatus.EndBattle:
					// 结束
					m_battleStatus = BattleStatus.BeforeBattle;
					return false;

				default:
					return false;
				}
			}
		}

		return false;
	}


	//==============================================================================================
	// 非公开成员变量

	/// <summary>该对象用于管理对象</summary>
	private ObjectManager m_objectManager;

	/// <summary>该对象用于管理声音</summary>
	private SoundManager m_soundManager;

	/// <summary>战斗指令的执行状态</summary>
	private BattleStatus m_battleStatus = BattleStatus.BeforeBattle;

	/// <summary>从战斗发生场地朝向摄像机的延伸射线（Ray）</summary>
	private Ray m_rayFromBattlePositionToCamera;

	/// <summary>战斗动画的帧序号</summary>
	private int m_currentFrame = 0;

	/// <summary>战斗动画的帧数</summary>
	private const int m_animationFrames = 300;
}
