/**
 * @author alteredq / http://alteredqualia.com/
 */
//该对象用于跟踪时间。
//如果performance.now()可用，则 Clock 对象通过该方法实现，否则通过歉精准的Date.now()实现。
//https://developer.mozilla.org/zh-CN/docs/Web/API/Performance/now 这篇文章介绍了两个的细节区别
//实际上可忽略，只需要知道 performance 和date .now()都是返回
//Date.now()返回从1970年1月1日00:00:00 UTC开始经过的毫秒数，
//performance.now()返回小数部分的毫秒数。另一个Date.now()和performance.now()重要区别是后者是单调递增的
THREE.Clock = function ( autoStart ) {

	//autoStart — (可选) 是否要自动开启时钟。默认值是 true。
	this.autoStart = ( autoStart !== undefined ) ? autoStart : true;
	//存储时钟最后一次调用 start 方法的时间。
	this.startTime = 0;
	//存储时钟最后一次调用 start, getElapsedTime 或 getDelta 方法的时间。
	this.oldTime = 0;
	//保存时钟运行的总时长。
	this.elapsedTime = 0;
	//判断时钟是否在运行。
	this.running = false;

};

THREE.Clock.prototype = {

	constructor: THREE.Clock,

	start: function () {

		this.startTime = ( performance || Date ).now();

		this.oldTime = this.startTime;
		this.running = true;

	},

	stop: function () {

		this.getElapsedTime();
		this.running = false;

	},

	getElapsedTime: function () {

		this.getDelta();
		return this.elapsedTime;

	},

	getDelta: function () {

		var diff = 0;

		if ( this.autoStart && ! this.running ) {

			this.start();

		}

		if ( this.running ) {

			var newTime = ( performance || Date ).now();

			diff = ( newTime - this.oldTime ) / 1000;
			this.oldTime = newTime;

			this.elapsedTime += diff;

		}

		return diff;

	}

};
