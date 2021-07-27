using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace amazing.fractals.Data
{
    public class JuliaGeneratorService
    {
        public Task<FractalGenerationData> Generate(int sizeX, int sizeY)
        {
            return Task.FromResult(new FractalGenerationData()
                {
                    FractalData = ImageToByteArray(GetFractalImage(sizeX, sizeY))
                }
            );
        }

        private Image GetFractalImage(int sizeX, int sizeY)
        {
            var bm = new Bitmap(sizeX, sizeY);
            
            for (var i=0; i < sizeX; i++)
            for (var j = 0; j < sizeY; j++)
                bm.SetPixel(i, j, Color.FromArgb(i % 255, j % 255, 0));
            
            return bm;
        }

        public byte[] ImageToByteArray(Image img)
        {
            using var ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            
            return ms.ToArray();
        }
    }
}