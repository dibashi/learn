/*

一个包含当前渲染环境(RenderingContext)的功能细节的对象。
- floatFragmentTextures: 环境是否支持OES_texture_float扩展。 根据WebGLStats, 截至2016年2月，超过95％的支持WebGL的设备支持此功能
- floatVertexTextures: 如果floatFragmentTextures和vertexTextures都是true， 则此值为true 
- getMaxAnisotropy(): 返回最大可用各向异性。
- getMaxPrecision(): 返回顶点着色器和片元着色器的最大可用精度。 
- logarithmicDepthBuffer: 如果logarithmicDepthBuffer在构造器中被设为true且 环境支持EXT_frag_depth扩展，则此值为true 根据WebGLStats, 截至2016年2月， 约66%的支持WebGL的设备支持此功能
- maxAttributes: gl.MAX_VERTEX_ATTRIBS的值
- maxCubemapSize: gl.MAX_CUBE_MAP_TEXTURE_SIZE 的值，着色器可使用的立方体贴图纹理的最大宽度*高度
- maxFragmentUniforms: gl.MAX_FRAGMENT_UNIFORM_VECTORS的值，片元着色器可使用的全局变量(uniforms)数量
- maxTextureSize: gl.MAX_TEXTURE_SIZE的值，着色器可使用纹理的最大宽度*高度
- maxTextures: *gl.MAX_TEXTURE_IMAGE_UNITS的值，着色器可使用的纹理数量
- maxVaryings: gl.MAX_VARYING_VECTORS的值，着色器可使用矢量的数量
- maxVertexTextures: gl.MAX_VERTEX_TEXTURE_IMAGE_UNITS的值，顶点着色器可使用的纹理数量。
- maxVertexUniforms: gl.MAX_VERTEX_UNIFORM_VECTORS的值，顶点着色器可使用的全局变量(uniforms)数量
- precision: 渲染器当前使用的着色器的精度
- vertexTextures: 如果# .maxVertexTextures : Integer大于0，此值为true (即可以使用顶点纹理)


*/

THREE.WebGLCapabilities = function ( gl, extensions, parameters ) {

	var maxAnisotropy;

	//在WebGLTextures里有使用
	function getMaxAnisotropy() {

		if ( maxAnisotropy !== undefined ) return maxAnisotropy;
		//http://www.cnblogs.com/kylegui/p/3855710.html非常好的科普文章
		var extension = extensions.get( 'EXT_texture_filter_anisotropic' );

		if ( extension !== null ) {

			maxAnisotropy = gl.getParameter( extension.MAX_TEXTURE_MAX_ANISOTROPY_EXT );

		} else {

			maxAnisotropy = 0;

		}

		return maxAnisotropy;

	}

	function getMaxPrecision( precision ) {

		if ( precision === 'highp' ) {

			if ( gl.getShaderPrecisionFormat( gl.VERTEX_SHADER, gl.HIGH_FLOAT ).precision > 0 &&
			     gl.getShaderPrecisionFormat( gl.FRAGMENT_SHADER, gl.HIGH_FLOAT ).precision > 0 ) {

				return 'highp';

			}

			precision = 'mediump';

		}

		if ( precision === 'mediump' ) {

			if ( gl.getShaderPrecisionFormat( gl.VERTEX_SHADER, gl.MEDIUM_FLOAT ).precision > 0 &&
			     gl.getShaderPrecisionFormat( gl.FRAGMENT_SHADER, gl.MEDIUM_FLOAT ).precision > 0 ) {

				return 'mediump';

			}

		}

		return 'lowp';

	}

	this.getMaxAnisotropy = getMaxAnisotropy;
	this.getMaxPrecision = getMaxPrecision;

	this.precision = parameters.precision !== undefined ? parameters.precision : 'highp';
	this.logarithmicDepthBuffer = parameters.logarithmicDepthBuffer !== undefined ? parameters.logarithmicDepthBuffer : false;

	this.maxTextures = gl.getParameter( gl.MAX_TEXTURE_IMAGE_UNITS );
	this.maxVertexTextures = gl.getParameter( gl.MAX_VERTEX_TEXTURE_IMAGE_UNITS );
	this.maxTextureSize = gl.getParameter( gl.MAX_TEXTURE_SIZE );
	this.maxCubemapSize = gl.getParameter( gl.MAX_CUBE_MAP_TEXTURE_SIZE );

	this.maxAttributes = gl.getParameter( gl.MAX_VERTEX_ATTRIBS );
	this.maxVertexUniforms = gl.getParameter( gl.MAX_VERTEX_UNIFORM_VECTORS );
	this.maxVaryings = gl.getParameter( gl.MAX_VARYING_VECTORS );
	this.maxFragmentUniforms = gl.getParameter( gl.MAX_FRAGMENT_UNIFORM_VECTORS );

	this.vertexTextures = this.maxVertexTextures > 0;
	this.floatFragmentTextures = !! extensions.get( 'OES_texture_float' );
	this.floatVertexTextures = this.vertexTextures && this.floatFragmentTextures;

	var _maxPrecision = getMaxPrecision( this.precision );

	if ( _maxPrecision !== this.precision ) {

		console.warn( 'THREE.WebGLRenderer:', this.precision, 'not supported, using', _maxPrecision, 'instead.' );
		this.precision = _maxPrecision;

	}

	if ( this.logarithmicDepthBuffer ) {

		this.logarithmicDepthBuffer = !! extensions.get( 'EXT_frag_depth' );

	}

};
