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
				link.SetOnExecute(activeCertificates.ShowModal);
				
				var buttonAdd = dialog.Buttons.AddCustom(Resources.ButtonAdd);
				var buttonReplace = dialog.Buttons.AddCustom(Resources.ButtonReplace);
				var buttonCancel = dialog.Buttons.AddCustom(Resources.ButtonCancel);
				
				dialog.Buttons.Default = buttonReplace;
				
				dialog.SetOnButtonClick(
					(b) =>
					{
						if (b.Button == buttonAdd)
							load = true;
						
						if (b.Button == buttonReplace)
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