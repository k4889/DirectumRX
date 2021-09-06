using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.Certificates.Server
{
	public class ModuleAsyncHandlers
	{

		public virtual void CloseCertificate(finex.Certificates.Server.AsyncHandlerInvokeArgs.CloseCertificateInvokeArgs args)
		{
			var certificateId = args.certificateId;
			if (certificateId == 0)
				return;
						
			var certificate = Sungero.CoreEntities.Certificates.GetAll().Where(c => c.Id == certificateId && c.Enabled == true).FirstOrDefault();
			if (certificate == null)
				return;
			
			var lockInfo = Locks.GetLockInfo(certificate);
			if (lockInfo.IsLockedByOther)
				return;
			
			try
			{
				certificate.Enabled = false;
				certificate.Save();
			}
			catch (Exception ex)
			{
				var message = Resources.CertificateCloseErrorFormat(certificate.Id, ex.Message);
				Logger.Error(message);
				finex.CollectionFunctions.PublicFunctions.Module.Remote.SendNotice(Resources.CertificateCloseTitleFormat(certificate.Id), message, certificate);
			}
		}

	}
}