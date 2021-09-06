using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.UserActivityControl.Server
{
	public class ModuleJobs
	{

		/// <summary>
		/// Фоновый процесс "Мониторинг пользователей в системе"
		/// </summary>
		public virtual void UserMonitoring()
		{
			var enableMonitoring = finex.EditableConstants.PublicFunctions.Module.Remote.GetValueBooleanByName("EnableMonitoring", false);
			if (!enableMonitoring.HasValue || enableMonitoring != true)
				return;
				
			using (var command = Sungero.Core.SQL.GetCurrentConnection().CreateCommand())
			{
				command.CommandText = Queries.Module.MonitoringUsersCount;
				var usersCountStr = command.ExecuteScalar().ToString();
				
				int usersCount;
				if (int.TryParse(usersCountStr, out usersCount))
				{
					var monitor = MonitoringHistories.Create();
					monitor.UsersActiveCount = usersCount;
					monitor.Save();
				}
				else
					Logger.Error("UserMonitoring: Не удалось получить количество пользователей работающих в системе!");
			}
		}

		/// <summary>
		/// Фоновый процесс "Контроль активности пользователей в системе"
		/// </summary>
		public virtual void UserActivityControl()
		{
			int? timeOut = finex.EditableConstants.PublicFunctions.Module.Remote.GetValueIntByName("TimeOutInactivity", false);
			
			var usersIds = new List<int> {};
			var recerveLoginIds = new List<string> {"0"};
			
			if (timeOut.HasValue)
			{
				Logger.DebugFormat("UnregisterUsers: Старт контроля активности пользователей.\r\nТекущий таймаут составляет {0} минут", timeOut.Value);
				
				// Получим пользователей с зарезервированной лицензией
				using (var command = Sungero.Core.SQL.GetCurrentConnection().CreateCommand())
				{
					command.CommandText = string.Format(Queries.Module.SelectReseveMemberIds, Constants.Module.RoleGuid.ReserveLicenseGuid);
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var id = string.Format("{0}", reader.GetValue(0).ToString());
							recerveLoginIds.Add(id);
						}
					}
				}
				
				// Получим пользователей на отключение
				using (var command = Sungero.Core.SQL.GetCurrentConnection().CreateCommand())
				{
					command.CommandText = string.Format(Queries.Module.GetUsersLoginIds, string.Join(", ", recerveLoginIds), Calendar.Now.ToString("o"), timeOut.Value);
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var id = string.Format("{0}", reader.GetValue(0).ToString());
							usersIds.Add(int.Parse(id));
						}
					}
				}
				
				UnregisterUsersAndRecordHistory(usersIds, timeOut.Value);
			}
		}
		
		/// <summary>
		/// Отключить пользователей и записать действие в историю
		/// </summary>
		/// <param name="loginIds">Список ИД пользователей</param>
		/// <param name="timeOut">Таймаут бездействия</param>
		private static void UnregisterUsersAndRecordHistory(List<int> usersIds, int timeOut)
		{
			var loginIds = new List<int> {};
			var dateNow = Calendar.Now;
			
			foreach(var userId in usersIds)
			{
				var user = Sungero.CoreEntities.Users.Get(userId);
				var loginID = user.Login.Id;
				
				var history = UnregisterUsersHistories.Create();
				history.UserID = userId;
				history.LoginID = user.Login.Id;
				history.UserName = user.Name;
				history.SetTimeOut = timeOut;
				history.DateTimeUnregister = dateNow;
				history.Save();
				
				loginIds.Add(loginID);
			}
			
			if (loginIds.Any())
			{
				Logger.DebugFormat("UnregisterUsers: Список логинов пользователей на отключение ({0})", string.Join(", ", loginIds));
				Sungero.Domain.Clients.ClientManager.Instance.Unregister(loginIds);
			}
		}
	}
}