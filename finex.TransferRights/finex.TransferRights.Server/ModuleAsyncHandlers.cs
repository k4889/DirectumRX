using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;

namespace finex.TransferRights.Server
{
  public class ModuleAsyncHandlers
  {

    /// <summary>
    /// Передача задач, заданий и документов
    /// </summary>
    /// <param name="args"></param>
    public virtual void TransferRightsHandler(finex.TransferRights.Server.AsyncHandlerInvokeArgs.TransferRightsHandlerInvokeArgs args)
    {
      var caseTransferHistory = CaseTransferHistories.GetAll().Where(c => c.Id == args.CaseTransferHistoryId).FirstOrDefault();
      if (caseTransferHistory == null)
      {
        var taskText = Resources.Handler_Error_CaseTransferHistoryNotFoundFormat(args.CaseTransferHistoryId);
        finex.CollectionFunctions.PublicFunctions.Module.Remote.SendNotice(Resources.Handler_Error_EmptySubject, taskText);
        return;
      }
      
      //Если статус записи - Выполнено
      if (caseTransferHistory.Status == TransferRights.CaseTransferHistory.Status.Done)
        return;

      //Если привысили количество итераций
      var subject = Resources.Handler_Error_SubjectFormat(caseTransferHistory.UserFrom, caseTransferHistory.UserTo);
      if (args.RetryIteration > 24)
      {
        var taskText = Resources.Handler_Error_RetryLimitFormat(args.RetryIteration);
        finex.CollectionFunctions.PublicFunctions.Module.Remote.SendNotice(subject, taskText);
        Functions.CaseTransferHistory.WriteError(caseTransferHistory, taskText, true);
        return;
      }
      
      //Если не удалось заблокировать запись История передачи дел
      if (!Locks.TryLock(caseTransferHistory))
      {
        var taskText = Resources.Handler_Error_NotTryLockFormat(caseTransferHistory.Name, caseTransferHistory.Id);
        finex.CollectionFunctions.PublicFunctions.Module.Remote.SendNotice(subject, taskText, caseTransferHistory);
        args.Retry = true;
        return;
      }
      
      //Смена статуса записи
      if (caseTransferHistory.Status != TransferRights.CaseTransferHistory.Status.Active)
      {
        caseTransferHistory.Status = TransferRights.CaseTransferHistory.Status.Active;
        caseTransferHistory.Save();
      }
      
      var errors = new List<string>();
      var errorsSubList = new List<string>();
      
      //Передать задания другому исполнителю
      var assignementsComplite = TransferAssignements(caseTransferHistory, out errorsSubList);
      errors.AddRange(errorsSubList);
      errorsSubList.Clear();
      
      //Заменить получателя уведомлений
      errorsSubList.Clear();
      var noticesComplite = TransferNotices(caseTransferHistory, out errorsSubList);
      errors.AddRange(errorsSubList);
      
      //Заменить автора задач
      errorsSubList.Clear();
      var tasksComplite = TransferTasks(caseTransferHistory, out errorsSubList);
      errors.AddRange(errorsSubList);
      
      
      //Если во время выполнения возникли ошибки, отправим уведомление администраторам
      if (errors.Any())
      {
        args.Retry = true;
        var taskText = string.Format("Произошли ошибки при передаче дел от пользователя {0} к пользователю {1}:{2}", caseTransferHistory.UserFrom.Id, caseTransferHistory.UserTo.Id, "\r\n");
        foreach (var errorText in errors)
          taskText = string.Format("{0}{1}{2}", taskText, errorText, "\r\n");
        finex.CollectionFunctions.PublicFunctions.Module.Remote.SendNotice(subject, taskText, caseTransferHistory);
        
        Functions.CaseTransferHistory.WriteError(caseTransferHistory, taskText, false);
      }
      
      if (assignementsComplite && noticesComplite && tasksComplite)
      {
        caseTransferHistory.Status = TransferRights.CaseTransferHistory.Status.Done;
        caseTransferHistory.Save();

        var message = Resources.Handler_Complite_SubjectFormat(caseTransferHistory.UserFrom, caseTransferHistory.UserTo);
        finex.CollectionFunctions.PublicFunctions.Module.Remote.SendNotice(message, message, caseTransferHistory.UserFrom, caseTransferHistory);
        finex.CollectionFunctions.PublicFunctions.Module.Remote.SendNotice(message, message, caseTransferHistory.UserTo, caseTransferHistory);
      }
      else
        caseTransferHistory.Save();
      
      Locks.Unlock(caseTransferHistory);
    }
    
    
    
