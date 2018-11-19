/**
 * @author mr.doob / http://mrdoob.com/
 */
//内部仅维护了一个颜色值
THREE.MeshColorFillMaterial = function ( hex, opacity ) {
	//opacity若未定义 必然小于任何数 做过测试 
	//若定义 貌似是个小数，乘以255 然后移位至最前方 与hex或运算 得出8个16进制数 即rgba颜色表示
	this.color = new THREE.Color( ( opacity >= 0 ? ( opacity * 0xff ) << 24 : 0xff000000 ) | hex );

	this.toString = function () {

		return 'THREE.MeshColorFillMaterial ( color: ' + this.color + ' )';

	};

};
