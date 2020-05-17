namespace ArrayFactor.View.Models
{
    internal class WelcomePageModel : PageModel
    {
        public WelcomePageModel(PresentationModel BaseModel) : base(BaseModel) { Title = "Добро пожаловать!"; }
    }
}