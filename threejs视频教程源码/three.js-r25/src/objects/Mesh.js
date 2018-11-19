/**
 * @author mr.doob / http://mrdoob.com/
 */
//mesh里面有 物体的模型矩阵 顶点坐标 uvs 法线 以及材质（颜色）
THREE.Mesh = function ( geometry, material, normUVs ) {
	//获得模型矩阵
	THREE.Object3D.call( this );
	//获得顶点坐标，纹理坐标，法线，面索引（由顶点索引组成）
	this.geometry = geometry;
	//面颜色，是否画出网格线？线颜色，初步理解 todo!!!1
	this.material = material instanceof Array ? material : [ material ];
	//这四个变量未测试 未知
	this.flipSided = false;
	this.doubleSided = false;

	this.overdraw = false;
	
	this.materialFaceGroup = {};
	
	this.sortFacesByMaterial();
	if( normUVs ) this.normalizeUVs();
	//计算AABB框
	this.geometry.computeBoundingBox();

};

THREE.Mesh.prototype = new THREE.Object3D();
THREE.Mesh.prototype.constructor = THREE.Mesh;


/**
 * 平面几何体在绘制的过程中， 由sortFacesByMaterial 函数处理生成几何体组。

首先根据材质对几何体分组，

     材质编号_当前材质几何体组编号  作为几何体组的标识。

    接着将相应的平面块 压入到对应的几何体组中。

   控制每个几何体组的定点个数 小于 65535.
 */
THREE.Mesh.prototype.sortFacesByMaterial = function () {

	// TODO
	// Should optimize by grouping faces with ColorFill / ColorStroke materials
	// which could then use vertex color attributes instead of each being
	// in its separate VBO

	var i, l, f, fl, face, material, hash_array;

	function materialHash( material ) {

		hash_array = [];

		for ( i = 0, l = material.length; i < l; i++ ) {

			if ( material[ i ] == undefined ) {

				hash_array.push( "undefined" );

			} else {

				hash_array.push( material[ i ].toString() );

			}

		}

		return hash_array.join("_");

	}

	for ( f = 0, fl = this.geometry.faces.length; f < fl; f++ ) {

		face = this.geometry.faces[ f ];
		material = face.material;

		hash = materialHash( material);

		if ( this.materialFaceGroup[ hash ] == undefined ) {

			this.materialFaceGroup[ hash ] = { 'faces': [], 'material': material };

		}

		this.materialFaceGroup[ hash ].faces.push( f );

	}

}

THREE.Mesh.prototype.normalizeUVs = function () {

	var i, il, j, jl, uvs;

	for ( i = 0, il = this.geometry.uvs.length; i < il; i++ ) {

		uvs = this.geometry.uvs[ i ];

		for ( j = 0, jl = uvs.length; j < jl; j++ ) {

			// texture repeat
			// (WebGL does this by default but canvas renderer needs to do it explicitly)

			if( uvs[ j ].u != 1.0 ) uvs[ j ].u = uvs[ j ].u - Math.floor( uvs[ j ].u );
			if( uvs[ j ].v != 1.0 ) uvs[ j ].v = uvs[ j ].v - Math.floor( uvs[ j ].v );

		}

	}

}
