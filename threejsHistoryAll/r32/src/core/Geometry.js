/**
 * @author mr.doob / http://mrdoob.com/
 * @author kile / http://kile.stravaganza.org/
 * @author alteredq / http://alteredqualia.com/
 */

THREE.Geometry = function () {

	this.vertices = [];
	this.faces = [];
	this.uvs = [];

	this.boundingBox = null;
	this.boundingSphere = null;

	this.geometryChunks = {};

	this.hasTangents = false;

};

THREE.Geometry.prototype = {
	//计算重心？质心？中心？中心是什么？这里作者直接全部相加除以顶点数量
	//我很难理解这个值到底是什么，如果规则的话 就是中心点
	computeCentroids: function () {

		var f, fl, face;

		for ( f = 0, fl = this.faces.length; f < fl; f ++ ) {

			face = this.faces[ f ];
			face.centroid.set( 0, 0, 0 );

			if ( face instanceof THREE.Face3 ) {

				face.centroid.addSelf( this.vertices[ face.a ].position );
				face.centroid.addSelf( this.vertices[ face.b ].position );
				face.centroid.addSelf( this.vertices[ face.c ].position );
				face.centroid.divideScalar( 3 );

			} else if ( face instanceof THREE.Face4 ) {

				face.centroid.addSelf( this.vertices[ face.a ].position );
				face.centroid.addSelf( this.vertices[ face.b ].position );
				face.centroid.addSelf( this.vertices[ face.c ].position );
				face.centroid.addSelf( this.vertices[ face.d ].position );
				face.centroid.divideScalar( 4 );

			}

		}

	},

	//计算每个面的法线
	//算法思路：三个顶点，然后计算出两个向量，根据两个向量的叉积计算法线
	//也就是说 这个算法 对某个面上的顶点而言 所有的法向量都是一样的 插值后还是一样的
	computeFaceNormals: function ( useVertexNormals ) {

		var n, nl, v, vl, vertex, f, fl, face, vA, vB, vC,
		//用于计算叉积的向量
		cb = new THREE.Vector3(), ab = new THREE.Vector3();

		//先全部置为0，然后再计算
		for ( v = 0, vl = this.vertices.length; v < vl; v ++ ) {

			vertex = this.vertices[ v ];
			vertex.normal.set( 0, 0, 0 );

		}

		for ( f = 0, fl = this.faces.length; f < fl; f ++ ) {

			face = this.faces[ f ];
			//如果使用顶点法线 并且该面确实有顶点法线集合
			if ( useVertexNormals && face.vertexNormals.length  ) {
				
				cb.set( 0, 0, 0 );

				for ( n = 0, nl = face.normal.length; n < nl; n++ ) {
					
					cb.addSelf( face.vertexNormals[n] );

				}

				//最终面的法线是三个顶点法线相加除以三，然后单位化
				cb.divideScalar( 3 );

				if ( ! cb.isZero() ) {

					cb.normalize();

				}

				face.normal.copy( cb );

			} else {
				//直接根据顶点坐标计算法线
				vA = this.vertices[ face.a ];
				vB = this.vertices[ face.b ];
				vC = this.vertices[ face.c ];

				cb.sub( vC.position, vB.position );
				ab.sub( vA.position, vB.position );
				cb.crossSelf( ab );

				if ( !cb.isZero() ) {

					cb.normalize();

				}

				face.normal.copy( cb );

			}

		}

	},

	//计算每个顶点的法线
	computeVertexNormals: function () {

		var v, vl, vertices = [],
		f, fl, face;

		for ( v = 0, vl = this.vertices.length; v < vl; v ++ ) {
			//先初始化
			vertices[ v ] = new THREE.Vector3();

		}

		for ( f = 0, fl = this.faces.length; f < fl; f ++ ) {

			face = this.faces[ f ];
			//看看各个面试face3，还是face4
			if ( face instanceof THREE.Face3 ) {
				//是face3，各个顶点的法线置为面法线
				vertices[ face.a ].addSelf( face.normal );
				vertices[ face.b ].addSelf( face.normal );
				vertices[ face.c ].addSelf( face.normal );

			} else if ( face instanceof THREE.Face4 ) {
				//是face4 各个顶点的法线置为面法线
				vertices[ face.a ].addSelf( face.normal );
				vertices[ face.b ].addSelf( face.normal );
				vertices[ face.c ].addSelf( face.normal );
				vertices[ face.d ].addSelf( face.normal );

			}

		}

		//全部单位化，总觉得还多地方有冗余运算
		for ( v = 0, vl = this.vertices.length; v < vl; v ++ ) {

			vertices[ v ].normalize();

		}

		for ( f = 0, fl = this.faces.length; f < fl; f ++ ) {

			face = this.faces[ f ];
			//将数据同步到face里，这样 顶点和face的法线数据能保持一致，好麻烦
			if ( face instanceof THREE.Face3 ) {

				face.vertexNormals[ 0 ] = vertices[ face.a ].clone();
				face.vertexNormals[ 1 ] = vertices[ face.b ].clone();
				face.vertexNormals[ 2 ] = vertices[ face.c ].clone();

			} else if ( face instanceof THREE.Face4 ) {

				face.vertexNormals[ 0 ] = vertices[ face.a ].clone();
				face.vertexNormals[ 1 ] = vertices[ face.b ].clone();
				face.vertexNormals[ 2 ] = vertices[ face.c ].clone();
				face.vertexNormals[ 3 ] = vertices[ face.d ].clone();

			}

		}

	},

	//这个是为了凹凸映射做的，比较麻烦，以后阅读
	computeTangents: function() {

		// based on http://www.terathon.com/code/tangent.html
		// tangents go to vertices

		var f, fl, v, vl, face, uv, vA, vB, vC, uvA, uvB, uvC,
			x1, x2, y1, y2, z1, z2,
			s1, s2, t1, t2, r, t, test,
			tan1 = [], tan2 = [],
			sdir = new THREE.Vector3(), tdir = new THREE.Vector3(),
			tmp = new THREE.Vector3(), tmp2 = new THREE.Vector3(),
			n = new THREE.Vector3(), w;

		for ( v = 0, vl = this.vertices.length; v < vl; v ++ ) {

			tan1[ v ] = new THREE.Vector3();
			tan2[ v ] = new THREE.Vector3();

		}

		function handleTriangle( context, a, b, c, ua, ub, uc ) {

			vA = context.vertices[ a ].position;
			vB = context.vertices[ b ].position;
			vC = context.vertices[ c ].position;

			uvA = uv[ ua ];
			uvB = uv[ ub ];
			uvC = uv[ uc ];

			x1 = vB.x - vA.x;
			x2 = vC.x - vA.x;
			y1 = vB.y - vA.y;
			y2 = vC.y - vA.y;
			z1 = vB.z - vA.z;
			z2 = vC.z - vA.z;

			s1 = uvB.u - uvA.u;
			s2 = uvC.u - uvA.u;
			t1 = uvB.v - uvA.v;
			t2 = uvC.v - uvA.v;

			r = 1.0 / ( s1 * t2 - s2 * t1 );
			sdir.set( ( t2 * x1 - t1 * x2 ) * r,
					  ( t2 * y1 - t1 * y2 ) * r,
					  ( t2 * z1 - t1 * z2 ) * r );
			tdir.set( ( s1 * x2 - s2 * x1 ) * r,
					  ( s1 * y2 - s2 * y1 ) * r,
					  ( s1 * z2 - s2 * z1 ) * r );

			tan1[ a ].addSelf( sdir );
			tan1[ b ].addSelf( sdir );
			tan1[ c ].addSelf( sdir );

			tan2[ a ].addSelf( tdir );
			tan2[ b ].addSelf( tdir );
			tan2[ c ].addSelf( tdir );

		}

		for ( f = 0, fl = this.faces.length; f < fl; f ++ ) {

			face = this.faces[ f ];
			uv = this.uvs[ f ];

			if ( face instanceof THREE.Face3 ) {

				handleTriangle( this, face.a, face.b, face.c, 0, 1, 2 );

				this.vertices[ face.a ].normal.copy( face.vertexNormals[ 0 ] );
				this.vertices[ face.b ].normal.copy( face.vertexNormals[ 1 ] );
				this.vertices[ face.c ].normal.copy( face.vertexNormals[ 2 ] );


			} else if ( face instanceof THREE.Face4 ) {

				handleTriangle( this, face.a, face.b, face.c, 0, 1, 2 );
				handleTriangle( this, face.a, face.b, face.d, 0, 1, 3 );

				this.vertices[ face.a ].normal.copy( face.vertexNormals[ 0 ] );
				this.vertices[ face.b ].normal.copy( face.vertexNormals[ 1 ] );
				this.vertices[ face.c ].normal.copy( face.vertexNormals[ 2 ] );
				this.vertices[ face.d ].normal.copy( face.vertexNormals[ 3 ] );

			}

		}

		for ( v = 0, vl = this.vertices.length; v < vl; v ++ ) {

			n.copy( this.vertices[ v ].normal );
			t = tan1[ v ];

			// Gram-Schmidt orthogonalize

			tmp.copy( t );
			tmp.subSelf( n.multiplyScalar( n.dot( t ) ) ).normalize();

			// Calculate handedness

			tmp2.cross( this.vertices[ v ].normal, t );
			test = tmp2.dot( tan2[ v ] );
			w = (test < 0.0) ? -1.0 : 1.0;

			this.vertices[ v ].tangent.set( tmp.x, tmp.y, tmp.z, w );

		}

		this.hasTangents = true;

	},

	//计算包围盒，思想很简单，一个正好包含此几何体的长方体盒子，用于碰撞计算等其他近似算法
	computeBoundingBox: function () {

		var vertex;

		if ( this.vertices.length > 0 ) {
			//包围盒的初始化，先用第一个顶点初始化，然后遍历剩余的，如果发现更适合的就更新包围盒
			//直到遍历完毕，就可以得到需要的包围盒
			this.boundingBox = { 'x': [ this.vertices[ 0 ].position.x, this.vertices[ 0 ].position.x ],
			'y': [ this.vertices[ 0 ].position.y, this.vertices[ 0 ].position.y ],
			'z': [ this.vertices[ 0 ].position.z, this.vertices[ 0 ].position.z ] };

			for ( var v = 1, vl = this.vertices.length; v < vl; v ++ ) {

				vertex = this.vertices[ v ];
				//求最小x
				if ( vertex.position.x < this.boundingBox.x[ 0 ] ) {

					this.boundingBox.x[ 0 ] = vertex.position.x;

				} 
				//求最大x
				else if ( vertex.position.x > this.boundingBox.x[ 1 ] ) {

					this.boundingBox.x[ 1 ] = vertex.position.x;

				}
	            //求最大y
				if ( vertex.position.y < this.boundingBox.y[ 0 ] ) {

					this.boundingBox.y[ 0 ] = vertex.position.y;

				} 
				//求最大y
				else if ( vertex.position.y > this.boundingBox.y[ 1 ] ) {

					this.boundingBox.y[ 1 ] = vertex.position.y;

				}
				//求最小z
				if ( vertex.position.z < this.boundingBox.z[ 0 ] ) {

					this.boundingBox.z[ 0 ] = vertex.position.z;

				} 
				//求最大z
				else if ( vertex.position.z > this.boundingBox.z[ 1 ] ) {

					this.boundingBox.z[ 1 ] = vertex.position.z;

				}

			}

		}

	},

	//计算包围球 它和AABB各有各的优点和缺点吧 
	//比如 长方体造型的东西用aabb更精确，球体造型的东西，用球包围框更精确
	//一个值，那就是半径，问题是球心在哪里？
	computeBoundingSphere: function () {

		//判断之前存储的有没有，若没有置为0，若有，先置为之前的。
		var radius = this.boundingSphere === null ? 0 : this.boundingSphere.radius;

		//找到最大的那个距离即可，距离需要两个点才能计算，当前顶点的坐标有了
		//起始点是什么呢？ 作者用的（0,0,0）点这样的话。。就必须以中心来定义几何体了
		for ( var v = 0, vl = this.vertices.length; v < vl; v ++ ) {
			//找到最长的半径，作为包围球的半径
			radius = Math.max( radius, this.vertices[ v ].position.length() );

		}

		this.boundingSphere = { radius: radius };

	},

	//这个算法将来再看，因为我还没阅读materials代码，奇怪的是，这个是geometry类，为什么要对
	//materials进行分类？
	//从算法名称来看，作者要对内部的各个面根据材质进行排序了，
	//排序的意义 我个人来看：是为了性能优化，同样的材质或许就不用切换着色器了来进行渲染了
	sortFacesByMaterial: function () {

		// TODO
		// Should optimize by grouping faces with ColorFill / ColorStroke materials
		// which could then use vertex color attributes instead of each being
		// in its separate VBO

		var i, l, f, fl, face, material, materials, vertices, mhash, ghash, hash_map = {};

		function materialHash( material ) {

			var hash_array = [];

			for ( i = 0, l = material.length; i < l; i++ ) {

				if ( material[ i ] == undefined ) {

					hash_array.push( "undefined" );

				} else {

					hash_array.push( material[ i ].toString() );

				}

			}

			return hash_array.join( '_' );

		}

		for ( f = 0, fl = this.faces.length; f < fl; f++ ) {

			face = this.faces[ f ];
			materials = face.materials;

			mhash = materialHash( materials );

			if ( hash_map[ mhash ] == undefined ) {

				hash_map[ mhash ] = { 'hash': mhash, 'counter': 0 };

			}

			ghash = hash_map[ mhash ].hash + '_' + hash_map[ mhash ].counter;

			if ( this.geometryChunks[ ghash ] == undefined ) {

				this.geometryChunks[ ghash ] = { 'faces': [], 'materials': materials, 'vertices': 0 };

			}

			vertices = face instanceof THREE.Face3 ? 3 : 4;

			if ( this.geometryChunks[ ghash ].vertices + vertices > 65535 ) {

				hash_map[ mhash ].counter += 1;
				ghash = hash_map[ mhash ].hash + '_' + hash_map[ mhash ].counter;

				if ( this.geometryChunks[ ghash ] == undefined ) {

					this.geometryChunks[ ghash ] = { 'faces': [], 'materials': materials, 'vertices': 0 };

				}

			}

			this.geometryChunks[ ghash ].faces.push( f );
			this.geometryChunks[ ghash ].vertices += vertices;

		}

	},

	toString: function () {

		return 'THREE.Geometry ( vertices: ' + this.vertices + ', faces: ' + this.faces + ', uvs: ' + this.uvs + ' )';

	}

};
