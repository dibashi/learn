var VSHADER_SOURCE =
    'attribute vec4 a_Position;\n' +
    'void main() {\n' +
    '  gl_Position = a_Position;\n' +
    '  gl_PointSize = 10.0;\n' +
    '}\n';

// Fragment shader program
var FSHADER_SOURCE =
    '#ifdef GL_ES\n' +
    'precision mediump float;\n' +
    '#endif\n' +
    'void main() {\n' +
    '  gl_FragColor = vec4(0.0,0.0,1.0,0.0);\n' +
    '}\n';

//1获得canvas  -->dom
//2获得gl--->需要外部库
//3编译着色器-->需要外部库 需要写着色器代码
//4初始化数据 顶点位置，颜色 -->需要看下源代码
//5传送到webgl -->原理还很混乱
//6画出来  --> drawElements or drawArrays
function main() {
    //1
    var canvas = document.getElementById("webgl");
    //2
    var gl = getWebGLContext(canvas);
    //3
    initShaders(gl, VSHADER_SOURCE, FSHADER_SOURCE);
    //4
   // var n = initVertexBuffers(gl);
    //5
    // Get the storage location of u_MvpMatrix
    // var u_MvpMatrix = gl.getUniformLocation(gl.program, 'u_MvpMatrix');
    // if (!u_MvpMatrix) {
    //     console.log('Failed to get the storage location of u_MvpMatrix');
    //     return;
    // }

    // // Set the eye point and the viewing volume
    // var mvpMatrix = new Matrix4();
    // mvpMatrix.setPerspective(30, 1, 1, 100);
    // mvpMatrix.lookAt(3, 3, 7, 0, 0, 0, 0, 1, 0);

    // Pass the model view projection matrix to u_MvpMatrix
    //gl.uniformMatrix4fv(u_MvpMatrix, false, mvpMatrix.elements);
   
    
    var vertices = new Float32Array([
        0.0,0.5
    ]);

    var verticesBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, verticesBuffer);
    gl.bufferData(gl.ARRAY_BUFFER,vertices,gl.STATIC_DRAW);
    var a_Position = gl.getAttribLocation(gl.program,'a_Position');
    gl.enableVertexAttribArray(a_Position);
    gl.vertexAttribPointer(a_Position,2,gl.FLOAT,false,0,0);
    //6
    gl.viewport(0,0,canvas.width/2,canvas.height/2);
    gl.clearColor(0.0, 0.0, 0.0, 1.0);
    gl.enable(gl.CULL_FACE);
    gl.enable(gl.DEPTH_TEST);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
    //gl.drawElements(gl.TRIANGLES, n, gl.UNSIGNED_BYTE, 0);

    gl.drawArrays(gl.POINTS,0,1);
}

//1 创建顶点 位置，颜色数据 创建索引数据
//2 创建顶点数据缓冲区，创建索引数据缓冲区
//3 将顶点数据传入顶点着色器，将索引数据传入gpu
function initVertexBuffers(gl) {
    //1
    var verticesColors = new Float32Array([
        // Vertex coordinates and color
        1.0, 1.0, 1.0, 1.0, 1.0, 1.0,  // v0 White
        -1.0, 1.0, 1.0, 1.0, 0.0, 1.0,  // v1 Magenta
        -1.0, -1.0, 1.0, 1.0, 0.0, 0.0,  // v2 Red
        1.0, -1.0, 1.0, 1.0, 1.0, 0.0,  // v3 Yellow

        1.0, -1.0, -1.0, 0.0, 1.0, 0.0,  // v4 Green
        1.0,  1.0, -1.0,     0.0,  1.0,  1.0,  // v5 Cyan
        -1.0, 1.0, -1.0, 0.0, 0.0, 1.0,  // v6 Blue
        -1.0, -1.0, -1.0, 0.0, 0.0, 0.0   // v7 Black
    ]);

    var indices = new Uint8Array([
        0, 1, 2, 0, 2, 3,    // front
        0, 3, 4, 0, 4, 5,    // right

        0, 5, 6, 0, 6, 1,    // up
        1, 6, 7, 1, 7, 2,    // left

        7, 4, 3, 7, 3, 2,    // down
        4, 7, 6, 4, 6, 5     // back

    ]);
    //2
    var vertexColorBuffer = gl.createBuffer();
    var indexBuffer = gl.createBuffer();

    //3
    // Write the vertex coordinates and color to the buffer object
    gl.bindBuffer(gl.ARRAY_BUFFER, vertexColorBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, verticesColors, gl.STATIC_DRAW);

    var FSIZE = verticesColors.BYTES_PER_ELEMENT;

    var a_Position = gl.getAttribLocation(gl.program, 'a_Position');
    gl.vertexAttribPointer(a_Position, 3, gl.FLOAT, false, FSIZE * 6, 0);
    gl.enableVertexAttribArray(a_Position);

    var a_Color = gl.getAttribLocation(gl.program, 'a_Color');
    gl.vertexAttribPointer(a_Color, 3, gl.FLOAT, false, FSIZE * 6, FSIZE * 3);
    gl.enableVertexAttribArray(a_Color);

    // Write the indices to the buffer object
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indexBuffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, indices, gl.STATIC_DRAW);

    return indices.length;
}