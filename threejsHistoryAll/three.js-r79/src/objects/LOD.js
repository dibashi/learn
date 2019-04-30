/**
 * @author mikael emtinger / http://gomo.se/
 * @author alteredq / http://alteredqualia.com/
 * @author mrdoob / http://mrdoob.com/
 */
/**
 * 多细节层次 —— 在显示网格时，根据摄像机距离物体的距离，来使用更多或者更少的几何体来对其进行显示。

每一个级别都和一个几何体相关联，且在渲染时，可以根据给定的距离，来在这些级别对应的几何体之间进行切换。 
通常情况下，你会创建多个几何体，比如说三个，一个距离很远（低细节），一个距离适中（中等细节），还有一个距离非常近（高质量）。
 */
//自己用官方示例 webgl_lod.html 在大数据量情况下的测试，用lod竟然性能更差，wtf!!
//如果第一层非常的精细 有必要lod 在这样一个前提下 lod确实提升性能
THREE.LOD = function () {

	THREE.Object3D.call(this);

	this.type = 'LOD';


	Object.defineProperties(this, {

		//本类的核心数据结构
		//每一个层级都是一个对象，具有两个属性，一个Object3D（这层次中将要显示的Object3D）
		// distance：将显示这一细节层级的距离（最为关键的是这个距离的边界，就是>=distance就显示这个级别的Object3D）
		//根据源码可以看到 distance的内部用法

		levels: {
			enumerable: true,
			value: []
		}
	});

};


THREE.LOD.prototype = Object.assign(Object.create(THREE.Object3D.prototype), {

	constructor: THREE.LOD,

	copy: function (source) {

		THREE.Object3D.prototype.copy.call(this, source, false);

		var levels = source.levels;

		for (var i = 0, l = levels.length; i < l; i++) {

			var level = levels[i];

			this.addLevel(level.object.clone(), level.distance);

		}

		return this;

	},

	addLevel: function (object, distance) {

		if (distance === undefined) distance = 0;
		//就是绝对距离，不需要什么正的负的
		distance = Math.abs(distance);

		var levels = this.levels;
		//可以认为这是插入排序的部分代码，很高效，
		//因为他内部是有序的，再放入一个元素的话只需要比较到目标位置，然后插入
		for (var l = 0; l < levels.length; l++) {
			//按距离近-->远排序的
			if (distance < levels[l].distance) {

				break;

			}

		}

		/**arrayObject.splice(index,howmany,item1,.....,itemX)
		 * index	必需。整数，规定添加/删除项目的位置，使用负数可从数组结尾处规定位置。
			howmany	必需。要删除的项目数量。如果设置为 0，则不会删除项目。
			item1, ..., itemX	可选。向数组添加的新项目。
		 */
		levels.splice(l, 0, { distance: distance, object: object });

		this.add(object);

	},

	//获得第一个比distance大的Object3D（网格）的引用。 其实就是显示中的level
	getObjectForDistance: function (distance) {

		var levels = this.levels;

		for (var i = 1, l = levels.length; i < l; i++) {

			//不可能显示i索引的物体 由于排过序了，所以必然显示i-1索引的物体
			if (distance < levels[i].distance) {

				break;

			}

		}

		return levels[i - 1].object;

	},
	//在一条投射出去的Ray（射线）和这个LOD之间获得交互。 Raycaster.intersectObject将会调用这个方法。
	raycast: (function () {

		var matrixPosition = new THREE.Vector3();

		return function raycast(raycaster, intersects) {

			matrixPosition.setFromMatrixPosition(this.matrixWorld);

			var distance = raycaster.ray.origin.distanceTo(matrixPosition);

			this.getObjectForDistance(distance).raycast(raycaster, intersects);

		};

	}()),

	update: function () {

		var v1 = new THREE.Vector3();
		var v2 = new THREE.Vector3();


		return function update(camera) {

			var levels = this.levels;
			//如果有细节层级
			if (levels.length > 1) {
				//获得摄像机的 世界坐标
				v1.setFromMatrixPosition(camera.matrixWorld);
				//获得lod的世界坐标
				v2.setFromMatrixPosition(this.matrixWorld);
				//计算lod距离摄像机的距离
				var distance = v1.distanceTo(v2);
				//先把最高级别置为可见，然后根据距离与“级别距离阀值”进行比较来最终决定显示谁
				//根据源码知道，第一个物体的距离实际上没有用到
				levels[0].object.visible = true;

				for (var i = 1, l = levels.length; i < l; i++) {
					//发现距离 大于当前距离阀值 就把前面显示的物体隐藏到，显示当前的 以此类推，
					if (distance >= levels[i].distance) {

						levels[i - 1].object.visible = false;
						levels[i].object.visible = true;

					}
					//到这里发现 后面的物体要求的距离很大，所以后面的要隐藏掉
					else {

						break;

					}

				}
				//break掉的i 就是当前不用显示的物体，其后面的都要隐藏掉
				for (; i < l; i++) {

					levels[i].object.visible = false;

				}

			}

		};

	}(),

	toJSON: function (meta) {

		var data = THREE.Object3D.prototype.toJSON.call(this, meta);

		data.object.levels = [];

		var levels = this.levels;

		for (var i = 0, l = levels.length; i < l; i++) {

			var level = levels[i];

			data.object.levels.push({
				object: level.object.uuid,
				distance: level.distance
			});

		}

		return data;

	}

});
