/**
 * @author supereggbert / http://www.paulbrunt.co.uk/
 * @author mrdoob / http://mrdoob.com/
 * @author alteredq / http://alteredqualia.com/
 */

 /**
  * 这是webgl渲染器，有几个点再看代码前要分析一下
  * 一个是scene，里面有各种3d对象，3d对象由几何体和材质组成
  * scene里面还有light，用于和材质进行交互
  * 3d对象还会有什么呢？有自己的模型矩阵
  * 接下来是camera 拥有投影矩阵和视图矩阵
  * 似乎一切都有了，接下来就是根据threejs内部组织的数据结构进行渲染了
  */
THREE.WebGLRenderer = function ( parameters ) {

	// Currently you can use just up to 4 directional / point lights total.
	// Chrome barfs on shader linking when there are more than 4 lights :(

	// The problem comes from shader using too many varying vectors.

	// This is not GPU limitation as the same shader works ok in Firefox
	// and Chrome with "--use-gl=desktop" flag.

	// Difference comes from Chrome on Windows using by default ANGLE,
	// thus going DirectX9 route (while FF uses OpenGL).

	// See http://code.google.com/p/chromium/issues/detail?id=63491

	//看来不需要自己创建canvas了
	var _canvas = document.createElement( 'canvas' ), _gl,
	//这个老着色器？ 本质就是优化性能，当前着色器和之前的不一样就切换
	//如果一样就不切换，毕竟切换着色器是需要性能消耗的，有多大性能消耗？
	_oldProgram = null,
	//模型矩阵初始化，法线矩阵
	_modelViewMatrix = new THREE.Matrix4(), _normalMatrix,
	//一下只是 矩阵需要的数据，并不是矩阵
	//视图矩阵初始化
	_viewMatrixArray = new Float32Array(16),
	//模型视图矩阵初始化
	_modelViewMatrixArray = new Float32Array(16),
	//投影矩阵初始化
	_projectionMatrixArray = new Float32Array(16),
	//法线矩阵初始化
	_normalMatrixArray = new Float32Array(9),
	//不清楚了 这个和模型矩阵的区别是？
	_objectMatrixArray = new Float32Array(16),

	// parameters defaults
	//是否抗锯齿
	antialias = true,
	//清屏色
	clearColor = new THREE.Color( 0x000000 ),
	//alpha通道和 rgb分开了
	clearAlpha = 0;

	//若传入这个参数，还要根据这个参数来初始化，否则用上面的默认值
	if ( parameters ) {
		
		if ( parameters.antialias !== undefined ) antialias = parameters.antialias;
		if ( parameters.clearColor !== undefined ) clearColor.setHex( parameters.clearColor );
		if ( parameters.clearAlpha !== undefined ) clearAlpha = parameters.clearAlpha;
		
	}
	//冗余的字段	
	this.domElement = _canvas;
	//肯定要每帧都清除呀，莫名其妙的
	this.autoClear = true;
	//webgl的启动相关的一些设置
	initGL( antialias, clearColor, clearAlpha );

	//alert( dumpObject( getGLParams() ) );
	//作者并没有将方法放入 原型中，看来渲染器他只打算实例化一份
	//重设viewport
	/**
	 * gl.viewport告诉WebGL如何将裁剪空间（-1 到 +1）中的点转换到像素空间，
	 * 也就是画布内。当你第一次创建WebGL上下文的时候WebGL会设置视域大小和画布大小匹配， 
	 * 但是在那之后就需要你自己设置。当你改变画布大小就需要告诉WebGL新的视域设置。
	 */
	this.setSize = function ( width, height ) {

		_canvas.width = width;
		_canvas.height = height;
		_gl.viewport( 0, 0, _canvas.width, _canvas.height );

	};

	/**
	 * 还是tm的清屏色，这么简单的东西，前前后后绕了上百行，外部代码也有，我真的服了
	 */
	this.setClearColor = function( hex, alpha ) {

		var color = new THREE.Color( hex );
		_gl.clearColor( color.r, color.g, color.b, alpha );

	};

	//清除颜色和深度缓冲区
	this.clear = function () {

		_gl.clear( _gl.COLOR_BUFFER_BIT | _gl.DEPTH_BUFFER_BIT );

	};

	this.setupLights = function ( program, lights ) {

		var l, ll, light, r = g = b = 0,
			dcolors = [], dpositions = [],
			pcolors = [], ppositions = [];


		for ( l = 0, ll = lights.length; l < ll; l++ ) {

			light = lights[ l ];

			if ( light instanceof THREE.AmbientLight ) {

				r += light.color.r;
				g += light.color.g;
				b += light.color.b;
				
			} else if ( light instanceof THREE.DirectionalLight ) {

				dcolors.push( light.color.r * light.intensity,
							  light.color.g * light.intensity,
							  light.color.b * light.intensity );

				dpositions.push( light.position.x,
								 light.position.y,
								 light.position.z );

			} else if( light instanceof THREE.PointLight ) {

				pcolors.push( light.color.r * light.intensity,
							  light.color.g * light.intensity,
							  light.color.b * light.intensity );

				ppositions.push( light.position.x,
								 light.position.y,
								 light.position.z );
				
			}

		}
		
		return { ambient: [ r, g, b ], directional: { colors: dcolors, positions: dpositions }, point: { colors: pcolors, positions: ppositions } };

	};

	this.createBuffers = function ( object, g ) {

		var f, fl, fi, face, vertexNormals, faceNormal, normal, uv, v1, v2, v3, v4, t1, t2, t3, t4, m, ml, i,

		faceArray = [],
		lineArray = [],

		vertexArray = [],
		normalArray = [],
		tangentArray = [],
		uvArray = [],

		vertexIndex = 0,

		geometryChunk = object.geometry.geometryChunks[ g ],

		needsSmoothNormals = bufferNeedsSmoothNormals ( geometryChunk, object );

		for ( f = 0, fl = geometryChunk.faces.length; f < fl; f++ ) {

			fi = geometryChunk.faces[ f ];

			face = object.geometry.faces[ fi ];
			vertexNormals = face.vertexNormals;
			faceNormal = face.normal;
			uv = object.geometry.uvs[ fi ];

			if ( face instanceof THREE.Face3 ) {

				v1 = object.geometry.vertices[ face.a ].position;
				v2 = object.geometry.vertices[ face.b ].position;
				v3 = object.geometry.vertices[ face.c ].position;

				vertexArray.push( v1.x, v1.y, v1.z,
								  v2.x, v2.y, v2.z,
								  v3.x, v3.y, v3.z );

				if ( object.geometry.hasTangents ) {

					t1 = object.geometry.vertices[ face.a ].tangent;
					t2 = object.geometry.vertices[ face.b ].tangent;
					t3 = object.geometry.vertices[ face.c ].tangent;

					tangentArray.push( t1.x, t1.y, t1.z, t1.w,
									   t2.x, t2.y, t2.z, t2.w,
									   t3.x, t3.y, t3.z, t3.w );

				}

				if ( vertexNormals.length == 3 && needsSmoothNormals ) {


					for ( i = 0; i < 3; i ++ ) {

						normalArray.push( vertexNormals[ i ].x, vertexNormals[ i ].y, vertexNormals[ i ].z );

					}

				} else {

					for ( i = 0; i < 3; i ++ ) {

						normalArray.push( faceNormal.x, faceNormal.y, faceNormal.z );

					}

				}

				if ( uv ) {

					for ( i = 0; i < 3; i ++ ) {

						uvArray.push( uv[ i ].u, uv[ i ].v );

					}

				}

				faceArray.push( vertexIndex, vertexIndex + 1, vertexIndex + 2 );

				// TODO: don't add lines that already exist (faces sharing edge)

				lineArray.push( vertexIndex, vertexIndex + 1,
								vertexIndex, vertexIndex + 2,
								vertexIndex + 1, vertexIndex + 2 );

				vertexIndex += 3;

			} else if ( face instanceof THREE.Face4 ) {

				v1 = object.geometry.vertices[ face.a ].position;
				v2 = object.geometry.vertices[ face.b ].position;
				v3 = object.geometry.vertices[ face.c ].position;
				v4 = object.geometry.vertices[ face.d ].position;

				vertexArray.push( v1.x, v1.y, v1.z,
								  v2.x, v2.y, v2.z,
								  v3.x, v3.y, v3.z,
								  v4.x, v4.y, v4.z );

				if ( object.geometry.hasTangents ) {

					t1 = object.geometry.vertices[ face.a ].tangent;
					t2 = object.geometry.vertices[ face.b ].tangent;
					t3 = object.geometry.vertices[ face.c ].tangent;
					t4 = object.geometry.vertices[ face.d ].tangent;

					tangentArray.push( t1.x, t1.y, t1.z, t1.w,
									   t2.x, t2.y, t2.z, t2.w,
									   t3.x, t3.y, t3.z, t3.w,
									   t4.x, t4.y, t4.z, t4.w );

				}

				if ( vertexNormals.length == 4 && needsSmoothNormals ) {

					for ( i = 0; i < 4; i ++ ) {

						normalArray.push( vertexNormals[ i ].x, vertexNormals[ i ].y, vertexNormals[ i ].z );

					}

				} else {

					for ( i = 0; i < 4; i ++ ) {

						normalArray.push( faceNormal.x, faceNormal.y, faceNormal.z );

					}

				}

				if ( uv ) {

					for ( i = 0; i < 4; i ++ ) {

						uvArray.push( uv[ i ].u, uv[ i ].v );

					}

				}

				faceArray.push( vertexIndex, vertexIndex + 1, vertexIndex + 2,
								vertexIndex, vertexIndex + 2, vertexIndex + 3 );

				// TODO: don't add lines that already exist (faces sharing edge)

				lineArray.push( vertexIndex, vertexIndex + 1,
								vertexIndex, vertexIndex + 2,
								vertexIndex, vertexIndex + 3,
								vertexIndex + 1, vertexIndex + 2,
								vertexIndex + 2, vertexIndex + 3 );

				vertexIndex += 4;

			}

		}

		if ( !vertexArray.length ) {

			return;

		}

		geometryChunk.__webGLVertexBuffer = _gl.createBuffer();
		_gl.bindBuffer( _gl.ARRAY_BUFFER, geometryChunk.__webGLVertexBuffer );
		_gl.bufferData( _gl.ARRAY_BUFFER, new Float32Array( vertexArray ), _gl.STATIC_DRAW );

		geometryChunk.__webGLNormalBuffer = _gl.createBuffer();
		_gl.bindBuffer( _gl.ARRAY_BUFFER, geometryChunk.__webGLNormalBuffer );
		_gl.bufferData( _gl.ARRAY_BUFFER, new Float32Array( normalArray ), _gl.STATIC_DRAW );

		if ( object.geometry.hasTangents ) {

			geometryChunk.__webGLTangentBuffer = _gl.createBuffer();
			_gl.bindBuffer( _gl.ARRAY_BUFFER, geometryChunk.__webGLTangentBuffer );
			_gl.bufferData( _gl.ARRAY_BUFFER, new Float32Array( tangentArray ), _gl.STATIC_DRAW );

		}

		if ( uvArray.length > 0 ) {

			geometryChunk.__webGLUVBuffer = _gl.createBuffer();
			_gl.bindBuffer( _gl.ARRAY_BUFFER, geometryChunk.__webGLUVBuffer );
			_gl.bufferData( _gl.ARRAY_BUFFER, new Float32Array( uvArray ), _gl.STATIC_DRAW );

		}

		geometryChunk.__webGLFaceBuffer = _gl.createBuffer();
		_gl.bindBuffer( _gl.ELEMENT_ARRAY_BUFFER, geometryChunk.__webGLFaceBuffer );
		_gl.bufferData( _gl.ELEMENT_ARRAY_BUFFER, new Uint16Array( faceArray ), _gl.STATIC_DRAW );

		geometryChunk.__webGLLineBuffer = _gl.createBuffer();
		_gl.bindBuffer( _gl.ELEMENT_ARRAY_BUFFER, geometryChunk.__webGLLineBuffer );
		_gl.bufferData( _gl.ELEMENT_ARRAY_BUFFER, new Uint16Array( lineArray ), _gl.STATIC_DRAW );

		geometryChunk.__webGLFaceCount = faceArray.length;
		geometryChunk.__webGLLineCount = lineArray.length;

	};

	function setMaterialShaders( material, shaders ) {

		material.fragment_shader = shaders.fragment_shader;
		material.vertex_shader = shaders.vertex_shader;
		material.uniforms = Uniforms.clone( shaders.uniforms );

	};

	function refreshUniformsCommon( material, fog ) {
		
		// premultiply alpha
		material.uniforms.color.value.setRGB( material.color.r * material.opacity, material.color.g * material.opacity, material.color.b * material.opacity );
		
		// pure color
		//material.uniforms.color.value.setHex( material.color.hex );
		
		material.uniforms.opacity.value = material.opacity;
		material.uniforms.map.texture = material.map;

		material.uniforms.env_map.texture = material.env_map;
		material.uniforms.reflectivity.value = material.reflectivity;
		material.uniforms.refraction_ratio.value = material.refraction_ratio;
		material.uniforms.combine.value = material.combine;
		material.uniforms.useRefract.value = material.env_map && material.env_map.mapping instanceof THREE.CubeRefractionMapping;

		if ( fog ) {

			material.uniforms.fogColor.value.setHex( fog.color.hex );

			if ( fog instanceof THREE.Fog ) {

				material.uniforms.fogNear.value = fog.near;
				material.uniforms.fogFar.value = fog.far;

			} else if ( fog instanceof THREE.FogExp2 ) {

				material.uniforms.fogDensity.value = fog.density;

			}

		}

	};
	
	function refreshUniformsPhong( material ) {
		
		//material.uniforms.ambient.value.setHex( material.ambient.hex );
		//material.uniforms.specular.value.setHex( material.specular.hex );
		material.uniforms.ambient.value.setRGB( material.ambient.r, material.ambient.g, material.ambient.b );
		material.uniforms.specular.value.setRGB( material.specular.r, material.specular.g, material.specular.b );
		material.uniforms.shininess.value = material.shininess;
		
	};
	
	
	function refreshLights( material, lights ) {
		
		material.uniforms.enableLighting.value = lights.directional.positions.length + lights.point.positions.length;
		material.uniforms.ambientLightColor.value = lights.ambient;
		material.uniforms.directionalLightColor.value = lights.directional.colors;
		material.uniforms.directionalLightDirection.value = lights.directional.positions;
		material.uniforms.pointLightColor.value = lights.point.colors;
		material.uniforms.pointLightPosition.value = lights.point.positions;
		
	};
	
	this.renderBuffer = function ( camera, lights, fog, material, geometryChunk ) {

		var program, u, identifiers, attributes, parameters, vector_lights, maxLightCount;

		if ( !material.program ) {

			if ( material instanceof THREE.MeshDepthMaterial ) {

				setMaterialShaders( material, THREE.ShaderLib[ 'depth' ] );

				material.uniforms.mNear.value = camera.near;
				material.uniforms.mFar.value = camera.far;

			} else if ( material instanceof THREE.MeshNormalMaterial ) {

				setMaterialShaders( material, THREE.ShaderLib[ 'normal' ] );

			} else if ( material instanceof THREE.MeshBasicMaterial ) {

				setMaterialShaders( material, THREE.ShaderLib[ 'basic' ] );

				refreshUniformsCommon( material, fog );
				
			} else if ( material instanceof THREE.MeshLambertMaterial ) {
				
				setMaterialShaders( material, THREE.ShaderLib[ 'lambert' ] );
				
				refreshUniformsCommon( material, fog );
				
			} else if ( material instanceof THREE.MeshPhongMaterial ) {
				
				setMaterialShaders( material, THREE.ShaderLib[ 'phong' ] );
				
				refreshUniformsCommon( material, fog );
				
			}

			// heuristics to create shader parameters according to lights in the scene
			// (not to blow over maxLights budget)

			maxLightCount = allocateLights( lights, 4 );

			parameters = { fog: fog, map: material.map, env_map: material.env_map, maxDirLights: maxLightCount.directional, maxPointLights: maxLightCount.point };
			material.program = buildProgram( material.fragment_shader, material.vertex_shader, parameters );

			identifiers = [ 'viewMatrix', 'modelViewMatrix', 'projectionMatrix', 'normalMatrix', 'objectMatrix', 'cameraPosition' ];
			for( u in material.uniforms ) {

				identifiers.push(u);

			}

			cacheUniformLocations( material.program, identifiers );
			cacheAttributeLocations( material.program, [ "position", "normal", "uv", "tangent" ] );

		}

		program = material.program;

		if( program != _oldProgram ) {

			_gl.useProgram( program );
			_oldProgram = program;

		}

		this.loadCamera( program, camera );
		this.loadMatrices( program );

		if ( material instanceof THREE.MeshPhongMaterial || 
			 material instanceof THREE.MeshLambertMaterial ) {

			vector_lights = this.setupLights( program, lights );
			refreshLights( material, vector_lights );

		}

		if ( material instanceof THREE.MeshBasicMaterial ||
			 material instanceof THREE.MeshLambertMaterial ||
			 material instanceof THREE.MeshPhongMaterial ) {
			
			refreshUniformsCommon( material, fog );

		}
		
		if ( material instanceof THREE.MeshPhongMaterial ) {
			
			refreshUniformsPhong( material );

		}

		setUniforms( program, material.uniforms );

		attributes = program.attributes;

		// vertices

		_gl.bindBuffer( _gl.ARRAY_BUFFER, geometryChunk.__webGLVertexBuffer );
		_gl.vertexAttribPointer( attributes.position, 3, _gl.FLOAT, false, 0, 0 );
		_gl.enableVertexAttribArray( attributes.position );

		// normals

		if ( attributes.normal >= 0 ) {

			_gl.bindBuffer( _gl.ARRAY_BUFFER, geometryChunk.__webGLNormalBuffer );
			_gl.vertexAttribPointer( attributes.normal, 3, _gl.FLOAT, false, 0, 0 );
			_gl.enableVertexAttribArray( attributes.normal );

		}

		// tangents

		if ( attributes.tangent >= 0 ) {

			_gl.bindBuffer( _gl.ARRAY_BUFFER, geometryChunk.__webGLTangentBuffer );
			_gl.vertexAttribPointer( attributes.tangent, 4, _gl.FLOAT, false, 0, 0 );
			_gl.enableVertexAttribArray( attributes.tangent );

		}

		// uvs

		if ( attributes.uv >= 0 ) {

			if ( geometryChunk.__webGLUVBuffer ) {

				_gl.bindBuffer( _gl.ARRAY_BUFFER, geometryChunk.__webGLUVBuffer );
				_gl.vertexAttribPointer( attributes.uv, 2, _gl.FLOAT, false, 0, 0 );

				_gl.enableVertexAttribArray( attributes.uv );

			} else {

				_gl.disableVertexAttribArray( attributes.uv );

			}

		}

		// render triangles

		if ( ! material.wireframe ) {

			_gl.bindBuffer( _gl.ELEMENT_ARRAY_BUFFER, geometryChunk.__webGLFaceBuffer );
			_gl.drawElements( _gl.TRIANGLES, geometryChunk.__webGLFaceCount, _gl.UNSIGNED_SHORT, 0 );

		// render lines

		} else {

			_gl.lineWidth( material.wireframe_linewidth );
			_gl.bindBuffer( _gl.ELEMENT_ARRAY_BUFFER, geometryChunk.__webGLLineBuffer );
			_gl.drawElements( _gl.LINES, geometryChunk.__webGLLineCount, _gl.UNSIGNED_SHORT, 0 );

		}

	};

	this.renderPass = function ( camera, lights, fog, object, geometryChunk, blending, transparent ) {

		var i, l, m, ml, material, meshMaterial;

		for ( m = 0, ml = object.materials.length; m < ml; m++ ) {

			meshMaterial = object.materials[ m ];

			if ( meshMaterial instanceof THREE.MeshFaceMaterial ) {

				for ( i = 0, l = geometryChunk.materials.length; i < l; i++ ) {

					material = geometryChunk.materials[ i ];

					if ( material && material.blending == blending && ( material.opacity < 1.0 == transparent ) ) {

						this.setBlending( material.blending );
						this.renderBuffer( camera, lights, fog, material, geometryChunk );

					}

				}

			} else {

				material = meshMaterial;
				if ( material && material.blending == blending && ( material.opacity < 1.0 == transparent ) ) {

					this.setBlending( material.blending );
					this.renderBuffer( camera, lights, fog, material, geometryChunk );

				}

			}

		}

	};

	/**
	 * 对外的核心接口，用这个来调用一切的一切，然后画出来，没了。
	 */
	this.render = function( scene, camera ) {

		var o, ol, webGLObject, object, buffer,
			lights = scene.lights,
			fog = scene.fog;

		this.initWebGLObjects( scene );

		//先清屏
		if ( this.autoClear ) {

			this.clear();

		}

		//更新视图矩阵 
		camera.autoUpdateMatrix && camera.updateMatrix();
		//获得视图矩阵元素
		_viewMatrixArray.set( camera.matrix.flatten() );
		//最早在new摄像机的时候 其内部就初始化了一个投影矩阵 
		//flatten()从矩阵对象中获得16个值。
		_projectionMatrixArray.set( camera.projectionMatrix.flatten() );

		/**
		 * 总览下方的两个主循环
		 * 本质上就是 先传入不透明的对象 画出来
		 * 再传入透明的对象 画出来
		 * 有一个问题是必须要考虑的，也是这个算法来源的根据
		 * 我先画不透明的ok，我把深度缓冲区也更新了，透明的假设深度缓冲大于当前的值，肯定是看不见了
		 * 小于当前的值 肯定是可以看见了，但是~！！他不能更新深度缓冲区，因为他是透明的，不能以他的
		 * 深度值为依据来判断物体是否可见，我要渲染透明的 只要他比所有不透明的深度值都小，他就应该参与
		 * 混合
		 */
		// opaque pass
		for ( o = 0, ol = scene.__webGLObjects.length; o < ol; o++ ) {

			webGLObject = scene.__webGLObjects[ o ];

			object = webGLObject.object;
			buffer = webGLObject.buffer;

			if ( object.visible ) {

				this.setupMatrices( object, camera );
				this.renderPass( camera, lights, fog, object, buffer, THREE.NormalBlending, false );

			}

		}

		// transparent pass

		for ( o = 0, ol = scene.__webGLObjects.length; o < ol; o++ ) {

			webGLObject = scene.__webGLObjects[ o ];

			object = webGLObject.object;
			buffer = webGLObject.buffer;

			if ( object.visible ) {

				this.setupMatrices( object, camera );

				// opaque blended materials

				this.renderPass( camera, lights, fog, object, buffer, THREE.AdditiveBlending, false );
				this.renderPass( camera, lights, fog, object, buffer, THREE.SubtractiveBlending, false );

				// transparent blended materials

				this.renderPass( camera, lights, fog, object, buffer, THREE.AdditiveBlending, true );
				this.renderPass( camera, lights, fog, object, buffer, THREE.SubtractiveBlending, true );

				// transparent normal materials

				this.renderPass( camera, lights, fog, object, buffer, THREE.NormalBlending, true );

			}

		}

	};

	//我并没有自信完全解释这个算法
	this.initWebGLObjects = function( scene ) {

		var o, ol, object, globject, g, geometryChunk, objmap;
		//第一次执行到这里的话，给他初始了两个集合 之后还是循环遍历这个场景的话 不用再初始化了
		if ( !scene.__webGLObjects ) {
			//这两个集合是什么？我还不清楚
			scene.__webGLObjects = [];
			scene.__webGLObjectsMap = {};

		}

		//遍历场景中所有的对象
		for ( o = 0, ol = scene.objects.length; o < ol; o++ ) {
			//取出当前对象
			object = scene.objects[ o ];

			if ( scene.__webGLObjectsMap[ object.id ] == undefined ) {
				
				scene.__webGLObjectsMap[ object.id ] = {};

			}

			objmap = scene.__webGLObjectsMap[ object.id ];
			//可以想象 场景的对象分为三类 除了光，雾等
			if ( object instanceof THREE.Mesh ) {
				//如果是mesh
				// create separate VBOs per geometry chunk

				for ( g in object.geometry.geometryChunks ) {

					geometryChunk = object.geometry.geometryChunks[ g ];

					// initialise VBO on the first access

					if( ! geometryChunk.__webGLVertexBuffer ) {

						this.createBuffers( object, g );

					}

					// create separate wrapper per each use of VBO

					if ( objmap[ g ] == undefined ) {

						globject = { buffer: geometryChunk, object: object };
						scene.__webGLObjects.push( globject );

						objmap[ g ] = 1;

					}

				}

			}/* else if ( object instanceof THREE.Line ) {

			} else if ( object instanceof THREE.Particle ) {

			}*/

		}

	};

	this.removeObject = function ( scene, object ) {

		var o, ol, zobject;

		for ( o = scene.__webGLObjects.length - 1; o >= 0; o-- ) {

			zobject = scene.__webGLObjects[ o ].object;

			if ( object == zobject ) {

				scene.__webGLObjects.splice( o, 1 );

			}

		}

	};

	this.setupMatrices = function ( object, camera ) {

		//更新物体自身的矩阵 代码逻辑上可能是这样的：
		//先物理算法，ai逻辑，动画逻辑改变物体自身的属性值，
		//到渲染这里才将属性值同步至矩阵，传入着色器
		object.autoUpdateMatrix && object.updateMatrix();

		_modelViewMatrix.multiply( camera.matrix, object.matrix );
		_modelViewMatrixArray.set( _modelViewMatrix.flatten() );
		//!!!这里的法线矩阵是以模型视图矩阵为基础的 为什么不是以模型矩阵为基础？
		//
		_normalMatrix = THREE.Matrix4.makeInvert3x3( _modelViewMatrix ).transpose();
		_normalMatrixArray.set( _normalMatrix.m );

		_objectMatrixArray.set( object.matrix.flatten() );

	};

	this.loadMatrices = function ( program ) {
		//塞入视图矩阵
		_gl.uniformMatrix4fv( program.uniforms.viewMatrix, false, _viewMatrixArray );
		_gl.uniformMatrix4fv( program.uniforms.modelViewMatrix, false, _modelViewMatrixArray );
		//塞入投影矩阵
		_gl.uniformMatrix4fv( program.uniforms.projectionMatrix, false, _projectionMatrixArray );
		_gl.uniformMatrix3fv( program.uniforms.normalMatrix, false, _normalMatrixArray );
		_gl.uniformMatrix4fv( program.uniforms.objectMatrix, false, _objectMatrixArray );

	};

	this.loadCamera = function( program, camera ) {

		_gl.uniform3f( program.uniforms.cameraPosition, camera.position.x, camera.position.y, camera.position.z );

	};

	this.setBlending = function( blending ) {

		switch ( blending ) {

			case THREE.AdditiveBlending:

				_gl.blendEquation( _gl.FUNC_ADD );
				_gl.blendFunc( _gl.ONE, _gl.ONE );

				break;

			case THREE.SubtractiveBlending:

				//_gl.blendEquation( _gl.FUNC_SUBTRACT );
				_gl.blendFunc( _gl.DST_COLOR, _gl.ZERO );

				break;

			default:

				_gl.blendEquation( _gl.FUNC_ADD );
				_gl.blendFunc( _gl.ONE, _gl.ONE_MINUS_SRC_ALPHA );

				break;
		}

	};

	this.setFaceCulling = function ( cullFace, frontFace ) {

		if ( cullFace ) {

			if ( !frontFace || frontFace == "ccw" ) {

				_gl.frontFace( _gl.CCW );

			} else {

				_gl.frontFace( _gl.CW );

			}

			if( cullFace == "back" ) {

				_gl.cullFace( _gl.BACK );

			} else if( cullFace == "front" ) {

				_gl.cullFace( _gl.FRONT );

			} else {

				_gl.cullFace( _gl.FRONT_AND_BACK );

			}

			_gl.enable( _gl.CULL_FACE );

		} else {

			_gl.disable( _gl.CULL_FACE );

		}

	};

	this.supportsVertexTextures = function() {

		return maxVertexTextures() > 0;

	};

	function maxVertexTextures() {

		return _gl.getParameter( _gl.MAX_VERTEX_TEXTURE_IMAGE_UNITS );

	};

	function initGL( antialias, clearColor, clearAlpha ) {

		try {
			//获得gl上下文 显然这里已经过时了 根据条件是否开启抗锯齿
			_gl = _canvas.getContext( 'experimental-webgl', { antialias: antialias } );

		} catch(e) { }
		//gl 没有获得
		if (!_gl) {

			alert("WebGL not supported");
			throw "cannot create webgl context";

		}
		//清屏色，与下方的冗余了
		_gl.clearColor( 0, 0, 0, 1 );
		//初始化深度缓冲区
		_gl.clearDepth( 1 );
		//开启深度测试
		_gl.enable( _gl.DEPTH_TEST );
		//小于等于就通过深度测试
		//specifies a function that compares incoming pixel depth to the current depth buffer value.
		//pass if the incoming value is less than or equal to the depth buffer value
		_gl.depthFunc( _gl.LEQUAL );
		//specifies whether polygons are front- or back-facing by setting a winding orientation.
		/**
		 * A GLenum type winding orientation. The default value is gl.CCW. Possible values:
           gl.CW: Clock-wise winding.
           gl.CCW: Counter-clock-wise winding.
		 */
		//指定 逆时针的缠绕方向为正面
		_gl.frontFace( _gl.CCW );
		//背面剔除
		_gl.cullFace( _gl.BACK );
		//开启背面剔除
		_gl.enable( _gl.CULL_FACE );
		//开启混合
		_gl.enable( _gl.BLEND );
		//混合方式The formula for the blending color can be described like this: 
		//color(RGBA) = (sourceColor * sfactor) + (destinationColor * dfactor). 
		//The RBGA values are between 0 and 1.
		_gl.blendFunc( _gl.ONE, _gl.ONE_MINUS_SRC_ALPHA );
		//清除缓冲区的颜色值
		_gl.clearColor( clearColor.r, clearColor.g, clearColor.b, clearAlpha );

	};
	
	function buildProgram( fragment_shader, vertex_shader, parameters ) {

		var program = _gl.createProgram(),

		prefix_fragment = [
			"#ifdef GL_ES",
			"precision highp float;",
			"#endif",
		
			"#define MAX_DIR_LIGHTS " + parameters.maxDirLights,
			"#define MAX_POINT_LIGHTS " + parameters.maxPointLights,

			parameters.fog ? "#define USE_FOG" : "",
			parameters.fog instanceof THREE.FogExp2 ? "#define FOG_EXP2" : "",

			parameters.map ? "#define USE_MAP" : "",
			parameters.env_map ? "#define USE_ENVMAP" : "",

			"uniform mat4 viewMatrix;",
			"uniform vec3 cameraPosition;",
			""
		].join("\n"),

		prefix_vertex = [
			maxVertexTextures() > 0 ? "#define VERTEX_TEXTURES" : "",

			"#define MAX_DIR_LIGHTS " + parameters.maxDirLights,
			"#define MAX_POINT_LIGHTS " + parameters.maxPointLights,

			parameters.map ? "#define USE_MAP" : "",
			parameters.env_map ? "#define USE_ENVMAP" : "",

			"uniform mat4 objectMatrix;",
			"uniform mat4 modelViewMatrix;",
			"uniform mat4 projectionMatrix;",
			"uniform mat4 viewMatrix;",
			"uniform mat3 normalMatrix;",
			"uniform vec3 cameraPosition;",
			"attribute vec3 position;",
			"attribute vec3 normal;",
			"attribute vec2 uv;",
			""
		].join("\n");

		_gl.attachShader( program, getShader( "fragment", prefix_fragment + fragment_shader ) );
		_gl.attachShader( program, getShader( "vertex", prefix_vertex + vertex_shader ) );

		_gl.linkProgram( program );

		if ( !_gl.getProgramParameter( program, _gl.LINK_STATUS ) ) {

			alert( "Could not initialise shaders\n"+
					"VALIDATE_STATUS: " + _gl.getProgramParameter( program, _gl.VALIDATE_STATUS ) + ", gl error [" + _gl.getError() + "]" );
			
			//console.log( prefix_fragment + fragment_shader );
			//console.log( prefix_vertex + vertex_shader );

		}

		program.uniforms = {};
		program.attributes = {};

		return program;

	};

	function setUniforms( program, uniforms ) {

		var u, value, type, location, texture;

		for( u in uniforms ) {

			type = uniforms[u].type;
			value = uniforms[u].value;
			location = program.uniforms[u];

			if( type == "i" ) {

				_gl.uniform1i( location, value );

			} else if( type == "f" ) {

				_gl.uniform1f( location, value );

			} else if( type == "fv" ) {

				_gl.uniform3fv( location, value );

			} else if( type == "v2" ) {

				_gl.uniform2f( location, value.x, value.y );

			} else if( type == "v3" ) {

				_gl.uniform3f( location, value.x, value.y, value.z );

			} else if( type == "c" ) {

				_gl.uniform3f( location, value.r, value.g, value.b );

			} else if( type == "t" ) {

				_gl.uniform1i( location, value );

				texture = uniforms[u].texture;

				if ( !texture ) continue;

				if ( texture.image instanceof Array && texture.image.length == 6 ) {

					setCubeTexture( texture, value );

				} else {

					setTexture( texture, value );

				}

			}

		}

	};

	function setCubeTexture( texture, slot ) {

		if ( texture.image.length == 6 ) {

			if ( !texture.image.__webGLTextureCube &&
				 !texture.image.__cubeMapInitialized && texture.image.loadCount == 6 ) {

				texture.image.__webGLTextureCube = _gl.createTexture();

				_gl.bindTexture( _gl.TEXTURE_CUBE_MAP, texture.image.__webGLTextureCube );

				_gl.texParameteri( _gl.TEXTURE_CUBE_MAP, _gl.TEXTURE_WRAP_S, _gl.CLAMP_TO_EDGE );
				_gl.texParameteri( _gl.TEXTURE_CUBE_MAP, _gl.TEXTURE_WRAP_T, _gl.CLAMP_TO_EDGE );

				_gl.texParameteri( _gl.TEXTURE_CUBE_MAP, _gl.TEXTURE_MAG_FILTER, _gl.LINEAR );
				_gl.texParameteri( _gl.TEXTURE_CUBE_MAP, _gl.TEXTURE_MIN_FILTER, _gl.LINEAR_MIPMAP_LINEAR );

				for ( var i = 0; i < 6; ++i ) {

					_gl.texImage2D( _gl.TEXTURE_CUBE_MAP_POSITIVE_X + i, 0, _gl.RGBA, _gl.RGBA, _gl.UNSIGNED_BYTE, texture.image[ i ] );

				}

				_gl.generateMipmap( _gl.TEXTURE_CUBE_MAP );

				_gl.bindTexture( _gl.TEXTURE_CUBE_MAP, null );

				texture.image.__cubeMapInitialized = true;

			}

			_gl.activeTexture( _gl.TEXTURE0 + slot );
			_gl.bindTexture( _gl.TEXTURE_CUBE_MAP, texture.image.__webGLTextureCube );

		}

	};

	function setTexture( texture, slot ) {

		if ( !texture.__webGLTexture && texture.image.loaded ) {

			texture.__webGLTexture = _gl.createTexture();
			_gl.bindTexture( _gl.TEXTURE_2D, texture.__webGLTexture );
			_gl.texImage2D( _gl.TEXTURE_2D, 0, _gl.RGBA, _gl.RGBA, _gl.UNSIGNED_BYTE, texture.image );

			_gl.texParameteri( _gl.TEXTURE_2D, _gl.TEXTURE_WRAP_S, paramThreeToGL( texture.wrap_s ) );
			_gl.texParameteri( _gl.TEXTURE_2D, _gl.TEXTURE_WRAP_T, paramThreeToGL( texture.wrap_t ) );

			_gl.texParameteri( _gl.TEXTURE_2D, _gl.TEXTURE_MAG_FILTER, paramThreeToGL( texture.mag_filter ) );
			_gl.texParameteri( _gl.TEXTURE_2D, _gl.TEXTURE_MIN_FILTER, paramThreeToGL( texture.min_filter ) );
			_gl.generateMipmap( _gl.TEXTURE_2D );
			_gl.bindTexture( _gl.TEXTURE_2D, null );

		}

		_gl.activeTexture( _gl.TEXTURE0 + slot );
		_gl.bindTexture( _gl.TEXTURE_2D, texture.__webGLTexture );

	};

	function cacheUniformLocations( program, identifiers ) {

		var i, l, id;

		for( i = 0, l = identifiers.length; i < l; i++ ) {

			id = identifiers[ i ];
			program.uniforms[ id ] = _gl.getUniformLocation( program, id );

		}

	};

	function cacheAttributeLocations( program, identifiers ) {

		var i, l, id;

		for( i = 0, l = identifiers.length; i < l; i++ ) {

			id = identifiers[ i ];
			program.attributes[ id ] = _gl.getAttribLocation( program, id );

		}

	};

	function getShader( type, string ) {

		var shader;

		if ( type == "fragment" ) {

			shader = _gl.createShader( _gl.FRAGMENT_SHADER );

		} else if ( type == "vertex" ) {

			shader = _gl.createShader( _gl.VERTEX_SHADER );

		}

		_gl.shaderSource( shader, string );
		_gl.compileShader( shader );

		if ( !_gl.getShaderParameter( shader, _gl.COMPILE_STATUS ) ) {

			alert( _gl.getShaderInfoLog( shader ) );
			return null;

		}

		return shader;

	};

	function paramThreeToGL( p ) {

		switch ( p ) {

			case THREE.RepeatWrapping: return _gl.REPEAT; break;
			case THREE.ClampToEdgeWrapping: return _gl.CLAMP_TO_EDGE; break;
			case THREE.MirroredRepeatWrapping: return _gl.MIRRORED_REPEAT; break;

			case THREE.NearestFilter: return _gl.NEAREST; break;
			case THREE.NearestMipMapNearestFilter: return _gl.NEAREST_MIPMAP_NEAREST; break;
			case THREE.NearestMipMapLinearFilter: return _gl.NEAREST_MIPMAP_LINEAR; break;

			case THREE.LinearFilter: return _gl.LINEAR; break;
			case THREE.LinearMipMapNearestFilter: return _gl.LINEAR_MIPMAP_NEAREST; break;
			case THREE.LinearMipMapLinearFilter: return _gl.LINEAR_MIPMAP_LINEAR; break;

		}

		return 0;

	};

	function materialNeedsSmoothNormals( material ) {

		return material && material.shading != undefined && material.shading == THREE.SmoothShading;

	};

	function bufferNeedsSmoothNormals( geometryChunk, object ) {

		var m, ml, i, l, meshMaterial, needsSmoothNormals = false;

		for ( m = 0, ml = object.materials.length; m < ml; m++ ) {

			meshMaterial = object.materials[ m ];

			if ( meshMaterial instanceof THREE.MeshFaceMaterial ) {

				for ( i = 0, l = geometryChunk.materials.length; i < l; i++ ) {

					if ( materialNeedsSmoothNormals( geometryChunk.materials[ i ] ) ) {

						needsSmoothNormals = true;
						break;

					}

				}

			} else {

				if ( materialNeedsSmoothNormals( meshMaterial ) ) {

					needsSmoothNormals = true;
					break;

				}

			}

			if ( needsSmoothNormals ) break;

		}

		return needsSmoothNormals;

	};

	function allocateLights( lights, maxLights ) {

		if ( scene ) {

			var l, ll, light, dirLights, pointLights, maxDirLights, maxPointLights;
			dirLights = pointLights = maxDirLights = maxPointLights = 0;

			for ( l = 0, ll = lights.length; l < ll; l++ ) {

				light = lights[ l ];

				if ( light instanceof THREE.DirectionalLight ) dirLights++;
				if ( light instanceof THREE.PointLight ) pointLights++;

			}

			if ( ( pointLights + dirLights ) <= maxLights ) {

				maxDirLights = dirLights;
				maxPointLights = pointLights;

			} else {

				maxDirLights = Math.ceil( maxLights * dirLights / ( pointLights + dirLights ) );
				maxPointLights = maxLights - maxDirLights;

			}

			return { 'directional' : maxDirLights, 'point' : maxPointLights };

		}

		return { 'directional' : 1, 'point' : maxLights - 1 };

	};

	/* DEBUG
	function getGLParams() {

		var params  = {

			'MAX_VARYING_VECTORS': _gl.getParameter( _gl.MAX_VARYING_VECTORS ),
			'MAX_VERTEX_ATTRIBS': _gl.getParameter( _gl.MAX_VERTEX_ATTRIBS ),

			'MAX_TEXTURE_IMAGE_UNITS': _gl.getParameter( _gl.MAX_TEXTURE_IMAGE_UNITS ),
			'MAX_VERTEX_TEXTURE_IMAGE_UNITS': _gl.getParameter( _gl.MAX_VERTEX_TEXTURE_IMAGE_UNITS ),
			'MAX_COMBINED_TEXTURE_IMAGE_UNITS' : _gl.getParameter( _gl.MAX_COMBINED_TEXTURE_IMAGE_UNITS ),

			'MAX_VERTEX_UNIFORM_VECTORS': _gl.getParameter( _gl.MAX_VERTEX_UNIFORM_VECTORS ),
			'MAX_FRAGMENT_UNIFORM_VECTORS': _gl.getParameter( _gl.MAX_FRAGMENT_UNIFORM_VECTORS )
		}

		return params;
	};

	function dumpObject( obj ) {

		var p, str = "";
		for ( p in obj ) {

			str += p + ": " + obj[p] + "\n";

		}

		return str;
	}
	*/

};

