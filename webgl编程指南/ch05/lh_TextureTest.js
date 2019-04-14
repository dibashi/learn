var v_shader =
  `
      attribute vec4 a_Position;
      attribute vec2 a_Texcoord;
      varying vec2 v_Texcoord;
      void main() {
        gl_Position = a_Position;
        v_Texcoord = a_Texcoord;
      }
    `;

var f_shader =
  `
      precision mediump float;
      uniform sampler2D u_Texture0;
      uniform sampler2D u_Texture1;
      varying vec2 v_Texcoord;
      void main() {
        vec4 color0 = texture2D(u_Texture0,v_Texcoord);
        vec4 color1 = texture2D(u_Texture1,v_Texcoord);
        gl_FragColor = color0*color1;
      }
    `;

    var f1_shader =
  `
      precision mediump float;
      uniform sampler2D u_Texture0;
      uniform sampler2D u_Texture1;
      varying vec2 v_Texcoord;
      void main() {
        vec4 color0 = texture2D(u_Texture0,v_Texcoord);
        vec4 color1 = texture2D(u_Texture1,v_Texcoord);
        gl_FragColor = color1;
      }
    `;




function main() {
  var gl = document.getElementById('webgl').getContext("webgl");
  var program = createProgram(gl, v_shader, f_shader);
  var program1 = createProgram(gl, v_shader, f1_shader);
  gl.clearColor(0.0, 0.0, 0.0, 1.0);
  gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, 1);

  var a_PositionLoc = gl.getAttribLocation(program, "a_Position");
  var a_TexcoordLoc = gl.getAttribLocation(program, "a_Texcoord");
  var u_Texture0Loc = gl.getUniformLocation(program, "u_Texture0");
  var u_Texture1Loc = gl.getUniformLocation(program, "u_Texture1");

  console.log(a_PositionLoc);
  console.log(a_TexcoordLoc);
  console.log(u_Texture0Loc);
  console.log(u_Texture1Loc);

  var vertexTexcoordBuffer = gl.createBuffer();
  gl.bindBuffer(gl.ARRAY_BUFFER, vertexTexcoordBuffer);
  var n = initVertexBuffers(gl);

  initTextures(['../resources/sky.jpg', '../resources/circle.gif']);

  function initTextures(pathnames) {

    var unit = 0;
    var images = [];
    var textures = [];
    for (var i = 0; i < pathnames.length; i++) {

      var texture = gl.createTexture();

      var image = new Image();
      images.push(image);
      textures.push(texture);
      image.onload = function () {
        unit++;
        if (unit == pathnames.length) {
          loadTexture(images, textures);
        }
      }
      image.src = pathnames[i];
    }

  }


  var iii = 1;

  function loadTexture(images, textures) {

    for (var i = 0; i < images.length; i++) {
      gl.activeTexture(gl.TEXTURE0 + i);
      gl.bindTexture(gl.TEXTURE_2D, textures[i]);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
      gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGB, gl.RGB, gl.UNSIGNED_BYTE, images[i]);
    }

    gl.clear(gl.COLOR_BUFFER_BIT);
    

    gl.bindBuffer(gl.ARRAY_BUFFER, vertexTexcoordBuffer);
    gl.vertexAttribPointer(a_PositionLoc, 2, gl.FLOAT, false, 4 * 4, 0);
    gl.enableVertexAttribArray(a_PositionLoc);
    gl.vertexAttribPointer(a_TexcoordLoc, 2, gl.FLOAT, false, 4 * 4, 2 * 4);
    gl.enableVertexAttribArray(a_TexcoordLoc);

    gl.useProgram(program);
    
    gl.uniform1i(u_Texture0Loc, 0);
    gl.uniform1i(u_Texture1Loc, 1);

    gl.drawArrays(gl.TRIANGLE_STRIP, 0, n);

  }

}

function initVertexBuffers(gl) {

  var verticesTexcoords = new Float32Array([
    -0.5, 0.5, 0.0, 1.0,
    0.5, 0.5, 1.0, 1.0,
    -0.5, -0.5, 0.0, 0.0,
    0.5, -0.5, 1.0, 0.0
  ]);

  gl.bufferData(gl.ARRAY_BUFFER, verticesTexcoords, gl.STATIC_DRAW);

  return 4;
}

