using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using Shell32;
using phy_exp_api.Models;
using System.Threading;

namespace phy_exp_api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AudioController : ControllerBase
    {
        private readonly IConfiguration Configuration;//读取配置文件
        List<string> Tasks { get; set; } // TODO: 添加任务类型配置
        string MicRootPath { get; set; }
        string AudioRootPath { get; set; }

        public AudioController(IConfiguration configuration)
        {
            Configuration = configuration;
            MicRootPath = Configuration["MicRootPath"];
            AudioRootPath = Configuration["AudioRootPath"];
            Directory.CreateDirectory(Path.Combine(AudioRootPath, "sr"));
            Directory.CreateDirectory(Path.Combine(AudioRootPath, "asr"));
            Directory.CreateDirectory(Path.Combine(AudioRootPath, "test"));

        }
        [HttpGet]
        public ActionResult<string> PlayAudio(string path = "test.wav")
        {
            try
            {
                string audioPath = Path.Combine(AudioRootPath, path);
                // 播放音频
                SoundPlayer player = new SoundPlayer();
                player.SoundLocation = audioPath;
                player.Load(); //同步加载声音
                player.Play(); //启用新线程播放
                return path;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult<List<AudioFile>> AudioTask(string task)
        {
            try
            {
                string taskPath = Path.Combine(AudioRootPath, task);
                void Recur(DirectoryInfo dir, ref List<AudioFile> audioFiles)
                {
                    var fsInfos = dir.GetFileSystemInfos();

                    foreach (var fsInfo in fsInfos)
                    {
                        if (fsInfo is DirectoryInfo)
                        {
                            Recur((DirectoryInfo)fsInfo, ref audioFiles);
                            continue;
                        }

                        int duration = 0;
                        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                        {
                            duration = GetAudioDuration(fsInfo.FullName);
                        }
                        else
                        {
                            Thread staThread = new Thread(new ThreadStart(() => { duration = GetAudioDuration(fsInfo.FullName); }));
                            staThread.SetApartmentState(ApartmentState.STA);
                            staThread.Start();
                            staThread.Join();
                        }

                        if (duration <= 0)
                            throw new Exception("音频时长获取错误");

                        AudioFile audioFile = new AudioFile
                        {
                            Name = Path.GetRelativePath(taskPath, fsInfo.FullName).Replace("\\", "/"),
                            Duration = duration
                        };
                        audioFiles.Add(audioFile);
                    }
                }
                string fullPath = Path.Combine(AudioRootPath, task);
                var dir = new DirectoryInfo(fullPath);
                List<AudioFile> audioFiles = new List<AudioFile>();
                Recur(dir, ref audioFiles);
                return audioFiles;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetAudioDuration(string path)
        {
            // 获取音频长度
            ShellClass sh = new ShellClass();
            Folder directory = sh.NameSpace(Path.GetDirectoryName(path));
            FolderItem item = directory.ParseName(Path.GetFileName(path));
            String durationStr = directory.GetDetailsOf(item, 27);    //获取时长字符串(00:00:01)
            int duration = 0; //时长(毫秒)
            if (!durationStr.Equals(""))
            {
                String[] durationArray = durationStr.Split(':');    //获取长度  iColumn:27
                duration += int.Parse(durationArray[0]) * 60 * 60 * 1000;
                duration += int.Parse(durationArray[1]) * 60 * 1000;
                duration += int.Parse(durationArray[2]) * 1000;
            }
            else
                throw new Exception("音频长度获取失败。");
            return duration;
        }

        [HttpPost]
        public ActionResult<string> UploadAudio([FromForm] IFormFile file, [FromForm] string path)
        {
            try
            {
                //保存文件到本地
                var filefullPath = Path.Combine(MicRootPath, path);
                Directory.CreateDirectory(filefullPath.Substring(0, filefullPath.LastIndexOf("/")));
                using (FileStream fs = new FileStream(filefullPath, FileMode.Create))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
                return path;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }


}
