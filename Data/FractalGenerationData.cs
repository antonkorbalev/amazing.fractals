namespace amazing.fractals.Data
{
    public class FractalGenerationData
    {
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public double CIm { get; set; }
        public double CRe { get; set; }
        public int MaxIterations { get; set; }
        public double Zoom { get; set; }
        public double MoveX { get; set; }
        public double MoveY { get; set; }

        public FractalGenerationData()
        {
            Zoom = 1;
        }
    }
}