THREE.Snippets = {
	
	fog_pars_fragment: [

	"#ifdef USE_FOG",

		"uniform vec3 fogColor;",

		"#ifdef FOG_EXP2",
			"uniform float fogDensity;",
		"#else",
			"uniform float fogNear;",
			"uniform float fogFar;",
		"#endif",

	"#endif"

	].join("\n"),

	fog_fragment: [

	"#ifdef USE_FOG",

		"float depth = gl_FragCoord.z / gl_FragCoord.w;",

		"#ifdef FOG_EXP2",
			"const float LOG2 = 1.442695;",
			"float fogFactor = exp2( - fogDensity * fogDensity * depth * depth * LOG2 );",
			"fogFactor = 1.0 - clamp( fogFactor, 0.0, 1.0 );",
		"#else",
			"float fogFactor = smoothstep( fogNear, fogFar, depth );",
		"#endif",

		"gl_FragColor = mix( gl_FragColor, vec4( fogColor, 1.0 ), fogFactor );",

	"#endif"
	
	].join("\n"),
	
	envmap_pars_fragment: [
	
	"#ifdef USE_ENVMAP",
	
		"varying vec3 vReflect;",
		"uniform float reflectivity;",
		"uniform samplerCube env_map;",
		"uniform int combine;",
	
	"#endif"
	
	].join("\n"),
	
	envmap_fragment: [
	
	"#ifdef USE_ENVMAP",

		"cubeColor = textureCube( env_map, vec3( -vReflect.x, vReflect.yz ) );",
		
		"if ( combine == 1 ) {",

			"gl_FragColor = mix( gl_FragColor, cubeColor, reflectivity );",

		"} else {",

			"gl_FragColor = gl_FragColor * cubeColor;",

		"}",	

	"#endif"
	
	].join("\n"),
	
	envmap_pars_vertex: [
	
	"#ifdef USE_ENVMAP",
	
		"varying vec3 vReflect;",
		"uniform float refraction_ratio;",
		"uniform bool useRefract;",
		
	"#endif"
	
	].join("\n"),

	envmap_vertex : [
	
	"#ifdef USE_ENVMAP",
	
		"vec4 mPosition = objectMatrix * vec4( position, 1.0 );",
		"vec3 nWorld = mat3( objectMatrix[0].xyz, objectMatrix[1].xyz, objectMatrix[2].xyz ) * normal;",
	
		"if ( useRefract ) {",

			"vReflect = refract( normalize( mPosition.xyz - cameraPosition ), normalize( nWorld.xyz ), refraction_ratio );",

		"} else {",

			"vReflect = reflect( normalize( mPosition.xyz - cameraPosition ), normalize( nWorld.xyz ) );",

		"}",

	"#endif"
	
	].join("\n"),
	
	map_pars_fragment: [
	
	"#ifdef USE_MAP",
		
		"varying vec2 vUv;",
		"uniform sampler2D map;",
		  
	"#endif"
	
	].join("\n"),
	
	map_pars_vertex: [
	
	"#ifdef USE_MAP",
	
		"varying vec2 vUv;",

	"#endif"
	
	].join("\n"),
	
	map_fragment: [

	"#ifdef USE_MAP",

		"mapColor = texture2D( map, vUv );",

	"#endif"
	
	].join("\n"),
	
	map_vertex: [
	
	"#ifdef USE_MAP",
	
		"vUv = uv;",
		
	"#endif"
	
	].join("\n"),
	
	lights_pars_vertex: [
	
	"uniform bool enableLighting;",
	"uniform vec3 ambientLightColor;",
	
	"#if MAX_DIR_LIGHTS > 0",
	
		"uniform vec3 directionalLightColor[ MAX_DIR_LIGHTS ];",
		"uniform vec3 directionalLightDirection[ MAX_DIR_LIGHTS ];",
		
	"#endif",

	"#if MAX_POINT_LIGHTS > 0",
	
		"uniform vec3 pointLightColor[ MAX_POINT_LIGHTS ];",
		"uniform vec3 pointLightPosition[ MAX_POINT_LIGHTS ];",
		
		"#ifdef PHONG",
			"varying vec3 vPointLightVector[ MAX_POINT_LIGHTS ];",
		"#endif",
		
	"#endif"
	
	].join("\n"),
	
	lights_vertex: [
	
	"if ( !enableLighting ) {",

		"vLightWeighting = vec3( 1.0 );",

	"} else {",

		"vLightWeighting = ambientLightColor;",

		"#if MAX_DIR_LIGHTS > 0",
		
		"for( int i = 0; i < MAX_DIR_LIGHTS; i++ ) {",
		
			"vec4 lDirection = viewMatrix * vec4( directionalLightDirection[ i ], 0.0 );",
			"float directionalLightWeighting = max( dot( transformedNormal, normalize( lDirection.xyz ) ), 0.0 );",
			"vLightWeighting += directionalLightColor[ i ] * directionalLightWeighting;",
			
		"}",
		
		"#endif",

		"#if MAX_POINT_LIGHTS > 0",
		
		"for( int i = 0; i < MAX_POINT_LIGHTS; i++ ) {",
		
			"vec4 lPosition = viewMatrix * vec4( pointLightPosition[ i ], 1.0 );",
			"vec3 pointLightVector = normalize( lPosition.xyz - mvPosition.xyz );",
			"float pointLightWeighting = max( dot( transformedNormal, pointLightVector ), 0.0 );",
			"vLightWeighting += pointLightColor[ i ] * pointLightWeighting;",
			
			"#ifdef PHONG",
				"vPointLightVector[ i ] = pointLightVector;",
			"#endif",
			
		"}",
		
		"#endif",
		
	"}"
	
	].join("\n"),
	
	lights_pars_fragment: [
	
	"#if MAX_DIR_LIGHTS > 0",
		"uniform vec3 directionalLightDirection[ MAX_DIR_LIGHTS ];",
	"#endif",
	
	"#if MAX_POINT_LIGHTS > 0",
		"varying vec3 vPointLightVector[ MAX_POINT_LIGHTS ];",
	"#endif",
	
	"varying vec3 vViewPosition;",
	"varying vec3 vNormal;"
	
	].join("\n"),
	
	lights_fragment: [
	
	"vec3 normal = normalize( vNormal );",
	"vec3 viewPosition = normalize( vViewPosition );",
	
	"vec4 mSpecular = vec4( specular, opacity );",

	"#if MAX_POINT_LIGHTS > 0",
		
		"vec4 pointDiffuse  = vec4( 0.0 );",
		"vec4 pointSpecular = vec4( 0.0 );",

		"for( int i = 0; i < MAX_POINT_LIGHTS; i++ ) {",

			"vec3 pointVector = normalize( vPointLightVector[ i ] );",
			"vec3 pointHalfVector = normalize( vPointLightVector[ i ] + vViewPosition );",

			"float pointDotNormalHalf = dot( normal, pointHalfVector );",
			"float pointDiffuseWeight = max( dot( normal, pointVector ), 0.0 );",

			"float pointSpecularWeight = 0.0;",
			"if ( pointDotNormalHalf >= 0.0 )",
				"pointSpecularWeight = pow( pointDotNormalHalf, shininess );",

			"pointDiffuse  += mColor * pointDiffuseWeight;",
			"pointSpecular += mSpecular * pointSpecularWeight;",

			"}",

	"#endif",
	
	"#if MAX_DIR_LIGHTS > 0",
	
		"vec4 dirDiffuse  = vec4( 0.0 );",
		"vec4 dirSpecular = vec4( 0.0 );" ,

		"for( int i = 0; i < MAX_DIR_LIGHTS; i++ ) {",

			"vec4 lDirection = viewMatrix * vec4( directionalLightDirection[ i ], 0.0 );",

			"vec3 dirVector = normalize( lDirection.xyz );",
			"vec3 dirHalfVector = normalize( lDirection.xyz + vViewPosition );",

			"float dirDotNormalHalf = dot( normal, dirHalfVector );",

			"float dirDiffuseWeight = max( dot( normal, dirVector ), 0.0 );",

			"float dirSpecularWeight = 0.0;",
			"if ( dirDotNormalHalf >= 0.0 )",
				"dirSpecularWeight = pow( dirDotNormalHalf, shininess );",

			"dirDiffuse  += mColor * dirDiffuseWeight;",
			"dirSpecular += mSpecular * dirSpecularWeight;",

		"}",
	
	"#endif",

	"vec4 totalLight = vec4( ambient, opacity );",

	"#if MAX_DIR_LIGHTS > 0",
		"totalLight += dirDiffuse + dirSpecular;",
	"#endif",
	
	"#if MAX_POINT_LIGHTS > 0",
		"totalLight += pointDiffuse + pointSpecular;",
	"#endif"

	].join("\n")

};

