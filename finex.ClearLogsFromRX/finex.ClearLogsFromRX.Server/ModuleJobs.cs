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

      var checkDate = Calendar.Now.AddDays(lifeTime.Value * -1);
      var currentUser = WindowsIdentity.GetCurrent();
      
      foreach (var folderPath in folderPaths.Where(f => !string.IsNullOrEmpty(f) && !string.IsNullOrWhiteSpace(f)))
      {
        List<FileInfo> filesInfoList = GetDocumentsByFolder(folderPath);
        foreach (var fileInfo in filesInfoList.Where(f => f.CreationTime <= checkDate))
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

    /// <summary>
    ///  Получить все логи в папке
    /// </summary>
    /// <param name="folderPath">Путь к папке</param>
    /// <returns>Лист с файлами логов</returns>
    public virtual List<FileInfo> GetDocumentsByFolder(string folderPath)
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