using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Http;

namespace thegreatest.Controllers
{
    public class RecordController : ApiController
    {
        //ex. http://localhost:63541/api/record/get?id=100&x=57&y=148
        public IHttpActionResult Get(int id, int x, int y)
        {
            Thread.Sleep(200);
            var bmp = new Bitmap(200, 100);
            var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(new Point(x, y), new Point(0, 0), bmp.Size);
            var path = HttpContext.Current.Request.MapPath($"~/imgs/{id}.jpg");
            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                {
                    bmp.Save(memory, ImageFormat.Jpeg);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            return this.Content(HttpStatusCode.OK, id);
        }
    }
}
