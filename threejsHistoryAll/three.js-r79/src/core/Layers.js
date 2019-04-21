/**
 * @author mrdoob / http://mrdoob.com/
 */
/**
 * A Layers object assigns an Object3D to 1 or more of 32 layers numbered 0 to 31 - 
 * internally the layers are stored as a bit mask,
 *  and by default all Object3Ds are a member of layer 0.

!!!!!这是这个类的目的-->This can be used to control visibility - 
-->具体的算法-->an object must share a layer with a camera to be visible when that camera's view is renderered.

All classes that inherit from Object3D have an Object3D.layers property which is an instance of this class.
 */
THREE.Layers = function () {

	this.mask = 1;

};

THREE.Layers.prototype = {

	constructor: THREE.Layers,

	//将其置为某一层，覆盖之前的
	set: function ( channel ) {

		this.mask = 1 << channel;

	},

	//让其成为某一层，保留之前的 比如摄像机想多拍摄一层
	enable: function ( channel ) {

		this.mask |= 1 << channel;

	},

	//切换某层 若其在此层 则关闭 若不在 则加入
	toggle: function ( channel ) {

		this.mask ^= 1 << channel;

	},

	//关闭某一层，让其他为1此层为0 然后且就可以保留之前 并且删除这一层
	disable: function ( channel ) {

		this.mask &= ~ ( 1 << channel );

	},

	//用于测试 摄像机是否拍摄此物体 renderer中有具体用法
	test: function ( layers ) {

		return ( this.mask & layers.mask ) !== 0;

	}

};
