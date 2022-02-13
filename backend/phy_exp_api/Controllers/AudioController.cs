using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;

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

        }
        [HttpGet]
        public ActionResult<string> PlayAudio(string path = "1000.wav")
        {
            try
            {
                string audioPath = Path.Combine(AudioRootPath, path);
                SoundPlayer player = new SoundPlayer();
                player.SoundLocation = audioPath;
                player.Load(); //同步加载声音
                player.Play(); //启用新线程播放
                return "ok";
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult<List<string>> AudioTask(string task)
        {
            try
            {
                string taskPath = Path.Combine(AudioRootPath, task);
                void Recur(DirectoryInfo dir, ref List<string> audioPaths)
                {
                    var fsInfos = dir.GetFileSystemInfos();

                    foreach (var fsInfo in fsInfos)
                    {
                        if (fsInfo is DirectoryInfo)
                        {
                            Recur((DirectoryInfo)fsInfo, ref audioPaths);
                            continue;
                        }
                        audioPaths.Add(Path.GetRelativePath(taskPath, fsInfo.FullName).Replace("\\", "/"));
                    }
                }
                string fullPath = Path.Combine(AudioRootPath, task);
                var dir = new DirectoryInfo(fullPath);
                List<string> audioPaths = new List<string>();
                Recur(dir, ref audioPaths);
                return audioPaths;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
