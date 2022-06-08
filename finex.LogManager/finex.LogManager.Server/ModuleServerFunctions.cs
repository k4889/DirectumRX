using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.IO;

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
      
      //ТОЛЬКО ДЛЯ ТЕСТИРОВАНИЯ: 
      //Включить запись ошибок логирования (после тестирования, логирование ошибок надо отключить)
      //NLog.LogManager.ThrowExceptions = true;
      //NLog.LogManager.ThrowConfigExceptions = true;
      
      var isReconfig = false;
      
      // Добавить новую цепочку записи в текущую конфигурацию
      var fileTarget = CreateTarget(configuration, folderName, fileName, isWriteToFolderProcess);
      if (fileTarget != null)
      {
        configuration.AddTarget(fileTarget);
        isReconfig = true;
      }
      
      // Добавить новое правило в текущую конфигурацию
      var loggingRule = CreateRule(fileName, fileTarget);
      if (loggingRule != null)
      {
        configuration.LoggingRules.Insert(0, loggingRule);
        isReconfig = true;
      }
      
      // Перезагрузим текущую конфигурацию
      if (isReconfig)
        NLog.LogManager.ReconfigExistingLoggers();
      
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
      
      var targetName = string.Format("customTarget{0}", fileName);
      var fileTarget = (NLog.Targets.FileTarget)configuration.FindTargetByName(targetName);
      if (fileTarget != null)
        return null;
      
      var folderPatch = configuration.Variables["logs-path"].Text;
      
      if (!isWriteToFolderProcess && !System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
      {
        var parentFolder = Directory.GetParent(folderPatch);
        if (parentFolder != null)
          folderPatch = Path.Combine(parentFolder.FullName, "DrxCustomLogs");
      }
      
      if (!string.IsNullOrEmpty(folderName))
        folderPatch = Path.Combine(folderPatch, folderName);
      
      fileTarget = new NLog.Targets.FileTarget();
      fileTarget.Name = targetName;
      fileTarget.FileName = string.Format("{0}/${{machinename}}.{1}.${{shortdate}}.log", folderPatch, fileName);
      
      NLog.Targets.FileTarget baseTarget = null;
      var target = configuration.FindTargetByName("file");
      var wrapperTarget = target as NLog.Targets.Wrappers.WrapperTargetBase;
      if (wrapperTarget == null)
        baseTarget = target as NLog.Targets.FileTarget;
      else
        baseTarget = wrapperTarget.WrappedTarget as NLog.Targets.FileTarget;
      
      if (baseTarget != null)
        fileTarget.Layout = baseTarget.Layout;
      else
        fileTarget.Layout = "${odate}${assembly-version}${processid:padding=6}+${threadid:padding=-2} ${level:padding=-5}${fixed-length:inner=${logger}:maxLength=45:keepRightPart=true:padding=45} - ${ndc:separator=, :addToStart= :addToEnd=\\:}${message} ${onexception:${event-properties:item=description:WhenEmpty=Contact your system administrator}} [${event-properties:item=userName:WhenEmpty=unknown} :${event-properties:item=tenant:WhenEmpty=unknown}]${onexception:${newline}${exception:format=tostring}}";
      
      return fileTarget;
    }
    
    /// <summary>
    /// Создать правило записи
    /// </summary>
    /// <param name="fileName">Имя лог файла</param>
    /// <param name="fileTarget">Цепочка записи</param>
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