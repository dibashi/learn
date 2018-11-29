/**
 * @author mr.doob / http://mrdoob.com/
 */

THREE.CanvasRenderer = function () {

	var _renderList = null,
	_projector = new THREE.Projector(),

	_canvas = document.createElement( 'canvas' ),
	_canvasWidth, _canvasHeight, _canvasWidthHalf, _canvasHeightHalf,
	_context = _canvas.getContext( '2d' ),

	_clearColor = null,
	_clearOpacity = null,

	_contextGlobalAlpha = 1,
	_contextGlobalCompositeOperation = 0,
	_contextStrokeStyle = null,
	_contextFillStyle = null,
	_contextLineWidth = 1,

	_v1, _v2, _v3,
	_v1x, _v1y, _v2x, _v2y, _v3x, _v3y,

	_color = new THREE.Color(),
	_color1 = new THREE.Color(),
	_color2 = new THREE.Color(),
	_color3 = new THREE.Color(),
	_color4 = new THREE.Color(),

	_near, _far,

	_bitmap,
	_uv1x, _uv1y, _uv2x, _uv2y, _uv3x, _uv3y,

	_clipRect = new THREE.Rectangle(),
	_clearRect = new THREE.Rectangle(),
	_bboxRect = new THREE.Rectangle(),

	_enableLighting = false,
	_light = new THREE.Color(),
	_ambientLight = new THREE.Color(),
	_directionalLights = new THREE.Color(),
	_pointLights = new THREE.Color(),

	_pi2 = Math.PI * 2,
	_vector3 = new THREE.Vector3(), // Needed for PointLight

	_pixelMap, _pixelMapContext, _pixelMapImage, _pixelMapData,
	_gradientMap, _gradientMapContext, _gradientMapQuality = 16;

	_pixelMap = document.createElement( 'canvas' );
	_pixelMap.width = _pixelMap.height = 2;

	_pixelMapContext = _pixelMap.getContext( '2d' );
	_pixelMapContext.fillStyle = 'rgba(0,0,0,1)';
	_pixelMapContext.fillRect( 0, 0, 2, 2 );

	_pixelMapImage = _pixelMapContext.getImageData( 0, 0, 2, 2 );
	_pixelMapData = _pixelMapImage.data;

	_gradientMap = document.createElement( 'canvas' );
	_gradientMap.width = _gradientMap.height = _gradientMapQuality;

	_gradientMapContext = _gradientMap.getContext( '2d' );
	_gradientMapContext.translate( - _gradientMapQuality / 2, - _gradientMapQuality / 2 );
	_gradientMapContext.scale( _gradientMapQuality, _gradientMapQuality );

	_gradientMapQuality --; // Fix UVs

	this.domElement = _canvas;

	this.autoClear = true;
	this.sortObjects = true;
	this.sortElements = true;

	this.setSize = function ( width, height ) {

		_canvasWidth = width;
		_canvasHeight = height;
		_canvasWidthHalf = _canvasWidth / 2;
		_canvasHeightHalf = _canvasHeight / 2;

		_canvas.width = _canvasWidth;
		_canvas.height = _canvasHeight;

		_clipRect.set( - _canvasWidthHalf, - _canvasHeightHalf, _canvasWidthHalf, _canvasHeightHalf );

	};

	this.setClearColor = function( hex, opacity ) {

		_clearColor = hex !== null ? new THREE.Color( hex ) : null;
		_clearOpacity = opacity;

		_clearRect.set( - _canvasWidthHalf, - _canvasHeightHalf, _canvasWidthHalf, _canvasHeightHalf );
		_context.setTransform( 1, 0, 0, - 1, _canvasWidthHalf, _canvasHeightHalf );
		this.clear();
	};

	this.clear = function () {

		if ( !_clearRect.isEmpty() ) {

			_clearRect.inflate( 1 );
			_clearRect.minSelf( _clipRect );

			if ( _clearColor !== null ) {

				setBlending( THREE.NormalBlending );
				setOpacity( 1 );

				_context.fillStyle = 'rgba(' + Math.floor( _clearColor.r * 255 ) + ',' + Math.floor( _clearColor.g * 255 ) + ',' + Math.floor( _clearColor.b * 255 ) + ',' + _clearOpacity + ')';
				_context.fillRect( _clearRect.getX(), _clearRect.getY(), _clearRect.getWidth(), _clearRect.getHeight() );

			} else {

				_context.clearRect( _clearRect.getX(), _clearRect.getY(), _clearRect.getWidth(), _clearRect.getHeight() );

			}

			_clearRect.empty();

		}
	};

	this.render = function ( scene, camera ) {

		var e, el, element, m, ml, fm, fml, material;

		_context.setTransform( 1, 0, 0, - 1, _canvasWidthHalf, _canvasHeightHalf );

		this.autoClear && this.clear();

		_renderList = _projector.projectScene( scene, camera, this.sortElements );

		/* DEBUG
		_context.fillStyle = 'rgba( 0, 255, 255, 0.5 )';
		_context.fillRect( _clipRect.getX(), _clipRect.getY(), _clipRect.getWidth(), _clipRect.getHeight() );
		*/

		_enableLighting = scene.lights.length > 0;

		if ( _enableLighting ) {

			 calculateLights( scene );

		}

		for ( e = 0, el = _renderList.length; e < el; e++ ) {

			element = _renderList[ e ];

			_bboxRect.empty();

			if ( element instanceof THREE.RenderableParticle ) {

				_v1 = element;
				_v1.x *= _canvasWidthHalf; _v1.y *= _canvasHeightHalf;

				for ( m = 0, ml = element.materials.length; m < ml; m++ ) {

					renderParticle( _v1, element, element.materials[ m ], scene );

				}

			} else if ( element instanceof THREE.RenderableLine ) {

				_v1 = element.v1; _v2 = element.v2;

				_v1.positionScreen.x *= _canvasWidthHalf; _v1.positionScreen.y *= _canvasHeightHalf;
				_v2.positionScreen.x *= _canvasWidthHalf; _v2.positionScreen.y *= _canvasHeightHalf;

				_bboxRect.addPoint( _v1.positionScreen.x, _v1.positionScreen.y );
				_bboxRect.addPoint( _v2.positionScreen.x, _v2.positionScreen.y );

				if ( _clipRect.instersects( _bboxRect ) ) {

					m = 0; ml = element.materials.length;

					while ( m < ml ) {

						renderLine( _v1, _v2, element, element.materials[ m ++ ], scene );

					}

				}


			} else if ( element instanceof THREE.RenderableFace3 ) {

				_v1 = element.v1; _v2 = element.v2; _v3 = element.v3;

				_v1.positionScreen.x *= _canvasWidthHalf; _v1.positionScreen.y *= _canvasHeightHalf;
				_v2.positionScreen.x *= _canvasWidthHalf; _v2.positionScreen.y *= _canvasHeightHalf;
				_v3.positionScreen.x *= _canvasWidthHalf; _v3.positionScreen.y *= _canvasHeightHalf;

				if ( element.overdraw ) {

					expand( _v1.positionScreen, _v2.positionScreen );
					expand( _v2.positionScreen, _v3.positionScreen );
					expand( _v3.positionScreen, _v1.positionScreen );

				}

				_bboxRect.add3Points( _v1.positionScreen.x, _v1.positionScreen.y,
						      _v2.positionScreen.x, _v2.positionScreen.y,
						      _v3.positionScreen.x, _v3.positionScreen.y );

				if ( _clipRect.instersects( _bboxRect ) ) {

					m = 0; ml = element.meshMaterials.length;

					while ( m < ml ) {

						material = element.meshMaterials[ m ++ ];

						if ( material instanceof THREE.MeshFaceMaterial ) {

							fm = 0; fml = element.faceMaterials.length;

							while ( fm < fml ) {

								material = element.faceMaterials[ fm ++ ];
								material && renderFace3( _v1, _v2, _v3, element, material, scene );

							}

							continue;

						}

						renderFace3( _v1, _v2, _v3, element, material, scene );

					}

				}

			}

			/*
			_context.lineWidth = 1;
			_context.strokeStyle = 'rgba( 0, 255, 0, 0.5 )';
			_context.strokeRect( _bboxRect.getX(), _bboxRect.getY(), _bboxRect.getWidth(), _bboxRect.getHeight() );
			*/

			_clearRect.addRectangle( _bboxRect );


		}

		/* DEBUG
		_context.lineWidth = 1;
		_context.strokeStyle = 'rgba( 255, 0, 0, 0.5 )';
		_context.strokeRect( _clearRect.getX(), _clearRect.getY(), _clearRect.getWidth(), _clearRect.getHeight() );
		*/

		_context.setTransform( 1, 0, 0, 1, 0, 0 );

		//

		function calculateLights( scene ) {

			var l, ll, light, lightColor,
			lights = scene.lights;

			_ambientLight.setRGB( 0, 0, 0 );
			_directionalLights.setRGB( 0, 0, 0 );
			_pointLights.setRGB( 0, 0, 0 );

			for ( l = 0, ll = lights.length; l < ll; l ++ ) {

				light = lights[ l ];
				lightColor = light.color;

				if ( light instanceof THREE.AmbientLight ) {

					_ambientLight.r += lightColor.r;
					_ambientLight.g += lightColor.g;
					_ambientLight.b += lightColor.b;

				} else if ( light instanceof THREE.DirectionalLight ) {

					_directionalLights.r += lightColor.r;
					_directionalLights.g += lightColor.g;
					_directionalLights.b += lightColor.b;

				} else if ( light instanceof THREE.PointLight ) {

					_pointLights.r += lightColor.r;
					_pointLights.g += lightColor.g;
					_pointLights.b += lightColor.b;

				}

			}

		}

		function calculateLight( scene, position, normal, color ) {

			var l, ll, light, lightColor, lightIntensity,
			amount, lights = scene.lights;

			for ( l = 0, ll = lights.length; l < ll; l ++ ) {

				light = lights[ l ];
				lightColor = light.color;
				lightIntensity = light.intensity;

				if ( light instanceof THREE.DirectionalLight ) {

					amount = normal.dot( light.position ) * lightIntensity;

					if ( amount > 0 ) {

						color.r += lightColor.r * amount;
						color.g += lightColor.g * amount;
						color.b += lightColor.b * amount;

					}

				} else if ( light instanceof THREE.PointLight ) {

					_vector3.sub( light.position, position );
					_vector3.normalize();

					amount = normal.dot( _vector3 ) * lightIntensity;

					if ( amount > 0 ) {

						color.r += lightColor.r * amount;
						color.g += lightColor.g * amount;
						color.b += lightColor.b * amount;

					}

				}

			}

		}

		function renderParticle ( v1, element, material, scene ) {

			if ( material.opacity == 0 ) return;

			setOpacity( material.opacity );
			setBlending( material.blending );

			var width, height, scaleX, scaleY,
			bitmap, bitmapWidth, bitmapHeight;

			if ( material instanceof THREE.ParticleBasicMaterial ) {

				if ( material.map ) {

					bitmap = material.map;
					bitmapWidth = bitmap.width >> 1;
					bitmapHeight = bitmap.height >> 1;

					scaleX = element.scale.x * _canvasWidthHalf;
					scaleY = element.scale.y * _canvasHeightHalf;

					width = scaleX * bitmapWidth;
					height = scaleY * bitmapHeight;

					// TODO: Rotations break this...

					_bboxRect.set( v1.x - width, v1.y - height, v1.x  + width, v1.y + height );

					if ( !_clipRect.instersects( _bboxRect ) ) {

						return;

					}

					_context.save();
					_context.translate( v1.x, v1.y );
					_context.rotate( - element.rotation );
					_context.scale( scaleX, - scaleY );
					_context.translate( - bitmapWidth, - bitmapHeight );

					_context.drawImage( bitmap, 0, 0 );

					_context.restore();

				}

				/* DEBUG
				_context.beginPath();
				_context.moveTo( v1.x - 10, v1.y );
				_context.lineTo( v1.x + 10, v1.y );
				_context.moveTo( v1.x, v1.y - 10 );
				_context.lineTo( v1.x, v1.y + 10 );
				_context.closePath();
				_context.strokeStyle = 'rgb(255,255,0)';
				_context.stroke();
				*/

			} else if ( material instanceof THREE.ParticleCircleMaterial ) {

				if ( _enableLighting ) {

					_light.r = _ambientLight.r + _directionalLights.r + _pointLights.r;
					_light.g = _ambientLight.g + _directionalLights.g + _pointLights.g;
					_light.b = _ambientLight.b + _directionalLights.b + _pointLights.b;

					_color.r = material.color.r * _light.r;
					_color.g = material.color.g * _light.g;
					_color.b = material.color.b * _light.b;

					_color.updateStyleString();

				} else {

					_color.__styleString = material.color.__styleString;

				}

				width = element.scale.x * _canvasWidthHalf;
				height = element.scale.y * _canvasHeightHalf;

				_bboxRect.set( v1.x - width, v1.y - height, v1.x + width, v1.y + height );

				if ( !_clipRect.instersects( _bboxRect ) ) {

					return;

				}

				setFillStyle( _color.__styleString );

				_context.save();
				_context.translate( v1.x, v1.y );
				_context.rotate( - element.rotation );
				_context.scale( width, height );

				_context.beginPath();
				_context.arc( 0, 0, 1, 0, _pi2, true );
				_context.closePath();

				_context.fill();
				_context.restore();

			}

		}

		function renderLine( v1, v2, element, material, scene ) {

			if ( material.opacity == 0 ) return;

			setOpacity( material.opacity );
			setBlending( material.blending );

			_context.beginPath();
			_context.moveTo( v1.positionScreen.x, v1.positionScreen.y );
			_context.lineTo( v2.positionScreen.x, v2.positionScreen.y );
			_context.closePath();

			if ( material instanceof THREE.LineBasicMaterial ) {

				_color.__styleString = material.color.__styleString;

				setLineWidth( material.linewidth );
				setStrokeStyle( _color.__styleString );

				_context.stroke();
				_bboxRect.inflate( material.linewidth * 2 );

			}

		}

		function renderFace3( v1, v2, v3, element, material, scene ) {

			if ( material.opacity == 0 ) return;

			setOpacity( material.opacity );
			setBlending( material.blending );

			_v1x = v1.positionScreen.x; _v1y = v1.positionScreen.y;
			_v2x = v2.positionScreen.x; _v2y = v2.positionScreen.y;
			_v3x = v3.positionScreen.x; _v3y = v3.positionScreen.y;

			drawTriangle( _v1x, _v1y, _v2x, _v2y, _v3x, _v3y );

			if ( material instanceof THREE.MeshBasicMaterial ) {

				if ( material.map/* && !material.wireframe*/ ) {

					if ( material.map.image.loaded ) {

						if ( material.map.mapping instanceof THREE.UVMapping ) {

							texturePath( _v1x, _v1y, _v2x, _v2y, _v3x, _v3y, material.map.image, element.uvs[ 0 ].u, element.uvs[ 0 ].v, element.uvs[ 1 ].u, element.uvs[ 1 ].v, element.uvs[ 2 ].u, element.uvs[ 2 ].v );

						}

					}

				} else if ( material.env_map ) {

					if ( material.env_map.image.loaded ) {

						if ( material.env_map.mapping instanceof THREE.SphericalReflectionMapping ) {

							var cameraMatrix = camera.matrix;

							_vector3.copy( element.vertexNormalsWorld[ 0 ] );
							_uv1x = ( _vector3.x * cameraMatrix.n11 + _vector3.y * cameraMatrix.n12 + _vector3.z * cameraMatrix.n13 ) * 0.5 + 0.5;
							_uv1y = - ( _vector3.x * cameraMatrix.n21 + _vector3.y * cameraMatrix.n22 + _vector3.z * cameraMatrix.n23 ) * 0.5 + 0.5;

							_vector3.copy( element.vertexNormalsWorld[ 1 ] );
							_uv2x = ( _vector3.x * cameraMatrix.n11 + _vector3.y * cameraMatrix.n12 + _vector3.z * cameraMatrix.n13 ) * 0.5 + 0.5;
							_uv2y = - ( _vector3.x * cameraMatrix.n21 + _vector3.y * cameraMatrix.n22 + _vector3.z * cameraMatrix.n23 ) * 0.5 + 0.5;

							_vector3.copy( element.vertexNormalsWorld[ 2 ] );
							_uv3x = ( _vector3.x * cameraMatrix.n11 + _vector3.y * cameraMatrix.n12 + _vector3.z * cameraMatrix.n13 ) * 0.5 + 0.5;
							_uv3y = - ( _vector3.x * cameraMatrix.n21 + _vector3.y * cameraMatrix.n22 + _vector3.z * cameraMatrix.n23 ) * 0.5 + 0.5;

							texturePath( _v1x, _v1y, _v2x, _v2y, _v3x, _v3y, material.env_map.image, _uv1x, _uv1y, _uv2x, _uv2y, _uv3x, _uv3y );

						}/* else if ( material.env_map.mapping == THREE.RefractionMapping ) {

						

						}*/

					}

				} else {

					material.wireframe ? strokePath( material.color.__styleString, material.wireframe_linewidth ) : fillPath( material.color.__styleString );

				}

			} else if ( material instanceof THREE.MeshLambertMaterial ) {

				if ( material.map && !material.wireframe ) {

					if ( material.map.mapping instanceof THREE.UVMapping ) {

						texturePath( _v1x, _v1y, _v2x, _v2y, _v3x, _v3y, material.map.image, element.uvs[ 0 ].u, element.uvs[ 0 ].v, element.uvs[ 1 ].u, element.uvs[ 1 ].v, element.uvs[ 2 ].u, element.uvs[ 2 ].v );

					}

					setBlending( THREE.SubtractiveBlending );

				}

				if ( _enableLighting ) {

					if ( !material.wireframe && material.shading == THREE.SmoothShading && element.vertexNormalsWorld.length == 3 ) {

						_color1.r = _color2.r = _color3.r = _ambientLight.r;
						_color1.g = _color2.g = _color3.g = _ambientLight.g;
						_color1.b = _color2.b = _color3.b = _ambientLight.b;

						calculateLight( scene, element.v1.positionWorld, element.vertexNormalsWorld[ 0 ], _color1 );
						calculateLight( scene, element.v2.positionWorld, element.vertexNormalsWorld[ 1 ], _color2 );
						calculateLight( scene, element.v3.positionWorld, element.vertexNormalsWorld[ 2 ], _color3 );

						_color4.r = ( _color2.r + _color3.r ) * 0.5;
						_color4.g = ( _color2.g + _color3.g ) * 0.5;
						_color4.b = ( _color2.b + _color3.b ) * 0.5;

						_bitmap = getGradientTexture( _color1, _color2, _color3, _color4 );

						texturePath( _v1x, _v1y, _v2x, _v2y, _v3x, _v3y, _bitmap, 0, 0, 1, 0, 0, 1 );

					} else {

						_light.r = _ambientLight.r;
						_light.g = _ambientLight.g;
						_light.b = _ambientLight.b;

						calculateLight( scene, element.centroidWorld, element.normalWorld, _light );

						_color.r = material.color.r * _light.r;
						_color.g = material.color.g * _light.g;
						_color.b = material.color.b * _light.b;

						_color.updateStyleString();
						material.wireframe ? strokePath( _color.__styleString, material.wireframe_linewidth ) : fillPath( _color.__styleString );

					} 

				} else {

					material.wireframe ? strokePath( material.color.__styleString, material.wireframe_linewidth ) : fillPath( material.color.__styleString );

				}

			} else if ( material instanceof THREE.MeshDepthMaterial ) {

				/*
				_w = 1 - ( material.__2near / (material.__farPlusNear - element.z * material.__farMinusNear ) );
				_color.setRGB( _w, _w, _w );
				*/

				_near = camera.near;
				_far = camera.far;

				_color1.r = _color1.g = _color1.b = 1 - smoothstep( v1.positionScreen.z, _near, _far );
				_color2.r = _color2.g = _color2.b = 1 - smoothstep( v2.positionScreen.z, _near, _far );
				_color3.r = _color3.g = _color3.b = 1 - smoothstep( v3.positionScreen.z, _near, _far );

				_color4.r = ( _color2.r + _color3.r ) * 0.5;
				_color4.g = ( _color2.g + _color3.g ) * 0.5;
				_color4.b = ( _color2.b + _color3.b ) * 0.5;

				_bitmap = getGradientTexture( _color1, _color2, _color3, _color4 );

				texturePath( _v1x, _v1y, _v2x, _v2y, _v3x, _v3y, _bitmap, 0, 0, 1, 0, 0, 1 );

			} else if ( material instanceof THREE.MeshNormalMaterial ) {

				_color.r = normalToComponent( element.normalWorld.x );
				_color.g = normalToComponent( element.normalWorld.y );
				_color.b = normalToComponent( element.normalWorld.z );
				_color.updateStyleString();

				material.wireframe ? strokePath( _color.__styleString, material.wireframe_linewidth ) : fillPath( _color.__styleString );

			}

		}

		function drawTriangle( x0, y0, x1, y1, x2, y2 ) {

			_context.beginPath();
			_context.moveTo( x0, y0 );
			_context.lineTo( x1, y1 );
			_context.lineTo( x2, y2 );
			_context.lineTo( x0, y0 );
			_context.closePath();

		}

		/*
		function drawQuad( x0, y0, x1, y1, x2, y2, x3, y3 ) {

			_context.beginPath();
			_context.moveTo( x0, y0 );
			_context.lineTo( x1, y1 );
			_context.lineTo( x2, y2 );
			_context.lineTo( x3, y3 );
			_context.lineTo( x0, y0 );
			_context.closePath();

		}
		*/

		function strokePath( color, linewidth ) {

			setStrokeStyle( color );
			setLineWidth( linewidth );

			_context.stroke();

			_bboxRect.inflate( linewidth * 2 );

		}

		function fillPath( color ) {

			setFillStyle( color );
			_context.fill();

		}

		function texturePath( x0, y0, x1, y1, x2, y2, bitmap, u0, v0, u1, v1, u2, v2 ) {

			// http://extremelysatisfactorytotalitarianism.com/blog/?p=2120

			var a, b, c, d, e, f, det,
			width = bitmap.width - 1,
			height = bitmap.height - 1;

			u0 *= width; v0 *= height;
			u1 *= width; v1 *= height;
			u2 *= width; v2 *= height;

			x1 -= x0; y1 -= y0;
			x2 -= x0; y2 -= y0;

			u1 -= u0; v1 -= v0;
			u2 -= u0; v2 -= v0;

			det = 1 / ( u1 * v2 - u2 * v1 ),

			a = ( v2 * x1 - v1 * x2 ) * det,
			b = ( v2 * y1 - v1 * y2 ) * det,
			c = ( u1 * x2 - u2 * x1 ) * det,
			d = ( u1 * y2 - u2 * y1 ) * det,

			e = x0 - a * u0 - c * v0,
			f = y0 - b * u0 - d * v0;

			_context.save();
			_context.transform( a, b, c, d, e, f );
			_context.clip();
			_context.drawImage( bitmap, 0, 0 );
			_context.restore();

		}

		function getGradientTexture( color1, color2, color3, color4 ) {

			// http://mrdoob.com/blog/post/710

			var c1r = ~~ ( color1.r * 255 ), c1g = ~~ ( color1.g * 255 ), c1b = ~~ ( color1.b * 255 ),
			c2r = ~~ ( color2.r * 255 ), c2g = ~~ ( color2.g * 255 ), c2b = ~~ ( color2.b * 255 ),
			c3r = ~~ ( color3.r * 255 ), c3g = ~~ ( color3.g * 255 ), c3b = ~~ ( color3.b * 255 ),
			c4r = ~~ ( color4.r * 255 ), c4g = ~~ ( color4.g * 255 ), c4b = ~~ ( color4.b * 255 );

			_pixelMapData[ 0 ] = c1r < 0 ? 0 : c1r > 255 ? 255 : c1r;
			_pixelMapData[ 1 ] = c1g < 0 ? 0 : c1g > 255 ? 255 : c1g;
			_pixelMapData[ 2 ] = c1b < 0 ? 0 : c1b > 255 ? 255 : c1b;

			_pixelMapData[ 4 ] = c2r < 0 ? 0 : c2r > 255 ? 255 : c2r;
			_pixelMapData[ 5 ] = c2g < 0 ? 0 : c2g > 255 ? 255 : c2g;
			_pixelMapData[ 6 ] = c2b < 0 ? 0 : c2b > 255 ? 255 : c2b;

			_pixelMapData[ 8 ] = c3r < 0 ? 0 : c3r > 255 ? 255 : c3r;
			_pixelMapData[ 9 ] = c3g < 0 ? 0 : c3g > 255 ? 255 : c3g;
			_pixelMapData[ 10 ] = c3b < 0 ? 0 : c3b > 255 ? 255 : c3b;

			_pixelMapData[ 12 ] = c4r < 0 ? 0 : c4r > 255 ? 255 : c4r;
			_pixelMapData[ 13 ] = c4g < 0 ? 0 : c4g > 255 ? 255 : c4g;
			_pixelMapData[ 14 ] = c4b < 0 ? 0 : c4b > 255 ? 255 : c4b;

			_pixelMapContext.putImageData( _pixelMapImage, 0, 0 );
			_gradientMapContext.drawImage( _pixelMap, 0, 0 );

			return _gradientMap;

		}

		function smoothstep( value, min, max ) {

			/*
			if ( value <= min ) return 0;
			if ( value >= max ) return 1;
			*/

			var x = ( value - min ) / ( max - min );
			return x * x * ( 3 - 2 * x );

		}

		function normalToComponent( normal ) {

			var component = ( normal + 1 ) * 0.5;
			return component < 0 ? 0 : ( component > 1 ? 1 : component );

		}

		// Hide anti-alias gaps

		function expand( v1, v2 ) {

			var x = v2.x - v1.x, y =  v2.y - v1.y,
			unit = 1 / Math.sqrt( x * x + y * y );

			x *= unit; y *= unit;

			v2.x += x; v2.y += y;
			v1.x -= x; v1.y -= y;

		}

	};

	// Context cached methods.

	function setOpacity( value ) {

		if ( _contextGlobalAlpha != value ) {

			_context.globalAlpha = _contextGlobalAlpha = value;

		}

	}

	function setBlending( value ) {

		if ( _contextGlobalCompositeOperation != value ) {

			switch ( value ) {

				case THREE.NormalBlending:

					_context.globalCompositeOperation = 'source-over';

					break;

				case THREE.AdditiveBlending:

					_context.globalCompositeOperation = 'lighter';

					break;

				case THREE.SubtractiveBlending:

					_context.globalCompositeOperation = 'darker';

					break;

			}

			_contextGlobalCompositeOperation = value;

		}

	}

	function setLineWidth( value ) {

		if ( _contextLineWidth != value ) {

			_context.lineWidth = _contextLineWidth = value;

		}

	}

	function setStrokeStyle( value ) {

		if ( _contextStrokeStyle != value ) {

			_context.strokeStyle = _contextStrokeStyle  = value;

		}

	}

	function setFillStyle( value ) {

		if ( _contextFillStyle != value ) {

			_context.fillStyle = _contextFillStyle = value;

		}

	}

};
