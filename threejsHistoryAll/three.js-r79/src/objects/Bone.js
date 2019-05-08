/**
 * @author mikael emtinger / http://gomo.se/
 * @author alteredq / http://alteredqualia.com/
 * @author ikerr / http://verold.com
 */
/**
 * A bone which is part of a Skeleton. 
 * The skeleton in turn is used by the SkinnedMesh. 
 * Bones are almost identical to a blank Object3D.
 */
THREE.Bone = function ( skin ) {

	THREE.Object3D.call( this );

	this.type = 'Bone';

	//新版本这个属性已经删除，不知道什么用
	this.skin = skin;

};

THREE.Bone.prototype = Object.assign( Object.create( THREE.Object3D.prototype ), {

	constructor: THREE.Bone,

	copy: function ( source ) {

		THREE.Object3D.prototype.copy.call( this, source );

		this.skin = source.skin;

		return this;

	}

} );
