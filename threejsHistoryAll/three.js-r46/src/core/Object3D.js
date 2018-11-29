/**
 * @author mr.doob / http://mrdoob.com/
 * @author mikael emtinger / http://gomo.se/
 * @author alteredq / http://alteredqualia.com/
 */
//总之定义内部的很多数据，目前不知道都有什么用
THREE.Object3D = function () {
	//取个名
	this.name = '';
	//唯一id 有用？
	this.id = THREE.Object3DCount ++;
	//作者准备用场景图来管理对象了
	this.parent = undefined;
	this.children = [];
	//这个 难道不是摄像机才有？
	this.up = new THREE.Vector3( 0, 1, 0 );

	this.position = new THREE.Vector3();
	this.rotation = new THREE.Vector3();
	//通常都是zyx
	this.eulerOrder = 'XYZ';
	this.scale = new THREE.Vector3( 1, 1, 1 );

	this.dynamic = false; // when true it retains arrays so they can be updated with __dirty*

	this.doubleSided = false;
	this.flipSided = false;

	this.renderDepth = null;

	this.rotationAutoUpdate = true;
	//三个初始化为单位矩阵
	this.matrix = new THREE.Matrix4();
	this.matrixWorld = new THREE.Matrix4();
	//这个还没用到 似乎
	this.matrixRotationWorld = new THREE.Matrix4();

	this.matrixAutoUpdate = true;
	this.matrixWorldNeedsUpdate = true;

	this.quaternion = new THREE.Quaternion();
	this.useQuaternion = false;

	this.boundRadius = 0.0;
	this.boundRadiusScale = 1.0;

	this.visible = true;

	this.castShadow = false;
	this.receiveShadow = false;

	this.frustumCulled = true;

	this._vector = new THREE.Vector3();

};


