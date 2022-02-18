<template>
  <div class="hello">
    <h3>{{ msg }}</h3>
    <!-- <el-input v-model="input" placeholder="音频保存路径"></el-input> -->
    <el-form label-position="top" label-width="100px" style="max-width: 460px">
      <el-form-item label="服务器地址">
        <el-input v-model="address" placeholder="ip"></el-input>
      </el-form-item>
      <el-form-item label="服务器端口">
        <el-input v-model="port" placeholder="port"></el-input>
      </el-form-item>
      <!-- <el-form-item>
        <el-button type="primary" @click="changeServer()"
          >修改服务器地址</el-button
        >
      </el-form-item> -->
    </el-form>
    <el-form
      label-position="top"
      label-width="100px"
      style="max-width: 460px"
      class="demo-dynamic"
      :model="domains"
    >
      <el-form-item label="实验任务" :required="true">
        <!-- <el-select v-model="task">
          <el-option label="asr" value="asr"></el-option>
          <el-option label="sr" value="sr"></el-option>
        </el-select> -->
        <el-input v-model="task"></el-input>
      </el-form-item>
      <!-- <el-form-item label="实验房间">
        <el-input v-model="room"></el-input>
      </el-form-item>
      <el-form-item label="场景布置（摆放位置或距离）">
        <el-input v-model="position"></el-input>
      </el-form-item>
      <el-form-item label="设备名称">
        <el-input v-model="device"></el-input>
      </el-form-item> -->
      <el-form-item
        v-for="(domain, index) in domains"
        :key="domain.key"
        :label="'实验条件' + index"
        :prop="index + '.value'"
        :rules="{
          required: true,
          message: '实验条件不可为空',
          trigger: 'blur',
        }"
      >
        <el-input v-model="domain.value"></el-input>
        <el-button class="mt-2" @click.prevent="removeDomain(domain)"
          >删除</el-button
        >
      </el-form-item>
      <el-form-item>
        <!-- <el-button type="primary" @click="submitForm(formRef)">Submit</el-button> -->
        <el-button @click="addDomain">添加实验条件</el-button>
        <!-- <el-button @click="resetForm(formRef)">Reset</el-button> -->
      </el-form-item>
      <el-form-item label="录音延时（ms）">
        <el-input-number v-model="delay" :min="-1000" :max="1000" />
      </el-form-item>
      <el-form-item label="重复次数">
        <el-input-number v-model="repeat" :min="0" :max="20" />
      </el-form-item>
    </el-form>
    <h3>{{ "实验录音保存目录：" + audioSaveDir }}</h3>
    <div>
      <el-tag :type="status">{{ statusString }}</el-tag>
    </div>
    <el-button type="primary" @click="test(2)">测试</el-button>
    <el-button type="primary" @click="getAudios(task)">获取音频列表</el-button>
    <el-button
      ref="btnStart"
      type="primary"
      @click="startExp(input)"
      :disabled="notReady"
      >开始实验</el-button
    >
  </div>
</template>

<script>
import request from "@/util/request";
import Recorder from "js-audio-recorder";

let recorder = new Recorder({
  sampleBits: 16, // 采样位数，支持 8 或 16，默认是16

  sampleRate: 16000, // 采样率，支持 11025、16000、22050、24000、44100、48000，根据浏览器默认值，我的chrome是48000

  numChannels: 1, // 声道，支持 1 或 2， 默认是1

  // compiling: false,(0.x版本中生效,1.x增加中) // 是否边录边转换，默认是false
});

const recordConfig = {
  sampleBits: 16, // 采样位数，支持 8 或 16，默认是16

  sampleRate: 16000, // 采样率，支持 11025、16000、22050、24000、44100、48000，根据浏览器默认值，我的chrome是48000

  numChannels: 1, // 声道，支持 1 或 2， 默认是1

  // compiling: false,(0.x版本中生效,1.x增加中) // 是否边录边转换，默认是false
};

// var recorder1 = new Recorder(recordConfig);

// 绑定事件-打印的是当前录音数据

// recorder.onprogress = function (params) {
//   // console.log('--------------START---------------')
//   // console.log('录音时长(秒)', params.duration);
//   // console.log('录音大小(字节)', params.fileSize);
//   // console.log('录音音量百分比(%)', params.vol);
//   // console.log('当前录音的总数据([DataView, DataView...])', params.data);
//   // console.log('--------------END---------------')
// };

