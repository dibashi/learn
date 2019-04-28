/**
 * @author mrdoob / http://mrdoob.com/
 * @author alteredq / http://alteredqualia.com/
 */

THREE.Material = function () {

	Object.defineProperty( this, 'id', { value: THREE.MaterialIdCount ++ } );

	this.uuid = THREE.Math.generateUUID();

	this.name = '';
	this.type = 'Material';

	this.fog = true;
	this.lights = true;

	this.blending = THREE.NormalBlending;
	this.side = THREE.FrontSide;

	/**
	 * 着色方式
	         绝大多数的3D物体是由多边形（polygon）所构成的，它们都必须经过某些着色处理的手续，才不会以线结构（wireframe）的方式显示。
	         这些着色处理方式有差到好，依次主要分为FlatShading、GouraudShading、PhoneShading、ScanlineRenderer、Ray-Traced。
	        FlatShading（平面着色）
	            也叫做“恒量着色”，平面着色是最简单也是最快速的着色方法，每个多边形都会被指定一个单一且没有变化的颜色。这种方法虽然会产生出不真实
	            的效果，不过它非常适用于快速成像及其它要求速度重于细致度的场合，如：生成预览动画。
	        GouraudShading（高洛德着色/高氏着色）
	            这种着色的效果要好得多，也是在游戏中使用最广泛的一种着色方式。它可对3D模型各顶点的颜色进行平滑、融合处理，将每个多边形上的每个点
	            赋以一组色调值，同时将多边形着上较为顺滑的渐变色，使其外观具有更强烈的实时感和立体动感，不过其着色速度比平面着色慢得多。
	        PhoneShading（补色着色）
	            首先，找出每个多边形顶点，然后根据内插值的方法，算出顶点间算连线上像素的光影值，接着再次运用线性的插值处理，算出其他所有像素高氏着
	            色在取样计算时，只顾及每个多边形顶点的光影效果，而补色着色却会把所有的点都计算进去。
	        ScanlineRenderer（扫描线着色）
	            这是3ds Max的默认渲染方式，它是一种基于一组连续水平线的着色方式，由于它渲染速度较快，一般被使用在预览场景中。
	        Ray-Traced（光线跟踪着色）
	            光线跟踪是真实按照物理照射光线的入射路径投射在物体上，最终反射回摄象机所得到每一个像素的真实值的着色算法，由于它计算精确，所得到的
	            图象效果优质，因此制作CG一定要使用该选项。
	        Radiosity（辐射着色）
	            这是一种类似光线跟踪的特效。它通过制定在场景中光线的来源并且根据物体的位置和反射情况来计算从观察者到光源的整个路径上的光影效果。在
	            这条线路上，光线受到不同物体的相互影响，如：反射、吸收、折射等情况都被计算在内。
	        glShadeModel( GLenum mode )可以设置的着色模型有：GL_SMOOTH和GL_FLAT
	            GL_FLAT恒定着色：对点，直线或多边形采用一种颜色进行绘制，整个图元的颜色就是它的任何一点的颜色。
	            GL_SMOOTH平滑着色：用多种颜色进行绘制，每个顶点都是单独进行处理的，各顶点和各图元之间采用均匀插值。

	 */
	this.shading = THREE.SmoothShading; // THREE.FlatShading, THREE.SmoothShading
	this.vertexColors = THREE.NoColors; // THREE.NoColors, THREE.VertexColors, THREE.FaceColors

	this.opacity = 1;
	/**renderer具体实现
	 * 	if (material.transparent) {

			array = transparentObjects;
			index = ++transparentObjectsLastIndex;

		} else {

			array = opaqueObjects;
			index = ++opaqueObjectsLastIndex;

		}

		if (material.transparent === true) {

			state.setBlending(material.blending, material.blendEquation, material.blendSrc, material.blendDst, material.blendEquationAlpha, material.blendSrcAlpha, material.blendDstAlpha, material.premultipliedAlpha);

		} else {

			state.setBlending(THREE.NoBlending);

		}

	 */
	this.transparent = false;

	this.blendSrc = THREE.SrcAlphaFactor;
	this.blendDst = THREE.OneMinusSrcAlphaFactor;
	this.blendEquation = THREE.AddEquation;
	this.blendSrcAlpha = null;
	this.blendDstAlpha = null;
	this.blendEquationAlpha = null;

	this.depthFunc = THREE.LessEqualDepth;
	this.depthTest = true;
	this.depthWrite = true;

	this.clippingPlanes = null;
	this.clipShadows = false;

	this.colorWrite = true;

	this.precision = null; // override the renderer's default precision for this material

	this.polygonOffset = false;
	this.polygonOffsetFactor = 0;
	this.polygonOffsetUnits = 0;

	this.alphaTest = 0;
	this.premultipliedAlpha = false;

	this.overdraw = 0; // Overdrawn pixels (typically between 0 and 1) for fixing antialiasing gaps in CanvasRenderer

	this.visible = true;

	this._needsUpdate = true;

};

