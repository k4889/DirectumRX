using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.UsingDirectoryEntry
{
  partial class UsingRecordingServerHandlers
  {

    public override void AfterExecute(Sungero.Reporting.Server.AfterExecuteEventArgs e)
    {
      // Очистить временную таблицу.
      Sungero.Docflow.PublicFunctions.Module.DeleteReportData(UsingRecording.TempReportTableName, UsingRecording.ReportSessionId);
    }

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      var tempReportTableName = Constants.Module.temporaryTableName;
      
      // Имя временной таблицы отчета в БД
      UsingRecording.TempReportTableName = tempReportTableName;

      // Т.к. имя в БД может быть полным, возьмем только имя колонки
      var entityTableName = UsingRecording.TableName;
      if (!string.IsNullOrEmpty(entityTableName))
        UsingRecording.TableName = entityTableName.Split('_').LastOrDefault();

      // Получим имена всех таблиц в БД
      var tablesNames = new List<string>();
      using (var command = SQL.GetCurrentConnection().CreateCommand())
      {
        var commandText = Queries.UsingRecording.SelectTableNames;
        command.CommandText = commandText;

        try
        {
          var reader = command.ExecuteReader();
          while (reader.Read())
            tablesNames.Add(string.Format("{0}", reader.GetValue(0).ToString()));
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("Возникла ошибка в запросе имен таблиц: {0}", commandText);
          Logger.ErrorFormat("Ошибка: {0}", ex.Message);
        }
      }
      
      var additionalColumnName = string.Empty;
      if (UsingRecording.AdditionalColumnName.Any())
      {
        foreach (var paramsValue in UsingRecording.AdditionalColumnName)
          additionalColumnName = string.Format("{0} OR column_name like '{1}%'", additionalColumnName, paramsValue.ToLower());
      }
      
      // Перебераем все полученные таблицы
      foreach (var tableName in tablesNames)
      {
        var columnsNames = new List<KeyValuePair<string, string>>();
        
        // Получим имена полей в таблице, которые соответствуют текущей сущности (Документы, Справочники)
        using (var command = SQL.GetCurrentConnection().CreateCommand())
        {
          var commandText = string.Empty;
          if (UsingRecording.FindAllParams.HasValue && UsingRecording.FindAllParams.Value)
            commandText = string.Format(Queries.UsingRecording.SelectColumnsAllNames, tableName);
          else
            commandText = string.Format(Queries.UsingRecording.SelectColumnsNames, tableName, UsingRecording.TableName, additionalColumnName);

          command.CommandText = commandText;
          
          try
          {
            var reader = command.ExecuteReader();
            while (reader.Read())
              columnsNames.Add(new KeyValuePair<string, string>(tableName, reader.GetValue(0).ToString()));
          }
          catch (Exception ex)
          {
            Logger.ErrorFormat("Возникла ошибка в запросе получения имен полей: {0}", commandText);
            Logger.ErrorFormat("Ошибка: {0}", ex.Message);
            continue;
          }
        }
        
        // Запишем данные во временную таблицу
        foreach (var data in columnsNames)
        {
          using (var commandNonQuery = SQL.GetCurrentConnection().CreateCommand())
          {
            var commandText = string.Format(Queries.UsingRecording.InsertEntityTableColumn,
                                            tempReportTableName,
                                            data.Key,
                                            data.Value,
                                            UsingRecording.EntityDirID,
                                            UsingRecording.ReportSessionId);
            commandNonQuery.CommandText = commandText;

            try
            {
              commandNonQuery.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
              Logger.ErrorFormat("Возникла ошибка в запросе записи данных: {0}", commandText);
              Logger.ErrorFormat("Ошибка: {0}", ex.Message);
              continue;
            }
          }
        }
      }
    }
  }
}