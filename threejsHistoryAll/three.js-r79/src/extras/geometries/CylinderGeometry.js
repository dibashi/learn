/**
 * @author mrdoob / http://mrdoob.com/
 */

THREE.CylinderGeometry = function ( radiusTop, radiusBottom, height, radialSegments, heightSegments, openEnded, thetaStart, thetaLength ) {

	THREE.Geometry.call( this );

	this.type = 'CylinderGeometry';

	/**
	 * An object with a property for each of the constructor parameters. 
	 * Any modification after instantiation does not change the geometry.
	 */
	//从官方注释可以看到，这是用于构造几何体的参数，几何体一旦实例化之后，修改这些参数不会更改几何体
	this.parameters = {
		radiusTop: radiusTop,
		radiusBottom: radiusBottom,
		height: height,
		radialSegments: radialSegments,
		heightSegments: heightSegments,
		openEnded: openEnded,
		thetaStart: thetaStart,
		thetaLength: thetaLength
	};

	this.fromBufferGeometry( new THREE.CylinderBufferGeometry( radiusTop, radiusBottom, height, radialSegments, heightSegments, openEnded, thetaStart, thetaLength ) );
	this.mergeVertices();

};

THREE.CylinderGeometry.prototype = Object.create( THREE.Geometry.prototype );
THREE.CylinderGeometry.prototype.constructor = THREE.CylinderGeometry;
