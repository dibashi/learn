/**
 * @author mrdoob / http://mrdoob.com/
 * @author alteredq / http://alteredqualia.com/
 * @author mikael emtinger / http://gomo.se/
 * @author jonobr1 / http://jonobr1.com/
 */

THREE.Mesh = function ( geometry, material ) {

	THREE.Object3D.call( this );

	this.type = 'Mesh';

	this.geometry = geometry !== undefined ? geometry : new THREE.BufferGeometry();
	this.material = material !== undefined ? material : new THREE.MeshBasicMaterial( { color: Math.random() * 0xffffff } );
	

	/**
	 * 
		if (object instanceof THREE.Mesh) {

			if (material.wireframe === true) {

				state.setLineWidth(material.wireframeLinewidth * getTargetPixelRatio());
				renderer.setMode(_gl.LINES);

			} else {

				switch (object.drawMode) {

					case THREE.TrianglesDrawMode:
						renderer.setMode(_gl.TRIANGLES);
						break;

					case THREE.TriangleStripDrawMode:
						renderer.setMode(_gl.TRIANGLE_STRIP);
						break;

					case THREE.TriangleFanDrawMode:
						renderer.setMode(_gl.TRIANGLE_FAN);
						break;

				}

			}
	 */
	//根据WebGLRenderer里的代码可以看到，如果材质的线框属性为true
	//就会以线条来渲染mesh
	//否则根据drawMode来以 三角形组 三角形带 三角形扇来渲染
	/**
	 * 	TrianglesDrawMode: 0,
	TriangleStripDrawMode: 1,
	TriangleFanDrawMode: 2,
	 */
	this.drawMode = THREE.TrianglesDrawMode;

	this.updateMorphTargets();

};

