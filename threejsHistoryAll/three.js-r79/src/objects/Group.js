/**
 * @author mrdoob / http://mrdoob.com/
 */
/**
 * This is almost identical to an Object3D. 
 * Its purpose is to make working with groups of objects syntactically clearer.
 */
//搜索过引擎了，没有任何特殊处理，就是一个Object3D 目的就是为了清晰表达意思
THREE.Group = function () {

	THREE.Object3D.call( this );

	this.type = 'Group';

};

THREE.Group.prototype = Object.assign( Object.create( THREE.Object3D.prototype ), {

	constructor: THREE.Group

} );
