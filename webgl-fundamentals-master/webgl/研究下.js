

function main() {
  var canvas = document.getElementById("canvas");
  var gl = canvas.getContext("webgl");
  //console.log(gl);
  // setup GLSL program
  var program = webglUtils.createProgramFromScripts(gl, ["3d-vertex-shader", "3d-fragment-shader"]);
  gl.useProgram(program);
  var a_position = gl.getAttribLocation(program, 'a_position');
  var a_color = gl.getAttribLocation(program, 'a_color');

  var positionBuffer = gl.createBuffer();
  gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);
  var n = initGeometry(gl);
  var colorBuffer = gl.createBuffer();
  gl.bindBuffer(gl.ARRAY_BUFFER, colorBuffer);
  initColor(gl);
  var matrixLocation = gl.getUniformLocation(program, 'u_matrix');

  gl.enable(gl.DEPTH_TEST);
  gl.enable(gl.CULL_FACE);
  function radToDeg(r) {
    return r * 180 / Math.PI;
  }

  function degToRad(d) {
    return d * Math.PI / 180;
  }

  var translation = [0, 0, -500];
  var rotation = [degToRad(0), degToRad(0), degToRad(0)];
  var scale = [1, 1, 1];
  function drawScene() {
    webglUtils.resizeCanvasToDisplaySize(gl.canvas);
    gl.viewport(0, 0, canvas.width, canvas.height);

    gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);
    gl.enableVertexAttribArray(a_position);
    gl.vertexAttribPointer(a_position, 3, gl.FLOAT, false, 0, 0);

    gl.bindBuffer(gl.ARRAY_BUFFER, colorBuffer);
    gl.enableVertexAttribArray(a_color);
    gl.vertexAttribPointer(a_color, 3, gl.UNSIGNED_BYTE, true, 0, 0);


    //var matrix = lhM4.projection(gl.canvas.clientWidth, gl.canvas.clientHeight, 402);
    var matrix = lhM4.orthographic(-200, 200, -200, 200, 1, 999);
    // console.log(matrix);
    matrix = lhM4.translate(matrix, translation[0], translation[1], translation[2]);


    matrix = lhM4.xRotate(matrix, rotation[0]);
    matrix = lhM4.yRotate(matrix, rotation[1]);
    matrix = lhM4.zRotate(matrix, rotation[2]);

    matrix = lhM4.scale(matrix, scale[0], scale[1], scale[2]);
    //console.log(matrix);
    gl.uniformMatrix4fv(matrixLocation, false, new Float32Array(matrix));




    // gl.uniform4fv(u_color, new Float32Array(color));
    gl.clearColor(0.0, 0.0, 0.0, 1.0);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

    gl.drawArrays(gl.TRIANGLES, 0, n);
  }

  drawScene();

  // Setup a ui.
  webglLessonsUI.setupSlider("#x", { value: translation[0], slide: updatePosition(0), max: gl.canvas.width });
  webglLessonsUI.setupSlider("#y", { value: translation[1], slide: updatePosition(1), max: gl.canvas.height });
  webglLessonsUI.setupSlider("#z", { value: translation[2], slide: updatePosition(2), max: 500 });
  webglLessonsUI.setupSlider("#angleX", { value: radToDeg(rotation[0]), slide: updateRotation(0), max: 360 });
  webglLessonsUI.setupSlider("#angleY", { value: radToDeg(rotation[1]), slide: updateRotation(1), max: 360 });
  webglLessonsUI.setupSlider("#angleZ", { value: radToDeg(rotation[2]), slide: updateRotation(2), max: 360 });
  webglLessonsUI.setupSlider("#scaleX", { value: scale[0], slide: updateScale(0), min: -5, max: 5, step: 0.01, precision: 2 });
  webglLessonsUI.setupSlider("#scaleY", { value: scale[1], slide: updateScale(1), min: -5, max: 5, step: 0.01, precision: 2 });
  webglLessonsUI.setupSlider("#scaleZ", { value: scale[2], slide: updateScale(2), min: -5, max: 5, step: 0.01, precision: 2 });

  function updatePosition(index) {
    return function (event, ui) {
      translation[index] = ui.value;
      drawScene();
    }
  }

  function updateRotation(index) {
    return function (event, ui) {
      var angleInDegrees = ui.value;
      //console.log(angleInDegrees);
      var angleInRadians = angleInDegrees * Math.PI / 180;
      rotation[index] = angleInRadians;
      drawScene();
    }
  }

  function updateScale(index) {
    return function (event, ui) {
      scale[index] = ui.value;
      drawScene();
    }
  }

}

