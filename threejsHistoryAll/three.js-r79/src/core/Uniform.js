/**
 * @author mrdoob / http://mrdoob.com/
 */

 /**
  * var material = new THREE.RawShaderMaterial( {
				vertexShader: vert,
				fragmentShader: frag,
				uniforms: {
					color: new THREE.Uniform( new THREE.Color() ).onUpdate( updateColor )
				}
			} );
			应该是为了自定义着色器而设计的类
  */

THREE.Uniform = function ( value ) {

	if ( typeof value === 'string' ) {

		console.warn( 'THREE.Uniform: Type parameter is no longer needed.' );
		value = arguments[ 1 ];

	}

	this.value = value;

	this.dynamic = false;

};

THREE.Uniform.prototype = {

	constructor: THREE.Uniform,

	onUpdate: function ( callback ) {

		this.dynamic = true;
		this.onUpdateCallback = callback;

		return this;

	}

};
