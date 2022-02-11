using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        string MicRootPath = @"D:\Users\KKKKKP\Desktop\phy_exp\collected";
        string AudioRootPath = @"D:\Users\KKKKKP\Desktop\phy_exp\adv_examples";
        [HttpGet]
        public ActionResult<string> PlayAudio(string path= "1000.wav")
        {
            try{
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
            try{
                string fullPath = Path.Combine(AudioRootPath, task);
                var dir = new DirectoryInfo(fullPath);
                var fsInfos = dir.GetFiles();
                List<string> audioPaths = new List<string>();
                foreach (var fsInfo in fsInfos)
                {
                    audioPaths.Add(fsInfo.Name);
                }
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
