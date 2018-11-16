// HelloPoint1.js (c) 2012 matsuda
// Vertex shader program
var VSHADER_SOURCE =
  'attribute vec4 a_Position;\n' +
'void main() {\n' +
  '  gl_Position = a_Position;\n' + // Set the vertex coordinates of the point
  '  gl_PointSize = 10.0;\n' +                    // Set the point size
  '}\n';

// Fragment shader program
var FSHADER_SOURCE =
  'void main() {\n' +
  '  gl_FragColor = vec4(0.2, 0.4, 0.0, 1.0);\n' + // Set the point color
  '}\n';

function main() {
  // Retrieve <canvas> element
  var canvas = document.getElementById('webgl');

  // Get the rendering context for WebGL
  var gl = getWebGLContext(canvas);
  if (!gl) {
    console.log('Failed to get the rendering context for WebGL');
    return;
  }

  // Initialize shaders
  if (!initShaders(gl, VSHADER_SOURCE, FSHADER_SOURCE)) {
    console.log('Failed to intialize shaders.');
    return;
  }

  var a_PositionLoc = gl.getAttribLocation(gl.program, 'a_Position');
  var n = createCircleDatas(gl,a_PositionLoc);
  // Specify the color for clearing <canvas>
  gl.clearColor(0.0, 0.0, 0.0, 1.0);

  // Clear <canvas>
  gl.clear(gl.COLOR_BUFFER_BIT);

  // Draw a point
  gl.drawArrays(gl.TRIANGLE_FAN, 0, n);
}

function createCircleDatas(gl,a_PositionLoc) {
  var positions = [0, 0];
  var fragCount = 64;
  var r = 0.5;

  function getPosition(index) {
    var angle = index * 360 / fragCount;
    var radians = angleToRadians(angle);
    return [r * Math.cos(radians), r * Math.sin(radians)];
  }

  for (var ii = 0; ii <= fragCount; ii++) {
    var position = getPosition(ii);
    positions.push(position[0]);
    positions.push(position[1]);
  }
  

  var positionsBuffer = gl.createBuffer();
  gl.bindBuffer(gl.ARRAY_BUFFER,positionsBuffer);
  gl.bufferData(gl.ARRAY_BUFFER,new Float32Array(positions),gl.STATIC_DRAW);
  gl.vertexAttribPointer(a_PositionLoc,2,gl.FLOAT,false,0,0);
  gl.enableVertexAttribArray(a_PositionLoc);
  return positions.length/2;
}

function angleToRadians(angle) {
  return angle * Math.PI / 180;
}