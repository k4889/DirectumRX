using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.AdditionalSettingsUI.Server
{
  public class ModuleFunctions
  {
    
    #region Удаление блокировок объектов
    
    /// <summary>
    /// Получить все блокировки из Sungero_System_Locks
    /// </summary>
    /// <returns>Список структур с данными о блокировках в системе</returns>
    [Remote]
    public static List<Structures.Module.LockData> GetLocksUsers()
    {
      var locksList = new List<Structures.Module.LockData> {};

      using (var command = Sungero.Core.SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText = Queries.Module.GetLocksUsers;
        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            var lockData = Structures.Module.LockData.Create();
            lockData.LoginID = int.Parse(reader.GetValue(0).ToString());
            lockData.UserName = reader.GetValue(1).ToString();
            lockData.EntityID = int.Parse(reader.GetValue(2).ToString());
            lockData.TypeGuid = reader.GetValue(3).ToString();
            lockData.LockTime = DateTime.Parse(reader.GetValue(4).ToString(), null, System.Globalization.DateTimeStyles.AdjustToUniversal);
            locksList.Add(lockData);
          }
        }
      }
      
      return locksList;
    }
    
    /// <summary>
    /// Удалить блокировку из Sungero_System_Locks
    /// </summary>
    [Remote]
    public static bool DeleteLock(int entityId, int loginId, string entityTypeGuid)
    {
      bool execute = true;
      
      var query = string.Format(Queries.Module.DeleteUserLock, entityId, loginId, entityTypeGuid);
      
      try
      {
        using (var command = SQL.GetCurrentConnection().CreateCommand())
        {
          command.CommandText = query;
          command.ExecuteNonQuery();
        }

      }
      catch (Exception ex)
      {
        execute = false;
        Logger.ErrorFormat("Во время удаления блокировки произошла ошибка: {0}", ex.Message);
        Logger.ErrorFormat("Query text: {0}", query);
      }

      return execute;
    }
    #endregion

    
    
    #region Работа с ТР Directum "Выполнение заданий через почту"
    /*
		/// <summary>
		/// Получить все записи журнала отправки Email сообщений
		/// </summary>
		[Remote]
		public static IQueryable<DirRX.MailAdapter.IMailLogs> GetMailLogs()
		{
			return DirRX.MailAdapter.MailLogses.GetAll();
		}
     */
    #endregion
    
    
    
    #region Общие функции
    /// <summary>
    /// Проверить является ли пользователь администратором.
    /// </summary>
    /// <returns>True, если является, иначе false.</returns>
    [Remote(IsPure = true)]
    public static bool IsAdministrator()
    {
      return Users.Current.IncludedIn(Roles.Administrators);
    }
    #endregion
    
  }
}