THREE.Mesh.prototype = Object.assign( Object.create( THREE.Object3D.prototype ), {

	constructor: THREE.Mesh,

	setDrawMode: function ( value ) {

		this.drawMode = value;

	},

	copy: function ( source ) {

		THREE.Object3D.prototype.copy.call( this, source );

		this.drawMode = source.drawMode;

		return this;

	},

	updateMorphTargets: function () {

		if ( this.geometry.morphTargets !== undefined && this.geometry.morphTargets.length > 0 ) {

			this.morphTargetBase = - 1;
			//创建了一个数组。内部是对每个变形目标影响的权重，初始化为0
			this.morphTargetInfluences = [];
			//内部复制了一份变形目标数据，以键值对的形式 ，也不能称之为复制，
			//又多了一个指针来引用顶点数据（这里说错了，应该是根据名，获得他在geometry中的索引)
			this.morphTargetDictionary = {};

			for ( var m = 0, ml = this.geometry.morphTargets.length; m < ml; m ++ ) {

				this.morphTargetInfluences.push( 0 );
				this.morphTargetDictionary[ this.geometry.morphTargets[ m ].name ] = m;

			}

		}

	},

	getMorphTargetIndexByName: function ( name ) {

		//正如updateMorphTargets函数中创建的字典
		//this.morphTargetDictionary[ this.geometry.morphTargets[ m ].name ] = m;
		//返回的是m，也就是变形目标在morphTargets中的索引
		if ( this.morphTargetDictionary[ name ] !== undefined ) {

			return this.morphTargetDictionary[ name ];

		}

		console.warn( 'THREE.Mesh.getMorphTargetIndexByName: morph target ' + name + ' does not exist. Returning 0.' );

		return 0;

	},

	raycast: ( function () {

		var inverseMatrix = new THREE.Matrix4();
		var ray = new THREE.Ray();
		var sphere = new THREE.Sphere();

		var vA = new THREE.Vector3();
		var vB = new THREE.Vector3();
		var vC = new THREE.Vector3();

		var tempA = new THREE.Vector3();
		var tempB = new THREE.Vector3();
		var tempC = new THREE.Vector3();

		var uvA = new THREE.Vector2();
		var uvB = new THREE.Vector2();
		var uvC = new THREE.Vector2();

		var barycoord = new THREE.Vector3();

		var intersectionPoint = new THREE.Vector3();
		var intersectionPointWorld = new THREE.Vector3();

		function uvIntersection( point, p1, p2, p3, uv1, uv2, uv3 ) {

			THREE.Triangle.barycoordFromPoint( point, p1, p2, p3, barycoord );

			uv1.multiplyScalar( barycoord.x );
			uv2.multiplyScalar( barycoord.y );
			uv3.multiplyScalar( barycoord.z );

			uv1.add( uv2 ).add( uv3 );

			return uv1.clone();

		}

		function checkIntersection( object, raycaster, ray, pA, pB, pC, point ) {

			var intersect;
			var material = object.material;

			if ( material.side === THREE.BackSide ) {

				intersect = ray.intersectTriangle( pC, pB, pA, true, point );

			} else {

				intersect = ray.intersectTriangle( pA, pB, pC, material.side !== THREE.DoubleSide, point );

			}

			if ( intersect === null ) return null;

			intersectionPointWorld.copy( point );
			intersectionPointWorld.applyMatrix4( object.matrixWorld );

			var distance = raycaster.ray.origin.distanceTo( intersectionPointWorld );

			if ( distance < raycaster.near || distance > raycaster.far ) return null;

			return {
				distance: distance,
				point: intersectionPointWorld.clone(),
				object: object
			};

		}

		function checkBufferGeometryIntersection( object, raycaster, ray, positions, uvs, a, b, c ) {

			vA.fromArray( positions, a * 3 );
			vB.fromArray( positions, b * 3 );
			vC.fromArray( positions, c * 3 );

			var intersection = checkIntersection( object, raycaster, ray, vA, vB, vC, intersectionPoint );

			if ( intersection ) {

				if ( uvs ) {

					uvA.fromArray( uvs, a * 2 );
					uvB.fromArray( uvs, b * 2 );
					uvC.fromArray( uvs, c * 2 );

					intersection.uv = uvIntersection( intersectionPoint,  vA, vB, vC,  uvA, uvB, uvC );

				}

				intersection.face = new THREE.Face3( a, b, c, THREE.Triangle.normal( vA, vB, vC ) );
				intersection.faceIndex = a;

			}

			return intersection;

		}

		return function raycast( raycaster, intersects ) {

			var geometry = this.geometry;
			var material = this.material;
			var matrixWorld = this.matrixWorld;

			if ( material === undefined ) return;

			// Checking boundingSphere distance to ray

			if ( geometry.boundingSphere === null ) geometry.computeBoundingSphere();

			sphere.copy( geometry.boundingSphere );
			sphere.applyMatrix4( matrixWorld );

			if ( raycaster.ray.intersectsSphere( sphere ) === false ) return;

			//wtf!! 上面包围球用 世界坐标 下面的 包围盒 为什么要在对象坐标系进行判断？ ray是在世界坐标的

			//作者应该是处于性能考虑的，比较变换一条射线比较简单，而变换一个大模型的所有顶点就麻烦了
			//下面会用到 逐三角面的 那是对象坐标系下的
			inverseMatrix.getInverse( matrixWorld );
			ray.copy( raycaster.ray ).applyMatrix4( inverseMatrix );

			// Check boundingBox before continuing
			//没有就不要了？看来包围球的计算复杂度低？
			if ( geometry.boundingBox !== null ) {

				if ( ray.intersectsBox( geometry.boundingBox ) === false ) return;

			}

			var uvs, intersection;
			//逐三角面判断
			if ( geometry instanceof THREE.BufferGeometry ) {

				var a, b, c;
				var index = geometry.index;
				var attributes = geometry.attributes;
				var positions = attributes.position.array;

				if ( attributes.uv !== undefined ) {

					uvs = attributes.uv.array;

				}

				if ( index !== null ) {

					var indices = index.array;

					for ( var i = 0, l = indices.length; i < l; i += 3 ) {

						a = indices[ i ];
						b = indices[ i + 1 ];
						c = indices[ i + 2 ];

						intersection = checkBufferGeometryIntersection( this, raycaster, ray, positions, uvs, a, b, c );

						if ( intersection ) {

							intersection.faceIndex = Math.floor( i / 3 ); // triangle number in indices buffer semantics
							intersects.push( intersection );

						}

					}

				} else {


					for ( var i = 0, l = positions.length; i < l; i += 9 ) {

						a = i / 3;
						b = a + 1;
						c = a + 2;

						intersection = checkBufferGeometryIntersection( this, raycaster, ray, positions, uvs, a, b, c );

						if ( intersection ) {

							intersection.index = a; // triangle number in positions buffer semantics
							intersects.push( intersection );

						}

					}

				}

			} else if ( geometry instanceof THREE.Geometry ) {

				var fvA, fvB, fvC;
				var isFaceMaterial = material instanceof THREE.MultiMaterial;
				var materials = isFaceMaterial === true ? material.materials : null;

				var vertices = geometry.vertices;
				var faces = geometry.faces;
				var faceVertexUvs = geometry.faceVertexUvs[ 0 ];
				if ( faceVertexUvs.length > 0 ) uvs = faceVertexUvs;

				for ( var f = 0, fl = faces.length; f < fl; f ++ ) {

					var face = faces[ f ];
					var faceMaterial = isFaceMaterial === true ? materials[ face.materialIndex ] : material;

					if ( faceMaterial === undefined ) continue;

					fvA = vertices[ face.a ];
					fvB = vertices[ face.b ];
					fvC = vertices[ face.c ];

					if ( faceMaterial.morphTargets === true ) {

						var morphTargets = geometry.morphTargets;
						var morphInfluences = this.morphTargetInfluences;

						vA.set( 0, 0, 0 );
						vB.set( 0, 0, 0 );
						vC.set( 0, 0, 0 );

						for ( var t = 0, tl = morphTargets.length; t < tl; t ++ ) {

							var influence = morphInfluences[ t ];

							if ( influence === 0 ) continue;

							var targets = morphTargets[ t ].vertices;

							vA.addScaledVector( tempA.subVectors( targets[ face.a ], fvA ), influence );
							vB.addScaledVector( tempB.subVectors( targets[ face.b ], fvB ), influence );
							vC.addScaledVector( tempC.subVectors( targets[ face.c ], fvC ), influence );

						}

						vA.add( fvA );
						vB.add( fvB );
						vC.add( fvC );

						fvA = vA;
						fvB = vB;
						fvC = vC;

					}

					intersection = checkIntersection( this, raycaster, ray, fvA, fvB, fvC, intersectionPoint );

					if ( intersection ) {

						if ( uvs ) {

							var uvs_f = uvs[ f ];
							uvA.copy( uvs_f[ 0 ] );
							uvB.copy( uvs_f[ 1 ] );
							uvC.copy( uvs_f[ 2 ] );

							intersection.uv = uvIntersection( intersectionPoint, fvA, fvB, fvC, uvA, uvB, uvC );

						}

						intersection.face = face;
						intersection.faceIndex = f;
						intersects.push( intersection );

					}

				}

			}

		};

	}() ),

	clone: function () {
		//这里的克隆体 指向的 同一个材质和几何对象？？
		return new this.constructor( this.geometry, this.material ).copy( this );

	}

} );
