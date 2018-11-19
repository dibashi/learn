/**
 * @author mr.doob / http://mrdoob.com/
 */
//本质就是对 模型矩阵的封装，性能看起来实现的很低
THREE.Object3D = function ( material ) {

	this.position = new THREE.Vector3();
	this.rotation = new THREE.Vector3();
	this.scale = new THREE.Vector3( 1, 1, 1 );

	this.matrix = new THREE.Matrix4();
	this.matrixTranslation = new THREE.Matrix4();
	this.matrixRotation = new THREE.Matrix4();
	this.matrixScale = new THREE.Matrix4();
	//这个是什么呢
	this.screen = new THREE.Vector3();
	//若为true 则主渲染循环中 每帧都会自动更新物体的矩阵
	//不用再手动更新，但是！我还是要吐槽这里的性能问题很严重，作者的代码看起来他对矩阵运算的功底不深
	//写的畏首畏尾。
	this.autoUpdateMatrix = true;

	//性能看起来好低 两个关键点 1 先缩放再旋转 最后平移 2先旋转z再y最后x
	this.updateMatrix = function () {

		this.matrixPosition = THREE.Matrix4.translationMatrix( this.position.x, this.position.y, this.position.z );

		this.matrixRotation = THREE.Matrix4.rotationXMatrix( this.rotation.x );
		this.matrixRotation.multiplySelf( THREE.Matrix4.rotationYMatrix( this.rotation.y ) );
		this.matrixRotation.multiplySelf( THREE.Matrix4.rotationZMatrix( this.rotation.z ) );

		this.matrixScale = THREE.Matrix4.scaleMatrix( this.scale.x, this.scale.y, this.scale.z );

		this.matrix.copy( this.matrixPosition );
		this.matrix.multiplySelf( this.matrixRotation );
		this.matrix.multiplySelf( this.matrixScale );

	};

};
