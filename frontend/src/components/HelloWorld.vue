<template>
  <div class="hello">
    <h3>{{ msg }}</h3>
    <!-- <el-input v-model="input" placeholder="音频保存路径"></el-input> -->
    <!-- <el-form :inline="true" :model="formInline" class="demo-form-inline">
      <el-form-item label="服务器地址">
        <el-input v-model="ip" placeholder="192.168.123.53"></el-input>
      </el-form-item>
      <el-form-item label="服务器端口">
        <el-input v-model="port" placeholder="8081"></el-input>
      </el-form-item>
      <el-form-item>
      <el-button type="修改服务器地址" @click="changeServer()">Query</el-button>
    </el-form-item>
    </el-form> -->
    <el-form
      label-position="top"
      label-width="100px"
      :model="formLabelAlign"
      style="max-width: 460px"
    >
      <el-form-item label="实验任务">
        <el-select v-model="task">
          <el-option label="asr" value="asr"></el-option>
          <el-option label="sr" value="sr"></el-option>
        </el-select>
      </el-form-item>
      <el-form-item label="实验房间">
        <el-input v-model="room"></el-input>
      </el-form-item>
      <el-form-item label="场景布置（摆放位置或距离）">
        <el-input v-model="position"></el-input>
      </el-form-item>
      <el-form-item label="设备名称">
        <el-input v-model="device"></el-input>
      </el-form-item>
      <el-form-item label="录音延时（ms）">
        <el-input-number v-model="delay" :min="-1000" :max="1000" />
      </el-form-item>
    </el-form>
    <h3>{{ "实验录音保存目录：" + audioSaveDir }}</h3>
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

// const lamejs = require('lamejs')

const recorder = new Recorder({
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
    task: {
      type: String,
      default: "asr",
    },
    room: {
      type: String,
      default: "test",
    },
    position: {
      type: String,
      default: "15cm",
    },
    device: String,
  },
  data() {
    return {
      audioList: [],
      notReady: true,
      count: -1,
      delay: -200,
    };
  },
  methods: {
    upload(audio, path) {
      let formData = new FormData();
      let data = audio;
      formData.append("file", data);
      formData.append("path", path);
      return request({
        url: "./audio/uploadAudio/",
        method: "post",
        data: formData,
      });
    },
    record(fullPath, cb) {
      // 开始录音
      //   recorder = new Recorder(recordConfig);
      recorder.resume();
      recorder.start().then(
        () => {
          console.log("start recording...");
          setTimeout(() => {
            console.log("finish recording.");
            recorder.stop();
            // recorder = null;
            this.$message({
              showClose: true,
              message: "录音时长2：" + recorder.duration,
              type: "success",
            });
            //   recorder.play();
            let wav = recorder.getWAVBlob();
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
          }, 1200);
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
        url: "./audio/playaudio",
        params: { path: path },
      });
    },
    getAudios(task) {
      request({
        url: "./audio/audioTask",
        params: { task: task },
      }).then(
        (res) => {
          //   console.log(res.data);
          this.audioList = res.data;
          this.$message({
            showClose: true,
            message: "音频列表获取成功",
            type: "success",
          });
          this.notReady = false;
          this.count = this.audioList.length;
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
    exp() {
      this.count -= 1;
      if (this.count < 0) return;
      this.notReady = true;
      console.log("exp " + this.count + " started");
      let audio = this.audioList[this.count];
      let audioPath = this.task + "/" + audio;
      let fileName = this.audioSaveDir.replaceAll("/", "-") + "-" + audio;
      let fullPath = this.audioSaveDir + "/" + fileName;
      setTimeout(
        () => {
          this.play(audioPath).then(
            // 成功回调
            () => {
              console.log("successfully played audio: " + audioPath);
              this.$message({
                showClose: true,
                message: "音频播放成功",
                type: "success",
              });
              // 播放成功后开始录音
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
        () => {
          this.record(fullPath, () => {
            setTimeout(this.exp, 1000); // 一秒实验间隔，确保录制成功！
          });
        },
        this.delay > 0 ? this.delay : 0
      );
    },
    startExp(path) {
      //   let count = 0;
      //   this.audioList.forEach((audio, idx) => {
      //     p = setTimeout(() => {}, 2000);
      //   });
      this.exp();
      //   alert("实验开始！保存路径：" + path);
    },
    test(count) {
      if (count <= 0) return;
      setTimeout(
        () => {
          this.play("test.wav").then(
            // 成功回调
            () => {
              console.log("successfully played audio: test.wav");
              this.$message({
                showClose: true,
                message: "音频播放成功",
                type: "success",
              });
              // 播放成功后开始录音
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
        () => {
          this.record("./test_record.wav", () => {
            recorder.play();
            this.test(count - 1);
          });
        },
        this.delay > 0 ? this.delay : 0
      );
    },
  },
  computed: {
    audioSaveDir() {
      return (
        (this.task ? this.task : "") +
        "/" +
        (this.room ? this.room : "") +
        "/" +
        (this.position ? this.position : "") +
        "/" +
        (this.device ? this.device : "")
      );
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
