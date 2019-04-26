/**
 * @author mrdoob / http://mrdoob.com/
 */

 /**
  * A series of lines drawn between pairs of vertices.
  * 这是线段列表，就是两个顶点一条线 收尾的顶点不共用
  * line是共用的
  * 在renderer中的实现
  * 可以看到 如果他是THREE.Line 的实例，先进行lineWidth的设置
  * 然后会设置渲染模式 如果又是LineSegments的实例 按gl.LINES渲染
  * 否则用gl.LINE_STRIP
  * THREE js 是如何画有宽度的直线的？ 以后看下着色器吧 TODO!!
  * if (object instanceof THREE.Line) {

			var lineWidth = material.linewidth;

			if (lineWidth === undefined) lineWidth = 1; // Not using Line*Material

			state.setLineWidth(lineWidth * getTargetPixelRatio());

			if (object instanceof THREE.LineSegments) {

				renderer.setMode(_gl.LINES);

			} else {

				renderer.setMode(_gl.LINE_STRIP);

			}

		}
  */
THREE.LineSegments = function ( geometry, material ) {

	THREE.Line.call( this, geometry, material );

	this.type = 'LineSegments';

};

THREE.LineSegments.prototype = Object.assign( Object.create( THREE.Line.prototype ), {

	constructor: THREE.LineSegments

} );
