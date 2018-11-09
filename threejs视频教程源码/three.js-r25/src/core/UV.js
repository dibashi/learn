/**
 * @author mr.doob / http://mrdoob.com/
 */
/**
 * 纹理坐标经常被简写为 texture coords，
 * texcoords 或 UVs(发音为 Ew-Vees)， 
 * 我不知道术语 UVs 是从哪来的，
 * 除了一点那就是顶点位置使用 x, y, z, w，
 *  所以对于纹理坐标他们决定使用s, t, u, v，
 * 好让你清楚使用的两个类型的区别。 
 * 有了这些你可能会想它应该读作 Es-Tees，
 * 因为纹理包裹的设置被叫做 TEXTURE_WRAP_S 
 * 和 TEXTURE_WRAP_T， 
 * 但是出于某些原因我的图形相关的同事都叫它 Ew-Vees。

所以现在你就知道了如果有人说 UVs 其实就是再说纹理坐标。
 */
THREE.UV = function ( u, v ) {

	this.u = u || 0;
	this.v = v || 0;

};

THREE.UV.prototype = {

	copy: function ( uv ) {

		this.u = uv.u;
		this.v = uv.v;

	},

	toString: function () {

		return 'THREE.UV (' + this.u + ', ' + this.v + ')';

	}

}
