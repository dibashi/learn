/**
 * @author mr.doob / http://mrdoob.com/
 * @author supereggbert / http://www.paulbrunt.co.uk/
 * @author philogb / http://blog.thejit.org/
 * @author jordi_ros / http://plattsoft.com
 * @author D1plo1d / http://github.com/D1plo1d
 */

THREE.Matrix4 = function ( n11, n12, n13, n14, n21, n22, n23, n24, n31, n32, n33, n34, n41, n42, n43, n44 ) {

	this.n11 = n11 || 1; this.n12 = n12 || 0; this.n13 = n13 || 0; this.n14 = n14 || 0;
	this.n21 = n21 || 0; this.n22 = n22 || 1; this.n23 = n23 || 0; this.n24 = n24 || 0;
	this.n31 = n31 || 0; this.n32 = n32 || 0; this.n33 = n33 || 1; this.n34 = n34 || 0;
	this.n41 = n41 || 0; this.n42 = n42 || 0; this.n43 = n43 || 0; this.n44 = n44 || 1;

};

THREE.Matrix4.prototype = {

	identity: function () {

		this.n11 = 1; this.n12 = 0; this.n13 = 0; this.n14 = 0;
		this.n21 = 0; this.n22 = 1; this.n23 = 0; this.n24 = 0;
		this.n31 = 0; this.n32 = 0; this.n33 = 1; this.n34 = 0;
		this.n41 = 0; this.n42 = 0; this.n43 = 0; this.n44 = 1;

		return this;

	},

	set: function ( n11, n12, n13, n14, n21, n22, n23, n24, n31, n32, n33, n34, n41, n42, n43, n44 ) {

		this.n11 = n11; this.n12 = n12; this.n13 = n13; this.n14 = n14;
		this.n21 = n21; this.n22 = n22; this.n23 = n23; this.n24 = n24;
		this.n31 = n31; this.n32 = n32; this.n33 = n33; this.n34 = n34;
		this.n41 = n41; this.n42 = n42; this.n43 = n43; this.n44 = n44;

		return this;

	},

	copy: function ( m ) {

		this.n11 = m.n11; this.n12 = m.n12; this.n13 = m.n13; this.n14 = m.n14;
		this.n21 = m.n21; this.n22 = m.n22; this.n23 = m.n23; this.n24 = m.n24;
		this.n31 = m.n31; this.n32 = m.n32; this.n33 = m.n33; this.n34 = m.n34;
		this.n41 = m.n41; this.n42 = m.n42; this.n43 = m.n43; this.n44 = m.n44;

		return this;

	},

	lookAt: function ( eye, center, up ) {

		var x = new THREE.Vector3(), y = new THREE.Vector3(), z = new THREE.Vector3();

		z.sub( eye, center ).normalize();
		x.cross( up, z ).normalize();
		y.cross( z, x ).normalize();

		this.n11 = x.x; this.n12 = x.y; this.n13 = x.z; this.n14 = - x.dot( eye );
		this.n21 = y.x; this.n22 = y.y; this.n23 = y.z; this.n24 = - y.dot( eye );
		this.n31 = z.x; this.n32 = z.y; this.n33 = z.z; this.n34 = - z.dot( eye );
		this.n41 = 0; this.n42 = 0; this.n43 = 0; this.n44 = 1;

		return this;

	},

	multiplyVector3: function ( v ) {

		var vx = v.x, vy = v.y, vz = v.z,
		d = 1 / ( this.n41 * vx + this.n42 * vy + this.n43 * vz + this.n44 );

		v.x = ( this.n11 * vx + this.n12 * vy + this.n13 * vz + this.n14 ) * d;
		v.y = ( this.n21 * vx + this.n22 * vy + this.n23 * vz + this.n24 ) * d;
		v.z = ( this.n31 * vx + this.n32 * vy + this.n33 * vz + this.n34 ) * d;

		return v;

	},

	multiplyVector4: function ( v ) {

		var vx = v.x, vy = v.y, vz = v.z, vw = v.w;

		v.x = this.n11 * vx + this.n12 * vy + this.n13 * vz + this.n14 * vw;
		v.y = this.n21 * vx + this.n22 * vy + this.n23 * vz + this.n24 * vw;
		v.z = this.n31 * vx + this.n32 * vy + this.n33 * vz + this.n34 * vw;
		v.w = this.n41 * vx + this.n42 * vy + this.n43 * vz + this.n44 * vw;

		return v;

	},

	crossVector: function ( a ) {

		var v = new THREE.Vector4();

		v.x = this.n11 * a.x + this.n12 * a.y + this.n13 * a.z + this.n14 * a.w;
		v.y = this.n21 * a.x + this.n22 * a.y + this.n23 * a.z + this.n24 * a.w;
		v.z = this.n31 * a.x + this.n32 * a.y + this.n33 * a.z + this.n34 * a.w;

		v.w = ( a.w ) ? this.n41 * a.x + this.n42 * a.y + this.n43 * a.z + this.n44 * a.w : 1;

		return v;

	},

	multiply: function ( a, b ) {

		var a11 = a.n11, a12 = a.n12, a13 = a.n13, a14 = a.n14,
		a21 = a.n21, a22 = a.n22, a23 = a.n23, a24 = a.n24,
		a31 = a.n31, a32 = a.n32, a33 = a.n33, a34 = a.n34,
		a41 = a.n41, a42 = a.n42, a43 = a.n43, a44 = a.n44,

		b11 = b.n11, b12 = b.n12, b13 = b.n13, b14 = b.n14,
		b21 = b.n21, b22 = b.n22, b23 = b.n23, b24 = b.n24,
		b31 = b.n31, b32 = b.n32, b33 = b.n33, b34 = b.n34,
		b41 = b.n41, b42 = b.n42, b43 = b.n43, b44 = b.n44;

		this.n11 = a11 * b11 + a12 * b21 + a13 * b31 + a14 * b41;
		this.n12 = a11 * b12 + a12 * b22 + a13 * b32 + a14 * b42;
		this.n13 = a11 * b13 + a12 * b23 + a13 * b33 + a14 * b43;
		this.n14 = a11 * b14 + a12 * b24 + a13 * b34 + a14 * b44;

		this.n21 = a21 * b11 + a22 * b21 + a23 * b31 + a24 * b41;
		this.n22 = a21 * b12 + a22 * b22 + a23 * b32 + a24 * b42;
		this.n23 = a21 * b13 + a22 * b23 + a23 * b33 + a24 * b43;
		this.n24 = a21 * b14 + a22 * b24 + a23 * b34 + a24 * b44;

		this.n31 = a31 * b11 + a32 * b21 + a33 * b31 + a34 * b41;
		this.n32 = a31 * b12 + a32 * b22 + a33 * b32 + a34 * b42;
		this.n33 = a31 * b13 + a32 * b23 + a33 * b33 + a34 * b43;
		this.n34 = a31 * b14 + a32 * b24 + a33 * b34 + a34 * b44;

		this.n41 = a41 * b11 + a42 * b21 + a43 * b31 + a44 * b41;
		this.n42 = a41 * b12 + a42 * b22 + a43 * b32 + a44 * b42;
		this.n43 = a41 * b13 + a42 * b23 + a43 * b33 + a44 * b43;
		this.n44 = a41 * b14 + a42 * b24 + a43 * b34 + a44 * b44;

		return this;

	},

	multiplySelf: function ( m ) {

		var n11 = this.n11, n12 = this.n12, n13 = this.n13, n14 = this.n14,
		n21 = this.n21, n22 = this.n22, n23 = this.n23, n24 = this.n24,
		n31 = this.n31, n32 = this.n32, n33 = this.n33, n34 = this.n34,
		n41 = this.n41, n42 = this.n42, n43 = this.n43, n44 = this.n44,
		mn11 = m.n11, mn21 = m.n21, mn31 = m.n31, mn41 = m.n41,
		mn12 = m.n12, mn22 = m.n22, mn32 = m.n32, mn42 = m.n42,
		mn13 = m.n13, mn23 = m.n23, mn33 = m.n33, mn43 = m.n43,
		mn14 = m.n14, mn24 = m.n24, mn34 = m.n34, mn44 = m.n44;

		this.n11 = n11 * mn11 + n12 * mn21 + n13 * mn31 + n14 * mn41;
		this.n12 = n11 * mn12 + n12 * mn22 + n13 * mn32 + n14 * mn42;
		this.n13 = n11 * mn13 + n12 * mn23 + n13 * mn33 + n14 * mn43;
		this.n14 = n11 * mn14 + n12 * mn24 + n13 * mn34 + n14 * mn44;

		this.n21 = n21 * mn11 + n22 * mn21 + n23 * mn31 + n24 * mn41;
		this.n22 = n21 * mn12 + n22 * mn22 + n23 * mn32 + n24 * mn42;
		this.n23 = n21 * mn13 + n22 * mn23 + n23 * mn33 + n24 * mn43;
		this.n24 = n21 * mn14 + n22 * mn24 + n23 * mn34 + n24 * mn44;

		this.n31 = n31 * mn11 + n32 * mn21 + n33 * mn31 + n34 * mn41;
		this.n32 = n31 * mn12 + n32 * mn22 + n33 * mn32 + n34 * mn42;
		this.n33 = n31 * mn13 + n32 * mn23 + n33 * mn33 + n34 * mn43;
		this.n34 = n31 * mn14 + n32 * mn24 + n33 * mn34 + n34 * mn44;

		this.n41 = n41 * mn11 + n42 * mn21 + n43 * mn31 + n44 * mn41;
		this.n42 = n41 * mn12 + n42 * mn22 + n43 * mn32 + n44 * mn42;
		this.n43 = n41 * mn13 + n42 * mn23 + n43 * mn33 + n44 * mn43;
		this.n44 = n41 * mn14 + n42 * mn24 + n43 * mn34 + n44 * mn44;

		return this;

	},

	multiplyScalar: function ( s ) {

		this.n11 *= s; this.n12 *= s; this.n13 *= s; this.n14 *= s;
		this.n21 *= s; this.n22 *= s; this.n23 *= s; this.n24 *= s;
		this.n31 *= s; this.n32 *= s; this.n33 *= s; this.n34 *= s;
		this.n41 *= s; this.n42 *= s; this.n43 *= s; this.n44 *= s;

		return this;

	},

	determinant: function () {

		//TODO: make this more efficient
		//( based on http://www.euclideanspace.com/maths/algebra/matrix/functions/inverse/fourD/index.htm )
		return (
			this.n14 * this.n23 * this.n32 * this.n41-
			this.n13 * this.n24 * this.n32 * this.n41-
			this.n14 * this.n22 * this.n33 * this.n41+
			this.n12 * this.n24 * this.n33 * this.n41+

			this.n13 * this.n22 * this.n34 * this.n41-
			this.n12 * this.n23 * this.n34 * this.n41-
			this.n14 * this.n23 * this.n31 * this.n42+
			this.n13 * this.n24 * this.n31 * this.n42+

			this.n14 * this.n21 * this.n33 * this.n42-
			this.n11 * this.n24 * this.n33 * this.n42-
			this.n13 * this.n21 * this.n34 * this.n42+
			this.n11 * this.n23 * this.n34 * this.n42+

			this.n14 * this.n22 * this.n31 * this.n43-
			this.n12 * this.n24 * this.n31 * this.n43-
			this.n14 * this.n21 * this.n32 * this.n43+
			this.n11 * this.n24 * this.n32 * this.n43+

			this.n12 * this.n21 * this.n34 * this.n43-
			this.n11 * this.n22 * this.n34 * this.n43-
			this.n13 * this.n22 * this.n31 * this.n44+
			this.n12 * this.n23 * this.n31 * this.n44+

			this.n13 * this.n21 * this.n32 * this.n44-
			this.n11 * this.n23 * this.n32 * this.n44-
			this.n12 * this.n21 * this.n33 * this.n44+
			this.n11 * this.n22 * this.n33 * this.n44 );

	},

	transpose: function () {

		function swap( obj, p1, p2 ) {

			var aux = obj[ p1 ];
			obj[ p1 ] = obj[ p2 ];
			obj[ p2 ] = aux;

		}

		swap( this, 'n21', 'n12' );
		swap( this, 'n31', 'n13' );
		swap( this, 'n32', 'n23' );
		swap( this, 'n41', 'n14' );
		swap( this, 'n42', 'n24' );
		swap( this, 'n43', 'n34' );

		return this;

	},

	clone: function () {

		var m = new THREE.Matrix4();

		m.n11 = this.n11; m.n12 = this.n12; m.n13 = this.n13; m.n14 = this.n14;
		m.n21 = this.n21; m.n22 = this.n22; m.n23 = this.n23; m.n24 = this.n24;
		m.n31 = this.n31; m.n32 = this.n32; m.n33 = this.n33; m.n34 = this.n34;
		m.n41 = this.n41; m.n42 = this.n42; m.n43 = this.n43; m.n44 = this.n44;

		return m;

	},

	flatten: function() {

		return [ this.n11, this.n21, this.n31, this.n41,
			  this.n12, this.n22, this.n32, this.n42,
			  this.n13, this.n23, this.n33, this.n43,
			  this.n14, this.n24, this.n34, this.n44 ];

	},

	toString: function() {

		return  "| " + this.n11 + " " + this.n12 + " " + this.n13 + " " + this.n14 + " |\n" +
			"| " + this.n21 + " " + this.n22 + " " + this.n23 + " " + this.n24 + " |\n" +
			"| " + this.n31 + " " + this.n32 + " " + this.n33 + " " + this.n34 + " |\n" +
			"| " + this.n41 + " " + this.n42 + " " + this.n43 + " " + this.n44 + " |";

	}

};

