/**
 * @author mr.doob / http://mrdoob.com/
 */

 //四个顶点索引！索引！
 //normal 可以是逐顶点的法线向量 也可以是单面发现向量
THREE.Face4 = function ( a, b, c, d, normal, material ) {

	this.a = a;
	this.b = b;
	this.c = c;
	this.d = d;

	this.centroid = new THREE.Vector3();
	//如果不是一个单独的vector3数据类型 就创建一个
	this.normal = normal instanceof THREE.Vector3 ? normal : new THREE.Vector3();
	//如果不是一个法线数组类型 ，就创建一个空的
	this.vertexNormals =  normal instanceof Array ? normal : [];
	//此面的材质 极其关键！ 说明threejs内部结构是对每个面赋予了一个材质
	//更为关键的是 这是一个材质数组！ 也就是一个面的材质是由多个材质组合构成的
	this.material = material instanceof Array ? material : [ material ];

};


THREE.Face4.prototype = {

	toString: function () {

		return 'THREE.Face4 ( ' + this.a + ', ' + this.b + ', ' + this.c + ' ' + this.d + ' )';

	}

}
