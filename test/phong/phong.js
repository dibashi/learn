
var v_shader =
    `   
        attribute vec4 a_vertexPosition;
        attribute vec4 a_vertexNormal;
        attribute vec2 a_textureCoordinates;

        uniform mat4 u_modelViewMatrix;
        uniform mat4 u_projectionMatrix;
        uniform mat4 u_normalMatrix;

        varying vec3 v_positionEye3;
        varying vec3 v_normalEye;
        varying vec2 v_textureCoordinates;

        void main() {
            vec4 positionEye4 = u_modelViewMatrix * a_vertexPosition;
            v_positionEye3 = positionEye4.xyz/positionEye4.w;
            v_normalEye = normalize((u_normalMatrix * a_vertexNormal).xyz);
            gl_Position = u_projectionMatrix * u_modelViewMatrix * a_vertexPosition;

            v_textureCoordinates = a_textureCoordinates;
        }


    `;


var f_shader =
    `
        precision mediump float;

        varying vec3 v_positionEye3;
        varying vec3 v_normalEye;
        varying vec2 v_textureCoordinates;

        uniform vec3 u_lightPosition;
        uniform vec3 u_aimbientLightColor;
        uniform vec3 u_diffuseLightColor;
        uniform vec3 u_specularLightColor;

        uniform sampler2D u_sampler;

        const float shininess = 70.0;
        const vec4 color = vec4(0.6,0.7,1.0,1.0);
        void main() {
            vec3 vectorToLightSource = normalize(u_lightPosition - v_positionEye3);
            float diffuseLightWeighting = max(dot(v_normalEye,vectorToLightSource),0.0);

            vec3 reflectionVector = normalize(reflect(-vectorToLightSource,v_normalEye));
            vec3 viewVectorEye = -normalize(v_positionEye3);
            float rdotv = max(dot(reflectionVector,viewVectorEye),0.0);
            float specularLightWeighting = pow(rdotv,shininess);

            vec3 lightWeighting = u_aimbientLightColor + u_diffuseLightColor * diffuseLightWeighting + u_specularLightColor * specularLightWeighting;
            
            vec4 texelColor = texture2D(u_sampler,v_textureCoordinates);
            gl_FragColor = vec4(lightWeighting.rgb*texelColor.rgb,texelColor.a);
        }
    `;


/**
 * gl_FragColor = vec4(lightWeighting.rgb*texelColor.rgb,texelColor.a);
 * gl_FragColor = vec4(u_aimbientLightColor.rgb*texelColor.rgb,texelColor.a);
 *  gl_FragColor = vec4((u_diffuseLightColor * diffuseLightWeighting).rgb*texelColor.rgb,texelColor.a);
 * gl_FragColor = vec4((u_specularLightColor * specularLightWeighting).rgb*texelColor.rgb,texelColor.a);
 */
var gl;
var pwgl = {};
function main() {

    var canvas = document.getElementById("webgl");

    gl = canvas.getContext('webgl');

    init();
    draw();
}

function init() {
    setupShaders();
    setupBuffers();
    setupLights();
    setupTextures();
    gl.clearColor(0.3, 0.3, 0.3, 1.0);
    gl.enable(gl.DEPTH_TEST);
}

function setupShaders() {
    var phongProgram = createProgram(gl, v_shader, f_shader);
    gl.useProgram(phongProgram);

    pwgl.vertexPositionAttributeLoc = gl.getAttribLocation(phongProgram, 'a_vertexPosition');
    pwgl.vertexNormalAttributeLoc = gl.getAttribLocation(phongProgram, 'a_vertexNormal');
    pwgl.vertexTextureAttributeLoc = gl.getAttribLocation(phongProgram, 'a_textureCoordinates');

    pwgl.unformMVMatrixLoc = gl.getUniformLocation(phongProgram, 'u_modelViewMatrix');
    pwgl.uniformProjMatrixLoc = gl.getUniformLocation(phongProgram, 'u_projectionMatrix');
    pwgl.uniformNormalMatrixLoc = gl.getUniformLocation(phongProgram, 'u_normalMatrix');

    pwgl.uniformLightPositionLoc = gl.getUniformLocation(phongProgram, 'u_lightPosition');
    pwgl.uniformAmbientLightColorLoc = gl.getUniformLocation(phongProgram, 'u_aimbientLightColor');
    pwgl.uniformDiffuseLightColorLoc = gl.getUniformLocation(phongProgram, 'u_diffuseLightColor');
    pwgl.uniformSpecularLightColorLoc = gl.getUniformLocation(phongProgram, 'u_specularLightColor');

    pwgl.uniformSamplerLoc = gl.getUniformLocation(phongProgram, 'u_sampler');

    gl.enableVertexAttribArray(pwgl.vertexPositionAttributeLoc);
    gl.enableVertexAttribArray(pwgl.vertexNormalAttributeLoc);
    gl.enableVertexAttribArray(pwgl.vertexTextureAttributeLoc);

    pwgl.modelMatrix = new Matrix4();
    pwgl.viewMatrix = new Matrix4();
    pwgl.modelViewMatrix = new Matrix4();
    pwgl.projectionMatrix = new Matrix4();
    pwgl.normalMatrix = new Matrix4();

    pwgl.xR = 0.0;
    pwgl.yR = 0.0;
    pwgl.zR = 0.0;

}

