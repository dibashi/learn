
var v_shader1 =
    'attribute vec4 a_Position;\n' +
    'void main() {\n' +
    '  gl_Position = a_Position;\n' +
    '  gl_PointSize = 10.0;\n' +
    '}\n';

var f_shader1 =
    'precision mediump float;\n' +
    'uniform float u_V1;\n' +
    'uniform vec2 u_V2[2];\n' +
    'uniform vec3 u_V3;\n' +
    'void main() {\n' +
    '  gl_FragColor = vec4(u_V1,u_V2[0].x,u_V2[1].y,1.0);\n' +
    '}\n';



var v_shader2 =
    'attribute vec4 a_Position;\n' +
    'void main() {\n' +
    '  gl_Position = a_Position;\n' +
    '  gl_PointSize = 20.0;\n' +
    '}\n';

var f_shader2 =
    'precision mediump float;\n' +
    'uniform vec4 u_Color;\n' +
    'void main() {\n' +
    '  gl_FragColor = u_Color;\n' +
    '}\n';


function main() {
    var canvas = document.getElementById('webgl');
    var gl = canvas.getContext('webgl');

    var program1 = webglUtils.createProgramFromSources(gl, [v_shader1, f_shader1]);
    var setters = {};
    setters.attribSetters = webglUtils.createAttributeSetters(gl,program1);

    var positions = new Float32Array([
        0.3,0.3
    ]);
    var positionsBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER,positionsBuffer);
    gl.bufferData(gl.ARRAY_BUFFER,positions,gl.STATIC_DRAW);
    var attribs = {
        a_Position:{buffer:positionsBuffer,numComponents:2}
    };

   

    setters.uniformSetters = webglUtils.createUniformSetters(gl,program1);
    var values = {
        u_V1: 0.7,
        u_V2:[1.0,0.0,0.0,1.0]
    };
    gl.useProgram(program1);
  



    webglUtils.setAttributes(setters,attribs);
    webglUtils.setUniforms(setters,values);

    gl.clearColor(0.0, 0.0, 0.0, 1.0);
    gl.clear(gl.COLOR_BUFFER_BIT);

    gl.drawArrays(gl.POINTS, 0, 1);




    // var program2 = webglUtils.createProgramFromSources(gl, [v_shader2, f_shader2]);
    // gl.useProgram(program2);
    // var a_PositionLoc = gl.getAttribLocation(program2, 'a_Position');
    // var u_ColorLoc = gl.getUniformLocation(program2, 'u_Color');

    // var vertexBuffer = gl.createBuffer();
    // gl.bindBuffer(gl.ARRAY_BUFFER, vertexBuffer);
    // var positions = new Float32Array([
    //     -0.3, -0.3,
    //     -0.3, 0.7
    // ]);
    // gl.bufferData(gl.ARRAY_BUFFER, positions, gl.STATIC_DRAW);
    // gl.vertexAttribPointer(a_PositionLoc, 2, gl.FLOAT, false, 0, 0);
    // gl.enableVertexAttribArray(gl.a_PositionLoc);

    // var u_ColorData = [0.4, 0.8, 0.8, 1.0];
    // gl.uniform4fv(u_ColorLoc, u_ColorData);

    // gl.drawArrays(gl.LINES, 0, 2);

    // var numUniforms = gl.getProgramParameter(program1, gl.ACTIVE_UNIFORMS);
    // console.log(numUniforms);
    // for (var ii = 0; ii < numUniforms; ++ii) {
    //     var uniformInfo = gl.getActiveUniform(program1, ii);
    //     console.log(uniformInfo);
    //     // if (!uniformInfo) {
    //     //   break;
    //     // }
    //     // var name = uniformInfo.name;
    //     // // remove the array suffix.
    //     // if (name.substr(-3) === "[0]") {
    //     //   name = name.substr(0, name.length - 3);
    //     // }
    //     // var setter = createUniformSetter(program, uniformInfo);
    //     // uniformSetters[name] = setter;
    // }
    // console.log(webglUtils.createUniformSetters(gl, program1));
}