var SOLID_VSHADER_SOURCE =
    'attribute vec4 a_Position;\n' +
    'attribute vec4 a_Normal;\n' +
    'uniform mat4 u_MVPMatrix;\n' +
    'uniform mat4 u_NormalMatrix;\n' +
    'varying vec4 v_Color;\n' +
    'void main() {\n' +
    '     gl_Position = u_MVPMatrix * a_Position;\n' +
    '     vec3 lightDirection = normalize(vec3(0.0,0.0,1.0));\n' +
    '     vec4 color = vec4(0.0,1.0,1.0,1.0);\n' +
    '     vec3 normal = normalize(vec3(u_NormalMatrix*a_Normal));\n' +
    '     float nDotL = max(dot(normal,lightDirection),0.0);\n' +
    '     v_Color = vec4(color.rgb * nDotL,color.a);\n' +
    '}\n';
var SOLID_FSHADER_SOURCE =
    'precision mediump float;\n' +
    'varying vec4 v_Color;\n' +
    'void main() {\n' +
    '     gl_FragColor = v_Color;\n' +
    '}\n';
var TEXTURE_VSHADER_SOURCE =
    'attribute vec4 a_Position;\n' +
    'attribute vec4 a_Normal;\n' +
    'attribute vec2 a_TexCoord;\n' +
    'uniform mat4 u_MVPMatrix;\n' +
    'uniform mat4 u_NormalMatrix;\n' +
    'varying vec2 v_TexCoord;\n' +
    'varying float v_NDotL;\n' +
    'void main() {\n' +
    '     gl_Position = u_MVPMatrix * a_Position;\n' +
    '     v_TexCoord = a_TexCoord;\n' +
    '     vec3 normal = normalize(vec3(u_NormalMatrix * a_Normal));\n' +
    '     vec3 lightDirection = normalize(vec3(0.0,0.0,1.0));\n' +
    '     v_NDotL = max(dot(normal,lightDirection),0.0);\n' +
    '}\n';
var TEXTURE_FSHADER_SOURCE =
    'precision mediump float;\n' +
    'uniform sampler2D u_Sampler;\n' +
    'varying vec2 v_TexCoord;\n' +
    'varying float v_NDotL;\n' +
    'void main() {\n' +
    '     vec4 color = texture2D(u_Sampler,v_TexCoord);\n' +
    '     gl_FragColor = vec4(color.rgb * v_NDotL,color.a);\n' +
    '}\n';

function main() {
    var canvas = document.getElementById('webgl');
    var gl = getWebGLContext(canvas);

    var solidProgram = createProgram(gl, SOLID_VSHADER_SOURCE, SOLID_FSHADER_SOURCE);
    var textureProgram = createProgram(gl, TEXTURE_VSHADER_SOURCE, TEXTURE_FSHADER_SOURCE);

    solidProgram.a_PositionLoc = gl.getAttribLocation(solidProgram, 'a_Position');
    solidProgram.a_NormalLoc = gl.getAttribLocation(solidProgram, 'a_Normal');
    solidProgram.u_MVPMatrixLoc = gl.getUniformLocation(solidProgram, 'u_MVPMatrix');
    solidProgram.u_NormalMatrixLoc = gl.getUniformLocation(solidProgram, 'u_NormalMatrix');

    textureProgram.a_PositionLoc = gl.getAttribLocation(textureProgram, 'a_Position');
    textureProgram.a_NormalLoc = gl.getAttribLocation(textureProgram, 'a_Normal');
    textureProgram.a_TexCoordLoc = gl.getAttribLocation(textureProgram, 'a_TexCoord');
    textureProgram.u_MVPMatrixLoc = gl.getUniformLocation(textureProgram, 'u_MVPMatrix');
    textureProgram.u_NormalMatrixLoc = gl.getUniformLocation(textureProgram, 'u_NormalMatrix');
    textureProgram.u_SamplerLoc = gl.getUniformLocation(textureProgram, 'u_Sampler');

    var VPMatrix = new Matrix4();
    VPMatrix.setPerspective(30, canvas.width / canvas.height, 1.0, 100);
    VPMatrix.lookAt(0, 0, 15, 0, 0, 0, 0, 1, 0);

    var cube = initCubeData(gl);
    initTextureData(gl,textureProgram);

    gl.clearColor(0.0, 0.0, 0.0, 1.0);
    gl.enable(gl.DEPTH_TEST);
    gl.enable(gl.CULL_FACE);
    var currentAngle = 0;
    var tick = function () {
        currentAngle = animate(currentAngle);
        gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
        drawSolidCube(gl, solidProgram, cube,  currentAngle,VPMatrix);
        drawTextureCube(gl, textureProgram, cube,  currentAngle,VPMatrix);
        requestAnimationFrame(tick);
    }
    tick();
}

var last = Date.now();
var step = 30;//每秒30度
function animate(currentAngle) {
    var now = Date.now();
    var dt = now - last;
    last = now;

    currentAngle += dt * step / 1000;
    return currentAngle % 360;
}

