using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using finex.TransferRights.CaseTransferHistory;

namespace finex.TransferRights
{
  partial class CaseTransferHistoryClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      var pages = _obj.State.Pages;
      var properties = _obj.State.Properties;
      
      pages.Errors.IsVisible = _obj.Errors.Any();
      pages.Assignments.IsVisible = _obj.AssignmentsState != CaseTransferHistory.AssignmentsState.DoNotTransfer;
      pages.Tasks.IsVisible = _obj.TasksState != CaseTransferHistory.TasksState.DoNotTransfer;
      pages.Notifications.IsVisible = _obj.NotificationsState != CaseTransferHistory.NotificationsState.DoNotTransfer;
      
      properties.SelectedAssignments.IsVisible = _obj.SelectedAssignments.Any();
      properties.SelectedTasks.IsVisible = _obj.SelectedTasks.Any();
      properties.SelectedNotifications.IsVisible = _obj.SelectedNotifications.Any();
    }

  }
}