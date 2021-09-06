using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace finex.UsingDirectoryEntry.Server
{
	public partial class ModuleInitializer
	{

		public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
		{
			SetGrantRights();
			
			CreateReportsTables();
		}
		
		#region Выдача прав на объекты
		/// <summary>
		/// Выдать права пользователям
		/// </summary>
		public static void SetGrantRights()
		{
			// Выдача прав на выполнение всем пользователям.
			InitializationLogger.Debug("Init: Для отчетов выданы права выполнения всем пользователям");
			GrantRightsOnReports(Roles.AllUsers, DefaultReportAccessRightsTypes.Execute);
		}
		
		/// <summary>
		/// Выдать права пользователям на отчеты.
		/// </summary>
		/// <param name="users">Группа пользователей.</param>
		/// <param name="accessRights">Тип прав.</param>
		public static void GrantRightsOnReports(IRole users, Guid accessRights)
		{
			finex.UsingDirectoryEntry.Reports.AccessRights.Grant(Reports.GetUsingRecording().Info, users, accessRights);
		}
		
		#endregion
		
		#region Отчеты
		
		/// <summary>
		/// Создать таблиц для отчетов.
		/// </summary>
		public static void CreateReportsTables()
		{
			var temporaryTableReportName = Constants.Module.temporaryTableName;
			Sungero.Docflow.PublicFunctions.Module.DropReportTempTables(new[] { temporaryTableReportName });
			Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(Queries.UsingRecording.CreateTempTable, new[] { temporaryTableReportName });
		}
		
		#endregion
	}
}
