

var vShader = 
    'attribute vec4 a_Position;\n' +
    'void main() {\n' +
    '  gl_Position = a_Position;\n' +
    '  gl_PointSize = 10.0;\n' +
    '}\n';

var fShader = 
    'precision mediump float;\n' +
    'void main() {\n' +
    '  gl_FragColor = vec4(0.7,0.3,0.0,1.0);\n' +
    '}\n';

function main() {

    var gl = document.getElementById('webgl').getContext("webgl");

    initShaders(gl,vShader,fShader);

    var a_PositionLoc = gl.getAttribLocation(gl.program,'a_Position');
    gl.vertexAttrib3f(a_PositionLoc,0.5,0.5,0.0);

    gl.clearColor(0.0,0.0,0.0,1.0);
    gl.clear(gl.COLOR_BUFFER_BIT);
    gl.drawArrays(gl.POINTS,0,1);
}