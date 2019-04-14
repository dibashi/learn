/**
 * @author mr.doob / http://mrdoob.com/
 */
//其实 projection matrix 和 view matrix的相乘的矩阵vp矩阵
//内部维护了构造这两个矩阵的所有变量 对外没有开放相机操作 应该是没有写完
//应该这样用
/**
 * 
				camera = new THREE.Camera( 75, SCREEN_WIDTH / SCREEN_HEIGHT, 1, 100000 );
                camera.position.z = 500;
                camera.updateMatrix();
 */
THREE.Camera = function ( fov, aspect, near, far ) {

	this.fov = fov;
	this.aspect = aspect;
	this.position = new THREE.Vector3( 0, 0, 0 );
	this.target = { position: new THREE.Vector3( 0, 0, 0 ) };

	this.projectionMatrix = THREE.Matrix4.makePerspective( fov, aspect, near, far );
	this.up = new THREE.Vector3( 0, 1, 0 );
	this.matrix = new THREE.Matrix4();

	//若为true 在WebGLRender.js中 会自动更新相机
	//camera.autoUpdateMatrix && camera.updateMatrix();
	this.autoUpdateMatrix = true;

	this.updateMatrix = function () {

		this.matrix.lookAt( this.position, this.target.position, this.up );

	};

	this.toString = function () {

		return 'THREE.Camera ( ' + this.position + ', ' + this.target.position + ' )';

	};

};
