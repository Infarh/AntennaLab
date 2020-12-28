using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MathCore.WPF.Commands;

namespace ArrayFactor.View.Models
{
    internal class RegistrationPageModel : PageModel
    {
        private string _UserGroupName;
        private string _UserName;
        private readonly Dictionary<int, string> _KnownGroups = new();
        private readonly Dictionary<int, string> _KnownUsers = new();

        public string UserGroupName
        {
            get => _UserGroupName;
            set
            {
                if(_UserGroupName == value) return;
                _UserGroupName = value;
                OnPropertyChanged();
            }
        }

        public string UserName
        {
            get => _UserName;
            set
            {
                if(_UserName == value) return;
                _UserName = value;
                OnPropertyChanged();
            }
        }

        public string[] KnownGroups => _KnownGroups.Values.ToArray();
        public string[] KnownUsers => _KnownUsers.Values.ToArray();

        public ICommand LoginCommand { get; }

        public RegistrationPageModel(PresentationModel BaseModel) : base(BaseModel)
        {
            Title = "Регистрация";
            LoginCommand = new LambdaCommand(LoginAction, () => !UserName.IsNullOrWhiteSpace());
            LoadData();
        }

        private async void LoadData()
        {
            var groups_load_task = LoadKnownGroups().ContinueWith(t => OnPropertyChanged(nameof(KnownGroups)));
            var users_load_task = LoadKnownUsers().ContinueWith(t => OnPropertyChanged(nameof(KnownUsers)));
            await Task.WhenAll(groups_load_task, users_load_task).ConfigureAwait(false);
        }

        private async Task LoadKnownGroups()
        {

        }

        private async Task LoadKnownUsers()
        {

        }

        private void LoginAction() => BaseModel.CurrentModelIndex++;
    }
}