THREE.Matrix4.translationMatrix = function ( x, y, z ) {

	var m = new THREE.Matrix4();

	m.n14 = x;
	m.n24 = y;
	m.n34 = z;

	return m;

};

THREE.Matrix4.scaleMatrix = function ( x, y, z ) {

	var m = new THREE.Matrix4();

	m.n11 = x;
	m.n22 = y;
	m.n33 = z;

	return m;

};

THREE.Matrix4.rotationXMatrix = function ( theta ) {

	var rot = new THREE.Matrix4();

	rot.n22 = rot.n33 = Math.cos( theta );
	rot.n32 = Math.sin( theta );
	rot.n23 = - rot.n32;

	return rot;

};

THREE.Matrix4.rotationYMatrix = function ( theta ) {

	var rot = new THREE.Matrix4();

	rot.n11 = rot.n33 = Math.cos( theta );
	rot.n13 = Math.sin( theta );
	rot.n31 = - rot.n13;

	return rot;

};

THREE.Matrix4.rotationZMatrix = function ( theta ) {

	var rot = new THREE.Matrix4();

	rot.n11 = rot.n22 = Math.cos( theta );
	rot.n21 = Math.sin( theta );
	rot.n12 = - rot.n21;

	return rot;

};

THREE.Matrix4.rotationAxisAngleMatrix = function ( axis, angle ) {

	//Based on http://www.gamedev.net/reference/articles/article1199.asp

	var rot = new THREE.Matrix4(),
	c = Math.cos( angle ),
	s = Math.sin( angle ),
	t = 1 - c,
	x = axis.x, y = axis.y, z = axis.z;

	rot.n11 = t * x * x + c;
	rot.n12 = t * x * y - s * z;
	rot.n13 = t * x * z + s * y;
	rot.n21 = t * x * y + s * z;
	rot.n22 = t * y * y + c;
	rot.n23 = t * y * z - s * x;
	rot.n31 = t * x * z - s * y;
	rot.n32 = t * y * z + s * x;
	rot.n33 = t * z * z + c;

	return rot;

};

