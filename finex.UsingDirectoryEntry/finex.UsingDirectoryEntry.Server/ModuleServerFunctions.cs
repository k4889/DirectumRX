using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.UsingDirectoryEntry.Server
{
	public class ModuleFunctions
	{
		/// <summary>
		/// Создать временную таблицу.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		public static void CreateTempTable(string tableName)
		{
			// Удалить старую временную таблицу.
			Sungero.Docflow.PublicFunctions.Module.DropReportTempTable(tableName);
			
			// Создать новую временную таблицу
			Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(Queries.UsingRecording.CreateTempTable, new[] { tableName });
		}
	}
}