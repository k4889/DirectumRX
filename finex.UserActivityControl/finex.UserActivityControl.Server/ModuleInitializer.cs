using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace finex.UserActivityControl.Server
{
	public partial class ModuleInitializer
	{

		public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
		{
			CreateConstansts();
			
			CreateSystemRoles();
		}
		
		#region Создание записей справочника "Константы"
		
		/// <summary>
		/// Создание записей справочников
		/// </summary>
		public static void CreateConstansts()
		{
			//Создание группы констант
			finex.EditableConstants.PublicInitializationFunctions.Module.CreateGroup("Контроль пользователей", "Группа констант относящихся к подсистеме контроля активности пользователей в Directum RX.");
			
			//Создание констант
			finex.EditableConstants.PublicInitializationFunctions.Module.CreateConstants("TimeOutInactivity", 480, "Таймаут бездействия в системе (в минутах)", "Контроль пользователей");
			//Создание констант
			finex.EditableConstants.PublicInitializationFunctions.Module.CreateConstants("EnableMonitoring", false, "Включить сбор статистики по количеству работающих пользователей в системе", "Контроль пользователей");
		}
		
		#endregion
		
		#region Создание ролей
		
		public static void CreateSystemRoles()
		{
			finex.CollectionFunctions.PublicInitializationFunctions.Module.CreateRole(Constants.Module.RoleNames.ReserveLicenseName,
			                                                                          "Сотрудники с зарезервированной лицензией.",
			                                                                          Constants.Module.RoleGuid.ReserveLicenseGuid,
			                                                                          false);
		}
		
		#endregion
	}
}
