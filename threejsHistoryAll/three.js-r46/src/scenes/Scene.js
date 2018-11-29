/**
 * @author mr.doob / http://mrdoob.com/
 */

THREE.Scene = function () {
	//场景也是一个Object3D对象
	THREE.Object3D.call( this );

	this.fog = null;
	this.overrideMaterial = null;

	this.matrixAutoUpdate = false;
	//内部维护所有对象 和光
	this.objects = [];
	this.lights = [];
	//难道是为了性能作者维护了这两个集合？目前来看 他们只会让性能变低，因为添加和删除的算法中总是要对他们进行维护
	//然而没有看到他们的实际用处
	this.__objectsAdded = [];
	this.__objectsRemoved = [];

};

THREE.Scene.prototype = new THREE.Object3D();
THREE.Scene.prototype.constructor = THREE.Scene;

//统一的api 既可以加入光对象 也可以加入 正常对象
THREE.Scene.prototype.addObject = function ( object ) {

	if ( object instanceof THREE.Light ) {

		if ( this.lights.indexOf( object ) === - 1 ) {

			this.lights.push( object );

		}

	} 
	//加入的对象根据上面 已经不是光 现在要求 不是摄像机 和Three.Bone(还未研究)
	else if ( !( object instanceof THREE.Camera || object instanceof THREE.Bone ) ) {

		if ( this.objects.indexOf( object ) === - 1 ) { //没有改对象 才加入

			this.objects.push( object ); //加入了
			this.__objectsAdded.push( object ); //为什么要备份一份？

			// check if previously removed

			var i = this.__objectsRemoved.indexOf( object ); //检查这个对象 是不是存在删除的对象数组中，若存在，由于它现在又被加入了，所以要从那个数组中删除

			if ( i !== -1 ) {

				this.__objectsRemoved.splice( i, 1 );

			}

		}

	}

	//这是一个递归，他的所有子对象都要加入这个场景中 子对象的子对象也要加入。。。。
	for ( var c = 0; c < object.children.length; c ++ ) {

		this.addObject( object.children[ c ] );

	}

};

//从场景中删除对象
THREE.Scene.prototype.removeObject = function ( object ) {

	if ( object instanceof THREE.Light ) { //若是光

		var i = this.lights.indexOf( object );//从光集合中查找

		if ( i !== -1 ) {

			this.lights.splice( i, 1 ); //找到，然后删掉

		}

	} else if ( !( object instanceof THREE.Camera ) ) { //非光 非摄像机，正常3d对象 进行删除

		var i = this.objects.indexOf( object ); //查找是否有

		if( i !== -1 ) { //有

			this.objects.splice( i, 1 ); //删掉
			this.__objectsRemoved.push( object ); //加入到删除的对象集合

			// check if previously added

			var ai = this.__objectsAdded.indexOf( object ); //看是否加入过

			if ( ai !== -1 ) {

				this.__objectsAdded.splice( ai, 1 ); //若加入过，就删掉

			}

		}

	}

	//递归删除子对象
	for ( var c = 0; c < object.children.length; c ++ ) {

		this.removeObject( object.children[ c ] );

	}

};
