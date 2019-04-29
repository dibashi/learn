/**
 * @author mrdoob / http://mrdoob.com/
 */
/**
 * This class stores data for an attribute 
 * (such as vertex positions, face indices, normals, colors, UVs, and any custom attributes ) 
 * associated with a BufferGeometry, 
 * which allows for more efficient passing of data to the GPU.
Data is stored as vectors of any length (defined by itemSize), 
and in general in the methods outlined below if passing in an index, 
this is automatically multiplied by the vector length.
 */

//https://threejs.org/docs/index.html#api/en/core/BufferAttribute
THREE.BufferAttribute = function (array, itemSize, normalized) {

	this.uuid = THREE.Math.generateUUID();
	//逐顶点的数据 这个数组长度 应该是为：itemSize * numVertices ，numVertices是关联的BufferGeometry的顶点数量
	this.array = array;
	//每个数据的长度
	this.itemSize = itemSize;
	// gl.STATIC_DRAW gl.DYNAMIC_DRAW 用于通知gpu数据的可能使用方式，很难看出效果，为了优化
	this.dynamic = false;
	/**
	 * Object containing:
offset: Default is 0. Position at which to start update.
count: Default is -1, which means don't use update ranges. 

This can be used to only update some components of stored vectors (for example,
	 just the component related to color).
	 */
	//数据的更新范围
	/*
	//WebGLObjects.js
function updateBuffer( attributeProperties, data, bufferType ) {

		gl.bindBuffer( bufferType, attributeProperties.__webglBuffer );

		if ( data.dynamic === false || data.updateRange.count === - 1 ) {

			// Not using update ranges

			gl.bufferSubData( bufferType, 0, data.array );

		} else if ( data.updateRange.count === 0 ) {

			console.error( 'THREE.WebGLObjects.updateBuffer: dynamic THREE.BufferAttribute marked as needsUpdate but updateRange.count is 0, ensure you are using set methods or updating manually.' );

		} else {

			gl.bufferSubData( bufferType, data.updateRange.offset * data.array.BYTES_PER_ELEMENT,
							  data.array.subarray( data.updateRange.offset, data.updateRange.offset + data.updateRange.count ) );

			data.updateRange.count = 0; // reset range

		}

		attributeProperties.version = data.version;

	}

	*/
	this.updateRange = { offset: 0, count: - 1 };

	/**
	 * Flag to indicate that this attribute has changed and should be re-sent to the GPU. 
	 * Set this to true when you modify the value of the array. <---needsUpdate

	Setting this to true also increments the version.
	 */
	this.version = 0;
	//是否归一化，印象中 只要颜色数据有必要 归一化的方式 取决于数据的类型
	this.normalized = normalized === true;

};

