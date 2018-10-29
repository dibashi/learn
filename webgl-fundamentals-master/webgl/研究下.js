

function main() {
  var canvas = document.getElementById("canvas");
  var gl = canvas.getContext("webgl");
  console.log(gl);
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
  //var u_color = gl.getUniformLocation(program, 'u_color');
  //var a_color = gl.getAttribLocation(program,'a_color');
  //var color = [Math.random(), Math.random(), Math.random(), 1];

  function drawScene() {
    gl.viewport(0, 0, canvas.width, canvas.height);

    gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);
    gl.enableVertexAttribArray(a_position);
    gl.vertexAttribPointer(a_position, 3, gl.FLOAT, false, 0, 0);

    gl.bindBuffer(gl.ARRAY_BUFFER, colorBuffer);
    gl.enableVertexAttribArray(a_color);
    gl.vertexAttribPointer(a_color, 3, gl.FLOAT, false, 0, 0);


    var matrix = lhM4.projection(gl.canvas.clientWidth, gl.canvas.clientHeight, 402);
    console.log(matrix);
    gl.uniformMatrix4fv(matrixLocation, false, new Float32Array(matrix));




    // gl.uniform4fv(u_color, new Float32Array(color));
    gl.clearColor(0.0, 0.0, 0.0, 1.0);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

    gl.drawArrays(gl.TRIANGLES, 0, n);
  }

  drawScene();

  // Setup a ui.
  webglLessonsUI.setupSlider("#x", {value: translation[0], slide: updatePosition(0), max: gl.canvas.width });
  webglLessonsUI.setupSlider("#y", {value: translation[1], slide: updatePosition(1), max: gl.canvas.height});
  webglLessonsUI.setupSlider("#z", {value: translation[2], slide: updatePosition(2), max: gl.canvas.height});
  webglLessonsUI.setupSlider("#angleX", {value: radToDeg(rotation[0]), slide: updateRotation(0), max: 360});
  webglLessonsUI.setupSlider("#angleY", {value: radToDeg(rotation[1]), slide: updateRotation(1), max: 360});
  webglLessonsUI.setupSlider("#angleZ", {value: radToDeg(rotation[2]), slide: updateRotation(2), max: 360});
  webglLessonsUI.setupSlider("#scaleX", {value: scale[0], slide: updateScale(0), min: -5, max: 5, step: 0.01, precision: 2});
  webglLessonsUI.setupSlider("#scaleY", {value: scale[1], slide: updateScale(1), min: -5, max: 5, step: 0.01, precision: 2});
  webglLessonsUI.setupSlider("#scaleZ", {value: scale[2], slide: updateScale(2), min: -5, max: 5, step: 0.01, precision: 2});
}

function initGeometry(gl) {
  var vertices = new Float32Array([
    // // 左竖
    // 0, 0, 0,
    // 30, 0, 0,
    // 0, 150, 0,
    // 0, 150, 0,
    // 30, 0, 0,
    // 30, 150, 0,

    // // 上横
    // 30, 0, 0,
    // 100, 0, 0,
    // 30, 30, 0,
    // 30, 30, 0,
    // 100, 0, 0,
    // 100, 30, 0,

    // // 下横
    // 30, 60, 0,
    // 67, 60, 0,
    // 30, 90, 0,
    // 30, 90, 0,
    // 67, 60, 0,
    // 67, 90, 0

    // 0,0,-200,
    // 0,100,-200, //红
    // 100,100,-200,

    // 0,0,200,
    // 0,100,200, //绿
    // 100,0,200
    0, 0, 0,
    30, 0, 0,
    0, 150, 0,

    0, 150, 0,
    30, 0, 0,
    30, 150, 0,

    // top rung
    30, 0, 0,
    100, 0, 0,
    30, 30, 0,

    30, 30, 0,
    100, 0, 0,
    100, 30, 0,

    // middle rung
    30, 60, 0,
    67, 60, 0,
    30, 90, 0,

    30, 90, 0,
    67, 60, 0,
    67, 90, 0
  ])
  var n = 18;


  gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);
  return n;
}

function initColor(gl) {
  var colors = new Float32Array([
    0.5, 0.0, 0.0,
    0.5, 0.0, 0.0,
    0.5, 0.0, 0.0,

    0.5, 0.0, 0.0,
    0.5, 0.0, 0.0,
    0.5, 0.0, 0.0,

    0.0, 0.5, 0.0,
    0.0, 0.5, 0.0,
    0.0, 0.5, 0.0,

    0.5, 0.0, 0.0,
    0.5, 0.0, 0.0,
    0.5, 0.0, 0.0,

   0.0, 0.5, 0.0,
    0.0, 0.5, 0.0,
    0.0, 0.5, 0.0,

    0.5, 0.0, 0.0,
    0.5, 0.0, 0.0,
    0.5, 0.0, 0.0,
  ]);
  var n = 18;
  gl.bufferData(gl.ARRAY_BUFFER, colors, gl.STATIC_DRAW);
  return n;
}

var lhM4 = {
  projection: function (width, height, depth) {
    // return [
    //   2 / width, 0, 0, 0,
    //   0, -2 / height, 0, 0,
    //   0, 0, -2 / depth, 0,
    //   -1, -1, 1, 1
    // ];
    return [
      2 / width, 0, 0, 0,
      0, -2 / height, 0, 0,
      0, 0, 2 / depth, 0,
      -1, 1, 0, 1,
    ];
  }
}

main();