
function DataObj(id, question, A, B, C, D, result) {
    this.id = id + "";
    this.question = question;
    this.result = result;
    this.A = A;
    this.B = B;
    this.C = C;
    this.D = D;

}

function convertStringToObj(strData, id) {
    //这里用的getAllSubStrIndex不是最好的方法，应该查到一次就返回，为了省事
    var indexs = getAllSubStrIndex_once(strData, ['、', 'A.', 'B.', 'C.', 'D.', '答案：']);
    //console.log(indexs);
    var question = strData.substring(indexs[0].pos + indexs[0].length, indexs[1].pos);

    if (question.length > 50) {
        return null;
    }
   
    var A = strData.substring(indexs[1].pos + indexs[1].length, indexs[2].pos);
    if (A.length > 20) {
        return null;
    }
    var B = strData.substring(indexs[2].pos + indexs[2].length, indexs[3].pos);
    if (B.length > 20) {
        return null;
    }
    var C = strData.substring(indexs[3].pos + indexs[3].length, indexs[4].pos);
    if (C.length > 20) {
        return null;
    }
    var D = strData.substring(indexs[4].pos + indexs[4].length, indexs[5].pos);
    if (D.length > 20) {
        return null;
    }
    var result = strData.charAt(strData.length - 1);
    var dataObj = new DataObj(id, question, A, B, C, D, result);
    return dataObj;
}

function getAllSubStrIndex_once(strData, subStrs) {
    var indexs = [];
    for (var i = 0; i < subStrs.length; i++) {
        var pos = strData.indexOf(subStrs[i]);
        if (pos > -1) {
            indexs.push({ "pos": pos, "length": subStrs[i].length });

            pos = strData.indexOf(subStrs[i], pos + subStrs[i].length);
        }
    }

    var compare = function (x, y) {
        if (x.pos < y.pos) {
            return -1;
        } else if (x.pos > y.pos) {
            return 1;
        } else {
            return 0;
        }
    }

    indexs.sort(compare);

    return indexs;
}

function getAllSubStrIndex(strData, subStrs) {
    var indexs = [];
    for (var i = 0; i < subStrs.length; i++) {
        var pos = strData.indexOf(subStrs[i]);
        while (pos > -1) {
            indexs.push({ "pos": pos, "length": subStrs[i].length });

            pos = strData.indexOf(subStrs[i], pos + subStrs[i].length);
        }
    }

    var compare = function (x, y) {
        if (x.pos < y.pos) {
            return -1;
        } else if (x.pos > y.pos) {
            return 1;
        } else {
            return 0;
        }
    }

    indexs.sort(compare);

    return indexs;
}

function trimHuanHang(dataStr) {
    //去掉空格
    dataStr = dataStr.replace(/\ +/g, "");
    //去掉回车换行  
    dataStr = dataStr.replace(/[\r\n]/g, "");
    

    return dataStr;
}


function main() {
    var pData = document.getElementById('data');
    var dataStr = pData.innerHTML;
    var dataStr = trimHuanHang(dataStr);
    var indexs = getAllSubStrIndex(dataStr, ["答案：A", "答案：B", "答案：C", "答案：D"]);

    var objs = [];
    var start = end = 0;
    var j = 0;//准备将长度超过10的题删除
    for (var i = 0; i < indexs.length; i++) {
        end = indexs[i].pos + indexs[i].length;
        var obj = convertStringToObj(dataStr.substring(start, end), 999 + j);
        start = end;
      
        if (!obj) {
            continue
        } else {
            j++;
            objs.push(obj);
        }
    }

    var resultString = JSON.stringify(objs);
    console.log(resultString);
}



