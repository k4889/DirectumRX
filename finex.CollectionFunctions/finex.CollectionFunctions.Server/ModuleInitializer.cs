using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;
using System.Text;
using Sungero.Company;

namespace finex.CollectionFunctions.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      
    }
    
    
    
    #region Создание ролей
    
    /// <summary>
    /// Создать роль.
    /// </summary>
    /// <param name="roleName">Название роли.</param>
    /// <param name="roleDescription">Описание роли.</param>
    /// <param name="roleGuid">Guid роли. Игнорирует имя.</param>
    /// <param name="isSingleUser">Роль с одним участником.</param>
    /// <returns>Новая роль.</returns>
    [Public]
    public static IRole CreateRole(string roleName, string roleDescription, Guid roleGuid, bool isSingleUser)
    {
      var role = Roles.GetAll(r => r.Sid == roleGuid).FirstOrDefault();
      
      if (role == null)
      {
        InitializationLogger.DebugFormat("Init: Create Role {0}", roleName);
        
        role = Roles.Create();
        role.Name = roleName;
        role.Description = roleDescription;
        role.Sid = roleGuid;
        role.IsSystem = true;
        role.IsSingleUser = isSingleUser;
        if (isSingleUser)
        {
          var newLink = role.RecipientLinks.AddNew();
          newLink.Member = Users.Current;
        }
        
        role.Save();
      }
      else
      {
        var message = new StringBuilder();
        
        if (role.Name != roleName)
        {
          message.AppendLine(string.Format("Role \"{0}\" (Sid = {1}) renamed as \"{2}\"", role.Name, role.Sid, roleName));
          role.Name = roleName;
        }
        
        if (role.Description != roleDescription)
        {
          message.AppendLine(string.Format("Role \"{0}\" (Sid = {1}) update Description \"{2}\"", role.Name, role.Sid, roleDescription));
          role.Description = roleDescription;
        }
        
        if (isSingleUser && role.IsSingleUser != isSingleUser)
        {
          message.AppendLine(string.Format("Role \"{0}\" (IsSingleUser = {1}) update as \"{2}\"", role.Name, role.IsSingleUser == true ? "Yes" : "No" , isSingleUser == true ? "Yes" : "No"));
          role.IsSingleUser = isSingleUser;
          
          if (role.RecipientLinks.Any())
            role.RecipientLinks.Clear();
          
          var newLink = role.RecipientLinks.AddNew();
          newLink.Member = Users.Current;
        }
        
        if (role.State.IsChanged)
        {
          role.Save();
          InitializationLogger.DebugFormat("{0}", string.Join("\r\n", message));
        }
      }
      return role;
    }
    
    #endregion
    
    
    
    #region Базы данных
    
    /// <summary>
    /// Создать таблицу в БД.
    /// </summary>
    /// <param name="sourceTableName">Имя таблицы в БД.</param>
    /// <param name="query">Insert запрос.</param>
    [Public]
    public static void CreateTable(string sourceTableName, string query)
    {
      CreateTable(sourceTableName, query, true);
    }
    
    /// <summary>
    /// Создать таблицу в БД.
    /// </summary>
    /// <param name="sourceTableName">Имя таблицы в БД.</param>
    /// <param name="query">Insert запрос.</param>
    /// <param name="isDrop">Удалить таблицу перед соданием, если она есть.</param>
    [Public]
    public static void CreateTable(string sourceTableName, string query, bool isDrop)
    {
      if (string.IsNullOrEmpty(sourceTableName) || string.IsNullOrEmpty(query))
        return;
      
      bool isCreate = false;
      
      if (isDrop)
      {
        Sungero.Docflow.PublicFunctions.Module.DropReportTempTable(sourceTableName);
        isCreate = true;
      }
      else
      {
        var result = ExecuteScalarSQLCommand(string.Format(Queries.Module.CheckTable, sourceTableName));
        
        if (result == "0")
          isCreate = true;
      }
      
      if (isCreate)
        Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(query, new[] { sourceTableName });
    }
    
    /// <summary>
    /// Выполнить SQL-запрос.
    /// </summary>
    /// <param name="commandText">Форматируемая строка запроса.</param>
    /// <returns>Возвращает первый столбец первой строки.</returns>
    [Public]
    public static string ExecuteScalarSQLCommand(string commandText)
    {
      string result = string.Empty;
      
      using (var command = Sungero.Core.SQL.GetCurrentConnection().CreateCommand())
      {
        command.CommandText = commandText;
        var executionResult = command.ExecuteScalar();
        if (!(executionResult is DBNull) && executionResult != null)
          result = executionResult.ToString();
      }
      
      return result;
    }
    
    /// <summary>
    /// Создать представление в БД.
    /// </summary>
    /// <param name="sourceTableName">Имя предствления в БД.</param>
    /// <param name="query">Insert запрос.</param>
    [Public]
    public static void CreateView(string sourceViewName, string query)
    {
      if (string.IsNullOrEmpty(sourceViewName) || string.IsNullOrEmpty(query))
        return;

      DropView(sourceViewName);
      Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(query, new[] { sourceViewName });
    }
    
    /// <summary>
    /// Удалить представление в БД.
    /// </summary>
    /// <param name="tableName">Название предствления.</param>
    /// <remarks>Для выполнения создает свой конект к БД.</remarks>
    [Public]
    public static void DropView(string viewName)
    {
      if (string.IsNullOrEmpty(viewName))
        return;
      
      var queryText = string.Format(Queries.Module.DropView, viewName);
      Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommand(queryText);
    }
    
    #endregion
    
    
    
    #region Создание записей справочников
    
    /// <summary>
    /// Создать НОР.
    /// </summary>
    /// <param name="name">Название НОР.</param>
    /// <param name="guid">Guid НОР. Игнорирует имя.</param>
    /// <param name="code">Код.</param>
    /// <param name="tin">ИНН.</param>
    /// <param name="trrc">КПП.</param>
    /// <param name="description">Примечание.</param>
    /// <returns>Новая НОР.</returns>
    [Public]
    public static IBusinessUnit CreateBusinessUnit(string name, Guid guid, string code, string tin, string trrc, string description)
    {
      var businessUnit = BusinessUnits.GetAll(b => b.Sid == guid).FirstOrDefault();

      if (businessUnit == null)
      {
        InitializationLogger.DebugFormat("Init: Create BusinessUnit {0}", name);
        
        businessUnit = BusinessUnits.Create();
        businessUnit.Name = name;
        businessUnit.Code = code;
        businessUnit.TIN = tin;
        businessUnit.TRRC = trrc;
        businessUnit.Description = description;
        businessUnit.Sid = guid;
        businessUnit.Save();
      }
      else
      {
        var change = false;
        var message = new StringBuilder();
        
        if (businessUnit.Name != name)
        {
          message.AppendLine(string.Format("BusinessUnit \"{0}\" (Sid = {1}) renamed as \"{2}\"", businessUnit.Name, businessUnit.Sid, name));
          businessUnit.Name = name;
          change = true;
        }
        
        if (businessUnit.Code != code)
        {
          message.AppendLine(string.Format("BusinessUnit \"{0}\" (Sid = {1}) update Code \"{2}\"", businessUnit.Name, businessUnit.Sid, code));
          businessUnit.Code = code;
          change = true;
        }
        
        if (businessUnit.TIN != tin)
        {
          message.AppendLine(string.Format("BusinessUnit \"{0}\" (Sid = {1}) update TIN \"{2}\"", businessUnit.Name, businessUnit.Sid, tin));
          businessUnit.TIN = tin;
          change = true;
        }
        
        if (businessUnit.TRRC != trrc)
        {
          message.AppendLine(string.Format("BusinessUnit \"{0}\" (Sid = {1}) update TRRC \"{2}\"", businessUnit.Name, businessUnit.Sid, trrc));
          businessUnit.TRRC = trrc;
          change = true;
        }
        
        if (businessUnit.Description != description)
        {
          message.AppendLine(string.Format("BusinessUnit \"{0}\" (Sid = {1}) update Description \"{2}\"", businessUnit.Name, businessUnit.Sid, description));
          businessUnit.Description = description;
          change = true;
        }
        
        if (change)
        {
          businessUnit.Save();
          InitializationLogger.DebugFormat("{0}", string.Join("\r\n", message));
        }
      }
      
      return businessUnit;
    }

    /// <summary>
    /// Создать должность.
    /// </summary>
    /// <param name="name">Наименование должности.</param>
    /// <returns>Новая должность.</returns>
    [Public]
    public static IJobTitle CreateJobTitle(string name)
    {
      var JobTitle = JobTitles.GetAll(j => j.Name == name).FirstOrDefault();
      
      if (JobTitle == null)
      {
        InitializationLogger.DebugFormat("Init: Create JobTitle {0}", name);
        JobTitle = JobTitles.Create();
        JobTitle.Name = name;
        JobTitle.Save();
      }
      
      return JobTitle;
    }
    
    /// <summary>
    /// Создать подразделение.
    /// </summary>
    /// <param name="name">Название подразделения.</param>
    /// <param name="guid">Guid подразделения. Игнорирует имя.</param>
    /// <param name="businessUnit">НОР.</param>
    /// <param name="code">Код.</param>
    /// <param name="description">Примечание.</param>
    /// <returns>Новая НОР.</returns>
    [Public]
    public static IDepartment CreateDepartment(string name, Guid guid, IBusinessUnit businessUnit, string code, string description)
    {
      var department = Departments.GetAll(d => d.Sid == guid).FirstOrDefault();

      if (department == null)
      {
        InitializationLogger.DebugFormat("Init: Create Department {0}", name);
        
        department = Departments.Create();
        department.Name = name;
        department.Code = code;
        department.BusinessUnit = businessUnit;
        department.Description = description;
        department.Sid = guid;
        department.Save();
      }
      else
      {
        var change = false;
        var message = new StringBuilder();
        
        if (department.Name != name)
        {
          message.AppendLine(string.Format("Department \"{0}\" (Sid = {1}) renamed as \"{2}\"", department.Name, department.Sid, name));
          department.Name = name;
          change = true;
        }
        
        if (businessUnit != null && !Equals(department.BusinessUnit, businessUnit))
        {
          message.AppendLine(string.Format("Department \"{0}\" (Sid = {1}) update BusinessUnit \"{2}\"", department.Name, department.Sid, businessUnit.Name));
          department.BusinessUnit = businessUnit;
          change = true;
        }
        
        if (department.Code != code)
        {
          message.AppendLine(string.Format("Department \"{0}\" (Sid = {1}) update Code \"{2}\"", department.Name, department.Sid, code));
          department.Code = code;
          change = true;
        }
        
        if (department.Description != description)
        {
          message.AppendLine(string.Format("Department \"{0}\" (Sid = {1}) update Description \"{2}\"", department.Name, department.Sid, description));
          department.Description = description;
          change = true;
        }
        
        if (change)
        {
          department.Save();
          InitializationLogger.DebugFormat("{0}", string.Join("\r\n", message));
        }
      }
      
      return department;
    }
    
    #endregion
    
  }
}
