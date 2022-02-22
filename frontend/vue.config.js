// import getNetworkIp from "@/util/ip";

const os = require('os')

function getNetworkIp() {
    // 打开的 host
    let needHost = '';
    try {
        // 获得网络接口列表
        let network = os.networkInterfaces();
        console.log("network: ", network);
        for (let dev in network) {
            let iface = network[dev];
            for (let i = 0; i < iface.length; i++) {
                let alias = iface[i];
                if (
                    alias.family === 'IPv4' &&
                    alias.address !== '127.0.0.1' &&
                    alias.address.startsWith("192.168") &&
                    !alias.internal
                ) {
                    needHost = alias.address;
                }
            }
        }
    } catch (e) {
        needHost = 'http://localhost';
    }
    return needHost;
}

// export default getNetworkIp

module.exports = {
    // 将资源打包为相对路径
    // publicPath: "././",
    lintOnSave: false, // 保存时检查格式，使用eslint
    crossorigin: 'anonymous', // htmlWebpackPlugin
    devServer: { // 对开发服务的设置
        // Various Dev Server settings
        port: 8081, // can be overwritten by process.env.PORT, if port is in use, a free one will be determined
        open: true, // 自动调用默认浏览器打开
        https: true // 是否使用https, https使用node的一个内部默认的ca证书
    },
    configureWebpack: {
        devtool: 'source-map'
    },
    chainWebpack: config => {
        config.plugin('define').tap((args) => {
            let ip = getNetworkIp();
            args[0]['process.env'].BASE_IP = `"${ip}"`;
            return args;
        });
    }

}