function initGeometry(gl) {
  var vertices = new Float32Array([
    // 前
    0, 150, 20,
    0, 0, 20,
    30, 0, 20,

    30, 150, 20,
    0, 150, 20,
    30, 0, 20,

    90,150,20,
    30,150,20,
    30,120,20,


    90,150,20,
    30,120,20,
    90,120,20,


    //上
    0, 150, 20,
    90,150,-20,
    0, 150, -20,
    
    0, 150, 20,
    90,150,20,
    90,150,-20,

    
    //后
    0, 0, -20,
    0, 150, -20,
    30, 0, -20,


    0, 150, -20,
    30, 150, -20,
    30, 0, -20,


    30,150,-20,
    90,150,-20,
    30,120,-20,

    30,120,-20,
    90,150,-20,
    90,120,-20,

    

    
   
    ]);
  var n = 30;


  gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);
  return n;
}

function initColor(gl) {
  var colors = new Uint8Array([
    // left column front
    200, 70, 120,
    200, 70, 120,
    200, 70, 120,
    200, 70, 120,
    200, 70, 120,
    200, 70, 120,

    200, 70, 120,
    200, 70, 120,
    200, 70, 120,
    200, 70, 120,
    200, 70, 120,
    200, 70, 120,


    70, 200, 120,
    70, 200, 120,
    70, 200, 120,
   

    70, 200, 120,
    70, 200, 120,
    70, 200, 120,
   
    // top rung front
    80, 70, 200,
    80, 70, 200,
    80, 70, 200,
    80, 70, 200,
    80, 70, 200,
    80, 70, 200,

    80, 70, 200,
    80, 70, 200,
    80, 70, 200,
    80, 70, 200,
    80, 70, 200,
    80, 70, 200,


    ]);
  gl.bufferData(gl.ARRAY_BUFFER, colors, gl.STATIC_DRAW);

}