function setupBuffers() {
    // Create a cube
    //    v6----- v5
    //   /|      /|
    //  v1------v0|
    //  | |     | |
    //  | |v7---|-|v4
    //  |/      |/
    //  v2------v3
    pwgl.cubeVertexPositionBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, pwgl.cubeVertexPositionBuffer);
    var cubeVertexPosition = [
        1.0, 1.0, 1.0, -1.0, 1.0, 1.0, -1.0, -1.0, 1.0, 1.0, -1.0, 1.0,  // v0-v1-v2-v3 front
        1.0, 1.0, 1.0, 1.0, -1.0, 1.0, 1.0, -1.0, -1.0, 1.0, 1.0, -1.0,  // v0-v3-v4-v5 right
        1.0, 1.0, 1.0, 1.0, 1.0, -1.0, -1.0, 1.0, -1.0, -1.0, 1.0, 1.0,  // v0-v5-v6-v1 up
        -1.0, 1.0, 1.0, -1.0, 1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, 1.0,  // v1-v6-v7-v2 left
        -1.0, -1.0, -1.0, 1.0, -1.0, -1.0, 1.0, -1.0, 1.0, -1.0, -1.0, 1.0,  // v7-v4-v3-v2 down
        1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, 1.0, -1.0, 1.0, 1.0, -1.0   // v4-v7-v6-v5 back
    ];


    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(cubeVertexPosition), gl.STATIC_DRAW);
    pwgl.CUBE_VERTEX_POS_BUF_ITEM_SIZE = 3;
    pwgl.CUBE_VERTEX_POS_BUF_NUM_ITEMS = 24;

    pwgl.cubeVertexNormalBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, pwgl.cubeVertexNormalBuffer);
    var cubeVertexNormal = [
        0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0,  // v0-v1-v2-v3 front
        1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0,  // v0-v3-v4-v5 right
        0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0,  // v0-v5-v6-v1 up
        -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0,  // v1-v6-v7-v2 left
        0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0,  // v7-v4-v3-v2 down
        0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0, 0.0, 0.0, -1.0   // v4-v7-v6-v5 back
    ];
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(cubeVertexNormal), gl.STATIC_DRAW);
    pwgl.CUBE_VERTEX_NORMAL_BUF_ITEM_SIZE = 3;
    pwgl.CUBE_VERTEX_NORMAL_BUF_NUM_ITEMS = 24;


    pwgl.cubeVertexTextureCoordinateBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, pwgl.cubeVertexTextureCoordinateBuffer);
    //前两个面的纹理坐标，我仔细算了，后面的胡乱弄的
    var textureCoordinates = [
        // 选择左下图
        0.25, 0.5,
        0.0, 0.5,
        0.0, 0.0,
        0.25, 0.0,

        // 选择中下图

        0.25, 0.5,
        0.25, 0.0,
        0.5, 0.0,
        0.5, 0.5,

        // 选择中右图
        0.25, 0.5,
        0.0, 0.5,
        0.0, 0.0,
        0.25, 0.0,

        // Create a cube
        //    v6----- v5
        //   /|      /|
        //  v1------v0|
        //  | |     | |
        //  | |v7---|-|v4
        //  |/      |/
        //  v2------v3
        // 选择左上图
        0.25, 0.5,
        0.25, 0.0,
        0.5, 0.0,
        0.5, 0.5,
        // 选择中上图
        0.25, 0.5,
        0.0, 0.5,
        0.0, 0.0,
        0.25, 0.0,

        // 选择右上图
        0.25, 0.5,
        0.25, 0.0,
        0.5, 0.0,
        0.5, 0.5,
    ];
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(textureCoordinates), gl.STATIC_DRAW);
    pwgl.CUBE_VERTEX_TEX_COORD_BUF_ITEM_SIZE = 2;
    pwgl.CUBE_VERTEX_TEX_COORD_BUF_NUM_ITEMS = 24;

    pwgl.cubeVertexIndexBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, pwgl.cubeVertexIndexBuffer);

    var cubeVertexIndices = [
        0, 1, 2, 0, 2, 3,    // front
        4, 5, 6, 4, 6, 7,    // right
        8, 9, 10, 8, 10, 11,    // up
        12, 13, 14, 12, 14, 15,    // left
        16, 17, 18, 16, 18, 19,    // down
        20, 21, 22, 20, 22, 23     // back
    ];
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, new Uint16Array(cubeVertexIndices), gl.STATIC_DRAW);
    pwgl.CUBE_VERTEX_INDEX_BUF_ITEM_SIZE = 1;
    pwgl.CUBE_VERTEX_INDEX_BUF_NUM_ITEMS = 36;

}

