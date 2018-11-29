/**
 * @author mr.doob / http://mrdoob.com/
 *
 * parameters = {
 *  color: <hex>,
 *  opacity: <float>,
 *  blending: THREE.NormalBlending
 * }
 */

THREE.ParticleCircleMaterial = function ( parameters ) {

	this.color = new THREE.Color( 0xffffff );
	this.opacity = 1;
	this.blending = THREE.NormalBlending;

	if ( parameters ) {

		if ( parameters.color !== undefined ) this.color.setHex( parameters.color );
		if ( parameters.opacity !== undefined ) this.opacity = parameters.opacity;
		if ( parameters.blending !== undefined ) this.blending = parameters.blending;

	}

};

THREE.ParticleCircleMaterial.prototype = {

	toString: function () {

		return 'THREE.ParticleCircleMaterial (<br/>' +
			'color: ' + this.color + '<br/>' +
			'opacity: ' + this.opacity + '<br/>' +
			'blending: ' + this.blending + '<br/>' +
			')';

	}

};
