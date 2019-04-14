
var v_Shader =
    `
    attribute vec4 a_Position;
    attribute vec4 a_Color;
    uniform mat4 u_ViewMatrix;
    uniform mat4 u_ModelMatrix;
    varying vec4 v_Color;    
    void main() {
            gl_Position = u_ViewMatrix * u_ModelMatrix * a_Position;
            v_Color = a_Color;
        }
    `;

var f_Shader =
    `
    precision mediump float;
    varying vec4 v_Color;
    void main() {
        gl_FragColor = v_Color;
    }
    `;




function main() {

    var gl = document.getElementById('webgl').getContext('webgl');

    gl.clearColor(0.0, 0.0, 0.0, 1.0);
    gl.viewport(0, 0, gl.canvas.width, gl.canvas.height);
    gl.enable(gl.DEPTH_TEST);
    // gl.enable(gl.CULL_FACE);
    gl.cullFace(gl.BACK);


    var viewProgram = createProgram(gl, v_Shader, f_Shader);

    var viewProgramInfo = {
        positionLoc: gl.getAttribLocation(viewProgram, 'a_Position'),
        colorLoc: gl.getAttribLocation(viewProgram, 'a_Color'),
        viewMatrixLoc: gl.getUniformLocation(viewProgram, 'u_ViewMatrix'),
        modelMatrixLoc: gl.getUniformLocation(viewProgram, 'u_ModelMatrix'),
        verticesColorBuffer: gl.createBuffer()
    };

    var n = initVerticesColorBuffer(gl, viewProgramInfo);

    gl.useProgram(viewProgram);
    initUniform(gl, viewProgramInfo);





    var tick = function () {
        gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
        initUniform(gl, viewProgramInfo);



        gl.drawArrays(gl.TRIANGLES, 0, n);

        requestAnimationFrame(tick);
    };

    tick();

    
}

function initVerticesColorBuffer(gl, viewProgramInfo) {

    gl.bindBuffer(gl.ARRAY_BUFFER, viewProgramInfo.verticesColorBuffer);

    var verticesColor = new Float32Array([

        // 50, 50, -100, 0.4, 0.4, 1.0,  // The front blue one 
        // -50, -50, -100, 0.4, 0.4, 1.0,
        // 50, -50, -100, -1.0, 0.4, 0.4,

        // // Vertex coordinates and color(RGBA)
        // 0.0, 50, -102, 0.4, 1.0, 0.4, // The back green one
        // -50, -50, -102, 0.4, 1.0, 0.4,
        // 50, -50, -102, 1.0, 0.4, 0.4,

        // 50, 30, -103, 1.0, 0.4, 0.4, // The middle yellow one
        // -50, 40, -103, 1.0, 1.0, 0.4,
        // 0.0, -60, -103, 1.0, 1.0, 0.4,

         0, 0.5, 0.9, 1.0, 0.4, 0.4, // The middle yellow one
        -0.5, -0.5, 1.3, 1.0, 1.0, 0.4,
        0.5, -0.5, 1.0, 1.0, 1.0, 0.4,

    ]);

    gl.bufferData(gl.ARRAY_BUFFER, verticesColor, gl.STATIC_DRAW);

    const stride = verticesColor.BYTES_PER_ELEMENT * 6;
    const offset = verticesColor.BYTES_PER_ELEMENT * 3;
    gl.vertexAttribPointer(viewProgramInfo.positionLoc, 3, gl.FLOAT, false, stride, 0);
    gl.enableVertexAttribArray(viewProgramInfo.positionLoc);

    gl.vertexAttribPointer(viewProgramInfo.colorLoc, 3, gl.FLOAT, false, stride, offset);
    gl.enableVertexAttribArray(viewProgramInfo.colorLoc);

    gl.bindBuffer(gl.ARRAY_BUFFER, null);

    return 3;
}

var angle = Math.PI / 2;
function initUniform(gl, viewProgramInfo) {
    var projMatrix = new Matrix4();
    // projMatrix.setOrtho(-100, 100, -100, 100, 0, 103.1);
    // var viewMatrix = new Matrix4();
    // angle += 0.01;

    // y = Math.cos(angle);
    // z = Math.sin(angle);
    // y1 = z;
    // z1 = -y;
    // viewMatrix.setLookAt(0, y, z, 0, 0, 0, 0, y1, z1);
    // projMatrix.concat(viewMatrix);

    var modelMatrix = new Matrix4();


    // angle += 1;
    // modelMatrix.setRotate(angle, 0, 1, 0);
    gl.uniformMatrix4fv(viewProgramInfo.viewMatrixLoc, false, projMatrix.elements);
    gl.uniformMatrix4fv(viewProgramInfo.modelMatrixLoc, false, modelMatrix.elements);
}