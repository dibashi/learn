

var VSHADER_SOURCE =
    'attribute vec4 a_Position;\n' +
    'void main() {\n' +
    '  gl_Position = a_Position;\n' +
    '}\n';

// Fragment shader program
var FSHADER_SOURCE =
    'void main() {\n' +
    '  gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);\n' +
    '}\n';

var gl;
var pwgl = {};
function main() {
    var canvas = document.getElementById("webgl");

    gl = canvas.getContext('webgl');



    var phongProgram = createProgram(gl, VSHADER_SOURCE, FSHADER_SOURCE);
    pwgl.vertexPositionAttributeLoc = gl.getAttribLocation(phongProgram, 'a_Position');

    pwgl.cubeVertexPositionBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, pwgl.cubeVertexPositionBuffer);

    var cubeVertexPosition = [
        // 0.5, 0.5, 0.5, -0.5, 0.5, 0.5, -0.5, -0.5, 0.5
        0.5, 0.5, -0.5, 0.2,
    ]
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(cubeVertexPosition), gl.STATIC_DRAW);


    pwgl.CUBE_VERTEX_POS_BUF_ITEM_SIZE = 2;
    pwgl.CUBE_VERTEX_POS_BUF_NUM_ITEMS = 3;




    gl.clearColor(0.3, 0.3, 0.3, 1.0);
    gl.enable(gl.DEPTH_TEST);



    gl.useProgram(phongProgram);









    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);


    gl.bindBuffer(gl.ARRAY_BUFFER, pwgl.cubeVertexPositionBuffer);
    gl.vertexAttribPointer(pwgl.vertexPositionAttributeLoc,
        pwgl.CUBE_VERTEX_POS_BUF_ITEM_SIZE,
        gl.FLOAT, false, 4, 4);

    gl.enableVertexAttribArray(pwgl.vertexPositionAttributeLoc);

    gl.drawArrays(gl.LINES, 0, 2);



}




