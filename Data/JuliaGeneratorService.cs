using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace amazing.fractals.Data
{
    public class JuliaGeneratorService
    {
        private readonly ILogger _logger;
        private const int ThreadsCount = 4;
        private const byte Treshold = 30;
        
        public JuliaGeneratorService(ILogger<JuliaGeneratorService> logger)
        {
            _logger = logger;
        }
        
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

            _logger.LogInformation($"Starting fractal generation for image {genData.ImageWidth}x{genData.ImageHeight}px  ..");
            var sw = new Stopwatch();
            sw.Start();
            
            for (var i = 0; i < ThreadsCount; i++)
                tasks[i] = Task.Run(() =>
                {
                    var num = Interlocked.Increment(ref n);

                    try
                    {
                        GetItersForImagePoints(iters,
                            genData,
                            (num - 1) * stepX,
                            num * stepX,
                            0,
                            genData.ImageHeight);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex.Message);
                        _logger.LogCritical(ex.StackTrace);
                    }
                });

            Task.WaitAll(tasks);
            sw.Stop();
            _logger.LogInformation($"Fractal generation took {sw.ElapsedMilliseconds} ms ..");
            _logger.LogInformation($"Processing image {genData.ImageWidth}x{genData.ImageHeight}px ..");
            sw.Restart();
            var data = bm.LockBits(
                new Rectangle(0, 0, bm.Width, bm.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);
            var stride = data.Stride;

            unsafe
            {
                var ptr = (byte*) data.Scan0;
                
                for (var x = 0; x < genData.ImageWidth; x++)
                for (var y = 0; y < genData.ImageHeight; y++)
                {
                    var cn = (byte)(255 - iters[x, y] % 255);
                    if (cn > 255 - Treshold)
                        cn = 255;
                    
                    if (ptr == null)
                        continue;
                    
                    ptr[(x * 3) + y * stride] = cn;
                    ptr[(x * 3) + y * stride + 1] = cn;
                    ptr[(x * 3) + y * stride + 2] = cn;
                }
            }

            bm.UnlockBits(data);
            _logger.LogInformation($"Image processing took {sw.ElapsedMilliseconds} ms ..");
            
            return bm;
        }

        private void GetItersForImagePoints(int[,] iters, 
            FractalGenerationData genData, 
            int fromX, 
            int toX, 
            int fromY, 
            int toY)
        {
            var sx = (genData.ToX - genData.FromX) / genData.ImageWidth;
            var sy = (genData.ToY - genData.FromY) / genData.ImageHeight;

            for (var x= fromX; x < toX; x++)
            for (var y = fromY; y < toY; y++)
            {
                var newRe = genData.FromX + sx * x;
                var newIm = genData.FromY + sy * y;

                var iter = 0;
                for (var i = 0; i < genData.MaxIterations; i++)
                {
                    var oldRe = newRe;
                    var oldIm = newIm;
                    
                    newRe = oldRe * oldRe - oldIm * oldIm + genData.CRe;
                    newIm = 2 * oldRe * oldIm + genData.CIm;

                    iter = i;
                    if((newRe * newRe + newIm * newIm) > genData.R * genData.R) 
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