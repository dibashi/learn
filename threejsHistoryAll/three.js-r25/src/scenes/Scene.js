/**
 * @author mr.doob / http://mrdoob.com/
 */
//内部存有两个集合，一个是光源结合，一个是3D对象集合 对这些集合增删改查
//个人觉得 场景也应该是对象 这样可以进行层次模型管理
THREE.Scene = function () {

	this.objects = [];
	this.lights = [];

	this.addObject = function ( object ) {

		this.objects.push(object);

	};

	this.removeObject = function ( object ) {

		for ( var i = 0, l = this.objects.length; i < l; i++ ) {

			if ( object == this.objects[ i ] ) {

				this.objects.splice( i, 1 );
				return;

			}
		}
	};

  this.addLight = function ( light ) {

    this.lights.push(light);

  };

  this.removeLight = function ( light ) {

    for ( var i = 0, l = this.lights.length; i < l; i++ ) {

      if ( light == this.lights[ i ] ) {

        this.lights.splice( i, 1 );
        return;

      }
    }
  };

	// Deprecated
	this.add = function ( object ) {

		this.addObject( object );

	};

	this.toString = function () {

		return 'THREE.Scene ( ' + this.objects + ' )';
	};

};
