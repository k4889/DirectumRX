using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.CollectionFunctions.Structures.Module
{
	/// <summary>
	/// Файл
	/// </summary>
	[Public]
	partial class File
	{
		/// <summary>
		/// Имя файла
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// Содержание файла
		/// </summary>
		public byte[] Content { get; set; }
	}
	
	/// <summary>
	/// Результат преобразования документа в PDF.
	/// </summary>
	[Public]
	partial class СonversionToPdfResult
	{
		public bool IsFastConvertion { get; set; }
		
		public bool IsOnConvertion { get; set; }
		
		public bool HasErrors { get; set; }
		
		public bool HasConvertionError { get; set; }
		
		public bool HasLockError { get; set; }
		
		public string ErrorTitle { get; set; }
		
		public string ErrorMessage { get; set; }
		
		public byte[] Body { get; set; }
	}
	
	/// <summary>
	/// Структура с позицией якоря на странице
	/// </summary>
	partial class AnchorPosition
	{
		public double XIndent { get; set; }
		
		public double YIndent { get; set; }
		
		public double RectangleHeight { get; set; }
		
		public int PageNumber { get; set; }
	}
	
	/// <summary>
	/// Структура c информацией о текущем подключении пользователя
	/// </summary>
	[Public]
	partial class UserInfo
	{
		/// <summary>
		/// ИД подключения
		/// </summary>
		public Guid ClientId { get; set; }
		
	  /// <summary>
		/// ИД учетной записи
		/// </summary>
		public int LoginId { get; set; }
		
	  /// <summary>
		/// Имя пользователя
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// Имя приложения сессии
		/// </summary>
		public string ApplicationName { get; set; }
		
		/// <summary>
		/// Имя хоста сессии
		/// </summary>
		public string HostName { get; set; }		
		
		/// <summary>
		/// Дата последней активности в системе
		/// </summary>
		public DateTime LastActivity { get; set; }
	}
	
}