/**
 * @author mr.doob / http://mrdoob.com/
 */

//顶点构造函数
THREE.Vertex = function ( position, normal ) {
	//信息量还是很多的，需要位置 
	this.position = position || new THREE.Vector3();
	//下面这两个不太清楚 将来注解，预测 是一个是顶点的世界坐标，一个是顶点的屏幕坐标
	this.positionWorld = new THREE.Vector3();
	this.positionScreen = new THREE.Vector4();

	//顶点的法线
	this.normal = normal || new THREE.Vector3();
	//世界坐标系下的法线？
	this.normalWorld = new THREE.Vector3();
	//屏幕坐标系下的法线？ 屏幕坐标系是什么？
	this.normalScreen = new THREE.Vector3();
	//顶点的切线 可能是用于凹凸纹理映射
	this.tangent = new THREE.Vector4();
	
	this.__visible = true;

};

THREE.Vertex.prototype = {

	toString: function () {

		return 'THREE.Vertex ( position: ' + this.position + ', normal: ' + this.normal + ' )';
	}
};
