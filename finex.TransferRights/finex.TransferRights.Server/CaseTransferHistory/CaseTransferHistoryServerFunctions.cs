using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using finex.TransferRights.CaseTransferHistory;

namespace finex.TransferRights.Server
{
  partial class CaseTransferHistoryFunctions
  {
    /// <summary>
    /// Создать "Историю передачи дел"
    /// </summary>
    /// <returns>История передачи дел</returns>
    [Remote]
    public static ICaseTransferHistory Create()
    {
      return CaseTransferHistories.Create();
    }
    
    /// <summary>
    /// Отправить задачу на "Передача задач, заданий и документов"
    /// </summary>
    [Remote]
    public virtual void StartTransferRightsHandler()
    {
      var handler = AsyncHandlers.TransferRightsHandler.Create();
      handler.CaseTransferHistoryId = _obj.Id;
      handler.ExecuteAsync();
    }
    
    /// <summary>
    /// Записать ошибку
    /// </summary>
    /// <param name="error">Текст ошибки</param>
    /// <param name="changeStatus">Установить статус "Ошибка"</param>
    public virtual void WriteError(string error, bool changeStatus)
    {
      var newError = _obj.Errors.AddNew();
      newError.DateError = Calendar.Now;
      newError.ErrorMessage = error;
      
      if (changeStatus)
        _obj.Status = TransferRights.CaseTransferHistory.Status.Error;
      
      try
      {
        _obj.Save();
      }
      catch (Exception ex)
      {
        var message = CaseTransferHistories.Resources.Error_MessageFormat(_obj.Id, error, ex.Message);
        finex.CollectionFunctions.PublicFunctions.Module.Remote.SendNotice(CaseTransferHistories.Resources.Error_Subject, message);
      }
    }
    
    /// <summary>
    /// Добавить запись в историю
    /// </summary>
    /// <param name="historyName">Имя коллекции</param>
    /// <param name="type">Тип объекта</param>
    /// <param name="entityId">ИД объекта</param>
    /// <param name="entityName">Наименование объекта</param>
    /// <param name="taskId">ИД задачи</param>
    public virtual void AddHistoryRecord(string historyName, Enumeration type, int entityId, string entityName, int taskId)
    {
      switch (historyName)
      {
        case Constants.CaseTransferHistory.HistoryCollection.Assignements:
          AddRecordHistoryAssignements(type, entityId, entityName, taskId);
          break;
          
        case Constants.CaseTransferHistory.HistoryCollection.Notifications:
          AddRecordHistoryNotifications(type, entityId, entityName, taskId);
          break;
          
        case Constants.CaseTransferHistory.HistoryCollection.Tasks:
          AddRecordHistoryTasks(type, entityId, entityName, taskId);
          break;
      }
    }
    
    /// <summary>
    /// Добавить запись в историю заданий
    /// </summary>
    /// <param name="type">Тип объекта</param>
    /// <param name="entityId">ИД объекта</param>
    /// <param name="entityName">Наименование объекта</param>
    /// <param name="taskId">ИД задачи</param>
    public virtual void AddRecordHistoryAssignements(Enumeration type, int entityId, string entityName, int taskId)
    {
      var newRecord = _obj.HistoryTransferAssignments.AddNew();
      newRecord.Entity = type;
      newRecord.EntityId = entityId;
      newRecord.EntityName = entityName;
      newRecord.GeneralObjectID = taskId;
    }
    
    /// <summary>
    /// Добавить запись в историю заданий
    /// </summary>
    /// <param name="type">Тип объекта</param>
    /// <param name="entityId">ИД объекта</param>
    /// <param name="entityName">Наименование объекта</param>
    /// <param name="taskId">ИД задачи</param>
    public virtual void AddRecordHistoryNotifications(Enumeration type, int entityId, string entityName, int taskId)
    {
      var newRecord = _obj.HistoryTransferNotifications.AddNew();
      newRecord.Entity = type;
      newRecord.EntityId = entityId;
      newRecord.EntityName = entityName;
      newRecord.GeneralObjectID = taskId;
    }
    
    /// <summary>
    /// Добавить запись в историю заданий
    /// </summary>
    /// <param name="type">Тип объекта</param>
    /// <param name="entityId">ИД объекта</param>
    /// <param name="entityName">Наименование объекта</param>
    /// <param name="taskId">ИД задачи</param>
    public virtual void AddRecordHistoryTasks(Enumeration type, int entityId, string entityName, int taskId)
    {
      var newRecord = _obj.HistoryTransferTasks.AddNew();
      newRecord.Entity = type;
      newRecord.EntityId = entityId;
      newRecord.EntityName = entityName;
      if (taskId > 0)
        newRecord.GeneralObjectID = taskId;
    }
    
  }
}