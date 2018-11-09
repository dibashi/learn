var VSHADER_SOURCE =
    'attribute vec4 a_Position;\n' +
    'attribute vec4 a_Normal;\n' +
    'attribute vec4 a_Color;\n' +
    'uniform mat4 u_MVPMatrix;\n' +
    'uniform mat4 u_NormalMatrix;\n' +
    'uniform vec3 u_LightDirection;\n' +
    'uniform vec3 u_LightColor;\n' +
    'varying vec4 v_Color;\n' +
    'void main() {\n' +
    '     gl_Position = u_MVPMatrix * a_Position;\n' +
    '     vec3 normal = normalize(vec3(u_NormalMatrix * a_Normal));\n' +
    '     vec3 lightDirection = normalize(u_LightDirection);\n' +
    '     float nDotL = max(dot(normal,lightDirection),0.0);\n' +
    '     vec3 ambientColor = vec3(0.1,0.1,0.1);\n' +
    '     v_Color = vec4((vec3(a_Color) * u_LightColor) * nDotL + ambientColor,a_Color.a);\n' +
    '}\n';
var FSHADER_SOURCE =
    'precision mediump float;\n' +
    'varying vec4 v_Color;\n' +
    'void main() {\n' +
    '     gl_FragColor = v_Color;\n' +
    '}\n';

function main() {
    var canvas = document.getElementById("webgl");
    var gl = getWebGLContext(canvas);

    initShaders(gl, VSHADER_SOURCE, FSHADER_SOURCE);
    var n = initVertexBuffers(gl);

    gl.enable(gl.DEPTH_TEST);
    gl.enable(gl.CULL_FACE);
    gl.clearColor(0.0, 0.0, 0.0, 1.0);

    var VPMatrix = new Matrix4();
    VPMatrix.setPerspective(50, canvas.width / canvas.height, 1.0, 100);
    VPMatrix.lookAt(0, 0, 20, 0, 0, 0, 0, 1, 0);

    var u_MVPMatrixLoc = gl.getUniformLocation(gl.program, 'u_MVPMatrix');
    var u_NormalMatrixLoc = gl.getUniformLocation(gl.program, 'u_NormalMatrix');

    var u_LightDirectionLoc = gl.getUniformLocation(gl.program, 'u_LightDirection');
    gl.uniform3fv(u_LightDirectionLoc, new Float32Array([0.0, 0.0, 1.0]));

    var u_LightColorLoc = gl.getUniformLocation(gl.program, 'u_LightColor');
    gl.uniform3fv(u_LightColorLoc, new Float32Array([1.0, 1.0, 1.0]));
    // var currentAngle = 0.0;
    // var tick = function() {
    //     currentAngle = animate(currentAngle);
    //     draw(gl,currentAngle,VPMatrix,u_MVPMatrixLoc,u_NormalMatrixLoc,n);
    //     requestAnimationFrame(tick);
    // }

    // tick();
    var arm1Angle = 0;
    var arm2Angle = 0;
    var stepAngle = 3;
    document.onkeydown = function (ev) {
        switch (ev.keyCode) {
            case 37:
                arm1Angle = arm1Angle - stepAngle;
                break;
            case 38:
                if (arm2Angle > -135) arm2Angle -= stepAngle;
                break;
            case 39:
                arm1Angle = arm1Angle + stepAngle;
                break;
            case 40:
                if (arm2Angle < 135) arm2Angle += stepAngle;
                break;

            default:
                return;
        }

        draw2(gl, VPMatrix, u_MVPMatrixLoc, u_NormalMatrixLoc, n, arm1Angle, arm2Angle);
    }
    draw2(gl, VPMatrix, u_MVPMatrixLoc, u_NormalMatrixLoc, n, arm1Angle, arm2Angle);
}

var g_MVPMatrix = new Matrix4();
var g_ModelMatrix = new Matrix4();
var g_NormalMatrix = new Matrix4();
function draw(gl, currentAngle, VPMatrix, u_MVPMatrixLoc, u_NormalMatrixLoc, n) {

    

    g_MVPMatrix.set(VPMatrix);
    //g_ModelMatrix.setRotate(currentAngle,1.0,0.0,0.0);
    g_ModelMatrix.setRotate(currentAngle, 0.0, 1.0, 0.0);
    g_MVPMatrix.multiply(g_ModelMatrix);
    //g_ModelMatrix.rotate(currentAngle,0.0,0.0,1.0);
    g_NormalMatrix.setInverseOf(g_ModelMatrix).transpose();
    gl.uniformMatrix4fv(u_MVPMatrixLoc, false, g_MVPMatrix.elements);
    gl.uniformMatrix4fv(u_NormalMatrixLoc, false, g_NormalMatrix.elements);

    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
    gl.drawElements(gl.TRIANGLES, n, gl.UNSIGNED_BYTE, 0);
}