export default {
  name: "HelloWorld",
  props: {
    msg: String,
    input: String,
    address: {
      type: String,
      default: "192.168.43.115",
    },
    port: {
      type: String,
      default: "5000",
    },
    task: {
      type: String,
      default: "asr",
    },
    // room: {
    //   type: String,
    //   default: "test_room",
    // },
    // position: {
    //   type: String,
    //   default: "15cm",
    // },
    // device: {
    //   type: String,
    //   default: "my_device",
    // },
  },
  data() {
    return {
      audioList: [],
      audioDuration: [],
      notReady: true,
      count: -1,
      delay: -200,
      repeat: 1,
      numRepeat: 0,
      status: "info",
      statusString: "空闲中",
      domains: [
        // { key: 0, value: "room" },
        { key: 1, value: "15cm" },
        { key: 2, value: "my_device" },
        { key: 3, value: "jbl" },
        { key: 4, value: "loudness60" },
        { key: 5, value: "noise30" },
      ],
    };
  },
  methods: {
    // changeServer() {
    //   changeBaseURL("https://" + this.address + ":" + this.port + "/api");
    // },
    upload(audio, path) {
      let formData = new FormData();
      let data = audio;
      formData.append("file", data);
      formData.append("path", path);
      return request({
        baseURL: this.ip,
        url: "./audio/uploadAudio/",
        method: "post",
        data: formData,
      });
    },
    record(fullPath, cb, play = false) {
      // 开始录音
      //   recorder = new Recorder(recordConfig);
      recorder.resume();
      recorder.start().then(
        () => {
          console.log("start recording...");
          setTimeout(() => {
            console.log("finish recording.");

            // recorder.stop();
            if (play) {
              recorder.stop();
              recorder.play();
              console.log("录制端开始播放录制音频");
            }
            let wav = recorder.getWAVBlob();
            if (recorder.duration * 1000 < this.audioDuration[this.count]) {
              this.$message({
                showClose: true,
                message: "录音时长不足：" + recorder.duration,
                type: "warning",
              });
              this.count += 1; // 录音失败，重来
              this.numRepeat += 1;
              // swap
              //   let temp = recorder;
              //   recorder = recorder1;
              //   recorder1 = temp;
              recorder = new Recorder(recordConfig);
              cb();
              return;
            }

            // recorder = null;
            this.$message({
              showClose: true,
              message: "录音时长：" + recorder.duration,
              type: "success",
            });
            //   recorder.play();
            // let wav = recorder.getWAVBlob();
            this.numRepeat = 0;
            console.log("uploading audio: " + fullPath);
            this.upload(wav, fullPath).then(cb, (e) => {
              this.$message({
                showClose: true,
                message: "音频上传失败：" + e,
                type: "error",
              });
            });
            //   recorder.play();
            return 1;
          }, this.audioDuration[this.count] * 1.2);
        },
        (e) => {
          this.$message({
            showClose: true,
            message: "录音失败：" + e,
            type: "error",
          });
        }
      );
    },
    // upload(fullPath) {
    //   let wav = recorder.getWAVBlob();
    //   this.upload(wav, fullPath);
    // },
    play(path) {
      // 请求播放音频
      return request({
        baseURL: this.ip,
        url: "./audio/playaudio",
        params: { path: path },
      });
    },
    getAudios(task, cb = () => {}) {
      request({
        baseURL: this.ip,
        url: "./audio/audioTask",
        params: { task: task },
      }).then(
        (res) => {
          //   console.log(res.data);
          this.audioList = [];
          this.audioDuration = [];
          res.data.forEach((audioFile) => {
            this.audioList.push(audioFile.Name);
            this.audioDuration.push(audioFile.Duration);
          });
          this.$message({
            showClose: true,
            message: "成功查询到" + this.audioList.length + "条音频",
            type: "success",
          });
          this.notReady = false;
          this.count = this.audioList.length;
          // execute command
          cb();
        },
        (e) => {
          this.$message({
            showClose: true,
            message: "音频列表获取失败：" + e,
            type: "error",
          });
        }
      );
    },
    exp(task, play = false) {
      this.count -= 1;
      if (this.count < 0) {
        this.repeat -= 1;
        if (this.repeat <= 0) {
          this.status = "success";
          this.statusString =
            "实验成功！共录制音频" + this.audioList.length + "条";
          this.repeat = 0;
          return;
        } else {
          this.count = this.audioList.length - 1;
        }
      }
      this.notReady = true;
      console.log("exp " + this.count + ", repeat " + this.repeat + " started");
      let audioRelativePath = this.audioList[this.count]; // 相对task目录的相对路径
      let audioPath = task + "/" + audioRelativePath; // 相对audio根路径的相对路径
      let audio = audioRelativePath.split("/").at(-1); // 文件名
      let audioSavePath = this.audioSaveDir + "/" + audioRelativePath;
      let finalDir = audioSavePath.substring(0, audioSavePath.indexOf(audio));
      let fullPath =
        finalDir +
        finalDir.replaceAll("/", "-") +
        "--r" +
        this.repeat +
        "--" +
        audio;
      //   let fullPath = this.audioSaveDir + "/" + fileName;
      setTimeout(
        // 播放延时
        () => {
          this.play(audioPath).then(
            // 成功回调
            () => {
              console.log("successfully played audio: " + audioPath);
              this.$message({
                showClose: true,
                message:
                  "音频播放成功：" +
                  this.count +
                  "；时长：" +
                  this.audioDuration[this.count] / 1000 +
                  " s",
                type: "success",
              });
            },
            // 失败回调
            (e) => {
              this.$message({
                showClose: true,
                message: "音频播放失败：" + e,
                type: "warning",
              });
            }
          );
        },
        this.delay < 0 ? -this.delay : 0
      );
      setTimeout(
        // 录音延时
        () => {
          this.record(
            fullPath,
            () => {
              if (this.numRepeat > 5) {
                this.$message({
                  showClose: true,
                  message: "录音故障",
                  type: "error",
                });
                this.numRepeat = 0;
                this.status = "danger";
                this.statusString = "实验失败，录音故障";
                return;
              }
              setTimeout(() => {
                this.exp(task, play);
              }, 1200); // 一秒实验间隔，确保录制成功！
            },
            play
          );
        },
        this.delay > 0 ? this.delay : 0
      );
    },
    startExp(path) {
      //   let count = 0;
      //   this.audioList.forEach((audio, idx) => {
      //     p = setTimeout(() => {}, 2000);
      //   });
      this.exp(this.task);
      this.status = "";
      this.statusString = "实验中...";
      //   alert("实验开始！保存路径：" + path);
    },
    test(count) {
      if (count <= 0) return;
      this.getAudios("test", () => {
        this.exp("test", true);
        // setTimeout(
        //   () => {
        //     this.play("test.wav").then(
        //       // 成功回调
        //       () => {
        //         console.log("successfully played audio: test.wav");
        //         this.$message({
        //           showClose: true,
        //           message: "音频播放成功",
        //           type: "success",
        //         });
        //         // 播放成功后开始录音
        //       },
        //       // 失败回调
        //       (e) => {
        //         this.$message({
        //           showClose: true,
        //           message: "音频播放失败：" + e,
        //           type: "warning",
        //         });
        //       }
        //     );
        //   },
        //   this.delay < 0 ? -this.delay : 0
        // );
        // setTimeout(
        //   () => {
        //     this.record("./test_record.wav", () => {
        //       recorder.play();
        //       this.test(count - 1);
        //     });
        //   },
        //   this.delay > 0 ? this.delay : 0
        // );
      });
    },
    removeDomain(item) {
      const index = this.domains.indexOf(item);
      if (index !== -1) {
        this.domains.splice(index, 1);
      }
    },
    addDomain() {
      this.domains.push({
        key: Date.now(),
        value: "",
      });
      //   this.$forceUpdate();
    },
  },
  computed: {
    audioSaveDir() {
      let dir = this.task ? this.task : "";
      dir += "/" + this.domains.map((d) => d.value).join("/");
      return dir;
    },
    ip() {
      return "https://" + this.address + ":" + this.port + "/api";
    },
  },
  created() {
    Recorder.getPermission().then(
      () => {
        this.$message({
          showClose: true,
          message: "获取权限成功",
          type: "success",
        });
        // this.$message.success("获取权限成功");
      },
      (error) => {
        this.$message({
          showClose: true,
          message: "获取权限失败:" + error,
          type: "warning",
        });
        // this.$message.success("获取权限失败");
        console.log(`${error.name} : ${error.message}`);
      }
    );
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
h3 {
  margin: 40px 0 0;
}
ul {
  list-style-type: none;
  padding: 0;
}
li {
  display: inline-block;
  margin: 0 10px;
}
a {
  color: #42b983;
}
</style>
