using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.LogManager.Server
{
  public class ModuleFunctions
  {
    
    #region Функции логирования

    #region Trace

    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    [Remote(IsPure=true), Public]
    public static void Trace(string fileName, string message)
    {
      WriteLog(string.Empty, fileName, message, NLog.LogLevel.Trace.Name, false);
    }
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="folderName">Имя папки</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    [Remote(IsPure=true), Public]
    public static void Trace(string folderName, string fileName, string message)
    {
      WriteLog(folderName, fileName, message, NLog.LogLevel.Trace.Name, false);
    }
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    /// <param name="isWriteToFolderProcess">Записывать лог в родительскую папку процесса</param>
    [Remote(IsPure=true), Public]
    public static void Trace(string fileName, string message, bool isWriteToFolderProcess)
    {
      WriteLog(string.Empty, fileName, message, NLog.LogLevel.Trace.Name, isWriteToFolderProcess);
    }
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="folderName">Имя папки</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    /// <param name="isWriteToFolderProcess">Записывать лог в родительскую папку процесса</param>
    [Remote(IsPure=true), Public]
    public static void Trace(string folderName, string fileName, string message, bool isWriteToFolderProcess)
    {
      WriteLog(folderName, fileName, message, NLog.LogLevel.Trace.Name, isWriteToFolderProcess);
    }
    
    #endregion
    
    #region Info

    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    [Remote(IsPure=true), Public]
    public static void Info(string fileName, string message)
    {
      WriteLog(string.Empty, fileName, message, NLog.LogLevel.Info.Name, false);
    }
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="folderName">Имя папки</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    [Remote(IsPure=true), Public]
    public static void Info(string folderName, string fileName, string message)
    {
      WriteLog(folderName, fileName, message, NLog.LogLevel.Info.Name, false);
    }
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    /// <param name="isWriteToFolderProcess">Записывать лог в родительскую папку процесса</param>
    [Remote(IsPure=true), Public]
    public static void Info(string fileName, string message, bool isWriteToFolderProcess)
    {
      WriteLog(string.Empty, fileName, message, NLog.LogLevel.Info.Name, isWriteToFolderProcess);
    }
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="folderName">Имя папки</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    /// <param name="isWriteToFolderProcess">Записывать лог в родительскую папку процесса</param>
    [Remote(IsPure=true), Public]
    public static void Info(string folderName, string fileName, string message, bool isWriteToFolderProcess)
    {
      WriteLog(folderName, fileName, message, NLog.LogLevel.Info.Name, isWriteToFolderProcess);
    }
    
    #endregion
    
    #region Debug
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    [Remote(IsPure=true), Public]
    public static void Debug(string fileName, string message)
    {
      WriteLog(string.Empty, fileName, message, NLog.LogLevel.Debug.Name, false);
    }
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="folderName">Имя папки</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    [Remote(IsPure=true), Public]
    public static void Debug(string folderName, string fileName, string message)
    {
      WriteLog(folderName, fileName, message, NLog.LogLevel.Debug.Name, false);
    }
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    /// <param name="isWriteToFolderProcess">Записывать лог в родительскую папку процесса</param>
    [Remote(IsPure=true), Public]
    public static void Debug(string fileName, string message, bool isWriteToFolderProcess)
    {
      WriteLog(string.Empty, fileName, message, NLog.LogLevel.Debug.Name, isWriteToFolderProcess);
    }
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="folderName">Имя папки</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    /// <param name="isWriteToFolderProcess">Записывать лог в родительскую папку процесса</param>
    [Remote(IsPure=true), Public]
    public static void Debug(string folderName, string fileName, string message, bool isWriteToFolderProcess)
    {
      WriteLog(folderName, fileName, message, NLog.LogLevel.Debug.Name, isWriteToFolderProcess);
    }
    
    #endregion

    #region Warn

    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    [Remote(IsPure=true), Public]
    public static void Warn(string fileName, string message)
    {
      WriteLog(string.Empty, fileName, message, NLog.LogLevel.Warn.Name, false);
    }
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="folderName">Имя папки</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    [Remote(IsPure=true), Public]
    public static void Warn(string folderName, string fileName, string message)
    {
      WriteLog(folderName, fileName, message, NLog.LogLevel.Warn.Name, false);
    }
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    /// <param name="isWriteToFolderProcess">Записывать лог в родительскую папку процесса</param>
    [Remote(IsPure=true), Public]
    public static void Warn(string fileName, string message, bool isWriteToFolderProcess)
    {
      WriteLog(string.Empty, fileName, message, NLog.LogLevel.Warn.Name, isWriteToFolderProcess);
    }
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="folderName">Имя папки</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    /// <param name="isWriteToFolderProcess">Записывать лог в родительскую папку процесса</param>
    [Remote(IsPure=true), Public]
    public static void Warn(string folderName, string fileName, string message, bool isWriteToFolderProcess)
    {
      WriteLog(folderName, fileName, message, NLog.LogLevel.Warn.Name, isWriteToFolderProcess);
    }
    
    #endregion
    
    #region Error
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    [Remote(IsPure=true), Public]
    public static void Error(string fileName, string message)
    {
      WriteLog(string.Empty, fileName, message, NLog.LogLevel.Error.Name, false);
    }

    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="folderName">Имя папки</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    [Remote(IsPure=true), Public]
    public static void Error(string folderName, string fileName, string message)
    {
      WriteLog(folderName, fileName, message, NLog.LogLevel.Error.Name, false);
    }
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    /// <param name="isWriteToFolderProcess">Записывать лог в родительскую папку процесса</param>
    [Remote(IsPure=true), Public]
    public static void Error(string fileName, string message, bool isWriteToFolderProcess)
    {
      WriteLog(string.Empty, fileName, message, NLog.LogLevel.Error.Name, isWriteToFolderProcess);
    }
    
    /// <summary>
    /// Записать сообщение в лог
    /// </summary>
    /// <param name="folderName">Имя папки</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="message">Сообщение</param>
    /// <param name="isWriteToFolderProcess">Записывать лог в родительскую папку процесса</param>
    [Remote(IsPure=true), Public]
    public static void Error(string folderName, string fileName, string message, bool isWriteToFolderProcess)
    {
      WriteLog(folderName, fileName, message, NLog.LogLevel.Error.Name, isWriteToFolderProcess);
    }
    
    #endregion
    
    #endregion
    
    
    #region NLog
    
    /// <summary>
    /// Записать лог
    /// </summary>
    /// <param name="fileName">Имя лог файла</param>
    /// <param name="message">Сообщение</param>
    /// <param name="logLevelName">Уровень логирования</param>
    /// <param name="isWriteToFolderProcess">Записывать лог в родительскую папку процесса</param>
    private static void WriteLog(string folderName, string fileName, string message, string logLevelName, bool isWriteToFolderProcess)
    {
      // Получить текущую конфигурацию логгера
      var configuration = NLog.LogManager.Configuration;
      
      //ТОЛЬКО ДЛЯ ТЕСТИРОВАНИЯ: Включить запись ошибок логирования (после записи, логирование ошибок надо отключить)
      //NLog.LogManager.ThrowExceptions = true;
      //NLog.LogManager.ThrowConfigExceptions = true;
      
      // Добавить новую цепочку записи в текущую конфигурацию
      var fileTarget = CreateTarget(configuration, folderName, fileName, isWriteToFolderProcess);
      if (fileTarget != null)
        configuration.AddTarget(fileTarget);
      
      // Добавить новое правило в текущую конфигурацию
      var loggingRule = CreateRule(fileName, fileTarget);
      if (loggingRule != null)
        configuration.LoggingRules.Insert(0, loggingRule);
      
      // Перезагрузим текущую конфигурацию
      configuration.Reload();
      
      var logger = NLog.LogManager.GetLogger(fileName);
      
      switch (logLevelName)
      {
        case "Trace":
          logger.Trace(message);
          break;
        case "Info":
          logger.Info(message);
          break;
        case "Warn":
          logger.Warn(message);
          break;
        case "Error":
          logger.Error(message);
          break;
        default:
          logger.Debug(message);
          break;
      }
      
      NLog.LogManager.Flush();
      
      //NLog.LogManager.ThrowExceptions = false;
      //NLog.LogManager.ThrowConfigExceptions = false;
    }
    
    /// <summary>
    /// Создать цепочку записи
    /// </summary>
    /// <param name="configuration">Конфигурация логгера</param>
    /// <param name="folderName">Имя папки</param>
    /// <param name="fileName">Имя лог файла</param>
    /// <param name="isWriteToFolderProcess">Записывать лог в родительскую папку процесса</param>
    /// <returns>Цепочка записи FileTarget</returns>
    private static NLog.Targets.FileTarget CreateTarget(NLog.Config.LoggingConfiguration configuration, string folderName, string fileName, bool isWriteToFolderProcess)
    {
      if (configuration == null)
        return null;
      
      var folderPatch = configuration.Variables["logs-path"].Text;
      
      if (!isWriteToFolderProcess)
      {
        var parentFolder = System.IO.Directory.GetParent(folderPatch);
        if (parentFolder != null)
          folderPatch = string.Format("{0}\\DrxCustomLogs", parentFolder.FullName);
      }
      
      if (!string.IsNullOrEmpty(folderName))
        folderPatch = string.Format("{0}\\{1}", folderPatch, folderName);
      
      var fileTarget = new NLog.Targets.FileTarget();
      fileTarget.Name = "finex-custom-logs";
      fileTarget.FileName = string.Format("{0}\\${{machinename}}.{1}.${{shortdate}}.log", folderPatch, fileName);
      fileTarget.Layout = configuration.Variables["file-layout"].Text;
      //На всякий случай оставил, чтобы была возможность собрать строку лога руками
      //fileTarget.Layout = "${odate}${assembly-version}${processid:padding=6}+${threadid:padding=-2} ${level:padding=-5}${fixed-length:inner=${logger}:maxLength=45:keepRightPart=true:padding=45} - ${ndc:separator=, :addToStart= :addToEnd=\\:}${message} ${onexception:${event-properties:item=description:WhenEmpty=Contact your system administrator}} [${event-properties:item=userName:WhenEmpty=unknown} :${event-properties:item=tenant:WhenEmpty=unknown}]${onexception:${newline}${exception:format=tostring}}";
      return fileTarget;
    }
    
    /// <summary>
    /// Создать правило записи
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="fileTarget"></param>
    /// <returns></returns>
    private static NLog.Config.LoggingRule CreateRule(string fileName, NLog.Targets.FileTarget fileTarget)
    {
      if (fileTarget == null)
        return null;
      
      var loggingRule = new NLog.Config.LoggingRule(fileName, NLog.LogLevel.Trace, NLog.LogLevel.Error, fileTarget);
      loggingRule.Final = true;
      return loggingRule;
    }
    
    #endregion
    
  }
}