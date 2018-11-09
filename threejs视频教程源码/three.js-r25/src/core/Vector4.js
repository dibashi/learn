/**
 * @author supereggbert / http://www.paulbrunt.co.uk/
 * @author philogb / http://blog.thejit.org/
 */
/**
 * color vec2 vec3 vec4 这几个封装类 内部数据极少 搭配了 几个操作函数
 */

THREE.Vector4 = function ( x, y, z, w ) {

	this.x = x || 0;
	this.y = y || 0;
	this.z = z || 0;
	/**
	 * 如果w是0 它就必须被修改为1 其他随便 这是其次坐标的定义
	 */
	this.w = w || 1;

};

THREE.Vector4.prototype = {

	set: function ( x, y, z, w ) {

		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;

		return this;

	},

	copy: function ( v ) {

		this.x = v.x;
		this.y = v.y;
		this.z = v.z;
		this.w = v.w;

		return this;

	},

	add: function ( v1, v2 ) {

		this.x = v1.x + v2.x;
		this.y = v1.y + v2.y;
		this.z = v1.z + v2.z;
		this.w = v1.w + v2.w;

		return this;

	},

	addSelf: function ( v ) {

		this.x += v.x;
		this.y += v.y;
		this.z += v.z;
		this.w += v.w;

		return this;

	},

	sub: function ( v1, v2 ) {

		this.x = v1.x - v2.x;
		this.y = v1.y - v2.y;
		this.z = v1.z - v2.z;
		this.w = v1.w - v2.w;

		return this;

	},

	subSelf: function ( v ) {

		this.x -= v.x;
		this.y -= v.y;
		this.z -= v.z;
		this.w -= v.w;

		return this;

	},
	/**
	 * 创建了一个与此对象数值一模一样的对象
	 */
	clone: function () {

		return new THREE.Vector4( this.x, this.y, this.z, this.w );

	},

	toString: function () {

		return 'THREE.Vector4 (' + this.x + ', ' + this.y + ', ' + this.z + ', ' + this.w + ')';

	}

};
