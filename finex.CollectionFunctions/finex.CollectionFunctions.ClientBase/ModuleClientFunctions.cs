using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Content;

namespace finex.CollectionFunctions.Client
{
	public class ModuleFunctions
	{
		
		#region Диалоги
		
		/// <summary>
		/// Создать версию документа из файла
		/// </summary>
		/// <param name="document">Документ</param>
		/// <param name="filter">Возможные расширения файлов. Если передать null, то игнорируется</param>
		[Public]
		public virtual void CreateDocumentVersionFromFile(Sungero.Content.IElectronicDocument document, string[] filter = null)
		{
			if (document == null)
			{
				Dialogs.ShowMessage(Resources.DocumentMissing, MessageType.Error);
				return;
			}
			
			var title = Resources.DialogCreateVersionFileTitleFormat(document.Name);
			
			var file = ShowFilesDialog(title, 0, filter);
			if (file == null)
				return;
			
			var byteArray = Sungero.Docflow.Structures.Module.ByteArray.Create(file.Content);
			var extention = file.Name.Split('.').LastOrDefault();
			
			Functions.Module.Remote.CreateVersionFromByteArray(document, byteArray, extention, true);
			Dialogs.ShowMessage(Resources.CreateVersionComplite, MessageType.Information);
		}
		
		/// <summary>
		/// Создать диалог выбора файла
		/// </summary>
		/// <param name="title">Заголовок диалога</param>
		/// <param name="maxFileSize">Максимальный размер файла в байтах. Если передать 0, то игнорируется</param>
		/// <param name="filter">Возможные расширения файлов. Если передать null, то игнорируется</param>
		/// <returns>Файл. Если нажата отмена - null</returns>
		[Public]
		public static CommonLibrary.IBinaryObject ShowFilesDialog(string title, int maxFileSize = 0, string[] filter = null)
		{
			var dialog = Dialogs.CreateInputDialog(title);
			
			var titleLength = title.Length > 40 ? title.Length - 15 : title.Length;
			var fakeControl = dialog.AddString(title.Substring(0, titleLength), false);
			fakeControl.IsVisible = false;
				
			var fileSelector = AddFileSelector(dialog, Resources.ShowFilesDialog_FileSelector, true, true, true, maxFileSize, filter);
			dialog.Buttons.AddOkCancel();
			
			dialog.SetOnRefresh(
				e =>
				{
					if (filter != null && fileSelector.Value != null)
					{
						var selectorExtention = fileSelector.Value.Name.Split('.').LastOrDefault().ToLower();
						if (!filter.Any(f => f.Split('.').LastOrDefault().ToLower() == selectorExtention))
							e.AddError(string.Format(Resources.ShowFilesDialog_FileExtentionError, selectorExtention), fileSelector);
					}
				});
			
			if (dialog.Show() == DialogButtons.Ok)
				return fileSelector.Value;
			else
				return null;
		}
		
		/// <summary>
		/// Создать FileSelector в переданном диалоге
		/// </summary>
		/// <param name="dialog">Диалог в котором создаётся селектор</param>
		/// <param name="title">Заголовок селектора</param>
		/// <param name="isRequired">Обязательность селектора</param>
		/// <param name="isEnabled">Активность селектора</param>
		/// <param name="isVisible">Видимость селектора</param>
		/// <param name="maxFileSize">Максимальный размер файла в байтах. Если передать 0, то игнорируется</param>
		/// <param name="filter">Возможные расширения файлов. Если передать null, то игнорируется</param>
		/// <returns>Селектор файла</returns>
		[Public]
		public static CommonLibrary.IFileSelectDialogValue AddFileSelector(CommonLibrary.IInputDialog dialog, string title, bool isRequired = false, bool isEnabled = true, bool isVisible = true, int maxFileSize = 0, string[] filter = null)
		{
			var fileSelector = dialog.AddFileSelect(title, isRequired);
			fileSelector.IsEnabled = isEnabled;
			fileSelector.IsVisible = isVisible;
			
			if (maxFileSize > 0)
				fileSelector.MaxFileSize(maxFileSize);
			
			if (filter != null)
				fileSelector.WithFilter("Файлы", "Расширения", filter);
			
			return fileSelector;
		}
		
		#endregion
				
		
		#region Публичные функции модуля для вызова из DrxUtil
		
		/// <summary>
		/// Пересохранить все записи справочника "Подразделения".
		/// </summary>
		[Public]
		public virtual void ReSaveAllDepartments()
		{
			var res = Functions.Module.Remote.ReSaveAllDepartments();
			if (res)
				Logger.Debug("Saves all departments complite!");
			else
				Logger.Debug("Saves departments error!");
		}
		
		/// <summary>
		/// Пересохранить все записи справочника "Сотрудники".
		/// </summary>
		[Public]
		public virtual void ReSaveAllEmployees()
		{
			var res = Functions.Module.Remote.ReSaveAllEmployees();
			if (res)
				Logger.Debug("Saves all employees complite!");
			else
				Logger.Debug("Saves employees error!");
		}
		
		#endregion
	
	}
}