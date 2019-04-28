var vertexGLSL = `
uniform float uTime;

uniform vec3 uFirePos;

attribute float aLifetime;

attribute vec2 aTextureCoords;

attribute vec2 aTriCorner;

attribute vec3 aCenterOffset;

attribute vec3 aVelocity;

uniform mat4 uPMatrix;
uniform mat4 uViewMatrix;

uniform bool uUseBillboarding;

varying float vLifetime;
varying vec2 vTextureCoords;

void main (void) {
  float time = mod(uTime, aLifetime);

  vec4 position = vec4(
    uFirePos + aCenterOffset + (time * aVelocity),
    1.0
  );

  vLifetime = 1.3 - (time / aLifetime);
  vLifetime = clamp(vLifetime, 0.0, 1.0);
  float size = (vLifetime * vLifetime) * 0.05;

  if (uUseBillboarding) {
    vec3 cameraRight = vec3(
      uViewMatrix[0].x, uViewMatrix[1].x, uViewMatrix[2].x
    );
    vec3 cameraUp = vec3(
      uViewMatrix[0].y, uViewMatrix[1].y, uViewMatrix[2].y
    );

    position.xyz += (cameraRight * aTriCorner.x * size) +
     (cameraUp * aTriCorner.y * size);
  } else {
    position.xy += aTriCorner.xy * size;
  }

  gl_Position = uPMatrix * uViewMatrix * position;

  vTextureCoords = aTextureCoords;
  vLifetime = aLifetime;
}`;

var fragmentGLSL = `
precision mediump float;

uniform vec4 uColor;

uniform float uTimeFrag;

varying float vLifetime;
varying vec2 vTextureCoords;

uniform sampler2D fireAtlas;

void main (void) {
  float time = mod(uTimeFrag, vLifetime);
  float percentOfLife = time / vLifetime;
  percentOfLife = clamp(percentOfLife, 0.0, 1.0);

  float offset = floor(16.0 * percentOfLife);
  float offsetX = floor(mod(offset, 4.0)) / 4.0;
  float offsetY = 0.75 - floor(offset / 4.0) / 4.0;

  vec4 texColor = texture2D(
    fireAtlas, 
    vec2(
      (vTextureCoords.x / 4.0) + offsetX,
      (vTextureCoords.y / 4.0) + offsetY
  ));
  gl_FragColor = uColor * texColor;

  gl_FragColor.a *= vLifetime;
}
`;

