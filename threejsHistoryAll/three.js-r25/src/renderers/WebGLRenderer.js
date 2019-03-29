/**
 * @author supereggbert / http://www.paulbrunt.co.uk/
 * @author mrdoob / http://mrdoob.com/
 */

THREE.WebGLRenderer = function () {

	var _canvas = document.createElement('canvas'), _gl, _program,
		_modelViewMatrix = new THREE.Matrix4(), _normalMatrix,
		COLORFILL = 0, COLORSTROKE = 1, BITMAP = 2, PHONG = 3; // material constants used in shader

	this.domElement = _canvas;
	this.autoClear = true;

	initGL();
	initProgram();

	this.setSize = function (width, height) {

		_canvas.width = width;
		_canvas.height = height;
		_gl.viewport(0, 0, _canvas.width, _canvas.height);

	};

	this.clear = function () {

		_gl.clear(_gl.COLOR_BUFFER_BIT | _gl.DEPTH_BUFFER_BIT);

	};

	this.setupLights = function (scene) {

		var l, ll, lightColor, lightPosition, lightIntensity, light;

		//这种写法是一个技巧，如果length等于0 就传false 不开启灯光
		//如果length不等于0 就传true 开启灯光
		_gl.uniform1i(_program.enableLighting, scene.lights.length);
		//根据场景中的灯光个数，循环传递uniform变量值
		for (l = 0, ll = scene.lights.length; l < ll; l++) {
			//scene中的光源集合
			light = scene.lights[l];
			//判断光源集合中的光对象 是什么类型 根据类型，向着色器传入相应的数据
			if (light instanceof THREE.AmbientLight) {
				//环境光只有颜色
				lightColor = light.color;
				_gl.uniform3f(_program.ambientColor, lightColor.r, lightColor.g, lightColor.b);

			} else if (light instanceof THREE.DirectionalLight) {
				//平行光 颜色 方向 强度 源码用position代替 变量名不好
				lightColor = light.color;
				lightPosition = light.position;
				lightIntensity = light.intensity;

				_gl.uniform3f(_program.lightingDirection, lightPosition.x, lightPosition.y, lightPosition.z);
				//强度和颜色值预乘了
				_gl.uniform3f(_program.directionalColor, lightColor.r * lightIntensity, lightColor.g * lightIntensity, lightColor.b * lightIntensity);

			} else if (light instanceof THREE.PointLight) {
				//点光源 颜色 位置 强度
				lightColor = light.color;
				lightPosition = light.position;
				lightIntensity = light.intensity;
				_gl.uniform3f(_program.pointPosition, lightPosition.x, lightPosition.y, lightPosition.z);
				//强度和颜色值预乘了
				_gl.uniform3f(_program.pointColor, lightColor.r * lightIntensity, lightColor.g * lightIntensity, lightColor.b * lightIntensity);

			}

		}

	};

	//仔细阅读代码后，发现这是一个性能极低的写法！
	//每帧都要执行渲染 
	//渲染的时候会遍历所有mesh
	//渲染每个mesh的时候要遍历所有的面
	//对于每个面 要传入这么多的数据
	//所有面传完 一个mesh结束 drawElements
	//所有mesh传完 一个场景结束 之前已经传过 Light 和 Matrix数据了
	this.createBuffers = function (object, mf) {

		var f, fl, fi, face, vertexNormals, normal, uv, v1, v2, v3, v4,

			materialFaceGroup = object.materialFaceGroup[mf],

			faceArray = [],
			lineArray = [],

			vertexArray = [],
			normalArray = [],
			uvArray = [],

			vertexIndex = 0;

			//逐面形成数据集合 
			//最终形成三个数组 
			//vertexArray ->position
			//normalArray ->normal
			//uvArray ->uv
		for (f = 0, fl = materialFaceGroup.faces.length; f < fl; f++) {

			fi = materialFaceGroup.faces[f];

			face = object.geometry.faces[fi];
			vertexNormals = face.vertexNormals;
			normal = face.normal;
			uv = object.geometry.uvs[fi];
			//面如果是face3类型 就直接根据顶点索引 取到顶点数据
			//塞入vertexArray
			if (face instanceof THREE.Face3) {
				//object-->geometry-->vertices-->position!!内部数据结构
				v1 = object.geometry.vertices[face.a].position;
				v2 = object.geometry.vertices[face.b].position;
				v3 = object.geometry.vertices[face.c].position;

				vertexArray.push(v1.x, v1.y, v1.z);
				vertexArray.push(v2.x, v2.y, v2.z);
				vertexArray.push(v3.x, v3.y, v3.z);

				//将法线数据插入数组中 若已经有三个 也就是逐顶点发现 对象插入
				if (vertexNormals.length == 3) {

					normalArray.push(vertexNormals[0].x, vertexNormals[0].y, vertexNormals[0].z);
					normalArray.push(vertexNormals[1].x, vertexNormals[1].y, vertexNormals[1].z);
					normalArray.push(vertexNormals[2].x, vertexNormals[2].y, vertexNormals[2].z);

				} else {
					//没有三个 其实是只有一个 还是逐顶点 但是每个顶点的发现一样
					normalArray.push(normal.x, normal.y, normal.z);
					normalArray.push(normal.x, normal.y, normal.z);
					normalArray.push(normal.x, normal.y, normal.z);

				}
				//纹理坐标逐个插入 前提是要有纹理坐标
				if (uv) {

					uvArray.push(uv[0].u, uv[0].v);
					uvArray.push(uv[1].u, uv[1].v);
					uvArray.push(uv[2].u, uv[2].v);

				}
				//顶点索引数组 将来肯定要绑定到 ELEMENTS_ARRAY_BUFFER
				faceArray.push(vertexIndex, vertexIndex + 1, vertexIndex + 2);

				// TODO: don't add lines that already exist (faces sharing edge)
				//这三根线的索引也传入 drawElements的参数必然是gl.LINES 否则不会这么传索引
				lineArray.push(vertexIndex, vertexIndex + 1);
				lineArray.push(vertexIndex, vertexIndex + 2);
				lineArray.push(vertexIndex + 1, vertexIndex + 2);
				//一次用三个顶点索引
				vertexIndex += 3;

			} 
			//面由四个顶点组成，某些情况下这种数据结构更好用
			//要划分为两个三角形(a,b,c,d)-->(a,b,c) (a,c,d);
			else if (face instanceof THREE.Face4) {

				v1 = object.geometry.vertices[face.a].position;
				v2 = object.geometry.vertices[face.b].position;
				v3 = object.geometry.vertices[face.c].position;
				v4 = object.geometry.vertices[face.d].position;

				vertexArray.push(v1.x, v1.y, v1.z);
				vertexArray.push(v2.x, v2.y, v2.z);
				vertexArray.push(v3.x, v3.y, v3.z);
				vertexArray.push(v4.x, v4.y, v4.z);

				//法线数据插入
				if (vertexNormals.length == 4) {

					normalArray.push(vertexNormals[0].x, vertexNormals[0].y, vertexNormals[0].z);
					normalArray.push(vertexNormals[1].x, vertexNormals[1].y, vertexNormals[1].z);
					normalArray.push(vertexNormals[2].x, vertexNormals[2].y, vertexNormals[2].z);
					normalArray.push(vertexNormals[3].x, vertexNormals[3].y, vertexNormals[3].z);

				} else {

					normalArray.push(normal.x, normal.y, normal.z);
					normalArray.push(normal.x, normal.y, normal.z);
					normalArray.push(normal.x, normal.y, normal.z);
					normalArray.push(normal.x, normal.y, normal.z);

				}
				//uv
				if (uv) {

					uvArray.push(uv[0].u, uv[0].v);
					uvArray.push(uv[1].u, uv[1].v);
					uvArray.push(uv[2].u, uv[2].v);
					uvArray.push(uv[3].u, uv[3].v);

				}
				//和我上方预测的方法一致
				faceArray.push(vertexIndex, vertexIndex + 1, vertexIndex + 2);
				faceArray.push(vertexIndex, vertexIndex + 2, vertexIndex + 3);

				// TODO: don't add lines that already exist (faces sharing edge)
				//四边形 一共5根线 边框和对角线
				//对角线一定要注意 是上方两个三角形的分界线
				lineArray.push(vertexIndex, vertexIndex + 1);
				lineArray.push(vertexIndex, vertexIndex + 2); //对角线
				lineArray.push(vertexIndex, vertexIndex + 3);
				lineArray.push(vertexIndex + 1, vertexIndex + 2);
				lineArray.push(vertexIndex + 2, vertexIndex + 3);
				//一定要+4 一次用4个顶点索引
				vertexIndex += 4;
			}
		}

		//什么也没有就不会传了，如果我来写，这部分我是必然忘掉判断的
		//似乎不会造成问题，但是性能上肯定很烂
		if (!vertexArray.length) {

			return;

		}

		//传入position
		materialFaceGroup.__webGLVertexBuffer = _gl.createBuffer();
		_gl.bindBuffer(_gl.ARRAY_BUFFER, materialFaceGroup.__webGLVertexBuffer);
		_gl.bufferData(_gl.ARRAY_BUFFER, new Float32Array(vertexArray), _gl.STATIC_DRAW);
		//传入normal
		materialFaceGroup.__webGLNormalBuffer = _gl.createBuffer();
		_gl.bindBuffer(_gl.ARRAY_BUFFER, materialFaceGroup.__webGLNormalBuffer);
		_gl.bufferData(_gl.ARRAY_BUFFER, new Float32Array(normalArray), _gl.STATIC_DRAW);
		//传入uv
		materialFaceGroup.__webGLUVBuffer = _gl.createBuffer();
		_gl.bindBuffer(_gl.ARRAY_BUFFER, materialFaceGroup.__webGLUVBuffer);
		_gl.bufferData(_gl.ARRAY_BUFFER, new Float32Array(uvArray), _gl.STATIC_DRAW);
		//传入索引 ELEMENT_ARRAY_BUFFER
		materialFaceGroup.__webGLFaceBuffer = _gl.createBuffer();
		_gl.bindBuffer(_gl.ELEMENT_ARRAY_BUFFER, materialFaceGroup.__webGLFaceBuffer);
		_gl.bufferData(_gl.ELEMENT_ARRAY_BUFFER, new Uint16Array(faceArray), _gl.STATIC_DRAW);
		//若还打算画上线框 那么 线的索引也要传入
		materialFaceGroup.__webGLLineBuffer = _gl.createBuffer();
		_gl.bindBuffer(_gl.ELEMENT_ARRAY_BUFFER, materialFaceGroup.__webGLLineBuffer);
		_gl.bufferData(_gl.ELEMENT_ARRAY_BUFFER, new Uint16Array(lineArray), _gl.STATIC_DRAW);
		//数组长度 drawElements要使用
		materialFaceGroup.__webGLFaceCount = faceArray.length;
		materialFaceGroup.__webGLLineCount = lineArray.length;

	};

	this.renderBuffer = function (material, materialFaceGroup) {
		//如果材质类型是phong类型 传入phong类型所需的变量
		if (material instanceof THREE.MeshPhongMaterial) {

			mAmbient = material.ambient;
			mDiffuse = material.diffuse;
			mSpecular = material.specular;

			_gl.uniform4f(_program.mAmbient, mAmbient.r, mAmbient.g, mAmbient.b, material.opacity);
			_gl.uniform4f(_program.mDiffuse, mDiffuse.r, mDiffuse.g, mDiffuse.b, material.opacity);
			_gl.uniform4f(_program.mSpecular, mSpecular.r, mSpecular.g, mSpecular.b, material.opacity);

			_gl.uniform1f(_program.mShininess, material.shininess);
			//修改 着色器 条件控制变量
			_gl.uniform1i(_program.material, PHONG);

		}
		//如果材质类型是单颜色填充
		else if (material instanceof THREE.MeshColorFillMaterial) {

			color = material.color;
			//怪怪怪！ 为什么要提前乘上alpha？？？
			_gl.uniform4f(_program.uniformColor, color.r * color.a, color.g * color.a, color.b * color.a, color.a);
			//修改 着色器 条件控制变量
			_gl.uniform1i(_program.material, COLORFILL);

		}

		else if (material instanceof THREE.MeshColorStrokeMaterial) {

			lineWidth = material.lineWidth;

			color = material.color;
			_gl.uniform4f(_program.uniformColor, color.r * color.a, color.g * color.a, color.b * color.a, color.a);
			//修改 着色器 条件控制变量
			_gl.uniform1i(_program.material, COLORSTROKE);

		}
		//材质类型是一个纹理
		else if (material instanceof THREE.MeshBitmapMaterial) {
			//很重要的 一个判断 是解开迷雾的关键
			//我找了半天没有找到loaded变量的声明
			//可以确定的是 肯定是在图片加载到之后才将此变量置为true
			//意味着 已经异步加载到了数据
			//__webGLTexture 这个东西 就是createTexture()返回的纹理缓冲
			//关键在与作者的写法很精妙
			//考虑以下细节 渲染一个有纹理的面
			//该纹理是否下载好？ok下载好了才绘制，只有下载好了 才在gpu中创建缓冲
			//所以 如果已经有了缓冲 就必然下载好了下面的代码就不会执行 
			//意味着如下代码只会执行一次。
			if (!material.__webGLTexture && material.loaded) {
				//纹理加载好以后的第一次设置
				//创建缓冲 绑定到目标 之后对目标进行操作
				//传数据texImage2D
				//配参数 texParameteri
				//mipmap 目前我还没研究过
				//解除目标
				material.__webGLTexture = _gl.createTexture();
				_gl.bindTexture(_gl.TEXTURE_2D, material.__webGLTexture);
				_gl.texImage2D(_gl.TEXTURE_2D, 0, _gl.RGBA, _gl.RGBA, _gl.UNSIGNED_BYTE, material.bitmap);
				_gl.texParameteri(_gl.TEXTURE_2D, _gl.TEXTURE_MAG_FILTER, _gl.LINEAR);
				//_gl.texParameteri( _gl.TEXTURE_2D, _gl.TEXTURE_MIN_FILTER, _gl.LINEAR_MIPMAP_NEAREST );
				_gl.texParameteri(_gl.TEXTURE_2D, _gl.TEXTURE_MIN_FILTER, _gl.LINEAR_MIPMAP_LINEAR);
				_gl.generateMipmap(_gl.TEXTURE_2D);
				_gl.bindTexture(_gl.TEXTURE_2D, null);

			}
			//激活纹理单元 将纹理数据绑定到目标，内部会和纹理单元关联
			//所以只用传入纹理单元的编号 即0
			_gl.activeTexture(_gl.TEXTURE0);
			_gl.bindTexture(_gl.TEXTURE_2D, material.__webGLTexture);
			_gl.uniform1i(_program.diffuse, 0);
			//修改 着色器 条件控制变量
			_gl.uniform1i(_program.material, BITMAP);

		}

		//到目前 需要对以上代码总结一下：
		//可以看到 要么是phong 要么是纹理等等
		//如果一个物体 既有纹理 又使用phong着色呢？
		//所以作者将mesh的material设置为了一个数组
		//在渲染一个物体的时候 会遍历它的material数组 然后组合在一起 所以本函数必然是在
		//一个循环之中的。
		// vertices
		_gl.bindBuffer(_gl.ARRAY_BUFFER, materialFaceGroup.__webGLVertexBuffer);
		_gl.vertexAttribPointer(_program.position, 3, _gl.FLOAT, false, 0, 0);

		// normals
		_gl.bindBuffer(_gl.ARRAY_BUFFER, materialFaceGroup.__webGLNormalBuffer);
		_gl.vertexAttribPointer(_program.normal, 3, _gl.FLOAT, false, 0, 0);

		// uvs
		if (material instanceof THREE.MeshBitmapMaterial) {

			_gl.bindBuffer(_gl.ARRAY_BUFFER, materialFaceGroup.__webGLUVBuffer);

			_gl.enableVertexAttribArray(_program.uv);
			_gl.vertexAttribPointer(_program.uv, 2, _gl.FLOAT, false, 0, 0);

		} else {

			_gl.disableVertexAttribArray(_program.uv);

		}

		// render triangles
		if (material instanceof THREE.MeshBitmapMaterial ||

			material instanceof THREE.MeshColorFillMaterial ||
			material instanceof THREE.MeshPhongMaterial) {

			_gl.bindBuffer(_gl.ELEMENT_ARRAY_BUFFER, materialFaceGroup.__webGLFaceBuffer);
			_gl.drawElements(_gl.TRIANGLES, materialFaceGroup.__webGLFaceCount, _gl.UNSIGNED_SHORT, 0);

			// render lines
		} else if (material instanceof THREE.MeshColorStrokeMaterial) {

			_gl.lineWidth(lineWidth);
			_gl.bindBuffer(_gl.ELEMENT_ARRAY_BUFFER, materialFaceGroup.__webGLLineBuffer);
			_gl.drawElements(_gl.LINES, materialFaceGroup.__webGLLineCount, _gl.UNSIGNED_SHORT, 0);

		}

	};

	//不看代码 先分析一下 这应该是最终的绘制过程
	//object即为mesh，因为引擎目前仅支持绘制mesh
	//mesh是什么？有集合数据geometry （顶点坐标，面集合其实就是顶点索引，纹理坐标，法线）
	//有材质数据 material（目前并没有基类）似乎是渲染模式 颜色 纹理的集合
	//要绘制一个mesh 就是传入数据 然后drawArrays 或者drawElements
	//传什么 如何传？
	//传上述的所有数据
	//bufferData传 用vertexAttribPointer来设置参数即可
	this.renderMesh = function (object, camera) {

		var i, l, m, ml, mf, material, meshMaterial, materialFaceGroup;

		// create separate VBOs per material
		for (mf in object.materialFaceGroup) {

			materialFaceGroup = object.materialFaceGroup[mf];

			// initialise on the first access
			//非常关键的性能优化方案，第一次传数据的时候 这个变量是未定义的
			//经过createBuffers后 已经 传过数据了 之后就再也不会传了
			if (!materialFaceGroup.__webGLVertexBuffer) {

				this.createBuffers(object, mf);

			}
			//上方代码确保数据已经传入 接下来绘制出来
			for (m = 0, ml = object.material.length; m < ml; m++) {

				meshMaterial = object.material[m];

				if (meshMaterial instanceof THREE.MeshFaceMaterial) {

					for (i = 0, l = materialFaceGroup.material.length; i < l; i++) {

						material = materialFaceGroup.material[i];
						this.renderBuffer(material, materialFaceGroup);

					}

				} else {

					material = meshMaterial;
					this.renderBuffer(material, materialFaceGroup);

				}

			}

		}

	};

	//object内有模型矩阵 camera里有视图矩阵和投影矩阵 三个矩阵就是数据源
	//该函数就是利用这三个矩阵（其实代码是两个，本质上只需要一个模型矩阵的 逆转置矩阵即可）
	// 计算出 法线矩阵 然后传入着色器
	this.setupMatrices = function (object, camera) {

		object.autoUpdateMatrix && object.updateMatrix();

		_modelViewMatrix.multiply(camera.matrix, object.matrix);

		_program.viewMatrixArray = new Float32Array(camera.matrix.flatten());
		_program.modelViewMatrixArray = new Float32Array(_modelViewMatrix.flatten());
		_program.projectionMatrixArray = new Float32Array(camera.projectionMatrix.flatten());

		//_normalMatrix = THREE.Matrix4.makeInvert3x3( object.matrix ).transpose();
		/**
		 * 这里可能有bug 光是在世界坐标系下定义的，这里的转置逆矩阵 是视图矩阵的转置逆矩阵
		 * 我觉得应该是 object.matrix的转置逆矩阵 不知作者为何把我认为正确的给注释掉了
		 * 或许是因为光不是在世界坐标系下定义的 又或者其他 源码还没怎么看。
		 */

		_normalMatrix = THREE.Matrix4.makeInvert3x3(_modelViewMatrix).transpose();
		_program.normalMatrixArray = new Float32Array(_normalMatrix.m);

		_gl.uniformMatrix4fv(_program.viewMatrix, false, _program.viewMatrixArray);
		_gl.uniformMatrix4fv(_program.modelViewMatrix, false, _program.modelViewMatrixArray);
		_gl.uniformMatrix4fv(_program.projectionMatrix, false, _program.projectionMatrixArray);
		_gl.uniformMatrix3fv(_program.normalMatrix, false, _program.normalMatrixArray);
		_gl.uniformMatrix4fv(_program.objMatrix, false, new Float32Array(object.matrix.flatten()));

	};

	this.render = function (scene, camera) {

		var o, ol, object;

		//渲染之前 先清空颜色、深度缓冲区
		if (this.autoClear) {

			this.clear();

		}
		//更新相机矩阵
		camera.autoUpdateMatrix && camera.updateMatrix();
		//相机数据 传入着色器
		_gl.uniform3f(_program.cameraPosition, camera.position.x, camera.position.y, camera.position.z);
		//将光源数据传入着色器 有个奇怪的感觉，光源传入的都是uniform变量的数据，有必要每帧传一次？
		//每帧都在传入相同的数据，可能这是低版本的threejs源码 但是我总觉得怪怪的。
		//光源内部维护了一个intensity变量 在传入着色器前 进行了预乘，为什么不直接删去这个变量，
		//把他们合在一起当做一个颜色值呢？难道是为了控制？可能吧
		this.setupLights(scene);
		//一个物体 一个着色器 还是 分类 一批物体一个着色器，若一批用一个着色器 要跟换着色器里的数据
		for (o = 0, ol = scene.objects.length; o < ol; o++) {

			object = scene.objects[o];
			//将所有矩阵数据 传入着色器
			this.setupMatrices(object, camera);
			//到目前 几乎所有的uniform变量都已经传入 就差物体自身的attribute变量了，该如何传入，如何切换 就是下面代码要做的
			//目前的源码，Object3D类下有三个子类 Object3D本质上只维护模型矩阵
			//三个子类的差别较大 需要根据类型 来分别传入数据
			if (object instanceof THREE.Mesh) {

				this.renderMesh(object, camera);

			}
			//线和点 作者还没实现 我说为什么画线的例子我把canvas切换为webgl绘制模式无法显示
			else if (object instanceof THREE.Line) {

				// TODO

				// It would be very inefficient to do lines one-by-one.

				// This will need a complete redesign from how CanvasRenderer does it.

				// Though it could be brute forced, if only used for lightweight
				// stuff (as CanvasRenderer can only handle small number of elements 
				// anyways). 

				// Heavy-duty wireframe lines are handled efficiently in mesh renderer.

			} else if (object instanceof THREE.Particle) {

				// TODO

				// The same as with lines, particles shouldn't be handled one-by-one.

				// Again, heavy duty particle system would require different approach,
				// like one VBO per particle system and then update attribute arrays, 
				// though the best would be to move also behavior computation
				// into the shader (ala http://spidergl.org/example.php?id=11)

			}

		}

	};

	function initGL() {

		try {

			_gl = _canvas.getContext('experimental-webgl', { antialias: true });

		} catch (e) { }

		if (!_gl) {

			alert("WebGL not supported");
			throw "cannot create webgl context";

		}

		_gl.clearColor(0, 0, 0, 1);
		_gl.clearDepth(1);

		_gl.enable(_gl.DEPTH_TEST);
		_gl.depthFunc(_gl.LEQUAL);

		_gl.enable(_gl.BLEND);
		//_gl.blendFunc( _gl.SRC_ALPHA, _gl.ONE_MINUS_SRC_ALPHA );
		// _gl.blendFunc( _gl.SRC_ALPHA, _gl.ONE ); // cool!
		_gl.blendFunc(_gl.ONE, _gl.ONE_MINUS_SRC_ALPHA);
		_gl.clearColor(0, 0, 0, 0);

	}

	function initProgram() {

		_program = _gl.createProgram();
		//作者似乎想用一个着色器就实现绘制所有物体？这种方法固然有其优点，不用来回切换着色器，性能上会有优势，但是灵活性如何保证？
		//目前我觉得要是加入新的着色器 代码写起来感觉蛮难的，因为这个脚本所有的一切耦合的都比较紧。（别的函数已经用到了这个着色器内部的变量了）
		_gl.attachShader(_program, getShader("fragment", [

			"#ifdef GL_ES",
			"precision highp float;",
			"#endif",

			"uniform sampler2D diffuse;",
			"uniform vec4 uniformColor;",

			"varying vec2 vertexUv;",
			"varying vec3 lightWeighting;",
			"varying vec3 vNormal;",
			"uniform int material;", // 0 - ColorFill, 1 - ColorStroke, 2 - Bitmap, 3 - Phong

			"uniform vec4 mAmbient;",
			"uniform vec4 mDiffuse;",
			"uniform vec4 mSpecular;",
			"uniform float mShininess;",

			"varying vec3 pLightVectorPoint;",
			"varying vec3 pLightVectorDirection;",
			"varying vec3 pViewPosition;",

			"void main(){",

			// Blinn-Phong
			// based on o3d example
			"if(material==3) { ",

			"vec3 lightVectorPoint = normalize(pLightVectorPoint);",
			"vec3 lightVectorDir = normalize(pLightVectorDirection);",

			"vec3 normal = normalize(vNormal);",
			"vec3 viewPosition = normalize(pViewPosition);",

			"vec3 halfVectorPoint = normalize(pLightVectorPoint + pViewPosition);",

			"float dotNormalHalfPoint = dot(normal, halfVectorPoint);",

			"float ambientCompPoint = 1.0;",
			"float diffuseCompPoint = max(dot(normal, lightVectorPoint), 0.0);",
			"float specularCompPoint = pow(dotNormalHalfPoint, mShininess);",
			//"float specularCompPoint = dot(normal, lightVectorPoint) < 0.0 || dotNormalHalfPoint < 0.0 ? 0.0 : pow(dotNormalHalfPoint, mShininess);",

			"vec4 ambientPoint  = mAmbient * ambientCompPoint;",
			"vec4 diffusePoint  = mDiffuse * diffuseCompPoint;",
			"vec4 specularPoint = mSpecular * specularCompPoint;",

			"vec3 halfVectorDir = normalize(pLightVectorDirection + pViewPosition);",

			"float dotNormalHalfDir = dot(normal, halfVectorDir);",

			"float ambientCompDir = 1.0;",
			"float diffuseCompDir = max(dot(normal, lightVectorDir), 0.0);",
			"float specularCompDir = pow(dotNormalHalfDir, mShininess);",

			"vec4 ambientDir  = mAmbient * ambientCompDir;",
			"vec4 diffuseDir  = mDiffuse * diffuseCompDir;",
			"vec4 specularDir = mSpecular * specularCompDir;",

			"vec4 pointLight = ambientPoint + diffusePoint + specularPoint;",
			"vec4 dirLight = ambientDir + diffuseDir + specularDir;",

			"gl_FragColor = vec4((pointLight.xyz + dirLight.xyz) * lightWeighting, 1.0);",

			// Bitmap: texture
			"} else if(material==2) {",

			"vec4 texelColor = texture2D(diffuse, vertexUv);",
			"gl_FragColor = vec4(texelColor.rgb * lightWeighting, texelColor.a);",

			// ColorStroke: wireframe using uniform color
			"} else if(material==1) {",

			"gl_FragColor = vec4(uniformColor.rgb * lightWeighting, uniformColor.a);",

			// ColorFill: triangle using uniform color
			"} else {",

			"gl_FragColor = vec4(uniformColor.rgb * lightWeighting, uniformColor.a);",
			//"gl_FragColor = vec4(vNormal, 1.0);",
			"}",

			"}"].join("\n")));

		_gl.attachShader(_program, getShader("vertex", [

			"attribute vec3 position;",
			"attribute vec3 normal;",
			"attribute vec2 uv;",

			"uniform bool enableLighting;",
			"uniform vec3 ambientColor;",
			"uniform vec3 directionalColor;",
			"uniform vec3 lightingDirection;",

			"uniform vec3 pointColor;",
			"uniform vec3 pointPosition;",

			"uniform mat4 objMatrix;",
			"uniform mat4 viewMatrix;",
			"uniform mat4 modelViewMatrix;",
			"uniform mat4 projectionMatrix;",
			"uniform mat3 normalMatrix;",

			"varying vec4 vertexColor;",
			"varying vec2 vertexUv;",
			"varying vec3 lightWeighting;",

			"varying vec3 vNormal;",

			"varying vec3 pLightVectorPoint;",
			"varying vec3 pLightVectorDirection;",
			"varying vec3 pViewPosition;",

			"uniform vec3 cameraPosition;",

			"void main(void) {",

			"vec4 mvPosition = modelViewMatrix * vec4( position, 1.0 );",
			"vec3 transformedNormal = normalize(normalMatrix * normal);",

			// Blinn-Phong
			"vec4 lPosition = viewMatrix * vec4( pointPosition, 1.0 );",
			"vec4 lDirection = viewMatrix * vec4( lightingDirection, 0.0 );",

			"pLightVectorPoint = normalize(pointPosition.xyz - position.xyz);",
			"pLightVectorDirection = normalize(lDirection.xyz);",

			"vec4 mPosition = objMatrix * vec4( position, 1.0 );",
			"pViewPosition = cameraPosition - mPosition.xyz;",

			"if(!enableLighting) {",

			"lightWeighting = vec3(1.0, 1.0, 1.0);",

			"} else {",

			"vec3 pointLight = normalize(lPosition.xyz - mvPosition.xyz);",
			"float directionalLightWeighting = max(dot(transformedNormal, normalize(lDirection.xyz)), 0.0);",
			"float pointLightWeighting = max(dot(transformedNormal, pointLight), 0.0);",
			"lightWeighting = ambientColor + directionalColor * directionalLightWeighting + pointColor * pointLightWeighting;",

			"}",

			"vNormal = transformedNormal;",
			"vertexUv = uv;",

			"gl_Position = projectionMatrix * mvPosition;",

			"}"].join("\n")));

		_gl.linkProgram(_program);

		if (!_gl.getProgramParameter(_program, _gl.LINK_STATUS)) {

			alert("Could not initialise shaders");

		}

		_gl.useProgram(_program);

		_program.viewMatrix = _gl.getUniformLocation(_program, "viewMatrix");
		_program.modelViewMatrix = _gl.getUniformLocation(_program, "modelViewMatrix");
		_program.projectionMatrix = _gl.getUniformLocation(_program, "projectionMatrix");
		_program.normalMatrix = _gl.getUniformLocation(_program, "normalMatrix");
		_program.objMatrix = _gl.getUniformLocation(_program, "objMatrix");

		_program.enableLighting = _gl.getUniformLocation(_program, 'enableLighting');
		_program.ambientColor = _gl.getUniformLocation(_program, 'ambientColor');
		_program.directionalColor = _gl.getUniformLocation(_program, 'directionalColor');
		_program.lightingDirection = _gl.getUniformLocation(_program, 'lightingDirection');

		_program.pointColor = _gl.getUniformLocation(_program, 'pointColor');
		_program.pointPosition = _gl.getUniformLocation(_program, 'pointPosition');

		_program.material = _gl.getUniformLocation(_program, 'material');
		_program.uniformColor = _gl.getUniformLocation(_program, 'uniformColor');

		_program.mAmbient = _gl.getUniformLocation(_program, 'mAmbient');
		_program.mDiffuse = _gl.getUniformLocation(_program, 'mDiffuse');
		_program.mSpecular = _gl.getUniformLocation(_program, 'mSpecular');
		_program.mShininess = _gl.getUniformLocation(_program, 'mShininess');

		_program.cameraPosition = _gl.getUniformLocation(_program, 'cameraPosition');

		_program.position = _gl.getAttribLocation(_program, "position");
		_gl.enableVertexAttribArray(_program.position);

		_program.normal = _gl.getAttribLocation(_program, "normal");
		_gl.enableVertexAttribArray(_program.normal);

		_program.uv = _gl.getAttribLocation(_program, "uv");
		_gl.enableVertexAttribArray(_program.uv);

		_program.diffuse = _gl.getUniformLocation(_program, "diffuse");
		_gl.uniform1i(_program.diffuse, 0);

		_program.viewMatrixArray = new Float32Array(16);
		_program.modelViewMatrixArray = new Float32Array(16);
		_program.projectionMatrixArray = new Float32Array(16);

	}

	function getShader(type, string) {

		var shader;

		if (type == "fragment") {

			shader = _gl.createShader(_gl.FRAGMENT_SHADER);

		} else if (type == "vertex") {

			shader = _gl.createShader(_gl.VERTEX_SHADER);

		}

		_gl.shaderSource(shader, string);
		_gl.compileShader(shader);

		if (!_gl.getShaderParameter(shader, _gl.COMPILE_STATUS)) {

			alert(_gl.getShaderInfoLog(shader));
			return null;

		}

		return shader;
	}

};