    #region Работа с заданиями
    
    /// <summary>
    /// Передать задания другому исполнителю
    /// </summary>
    /// <param name="caseTransferHistory">Запись справочника "История передачи дел"</param>
    /// <returns>True - если успешно, иначе False</returns>
    public static bool TransferAssignements(ICaseTransferHistory caseTransferHistory, out List<string> errors)
    {
      //Список ошибок
      errors = new List<string>();
      var errorsSubList = new List<string>();
      var complite = true;
      
      if (caseTransferHistory.AssignmentsState == TransferRights.CaseTransferHistory.AssignmentsState.DoNotTransfer)
        return complite;
      
      Enumeration? assignmentsStatus = null;
      if (caseTransferHistory.AssignmentsState == TransferRights.CaseTransferHistory.AssignmentsState.InWork)
        assignmentsStatus = Sungero.Workflow.Assignment.Status.InProcess;

      var assignmentsIds = caseTransferHistory.SelectedAssignments.Select(a => a.EntityId.Value).ToList();
      
      //Получить все задания исполнителя с учетом фильтрации
      var assignments = Assignments.GetAll()
        .Where(a => Equals(a.Performer, caseTransferHistory.UserFrom))
        .Where(a => (caseTransferHistory.DateFrom.HasValue && a.Created >= caseTransferHistory.DateFrom) || !caseTransferHistory.DateFrom.HasValue)
        .Where(a => (caseTransferHistory.DateTo.HasValue && a.Created <= caseTransferHistory.DateTo) || !caseTransferHistory.DateTo.HasValue)
        .Where(a => (assignmentsIds.Any() && assignmentsIds.Contains(a.Id)) || !assignmentsIds.Any())
        .Where(a => (assignmentsStatus.HasValue && a.Status == assignmentsStatus) || !assignmentsStatus.HasValue);
      
      if (!assignments.Any())
        return complite;
      
      var collectionName = Constants.CaseTransferHistory.HistoryCollection.Assignements;
      var assignementType = TransferRights.CaseTransferHistoryHistoryTransferAssignments.Entity.Assignment;
      var attachmentType = TransferRights.CaseTransferHistoryHistoryTransferAssignments.Entity.Attachment;
      var task = Tasks.Null;
      
      //Обработка заданий с группировкой по задачам
      foreach (IGrouping<ITask, IAssignment> assignmentGroup in assignments.GroupBy(a => a.Task))
      {
        if (task == null || !Equals(task, assignmentGroup.Key))
        {
          //Передать права на вложения задачи
          if (TransferRightsFromAttachmentsTask(caseTransferHistory, assignmentGroup.Key, collectionName, attachmentType, out errorsSubList))
          {
            errors.AddRange(errorsSubList);
            errorsSubList.Clear();

            if (ReplaceParametesTask(caseTransferHistory, assignmentGroup.Key, out errorsSubList))
              task = assignmentGroup.Key;
            else
              task = Tasks.Null;
            
            errors.AddRange(errorsSubList);
            errorsSubList.Clear();
          }
          else
          {
            task = Tasks.Null;
            errors.AddRange(errorsSubList);
            errorsSubList.Clear();
            complite = false;
          }
        }
        
        if (task == null)
          continue;
        
        foreach (var assignment in assignmentGroup)
        {
          if (TransferRightsFromEntity(caseTransferHistory.UserFrom, caseTransferHistory.UserTo, assignment, out errorsSubList))
          {
            assignment.Performer = caseTransferHistory.UserTo;
            assignment.Save();
            
            Functions.CaseTransferHistory.AddHistoryRecord(caseTransferHistory, collectionName, assignementType, assignment.Id, assignment.Subject, assignment.Task.Id);
          }
          else
          {
            errors.AddRange(errorsSubList);
            errorsSubList.Clear();
            complite = false;
          }
        }
      }
      
      return complite;
    }

    #endregion
    
    
    
    #region Работа с уведомлениями
    
