/**
 * @author mrdoob / http://mrdoob.com/
 * @author mikael emtinger / http://gomo.se/
 * @author WestLangley / http://github.com/WestLangley
*/

THREE.Camera = function () {

	THREE.Object3D.call( this );

	this.type = 'Camera';
	/**
	 * This is the inverse of matrixWorld. 
	 * MatrixWorld contains the Matrix which has the world transform of the Camera.
	 */
	//camera.matrixWorldInverse.getInverse( camera.matrixWorld );
	//对象的模型视图矩阵
	//object.modelViewMatrix.multiplyMatrices( camera.matrixWorldInverse, object.matrixWorld );
	this.matrixWorldInverse = new THREE.Matrix4();
	/**
	 * This is the matrix which contains the projection.
	 */
	this.projectionMatrix = new THREE.Matrix4();

};

THREE.Camera.prototype = Object.create( THREE.Object3D.prototype );
THREE.Camera.prototype.constructor = THREE.Camera;

THREE.Camera.prototype.getWorldDirection = function () {

	var quaternion = new THREE.Quaternion();

	return function getWorldDirection( optionalTarget ) {

		var result = optionalTarget || new THREE.Vector3();

		this.getWorldQuaternion( quaternion );

		return result.set( 0, 0, - 1 ).applyQuaternion( quaternion );

	};

}();

THREE.Camera.prototype.lookAt = function () {

	// This routine does not support cameras with rotated and/or translated parent(s)

	var m1 = new THREE.Matrix4();

	return function lookAt( vector ) {

		m1.lookAt( this.position, vector, this.up );

		this.quaternion.setFromRotationMatrix( m1 );

	};

}();

THREE.Camera.prototype.clone = function () {

	return new this.constructor().copy( this );

};

THREE.Camera.prototype.copy = function ( source ) {

	THREE.Object3D.prototype.copy.call( this, source );

	this.matrixWorldInverse.copy( source.matrixWorldInverse );
	this.projectionMatrix.copy( source.projectionMatrix );

	return this;

};