namespace amazing.fractals.Data
{
    public class FractalGenerationData
    {
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public double CIm { get; set; }
        public double CRe { get; set; }
        public int MaxIterations { get; set; }
        
        public double R { get; set; }

        public double FromX { get; set; }
        public double ToX { get; set; }
        public double FromY { get; set; }
        public double ToY { get; set; }
        
        public FractalGenerationData()
        {
            FromX = -1;
            ToX = 1;
            FromY = -1;
            ToY = 1;
        }
    }
}