    /// <summary>
    /// Передать уведомления другому исполнителю
    /// </summary>
    /// <param name="caseTransferHistory">Запись справочника "История передачи дел"</param>
    /// <returns>True - если успешно, иначе False</returns>
    public static bool TransferNotices(ICaseTransferHistory caseTransferHistory, out List<string> errors)
    {
      //Список ошибок
      errors = new List<string>();
      var errorsSubList = new List<string>();
      var complite = true;
      
      if (caseTransferHistory.NotificationsState == TransferRights.CaseTransferHistory.NotificationsState.DoNotTransfer)
        return complite;
      
      Enumeration? taskStatus = null;
      if (caseTransferHistory.NotificationsState == TransferRights.CaseTransferHistory.NotificationsState.InWork)
        taskStatus = Sungero.Workflow.Task.Status.InProcess;

      var noticesIds = caseTransferHistory.SelectedNotifications.Select(a => a.EntityId.Value).ToList();

      //Получить все уведомления получателя с учетом фильтрации
      var notices = Notices.GetAll()
        .Where(a => Equals(a.Performer, caseTransferHistory.UserFrom))
        .Where(a => (caseTransferHistory.DateFrom.HasValue && a.Created >= caseTransferHistory.DateFrom) || !caseTransferHistory.DateFrom.HasValue)
        .Where(a => (caseTransferHistory.DateTo.HasValue && a.Created <= caseTransferHistory.DateTo) || !caseTransferHistory.DateTo.HasValue)
        .Where(a => (noticesIds.Any() && noticesIds.Contains(a.Id)) || !noticesIds.Any())
        .Where(a => (taskStatus.HasValue && a.Task.Status == taskStatus) || !taskStatus.HasValue)
        .AsEnumerable()
        .Where(a => !a.Task.Attachments.Any(c => CaseTransferHistories.Is(c)));
      
      if (!notices.Any())
        return complite;
      
      var collectionName = Constants.CaseTransferHistory.HistoryCollection.Notifications;
      var notificationType = TransferRights.CaseTransferHistoryHistoryTransferNotifications.Entity.Notice;
      var attachmentType = TransferRights.CaseTransferHistoryHistoryTransferNotifications.Entity.Attachment;
      var task = Tasks.Null;
      
      //Обработка уведомлений с группировкой по задачам
      foreach (IGrouping<ITask, INotice> noticesGroup in notices.GroupBy(a => a.Task))
      {
        if (task == null || !Equals(task, noticesGroup.Key))
        {
          //Передать права на вложения задачи
          if (TransferRightsFromAttachmentsTask(caseTransferHistory, noticesGroup.Key, collectionName, attachmentType, out errorsSubList))
          {
            errors.AddRange(errorsSubList);
            errorsSubList.Clear();
            
            if (ReplaceParametesTask(caseTransferHistory, noticesGroup.Key, out errorsSubList))
              task = noticesGroup.Key;
            else
              task = Tasks.Null;
          }
          else
          {
            task = Tasks.Null;
            errors.AddRange(errorsSubList);
            errorsSubList.Clear();
            complite = false;
          }
        }
        
        if (task == null)
          continue;
        
        foreach (var notice in noticesGroup)
        {
          if (TransferRightsFromEntity(caseTransferHistory.UserFrom, caseTransferHistory.UserTo, notice, out errorsSubList))
          {
            notice.Performer = caseTransferHistory.UserTo;
            notice.Save();
            
            Functions.CaseTransferHistory.AddHistoryRecord(caseTransferHistory, collectionName, notificationType, notice.Id, notice.Subject, notice.Task.Id);
          }
          else
          {
            errors.AddRange(errorsSubList);
            errorsSubList.Clear();
            complite = false;
          }
        }
      }
      
      return complite;
    }

    #endregion
    
    
    
    #region Работа с задачами

