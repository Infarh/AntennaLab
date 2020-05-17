namespace ArrayFactor.View.Models
{
    internal abstract class SubPageModel : PageModel
    {
        public PageModel PreviousModel { get; }

        protected SubPageModel(PageModel PreviousModel) : base(PreviousModel.BaseModel)
        {
            this.PreviousModel = PreviousModel;
        }
    }
}