function initCubeData(gl) {
    // Create a cube
    //    v6----- v5
    //   /|      /|
    //  v1------v0|
    //  | |     | |
    //  | |v7---|-|v4
    //  |/      |/
    //  v2------v3
    var positions = new Float32Array([
        1.0, 1.0, 1.0, -1.0, 1.0, 1.0, -1.0, -1.0, 1.0, 1.0, -1.0, 1.0,//0 1 2 3
        1.0, 1.0, 1.0, 1.0, -1.0, 1.0, 1.0, -1.0, -1.0, 1.0, 1.0, -1.0,//0 3 4 5
        1.0, 1.0, 1.0, 1.0, 1.0, -1.0, -1.0, 1.0, -1.0, -1.0, 1.0, 1.0,//0 5 6 1
        -1.0, 1.0, 1.0, -1.0, 1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, 1.0,//1 6 7 2
        -1.0, -1.0, -1.0, 1.0, -1.0, -1.0, 1.0, -1.0, 1.0, -1.0, -1.0, 1.0,//7 4 3 2
        1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, 1.0, -1.0, 1.0, 1.0, -1.0//4 7 6 5

    ]);

    var normals = new Float32Array([
        0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0,
        1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0,
        0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0,
        -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0,
        0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0,
        0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0,
    ]);

    var texCoords = new Float32Array([
        1.0, 1.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0,
        0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 1.0, 1.0,
        1.0, 0.0, 1.0, 1.0, 0.0, 1.0, 0.0, 0.0,
        1.0, 1.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0,
        0.0, 0.0, 1.0, 0.0, 1.0, 1.0, 0.0, 1.0,
        0.0, 0.0, 1.0, 0.0, 1.0, 1.0, 0.0, 1.0
    ]);

    var indices = new Uint8Array([
        0, 1, 2, 0, 2, 3,
        4, 5, 6, 4, 6, 7,
        8, 9, 10, 8, 10, 11,
        12, 13, 14, 12, 14, 15,
        16, 17, 18, 16, 18, 19,
        20, 21, 22, 20, 22, 23
    ]);

    var o = new Object();
    o.positionBuffer = initArrayBufferForLaterUse(gl, positions, 3, gl.FLOAT);
    o.normalBuffer = initArrayBufferForLaterUse(gl, normals, 3, gl.FLOAT);
    o.texCoordBuffer = initArrayBufferForLaterUse(gl, texCoords, 2, gl.FLOAT);

    o.indexBuffer = initElementArrayBufferForLaterUse(gl, indices,gl.UNSIGNED_BYTE);
    o.numIndices = indices.length;

    gl.bindBuffer(gl.ARRAY_BUFFER, null);
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, null);

    return o;
}

function initTextureData(gl, program) {
    var texture = gl.createTexture();
    var image = new Image();
    image.onload = function () {
        gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, 1);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, texture);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGB, gl.RGB, gl.UNSIGNED_BYTE, image);
        gl.useProgram(program);
        gl.uniform1i(program.u_SamplerLoc, 0);
    };
    image.src = './resources/7herbs.jpg';
}

function initArrayBufferForLaterUse(gl, data, num, type) {
    var buffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
    gl.bufferData(gl.ARRAY_BUFFER, data, gl.STATIC_DRAW);
    buffer.num = num;
    buffer.type = type;
    return buffer;
}

function initElementArrayBufferForLaterUse(gl, data,type) {
    var buffer = gl.createBuffer();
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, buffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, data, gl.STATIC_DRAW);

    buffer.type = type;
    return buffer;
}

function drawSolidCube(gl, program, cube,  currentAngle,VPMatrix) {
    gl.useProgram(program);
    initAttributeVariable(gl, program.a_PositionLoc, cube.positionBuffer);
    initAttributeVariable(gl, program.a_NormalLoc, cube.normalBuffer);

    drawBox(gl, program, cube, currentAngle, VPMatrix,-2);
}

var g_MVPMatrix = new Matrix4();
var g_NormalMatrix = new Matrix4();
var g_ModelMatrix = new Matrix4();
function drawBox(gl, program, cube,  currentAngle, VPMatrix,tp) {
   
    g_ModelMatrix.setTranslate(tp,0,0);
    g_ModelMatrix.rotate(currentAngle, 0, 1, 0);
    g_MVPMatrix.set(VPMatrix);
    g_MVPMatrix.multiply(g_ModelMatrix);
    g_NormalMatrix.setInverseOf(g_ModelMatrix).transpose();
    gl.uniformMatrix4fv(program.u_MVPMatrixLoc, false, g_MVPMatrix.elements);
    gl.uniformMatrix4fv(program.u_NormalMatrixLoc, false, g_NormalMatrix.elements);

    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, cube.indexBuffer);
    gl.drawElements(gl.TRIANGLES, cube.numIndices, cube.indexBuffer.type, 0);

}

function initAttributeVariable(gl, a_AttributeLoc, buffer) {
    gl.bindBuffer(gl.ARRAY_BUFFER, buffer);

    gl.vertexAttribPointer(a_AttributeLoc, buffer.num, buffer.type, false, 0, 0);
    gl.enableVertexAttribArray(a_AttributeLoc);
}

function drawTextureCube(gl, program, cube, currentAngle,VPMatrix) {
    gl.useProgram(program);
    initAttributeVariable(gl, program.a_PositionLoc, cube.positionBuffer);
    initAttributeVariable(gl, program.a_NormalLoc, cube.normalBuffer);
    initAttributeVariable(gl, program.a_TexCoordLoc, cube.texCoordBuffer);

    drawBox(gl, program, cube, currentAngle, VPMatrix,2);
}
