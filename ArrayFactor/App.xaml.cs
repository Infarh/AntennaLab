using System;
using System.Security.Principal;
using System.Windows;

namespace ArrayFactor
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

            //var fileIOPermission = new FileIOPermission(PermissionState.Unrestricted);
            //fileIOPermission.Demand();

            //var princopal = (WindowsPrincipal)Thread.CurrentPrincipal;
            //var identity = princopal.Identity;
            //princopal.AddIdentity(new ClaimsIdentity(new [] { new Claim("type", "value", "value_type") }, "auth_type", "name_type", "role_type"));

            //try
            //{
            //    ShowMessage("Hello!");
            //}
            //catch (SecurityException error)
            //{
            //    MessageBox.Show(error.Message, "Sequrity error!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            //}
            //catch (Exception error)
            //{
            //    MessageBox.Show(error.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            //}

        }

        //[PrincipalPermission(SecurityAction.Demand, Role = "qwe")]
        //private void ShowMessage(string msg) => MessageBox.Show(msg, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
    }  
}
