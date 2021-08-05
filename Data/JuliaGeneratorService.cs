using System;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace amazing.fractals.Data
{
    public class JuliaGeneratorService
    {
        private const int ThreadsCount = 4;
        
        public Task<byte[]> Generate(FractalGenerationData genData)
        {
            return Task.FromResult(ImageToByteArray(GetFractalImage(genData)));
        }

        private Image GetFractalImage(FractalGenerationData genData)
        {
            var bm = new Bitmap(genData.ImageWidth, genData.ImageHeight);

            var tasks = new Task[ThreadsCount];

            var stepX = genData.ImageWidth / ThreadsCount;
            var n = 0;
            var iters = new int[genData.ImageWidth, genData.ImageHeight];

            for (var i = 0; i < ThreadsCount; i++)
                tasks[i] = Task.Run(() =>
                {
                    var num = Interlocked.Increment(ref n);

                    GetIters(iters,
                        genData,
                        (num - 1) * stepX,
                        num * stepX,
                        0,
                        genData.ImageHeight);
                });

            Task.WaitAll(tasks);

            var data = bm.LockBits(
                new Rectangle(0, 0, bm.Width, bm.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);
            var stride = data.Stride;

            unsafe
            {
                byte* ptr = (byte*) data.Scan0;
                
                for (var x = 0; x < genData.ImageWidth; x++)
                for (var y = 0; y < genData.ImageHeight; y++)
                {
                    var cn = (byte)(255 - iters[x, y] % 255);
                    ptr[(x * 3) + y * stride] = cn;
                    ptr[(x * 3) + y * stride + 1] = cn;
                    ptr[(x * 3) + y * stride + 2] = cn;
                }
            }

            bm.UnlockBits(data);

            return bm;
        }

        private void GetIters(int[,] iters, 
            FractalGenerationData genData, 
            int fromX, 
            int toX, 
            int fromY, 
            int toY)
        {
            for (var x= fromX; x < toX; x++)
            for (var y = fromY; y < toY; y++)
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

                iters[x, y] = iter;
            }
        }

        private byte[] ImageToByteArray(Image img)
        {
            using var ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            
            return ms.ToArray();
        }
    }
}