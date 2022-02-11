import { Message } from "element-ui";

/**
 * 防抖函数
 * @param {*} func 防抖后要执行的回调
 * @param {*} wait 等待时间
 * @param {*} immediate 
 */
function debounce(func, wait, immediate) {
    let timeout, args, context, timestamp, result;

    const later = function() {
        // 据上一次触发时间间隔
        const last = +new Date() - timestamp;

        // 上次被包装函数被调用时间间隔last小于设定时间间隔wait
        if (last < wait && last > 0) {
            timeout = setTimeout(later, wait - last);
        } else {
            timeout = null;
            // 如果设定为immediate===true，因为开始边界已经调用过了此处无需调用
            if (!immediate) {
                result = func.apply(context, args);
                if (!timeout) context = args = null;
            }
        }
    };

    return function(...args) {
        context = this;
        timestamp = +new Date();
        const callNow = immediate && !timeout;
        // 如果延时不存在，重新设定延时
        if (!timeout) timeout = setTimeout(later, wait);
        if (callNow) {
            result = func.apply(context, args);
            context = args = null;
        }

        return result;
    };
}

/**
 * 将数组转化成树结构 array to tree
 * @param {*} array 数据源
 * @param {*} options 字段名配置项
 */
// function arrayToTree(array) {
//     return array.filter(item => item.Type === 1).map(item => item.Path)
// }
function arrayToTree(
    array = [],
    options = { Id: "Id", pid: "Pid", Children: "Children" },
) {
    // return []
    // console.log("enter arrayToTree from util");
    let array_ = []; // 创建储存剔除叶子节点后的骨架节点数组
    let unique = {}; // 创建盒子辅助本轮Children合并去重
    let root_pid = [
        "./"
    ]; // 可能存在的根节点pid形式
    array.forEach(item => {
        // var pathStack = item.Path.split("\\")
        // item.Id = pathStack.slice(-1)
        // item.pid = pathStack.length > 1 ? pathStack.slice(-2) : "./"
        // 筛选可以插入当前节点的所有子节点
        let Children_array = array.filter(
            it => {
                // debugger
                return it["Parent"] === item["Path"]
            }
        );
        // console.log("Children_array: ", Children_array)
        if (item["Children"] && item["Children"] instanceof Array && item["Children"].length > 0) {
            // 去重合并数组
            item["Children"].map(i => (unique[i["Path"]] = 1));
            item["Children"].push(
                ...Children_array.filter(i => unique[i["Path"]] !== 1)
            );
        } else {
            item["Children"] = Children_array;
        }
        // 当Children_array有数据时插入下一轮array_，当无数据时将最后留下来的根节点树形插入数组
        let has_Children = Children_array.length > 0;
        if (has_Children || root_pid.includes(item["Parent"])) {
            array_.push(item);
        }
    });
    // 当数组内仅有根节点时退出，否组继续处理 最终递归深度次
    if (!array_.every(item => root_pid.includes(item["Parent"]))) {
        return arrayToTree(array_, options);
    } else {
        // debugger
        return array_;
    }
}

/**
 * 从坐标值拼接指定字段到祖先元素
 * @param {*} data 一维数据源
 * @param {*} coordinate 坐标值数据
 * @param {*} options 配置项
 */
function splicParentsUntil(data, coordinate, options = {
    pathName: 'name', // 所要拼接字段
    pathConnector: '/', // 连接符 
    pathId: "Id", // 数据源匹配字段 
    pathParents: "parents",
    pathIdentityId: "identityId",
}) {
    // return;
    let coordinate_item = data.find(i => i[options.pathId] === coordinate[options.pathId]);
    if (!coordinate_item) return '';
    if (!coordinate_item[options.pathParents]) return coordinate_item[options.pathName];
    let _parents = coordinate_item[options.pathParents]
        .substring(1, coordinate_item[options.pathParents].length - 1)
        .split(",")
        .filter(i => !!i);
    let splic_parents = '';
    _parents.forEach(i => {
        let _parent = data.find(t => t[options.pathIdentityId] == i);
        splic_parents += `${_parent[options.pathName]}${options.pathConnector}`
    })
    return splic_parents + coordinate_item[options.pathName];
}

/**
 * 处理下载接口返回的文件流数据
 * @param {*} res http请求返回数据
 */
function download(res, name) {
    // 错误处理
    if (res.status != 200) {
        let reader = new FileReader();
        reader.readAsText(res.data, 'utf-8');
        reader.onload = function() {
            // let json_data = JSON.parse(reader.result);
            Message({
                showClose: true,
                message: "文件不存在！",
                // message: json_data.Message,
                type: "error"
            });
        }
        return;
    }
    // 下载处理
    let filename = "content-disposition" in res.headers ?
        decodeURIComponent(
            res.headers["content-disposition"]
            .split(";")[1]
            .split("=")[1]
            .replace(/"/g, "")
        ) :
        name;
    try {
        if (window.navigator.msSaveOrOpenBlob) {
            navigator.msSaveBlob(res.data, filename);
        } else {
            // console.log("res.headers.contentType: ", res.headers)
            let blob = new Blob([res.data], {
                type: res.headers["content-type"]
            });
            let url = URL.createObjectURL(blob);
            let link = document.createElement("a");
            link.setAttribute("href", url);
            link.setAttribute("download", filename);
            link.style.display = "none";
            document.body.appendChild(link);
            link.click();
            URL.revokeObjectURL(url); // 释放URL 对象
            document.body.removeChild(link);
        }
    } catch (err) {
        // console.log(err)
    }
}

function getUrl(res, name) {
    // 错误处理
    if (res.status != 200) {
        let reader = new FileReader();
        reader.readAsText(res.data, 'utf-8');
        reader.onload = function() {
            // let json_data = JSON.parse(reader.result);
            Message({
                showClose: true,
                message: "文件不存在！",
                // message: json_data.Message,
                type: "error"
            });
        }
        return;
    }
    // url处理
    try {
        let blob = new Blob([res.data], {
            type: res.headers["content-type"]
        });
        let url = URL.createObjectURL(blob);
        return url;
    } catch (e) {

    }
}

/**
 * 关闭其他弹出类视图函数
 * 用于切换侧滑区域内容
 * data:object 要求为该页面layout字段
 * key:string 需要打开的视图
 */
function closeOtherLayout(data = {}, key) {
    for (let item in data) {
        data[item] = false;
    }
    if (key) data[key] = true;
}

export {
    debounce, // 防抖函数
    arrayToTree, // 将数组转化成树结构
    splicParentsUntil, // 从坐标值拼接指定字段到祖先元素
    download, // download
    closeOtherLayout, // 关闭其他弹出类视图函数
    getUrl
}