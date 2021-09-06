using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.Certificates.Client
{
	public class ModuleFunctions
	{
		
		/// <summary>
		/// Показать диалог импорта сертификата
		/// </summary>
		[Public]
		public virtual void ShowImportCertificateDialog()
		{
			bool load = true;
			var closeCertificate = Sungero.CoreEntities.Certificates.Null;
			
			var activeCertificates = finex.Certificates.PublicFunctions.Module.Remote.GetActiveCertificate();
			if (activeCertificates.Any())
			{
				load = false;
				var dialog = Dialogs.CreateInputDialog(Resources.ActiveCertificatesTitle);
				dialog.Text = Resources.ActiveCertificatesMessage;
				
				var fakeControl = dialog.AddString("hideControl", false);
				fakeControl.IsVisible = false;
				
				var link = dialog.AddHyperlink(Resources.LinkTitle);
				link.SetOnExecute(() => {
				                  	activeCertificates.ShowModal();
				                  });
				
				var button_Add = dialog.Buttons.AddCustom(Resources.ButtonAdd);
				var button_Replace = dialog.Buttons.AddCustom(Resources.ButtonReplace);
				var button_Cancel = dialog.Buttons.AddCustom(Resources.ButtonCancel);
				
				dialog.Buttons.Default = button_Replace;
				
				dialog.SetOnButtonClick(
					(b) =>
					{
						if (b.Button == button_Add)
							load = true;
						
						if (b.Button == button_Replace)
						{
							closeCertificate = activeCertificates.ShowSelect();
							if (closeCertificate == null)
								b.AddError(Resources.SelectCertificateCloseMessage);
							else
								load = true;
						}	
					});
				
				dialog.Show();
			}
			
			if (!load)
				return;
			
			var file = finex.CollectionFunctions.PublicFunctions.Module.ShowFilesDialog(Resources.ImportingCertificateDialogTitleFormat(Sungero.CoreEntities.Users.Current.Name), 26214400, new string[] {".cer",".crt"});
			if (file != null)
			{
				var byteArray = Sungero.Docflow.Structures.Module.ByteArray.Create(file.Content);
				if (finex.Certificates.PublicFunctions.Module.Remote.CheckCopyCertificate(byteArray))
				{
					Dialogs.ShowMessage(Resources.CopyCertificateMessage, MessageType.Error);
					return;
				}

				if (closeCertificate != null)
					finex.Certificates.PublicFunctions.Module.Remote.CloseCertificateAsyncHandler(closeCertificate.Id);
				
				var certificate = finex.Certificates.PublicFunctions.Module.Remote.CreateCertificate(byteArray, string.Empty);
				if (Dialogs.CreateConfirmDialog(Resources.ImportingCertificateDialogComplite).Show())
					certificate.ShowModal();
			}
		}
		
	}
}