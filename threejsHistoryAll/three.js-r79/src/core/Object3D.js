/**
 * @author mrdoob / http://mrdoob.com/
 * @author mikael emtinger / http://gomo.se/
 * @author alteredq / http://alteredqualia.com/
 * @author WestLangley / http://github.com/WestLangley
 * @author elephantatwork / www.elephantatwork.ch
 */

 //https://threejs.org/docs/index.html#manual/en/introduction/Matrix-transformations
 //three.js内部核心矩阵变换 综述
THREE.Object3D = function () {
	/**
	 * 该方法允许精确添加或修改对象的属性。
	 * 通过赋值操作添加的普通属性是可枚举的，
	 * 能够在属性枚举期间呈现出来（for...in 或 Object.keys 方法）， 
	 * 这些属性的值可以被改变，也可以被删除。这个方法允许修改默认的额外选项（或配置）。
	 * 默认情况下，使用 Object.defineProperty() 添加的属性值是不可修改的。
	 */
	/**
	 * // 在对象中添加一个属性与存取描述符的示例
var bValue;
Object.defineProperty(o, "b", {
  get : function(){
    return bValue;
  },
  set : function(newValue){
    bValue = newValue;
  },
  enumerable : true,
  configurable : true
});

o.b = 38;
// 对象o拥有了属性b，值为38

// o.b的值现在总是与bValue相同，除非重新定义o.b
	 * 
	 */
	//mdn对该方法的详细解释
	//https://developer.mozilla.org/zh-CN/docs/Web/JavaScript/Reference/Global_Objects/Object/defineProperty
	
	Object.defineProperty(this, 'id', { value: THREE.Object3DIdCount++ });

	this.uuid = THREE.Math.generateUUID();

	//字符串标示，可根据名字查询到对象
	this.name = '';
	//标志着这是什么对象
	this.type = 'Object3D';
	//父节点 这是渲染的场景图。 图这个词很奇怪，为什么不叫场景树？现在 three.js内部是维护了一个树状的对象结构
	this.parent = null;
	//子节点
	this.children = [];
	//对象的默认上方向，似乎只有摄像机需要？ 为什么不定义在摄像机中？ 以后再详细研究
	this.up = THREE.Object3D.DefaultUp.clone();

	//这自然是一个对象在3维空间的核心数据，无需解释
	var position = new THREE.Vector3();
	var rotation = new THREE.Euler();
	var quaternion = new THREE.Quaternion();
	var scale = new THREE.Vector3(1, 1, 1);

	function onRotationChange() {

		quaternion.setFromEuler(rotation, false);

	}

	function onQuaternionChange() {

		rotation.setFromQuaternion(quaternion, undefined, false);

	}

	//现在想想，确实要用Object.defineProperties来定义属性，这样这个对象被修改时可以保证四元数和欧拉角的统一
	//否则 被赋予新的向量 事件是无法触发的，内部数据紊乱，bug极其难调
	rotation.onChange(onRotationChange);
	quaternion.onChange(onQuaternionChange);

	/**
	 * 	// mesh.position = new THREE.Vector3(100,0,0);//这是错误的用法因为这个属性定义时writable为fasle
		// console.log(mesh.position);//并没有改变
		// mesh.position.x = 100;//只能这样
	 */
	Object.defineProperties(this, {
		position: {
			enumerable: true,
			value: position
		},
		rotation: {
			enumerable: true,
			value: rotation
		},
		quaternion: {
			enumerable: true,
			value: quaternion
		},
		scale: {
			enumerable: true,
			value: scale
		},
		//renderer中以下两个矩阵的更新
		/**
		 * object.modelViewMatrix.multiplyMatrices( camera.matrixWorldInverse, object.matrixWorld );
			object.normalMatrix.getNormalMatrix( object.modelViewMatrix );
		 */
		modelViewMatrix: {
			value: new THREE.Matrix4()
		},
		/**
		 * object.normalMatrix.getNormalMatrix( object.modelViewMatrix );
		 */
		//注意！！ 这是一个3*3的矩阵  平移部分舍去了，只是旋转 缩放矩阵的 逆转置矩阵
		//这个推导 比较简单，核心思路就是 转换后的法线向量要与 转换后的之前垂直的向量依然垂直
		//即可推出公式
		/**
		 * getNormalMatrix: function ( matrix4 ) {

		return this.setFromMatrix4( matrix4 ).getInverse( this ).transpose();

	},
		 */
		normalMatrix: {
			value: new THREE.Matrix3()
		}
	});

	//存储自身的平移旋转 缩放
	this.matrix = new THREE.Matrix4();
	//乘以父节点的平移旋转缩放后 得到的世界矩阵，这两个矩阵一定要同步，matrix改变了，matrixWorld就要改变，
	//问题还没这么简单，matrixWorld改变了，他的子节点的matrixWorld也要改变
	//如果由系统每帧的自动刷新所有，性能上不太好，所以内部加入了很多变量来标记是否需要更新矩阵
	this.matrixWorld = new THREE.Matrix4();

	this.matrixAutoUpdate = THREE.Object3D.DefaultMatrixAutoUpdate;

	//脏模式 需要标记的变量
	this.matrixWorldNeedsUpdate = false;

	//用于给对象分层，摄像机可以专注拍摄某层的对象，作者的实现方式不错，用极小的数据量和高效的位操作解决了原本复杂的问题
	//要是我就会用字符串。。摄像机存储一个层级的标示数组，来比较这个对象是否在该层级。数据量和性能不可同日而语
	this.layers = new THREE.Layers();
	//对象是否可见，若不可见 renderer直接不渲染了
	this.visible = true;

	//物体是否投射阴影，物体是否接收阴影
	//根本原因在于这是一个局部光照，需要手动模拟。以后会详细注释
	this.castShadow = false;
	this.receiveShadow = false;

	/**
	 * When this is set, 
	 * it checks every frame if the object is in the frustum of the camera. 
	 * Otherwise the object gets drawn every frame even if it isn't visible.
	 */
	//为了性能，物体剔除算法的开关
	this.frustumCulled = true;
	
	this.renderOrder = 0;

	this.userData = {};

};