    /// <summary>
    /// Передать задачи другому автору
    /// </summary>
    /// <param name="caseTransferHistory">Запись справочника "История передачи дел"</param>
    /// <returns>True - если успешно, иначе False</returns>
    public static bool TransferTasks(ICaseTransferHistory caseTransferHistory, out List<string> errors)
    {
      //Список ошибок
      errors = new List<string>();
      var errorsSubList = new List<string>();
      var complite = true;
      
      if (caseTransferHistory.TasksState == TransferRights.CaseTransferHistory.TasksState.DoNotTransfer)
        return complite;
      
      Enumeration? taskStatus = null;
      if (caseTransferHistory.TasksState == TransferRights.CaseTransferHistory.TasksState.InWork)
        taskStatus = Sungero.Workflow.Task.Status.InProcess;

      var tasksIds = caseTransferHistory.SelectedTasks.Select(a => a.EntityId.Value).ToList();

      //Получить все задачи автора с учетом фильтрации
      var tasks = Tasks.GetAll()
        .Where(a => Equals(a.Author, caseTransferHistory.UserFrom) || a.Observers.Any(o => Equals(o.Observer, caseTransferHistory.UserFrom)))
        .Where(a => (caseTransferHistory.DateFrom.HasValue && a.Created >= caseTransferHistory.DateFrom) || !caseTransferHistory.DateFrom.HasValue)
        .Where(a => (caseTransferHistory.DateTo.HasValue && a.Created <= caseTransferHistory.DateTo) || !caseTransferHistory.DateTo.HasValue)
        .Where(a => (tasksIds.Any() && tasksIds.Contains(a.Id)) || !tasksIds.Any())
        .Where(a => (taskStatus.HasValue && a.Status == taskStatus) || !taskStatus.HasValue);
      
      if (!tasks.Any())
        return complite;
      
      var collectionName = Constants.CaseTransferHistory.HistoryCollection.Tasks;
      var taskType = TransferRights.CaseTransferHistoryHistoryTransferTasks.Entity.Task;
      var attachmentType = TransferRights.CaseTransferHistoryHistoryTransferTasks.Entity.Attachment;
      
      //Обработка задач
      foreach (var task in tasks)
      {
        //Передать права на вложения задачи
        if (!TransferRightsFromAttachmentsTask(caseTransferHistory, task, collectionName, attachmentType, out errorsSubList))
        {
          errors.AddRange(errorsSubList);
          errorsSubList.Clear();
          continue;
        }
        
        errors.AddRange(errorsSubList);
        errorsSubList.Clear();
        
        if (TransferRightsFromEntity(caseTransferHistory.UserFrom, caseTransferHistory.UserTo, task, out errorsSubList))
        {
          if (Equals(task.Author, caseTransferHistory.UserFrom))
            task.Author = caseTransferHistory.UserTo;
          
          var observers = task.Observers.Where(o => Equals(o.Observer, caseTransferHistory.UserFrom));
          foreach (var observer in observers)
            observer.Observer = caseTransferHistory.UserTo;
          
          if (task.State.IsChanged)
          {
            task.Save();
            Functions.CaseTransferHistory.AddHistoryRecord(caseTransferHistory, collectionName, taskType, task.Id, task.Subject, 0);
          }
        }
        else
        {
          errors.AddRange(errorsSubList);
          errorsSubList.Clear();
          complite = false;
        }
      }
      
      return complite;
    }
    
    /// <summary>
    /// Передать права на вложения задачи
    /// </summary>
    /// <param name="caseTransferHistory">Запись справочника "История передачи дел"</param>
    /// <param name="task">Задача</param>
    /// <param name="historyName">Имя коллекции</param>
    /// <param name="type">Тип объекта</param>
    /// <returns>True - если успешно, иначе False</returns>
    public static bool TransferRightsFromAttachmentsTask(ICaseTransferHistory caseTransferHistory,
                                                         ITask task,
                                                         string historyName,
                                                         Enumeration type,
                                                         out List<string> errors)
    {
      //Список ошибок
      errors = new List<string>();
      
      var isLocked = false;
      
      foreach (var attachment in task.Attachments.Where(a => !CaseTransferHistories.Is(a)))
      {
        if (TransferRightsFromEntity(caseTransferHistory.UserFrom, caseTransferHistory.UserTo, attachment, out errors))
          Functions.CaseTransferHistory.AddHistoryRecord(caseTransferHistory, historyName, type, attachment.Id, attachment.Info.LocalizedName, task.Id);
        else
        {
          isLocked = true;
          break;
        }
      }
      
      return !isLocked;
    }    
    
