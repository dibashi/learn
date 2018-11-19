/**
 * @author mr.doob / http://mrdoob.com/
 * @author kile / http://kile.stravaganza.org/
 */
/**
 * Geometry is a user-friendly alternative to BufferGeometry.
 *  Geometries store attributes (vertex positions, faces, colors, etc.) 
 * using objects like Vector3 or Color that are easier to read and edit,
 *  but less efficient than typed arrays.

Prefer BufferGeometry for large or serious projects.
 */
THREE.Geometry = function () {

	this.vertices = [];
	this.faces = [];
	this.uvs = [];

};

THREE.Geometry.prototype = {

	computeCentroids: function () {

		var f, fl, face;

		for (f = 0, fl = this.faces.length; f < fl; f++) {

			face = this.faces[f];
			face.centroid.set(0, 0, 0);

			if (face instanceof THREE.Face3) {

				face.centroid.addSelf(this.vertices[face.a].position);
				face.centroid.addSelf(this.vertices[face.b].position);
				face.centroid.addSelf(this.vertices[face.c].position);
				face.centroid.divideScalar(3);

			} else if (face instanceof THREE.Face4) {

				face.centroid.addSelf(this.vertices[face.a].position);
				face.centroid.addSelf(this.vertices[face.b].position);
				face.centroid.addSelf(this.vertices[face.c].position);
				face.centroid.addSelf(this.vertices[face.d].position);
				face.centroid.divideScalar(4);

			}

		}

	},

	computeNormals: function (useVertexNormals) {

		var n, nl, v, vl, vertex, f, fl, face, vA, vB, vC, cb = new THREE.Vector3(), ab = new THREE.Vector3();

		for (v = 0, vl = this.vertices.length; v < vl; v++) {

			vertex = this.vertices[v];
			vertex.normal.set(0, 0, 0);

		}

		for (f = 0, fl = this.faces.length; f < fl; f++) {

			face = this.faces[f];

			if (useVertexNormals && face.vertexNormals.length) {

				cb.set(0, 0, 0);

				for (n = 0, nl = face.normal.length; n < nl; n++) {

					cb.addSelf(face.vertexNormals[n]);

				}

				cb.divideScalar(3);

				if (!cb.isZero()) {

					cb.normalize();

				}

				face.normal.copy(cb);

			} else {

				vA = this.vertices[face.a];
				vB = this.vertices[face.b];
				vC = this.vertices[face.c];

				cb.sub(vC.position, vB.position);
				ab.sub(vA.position, vB.position);
				cb.crossSelf(ab);

				if (!cb.isZero()) {

					cb.normalize();

				}

				face.normal.copy(cb);

			}

		}

	},
	//计算AABB框
	/**Ymin <= Y <= Ymax
Zmin <= Z <= Zmax
特别重要的两个顶点为：
Pmin = [Xmin Ymin Zmin]，Pmax = [ Xmax Ymax Zmax].
AABB对物体的方向很敏感，
同一物体的不同方向，AABB也可能不同（由于球体只有一个自由度，所以检测球对物体方向不敏感）。
当物体在场景中移动时，它的AABB也需要随之移动，
当物体发生旋转时，有两种选择：用变换后的物体来重新计算AABB，或者对AABB做和物体同样的变换。
如果物体没有发生扭曲，可以通过“变换后的AABB”重新计算，
因为该方法要比通过“变换后的物体”计算快得多，
因为AABB只有8个顶点。变换AABB得出新的AABB要比变换物体的运算量小，但是也会带来一定的误差，
因为AABB总是与坐标轴平行，不能在旋转物体时简单地旋转AABB，
而是应该在每一帧都重新计算。如果知道每个对象的内容，
这个计算就不算困难，也不会降低游戏的速度。然而，还面临着精度的问题。 */
	computeBoundingBox: function () {
		//如果顶点数大于0才有AABB框
		if (this.vertices.length > 0) {
			//初始化为第一个顶点，然后比较更新即可得出
			this.bbox = {
				'x': [this.vertices[0].position.x, this.vertices[0].position.x],
				'y': [this.vertices[0].position.y, this.vertices[0].position.y],
				'z': [this.vertices[0].position.z, this.vertices[0].position.z]
			};
			//循环遍历 进行比较 然后更新bbox 得出结果
			for (var v = 1, vl = this.vertices.length; v < vl; v++) {

				vertex = this.vertices[v];

				if (vertex.position.x < this.bbox.x[0]) {

					this.bbox.x[0] = vertex.position.x;

				} else if (vertex.position.x > this.bbox.x[1]) {

					this.bbox.x[1] = vertex.position.x;

				}

				if (vertex.position.y < this.bbox.y[0]) {

					this.bbox.y[0] = vertex.position.y;

				} else if (vertex.position.y > this.bbox.y[1]) {

					this.bbox.y[1] = vertex.position.y;

				}

				if (vertex.position.z < this.bbox.z[0]) {

					this.bbox.z[0] = vertex.position.z;

				} else if (vertex.position.z > this.bbox.z[1]) {

					this.bbox.z[1] = vertex.position.z;

				}

			}

		}

	},

	toString: function () {

		return 'THREE.Geometry ( vertices: ' + this.vertices + ', faces: ' + this.faces + ' )';

	}

};
