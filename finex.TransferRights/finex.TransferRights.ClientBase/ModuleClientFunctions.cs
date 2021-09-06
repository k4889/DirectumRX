using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.TransferRights.Client
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Диалог передачи прав на задачи, задания и документы
    /// </summary>
    [Public]
    public virtual void TransferRightsDialog()
    {
      var isSend = false;
      var selectedAssignments = new List<Sungero.Workflow.IAssignment>();
      var selectedNotices = new List<Sungero.Workflow.INotice>();
      var selectedTasks = new List<Sungero.Workflow.ITask>();
      
      var oldUserFrom = Users.Null;
      var transferUser = Users.Current;
      if (finex.CollectionFunctions.PublicFunctions.Module.Remote.IsAdministrator())
        transferUser = Users.Null;
      
      var dialog = Dialogs.CreateInputDialog(Resources.Dialog_Title);

      #region Поля диалога
      dialog.Text = Resources.Dialog_Text;
      
      //От кого
      var userFrom = dialog.AddSelect(Resources.Dialog_From, true, transferUser);
      userFrom.IsEnabled = transferUser == null;
      //Кому
      var userTo = dialog.AddSelect(Resources.Dialog_To, true, Users.Null);
      
      //Дата с
      var dateFrom = dialog.AddDate(Resources.Dialog_DateFrom, false);
      //Дата по
      var dateTo = dialog.AddDate(Resources.Dialog_DateTo, false);
      
      //Статус заданий
      var statusAssignments = dialog.AddSelect(Resources.Dialog_Status_AssignmentsTitle, true, Resources.Dialog_Status_All)
        .From(Resources.Dialog_Status_All, Resources.Dialog_Status_InWork, Resources.Dialog_Status_NotTransfer);
      
      //Только выбранные задания
      var assignments = dialog.AddSelectMany(Resources.Dialog_Assignments, false, Sungero.Workflow.Assignments.Null).From(selectedAssignments);
      assignments.IsVisible = false;
      
      //Статус уведомлений
      var statusNotices = dialog.AddSelect(Resources.Dialog_Status_NoticeTitle, true, Resources.Dialog_Status_All)
        .From(Resources.Dialog_Status_All, Resources.Dialog_Status_InWork, Resources.Dialog_Status_NotTransfer);
      
      //Только выбранные уведомления
      var notices = dialog.AddSelectMany(Resources.Dialog_Notices, false, Sungero.Workflow.Notices.Null).From(selectedNotices);
      notices.IsVisible = false;
      
      //Статус задач
      var statusTasks = dialog.AddSelect(Resources.Dialog_Status_TaskTitle, true, Resources.Dialog_Status_All)
        .From(Resources.Dialog_Status_All, Resources.Dialog_Status_InWork, Resources.Dialog_Status_NotTransfer);
      
      //Только выбранные задачи
      var tasks = dialog.AddSelectMany(Resources.Dialog_Tasks, false, Sungero.Workflow.Tasks.Null).From(selectedTasks);
      tasks.IsVisible = false;
      #endregion

      #region Кнопки диалога
      var transferButton = dialog.Buttons.AddCustom(Resources.Dialog_Button_Transfer);
      var exitButton = dialog.Buttons.AddCustom(Resources.Dialog_Button_Exit);
      #endregion
      
      #region Обработчики свойств диалога
      userFrom.SetOnValueChanged(
        (usr) =>
        {
          if (statusAssignments.Value == Resources.Dialog_Status_InWork)
            selectedAssignments = Functions.Module.Remote.GetInWorkAssignments(usr.NewValue);
          else
            selectedAssignments.Clear();
          
          if (statusNotices.Value == Resources.Dialog_Status_InWork)
            selectedNotices = Functions.Module.Remote.GetInWorkNotices(usr.NewValue);
          else
            selectedNotices.Clear();
          
          if (statusTasks.Value == Resources.Dialog_Status_InWork)
            selectedTasks = Functions.Module.Remote.GetInWorkTask(usr.NewValue);
          else
            selectedTasks.Clear();
          
          oldUserFrom = usr.NewValue;
        });
      
      statusAssignments.SetOnValueChanged(
        (s) =>
        {
          if (s.NewValue == Resources.Dialog_Status_InWork)
            selectedAssignments = Functions.Module.Remote.GetInWorkAssignments(userFrom.Value);
          else
            selectedAssignments.Clear();
        });
      
      statusNotices.SetOnValueChanged(
        (s) =>
        {
          if (s.NewValue == Resources.Dialog_Status_InWork)
            selectedNotices = Functions.Module.Remote.GetInWorkNotices(userFrom.Value);
          else
            selectedNotices.Clear();
        });
      
      statusTasks.SetOnValueChanged(
        (s) =>
        {
          if (s.NewValue == Resources.Dialog_Status_InWork)
            selectedTasks = Functions.Module.Remote.GetInWorkTask(userFrom.Value);
          else
            selectedTasks.Clear();
        });
      #endregion

      #region Обновление диалога
      Action<CommonLibrary.InputDialogRefreshEventArgs> refresh = (r) =>
      {
        var fromAssignments = selectedAssignments;
        var fromNotices = selectedNotices;
        var fromTasks = selectedTasks;
        
        if (dateFrom.Value.HasValue)
        {
          fromAssignments = fromAssignments.Where(a => a.Created >= dateFrom.Value.Value).ToList();
          fromNotices = fromNotices.Where(n => n.Created >= dateFrom.Value.Value).ToList();
          fromTasks = fromTasks.Where(t => t.Created >= dateFrom.Value.Value).ToList();
        }
        
        if (dateTo.Value.HasValue)
        {
          var dateToEndDate = Calendar.EndOfDay(dateTo.Value.Value);
          fromAssignments = fromAssignments.Where(a => a.Created <= dateToEndDate).ToList();
          fromNotices = fromNotices.Where(n => n.Created <= dateToEndDate).ToList();
          fromTasks = fromTasks.Where(t => t.Created <= dateToEndDate).ToList();
        }
        
        assignments.IsVisible = fromAssignments.Any();
        assignments.From(fromAssignments);
        
        notices.IsVisible = fromNotices.Any();
        notices.From(fromNotices);
        
        tasks.IsVisible = fromTasks.Any();
        tasks.From(fromTasks);
        
        if (userFrom.Value != null)
        {
          if (userTo.Value != null && Equals(userFrom.Value, userTo.Value))
            r.AddError(Resources.Dialog_UsersError);
        
          if ((oldUserFrom == null || !Equals(userFrom.Value, oldUserFrom)) && Functions.CaseTransferHistory.ChangeUserFromRecords(userFrom.Value))
            r.AddError(Resources.Dialog_CaseTransferHistoryError);
        }
      };
      
      dialog.SetOnRefresh(refresh);
      #endregion

      #region	Обработчик кнопок
      dialog.SetOnButtonClick(
        (b) =>
        {
          b.CloseAfterExecute = false;
          
          #region Передать
          if (b.Button == transferButton && b.IsValid)
          {
            if (Dialogs.CreateConfirmDialog(Resources.ConfirmDialog_Title, Resources.ConfirmDialog_Description).Show())
            {
              var caseTransferHistory = Functions.CaseTransferHistory.Remote.Create();
              
              caseTransferHistory.UserFrom = userFrom.Value;
              caseTransferHistory.UserTo = userTo.Value;
              
              caseTransferHistory.DateFrom = dateFrom.Value;
              caseTransferHistory.DateTo = dateTo.Value;
              
              #region Состояния объектов
              if (statusAssignments.Value == Resources.Dialog_Status_All)
                caseTransferHistory.AssignmentsState = CaseTransferHistory.AssignmentsState.All;
              else
                if (statusAssignments.Value == Resources.Dialog_Status_InWork)
                  caseTransferHistory.AssignmentsState = CaseTransferHistory.AssignmentsState.InWork;
                else
                  if (statusAssignments.Value == Resources.Dialog_Status_NotTransfer)
                    caseTransferHistory.AssignmentsState = CaseTransferHistory.AssignmentsState.DoNotTransfer;

              if (statusNotices.Value == Resources.Dialog_Status_All)
                caseTransferHistory.NotificationsState = CaseTransferHistory.NotificationsState.All;
              else
                if (statusNotices.Value == Resources.Dialog_Status_InWork)
                  caseTransferHistory.NotificationsState = CaseTransferHistory.NotificationsState.InWork;
                else
                  if (statusNotices.Value == Resources.Dialog_Status_NotTransfer)
                    caseTransferHistory.NotificationsState = CaseTransferHistory.NotificationsState.DoNotTransfer;

              if (statusTasks.Value == Resources.Dialog_Status_All)
                caseTransferHistory.TasksState = CaseTransferHistory.TasksState.All;
              else
                if (statusTasks.Value == Resources.Dialog_Status_InWork)
                  caseTransferHistory.TasksState = CaseTransferHistory.TasksState.InWork;
                else
                  if (statusTasks.Value == Resources.Dialog_Status_NotTransfer)
                    caseTransferHistory.TasksState = CaseTransferHistory.TasksState.DoNotTransfer;
              #endregion
              
              #region Выбранные записи
              if (assignments.Value.Any() && statusAssignments.Value == Resources.Dialog_Status_InWork)
              {
                foreach (var entity in assignments.Value)
                {
                  var record = caseTransferHistory.SelectedAssignments.AddNew();
                  record.EntityId = entity.Id;
                  record.Name = entity.Subject;
                }
              }
              
              if (notices.Value.Any() && statusNotices.Value == Resources.Dialog_Status_InWork)
              {
                foreach (var entity in notices.Value)
                {
                  var record = caseTransferHistory.SelectedNotifications.AddNew();
                  record.EntityId = entity.Id;
                  record.Name = entity.Subject;
                }
              }

              if (tasks.Value.Any() && statusTasks.Value == Resources.Dialog_Status_InWork)
              {
                foreach (var entity in tasks.Value)
                {
                  var record = caseTransferHistory.SelectedTasks.AddNew();
                  record.EntityId = entity.Id;
                  record.Name = entity.Subject;
                }
              }
              #endregion

              caseTransferHistory.Save();
              
              Functions.CaseTransferHistory.Remote.StartTransferRightsHandler(caseTransferHistory);
              isSend = true;
              
              b.CloseAfterExecute = true;
            }
          }
          #endregion

          #region	Выход
          if (b.Button == exitButton)
          {
            b.CloseAfterExecute = true;
            userFrom.IsRequired = false;
            userTo.IsRequired = false;
            statusAssignments.IsRequired = false;
            statusTasks.IsRequired = false;
          }
          #endregion
          
        });
      #endregion
      
      dialog.Show();
      
      if (isSend)
        Dialogs.ShowMessage(Resources.Dialog_CompliteMessage);
    }
    
  }
}