var v_shader =
  'attribute vec4 a_Position;\n' +
  'attribute vec2 a_Texcoord;\n' +
  'varying vec2 v_Texcoord;\n' +
  'void main() {\n' +
  '  gl_Position = a_Position;\n' +
  '  v_Texcoord = a_Texcoord;\n' +
  '}\n';

var f_shader =
  'precision mediump float;\n' +
  'uniform sampler2D u_Sampler1;\n' +
  'uniform sampler2D u_Sampler2;\n' +
  'varying vec2 v_Texcoord;\n' +
  'void main() {\n' +
  '  vec4 color1 = texture2D(u_Sampler1,v_Texcoord);\n' +
  '  vec4 color2 = texture2D(u_Sampler2,v_Texcoord);\n' +
  '  gl_FragColor = color1*color2;\n' +
  '}\n';



function main() {
  var gl = document.getElementById('webgl').getContext('webgl');

  initShaders(gl, v_shader, f_shader);

  initVertexBuffers(gl);
  
  gl.clearColor(0.4, 0.4, 0.0, 1.0);

  initTextures(gl);
}

function initTextures(gl) {
  var image1 = new Image();
  image1.onload = function () {
    loadTexture(gl, image1, 1);
  }
  image1.src = '../resources/skytest.jpg';

  var image2 = new Image();
  image2.onload = function () {
    loadTexture(gl, image2, 2);
  }

  image2.src = '../resources/circle.gif';
}

var image1_flag = false;
var image2_flag = false;
function loadTexture(gl, image, index) {
  var u_SamplerLoc = null;
  if (index == 1) {
    image1_flag = true;
    u_SamplerLoc = gl.getUniformLocation(gl.program, 'u_Sampler1');
  } else if (index == 2) {
    image2_flag = true;
    u_SamplerLoc = gl.getUniformLocation(gl.program, 'u_Sampler2');
  }
  var texture = gl.createTexture();
  gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, 1);
  gl.activeTexture(gl.TEXTURE0 + index);
  gl.bindTexture(gl.TEXTURE_2D, texture);
  gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
  gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGB, gl.RGB, gl.UNSIGNED_BYTE, image);

  gl.uniform1i(u_SamplerLoc, index);

  if (image1_flag && image2_flag) {
    draw(gl);
  }

}

function draw(gl) {
  gl.clear(gl.COLOR_BUFFER_BIT);

  gl.drawArrays(gl.TRIANGLE_STRIP, 0, 4);
}

function initVertexBuffers(gl) {

  var vertices = new Float32Array([
    -0.5, 0.5, 0.0, 1.0,
    -0.5, -0.5, 0.0, 0.0,
    0.5, 0.5, 1.0, 1.0,
    0.5, -0.5, 1.0, 0.0
  ]);

  var fsize = vertices.BYTES_PER_ELEMENT;

  var vertexBuffer = gl.createBuffer();

  gl.bindBuffer(gl.ARRAY_BUFFER, vertexBuffer);

  gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

  var a_Position = gl.getAttribLocation(gl.program, 'a_Position');
  var a_Texcoord = gl.getAttribLocation(gl.program, 'a_Texcoord');

  gl.enableVertexAttribArray(a_Position);
  gl.enableVertexAttribArray(a_Texcoord);

  gl.vertexAttribPointer(a_Position, 2, gl.FLOAT, false, 4 * fsize, 0);
  gl.vertexAttribPointer(a_Texcoord, 2, gl.FLOAT, false, 4 * fsize, 2 * fsize);
  gl.bindBuffer(gl.ARRAY_BUFFER, null);
  return;
}