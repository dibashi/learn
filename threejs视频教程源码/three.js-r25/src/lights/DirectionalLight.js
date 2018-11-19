
//平行光 颜色继承父类 方向是自己的特有属性
THREE.DirectionalLight = function ( hex, intensity ) {

	THREE.Light.call( this, hex );
	//这个变量名似乎不合适
	this.position = new THREE.Vector3( 0, 1, 0 );
	//强度默认为1 用法还不清楚
	this.intensity = intensity || 1;

};

THREE.DirectionalLight.prototype = new THREE.Light();
THREE.DirectionalLight.prototype.constructor = THREE.DirectionalLight; 
