using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.AdditionalSettingsUI.Structures.Module
{
	/// <summary>
	/// Структура данных блокировок в системе
	/// </summary>
	partial class LockData
	{
		/// <summary>
		/// ИД логина пользователя
		/// </summary>
		public int LoginID { get; set; }
		
		/// <summary>
		/// Имя пользователя
		/// </summary>
		public string UserName { get; set; }
		
		/// <summary>
		/// ИД заблокированного объекта
		/// </summary>
		public int EntityID { get; set; }
		
		/// <summary>
		/// GUID типа заблокированного объекта
		/// </summary>
		public string TypeGuid { get; set; }
		
		/// <summary>
		/// Дата и время блокировки
		/// </summary>
		public DateTime LockTime { get; set; }
	}
}