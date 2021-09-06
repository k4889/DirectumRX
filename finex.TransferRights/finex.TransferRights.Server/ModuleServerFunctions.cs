using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.TransferRights.Server
{
  public class ModuleFunctions
  {
    
    /// <summary>
    /// Получить задания в работе
    /// </summary>
    /// <param name="performer">Исполнитель</param>
    /// <returns>Список заданий</returns>
    [Remote(IsPure=true)]
    public virtual List<Sungero.Workflow.IAssignment> GetInWorkAssignments(IUser performer)
    {
      if (performer == null)
        return new List<Sungero.Workflow.IAssignment>();
      
      return Sungero.Workflow.Assignments.GetAll()
        .Where(a => Equals(a.Performer, performer))
        .Where(a => a.Status == Sungero.Workflow.Assignment.Status.InProcess)
        .ToList();
    }
    
    /// <summary>
    /// Получить увеломления в работе
    /// </summary>
    /// <param name="performer">Исполнитель</param>
    /// <returns>Список уведомлений</returns>
    [Remote(IsPure=true)]
    public virtual List<Sungero.Workflow.INotice> GetInWorkNotices(IUser performer)
    {
      if (performer == null)
        return new List<Sungero.Workflow.INotice>();
      
      return Sungero.Workflow.Notices.GetAll()
        .Where(a => Equals(a.Performer, performer))
        .Where(a => a.Task.Status == Sungero.Workflow.Task.Status.InProcess)
        .ToList();
    }
    
    /// <summary>
    /// Получить задачи в работе
    /// </summary>
    /// <param name="performer">Автор</param>
    /// <returns>Список задач</returns>
    [Remote(IsPure=true)]
    public virtual List<Sungero.Workflow.ITask> GetInWorkTask(IUser author)
    {
      if (author == null)
        return new List<Sungero.Workflow.ITask>();
      
      return Sungero.Workflow.Tasks.GetAll()
        .Where(a => Equals(a.Author, author))
        .Where(a => a.Status == Sungero.Workflow.Task.Status.InProcess)
        .ToList();
    }

  }
}