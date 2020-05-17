using MathCore.WPF.ViewModels;

namespace ArrayFactor.View.Models
{
    internal abstract class PageModel : ViewModel
    {
        private readonly PresentationModel _BaseModel;
        private string _Title = "Page";

        public string Title
        {
            get => _Title;
            set
            {
                if(_Title == value) return;
                _Title = value;
                OnPropertyChanged();
            }
        }

        public PresentationModel BaseModel => _BaseModel;

        protected PageModel(PresentationModel BaseModel) { _BaseModel = BaseModel; }

        public virtual void OnActivated() { }
        public virtual void OnDeactivated() { }
    }
}
