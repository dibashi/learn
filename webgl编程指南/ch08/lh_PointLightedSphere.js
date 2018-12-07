var v_shader =
    'attribute vec4 a_Position;\n' +
    'attribute vec4 a_Normal;\n' +
    'uniform mat4 u_MvpMatrix;\n' +
    'uniform mat4 u_NormalMatrix;\n' +
    'uniform mat4 u_ModelMatrix;\n' +
    'varying vec3 v_Position;\n' +
    'varying vec3 v_Normal;\n' +
    'void main() {\n' +
    '  gl_Position = u_MvpMatrix * a_Position;\n' +
    '  v_Position = vec3(u_ModelMatrix * a_Position);\n' +
    '  v_Normal = normalize(vec3(u_NormalMatrix * a_Normal));\n' +
    '}\n';

var f_shader =
    'precision mediump float;\n' +
    'uniform vec3 u_PointLightColor;\n' +
    'uniform vec3 u_PointLightPosition;\n' +
    'uniform vec3 u_AmbientColor;\n' +
    'varying vec3 v_Position;\n' +
    'varying vec3 v_Normal;\n' +
    'void main() {\n' +
    '  vec4 color = vec4(1.0,1.0,1.0,1.0);\n' +
    '  vec3 normal = normalize(v_Normal);\n' +
    '  vec3 lightDirection = normalize(u_PointLightPosition - v_Position);\n' +
    '  float nDotL = max(dot(normal, lightDirection),0.0);\n' +
    '  vec3 diffuse = u_PointLightColor * color.rgb * nDotL;\n' +
    '  vec3 ambient = u_AmbientColor * color.rgb;\n' +
    '  gl_FragColor = vec4(diffuse + ambient,color.a);\n' +
    '}\n';

function main() {
    var canvas = document.getElementById('webgl');
    var gl = canvas.getContext('webgl');

    initShaders(gl, v_shader, f_shader);

    var n = initVertexBuffers(gl);
    initUniform(gl);
    initUniformDynamic(gl);
    preDraw(gl);


    var tick = function () {
        animate();
        draw(gl, n);
        requestAnimationFrame(tick);
    }
    tick();
}

function initVertexBuffers(gl) {
    var v_Tiles = h_Tiles = 30;
    var r = 0.6;
    var i, j, hAnlge, vAngle, hSin, hCos, vSin, vCos;

    var p1, p2;

    var positions = [];
    var indices = [];
    for (i = 0; i <= h_Tiles; i++) {
        hAnlge = (i * Math.PI / h_Tiles);
        hSin = Math.sin(hAnlge);
        hCos = Math.cos(hAnlge);
        for (j = 0; j <= v_Tiles; j++) {
            vAngle = (j * 2 * Math.PI / v_Tiles);
            vSin = Math.sin(vAngle);
            vCos = Math.cos(vAngle);

            positions.push(r * hSin * vCos);
            positions.push(r * hCos);
            positions.push(-r * hSin * vSin);

        }
    }

    for (i = 0; i < h_Tiles; i++) {

        for (j = 0; j < v_Tiles; j++) {
            p1 = i * (v_Tiles + 1) + j;
            p2 = p1 + (v_Tiles + 1);
            indices.push(p1);
            indices.push(p2);
            indices.push(p2 + 1);

            indices.push(p1 + 1);
            indices.push(p1);
            indices.push(p2 + 1);
        }

    }

    initArrayBuffer(gl, 'a_Position', new Float32Array(positions), 3);
    initArrayBuffer(gl, 'a_Normal', new Float32Array(positions), 3);
    var indicesBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indicesBuffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, new Uint16Array(indices), gl.STATIC_DRAW);
    // gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER,null);
    return indices.length;
}

function initArrayBuffer(gl, a_Attirbute, data, num) {
    var buffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
    gl.bufferData(gl.ARRAY_BUFFER, data, gl.STATIC_DRAW);
    var a_AttirbuteLoc = gl.getAttribLocation(gl.program, a_Attirbute);
    gl.vertexAttribPointer(a_AttirbuteLoc, num, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(a_AttirbuteLoc);
    gl.bindBuffer(gl.ARRAY_BUFFER, null);
}

function initUniform(gl) {
    // 'uniform mat4 u_MvpMatrix;\n' +
    // 'uniform mat4 u_NormalMatrix;\n' +
    // 'uniform mat4 u_ModelMatrix;\n' +
    // 'uniform vec3 u_PointLightColor;\n' +
    // 'uniform vec3 u_PointLightPosition;\n' +
    // 'uniform vec3 u_AmbientColor;\n' +



    var u_PointLightColorLoc = gl.getUniformLocation(gl.program, 'u_PointLightColor');
    gl.uniform3fv(u_PointLightColorLoc, [0.8, 0.8, 0.8]);
    var u_PointLightPositionLoc = gl.getUniformLocation(gl.program, 'u_PointLightPosition');
    gl.uniform3fv(u_PointLightPositionLoc, [3,3,0]);
    var u_AmbientColorLoc = gl.getUniformLocation(gl.program, 'u_AmbientColor');
    gl.uniform3fv(u_AmbientColorLoc, [0.2, 0.2, 0.2]);
}

function initUniformDynamic(gl) {
    var u_MvpMatrixLoc = gl.getUniformLocation(gl.program, 'u_MvpMatrix');
    var mvpMatrix = new Matrix4();
    mvpMatrix.setPerspective(30, gl.canvas.width / gl.canvas.height, 1, 1000);
    mvpMatrix.lookAt(0, 0, 6, 0, 0, 0, 0, 1, 0);
    var modelMatrix = new Matrix4();
    modelMatrix.setRotate(angleX, 1, 0, 0);
    modelMatrix.rotate(angleY, 0, 1, 0);
    modelMatrix.rotate(angleZ, 0, 0, 1);
    modelMatrix.setRotate(90, 0, 1, 0); // Rotate around the y-axis
    mvpMatrix.multiply(modelMatrix);
    gl.uniformMatrix4fv(u_MvpMatrixLoc, false, mvpMatrix.elements);
    var u_ModelMatrixLoc = gl.getUniformLocation(gl.program, 'u_ModelMatrix');
    gl.uniformMatrix4fv(u_ModelMatrixLoc, false, modelMatrix.elements);

    var u_NormalMatrixLoc = gl.getUniformLocation(gl.program, 'u_NormalMatrix');
    var normalMatrix = new Matrix4();
    normalMatrix.setInverseOf(modelMatrix);
    normalMatrix.transpose();
    gl.uniformMatrix4fv(u_NormalMatrixLoc, false, normalMatrix.elements);
}

function preDraw(gl) {
    gl.clearColor(0.0, 0.0, 0.0, 1.0);
    gl.enable(gl.DEPTH_TEST);
    gl.enable(gl.CULL_FACE);
}

function draw(gl, n) {
    initUniformDynamic(gl);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
    gl.drawElements(gl.TRIANGLES, n, gl.UNSIGNED_SHORT, 0);
}

var angleX = 0.0;
var angleY = 0.0;
var angleZ = 0.0;
function animate() {
    angleX++;
    angleY++;
    angleZ++;
}