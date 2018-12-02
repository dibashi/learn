
var v_shader =
    'attribute vec4 a_Position;\n' +
    'attribute vec4 a_Color;\n' +
    'uniform mat4 u_MvpMatrix;\n' +
   
    'uniform vec4 u_Eye;\n' +
    'uniform mat4 u_ModelMatrix;\n' +
    'varying vec4 v_Color;\n' +
    'varying float v_Distance;\n' +
    'void main() {\n' +
    '  gl_Position = u_MvpMatrix * a_Position;\n' +
    '  vec4 mPosition = u_ModelMatrix * a_Position;\n' +
    '  v_Distance = distance(u_Eye,mPosition);\n' +
    '  v_Color = a_Color;\n' +
    '}\n';

var f_shader =
    'precision mediump float;\n' +
    'uniform vec2 u_FogRange;\n' +
    'uniform vec4 u_FogColor;\n' +
    'varying vec4 v_Color;\n' +
    'varying float v_Distance;\n' +
    'void main() {\n' +
    '  float fogFactor = clamp((u_FogRange.y - v_Distance)/(u_FogRange.y - u_FogRange.x),0.0,1.0);\n' +
    '  vec4 color = mix(u_FogColor,v_Color,fogFactor);\n' +
    '  gl_FragColor = color;\n' +
    '}\n';


function main() {
    var canvas = document.getElementById('webgl');
    var gl = canvas.getContext('webgl');
    initShaders(gl, v_shader, f_shader);
    var fogColor = [0.6, 0.0, 0.0, 1.0];
    var eye = [20, 20, 20, 1.0];
    var fogRange = [30, 50];

    var u_MvpMatrixLoc = gl.getUniformLocation(gl.program, 'u_MvpMatrix');
    var u_FogRangeLoc = gl.getUniformLocation(gl.program, 'u_FogRange');
    var u_FogColorLoc = gl.getUniformLocation(gl.program, 'u_FogColor');
    var u_EyeLoc = gl.getUniformLocation(gl.program, 'u_Eye');
    var u_ModelMatrixLoc = gl.getUniformLocation(gl.program, 'u_ModelMatrix');
    var mvpMatrix = new Matrix4();
    mvpMatrix.setPerspective(30, canvas.width / canvas.height, 1, 1000);
    mvpMatrix.lookAt(eye[0], eye[1], eye[2], 0, 0, 0, 0, 1, 0);
    var modelMatrix = new Matrix4();
    modelMatrix.scale(3, 3, 3);
    mvpMatrix.multiply(modelMatrix);
    gl.uniformMatrix4fv(u_MvpMatrixLoc, false, mvpMatrix.elements);

    gl.uniform2fv(u_FogRangeLoc, fogRange);
    gl.uniform4fv(u_FogColorLoc, fogColor);
    gl.uniform4fv(u_EyeLoc, eye);
    gl.uniformMatrix4fv(u_ModelMatrixLoc, false, modelMatrix.elements);
    //传入 position color 等顶点属性值 以及传入 索引
    var n = initVertexBuffers(gl);

    document.onkeydown = function (ev) {
        keydown(ev, gl, n, u_FogRangeLoc,fogRange)
    }

    preDraw(gl,fogColor);
    draw(gl, n);
}

function keydown(ev, gl, n, u_FogDist, fogDist) {
    switch (ev.keyCode) {
        case 38:
            fogDist[1] += 1;
            break;
        case 40: // Down arrow key -> Decrease the maximum distance of fog
            if (fogDist[1] > fogDist[0]) fogDist[1] -= 1;
            break;
        default:
            return;
    }

    gl.uniform2fv(u_FogDist, fogDist);   // Pass the distance of fog
    draw(gl, n);

}

function preDraw(gl,fogColor) {
    gl.enable(gl.CULL_FACE);
    gl.enable(gl.DEPTH_TEST);

    gl.clearColor(fogColor[0], fogColor[1], fogColor[2], fogColor[3]);


}

function draw(gl, n) {
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
    gl.drawElements(gl.TRIANGLES, n, gl.UNSIGNED_BYTE, 0);
}

function initVertexBuffers(gl) {
    // Create a cube
    //    v6----- v5
    //   /|      /|
    //  v1------v0|
    //  | |     | |
    //  | |v7---|-|v4
    //  |/      |/
    //  v2------v3
    var positions = new Float32Array([
        1, 1, 1, -1, 1, 1, -1, -1, 1, 1, -1, 1,
        1, 1, 1, 1, -1, 1, 1, -1, -1, 1, 1, -1,
        1, 1, 1, 1, 1, -1, -1, 1, -1, -1, 1, 1,
        -1, 1, 1, -1, 1, -1, -1, -1, -1, -1, -1, 1,
        -1, -1, -1, 1, -1, -1, 1, -1, 1, -1, -1, 1,
        1, -1, -1, -1, -1, -1, -1, 1, -1, 1, 1, -1
    ]);

    var colors = new Float32Array([
        0.4, 0.4, 1.0, 0.4, 0.4, 1.0, 0.4, 0.4, 1.0, 0.4, 0.4, 1.0,  // v0-v1-v2-v3 front
        0.4, 1.0, 0.4, 0.4, 1.0, 0.4, 0.4, 1.0, 0.4, 0.4, 1.0, 0.4,  // v0-v3-v4-v5 right
        1.0, 0.4, 0.4, 1.0, 0.4, 0.4, 1.0, 0.4, 0.4, 1.0, 0.4, 0.4,  // v0-v5-v6-v1 up
        1.0, 1.0, 0.4, 1.0, 1.0, 0.4, 1.0, 1.0, 0.4, 1.0, 1.0, 0.4,  // v1-v6-v7-v2 left
        1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0,  // v7-v4-v3-v2 down
        0.4, 1.0, 1.0, 0.4, 1.0, 1.0, 0.4, 1.0, 1.0, 0.4, 1.0, 1.0   // v4-v7-v6-v5 back
    ]);

    var indices = new Uint8Array([
        0, 1, 2, 0, 2, 3,
        4, 5, 6, 4, 6, 7,
        8, 9, 10, 8, 10, 11,
        12, 13, 14, 12, 14, 15,
        16, 17, 18, 16, 18, 19,
        20, 21, 22, 20, 22, 23
    ]);

    initArrayBuffer(gl, positions, 3, gl.FLOAT, 'a_Position');
    initArrayBuffer(gl, colors, 3, gl.FLOAT, 'a_Color');

    var indecesBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indecesBuffer)
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, indices, gl.STATIC_DRAW);

    return indices.length;
}

function initArrayBuffer(gl, data, size, type, attribute) {
    var buffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
    gl.bufferData(gl.ARRAY_BUFFER, data, gl.STATIC_DRAW);
    var attributeLoc = gl.getAttribLocation(gl.program, attribute);
    gl.vertexAttribPointer(attributeLoc, size, type, false, 0, 0);
    gl.enableVertexAttribArray(attributeLoc);

    gl.bindBuffer(gl.ARRAY_BUFFER, null);
}
