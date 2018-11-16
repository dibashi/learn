var SOILD_VSHADER_SOURCE =
    'attribute vec4 a_Position;\n' +
    'attribute vec4 a_Normal;\n' +
    'uniform mat4 u_MVPMatrix;\n' +
    'uniform mat4 u_NormalMatrix;\n' +
    'varying vec4 v_Color;\n' +
    'void main() {\n' +
    '  gl_Position = u_MVPMatrix * a_Position;\n' +
    '  vec4 color = vec4(0.0,0.5,0.7,1.0);\n' +
    '  vec3 normal = normalize(vec3(u_NormalMatrix * a_Normal));\n' +
    '  vec3 lightDirection = normalize(vec3(0.0,0.0,1.0));\n' +
    '  float nDotL = max(dot(normal,lightDirection),0.0);\n' +
    '  v_Color = vec4(color.rgb * nDotL,color.a);\n' +
    '}\n';
var SOLID_FSHADER_SOURCE =
    'precision mediump float;\n' +
    'varying vec4 v_Color;\n' +
    'void main() {\n' +
    '  gl_FragColor = v_Color;\n' +
    '}\n';

var TEX_VSHADER_SOURCE =
    'attribute vec4 a_Position;\n' +
    'attribute vec4 a_Normal;\n' +
    'attribute vec2 a_TexCoord;\n' +
    'uniform mat4 u_MVPMatrix;\n' +
    'uniform mat4 u_NormalMatrix;\n' +
    'varying vec2 v_TexCoord;\n' +
    'varying float v_NDotL;\n' +
    'void main() {\n' +
    '  gl_Position = u_MVPMatrix * a_Position;\n' +
    '  vec3 lightDirection = normalize(vec3(0.0,0.0,1.0));\n' +
    '  vec3 normal  = normalize(vec3(u_NormalMatrix * a_Normal));\n' +
    '  v_NDotL = max(dot(normal, lightDirection), 0.0);\n' +
    '  v_TexCoord = a_TexCoord;\n' +
    '}\n';
var TEX_FSHADER_SOURCE =
    'precision mediump float;\n' +
    'uniform sampler2D u_Sampler;\n' +
    'varying vec2 v_TexCoord;\n' +
    'varying float v_NDotL;\n' +
    'void main() {\n' +
    '  vec4 color  = texture2D(u_Sampler,v_TexCoord);\n' +
    '  gl_FragColor = vec4(color.rgb * v_NDotL,color.a);\n' +
    '}\n';

function main() {
    var canvas = document.getElementById('webgl');
    var gl = canvas.getContext('webgl');
    //创建所有程序
    var solidProgram = createProgram(gl, SOILD_VSHADER_SOURCE, SOLID_FSHADER_SOURCE);
    var texProgram = createProgram(gl, TEX_VSHADER_SOURCE, TEX_FSHADER_SOURCE);
    //1寻找程序的参数位置 存起来。
    solidProgram.a_PositionLoc = gl.getAttribLocation(solidProgram, 'a_Position');
    solidProgram.a_NormalLoc = gl.getAttribLocation(solidProgram, 'a_Normal');

    solidProgram.u_MVPMatrixLoc = gl.getUniformLocation(solidProgram, 'u_MVPMatrix');
    solidProgram.u_NormalMatrixLoc = gl.getUniformLocation(solidProgram, 'u_NormalMatrix');

    texProgram.a_PositionLoc = gl.getAttribLocation(texProgram, 'a_Position');
    texProgram.a_NormalLoc = gl.getAttribLocation(texProgram, 'a_Normal');
    texProgram.a_TexCoordLoc = gl.getAttribLocation(texProgram, 'a_TexCoord');

    texProgram.u_MVPMatrixLoc = gl.getUniformLocation(texProgram, 'u_MVPMatrix');
    texProgram.u_NormalMatrixLoc = gl.getUniformLocation(texProgram, 'u_NormalMatrix');
    texProgram.u_SamplerLoc = gl.getUniformLocation(texProgram, 'u_Sampler');

    //2 上传顶点数据 位置 法线 纹理坐标 索引
    var cube = initVertexBuffers(gl);
    //3 创建纹理 上传纹理数据
    var texture = initTextures(gl, texProgram);
    //4准备渲染阶段 需要的全局变量和状态（清空的颜色 开启深度检测，剔除等）
    gl.clearColor(0.0, 0.0, 0.0, 1.0);
    gl.enable(gl.DEPTH_TEST);
    gl.enable(gl.CULL_FACE);

    var vpMatrix = new Matrix4();
    setVPMatrix(canvas,vpMatrix);
   
    var currentAngle = 0.0;
    function draw() {
        //设置和清空视图
        resize(gl,vpMatrix);
        gl.viewport(0, 0, gl.canvas.width, gl.canvas.height);
        gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

        //对每个要绘制的物体 调用 gl.useProgram()

        //设置属性
        //设置全局变量
        //设置纹理
        //drawElements
        currentAngle = animate(currentAngle);
        drawSolidCube(gl, solidProgram, cube, vpMatrix, currentAngle);
        drawTexCube(gl, texProgram, cube, vpMatrix, currentAngle, texture);
    };

    var mainloop = function() {
        draw();
        requestAnimationFrame(mainloop);
    }
    mainloop();
}

