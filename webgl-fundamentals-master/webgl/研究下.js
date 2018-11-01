

function main() {
  var canvas = document.getElementById("canvas");
  var gl = canvas.getContext("webgl");
  //console.log(gl);
  // setup GLSL program
  var program = webglUtils.createProgramFromScripts(gl, ["3d-vertex-shader", "3d-fragment-shader"]);
  gl.useProgram(program);

  console.log("gl.FLOAT   " + gl.FLOAT);
  console.log("gl.FLOAT_VEC4  "+gl.FLOAT_VEC4);
  console.log("gl.FLOAT_MAT4  "+gl.FLOAT_MAT4);
  LHcreateUniformSetters(gl,program);
}

function LHcreateUniformSetters(gl, program) {
  
  console.log("----以下是uniformInfo")

  var numUniforms = gl.getProgramParameter(program, gl.ACTIVE_UNIFORMS);
  console.log(numUniforms);
  for (var ii = 0; ii < numUniforms; ++ii) {
    var uniformInfo = gl.getActiveUniform(program, ii);
    console.log(uniformInfo);
    
  }
}

main();