function draw2(gl, VPMatrix, u_MVPMatrixLoc, u_NormalMatrixLoc, n, arm1Angle, arm2Angle) {
    adjustCanvasBitmapSize(gl.canvas);
    gl.viewport(0,0,gl.canvas.width,gl.canvas.height);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

    g_ModelMatrix.setRotate(arm1Angle, 0.0, 1.0, 0.0);
    drawPlane(gl, VPMatrix, u_MVPMatrixLoc, u_NormalMatrixLoc, n);

    g_ModelMatrix.translate(0.0, 6.0, 0.0);
    g_ModelMatrix.rotate(arm2Angle, 1.0, 0.0, 0.0);
    drawPlane(gl, VPMatrix, u_MVPMatrixLoc, u_NormalMatrixLoc, n);

}

function drawPlane(gl, VPMatrix, u_MVPMatrixLoc, u_NormalMatrixLoc, n) {
    g_MVPMatrix.set(VPMatrix);
    g_MVPMatrix.multiply(g_ModelMatrix);
    g_NormalMatrix.setInverseOf(g_ModelMatrix).transpose();
    gl.uniformMatrix4fv(u_MVPMatrixLoc, false, g_MVPMatrix.elements);
    gl.uniformMatrix4fv(u_NormalMatrixLoc, false, g_NormalMatrix.elements);

    gl.drawElements(gl.TRIANGLES, n, gl.UNSIGNED_BYTE, 0);
}

var g_Speed = 20;
var g_LastTime = Date.now();
function animate(currentAngle) {
    //console.log(g_LastTime);
    var currentTime = Date.now();
    var dt = currentTime - g_LastTime;
    g_LastTime = currentTime;
    currentAngle += dt * g_Speed / 1000;
    return currentAngle % 360;
}

function initVertexBuffers(gl) {
    var positions = new Float32Array([
        3.0, 3.0, 0.0, -3.0, 3.0, 0.0, -3.0, -3.0, 0.0, 3.0, -3.0, 0.0,

        3.0, 3.0, 0.0, -3.0, 3.0, 0.0, -3.0, -3.0, 0.0, 3.0, -3.0, 0.0,
    ]);

    var normals = new Float32Array([
        0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0,

        0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0
    ]);

    var colors = new Float32Array([
        0.7, 0.0, 0.0, 0.7, 0.0, 0.0, 0.7, 0.0, 0.0, 0.7, 0.0, 0.0,
        0.0, 0.0, 0.7, 0.0, 0.0, 0.7, 0.0, 0.0, 0.7, 0.0, 0.0, 0.7
    ]);

    var indices = new Uint8Array([
        0, 1, 2, 0, 2, 3,
        4, 6, 5, 4, 7, 6
    ]);

    initBuffer(gl, positions, 'a_Position', 3);
    initBuffer(gl, normals, 'a_Normal', 3);
    initBuffer(gl, colors, 'a_Color', 3);

    var indicesBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indicesBuffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, indices, gl.STATIC_DRAW);

    return indices.length;
}

function initBuffer(gl, data, a_Attribute, size) {
    var buffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
    gl.bufferData(gl.ARRAY_BUFFER, data, gl.STATIC_DRAW);
    var a_AttributeLoc = gl.getAttribLocation(gl.program, a_Attribute);

    gl.vertexAttribPointer(a_AttributeLoc, size, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(a_AttributeLoc);
}


function adjustCanvasBitmapSize(canvas) {
    var realToCSSPixels = window.devicePixelRatio;
    console.log(realToCSSPixels);
    var displayWidth = realToCSSPixels * canvas.clientWidth;
    var displayHeight = realToCSSPixels * canvas.clientHeight;

    if(canvas.width != displayWidth ||canvas.height != displayHeight) {
        canvas.width = displayWidth;
        canvas.height = displayHeight;
    }
}