function setVPMatrix(canvas,vpMatrix) {
    vpMatrix.setPerspective(90, canvas.width / canvas.height, 1.0, 100.0);
    vpMatrix.lookAt(0, 1, 5, 0, 0, 0, 0, 1, 0);
}

var g_MVPMatrix = new Matrix4();
var g_ModelMatrix = new Matrix4();
var g_NormalMatrix = new Matrix4();
function drawCube(gl, program, cube, vpMatrix, x, currentAngle) {
    g_ModelMatrix.setTranslate(x, 0, 0);
    g_ModelMatrix.rotate(20, 1, 0, 0);
    g_ModelMatrix.rotate(currentAngle, 0, 1, 0);
    g_MVPMatrix.set(vpMatrix);
    g_MVPMatrix.multiply(g_ModelMatrix);
    g_NormalMatrix.setInverseOf(g_ModelMatrix).transpose();
    gl.uniformMatrix4fv(program.u_MVPMatrixLoc, false, g_MVPMatrix.elements);
    gl.uniformMatrix4fv(program.u_NormalMatrixLoc, false, g_NormalMatrix.elements);

    gl.drawElements(gl.TRIANGLES, cube.indicesBuffer.length, cube.indicesBuffer.type, 0);
}

function drawSolidCube(gl, program, cube, vpMatrix, currentAngle) {
    gl.useProgram(program);
    initAttributeVariable(gl, cube.positionsBuffer, program.a_PositionLoc);
    initAttributeVariable(gl, cube.normalsBuffer, program.a_NormalLoc);

    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER,cube.indicesBuffer);
    drawCube(gl, program, cube, vpMatrix, -2, currentAngle);
}

function drawTexCube(gl, program, cube, vpMatrix, currentAngle, texture) {
    gl.useProgram(program);
    initAttributeVariable(gl, cube.positionsBuffer, program.a_PositionLoc);
    initAttributeVariable(gl, cube.normalsBuffer, program.a_NormalLoc);
    initAttributeVariable(gl, cube.texCoordsBuffer, program.a_TexCoordLoc);
    gl.activeTexture(gl.TEXTURE0);
    gl.bindTexture(gl.TEXTURE_2D, texture);
    

    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER,cube.indicesBuffer);
    drawCube(gl, program, cube, vpMatrix, 2, currentAngle);
}


function initAttributeVariable(gl, buffer, a_AttributeLoc) {
    gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
    gl.vertexAttribPointer(a_AttributeLoc, buffer.num, buffer.type, false, 0, 0);
    gl.enableVertexAttribArray(a_AttributeLoc);
}

