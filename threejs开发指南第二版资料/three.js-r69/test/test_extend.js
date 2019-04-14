

function ppp() {
    this.a = 1;
}

ppp.prototype.wwwww = function() {

}

function ccc() {
    ppp.call(this);
    this.b = 2;
}

ccc.prototype = Object.create(ppp.prototype);

ccc.prototype.aaaaa = function() {

}

console.log(new ccc());
