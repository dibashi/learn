/**
 * @author mr.doob / http://mrdoob.com/
 * @author mikael emtinger / http://gomo.se/
 */

 
THREE.Camera = function () {

	if ( arguments.length ) {
		//作者把这个接口废弃了
		console.warn( 'DEPRECATED: Camera() is now PerspectiveCamera() or OrthographicCamera().' );
		return new THREE.PerspectiveCamera( arguments[ 0 ], arguments[ 1 ], arguments[ 2 ], arguments[ 3 ] );

	}

	//去调用threejs中的基类进行对所有对象都需要的初始化
	THREE.Object3D.call( this );

	//核心任务就是创建三个矩阵 其中这个两个逆矩阵感觉好奇怪
	this.matrixWorldInverse = new THREE.Matrix4();

	this.projectionMatrix = new THREE.Matrix4();
	this.projectionMatrixInverse = new THREE.Matrix4();


};

THREE.Camera.prototype = new THREE.Object3D();
THREE.Camera.prototype.constructor = THREE.Camera;

THREE.Camera.prototype.lookAt = function ( vector ) {

	// TODO: Add hierarchy support.

	this.matrix.lookAt( this.position, vector, this.up );

	if ( this.rotationAutoUpdate ) {

		this.rotation.setRotationFromMatrix( this.matrix );

	}

};
