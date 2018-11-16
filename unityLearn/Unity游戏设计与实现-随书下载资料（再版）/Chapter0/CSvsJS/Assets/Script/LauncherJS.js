
var ball_prefab : GameObject;						// 小球对象的预设
var player      : PlayerJS;							// 玩家对象
var result      : String = "";						// 当前结果

private var is_launch_ball	: boolean = false;		// “小球发射”标志位

private var good_mess		: String[ ];			// 成功时的消息
private var good_mess_index : int;					// 下次表示的成功信息

// ---------------------------------------------------------------- //

function Start() : void
{
	this.player = GameObject.FindGameObjectWithTag( "Player" ).GetComponent.< PlayerJS >();

	this.is_launch_ball = true;

		//

	this.good_mess = new String[4];

	this.good_mess[0] = "Nice!";
	this.good_mess[1] = "Okay!";
	this.good_mess[2] = "Yatta!";
	this.good_mess[3] = "*^o^*v";

	this.good_mess_index = 0;

}

function Update() : void
{
	// “小球发射”标志位为真并且玩家已经着陆……
	if ( this.is_launch_ball && this.player.isLanded() )
	{
		//

		var ball : GameObject = Instantiate( this.ball_prefab );

		ball.transform.position = new Vector3( 5.0f, 2.0f, 0.0f );

		//　随机算出X轴的速度和到达最高点时的高度

		// 为了使每次生成的值能有一定差异，这里使用整数版本的Range方法
		var x_speed : float = -Random.Range( 2, 7 ) * 2.5f;
		var height  : float =  Random.Range( 2.0f, 3.0f );

		// 通过X轴的速度和玩家位置高度 求出小球初始速度的Y轴分量

		var y_speed : float = this.calc_ball_y_speed( x_speed, height, ball.transform.position );

		var velocity : Vector3 = new Vector3( x_speed, y_speed, 0.0f );

		ball.GetComponent.< BallJS >().velocity = velocity;

		// 清空“小球发射”标志位
		this.is_launch_ball = false;
	}
}

// 通过X轴的速度和玩家位置的高度求出小球初始速度的Y轴分量
private function calc_ball_y_speed( x_speed : float, height : float, ball_position : Vector3 ) : float
{
	var		t : float;
	var		y_speed : float;

	// 到达玩家位置所需要的时间
	t = (this.player.transform.position.x - ball_position.x)/x_speed;

	// 利用公式y = v*t - 0.5f*g*t*t
	// 求出 v 的值
	y_speed = ((height - ball_position.y) - 0.5f*Physics.gravity.y*t*t)/t;

	return(y_speed);
}

// 设置成功／失败
function setResult(is_success : boolean) : void
{
	if(is_success) {

		// 如果成功，则按顺序显示成功消息
		this.result = this.good_mess[this.good_mess_index];

		this.good_mess_index = (this.good_mess_index + 1)%4;

	} else {

		this.result = "Miss!";
	}

	// 经过一定的时间后，显示的结果将消失
	StartCoroutine( clearResult() );
}

// 用于使显示的结果在一定时间后消失的协程
function clearResult() : IEnumerator
{
	yield WaitForSeconds( 0.5f );

	this.result = "";
}

function OnGUI() : void
{
	GUI.Label( Rect( 200, 200, 200, 20 ), this.result );
}

// 小球被销毁时的处理
function OnBallDestroy() : void
{
	// 设置“小球发射”标志位
	this.is_launch_ball = true;
}