THREE.BufferAttribute.prototype = {

	constructor: THREE.BufferAttribute,

	/**
	 * var vertices = new Float32Array([
      0, 0, 0, //顶点1坐标
      50, 0, 0, //顶点2坐标
      0, 100, 0, //顶点3坐标
      0, 0, 10, //顶点4坐标
      0, 0, 100, //顶点5坐标
      50, 0, 10, //顶点6坐标
    ]);
    // 创建属性缓冲区对象
    var attribue = new THREE.BufferAttribute(vertices, 3); 
	 */
	//实际上应该和顶点数量一致
	get count() {

		return this.array.length / this.itemSize;

	},

	//.needsUpdate = true 会调用如下set函数，version的值会修改，也就是说 如果想自动更新，可以直接修改version
	set needsUpdate(value) {

		if (value === true) this.version++;

	},

	setDynamic: function (value) {

		this.dynamic = value;

		return this;

	},

	copy: function (source) {

		//这种写法很妙，并不知道source.array的构造函数，很动态 问题在于他们是用的同一份数据？？
		//必须测试
		//应该不是 类型化数组的构造函数 可以传入另一个数组 应该是数据的复制
		this.array = new source.array.constructor(source.array);
		this.itemSize = source.itemSize;

		this.dynamic = source.dynamic;

		return this;

	},

	//Copy a vector from bufferAttribute[index2] to array[index1].
	//它只拷贝了一个向量的数据。。而且是按照自身长度的
	copyAt: function (index1, attribute, index2) {

		index1 *= this.itemSize;
		index2 *= attribute.itemSize;

		for (var i = 0, l = this.itemSize; i < l; i++) {

			this.array[index1 + i] = attribute.array[index2 + i];

		}

		return this;

	},


	copyArray: function (array) {
		//类型化数组的set方法用于复制数组，也就是将一段内容完全复制到另一段内存。
		this.array.set(array);

		return this;

	},

	//为什么要这么特殊的弄这么一个函数？因为不好new？？color = new THREE.Color();和下面的区别
	copyColorsArray: function (colors) {

		var array = this.array, offset = 0;

		for (var i = 0, l = colors.length; i < l; i++) {

			var color = colors[i];

			if (color === undefined) {

				console.warn('THREE.BufferAttribute.copyColorsArray(): color is undefined', i);
				color = new THREE.Color();

			}

			array[offset++] = color.r;
			array[offset++] = color.g;
			array[offset++] = color.b;

		}

		return this;

	},

	//这。。indices必须三个一组吗 画点 和线 怎么办？ 不太清楚 意图
	//貌似最新版three.js已经移除
	copyIndicesArray: function (indices) {

		var array = this.array, offset = 0;

		for (var i = 0, l = indices.length; i < l; i++) {

			var index = indices[i];

			array[offset++] = index.a;
			array[offset++] = index.b;
			array[offset++] = index.c;

		}

		return this;

	},

	//Copy an array representing Vector2s into array.
	copyVector2sArray: function (vectors) {

		var array = this.array, offset = 0;

		for (var i = 0, l = vectors.length; i < l; i++) {

			var vector = vectors[i];

			if (vector === undefined) {

				console.warn('THREE.BufferAttribute.copyVector2sArray(): vector is undefined', i);
				vector = new THREE.Vector2();

			}

			array[offset++] = vector.x;
			array[offset++] = vector.y;

		}

		return this;

	},

	//Copy an array representing Vector3s into array.
	copyVector3sArray: function (vectors) {

		var array = this.array, offset = 0;

		for (var i = 0, l = vectors.length; i < l; i++) {

			var vector = vectors[i];

			//数组内某个元素为undefined?? 
			if (vector === undefined) {

				console.warn('THREE.BufferAttribute.copyVector3sArray(): vector is undefined', i);
				//这样相对健壮，但问题会变的很难找，作者提供了警告
				vector = new THREE.Vector3();

			}

			array[offset++] = vector.x;
			array[offset++] = vector.y;
			array[offset++] = vector.z;

		}

		return this;

	},

	//Copy an array representing Vector4s into array.
	copyVector4sArray: function (vectors) {

		var array = this.array, offset = 0;

		for (var i = 0, l = vectors.length; i < l; i++) {

			var vector = vectors[i];

			if (vector === undefined) {

				console.warn('THREE.BufferAttribute.copyVector4sArray(): vector is undefined', i);
				vector = new THREE.Vector4();

			}

			array[offset++] = vector.x;
			array[offset++] = vector.y;
			array[offset++] = vector.z;
			array[offset++] = vector.w;

		}

		return this;

	},


	//value -->an Array or TypedArray from which to copy values.
	//offset -- (optional) index of the array at which to start copying.
	set: function (value, offset) {

		if (offset === undefined) offset = 0;

		this.array.set(value, offset);

		return this;

	},

	//Returns the x component of the vector at the given index.
	getX: function (index) {

		return this.array[index * this.itemSize];

	},
	//Sets the x component of the vector at the given index.
	setX: function (index, x) {

		this.array[index * this.itemSize] = x;

		return this;

	},

	getY: function (index) {

		return this.array[index * this.itemSize + 1];

	},

	setY: function (index, y) {

		this.array[index * this.itemSize + 1] = y;

		return this;

	},

	getZ: function (index) {

		return this.array[index * this.itemSize + 2];

	},

	setZ: function (index, z) {

		this.array[index * this.itemSize + 2] = z;

		return this;

	},

	getW: function (index) {

		return this.array[index * this.itemSize + 3];

	},

	setW: function (index, w) {

		this.array[index * this.itemSize + 3] = w;

		return this;

	},

	setXY: function (index, x, y) {

		index *= this.itemSize;

		this.array[index + 0] = x;
		this.array[index + 1] = y;

		return this;

	},

	setXYZ: function (index, x, y, z) {

		index *= this.itemSize;

		this.array[index + 0] = x;
		this.array[index + 1] = y;
		this.array[index + 2] = z;

		return this;

	},

	setXYZW: function (index, x, y, z, w) {

		index *= this.itemSize;

		this.array[index + 0] = x;
		this.array[index + 1] = y;
		this.array[index + 2] = z;
		this.array[index + 3] = w;

		return this;

	},
	//以上这些函数真啰嗦啊。。

	clone: function () {

		return new this.constructor().copy(this);

	}

};

//包装类，不用再创建类型化数组 然后实例化BufferAttribute对象，
//只需要 传入数据数组，和itemSize即可生成

THREE.Int8Attribute = function (array, itemSize) {

	return new THREE.BufferAttribute(new Int8Array(array), itemSize);

};

THREE.Uint8Attribute = function (array, itemSize) {

	return new THREE.BufferAttribute(new Uint8Array(array), itemSize);

};
/**
 * var uintc8 = new Uint8ClampedArray(2);
uintc8[0] = 42;
uintc8[1] = 1337;
console.log(uintc8[0]); // 42
console.log(uintc8[1]); // 255 (clamped)
console.log(uintc8.length); // 2
console.log(uintc8.BYTES_PER_ELEMENT); // 1
 */
//https://developer.mozilla.org/zh-CN/docs/Web/JavaScript/Reference/Global_Objects/Uint8ClampedArray
THREE.Uint8ClampedAttribute = function (array, itemSize) {

	return new THREE.BufferAttribute(new Uint8ClampedArray(array), itemSize);

};

THREE.Int16Attribute = function (array, itemSize) {

	return new THREE.BufferAttribute(new Int16Array(array), itemSize);

};

THREE.Uint16Attribute = function (array, itemSize) {

	return new THREE.BufferAttribute(new Uint16Array(array), itemSize);

};

THREE.Int32Attribute = function (array, itemSize) {

	return new THREE.BufferAttribute(new Int32Array(array), itemSize);

};

THREE.Uint32Attribute = function (array, itemSize) {

	return new THREE.BufferAttribute(new Uint32Array(array), itemSize);

};

THREE.Float32Attribute = function (array, itemSize) {

	return new THREE.BufferAttribute(new Float32Array(array), itemSize);

};

THREE.Float64Attribute = function (array, itemSize) {

	return new THREE.BufferAttribute(new Float64Array(array), itemSize);

};


// Deprecated

THREE.DynamicBufferAttribute = function (array, itemSize) {

	console.warn('THREE.DynamicBufferAttribute has been removed. Use new THREE.BufferAttribute().setDynamic( true ) instead.');
	return new THREE.BufferAttribute(array, itemSize).setDynamic(true);

};
