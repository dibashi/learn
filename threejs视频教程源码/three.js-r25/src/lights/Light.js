
//抽象的光，内部只有一个光的颜色 alpha永远为255
THREE.Light = function ( hex ) {

	this.color = new THREE.Color( 0xff << 24 | hex );

};