function initVertexBuffers(gl) {
    var positions = new Float32Array([
        1.0, 1.0, 1.0, -1.0, 1.0, 1.0, -1.0, -1.0, 1.0, 1.0, -1.0, 1.0,    // v0-v1-v2-v3 front
        1.0, 1.0, 1.0, 1.0, -1.0, 1.0, 1.0, -1.0, -1.0, 1.0, 1.0, -1.0,    // v0-v3-v4-v5 right
        1.0, 1.0, 1.0, 1.0, 1.0, -1.0, -1.0, 1.0, -1.0, -1.0, 1.0, 1.0,    // v0-v5-v6-v1 up
        -1.0, 1.0, 1.0, -1.0, 1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, 1.0,    // v1-v6-v7-v2 left
        -1.0, -1.0, -1.0, 1.0, -1.0, -1.0, 1.0, -1.0, 1.0, -1.0, -1.0, 1.0,    // v7-v4-v3-v2 down
        1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, 1.0, -1.0, 1.0, 1.0, -1.0     // v4-v7-v6-v5 back
    ]);

    var normals = new Float32Array([
        0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0,     // v0-v1-v2-v3 front
        1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0,     // v0-v3-v4-v5 right
        0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0,     // v0-v5-v6-v1 up
        -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0,     // v1-v6-v7-v2 left
        0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0,     // v7-v4-v3-v2 down
        0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0      // v4-v7-v6-v5 back
    ]);

    var texCoords = new Float32Array([
        1.0, 1.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0,    // v0-v1-v2-v3 front
        0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 1.0, 1.0,    // v0-v3-v4-v5 right
        1.0, 0.0, 1.0, 1.0, 0.0, 1.0, 0.0, 0.0,    // v0-v5-v6-v1 up
        1.0, 1.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0,    // v1-v6-v7-v2 left
        0.0, 0.0, 1.0, 0.0, 1.0, 1.0, 0.0, 1.0,    // v7-v4-v3-v2 down
        0.0, 0.0, 1.0, 0.0, 1.0, 1.0, 0.0, 1.0     // v4-v7-v6-v5 back
    ]);

    var indices = new Uint8Array([
        0, 1, 2, 0, 2, 3,    // front
        4, 5, 6, 4, 6, 7,    // right
        8, 9, 10, 8, 10, 11,    // up
        12, 13, 14, 12, 14, 15,    // left
        16, 17, 18, 16, 18, 19,    // down
        20, 21, 22, 20, 22, 23     // back
    ]);

    var o = new Object();
    o.positionsBuffer = initArrayBufferForLaterUse(gl, positions, 3, gl.FLOAT);
    o.normalsBuffer = initArrayBufferForLaterUse(gl, normals, 3, gl.FLOAT);
    o.texCoordsBuffer = initArrayBufferForLaterUse(gl, texCoords, 2, gl.FLOAT);
    o.indicesBuffer = initElementArrayBufferForLaterUse(gl, indices, gl.UNSIGNED_BYTE);
    o.indicesBuffer.length = indices.length;

    return o;
}

function initTextures(gl, texProgram) {
    var texture = gl.createTexture();

    var image = new Image();
    image.onload = function () {
        gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, 1);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, texture);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGB, gl.RGB, gl.UNSIGNED_BYTE, image);
        
        gl.useProgram(texProgram);
        gl.uniform1i(texProgram.u_SamplerLoc, 0);
        gl.bindTexture(gl.TEXTURE_2D,null);
    }

    image.src = '../resources/7herbs.jpg';

    return texture;
}

function initArrayBufferForLaterUse(gl, data, num, type) {
    var buffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
    gl.bufferData(gl.ARRAY_BUFFER, data, gl.STATIC_DRAW);
    buffer.num = num;
    buffer.type = type;
    gl.bindBuffer(gl.ARRAY_BUFFER, null);
    return buffer;
}

function initElementArrayBufferForLaterUse(gl, data, type) {
    var buffer = gl.createBuffer();
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, buffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, data, gl.STATIC_DRAW);
    buffer.type = type;
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, null);
    return buffer;
}

function resize(gl,vpMatrix) {
    var canvas = gl.canvas;
    if (canvas.width != canvas.clientWidth || canvas.height != canvas.clientHeight) {
        setVPMatrix(canvas,vpMatrix);
        canvas.width = canvas.clientWidth;
        canvas.height = canvas.clientHeight;
    }
}

var g_LastTime = Date.now();
var g_StepSpeed = 30;
function animate(currentAngle) {
    var currentTime = Date.now();
    var dt = currentTime - g_LastTime;
    g_LastTime = currentTime;
    currentAngle += dt * g_StepSpeed / 1000;
    return currentAngle % 360;
}