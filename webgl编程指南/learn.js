var VSHADER_SOURCE =
    'attribute vec4 a_Position;\n' +
    'attribute vec4 a_Color;\n' +
    'uniform mat4 u_projectMatrix;\n' +
    'varying vec4 v_Color;\n' +
    'void main(){\n' +
    '      gl_Position = u_projectMatrix*a_Position;\n' +
    '      v_Color = a_Color;\n' +
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

    var nf = document.getElementById("nf");

    initShaders(gl, VSHADER_SOURCE, FSHADER_SOURCE);

    var n = initVertexBuffers(gl);

    document.onkeydown = function (ev) {
        keydown(ev, gl, n, u_projectMatrix, projectMatrix,nf);
    }

    // var u_ViewMatrix = gl.getUniformLocation(gl.program,'u_ViewMatrix');
    var u_projectMatrix = gl.getUniformLocation(gl.program, 'u_projectMatrix');
    var projectMatrix = new Matrix4();

    draw(gl, n, u_projectMatrix, projectMatrix,nf);

}

var g_near = 0.2; var g_far = 1.0;
function keydown(ev, gl, n, u_projectMatrix, projectMatrix,nf) {
    if (ev.keyCode == 37) {
        g_near -= 0.01;
    } else if (ev.keyCode == 39) {
        g_near += 0.01;
    } else if (ev.keyCode == 38) {
        g_far += 0.01;
    } else if (ev.keyCode == 40) {
        g_far -= 0.01;
    }
    else {
        return;
    }

    
    draw(gl, n, u_projectMatrix, projectMatrix,nf);
}


function draw(gl, n, u_projectMatrix, projectMatrix,nf) {
    projectMatrix.setOrtho(-1, 1, -1, 1, g_near, g_far);
    gl.uniformMatrix4fv(u_projectMatrix, false, projectMatrix.elements);
    gl.clearColor(0.0, 0.0, 0.0, 1.0);
    gl.clear(gl.COLOR_BUFFER_BIT);
    gl.drawArrays(gl.TRIANGLES, 0, n);

    nf.innerHTML = "g_near = " + g_near  + " g_far = " + g_far;
}



function initVertexBuffers(gl) {
    var vertices = new Float32Array([


        0.0, 0.6, -0.9, 0.4, 1.0, 0.4, // The back green one
        -0.6, -0.6, -0.4, 0.4, 1.0, 0.4,
        0.6, -0.6, -0.4, 1.0, 0.4, 0.4,

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
    gl.bindBuffer(gl.ARRAY_BUFFER, vertexBuferr);
    gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);


    var a_Position = gl.getAttribLocation(gl.program, 'a_Position');
    gl.vertexAttribPointer(a_Position, 3, gl.FLOAT, false, 6 * FSIZE, 0);
    gl.enableVertexAttribArray(a_Position);

    var a_Color = gl.getAttribLocation(gl.program, 'a_Color');
    gl.vertexAttribPointer(a_Color, 3, gl.FLOAT, false, 6 * FSIZE, 3 * FSIZE);
    gl.enableVertexAttribArray(a_Color);
    return n;
}
