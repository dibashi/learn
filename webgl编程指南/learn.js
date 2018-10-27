var VSHADER_SOURCE = 
          'uniform mat4 u_ViewMatrix;\n' +
          'attribute vec4 a_Position;\n' +
          'attribute vec4 a_Color;\n' +
          'varying vec4 v_Color;\n' +
          'void main(){\n' +
          '      gl_Position = u_ViewMatrix * a_Position;\n' +
          '      v_Color = a_Color;\n'+
          '}';

var FSHADER_SOURCE = 
          'precision mediump float;\n' +
          'varying vec4 v_Color;\n' +
          'void main() {\n' +
          '      gl_FragColor = v_Color;\n' +
          '}';

function main() {
    var canvas = document.getElementById("webgl");
    var gl = getWebGLContext(canvas);

    initShaders(gl, VSHADER_SOURCE, FSHADER_SOURCE);

    var n = initVertexBuffers(gl);

    //gl.enable(gl.DEPTH_TEST);
    gl.clearColor(0.0, 0.0, 0.0, 1.0);
    gl.clear(gl.COLOR_BUFFER_BIT);
    gl.drawArrays(gl.TRIANGLES, 0, n);
}

function initVertexBuffers(gl) {
    var vertices = new Float32Array([
       
        
        0.0, 0.5, -0.4, 0.4, 1.0, 0.4, // The back green one
        -0.5, -0.5, -0.4, 0.4, 1.0, 0.4,
        0.5, -0.5, -0.4, 1.0, 0.4, 0.4,

        0.5, 0.4, -0.2, 1.0, 0.4, 0.4, // The middle yellow one
        -0.5, 0.4, -0.2, 1.0, 1.0, 0.4,
        0.0, -0.6, -0.2, 1.0, 1.0, 0.4,

        0.0, 0.5, 0.0, 0.4, 0.4, 1.0,  // The front blue one 
        -0.5, -0.5, 0.0, 0.4, 0.4, 1.0,
        0.5, -0.5, 0.0, 1.0, 0.4, 0.4,

      
    ]);
    var n = 9;
    var FSIZE = vertices.BYTES_PER_ELEMENT;
    var vertexBuferr = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER,vertexBuferr);
    gl.bufferData(gl.ARRAY_BUFFER,vertices,gl.STATIC_DRAW);

    var u_ViewMatrix = gl.getUniformLocation(gl.program,'u_ViewMatrix');
    viewMatrix = new Matrix4();
    viewMatrix.setLookAt(0.2,0.25,0.25,0,0,0,0,1,0);
    gl.uniformMatrix4fv(u_ViewMatrix,false,viewMatrix.elements);

    var a_Position = gl.getAttribLocation(gl.program,'a_Position');
    gl.vertexAttribPointer(a_Position,3,gl.FLOAT,false,6*FSIZE,0);
    gl.enableVertexAttribArray(a_Position);

    var a_Color = gl.getAttribLocation(gl.program,'a_Color');
    gl.vertexAttribPointer(a_Color,3,gl.FLOAT,false,6*FSIZE,3*FSIZE);
    gl.enableVertexAttribArray(a_Color);
    return n;
}
