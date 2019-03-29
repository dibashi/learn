/**
 * @author mr.doob / http://mrdoob.com/
 */
/**
 * 6个值 真正确定此长方形的是4个值 左下坐标 右上坐标
 * 疑惑在 为什么作者把函数放在了内部 是为了闭包访问？
 */
THREE.Rectangle = function () {

	var _x1, _y1, _x2, _y2,
	_width, _height,
	_isEmpty = true;

	function resize() {

		_width = _x2 - _x1;
		_height = _y2 - _y1;

	}

	this.getX = function () {

		return _x1;

	};

	this.getY = function () {

		return _y1;

	};

	this.getWidth = function () {

		return _width;

	};

	this.getHeight = function () {

		return _height;

	};

	this.getX1 = function() {

		return _x1;

	};

	this.getY1 = function() {

		return _y1;

	};

	this.getX2 = function() {

		return _x2;

	};

	this.getY2 = function() {

		return _y2;

	};

	this.set = function ( x1, y1, x2, y2 ) {

		_isEmpty = false;

		_x1 = x1; _y1 = y1;
		_x2 = x2; _y2 = y2;

		resize();

	};
	/**
	 * 好奇怪不知道要做什么 lhtodo
	 */
	this.addPoint = function ( x, y ) {

		if ( _isEmpty ) {

			_isEmpty = false;
			_x1 = x; _y1 = y;
			_x2 = x; _y2 = y;

		} else {

			_x1 = Math.min( _x1, x );
			_y1 = Math.min( _y1, y );
			_x2 = Math.max( _x2, x );
			_y2 = Math.max( _y2, y );

		}

		resize();

	};

	this.addRectangle = function ( r ) {

		if ( _isEmpty ) {

			_isEmpty = false;
			_x1 = r.getX1(); _y1 = r.getY1();
			_x2 = r.getX2(); _y2 = r.getY2();

		} else {

			_x1 = Math.min(_x1, r.getX1());
			_y1 = Math.min(_y1, r.getY1());
			_x2 = Math.max(_x2, r.getX2());
			_y2 = Math.max(_y2, r.getY2());

		}

		resize();

	};

	this.inflate = function ( v ) {

		_x1 -= v; _y1 -= v;
		_x2 += v; _y2 += v;

		resize();

	};

	this.minSelf = function( r ) {

		_x1 = Math.max( _x1, r.getX1() );
		_y1 = Math.max( _y1, r.getY1() );
		_x2 = Math.min( _x2, r.getX2() );
		_y2 = Math.min( _y2, r.getY2() );

		resize();

	};

	/*
	this.containsPoint = function (x, y) {

		return x > _x1 && x < _x2 && y > _y1 && y < _y2;

	}
	*/
	/**
	 * https://blog.csdn.net/szfhy/article/details/49740191解释的很清楚
	 * 两个矩形中心点距离x 必须小于 两个矩形宽度和的一半
	 * 距离y 必须小于 两个矩形高度和的一半
	 * 这里作者是另一种方法https://blog.csdn.net/wongson/article/details/45314551
	 */
	this.instersects = function ( r ) {

		return Math.min( _x2, r.getX2() ) - Math.max( _x1, r.getX1() ) >= 0 && Math.min( _y2, r.getY2() ) - Math.max( _y1, r.getY1() ) >= 0;

	};

	this.empty = function () {

		_isEmpty = true;

		_x1 = 0; _y1 = 0;
		_x2 = 0; _y2 = 0;

		resize();

	};

	this.isEmpty = function () {

		return _isEmpty;

	};

	this.toString = function () {

		return "THREE.Rectangle (x1: " + _x1 + ", y1: " + _y2 + ", x2: " + _x2 + ", y1: " + _y1 + ", width: " + _width + ", height: " + _height + ")";

	};

};