THREE.Matrix4.makeInvert = function ( m1 ) {

	//TODO: make this more efficient
	//( based on http://www.euclideanspace.com/maths/algebra/matrix/functions/inverse/fourD/index.htm )
	var m2 = new THREE.Matrix4();

	m2.n11 = m1.n23*m1.n34*m1.n42 - m1.n24*m1.n33*m1.n42 + m1.n24*m1.n32*m1.n43 - m1.n22*m1.n34*m1.n43 - m1.n23*m1.n32*m1.n44 + m1.n22*m1.n33*m1.n44;
	m2.n12 = m1.n14*m1.n33*m1.n42 - m1.n13*m1.n34*m1.n42 - m1.n14*m1.n32*m1.n43 + m1.n12*m1.n34*m1.n43 + m1.n13*m1.n32*m1.n44 - m1.n12*m1.n33*m1.n44;
	m2.n13 = m1.n13*m1.n24*m1.n42 - m1.n14*m1.n23*m1.n42 + m1.n14*m1.n22*m1.n43 - m1.n12*m1.n24*m1.n43 - m1.n13*m1.n22*m1.n44 + m1.n12*m1.n23*m1.n44;
	m2.n14 = m1.n14*m1.n23*m1.n32 - m1.n13*m1.n24*m1.n32 - m1.n14*m1.n22*m1.n33 + m1.n12*m1.n24*m1.n33 + m1.n13*m1.n22*m1.n34 - m1.n12*m1.n23*m1.n34;
	m2.n21 = m1.n24*m1.n33*m1.n41 - m1.n23*m1.n34*m1.n41 - m1.n24*m1.n31*m1.n43 + m1.n21*m1.n34*m1.n43 + m1.n23*m1.n31*m1.n44 - m1.n21*m1.n33*m1.n44;
	m2.n22 = m1.n13*m1.n34*m1.n41 - m1.n14*m1.n33*m1.n41 + m1.n14*m1.n31*m1.n43 - m1.n11*m1.n34*m1.n43 - m1.n13*m1.n31*m1.n44 + m1.n11*m1.n33*m1.n44;
	m2.n23 = m1.n14*m1.n23*m1.n41 - m1.n13*m1.n24*m1.n41 - m1.n14*m1.n21*m1.n43 + m1.n11*m1.n24*m1.n43 + m1.n13*m1.n21*m1.n44 - m1.n11*m1.n23*m1.n44;
	m2.n24 = m1.n13*m1.n24*m1.n31 - m1.n14*m1.n23*m1.n31 + m1.n14*m1.n21*m1.n33 - m1.n11*m1.n24*m1.n33 - m1.n13*m1.n21*m1.n34 + m1.n11*m1.n23*m1.n34;
	m2.n31 = m1.n22*m1.n34*m1.n41 - m1.n24*m1.n32*m1.n41 + m1.n24*m1.n31*m1.n42 - m1.n21*m1.n34*m1.n42 - m1.n22*m1.n31*m1.n44 + m1.n21*m1.n32*m1.n44;
	m2.n32 = m1.n14*m1.n32*m1.n41 - m1.n12*m1.n34*m1.n41 - m1.n14*m1.n31*m1.n42 + m1.n11*m1.n34*m1.n42 + m1.n12*m1.n31*m1.n44 - m1.n11*m1.n32*m1.n44;
	m2.n33 = m1.n13*m1.n24*m1.n41 - m1.n14*m1.n22*m1.n41 + m1.n14*m1.n21*m1.n42 - m1.n11*m1.n24*m1.n42 - m1.n12*m1.n21*m1.n44 + m1.n11*m1.n22*m1.n44;
	m2.n34 = m1.n14*m1.n22*m1.n31 - m1.n12*m1.n24*m1.n31 - m1.n14*m1.n21*m1.n32 + m1.n11*m1.n24*m1.n32 + m1.n12*m1.n21*m1.n34 - m1.n11*m1.n22*m1.n34;
	m2.n41 = m1.n23*m1.n32*m1.n41 - m1.n22*m1.n33*m1.n41 - m1.n23*m1.n31*m1.n42 + m1.n21*m1.n33*m1.n42 + m1.n22*m1.n31*m1.n43 - m1.n21*m1.n32*m1.n43;
	m2.n42 = m1.n12*m1.n33*m1.n41 - m1.n13*m1.n32*m1.n41 + m1.n13*m1.n31*m1.n42 - m1.n11*m1.n33*m1.n42 - m1.n12*m1.n31*m1.n43 + m1.n11*m1.n32*m1.n43;
	m2.n43 = m1.n13*m1.n22*m1.n41 - m1.n12*m1.n23*m1.n41 - m1.n13*m1.n21*m1.n42 + m1.n11*m1.n23*m1.n42 + m1.n12*m1.n21*m1.n43 - m1.n11*m1.n22*m1.n43;
	m2.n44 = m1.n12*m1.n23*m1.n31 - m1.n13*m1.n22*m1.n31 + m1.n13*m1.n21*m1.n32 - m1.n11*m1.n23*m1.n32 - m1.n12*m1.n21*m1.n33 + m1.n11*m1.n22*m1.n33;
	m2.multiplyScalar( 1 / m1.determinant() );

	return m2;

};