THREE.UniformsLib = {
	
	common: {
		
	"color"   : { type: "c", value: new THREE.Color( 0xeeeeee ) },
	"opacity" : { type: "f", value: 1 },
	"map"     : { type: "t", value: 0, texture: null },
	
	"env_map" 		  : { type: "t", value: 1, texture: null },
	"useRefract"	  : { type: "i", value: 0 },
	"reflectivity"    : { type: "f", value: 1 },
	"refraction_ratio": { type: "f", value: 0.98 },
	"combine"		  : { type: "i", value: 0 },
	
	"fogDensity": { type: "f", value: 0.00025 },
	"fogNear"	: { type: "f", value: 1 },
	"fogFar"	: { type: "f", value: 2000 },
	"fogColor"	: { type: "c", value: new THREE.Color( 0xffffff ) }
	
	},
	
	lights: {
		
	"enableLighting" 			: { type: "i", value: 1 },
	"ambientLightColor" 		: { type: "fv", value: [] },
	"directionalLightDirection" : { type: "fv", value: [] },
	"directionalLightColor" 	: { type: "fv", value: [] },
	"pointLightPosition"		: { type: "fv", value: [] },
	"pointLightColor"			: { type: "fv", value: [] }
	
	}
	
};

THREE.ShaderLib = {

	'depth': {

		uniforms: { "mNear": { type: "f", value: 1.0 },
					"mFar" : { type: "f", value: 2000.0 } },

		fragment_shader: [

			"uniform float mNear;",
			"uniform float mFar;",

			"void main() {",

				"float depth = gl_FragCoord.z / gl_FragCoord.w;",
				"float color = 1.0 - smoothstep( mNear, mFar, depth );",
				"gl_FragColor = vec4( vec3( color ), 1.0 );",

			"}"

		].join("\n"),

		vertex_shader: [

			"void main() {",

				"gl_Position = projectionMatrix * modelViewMatrix * vec4( position, 1.0 );",

			"}"

		].join("\n")

	},

	'normal': {

		uniforms: { },

		fragment_shader: [

			"varying vec3 vNormal;",

			"void main() {",

				"gl_FragColor = vec4( 0.5 * normalize( vNormal ) + 0.5, 1.0 );",

			"}"

		].join("\n"),

		vertex_shader: [

			"varying vec3 vNormal;",

			"void main() {",

				"vec4 mvPosition = modelViewMatrix * vec4( position, 1.0 );",
				"vNormal = normalize( normalMatrix * normal );",

				"gl_Position = projectionMatrix * mvPosition;",

			"}"

		].join("\n")

	},

	'basic': {
		
		uniforms: THREE.UniformsLib[ "common" ],
		
		fragment_shader: [

			"uniform vec3 color;",
			"uniform float opacity;",
			
			THREE.Snippets[ "map_pars_fragment" ],
			THREE.Snippets[ "envmap_pars_fragment" ],
			THREE.Snippets[ "fog_pars_fragment" ],
				
			"void main() {",

				"vec4 mColor = vec4( color, opacity );",
				"vec4 mapColor = vec4( 1.0 );",
				"vec4 cubeColor = vec4( 1.0 );",

				THREE.Snippets[ "map_fragment" ],
				
				"gl_FragColor = mColor * mapColor;",
				
				THREE.Snippets[ "envmap_fragment" ],
				THREE.Snippets[ "fog_fragment" ],
				
			"}"

		].join("\n"),
		
		vertex_shader: [
			
			THREE.Snippets[ "map_pars_vertex" ],
			THREE.Snippets[ "envmap_pars_vertex" ],
			
			"void main() {",
		
				"vec4 mvPosition = modelViewMatrix * vec4( position, 1.0 );",
				
				THREE.Snippets[ "map_vertex" ],
				THREE.Snippets[ "envmap_vertex" ],
				
				"gl_Position = projectionMatrix * mvPosition;",

			"}"

		].join("\n")
		
	},

	'lambert': {
		
		uniforms: Uniforms.merge( [ THREE.UniformsLib[ "common" ], 
									THREE.UniformsLib[ "lights" ] ] ),
		
		fragment_shader: [
			
			"uniform vec3 color;",
			"uniform float opacity;",
			
			"varying vec3 vLightWeighting;",
				
			THREE.Snippets[ "map_pars_fragment" ],
			THREE.Snippets[ "envmap_pars_fragment" ],
			THREE.Snippets[ "fog_pars_fragment" ],
				
			"void main() {",
					
				"vec4 mColor = vec4( color, opacity );",
				"vec4 mapColor = vec4( 1.0 );",
				"vec4 cubeColor = vec4( 1.0 );",

				THREE.Snippets[ "map_fragment" ],

				"gl_FragColor =  mColor * mapColor * vec4( vLightWeighting, 1.0 );",
				
				THREE.Snippets[ "envmap_fragment" ],
				THREE.Snippets[ "fog_fragment" ],

			"}"

		].join("\n"),

		vertex_shader: [
		
			"varying vec3 vLightWeighting;",
			
			THREE.Snippets[ "map_pars_vertex" ],
			THREE.Snippets[ "envmap_pars_vertex" ],
			THREE.Snippets[ "lights_pars_vertex" ],
			
			"void main() {",
		
				"vec4 mvPosition = modelViewMatrix * vec4( position, 1.0 );",
				
				THREE.Snippets[ "map_vertex" ],
				THREE.Snippets[ "envmap_vertex" ],
				
				"vec3 transformedNormal = normalize( normalMatrix * normal );",
				
				THREE.Snippets[ "lights_vertex" ],
				
				"gl_Position = projectionMatrix * mvPosition;",

			"}"

		].join("\n")
		
	},
	
	'phong': {
		
		uniforms: Uniforms.merge( [ THREE.UniformsLib[ "common" ], 
									THREE.UniformsLib[ "lights" ],
									
								    { "ambient"  : { type: "c", value: new THREE.Color( 0x050505 ) },
									  "specular" : { type: "c", value: new THREE.Color( 0x111111 ) },
									  "shininess": { type: "f", value: 30 }
									}
									
								] ),
		
		fragment_shader: [
			
			"uniform vec3 color;",
			"uniform float opacity;",
			
			"uniform vec3 ambient;",
			"uniform vec3 specular;",
			"uniform float shininess;",
				
			"varying vec3 vLightWeighting;",
				
			THREE.Snippets[ "map_pars_fragment" ],
			THREE.Snippets[ "envmap_pars_fragment" ],
			THREE.Snippets[ "fog_pars_fragment" ],
			THREE.Snippets[ "lights_pars_fragment" ],
				
			"void main() {",
					
				"vec4 mColor = vec4( color, opacity );",
				"vec4 mapColor = vec4( 1.0 );",
				"vec4 cubeColor = vec4( 1.0 );",

				THREE.Snippets[ "map_fragment" ],
				THREE.Snippets[ "lights_fragment" ],

				"gl_FragColor =  mapColor * totalLight * vec4( vLightWeighting, 1.0 );",
				
				THREE.Snippets[ "envmap_fragment" ],
				THREE.Snippets[ "fog_fragment" ],

			"}"

		].join("\n"),

		vertex_shader: [
		
			"#define PHONG",
			
			"varying vec3 vLightWeighting;",
			"varying vec3 vViewPosition;",
			"varying vec3 vNormal;",
			
			THREE.Snippets[ "map_pars_vertex" ],
			THREE.Snippets[ "envmap_pars_vertex" ],
			THREE.Snippets[ "lights_pars_vertex" ],
			
			"void main() {",
		
				"vec4 mvPosition = modelViewMatrix * vec4( position, 1.0 );",
				
				THREE.Snippets[ "map_vertex" ],
				THREE.Snippets[ "envmap_vertex" ],
				
				"#ifndef USE_ENVMAP",
					"vec4 mPosition = objectMatrix * vec4( position, 1.0 );",
				"#endif",
				
				"vViewPosition = cameraPosition - mPosition.xyz;",
				
				"vec3 transformedNormal = normalize( normalMatrix * normal );",
				"vNormal = transformedNormal;",

				THREE.Snippets[ "lights_vertex" ],
				
				"gl_Position = projectionMatrix * mvPosition;",

			"}"

		].join("\n")
		
	}	

};