//在函数上，定义的属性。很少用到这种方式，觉得很奇怪（之前面向对象语言造成的思维限制）
THREE.Object3D.DefaultUp = new THREE.Vector3(0, 1, 0);
THREE.Object3D.DefaultMatrixAutoUpdate = true;

Object.assign(THREE.Object3D.prototype, THREE.EventDispatcher.prototype, {

	applyMatrix: function (matrix) {

		this.matrix.multiplyMatrices(matrix, this.matrix);

		this.matrix.decompose(this.position, this.quaternion, this.scale);

	},

	setRotationFromAxisAngle: function (axis, angle) {

		// assumes axis is normalized

		this.quaternion.setFromAxisAngle(axis, angle);

	},

	setRotationFromEuler: function (euler) {

		this.quaternion.setFromEuler(euler, true);

	},

	setRotationFromMatrix: function (m) {

		// assumes the upper 3x3 of m is a pure rotation matrix (i.e, unscaled)

		this.quaternion.setFromRotationMatrix(m);

	},

	setRotationFromQuaternion: function (q) {

		// assumes q is normalized

		this.quaternion.copy(q);

	},

	rotateOnAxis: function () {

		// rotate object on axis in object space
		// axis is assumed to be normalized

		var q1 = new THREE.Quaternion();

		return function rotateOnAxis(axis, angle) {

			q1.setFromAxisAngle(axis, angle);

			this.quaternion.multiply(q1);

			return this;

		};

	}(),

	rotateX: function () {

		var v1 = new THREE.Vector3(1, 0, 0);

		return function rotateX(angle) {

			return this.rotateOnAxis(v1, angle);

		};

	}(),

	rotateY: function () {

		var v1 = new THREE.Vector3(0, 1, 0);

		return function rotateY(angle) {

			return this.rotateOnAxis(v1, angle);

		};

	}(),

	rotateZ: function () {

		var v1 = new THREE.Vector3(0, 0, 1);

		return function rotateZ(angle) {

			return this.rotateOnAxis(v1, angle);

		};

	}(),

	translateOnAxis: function () {

		// translate object by distance along axis in object space
		// axis is assumed to be normalized

		var v1 = new THREE.Vector3();

		return function translateOnAxis(axis, distance) {

			v1.copy(axis).applyQuaternion(this.quaternion);

			this.position.add(v1.multiplyScalar(distance));

			return this;

		};

	}(),

	translateX: function () {

		var v1 = new THREE.Vector3(1, 0, 0);

		return function translateX(distance) {

			return this.translateOnAxis(v1, distance);

		};

	}(),

	translateY: function () {

		var v1 = new THREE.Vector3(0, 1, 0);

		return function translateY(distance) {

			return this.translateOnAxis(v1, distance);

		};

	}(),

	translateZ: function () {

		var v1 = new THREE.Vector3(0, 0, 1);

		return function translateZ(distance) {

			return this.translateOnAxis(v1, distance);

		};

	}(),

	localToWorld: function (vector) {

		return vector.applyMatrix4(this.matrixWorld);

	},

	worldToLocal: function () {

		var m1 = new THREE.Matrix4();

		return function worldToLocal(vector) {

			return vector.applyMatrix4(m1.getInverse(this.matrixWorld));

		};

	}(),

	//在最新版104three.js源码中
	/**
	 * camera与object3d的 lookAt整合了
	 * 
	 * 
			if ( this.isCamera || this.isLight ) {

				m1.lookAt( position, target, this.up );

			} else {

				m1.lookAt( target, position, this.up );

			}
	 */

	 //这个则是专注于对象自身的旋转
	lookAt: function () {

		// This routine does not support objects with rotated and/or translated parent(s)

		var m1 = new THREE.Matrix4();

		return function lookAt(vector) {

			m1.lookAt(vector, this.position, this.up);

			this.quaternion.setFromRotationMatrix(m1);

		};

	}(),

	add: function (object) {

		if (arguments.length > 1) {

			for (var i = 0; i < arguments.length; i++) {

				this.add(arguments[i]);

			}

			return this;

		}

		if (object === this) {

			console.error("THREE.Object3D.add: object can't be added as a child of itself.", object);
			return this;

		}

		if (object instanceof THREE.Object3D) {

			if (object.parent !== null) {

				object.parent.remove(object);

			}

			object.parent = this;
			object.dispatchEvent({ type: 'added' });

			this.children.push(object);

		} else {

			console.error("THREE.Object3D.add: object not an instance of THREE.Object3D.", object);

		}

		return this;

	},

	remove: function (object) {

		if (arguments.length > 1) {

			for (var i = 0; i < arguments.length; i++) {

				this.remove(arguments[i]);

			}

		}

		var index = this.children.indexOf(object);

		if (index !== - 1) {

			object.parent = null;

			object.dispatchEvent({ type: 'removed' });

			this.children.splice(index, 1);

		}

	},

	getObjectById: function (id) {

		return this.getObjectByProperty('id', id);

	},

	getObjectByName: function (name) {

		return this.getObjectByProperty('name', name);

	},

	getObjectByProperty: function (name, value) {

		if (this[name] === value) return this;

		for (var i = 0, l = this.children.length; i < l; i++) {

			var child = this.children[i];
			var object = child.getObjectByProperty(name, value);

			if (object !== undefined) {

				return object;

			}

		}

		return undefined;

	},

	getWorldPosition: function (optionalTarget) {

		var result = optionalTarget || new THREE.Vector3();

		this.updateMatrixWorld(true);

		return result.setFromMatrixPosition(this.matrixWorld);

	},

	getWorldQuaternion: function () {

		var position = new THREE.Vector3();
		var scale = new THREE.Vector3();

		return function getWorldQuaternion(optionalTarget) {

			var result = optionalTarget || new THREE.Quaternion();

			this.updateMatrixWorld(true);

			this.matrixWorld.decompose(position, result, scale);

			return result;

		};

	}(),

	getWorldRotation: function () {

		var quaternion = new THREE.Quaternion();

		return function getWorldRotation(optionalTarget) {

			var result = optionalTarget || new THREE.Euler();

			this.getWorldQuaternion(quaternion);

			return result.setFromQuaternion(quaternion, this.rotation.order, false);

		};

	}(),

	getWorldScale: function () {

		var position = new THREE.Vector3();
		var quaternion = new THREE.Quaternion();

		return function getWorldScale(optionalTarget) {

			var result = optionalTarget || new THREE.Vector3();

			this.updateMatrixWorld(true);

			this.matrixWorld.decompose(position, quaternion, result);

			return result;

		};

	}(),

	getWorldDirection: function () {

		var quaternion = new THREE.Quaternion();

		return function getWorldDirection(optionalTarget) {

			var result = optionalTarget || new THREE.Vector3();

			this.getWorldQuaternion(quaternion);

			return result.set(0, 0, 1).applyQuaternion(quaternion);

		};

	}(),

	raycast: function () { },

	traverse: function (callback) {

		callback(this);

		var children = this.children;

		for (var i = 0, l = children.length; i < l; i++) {

			children[i].traverse(callback);

		}

	},

	traverseVisible: function (callback) {

		if (this.visible === false) return;

		callback(this);

		var children = this.children;

		for (var i = 0, l = children.length; i < l; i++) {

			children[i].traverseVisible(callback);

		}

	},

	traverseAncestors: function (callback) {

		var parent = this.parent;

		if (parent !== null) {

			callback(parent);

			parent.traverseAncestors(callback);

		}

	},

	updateMatrix: function () {

		this.matrix.compose(this.position, this.quaternion, this.scale);

		//当前的对象的模型矩阵一旦更改。必须更改其世界矩阵，其他地方无法设置为true
		this.matrixWorldNeedsUpdate = true;

	},

	updateMatrixWorld: function (force) {

		if (this.matrixAutoUpdate === true) this.updateMatrix();

		if (this.matrixWorldNeedsUpdate === true || force === true) {

			if (this.parent === null) {

				this.matrixWorld.copy(this.matrix);

			} else {

				this.matrixWorld.multiplyMatrices(this.parent.matrixWorld, this.matrix);

			}
			//世界矩阵更新过了，就关掉。下次模型矩阵再改 才修改，只有这里设置为false
			this.matrixWorldNeedsUpdate = false;
			//父的世界矩阵已经修改，要强制修改其所有子节点的世界矩阵
			force = true;

		}

		// update children

		for (var i = 0, l = this.children.length; i < l; i++) {

			this.children[i].updateMatrixWorld(force);

		}

	},

	toJSON: function (meta) {

		// meta is '' when called from JSON.stringify
		var isRootObject = (meta === undefined || meta === '');

		var output = {};

		// meta is a hash used to collect geometries, materials.
		// not providing it implies that this is the root object
		// being serialized.
		if (isRootObject) {

			// initialize meta obj
			meta = {
				geometries: {},
				materials: {},
				textures: {},
				images: {}
			};

			output.metadata = {
				version: 4.4,
				type: 'Object',
				generator: 'Object3D.toJSON'
			};

		}

		// standard Object3D serialization

		var object = {};

		object.uuid = this.uuid;
		object.type = this.type;

		if (this.name !== '') object.name = this.name;
		if (JSON.stringify(this.userData) !== '{}') object.userData = this.userData;
		if (this.castShadow === true) object.castShadow = true;
		if (this.receiveShadow === true) object.receiveShadow = true;
		if (this.visible === false) object.visible = false;

		object.matrix = this.matrix.toArray();

		//

		if (this.geometry !== undefined) {

			if (meta.geometries[this.geometry.uuid] === undefined) {

				meta.geometries[this.geometry.uuid] = this.geometry.toJSON(meta);

			}

			object.geometry = this.geometry.uuid;

		}

		if (this.material !== undefined) {

			if (meta.materials[this.material.uuid] === undefined) {

				meta.materials[this.material.uuid] = this.material.toJSON(meta);

			}

			object.material = this.material.uuid;

		}

		//

		if (this.children.length > 0) {

			object.children = [];

			for (var i = 0; i < this.children.length; i++) {

				object.children.push(this.children[i].toJSON(meta).object);

			}

		}

		if (isRootObject) {

			var geometries = extractFromCache(meta.geometries);
			var materials = extractFromCache(meta.materials);
			var textures = extractFromCache(meta.textures);
			var images = extractFromCache(meta.images);

			if (geometries.length > 0) output.geometries = geometries;
			if (materials.length > 0) output.materials = materials;
			if (textures.length > 0) output.textures = textures;
			if (images.length > 0) output.images = images;

		}

		output.object = object;

		return output;

		// extract data from the cache hash
		// remove metadata on each item
		// and return as array
		function extractFromCache(cache) {

			var values = [];
			for (var key in cache) {

				var data = cache[key];
				delete data.metadata;
				values.push(data);

			}
			return values;

		}

	},

	clone: function (recursive) {

		return new this.constructor().copy(this, recursive);

	},

	copy: function (source, recursive) {

		if (recursive === undefined) recursive = true;

		this.name = source.name;

		this.up.copy(source.up);

		this.position.copy(source.position);
		this.quaternion.copy(source.quaternion);
		this.scale.copy(source.scale);

		this.matrix.copy(source.matrix);
		this.matrixWorld.copy(source.matrixWorld);

		this.matrixAutoUpdate = source.matrixAutoUpdate;
		this.matrixWorldNeedsUpdate = source.matrixWorldNeedsUpdate;

		this.visible = source.visible;

		this.castShadow = source.castShadow;
		this.receiveShadow = source.receiveShadow;

		this.frustumCulled = source.frustumCulled;
		this.renderOrder = source.renderOrder;

		this.userData = JSON.parse(JSON.stringify(source.userData));

		if (recursive === true) {

			for (var i = 0; i < source.children.length; i++) {

				var child = source.children[i];
				this.add(child.clone());

			}

		}

		return this;

	}

});

THREE.Object3DIdCount = 0;
