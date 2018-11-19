/**
 * @author mr.doob / http://mrdoob.com/
 */

 //这是一个线框的材质 线的宽度，线的颜色
THREE.MeshColorStrokeMaterial = function ( hex, opacity, lineWidth ) {
	//线框宽度默认值为1
	this.lineWidth = lineWidth || 1;

	this.color = new THREE.Color( ( opacity >= 0 ? ( opacity * 0xff ) << 24 : 0xff000000 ) | hex );

	this.toString = function () {

		return 'THREE.MeshColorStrokeMaterial ( lineWidth: ' + this.lineWidth + ', color: ' + this.color + ' )';

	};

};