THREE.Material.prototype = {

	constructor: THREE.Material,

	get needsUpdate() {

		return this._needsUpdate;

	},

	set needsUpdate( value ) {

		if ( value === true ) this.update();
		this._needsUpdate = value;

	},

	setValues: function ( values ) {

		if ( values === undefined ) return;

		for ( var key in values ) {

			var newValue = values[ key ];

			if ( newValue === undefined ) {

				console.warn( "THREE.Material: '" + key + "' parameter is undefined." );
				continue;

			}

			var currentValue = this[ key ];

			if ( currentValue === undefined ) {

				console.warn( "THREE." + this.type + ": '" + key + "' is not a property of this material." );
				continue;

			}

			if ( currentValue instanceof THREE.Color ) {

				currentValue.set( newValue );

			} else if ( currentValue instanceof THREE.Vector3 && newValue instanceof THREE.Vector3 ) {

				currentValue.copy( newValue );

			} else if ( key === 'overdraw' ) {

				// ensure overdraw is backwards-compatible with legacy boolean type
				this[ key ] = Number( newValue );

			} else {

				this[ key ] = newValue;

			}

		}

	},

	toJSON: function ( meta ) {

		var isRoot = meta === undefined;

		if ( isRoot ) {

			meta = {
				textures: {},
				images: {}
			};

		}

		var data = {
			metadata: {
				version: 4.4,
				type: 'Material',
				generator: 'Material.toJSON'
			}
		};

		// standard Material serialization
		data.uuid = this.uuid;
		data.type = this.type;

		if ( this.name !== '' ) data.name = this.name;

		if ( this.color instanceof THREE.Color ) data.color = this.color.getHex();

		if ( this.roughness !== undefined ) data.roughness = this.roughness;
		if ( this.metalness !== undefined ) data.metalness = this.metalness;

		if ( this.emissive instanceof THREE.Color ) data.emissive = this.emissive.getHex();
		if ( this.specular instanceof THREE.Color ) data.specular = this.specular.getHex();
		if ( this.shininess !== undefined ) data.shininess = this.shininess;

		if ( this.map instanceof THREE.Texture ) data.map = this.map.toJSON( meta ).uuid;
		if ( this.alphaMap instanceof THREE.Texture ) data.alphaMap = this.alphaMap.toJSON( meta ).uuid;
		if ( this.lightMap instanceof THREE.Texture ) data.lightMap = this.lightMap.toJSON( meta ).uuid;
		if ( this.bumpMap instanceof THREE.Texture ) {

			data.bumpMap = this.bumpMap.toJSON( meta ).uuid;
			data.bumpScale = this.bumpScale;

		}
		if ( this.normalMap instanceof THREE.Texture ) {

			data.normalMap = this.normalMap.toJSON( meta ).uuid;
			data.normalScale = this.normalScale.toArray();

		}
		if ( this.displacementMap instanceof THREE.Texture ) {

			data.displacementMap = this.displacementMap.toJSON( meta ).uuid;
			data.displacementScale = this.displacementScale;
			data.displacementBias = this.displacementBias;

		}
		if ( this.roughnessMap instanceof THREE.Texture ) data.roughnessMap = this.roughnessMap.toJSON( meta ).uuid;
		if ( this.metalnessMap instanceof THREE.Texture ) data.metalnessMap = this.metalnessMap.toJSON( meta ).uuid;

		if ( this.emissiveMap instanceof THREE.Texture ) data.emissiveMap = this.emissiveMap.toJSON( meta ).uuid;
		if ( this.specularMap instanceof THREE.Texture ) data.specularMap = this.specularMap.toJSON( meta ).uuid;

		if ( this.envMap instanceof THREE.Texture ) {

			data.envMap = this.envMap.toJSON( meta ).uuid;
			data.reflectivity = this.reflectivity; // Scale behind envMap

		}

		if ( this.size !== undefined ) data.size = this.size;
		if ( this.sizeAttenuation !== undefined ) data.sizeAttenuation = this.sizeAttenuation;

		if ( this.blending !== THREE.NormalBlending ) data.blending = this.blending;
		if ( this.shading !== THREE.SmoothShading ) data.shading = this.shading;
		if ( this.side !== THREE.FrontSide ) data.side = this.side;
		if ( this.vertexColors !== THREE.NoColors ) data.vertexColors = this.vertexColors;

		if ( this.opacity < 1 ) data.opacity = this.opacity;
		if ( this.transparent === true ) data.transparent = this.transparent;
		if ( this.alphaTest > 0 ) data.alphaTest = this.alphaTest;
		if ( this.premultipliedAlpha === true ) data.premultipliedAlpha = this.premultipliedAlpha;
		if ( this.wireframe === true ) data.wireframe = this.wireframe;
		if ( this.wireframeLinewidth > 1 ) data.wireframeLinewidth = this.wireframeLinewidth;

		// TODO: Copied from Object3D.toJSON

		function extractFromCache ( cache ) {

			var values = [];

			for ( var key in cache ) {

				var data = cache[ key ];
				delete data.metadata;
				values.push( data );

			}

			return values;

		}

		if ( isRoot ) {

			var textures = extractFromCache( meta.textures );
			var images = extractFromCache( meta.images );

			if ( textures.length > 0 ) data.textures = textures;
			if ( images.length > 0 ) data.images = images;

		}

		return data;

	},

	clone: function () {

		return new this.constructor().copy( this );

	},

	copy: function ( source ) {

		this.name = source.name;

		this.fog = source.fog;
		this.lights = source.lights;

		this.blending = source.blending;
		this.side = source.side;
		this.shading = source.shading;
		this.vertexColors = source.vertexColors;

		this.opacity = source.opacity;
		this.transparent = source.transparent;

		this.blendSrc = source.blendSrc;
		this.blendDst = source.blendDst;
		this.blendEquation = source.blendEquation;
		this.blendSrcAlpha = source.blendSrcAlpha;
		this.blendDstAlpha = source.blendDstAlpha;
		this.blendEquationAlpha = source.blendEquationAlpha;

		this.depthFunc = source.depthFunc;
		this.depthTest = source.depthTest;
		this.depthWrite = source.depthWrite;

		this.colorWrite = source.colorWrite;

		this.precision = source.precision;

		this.polygonOffset = source.polygonOffset;
		this.polygonOffsetFactor = source.polygonOffsetFactor;
		this.polygonOffsetUnits = source.polygonOffsetUnits;

		this.alphaTest = source.alphaTest;

		this.premultipliedAlpha = source.premultipliedAlpha;

		this.overdraw = source.overdraw;

		this.visible = source.visible;
		this.clipShadows = source.clipShadows;

		var srcPlanes = source.clippingPlanes,
			dstPlanes = null;

		if ( srcPlanes !== null ) {

			var n = srcPlanes.length;
			dstPlanes = new Array( n );

			for ( var i = 0; i !== n; ++ i )
				dstPlanes[ i ] = srcPlanes[ i ].clone();

		}

		this.clippingPlanes = dstPlanes;

		return this;

	},

	update: function () {

		this.dispatchEvent( { type: 'update' } );

	},

	dispose: function () {

		this.dispatchEvent( { type: 'dispose' } );

	}

};

Object.assign( THREE.Material.prototype, THREE.EventDispatcher.prototype );

THREE.MaterialIdCount = 0;
