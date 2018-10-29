
var VSHADER_SOURCE =
    'attribute vec2 a_position;' +
    'uniform mat3 u_matrix;' +
    'void main() {' +
    'gl_Position = vec4((u_matrix * vec3(a_position, 1)).xy, 0, 1);' +
    '}';

var FSHADER_SOURCE =
    'precision mediump float;' +
    'uniform vec4 u_color;' +
    'void main() {' +
    'gl_FragColor = u_color;' +
    '}';


function main() {
    // Get A WebGL context
    /** @type {HTMLCanvasElement} */
    var canvas = document.getElementById("canvas");
    var gl = canvas.getContext("webgl");
    if (!gl) {
        return;
    }

    initShaders(gl, VSHADER_SOURCE, FSHADER_SOURCE);
    // setup GLSL program

    // look up where the vertex data needs to go.
    var positionAttributeLocation = gl.getAttribLocation(gl.program, "a_position");

    // lookup uniforms
    var colorLocation = gl.getUniformLocation(gl.program, "u_color");
    gl.uniform4fv(colorLocation, [1, 0, 0, 1]);
    var matrixLocation = gl.getUniformLocation(gl.program, "u_matrix");

    // Create a buffer to put three 2d clip space points in
    var positionBuffer = gl.createBuffer();

    // Bind it to ARRAY_BUFFER (think of it as ARRAY_BUFFER = positionBuffer)
    gl.clearColor(0.0, 0.0, 0.0, 1.0);

    requestAnimationFrame(drawScene);

    // Draw the scene.
    function drawScene(now) {
        now *= 0.001;  // convert to seconds
        resizeRetina(gl.canvas);
        // Tell WebGL how to convert from clip space to pixels
       
        gl.viewport(canvas.width/2-50, canvas.height/2-50, 100,100);
     
        // Clear the canvas.
        gl.clear(gl.COLOR_BUFFER_BIT);

        // // Tell it to use our program (pair of shaders)
        // gl.useProgram(program);

        // Turn on the attribute
        gl.enableVertexAttribArray(positionAttributeLocation);

        // Bind the position buffer.
        gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);

        // Tell the attribute how to get data out of positionBuffer (ARRAY_BUFFER)
        var size = 2;          // 2 components per iteration
        var type = gl.FLOAT;   // the data is 32bit floats
        var normalize = false; // don't normalize the data
        var stride = 0;        // 0 = move forward size * sizeof(type) each iteration to get the next position
        var offset = 0;        // start at the beginning of the buffer
        gl.vertexAttribPointer(
            positionAttributeLocation, size, type, normalize, stride, offset)

        // Set Geometry.
        // var radius = Math.sqrt(gl.canvas.width * gl.canvas.width + gl.canvas.height * gl.canvas.height) * 0.5;
        // var angle = now;
        // var x = Math.cos(angle) * radius;
        // var y = Math.sin(angle) * radius;
        // var centerX = gl.canvas.width / 2;
        // var centerY = gl.canvas.height / 2;
        // setGeometry(gl, centerX + x, centerY + y, centerX - x, centerY - y);
        setGeometry(gl,0,0,gl.canvas.width,gl.canvas.height);

        // Compute the matrices
        var projectionMatrix = projection(gl.canvas.width, gl.canvas.height);

        // Set the matrix.
        gl.uniformMatrix3fv(matrixLocation, false, projectionMatrix);


        // Draw the geometry.
        var primitiveType = gl.LINES;
        var offset = 0;
        var count = 2;
        gl.drawArrays(primitiveType, offset, count);

        requestAnimationFrame(drawScene);
    }
}

// Fill the buffer with a line
function setGeometry(gl, x1, y1, x2, y2) {
    gl.bufferData(
        gl.ARRAY_BUFFER,
        new Float32Array([
            x1, y1,
            x2, y2]),
        gl.STATIC_DRAW);
}

function projection(width, height) {
    return new Float32Array([2.0 / width, 0.0, 0.0, 0.0, -2.0/height, 0.0, -1.0, 1.0, 1.0]);
}

function resize(canvas) {
    // 获取浏览器中画布的显示尺寸
    var displayWidth  = canvas.clientWidth;
    var displayHeight = canvas.clientHeight;
   
    // 检尺寸是否相同
    if (canvas.width  != displayWidth ||
        canvas.height != displayHeight) {
   
      // 设置为相同的尺寸
      canvas.width  = displayWidth;
      canvas.height = displayHeight;
    }
  }

  function resizeRetina(canvas) {
    var realToCSSPixels = window.devicePixelRatio;
    console.log("realToCSSPixels--->  ");
    console.log(realToCSSPixels);
    // 获取浏览器显示的画布的CSS像素值
    // 然后计算出设备像素设置drawingbuffer
    var displayWidth  = Math.floor(canvas.clientWidth  * realToCSSPixels);
    var displayHeight = Math.floor(canvas.clientHeight * realToCSSPixels);
  
    // 检查画布尺寸是否相同
    if (canvas.width  !== displayWidth ||
        canvas.height !== displayHeight) {
  
      // 设置为相同的尺寸
      canvas.width  = displayWidth;
      canvas.height = displayHeight;
    }
  }