using System.Threading.Tasks;

namespace amazing.fractals.Data
{
    public class JuliaGeneratorService
    {
        public Task<FractalGenerationData> Generate()
        {
            return Task.FromResult(new FractalGenerationData() { Test = "a" });
        }
    }
}