THREE.Object3D.prototype = {

	constructor: THREE.Object3D,

	//似乎是沿着某个轴 移动多少距离
	translate: function ( distance, axis ) {

		this.matrix.rotateAxis( axis );
		this.position.addSelf( axis.multiplyScalar( distance ) );

	},

	translateX: function ( distance ) {

		this.translate( distance, this._vector.set( 1, 0, 0 ) );

	},

	translateY: function ( distance ) {

		this.translate( distance, this._vector.set( 0, 1, 0 ) );

	},

	translateZ: function ( distance ) {

		this.translate( distance, this._vector.set( 0, 0, 1 ) );

	},

	lookAt: function ( vector ) {

		// TODO: Add hierarchy support.

		this.matrix.lookAt( vector, this.position, this.up );

		if ( this.rotationAutoUpdate ) {

			this.rotation.setRotationFromMatrix( this.matrix );

		}

	},

	add: function ( object ) {

		if ( this.children.indexOf( object ) === - 1 ) { //保证没有这个孩子 才加入

			if ( object.parent !== undefined ) { //这个带加入的孩子有父亲吗？若有 需要先去掉，这种api设计不好 webglfoundation中有说

				object.parent.remove( object );

			}
			//这两步 让父与子有了联系
			object.parent = this;
			this.children.push( object );

			// add to scene
			//往场景中加
			var scene = this;
			//这里可以看到，作者把场景当做跟节点来使用了，当一个节点没有父节点时，它就是场景，就是根
			while ( scene.parent !== undefined ) {

				scene = scene.parent;

			}
			//这里又把它加入到了场景中，这个逻辑有点混乱了，作者到底要做什么，
			//场景中 加入对象的时候已经加入了对象的子对象
			//如果要加入一个对象 就加入到他的父节点下，但是场景在加入他父节点的时候 已经遍历过其所有的子节点了
			//新加入的节点并不会加入到场景的对象集合之中，所以这里要自己来加入
			//经过分析 还是比价严谨的
			if ( scene !== undefined && scene instanceof THREE.Scene )  {

				scene.addObject( object );

			}

		}

	},

	//remove的时刻 可以想象，要删除其children 要删除其parent 要删除对象本身
	//！！！但是场景中还是有其指针的 要从场景删除，必然涉及到递归删除其子对象

	//到这里 代码都看明白了，其内部有两个数据结构
	//1 子与父的关系 2, 场景有所有对象的集合 场景是个线性的集合
	//父与子是个多叉树结构
	remove: function ( object ) {

		var index = this.children.indexOf( object ); //先查找是否有这个子节点

		if ( index !== - 1 ) {  //有才删

			object.parent = undefined; //去除父节点关系
			this.children.splice( index, 1 );//去除子节点关系

			// remove from scene

			var scene = this;

			while ( scene.parent !== undefined ) {//找到场景

				scene = scene.parent;

			}

			//关键在于 这里的判断是必须的吗？ 我很疑惑，如果不加呢？为什么必须加！？
			//理解这一行 是揭开谜底的关键所在
			if ( scene !== undefined && scene instanceof THREE.Scene ) {

				scene.removeObject( object );//从场景中喀嚓掉 那么在下次渲染中就不会看到了

			}

		}

	},

	//是时候用到name属性了 逻辑还是较为复杂的 关键在于 我很难验证这个程序的正确性，递归程序实在太难验证了
	getChildByName: function ( name, doRecurse ) {

		var c, cl, child, recurseResult;
		//遍历所有子节点
		for ( c = 0, cl = this.children.length; c < cl; c ++ ) {
			//取出
			child = this.children[ c ];
			//name是否一样
			if ( child.name === name ) {
				//一样就返回
				return child;

			}
			//到这里 说明一个问题 上面并没有找到这个name一样的子节点
			//那么又有一个新问题，是否查找子子节点？
			//若是递归 就先停止当前的遍历 先递归下去 若没有 即返回 然后循环继续
			if ( doRecurse ) {

				recurseResult = child.getChildByName( name, doRecurse );

				if ( recurseResult !== undefined ) {

					return recurseResult;

				}

			}

		}

		return undefined;

	},

	//先把自己的模型矩阵给更新了 
	//这次作者比r25版本的性能高了不少，程序变得复杂了 以前很粗鲁我嫌弃性能低
	//现在 性能不错，我又很难受。
	updateMatrix: function () {
		
		this.matrix.setPosition( this.position );

		if ( this.useQuaternion )  {

			this.matrix.setRotationFromQuaternion( this.quaternion );

		} else {

			this.matrix.setRotationFromEuler( this.rotation, this.eulerOrder );

		}

		if ( this.scale.x !== 1 || this.scale.y !== 1 || this.scale.z !== 1 ) {

			this.matrix.scale( this.scale );
			this.boundRadiusScale = Math.max( this.scale.x, Math.max( this.scale.y, this.scale.z ) );

		}

		this.matrixWorldNeedsUpdate = true;

	},

	//更新世界矩阵
	updateMatrixWorld: function ( force ) {
		//有了这些 可自动更新矩阵
		this.matrixAutoUpdate && this.updateMatrix();

		// update matrixWorld

		if ( this.matrixWorldNeedsUpdate || force ) {
			
			if ( this.parent ) {
				//自身的矩阵 还要受到父节点的影响 层次模型 可参阅图形学方面的书 网上也有相关资料
				//还有一个隐藏的信息，就是必须先更新父节点的矩阵  对于其父节点也是同理 是个矩阵级联
				this.matrixWorld.multiply( this.parent.matrixWorld, this.matrix );

			} else {
				//本身是根节点 就是不需要父节点的矩阵来施加影响了
				//但问题是 场景没有父子节点的概念 也就意味着 我突然觉得之前的想法有问题 先不管了
				this.matrixWorld.copy( this.matrix );

			}

			this.matrixWorldNeedsUpdate = false;

			force = true;

		}

		// update children

		for ( var i = 0, l = this.children.length; i < l; i ++ ) {
			//还要更新其子节点 父节点矩阵已经更新过了 所以这种顺序没问题 自顶向下的更新
			this.children[ i ].updateMatrixWorld( force );

		}

	}

};

// 每个3d对象的 Object3DCount 都不一样 一直在递增 目前不知道要这个干什么
THREE.Object3DCount = 0;