THREE.Matrix4.makeInvert3x3 = function ( m1 ) {

	// input:  THREE.Matrix4, output: THREE.Matrix3
	// ( based on http://code.google.com/p/webgl-mjs/ )

	var m = m1.flatten(),
	m2 = new THREE.Matrix3(),

	a11 = m[ 10 ] * m[ 5 ] - m[ 6 ] * m[ 9 ],
	a21 = - m[ 10 ] * m[ 1 ] + m[ 2 ] * m[ 9 ],
	a31 = m[ 6 ] * m[ 1 ] - m[ 2 ] * m[ 5 ],
	a12 = - m[ 10 ] * m[ 4 ] + m[ 6 ] * m[ 8 ],
	a22 = m[ 10 ] * m[ 0 ] - m[ 2 ] * m[ 8 ],
	a32 = - m[ 6 ] * m[ 0 ] + m[ 2 ] * m[ 4 ],
	a13 = m[ 9 ] * m[ 4 ] - m[ 5 ] * m[ 8 ],
	a23 = - m[ 9 ] * m[ 0 ] + m[ 1 ] * m[ 8 ],
	a33 = m[ 5 ] * m[ 0 ] - m[ 1 ] * m[ 4 ],
	det = m[ 0 ] * ( a11 ) + m[ 1 ] * ( a12 ) + m[ 2 ] * ( a13 ),
	idet;

	// no inverse
	if (det == 0) throw "matrix not invertible";

	idet = 1.0 / det;

	m2.m[ 0 ] = idet * a11; m2.m[ 1 ] = idet * a21; m2.m[ 2 ] = idet * a31;
	m2.m[ 3 ] = idet * a12; m2.m[ 4 ] = idet * a22; m2.m[ 5 ] = idet * a32;
	m2.m[ 6 ] = idet * a13; m2.m[ 7 ] = idet * a23; m2.m[ 8 ] = idet * a33;

	return m2;

}

