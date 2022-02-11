 # 简易物理实验自动化Web程序

## 安装

1. .net framework

   参考：https://docs.microsoft.com/zh-cn/dotnet/core/install/

   操作系统需支持  .NET Core 3.1 的安装

2. node.js

   参考：https://nodejs.org/en/download/

3.  cnpm

   npm install cnpm -g

4. vue-cli

   cnpm install vue-cli -g



## 程序运行

1. 后端

   使用 VS 打开解决方案（.sln）

   在 Controllers/AudioController.cs 文件中修改 MicRootPath 为电脑上存放采集到音频的位置，将 AudioRootPath 修改为保存了对抗样本的目录

   **注意 :AudioRootPath 目录下应有两个子目录 sr 和 asr 以及一个名为test.wav的1s测试音频文件（随便找一个），在这两个子目录下直接存放相应的对抗样本，**

   F5运行即可。

   命令行中出现**后端端口**

2. 前端

   在 src\util\request.js 文件第6行修改ip以及端口

   ip为**本机在局域网中的ip地址**，端口为后端服务的监听端口（端口应该不用改）

   在 cmd 中将路径切换到前端项目的根目录，运行以下指令

   npm install

   npm i js-audio-recorder

   npm install element-plus --save

    npm install axios

   （以上若安装失败可以将npm 替换为cnpm）

   npm run serve

   （若这一步还提示存在包缺失，按照报错提示的指令安装）

   成功弹出页面则运行成功，命令行中出现**前端访问地址**

   在windows 防火墙中添加出站规则开放 相应的前后端端口

   接下来使用电脑和移动设备连接同一局域网执行以下步骤进行测试

   > 在浏览器中打开页面：https://192.168.123.53:5000/api/audio/playaudio?path=test.wav （注意替换ip地址和**后端**端口），会提示页面不安全，选择信任继续访问，若这时电脑开始播放test.wav 则该设备后端访问正常
   >
   > 接着打开页面：https://192.168.123.53:8081/（注意替换为ip地址和**前端**端口）
   >
   > 点击测试，若电脑端成功播放，接收端成功录制（时长大于1）并播放录制音频则运行成功
   >
   > 

   

   ## 程序使用

   可以通过点击测试按钮，聆听录制的音频，调整 录音延时 使录音能将播放音频的主要内容录制进去。录音延时为正则延后开始录音时间，为负则提前开始录音，单位为ms。

   调整好后点击 获取音频列表 ，获取成功后即可点击 开始实验。实验会播放电脑端，相应任务的对抗样本文件夹中的**所有音频**，并根据实验条件将录音结果保存在相应的目录下。

