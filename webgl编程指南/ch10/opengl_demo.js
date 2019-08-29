var textureVS = `#version 300 es

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColors;

out vec3 Colors;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() {
    Colors = aColors;
    gl_Position = projection * view * model * vec4(aPos, 1.0);
}
`;

var textureFS = `#version 300 es

precision mediump float;

out vec4 FragColor;

in vec3 Colors;

void main() {
    FragColor = vec4(Colors,1.0);
}

`;

var singleColorFS = `#version 300 es

precision mediump float;

out vec4 FragColor;

void main() {
    FragColor = vec4(0.0, 0.0, 0.3, 1.0);
}

`;

var gl, textureProgram, singleColorProgram, cubeVAO, planeVAO;

function main() {

    gl = document.getElementById("webgl").getContext("webgl2", { "stencil": true });

    gl.enable(gl.DEPTH_TEST);
    gl.depthFunc(gl.LESS);
    gl.enable(gl.STENCIL_TEST);

    gl.stencilOp(gl.KEEP, gl.KEEP, gl.REPLACE);

    gl.clearColor(1.0, 1.0, 1.0, 1.0);

    gl.clearStencil(0);

    gl.enable(gl.SCISSOR_TEST);

    gl.scissor(100, 100, 100, 100);

    textureProgram = createProgram(gl, textureVS, textureFS);
    singleColorProgram = createProgram(gl, textureVS, singleColorFS);

    textureProgram.model = gl.getUniformLocation(textureProgram, 'model');
    textureProgram.view = gl.getUniformLocation(textureProgram, 'view');
    textureProgram.projection = gl.getUniformLocation(textureProgram, 'projection');

    singleColorProgram.model = gl.getUniformLocation(singleColorProgram, 'model');
    singleColorProgram.view = gl.getUniformLocation(singleColorProgram, 'view');
    singleColorProgram.projection = gl.getUniformLocation(singleColorProgram, 'projection');

    var cubeVertices = [
        -0.5, -0.5, -0.5, 0.3, 0.0, 0.0,
        0.5, -0.5, -0.5, 0.3, 0.0, 0.0,
        0.5, 0.5, -0.5, 0.3, 0.0, 0.0,
        0.5, 0.5, -0.5, 0.3, 0.0, 0.0,
        -0.5, 0.5, -0.5, 0.3, 0.0, 0.0,
        -0.5, -0.5, -0.5, 0.3, 0.0, 0.0,
    ];

    var planeVertices = [
        // positions          // texture Coords (note we set these higher than 1 (together with GL_REPEAT as texture wrapping mode). this will cause the floor texture to repeat)
        5.0, -0.5, 5.0, 0.0, 0.3, 0.0,
        -5.0, -0.5, 5.0, 0.0, 0.3, 0.0,
        -5.0, -0.5, -5.0, 0.0, 0.3, 0.0,

        5.0, -0.5, 5.0, 0.0, 0.3, 0.0,
        -5.0, -0.5, -5.0, 0.0, 0.3, 0.0,
        5.0, -0.5, -5.0, 0.0, 0.3, 0.0,
    ];

    var cubeBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, cubeBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(cubeVertices), gl.STATIC_DRAW);

    cubeVAO = gl.createVertexArray();
    gl.bindVertexArray(cubeVAO);
    gl.bindBuffer(gl.ARRAY_BUFFER, cubeBuffer);
    gl.vertexAttribPointer(0, 3, gl.FLOAT, false, 6 * 4, 0);
    gl.enableVertexAttribArray(0);
    gl.vertexAttribPointer(1, 3, gl.FLOAT, false, 6 * 4, 3 * 4);
    gl.enableVertexAttribArray(1);

    var planeBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, planeBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(planeVertices), gl.STATIC_DRAW);

    planeVAO = gl.createVertexArray();
    gl.bindVertexArray(planeVAO);
    gl.bindBuffer(gl.ARRAY_BUFFER, planeBuffer);
    gl.vertexAttribPointer(0, 3, gl.FLOAT, false, 6 * 4, 0);
    gl.enableVertexAttribArray(0);
    gl.vertexAttribPointer(1, 3, gl.FLOAT, false, 6 * 4, 3 * 4);
    gl.enableVertexAttribArray(1);

    gl.bindVertexArray(null);

    function rand(max) {
        return Math.random() * max;
    }

    function draw() {

        var width = rand(gl.canvas.width);
        var height = rand(gl.canvas.height);
        var x = rand(gl.canvas.width - width);
        var y = rand(gl.canvas.height - height);
        gl.enable(gl.SCISSOR_TEST);
        gl.scissor(x, y, width, height);

        gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT | gl.STENCIL_BUFFER_BIT);



        var model = new Matrix4();
        var view = new Matrix4().setLookAt(0, 2, 3, 0, 0, 0, 0, 1, 0);
        var projection = new Matrix4().setPerspective(45, gl.canvas.width / gl.canvas.height, 0.1, 100);

        gl.useProgram(textureProgram);
        gl.uniformMatrix4fv(textureProgram.model, false, model.elements);
        gl.uniformMatrix4fv(textureProgram.view, false, view.elements);
        gl.uniformMatrix4fv(textureProgram.projection, false, projection.elements);

        gl.stencilFunc(gl.ALWAYS, 1, 0xff);
        gl.stencilMask(0x00);
        gl.bindVertexArray(planeVAO);
        gl.drawArrays(gl.TRIANGLES, 0, 6);


        gl.stencilMask(0xff);

        gl.bindVertexArray(cubeVAO);
        gl.drawArrays(gl.TRIANGLES, 0, 6);


        gl.stencilFunc(gl.NOTEQUAL, 1, 0xFF);
        gl.stencilMask(0x00);
        gl.disable(gl.DEPTH_TEST);

        gl.useProgram(singleColorProgram);
        var scale = 1.1;
        gl.bindVertexArray(cubeVAO);
        model = model.setScale(scale, scale, 1);
        gl.uniformMatrix4fv(singleColorProgram.model, false, model.elements);
        gl.uniformMatrix4fv(singleColorProgram.view, false, view.elements);
        gl.uniformMatrix4fv(singleColorProgram.projection, false, projection.elements);
        gl.drawArrays(gl.TRIANGLES, 0, 6);

        gl.stencilMask(0xff);
        gl.enable(gl.DEPTH_TEST);
        gl.bindVertexArray(null);

        //     var pixels = new Uint8Array(gl.drawingBufferWidth * gl.drawingBufferHeight*4 );
        //     gl.readPixels(0,0,gl.canvas.width,gl.canvas.height,gl.RGBA,gl.UNSIGNED_BYTE,pixels);
        //    console.log(pixels);

        requestAnimationFrame(draw);
    }

    draw();
}