THREE.Matrix4.makeFrustum = function( left, right, bottom, top, near, far ) {

	var m, x, y, a, b, c, d;

	m = new THREE.Matrix4();
	x = 2 * near / ( right - left );
	y = 2 * near / ( top - bottom );
	a = ( right + left ) / ( right - left );
	b = ( top + bottom ) / ( top - bottom );
	c = - ( far + near ) / ( far - near );
	d = - 2 * far * near / ( far - near );

	m.n11 = x;  m.n12 = 0;  m.n13 = a;   m.n14 = 0;
	m.n21 = 0;  m.n22 = y;  m.n23 = b;   m.n24 = 0;
	m.n31 = 0;  m.n32 = 0;  m.n33 = c;   m.n34 = d;
	m.n41 = 0;  m.n42 = 0;  m.n43 = - 1; m.n44 = 0;

	return m;

};

THREE.Matrix4.makePerspective = function( fov, aspect, near, far ) {

	var ymax, ymin, xmin, xmax;

	ymax = near * Math.tan( fov * Math.PI / 360 );
	ymin = - ymax;
	xmin = ymin * aspect;
	xmax = ymax * aspect;

	return THREE.Matrix4.makeFrustum( xmin, xmax, ymin, ymax, near, far );

};

THREE.Matrix4.makeOrtho = function( left, right, top, bottom, near, far ) {

	var m, x, y, z, w, h, p;

	m = new THREE.Matrix4();
	w = right - left;
	h = top - bottom;
	p = far - near;
	x = ( right + left ) / w;
	y = ( top + bottom ) / h;
	z = ( far + near ) / p;

	m.n11 = 2 / w; m.n12 = 0;     m.n13 = 0;      m.n14 = -x;
	m.n21 = 0;     m.n22 = 2 / h; m.n23 = 0;      m.n24 = -y;
	m.n31 = 0;     m.n32 = 0;     m.n33 = -2 / p; m.n34 = -z;
	m.n41 = 0;     m.n42 = 0;     m.n43 = 0;      m.n44 = 1;

	return m;

};
