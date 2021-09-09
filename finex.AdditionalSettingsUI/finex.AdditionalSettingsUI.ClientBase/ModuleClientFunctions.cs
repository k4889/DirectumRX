using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.AdditionalSettingsUI.Client
{
  public class ModuleFunctions
  {
    
    /// <summary>
    /// Открыть справочник констант
    /// </summary>
    public virtual void OpenConstants()
    {
      if (Functions.Module.Remote.IsAdministrator())
      {
        var constants = EditableConstants.PublicFunctions.Module.Remote.GetConstants();
        constants.Show();
      }
      else
        Dialogs.ShowMessage("Данное действие доступно только администраторам системы.");
    }

    /// <summary>
    /// Открыть журнал отправки Email сообщений
    /// </summary>
    public virtual void OpenMailLogs()
    {
      if (Functions.Module.Remote.IsAdministrator())
      {
        //var mailLogs = Functions.Module.Remote.GetMailLogs();
        //mailLogs.Show();
      }
      else
        Dialogs.ShowMessage("Данное действие доступно только администраторам системы.");
    }

    /// <summary>
    /// Удалить блокировку объекта
    /// </summary>
    public virtual void DeleteEntityLock()
    {
      if (Functions.Module.Remote.IsAdministrator())
      {
        CreateSelectionDialog();
      }
      else
        Dialogs.ShowMessage("Данное действие доступно только администраторам системы.");
    }
    
    /// <summary>
    /// Диалог удаления блокировки объектов
    /// </summary>
    public static void CreateSelectionDialog()
    {
      var locksList = Functions.Module.Remote.GetLocksUsers();
      if (!locksList.Any())
      {
        Dialogs.ShowMessage("Блокировки в БД отсутствуют.");
        return;
      }
      
      
      var dialog = Dialogs.CreateInputDialog("Удаление блокировки");
      
      var user = dialog.AddSelect("Выберите пользователя", true, locksList.Count == 1 ? locksList.FirstOrDefault().UserName : string.Empty)
        .From(locksList.Select(l => l.UserName).Distinct().ToArray());

      var filteredEntitys = locksList.Where(l => l.UserName == user.Value);      
      var firstEntity = filteredEntitys.FirstOrDefault();     
      var entityID = dialog.AddSelect("Выберите ID объекта", true, firstEntity != null ? firstEntity.EntityID.ToString() : string.Empty)
        .From(filteredEntitys.Select(l => l.EntityID.ToString()).ToArray());
      entityID.IsEnabled = !string.IsNullOrEmpty(user.Value);
      
      var lockDate = dialog.AddString("Дата блокировки", true);
      if (firstEntity != null)
        lockDate.Value = firstEntity.LockTime.ToString();
      lockDate.IsEnabled = false;
      
      user.SetOnValueChanged((e) => {
                               entityID.Value = string.Empty;
                               if (string.IsNullOrEmpty(e.NewValue))
                                 entityID.From();
                               else
                               {
                                 var locksFiltered = locksList.Where(l => l.UserName == e.NewValue).Select(l => l.EntityID.ToString());
                                 entityID.From(locksFiltered.ToArray());
                                 entityID.Value = locksFiltered.FirstOrDefault();
                               }
                             });
      
      entityID.SetOnValueChanged((e) => {
                                   if (string.IsNullOrEmpty(e.NewValue))
                                     lockDate.Value = string.Empty;
                                   else
                                     lockDate.Value = locksList.Where(l => l.UserName == user.Value && l.EntityID.ToString() == entityID.Value).Select(l => l.LockTime).FirstOrDefault().ToString();
                                 });
      
      Action<CommonLibrary.InputDialogRefreshEventArgs> refresh = (r) =>
      {
         entityID.IsEnabled = !string.IsNullOrEmpty(user.Value);
      };
      
      dialog.SetOnRefresh(refresh);
      
      dialog.Buttons.AddOkCancel();
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        var lockEntity = locksList.Find(l => l.UserName == user.Value && l.EntityID.ToString() == entityID.Value && l.LockTime.ToString() == lockDate.Value);
        if (lockEntity != null)
          CreateConfirmDialog(lockEntity.EntityID, lockEntity.LoginID, lockEntity.TypeGuid);
        else
          Dialogs.ShowMessage("Запись с выбранными параметрами не найдена.");
      }
    }
    
    /// <summary>
    /// Диалог подтверждения удаления выбранной блокировки объекта
    /// </summary>
    /// <param name="entityId">ИД сущности</param>
    /// <param name="loginId">ИД логина пользователя</param>
    /// <param name="entityTypeGuid">Guid типа сущности</param>
    public static void CreateConfirmDialog(int entityId, int loginId, string entityTypeGuid)
    {
      var dialog = Dialogs.CreateTaskDialog("Удаление блокировки из БД.", "Подтвердите удаление блокировки из БД DirectumRX", MessageType.Information, "Подтвердите");
      dialog.Buttons.AddYesNo();
      if (dialog.Show() == DialogButtons.Yes)
      {
        var resultT = Functions.Module.Remote.DeleteLock(entityId, loginId, entityTypeGuid);
        if (resultT)
          Dialogs.ShowMessage("Блокировка успешно удалена!");
        else
          Dialogs.ShowMessage("Во время удаления блокировки возникли ошибки!\r\nСм. серверные лог файлы DirectumRx или DirectumRxWeb.");
      }
    }
  }
}