    /// <summary>
    /// Заменить параметры в задаче
    /// </summary>
    /// <param name="caseTransferHistory">Запись справочника "История передачи дел"</param>
    /// <param name="task">Задача</param>
    /// <returns>True - если успешно, иначе False</returns>
    public static bool ReplaceParametesTask(ICaseTransferHistory caseTransferHistory,
                                            ITask task,
                                            out List<string> errors)
    {
      //Список ошибок
      errors = new List<string>();

      var taskLocks = Locks.GetLockInfo(task);
      if (taskLocks == null)
      {
        var message = Resources.Handler_Error_LocksNullFormat(task.Info.LocalizedName, task.Id);
        errors.Add(message);
        Logger.ErrorFormat("TransferRights (ReplaceParametesTask): {0}", message);
        return true;
      }
      
      if (taskLocks.IsLockedByOther)
      {
        var message = Resources.Handler_Error_LockedFormat(task.Info.LocalizedName, task.Id, taskLocks.LockedMessage);
        errors.Add(message);
        Logger.ErrorFormat("TransferRights (ReplaceParametesTask): {0}", message);
        return false;
      }
      
      try
      {
        var taskProperties = task.GetType().GetProperties()
          .Where(p => p.Name != "Author")
          .Where(p => p.CanRead && p.CanWrite);
        
        foreach (System.Reflection.PropertyInfo propertyInfo in taskProperties)
        {
          dynamic propertyValue = propertyInfo.GetValue(task);
          if (propertyValue == null)
            continue;
          
          //Если коллекция
          if (propertyInfo.PropertyType.AssemblyQualifiedName.StartsWith("Sungero.Domain.Shared.IChildEntityCollection"))
          {
            foreach (dynamic line in propertyValue)
            {
              //Перебираем свойства записи коллекции
              Type lineType = line.GetType();
              var lineProperties = lineType.GetProperties().Where(p => p.CanRead && p.CanWrite);
              foreach (System.Reflection.PropertyInfo linePropertyInfo in lineProperties)
              {
                dynamic linePropertyValue = linePropertyInfo.GetValue(line);
                if (linePropertyValue == null)
                  continue;
                
                if (Equals(linePropertyValue, caseTransferHistory.UserFrom))
                  linePropertyInfo.SetValue(line, caseTransferHistory.UserTo);
              }
            }
          }
          else
          {
            if (Equals(propertyValue, caseTransferHistory.UserFrom))
              propertyInfo.SetValue(task, caseTransferHistory.UserTo);
          }
        }
      }
      catch (Exception ex)
      {
        errors.Add(ex.Message);
        Logger.ErrorFormat("TransferRights (ReplaceParametesTask): Возникла ошибка при замене параметров в задаче {1} (ИД {0})\nОшибка: {2}", task.Id, task.Info.LocalizedName, ex.Message);
        return false;
      }
      
      return true;
    }
    
    #endregion
    
    
    
    #region Работа с правами
    
    /// <summary>
    /// Передать права на объект
    /// </summary>
    /// <param name="userFrom">От кого</param>
    /// <param name="userTo">Кому</param>
    /// <param name="entity">Объект</param>
    /// <returns>True - если успешно, иначе False</returns>
    public static bool TransferRightsFromEntity(IUser userFrom,
                                                IUser userTo,
                                                Sungero.Domain.Shared.IEntity entity,
                                                out List<string> errors)
    {
      //Список ошибок
      errors = new List<string>();

      var entityLocks = Locks.GetLockInfo(entity);
      if (entityLocks == null)
      {
        var message = Resources.Handler_Error_LocksNullFormat(entity.Info.LocalizedName, entity.Id);
        errors.Add(message);
        Logger.ErrorFormat("TransferRights (TransferRightsFromEntity): {0}", message);
        return true;
      }
      
      if (entityLocks.IsLockedByOther)
      {
        var message = Resources.Handler_Error_LockedFormat(entity.Info.LocalizedName, entity.Id, entityLocks.LockedMessage);
        errors.Add(message);
        Logger.ErrorFormat("TransferRights (TransferRightsFromEntity): {0}", message);
        return false;
      }
      
      if (entity.AccessRights == null)
        return true;
      
      try
      {
        if (!entity.AccessRights.Current.Any())
          return true;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("TransferRights (TransferRightsFromEntity): Не удалось получить список текущих прав объекта {1} (ИД {0})\nОшибка: {2}", entity.Id, entity.Info.LocalizedName, ex.Message);
        return true;
      }
      
      var accessRights = entity.AccessRights.Current
        .Where(r => r.Recipient != null)
        .Where(r => r.Recipient.Equals(userFrom));
      
      if (!accessRights.Any())
        return true;

      var isGrant = false;
      foreach (var accessRight in accessRights)
      {
        if (Users.Is(accessRight.Recipient) || Sungero.Company.Employees.Is(accessRight.Recipient))
          entity.AccessRights.Revoke(userFrom, accessRight.AccessRightsType);
        
        if (!entity.AccessRights.IsGrantedDirectly(accessRight.AccessRightsType, userTo))
        {
          entity.AccessRights.Grant(userTo, accessRight.AccessRightsType);
          isGrant = true;
        }
      }
      
      if (!isGrant)
        return true;
      
      try
      {
        entity.AccessRights.Save();

        var operation = new Enumeration("Manage");
        var comment = Resources.History_TransferRightsCommentFormat(userFrom.Name, userTo.Name);
        
        entity.History.Write(operation, operation, comment);
      }
      catch (Exception ex)
      {
        errors.Add(ex.Message);
        Logger.ErrorFormat("TransferRights (TransferRightsFromEntity): Ошибка передачи прав на объект {0} (ИД {1}) от {2} к {3}\nОшибка: {4}", entity.Info.LocalizedName, entity.Id, userFrom.Id, userTo.Id, ex.Message);
        return false;
      }
      
      return true;
    }
    
    #endregion
    
  }
}