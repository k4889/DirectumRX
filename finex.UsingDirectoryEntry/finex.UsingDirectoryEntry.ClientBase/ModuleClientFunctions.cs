using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.UsingDirectoryEntry.Client
{
	public class ModuleFunctions
	{

		/// <summary>
		/// Вызвать отчет "Использование объекта"
		/// </summary>
		/// <param name="entityID">Id сущности.</param>
		/// <param name="dbTableName">Название таблицы сущности в БД.</param>
		/// <param name="objectName">Имя сущности.</param>
		/// <param name="additionalColums">Список дополнительных полей БД для поиска.</param>
		[Public]
		public void OpenUsingRecordReport(int entityID, string dbTableName, string objectName, List<string> additionalColums)
		{
			if (string.IsNullOrEmpty(dbTableName) || string.IsNullOrWhiteSpace(dbTableName))
				Dialogs.NotifyMessage("Передано пусто имя таблицы!");
			else
			{
				dbTableName = dbTableName.Split('_').LastOrDefault();
				if (dbTableName.Length > 13)
					dbTableName = dbTableName.Substring(0, 13);
				
				var report = Reports.GetUsingRecording();
				report.EntityDirID = entityID;
				report.TableName = dbTableName;
				report.ObjectName = objectName;
				report.FindAllParams = false;
				report.ReportSessionId = System.Guid.NewGuid().ToString();
				
				foreach (var additionValue in additionalColums)
					report.AdditionalColumnName.Add(additionValue);
				
				report.Open();
			}
		}
		
		/// <summary>
		/// Вызвать отчет "Использование объекта"
		/// </summary>
		/// <param name="entityID">Id сущности.</param>
		/// <param name="dbTableName">Название таблицы сущности в БД.</param>
		/// <param name="objectName">Имя сущности.</param>
		[Public]
		public void OpenUsingRecordReport(int entityID, string dbTableName, string objectName)
		{
			if (string.IsNullOrEmpty(dbTableName) || string.IsNullOrWhiteSpace(dbTableName))
				Dialogs.NotifyMessage("Передано пусто имя таблицы!");
			else
			{
				dbTableName = dbTableName.Split('_').LastOrDefault();
				if (dbTableName.Length > 13)
					dbTableName = dbTableName.Substring(0, 13);
				
				var report = Reports.GetUsingRecording();
				report.EntityDirID = entityID;
				report.TableName = dbTableName;
				report.ObjectName = objectName;
				report.FindAllParams = false;
				report.ReportSessionId = System.Guid.NewGuid().ToString();
				report.Open();
			}
		}
		
		/// <summary>
		/// Вызвать отчет "Использование объекта" (поиск по всем колонкам типа int в БД)
		/// </summary>
		/// <param name="entityID">Id сущности.</param>
		/// <param name="objectName">Имя сущности.</param>
		[Public]
		public void OpenUsingRecordReport(int entityID, string objectName)
		{
			var report = Reports.GetUsingRecording();
			report.EntityDirID = entityID;
			report.ObjectName = objectName;
			report.FindAllParams = true;
			report.ReportSessionId = System.Guid.NewGuid().ToString();			
			report.Open();
		}
	}
}