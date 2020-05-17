namespace ArrayFactor.View.Models
{
    internal class RawDataPageModel : PageModel
    {
        public double BeamWidthOX { get; }
        public double BeamWidthOY { get; }

        public double ScanSectorWidthOX { get; }
        public double ScanSectorWidthOY { get; }

        public RawDataPageModel(PresentationModel BaseModel) : base(BaseModel)
        {
            BeamWidthOX = BaseModel.RandomValue(2, 3, 4, 5);
            BeamWidthOY = BaseModel.RandomValue(7, 8, 9, 10);

            ScanSectorWidthOX = BaseModel.RandomValue(30, 35, 40, 45);
            ScanSectorWidthOY = BaseModel.RandomValue(10, 15, 20);
        }
    }
}