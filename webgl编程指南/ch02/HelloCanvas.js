

function main() {
  var idioms_result = generate_result(idioms);
  console.log(idioms_result);
}


function generate_result(idioms) {
  var result = [];
  for (var i = 0, j = idioms.length; i < j; i++) {
    var row = [];
    row.push(idioms[i]);
    generate_for_head(row, idioms);
    if (row.length > 1) {
      result.push(row);
    }

  }
  return result;
}

//检测所有非放入的元素，并且该元素与数组尾部的元素有重字的，然后将其放入数组尾部
function generate_for_head(row, idioms) {

  for (var i = 0, j = idioms.length; i < j; i++) {
    if (notPutIn(row, idioms[i])) {
      var word = hasSameWord(row[row.length - 1], idioms[i]);
      if (word) {

        if (row.length > 1 && row[row.length - 2].indexOf(word) != -1) {
          continue;
        }
        row.push(idioms[i]);
        // if(row.length>6) {
        //   return;
        // }
        i = 0;
      }

    }
  }
}

function notPutIn(row, idiom) {
  for (var i = 0, j = row.length; i < j; i++) {
    if (row[i] === idiom) {
      return false;
    }
  }
  return true;
}

//这俩串有相同的字
function hasSameWord(strA, strB) {
  for (var i = 0, j = strA.length; i < j; i++) {
    var word = strA.charAt(i);
    var index = strB.indexOf(word);
    if (index != -1) {
      return word;
    }
  }
  return '';
}

