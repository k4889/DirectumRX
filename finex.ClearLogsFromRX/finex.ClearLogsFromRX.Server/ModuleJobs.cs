using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;

namespace finex.ClearLogsFromRX.Server
{
	public class ModuleJobs
	{

		/// <summary>
		/// Очистка лог файлов
		/// </summary>
		public virtual void ClearLogs()
		{
			var folderPaths = finex.EditableConstants.PublicFunctions.Module.Remote.GetValueListStringByName("ClearLogsPaths", true);
			if (!folderPaths.Any())
				return;

			var lifeTime = finex.EditableConstants.PublicFunctions.Module.Remote.GetValueIntByName("LogsLifeTime", true);
			if (!lifeTime.HasValue)
				lifeTime = 14;

			var dateNow = Calendar.Today;
			var currentUser = WindowsIdentity.GetCurrent();
			
			foreach (var folderPath in folderPaths.Where(f => !string.IsNullOrEmpty(f) && !string.IsNullOrWhiteSpace(f)))
			{
				List<FileInfo> filesInfoList = GetDocumentsCount(folderPath);
				foreach (var fileInfo in filesInfoList)
				{
					var createDateString = fileInfo.CreationTime.ToString().Split(' ').FirstOrDefault();
					var createDate = DateTime.ParseExact(createDateString, "dd.MM.yyyy", TenantInfo.Culture);
					var difference = dateNow.Subtract(createDate.Date).Days;
					
					if (difference >= lifeTime)
					{
						try
						{
							Logger.DebugFormat("ClearLogs: Удаляем лог: {0}", fileInfo.FullName);
							fileInfo.Delete();
						}
						catch (Exception ex)
						{
							Logger.ErrorFormat("ClearLogs: Ошибка при удалении лог файла (пользователь {0}): {1}", currentUser.Name, ex.Message);
						}
					}
				}
			}
		}

		/// <summary>
		/// Подсчет количества документов в папке
		/// </summary>
		/// <param name="folderPath"> Папка .</param>
		public virtual List<FileInfo> GetDocumentsCount(string folderPath)
		{
			DirectoryInfo pathDir = new DirectoryInfo(folderPath);
			string[] filters = { "*.log" };

			List<FileInfo> pathfiles =  filters
				.SelectMany(filter => pathDir.EnumerateFiles(filter, SearchOption.AllDirectories))
				.Distinct()
				.ToList();
			return pathfiles;
		}
	}
}