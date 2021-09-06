using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.Certificates.Server
{
	public class ModuleJobs
	{

		/// <summary>
		/// Мониторинг сроков действия сертификатов
		/// </summary>
		public virtual void MonitoringCertificateDates()
		{
			var noticeText = finex.EditableConstants.PublicFunctions.Module.Remote.GetValueTextByName("CertificateNoticeText", true);
			
			var monitoringPeriod = finex.EditableConstants.PublicFunctions.Module.Remote.GetValueIntByName("CertificateMonitoringPeriod", false);
			if (!monitoringPeriod.HasValue)
				monitoringPeriod = 14;
			
			if (monitoringPeriod.Value < 0)
				monitoringPeriod = 0;

			var today = Calendar.Today;
			var beginingDate = today.AddDays(monitoringPeriod.Value);
				
			var certificates = Sungero.CoreEntities.Certificates.GetAll()
				.Where(c => c.Enabled == true)
				.Where(c => c.NotAfter.HasValue)
				.Where(c => c.NotAfter > today)
				.Where(c => c.NotAfter <= beginingDate);

			foreach (var certificate in certificates)
			{			
				var subject = Resources.NoticeSubjectFormat(certificate.NotAfter.Value.ToString("D"));
				finex.CollectionFunctions.PublicFunctions.Module.Remote.SendNotice(subject, noticeText, certificate.Owner, certificate);
			}
		}

	}
}