var lhM4 = {
  projection: function (width, height, depth) {

    return [
      2 / width, 0, 0, 0,
      0, -2 / height, 0, 0,
      0, 0, 2 / depth, 0,
      -1, 1, 0, 1,
    ];
  },

  orthographic: function (left, right, bottom, top, near, far) {

    // if (left === right || top === bottom || near === far) {
    //   throw 'null frustum';
    // }
    // if (near <= 0) {
    //   throw 'near <= 0';
    // }
    // if (far <= 0) {
    //   throw 'far <= 0';
    // }

    return [
      2 / (right - left), 0, 0, 0,
      0, 2 / (top - bottom), 0, 0,
      0, 0, 2 / (near - far), 0,
      (left + right) / (left - right), (bottom + top) / (bottom - top), (near + far) / (near - far), 1
    ];
  },

  multiply: function (a, b) {
    var a00 = a[0 * 4 + 0];
    var a01 = a[0 * 4 + 1];
    var a02 = a[0 * 4 + 2];
    var a03 = a[0 * 4 + 3];
    var a10 = a[1 * 4 + 0];
    var a11 = a[1 * 4 + 1];
    var a12 = a[1 * 4 + 2];
    var a13 = a[1 * 4 + 3];
    var a20 = a[2 * 4 + 0];
    var a21 = a[2 * 4 + 1];
    var a22 = a[2 * 4 + 2];
    var a23 = a[2 * 4 + 3];
    var a30 = a[3 * 4 + 0];
    var a31 = a[3 * 4 + 1];
    var a32 = a[3 * 4 + 2];
    var a33 = a[3 * 4 + 3];

    var b00 = b[0 * 4 + 0];
    var b01 = b[0 * 4 + 1];
    var b02 = b[0 * 4 + 2];
    var b03 = b[0 * 4 + 3];
    var b10 = b[1 * 4 + 0];
    var b11 = b[1 * 4 + 1];
    var b12 = b[1 * 4 + 2];
    var b13 = b[1 * 4 + 3];
    var b20 = b[2 * 4 + 0];
    var b21 = b[2 * 4 + 1];
    var b22 = b[2 * 4 + 2];
    var b23 = b[2 * 4 + 3];
    var b30 = b[3 * 4 + 0];
    var b31 = b[3 * 4 + 1];
    var b32 = b[3 * 4 + 2];
    var b33 = b[3 * 4 + 3];

    return [
      b00 * a00 + b01 * a10 + b02 * a20 + b03 * a30,
      b00 * a01 + b01 * a11 + b02 * a21 + b03 * a31,
      b00 * a02 + b01 * a12 + b02 * a22 + b03 * a32,
      b00 * a03 + b01 * a13 + b02 * a23 + b03 * a33,
      b10 * a00 + b11 * a10 + b12 * a20 + b13 * a30,
      b10 * a01 + b11 * a11 + b12 * a21 + b13 * a31,
      b10 * a02 + b11 * a12 + b12 * a22 + b13 * a32,
      b10 * a03 + b11 * a13 + b12 * a23 + b13 * a33,
      b20 * a00 + b21 * a10 + b22 * a20 + b23 * a30,
      b20 * a01 + b21 * a11 + b22 * a21 + b23 * a31,
      b20 * a02 + b21 * a12 + b22 * a22 + b23 * a32,
      b20 * a03 + b21 * a13 + b22 * a23 + b23 * a33,
      b30 * a00 + b31 * a10 + b32 * a20 + b33 * a30,
      b30 * a01 + b31 * a11 + b32 * a21 + b33 * a31,
      b30 * a02 + b31 * a12 + b32 * a22 + b33 * a32,
      b30 * a03 + b31 * a13 + b32 * a23 + b33 * a33
    ];
  },

  translation: function (tx, ty, tz) {
    return [
      1, 0, 0, 0,
      0, 1, 0, 0,
      0, 0, 1, 0,
      tx, ty, tz, 1
    ];
  },

  scaling: function (sx, sy, sz) {
    return [
      sx, 0, 0, 0,
      0, sy, 0, 0,
      0, 0, sz, 0,
      0, 0, 0, 1
    ];
  },

  zRotation: function (angleInRadians) {
    var c = Math.cos(angleInRadians);
    var s = Math.sin(angleInRadians);
    return [
      c, s, 0, 0,
      -s, c, 0, 0,
      0, 0, 1, 0,
      0, 0, 0, 1
    ];
  },

  yRotation: function (angleInRadians) {
    var c = Math.cos(angleInRadians);
    var s = Math.sin(angleInRadians);
    return [
      c, 0, -s, 0,
      0, 1, 0, 0,
      s, 0, c, 0,
      0, 0, 0, 1
    ];
  },

  xRotation: function (angleInRadians) {
    var c = Math.cos(angleInRadians);
    var s = Math.sin(angleInRadians);
    return [
      1, 0, 0, 0,
      0, c, s, 0,
      0, -s, c, 0,
      0, 0, 0, 1
    ];
  },


  translate: function (m, tx, ty, tz) {
    return lhM4.multiply(m, lhM4.translation(tx, ty, tz));
  },
  scale: function (m, sx, sy, sz) {
    return lhM4.multiply(m, lhM4.scaling(sx, sy, sz));
  },

  zRotate: function (m, angleInRadians) {
    return lhM4.multiply(m, lhM4.zRotation(angleInRadians));
  },
  yRotate: function (m, angleInRadians) {
    // console.log(lhM4.yRotation(angleInRadians));
    return lhM4.multiply(m, lhM4.yRotation(angleInRadians));
  },
  xRotate: function (m, angleInRadians) {
    return lhM4.multiply(m, lhM4.xRotation(angleInRadians));
  }
}

main();