import System.Collections.Generic;

private var texts : List.< String >;
//                      ↑ 注意不要忘记（“.”）符号

private static var instance : DebugPrintJS = null;

// ---------------------------------------------------------------- //

static function getInstance() : DebugPrintJS
{
	if ( DebugPrintJS.instance == null )
	{
		var go : GameObject = GameObject( "DebugPrint" );

		DebugPrintJS.instance = go.AddComponent( DebugPrintJS );
		DebugPrintJS.instance.create();

		DontDestroyOnLoad( go );
	}

	return DebugPrintJS.instance;
}

static function print( text : String ) : void
{
	var dp : DebugPrintJS = DebugPrintJS.getInstance();

	dp.texts.Add( text );
}

function Start() : void
{
}

function Update() : void
{
}

function OnGUI() : void
{
	var x : int = 100;
	var y : int = 100;

	for ( var text : String in this.texts.ToArray() )
	{
		GUI.Label( Rect( x, y, 100, 20 ), text );
		y += 20;
	}

	// 绘制结束后，清空缓冲区
	if ( UnityEngine.Event.current.type == UnityEngine.EventType.Repaint )
	{
		this.texts.Clear();
	}
}

function create() : void
{
	this.texts = new List.< String >();
}
