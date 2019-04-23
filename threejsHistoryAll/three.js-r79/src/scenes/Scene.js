/**
 * @author mrdoob / http://mrdoob.com/
 */

THREE.Scene = function () {

	//获得Object3D构造函数中定义的所有属性
	THREE.Object3D.call(this);

	//修改 type属性 否则是 Object3D;
	this.type = 'Scene';

	//一下四个是场景独有的属性，在renderer中进行使用
	/**
	 * var background = scene.background;

		if (background === null) {

			glClearColor(_clearColor.r, _clearColor.g, _clearColor.b, _clearAlpha);

		} else if (background instanceof THREE.Color) {

			glClearColor(background.r, background.g, background.b, 1);

		}

		if ( background instanceof THREE.CubeTexture ) {

			backgroundCamera2.projectionMatrix.copy( camera.projectionMatrix );

			backgroundCamera2.matrixWorld.extractRotation( camera.matrixWorld );
			backgroundCamera2.matrixWorldInverse.getInverse( backgroundCamera2.matrixWorld );

			backgroundBoxMesh.material.uniforms[ "tCube" ].value = background;
			backgroundBoxMesh.modelViewMatrix.multiplyMatrices( backgroundCamera2.matrixWorldInverse, backgroundBoxMesh.matrixWorld );

			objects.update( backgroundBoxMesh );

			_this.renderBufferDirect( backgroundCamera2, null, backgroundBoxMesh.geometry, backgroundBoxMesh.material, backgroundBoxMesh, null );

		} else if ( background instanceof THREE.Texture ) {

			backgroundPlaneMesh.material.map = background;

			objects.update( backgroundPlaneMesh );

			_this.renderBufferDirect( backgroundCamera, null, backgroundPlaneMesh.geometry, backgroundPlaneMesh.material, backgroundPlaneMesh, null );

		}

	 */
	this.background = null;
	//该属性可以为场景添加雾化效果
	/**
	 * SpriteMaterial.js 中的 雾化，可以了解下这个参数的使用
	 * if ( fog ) {

			gl.uniform3f( uniforms.fogColor, fog.color.r, fog.color.g, fog.color.b );

			if ( fog instanceof THREE.Fog ) {

				gl.uniform1f( uniforms.fogNear, fog.near );
				gl.uniform1f( uniforms.fogFar, fog.far );

				gl.uniform1i( uniforms.fogType, 1 );
				oldFogType = 1;
				sceneFogType = 1;

			} else if ( fog instanceof THREE.FogExp2 ) {

				gl.uniform1f( uniforms.fogDensity, fog.density );

				gl.uniform1i( uniforms.fogType, 2 );
				oldFogType = 2;
				sceneFogType = 2;

			}

		} else {

			gl.uniform1i( uniforms.fogType, 0 );
			oldFogType = 0;
			sceneFogType = 0;

		}
	 */
	this.fog = null;
	//可强制场景中所有物体使用相同的材质
	/**
	 * if ( scene.overrideMaterial ) {

			var overrideMaterial = scene.overrideMaterial;

			renderObjects( opaqueObjects, camera, fog, overrideMaterial );
			renderObjects( transparentObjects, camera, fog, overrideMaterial );

		} else {

			// opaque pass (front-to-back order)

			state.setBlending( THREE.NoBlending );
			renderObjects( opaqueObjects, camera, fog );

			// transparent pass (back-to-front order)

			renderObjects( transparentObjects, camera, fog );

		}


		var material = overrideMaterial === undefined ? renderItem.material : overrideMaterial;
	 */
	this.overrideMaterial = null;

	//if ( scene.autoUpdate === true ) scene.updateMatrixWorld();
	//在renderer中 自动更新场景中所有物体的世界矩阵
	this.autoUpdate = true; // checked by the renderer

};

THREE.Scene.prototype = Object.create(THREE.Object3D.prototype);
THREE.Scene.prototype.constructor = THREE.Scene;

THREE.Scene.prototype.copy = function (source, recursive) {

	THREE.Object3D.prototype.copy.call(this, source, recursive);

	if (source.background !== null) this.background = source.background.clone();
	if (source.fog !== null) this.fog = source.fog.clone();
	if (source.overrideMaterial !== null) this.overrideMaterial = source.overrideMaterial.clone();

	this.autoUpdate = source.autoUpdate;
	this.matrixAutoUpdate = source.matrixAutoUpdate;

	return this;

};
