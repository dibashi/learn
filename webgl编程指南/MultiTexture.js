var VSHADER_SOURCE =
  'attribute vec2 a_position;\n' +
  'attribute vec2 a_texCoord;\n' +
  'varying vec2 v_texCoord;\n' +
  'uniform vec2 u_resolution;\n' +
  'void main() {' +
  '     vec2 zeroToOne = a_position/u_resolution;\n' +
  '     vec2 zeroToTwo = zeroToOne*2.0;\n' +
  '     vec2 clipSpace = zeroToTwo - 1.0;\n' +
  '     gl_Position = vec4(clipSpace * vec2(1,-1),0,1);\n' +
  '     v_texCoord = a_texCoord;\n' +
  '}';

var FSHADER_SOURCE =
  'precision mediump float;\n' +
  'varying vec2 v_texCoord;\n' +
  'uniform sampler2D u_image;\n' +
  'void main() {\n' +
  '     gl_FragColor = texture2D(u_image,v_texCoord).brga;\n' +
  '}';

function main() {


  // var n = initVertexBuffer(gl);
  // initImageData(gl);

  // gl.viewport(0,0,canvas.clientWidth,canvas.clientHeight);
  // gl.clearColor(0.0,0.0,0.0,1.0);
  // gl.clear(gl.COLOR_BUFFER_BIT);

  // gl.drawArrays(gl.TRIANGLES,0,n);
  var image = new Image();
  image.src = './leaves.jpg';
  image.onload = function () {
    render(image);
  }


}


function render(image) {
  var canvas = document.getElementById("webgl");
  var gl = getWebGLContext(canvas);
  initShaders(gl, VSHADER_SOURCE, FSHADER_SOURCE);

  var positionLocation = gl.getAttribLocation(gl.program, 'a_position');
  var texCoordLocation = gl.getAttribLocation(gl.program, 'a_texCoord');

  var positionBuffer = gl.createBuffer();
  gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);
  setRectangle(gl, 0, 0, image.width, image.height);

  var texCoordBuffer = gl.createBuffer();
  gl.bindBuffer(gl.ARRAY_BUFFER, texCoordBuffer);
  gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
    0.0, 0.0,
    1.0, 0.0,
    0.0, 1.0,
    0.0, 1.0,
    1.0, 0.0,
    1.0, 1.0,
  ]), gl.STATIC_DRAW);

  var texture = gl.createTexture();
  gl.activeTexture(gl.TEXTURE0);
  gl.bindTexture(gl.TEXTURE_2D, texture);
  // Set the parameters so we can render any size image.
  gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
  gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
  gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
  gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);

  gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
  var imageLocation = gl.getUniformLocation(gl.program,'u_image');
  gl.uniform1i(imageLocation,0);
  // lookup uniforms
  var resolutionLocation = gl.getUniformLocation(gl.program, "u_resolution");

  // Tell WebGL how to convert from clip space to pixels
  gl.viewport(0, 0, gl.canvas.width, gl.canvas.height);

  // Clear the canvas
  gl.clearColor(0, 0, 0, 1);
  gl.clear(gl.COLOR_BUFFER_BIT);
  // Tell it to use our program (pair of shaders)
  gl.useProgram(gl.program);

  gl.enableVertexAttribArray(positionLocation);
  gl.bindBuffer(gl.ARRAY_BUFFER,positionBuffer);
  gl.vertexAttribPointer(positionLocation,2,gl.FLOAT,false,0,0);
  gl.enableVertexAttribArray(texCoordLocation);
  gl.bindBuffer(gl.ARRAY_BUFFER,texCoordBuffer);
  gl.vertexAttribPointer(texCoordLocation,2,gl.FLOAT,false,0,0);
  gl.uniform2f(resolutionLocation,canvas.width,canvas.height);
  gl.drawArrays(gl.TRIANGLES,0,6);
}

function setRectangle(gl, x, y, width, height) {
  var x1 = x;
  var x2 = x + width;
  var y1 = y;
  var y2 = y + height;
  gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
    x1, y1,
    x2, y1,
    x1, y2,
    x1, y2,
    x2, y1,
    x2, y2,
  ]), gl.STATIC_DRAW);

  
}


function initVertexBuffer(gl) {

}