function setupLights() {

    gl.uniform3fv(pwgl.uniformLightPositionLoc, [0.0, 0.0, 20.0]);
    gl.uniform3fv(pwgl.uniformAmbientLightColorLoc, [0.2, 0.2, 0.2]);
    gl.uniform3fv(pwgl.uniformDiffuseLightColorLoc, [0.7, 0.7, 0.7]);
    gl.uniform3fv(pwgl.uniformSpecularLightColorLoc, [0.8, 0.8, 0.8]);

}

function setupTextures() {
    pwgl.cubeTexture = gl.createTexture();
    var image = new Image();
    image.onload = function () {
        gl.bindTexture(gl.TEXTURE_2D, pwgl.cubeTexture);
        gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGB, gl.RGB, gl.UNSIGNED_BYTE, image);
        gl.generateMipmap(gl.TEXTURE_2D);

        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    }

    image.src = "cube.jpg";
}


function draw() {
    requestAnimationFrame(draw);
    gl.viewport(0, 0, gl.canvas.width, gl.canvas.height);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
    //pwgl.modelMatrix.setScale(1, 1, 1);
    pwgl.xR += 0.2;
    pwgl.yR += 0.3;
    pwgl.zR += 0.4;
    pwgl.modelMatrix.setRotate(pwgl.zR, 0.0, 0.0, 1.0).rotate(pwgl.yR, 0.0, 1.0, 0.0).rotate(pwgl.xR, 1.0, 0.0, 0.0);
    // pwgl.modelMatrix.setRotate(pwgl.yR, 0.0, 1.0, 0.0);
    // pwgl.viewMatrix.setLookAt(10 * Math.sin(pwgl.yR), 0, 10 * Math.cos(pwgl.yR), 0, 0, 0, 0, 1, 0);
    pwgl.viewMatrix.setLookAt(0, 0, 10, 0, 0, 0, 0, 1, 0);
    pwgl.modelViewMatrix.set(pwgl.viewMatrix).concat(pwgl.modelMatrix);
    pwgl.projectionMatrix.setPerspective(45, gl.canvas.width / gl.canvas.height, 0.1, 100);
    pwgl.normalMatrix.setInverseOf(pwgl.modelViewMatrix).transpose();

    gl.uniformMatrix4fv(pwgl.unformMVMatrixLoc, false, pwgl.modelViewMatrix.elements);
    gl.uniformMatrix4fv(pwgl.uniformNormalMatrixLoc, false, pwgl.normalMatrix.elements);
    gl.uniformMatrix4fv(pwgl.uniformProjMatrixLoc, false, pwgl.projectionMatrix.elements);

    gl.uniform1i(pwgl.uniformSamplerLoc, 0);

    drawCube();
}

function drawCube() {

    gl.bindBuffer(gl.ARRAY_BUFFER, pwgl.cubeVertexPositionBuffer);
    gl.vertexAttribPointer(pwgl.vertexPositionAttributeLoc,
        pwgl.CUBE_VERTEX_POS_BUF_ITEM_SIZE,
        gl.FLOAT, false, 0, 0);

    // Bind normal buffer
    gl.bindBuffer(gl.ARRAY_BUFFER, pwgl.cubeVertexNormalBuffer);
    gl.vertexAttribPointer(pwgl.vertexNormalAttributeLoc,
        pwgl.CUBE_VERTEX_NORMAL_BUF_ITEM_SIZE,
        gl.FLOAT, false, 0, 0);

    gl.bindBuffer(gl.ARRAY_BUFFER, pwgl.cubeVertexTextureCoordinateBuffer);
    gl.vertexAttribPointer(pwgl.vertexTextureAttributeLoc,
        pwgl.CUBE_VERTEX_TEX_COORD_BUF_ITEM_SIZE,
        gl.FLOAT, false, 0, 0);

    gl.activeTexture(gl.TEXTURE0);
    gl.bindTexture(gl.TEXTURE_2D, pwgl.cubeTexture);

    // Bind index buffer and draw the floor                    
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, pwgl.cubeVertexIndexBuffer);
    gl.drawElements(gl.TRIANGLES, pwgl.CUBE_VERTEX_INDEX_BUF_NUM_ITEMS,
        gl.UNSIGNED_SHORT, 0);
}