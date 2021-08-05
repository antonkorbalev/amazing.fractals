using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace amazing.fractals.Data
{
    public class JuliaGeneratorService
    {
        public Task<byte[]> Generate(FractalGenerationData genData)
        {
            return Task.FromResult(ImageToByteArray(GetFractalImage(genData)));
        }

        private Image GetFractalImage(FractalGenerationData genData)
        {
            var bm = new Bitmap(genData.ImageWidth, genData.ImageHeight);
            
            for (var x=0; x < genData.ImageWidth; x++)
            for (var y = 0; y < genData.ImageHeight; y++)
            {
                var newRe = 1.5 * (x + genData.MoveX - genData.ImageWidth / 2) / (0.5 * genData.Zoom * genData.ImageWidth);
                var newIm = (y + genData.MoveY - genData.ImageHeight / 2) / (0.5 * genData.Zoom * genData.ImageHeight);

                var iter = 0;
                for (var i = 0; i < genData.MaxIterations; i++)
                {
                    var oldRe = newRe;
                    var oldIm = newIm;
                    
                    newRe = oldRe * oldRe - oldIm * oldIm + genData.CRe;
                    newIm = 2 * oldRe * oldIm + genData.CIm;

                    iter = i;
                    if((newRe * newRe + newIm * newIm) > 4) 
                        break;
                }
                
                bm.SetPixel(x, y, 
                    Color.FromArgb(
                        255, 
                        255 - iter % 255, 
                        255 - iter % 255, 
                        255 - iter % 255));
            }
            
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