function main() {

    var billboardingEnabled = true

    var canvas = document.getElementById('webgl')
    var mountLocation = document.getElementById(
        'webgl-particle-effect-tutorial'
    ) || document.body
    var billboardButton = document.createElement('button')
    billboardButton.innerHTML = 'Click to disable billboarding'
    billboardButton.style.display = 'block'
    billboardButton.style.cursor = 'pointer'
    billboardButton.style.marginBottom = '3px'
    billboardButton.style.height = '40px'
    billboardButton.style.width = '160px'
    billboardButton.onclick = function () {
        billboardingEnabled = !billboardingEnabled
        billboardButton.innerHTML = (
            billboardingEnabled ?
                'Click to disable billboarding' :
                'Click to enable billboarding'
        )
    }
    mountLocation.appendChild(billboardButton);


    var isDragging = false

    var xRotation = 0
    var yRotation = 0

    var lastMouseX = 0
    var lastMouseY = 0

    canvas.onmousedown = function (e) {
        isDragging = true
        lastMouseX = e.pageX
        lastMouseY = e.pageY
    }
    canvas.onmousemove = function (e) {
        if (isDragging) {
            console.log("ghpog");
            xRotation += (e.pageY - lastMouseY) / 50
            yRotation -= (e.pageX - lastMouseX) / 50

            xRotation = Math.min(xRotation, Math.PI / 2.5)
            xRotation = Math.max(xRotation, -Math.PI / 2.5)

            lastMouseX = e.pageX
            lastMouseY = e.pageY
        }
    }
    canvas.onmouseup = function (e) {
        isDragging = false
    }

    var gl = canvas.getContext('webgl')
    gl.clearColor(0.3, 0.3, 0.3, 1.0);
    gl.clear(gl.COLOR_BUFFER_BIT);
    // gl.viewport(0, 0, 500, 500);

    var vertexShader = gl.createShader(gl.VERTEX_SHADER)
    gl.shaderSource(vertexShader, vertexGLSL)
    gl.compileShader(vertexShader)

    var fragmentShader = gl.createShader(gl.FRAGMENT_SHADER)
    gl.shaderSource(fragmentShader, fragmentGLSL)
    gl.compileShader(fragmentShader)

    var shaderProgram = gl.createProgram()
    gl.attachShader(shaderProgram, vertexShader)
    gl.attachShader(shaderProgram, fragmentShader)
    gl.linkProgram(shaderProgram)
    gl.useProgram(shaderProgram)

    var lifetimeAttrib = gl.getAttribLocation(
        shaderProgram, 'aLifetime'
    )
    var texCoordAttrib = gl.getAttribLocation(
        shaderProgram, 'aTextureCoords'
    )
    var triCornerAttrib = gl.getAttribLocation(
        shaderProgram, 'aTriCorner'
    )
    var centerOffsetAttrib = gl.getAttribLocation(
        shaderProgram, 'aCenterOffset'
    )
    var velocityAttrib = gl.getAttribLocation(
        shaderProgram, 'aVelocity'
    )
    gl.enableVertexAttribArray(lifetimeAttrib)
    gl.enableVertexAttribArray(texCoordAttrib)
    gl.enableVertexAttribArray(triCornerAttrib)
    gl.enableVertexAttribArray(centerOffsetAttrib)
    gl.enableVertexAttribArray(velocityAttrib)

    var timeUni = gl.getUniformLocation(shaderProgram, 'uTime')
    var timeUniFrag = gl.getUniformLocation(shaderProgram, 'uTimeFrag')
    var firePosUni = gl.getUniformLocation(shaderProgram, 'uFirePos')
    var perspectiveUni = gl.getUniformLocation(shaderProgram, 'uPMatrix')
    var viewUni = gl.getUniformLocation(shaderProgram, 'uViewMatrix')
    var colorUni = gl.getUniformLocation(shaderProgram, 'uColor')
    var fireAtlasUni = gl.getUniformLocation(shaderProgram, 'uFireAtlas')
    var useBillboardUni = gl.getUniformLocation(
        shaderProgram, 'uUseBillboarding'
    )

    var imageIsLoaded = false
    var fireTexture = gl.createTexture()
    var fireAtlas = new window.Image()
    fireAtlas.onload = function () {
        gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true)
        gl.bindTexture(gl.TEXTURE_2D, fireTexture)
        gl.texImage2D(
            gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, fireAtlas
        )
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR)
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR)
        imageIsLoaded = true
    }
    fireAtlas.src = 'fire-texture-atlas.jpg'


    var numParticles = 1000
    var lifetimes = []
    var triCorners = []
    var texCoords = []
    var vertexIndices = []
    var centerOffsets = []
    var velocities = []

    var triCornersCycle = [
        -1.0, -1.0,
        1.0, -1.0,
        1.0, 1.0,
        -1.0, 1.0
    ]
    var texCoordsCycle = [
        0, 0,
        1, 0,
        1, 1,
        0, 1
    ]

    for (var i = 0; i < numParticles; i++) {
        var lifetime = 8 * Math.random()

        var diameterAroundCenter = 0.5
        var halfDiameterAroundCenter = diameterAroundCenter / 2

        var xStartOffset = diameterAroundCenter *
            Math.random() - halfDiameterAroundCenter
        xStartOffset /= 3

        var yStartOffset = diameterAroundCenter *
            Math.random() - halfDiameterAroundCenter
        yStartOffset /= 10

        var zStartOffset = diameterAroundCenter *
            Math.random() - halfDiameterAroundCenter
        zStartOffset /= 3

        var upVelocity = 0.1 * Math.random()

        var xSideVelocity = 0.02 * Math.random()
        if (xStartOffset > 0) {
            xSideVelocity *= -1
        }

        var zSideVelocity = 0.02 * Math.random()
        if (zStartOffset > 0) {
            zSideVelocity *= -1
        }

        for (var j = 0; j < 4; j++) {
            lifetimes.push(lifetime)

            triCorners.push(triCornersCycle[j * 2])
            triCorners.push(triCornersCycle[j * 2 + 1])

            texCoords.push(texCoordsCycle[j * 2])
            texCoords.push(texCoordsCycle[j * 2 + 1])

            centerOffsets.push(xStartOffset)
            centerOffsets.push(yStartOffset + Math.abs(xStartOffset / 2.0))
            centerOffsets.push(zStartOffset)

            velocities.push(xSideVelocity)
            velocities.push(upVelocity)
            velocities.push(zSideVelocity)
        }

        vertexIndices = vertexIndices.concat([
            0, 1, 2, 0, 2, 3
        ].map(function (num) { return num + 4 * i }))
    }


    function createBuffer(bufferType, DataType, data) {
        var buffer = gl.createBuffer()
        gl.bindBuffer(gl[bufferType], buffer)
        gl.bufferData(gl[bufferType], new DataType(data), gl.STATIC_DRAW)
        return buffer
    }
    createBuffer('ARRAY_BUFFER', Float32Array, lifetimes)
    gl.vertexAttribPointer(lifetimeAttrib, 1, gl.FLOAT, false, 0, 0)

    createBuffer('ARRAY_BUFFER', Float32Array, texCoords)
    gl.vertexAttribPointer(texCoordAttrib, 2, gl.FLOAT, false, 0, 0)

    createBuffer('ARRAY_BUFFER', Float32Array, triCorners)
    gl.vertexAttribPointer(triCornerAttrib, 2, gl.FLOAT, false, 0, 0)

    createBuffer('ARRAY_BUFFER', Float32Array, centerOffsets)
    gl.vertexAttribPointer(centerOffsetAttrib, 3, gl.FLOAT, false, 0, 0)

    createBuffer('ARRAY_BUFFER', Float32Array, velocities)
    gl.vertexAttribPointer(velocityAttrib, 3, gl.FLOAT, false, 0, 0)

    createBuffer('ELEMENT_ARRAY_BUFFER', Uint16Array, vertexIndices)

    gl.enable(gl.BLEND)
    gl.blendFunc(gl.ONE, gl.ONE)

    gl.activeTexture(gl.TEXTURE0)
    gl.bindTexture(gl.TEXTURE_2D, fireTexture)
    gl.uniform1i(fireAtlasUni, 0)

    gl.uniformMatrix4fv(
        perspectiveUni,
        false,
        new Matrix4().setPerspective(60, 1, 0.01, 1000).elements
    )


    function createCamera() {
        var camera = new Matrix4();

        camera.translate(0, 0.25, 1);

console.log(yRotation);
        //camera.rotate(-xRotation, 1, 0, 0);
        camera.rotate(yRotation*300, 0, 1, 0);
        camera.setLookAt(camera.elements[12], camera.elements[13], camera.elements[14], 0, 0, 0, 0, 1, 0)

        return camera
    }




    var previousTime = new Date().getTime()

    var clockTime = 3

    var redFirePos = [0.0, 0.0, 0.0]
    var redFireColor = [0.8, 0.25, 0.25, 1.0]

    var purpFirePos = [0.5, 0.0, 0.0]
    var purpFireColor = [0.25, 0.25, 8.25, 1.0]

    function draw() {
        if (imageIsLoaded) {
            gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT)

            var currentTime = new Date().getTime()
            clockTime += (currentTime - previousTime) / 1000
            previousTime = currentTime

            gl.uniform1f(timeUni, clockTime)
            gl.uniform1f(timeUniFrag, clockTime)

            gl.uniformMatrix4fv(viewUni, false, createCamera().elements);

            gl.uniform1i(useBillboardUni, billboardingEnabled)

            gl.uniform3fv(firePosUni, redFirePos)
            gl.uniform4fv(colorUni, redFireColor)

            gl.drawElements(gl.TRIANGLES, numParticles * 6, gl.UNSIGNED_SHORT, 0)

            gl.uniform3fv(firePosUni, purpFirePos)
            gl.uniform4fv(colorUni, purpFireColor)

            gl.drawElements(gl.TRIANGLES, numParticles * 6, gl.UNSIGNED_SHORT, 0)
        }

        window.requestAnimationFrame(draw)
    }
    draw()
}