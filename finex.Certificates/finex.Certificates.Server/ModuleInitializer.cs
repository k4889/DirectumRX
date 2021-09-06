using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace finex.Certificates.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
		{
			CreateConstants();
		}
		
		#region Создание записей справочника "Константы"
		
		/// <summary>
		/// Создание записей справочников
		/// </summary>
		public static void CreateConstants()
		{
			//Создание группы констант
			finex.EditableConstants.PublicInitializationFunctions.Module.CreateGroup("Сертификаты", "Группа констант предназначенная для работы с пользовательскими сертификатами в системе DirectumRX");
			
			//Создание констант
			finex.EditableConstants.PublicInitializationFunctions.Module.CreateConstants("CertificateMonitoringPeriod",
			                                                                             14,
			                                                                             "Срок мониторинга даты окончания сертификатов (в календарных днях)",
			                                                                             "Сертификаты");
			
			//Создание констант
			finex.EditableConstants.PublicInitializationFunctions.Module.CreateConstants("CertificateNoticeText",
			                                                                             "Заканчивается срок действия сертификата",
			                                                                             true,
			                                                                             "Текст уведомления отправляемый пользователю при окончании срока действия сертификата",
			                                                                             "Сертификаты");
		}
		
		#endregion
		
  }
}
