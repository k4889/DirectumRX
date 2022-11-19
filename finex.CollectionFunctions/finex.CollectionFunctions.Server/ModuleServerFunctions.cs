using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;
using Sungero.Docflow;
using Sungero.Content;
using Sungero.Content.Shared;
using System.IO;
using Aspose.Pdf.Text;
using PdfSharp.Pdf;
//using Sungero.AsposeExtensions;
using System.IO.Packaging;
using System.Net.Mail;

namespace finex.CollectionFunctions.Server
{
  public class ModuleFunctions
  {
    
    #region Общие функции

    /// <summary>
    /// Проверить является ли текущий пользователь администратором.
    /// </summary>
    /// <returns>True, если является, иначе false.</returns>
    [Remote(IsPure = true), Public]
    public static bool IsAdministrator()
    {
      return Users.Current.IncludedIn(Roles.Administrators);
    }

    /// <summary>
    /// Получить ИД всех нижестоящиех подразделений включая текущие
    /// </summary>
    /// <param name="departaments">Список ИД подразделений</param>
    [Public]
    public static List<int> GetSubordinateDepartaments(List<int> departaments)
    {
      if (!departaments.Any())
        return departaments;
      
      var subordinateDepartaments = new List<int>();
      
      using (var command = Sungero.Core.SQL.GetCurrentConnection().CreateCommand())
      {
        var query = string.Format(Queries.Module.SelectSubordinateDepartments, Constants.Module.RecipientTypes.DepartmentTypeGuid, string.Join(",", departaments));
        command.CommandText = query;
        
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
          int id;
          if (int.TryParse(reader.GetValue(0).ToString(), out id))
            subordinateDepartaments.Add(id);
        }
      }
      
      return subordinateDepartaments;
    }
    
    /// <summary>
    /// Получить ИД всех вышестоящиех подразделений включая текущие
    /// </summary>
    /// <param name="departaments">Список ИД подразделений</param>
    [Public]
    public static List<int> GetSuperiorDepartaments(List<int> departaments)
    {
      if (!departaments.Any())
        return departaments;
      
      var superiorDepartaments = departaments;
      using (var command = Sungero.Core.SQL.GetCurrentConnection().CreateCommand())
      {
        var query = string.Format(Queries.Module.SelectSuperiorDepartments, Constants.Module.RecipientTypes.DepartmentTypeGuid, string.Join(",", departaments));
        command.CommandText = query;
        
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
          int id;
          if (int.TryParse(reader.GetValue(0).ToString(), out id))
            superiorDepartaments.Add(id);
        }
      }
      return superiorDepartaments;
    }
    
    /// <summary>
    /// Получить GUID типа сущности по строковому наименованию интерфейса
    /// </summary>
    /// <param name="entityTypeName">Строковое имя интерфейса</param>
    /// <returns></returns>
    [Public]
    public string GetTypeObjectGuid(string entityTypeName)
    {
      var entityType = Type.GetType(string.Format("{0}, Sungero.Domain.Interfaces, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", entityTypeName));
      return entityType.GetTypeGuid().ToString();
    }
    
    #endregion
    
    
    
    #region Работа с подключениями
    
    /// <summary>
    /// Получить информацию о подключении к системе для текущего пользователя
    /// </summary>
    /// <returns>Структура с информацией о подключении</returns>
    [Remote(IsPure = true), Public]
    public finex.CollectionFunctions.Structures.Module.IUserInfo GetCurrentConnectInfo()
    {
      var instance = Sungero.Domain.Clients.ClientManager.Instance;
      var currentClient = instance.CurrentClient;

      var userInfo = finex.CollectionFunctions.Structures.Module.UserInfo.Create();
      userInfo.ClientId = instance.CurrentClientId;
      userInfo.Name = currentClient.Name;
      userInfo.ApplicationName = currentClient.ApplicationName;
      userInfo.HostName = currentClient.HostName;
      userInfo.LoginId = currentClient.LoginId;
      userInfo.LastActivity = currentClient.LastActivity;

      return userInfo;
    }
    
    /// <summary>
    /// Получить наименование приложения в рамках которого заблокирован объект
    /// </summary>
    /// <param name="loginId">ИД учетной записи установившей блокировку</param>
    /// <param name="entityId">ИД заблокированного объекта</param>
    /// <returns>Наименование приложения в рамках которого заблокирован объект</returns>
    [Remote(IsPure = true), Public]
    public string GetLockApplicationName(int loginId, int entityId)
    {
      var query = string.Format(Queries.Module.GetLockApplicationName, loginId, entityId);
      return PublicInitializationFunctions.Module.ExecuteScalarSQLCommand(query);
    }
    
    /// <summary>
    /// Получить идентификатор клиента заблокировавшего объект
    /// </summary>
    /// <param name="loginId">ИД учетной записи установившей блокировку</param>
    /// <param name="entityId">ИД заблокированного объекта</param>
    /// <returns>Идентификатор клиента заблокировавшего объект</returns>
    [Remote(IsPure = true), Public]
    public string GetLockClientID(int loginId, int entityId)
    {
      var query = string.Format(Queries.Module.GetClientId, loginId, entityId);
      return PublicInitializationFunctions.Module.ExecuteScalarSQLCommand(query);
    }
    
    #endregion
    
    
    
    #region Работа с историей документа
    
    /// <summary>
    /// Проверить, просматривал ли пользователь документ
    /// </summary>
    /// <param name="userId">ИД пользователя</param>
    /// <returns>True если просматривал, иначе False</returns>
    [Public, Remote(IsPure = true)]
    public static bool CheckViewed(Sungero.Content.IElectronicDocument document, int userId)
    {
      return document.History.GetAll()
        .Any(h => h.Action == Sungero.CoreEntities.History.Action.Read
             && h.UserId == userId);
    }
    
    /// <summary>
    /// Проверить, просматривал ли пользователь документ начиная с даты
    /// </summary>
    /// <param name="userId">ИД пользователя</param>
    /// <param name="date">Дата и время</param>
    /// <returns>True если просматривал, иначе False</returns>
    [Public, Remote(IsPure = true)]
    public static bool CheckViewed(Sungero.Content.IElectronicDocument document, int userId, DateTime date)
    {
      return document.History.GetAll()
        .Any(h => h.Action == Sungero.CoreEntities.History.Action.Read
             && h.UserId == userId
             && h.HistoryDate.HasValue
             && h.HistoryDate.Value >= date);
    }
    
    /// <summary>
    /// Проверить, просматривал ли пользователь версию документа
    /// </summary>
    /// <param name="userId">ИД пользователя</param>
    /// <param name="versionNumber">Порядковый номер версии</param>
    /// <returns>True если просматривал, иначе False</returns>
    [Public, Remote(IsPure = true)]
    public static bool CheckViewed(Sungero.Content.IElectronicDocument document, int userId, int versionNumber)
    {
      return document.History.GetAll()
        .Any(h => h.Action == Sungero.CoreEntities.History.Action.Read
             && h.UserId == userId
             && h.VersionNumber == versionNumber
             && h.Operation == Sungero.Content.DocumentHistory.Operation.ReadVerBody);
    }

    /// <summary>
    /// Проверить, просматривал ли пользователь версию документа начиная с даты
    /// </summary>
    /// <param name="userId">ИД пользователя</param>
    /// <param name="date">Дата и время</param>
    /// <param name="versionNumber">Порядковый номер версии</param>
    /// <returns>True если просматривал, иначе False</returns>
    [Public, Remote(IsPure = true)]
    public static bool CheckViewed(Sungero.Content.IElectronicDocument document, int userId, DateTime date, int versionNumber)
    {
      return document.History.GetAll()
        .Any(h => h.Action == Sungero.CoreEntities.History.Action.Read
             && h.UserId == userId
             && h.VersionNumber == versionNumber
             && h.Operation == Sungero.Content.DocumentHistory.Operation.ReadVerBody
             && h.HistoryDate.HasValue
             && h.HistoryDate.Value >= date);
    }
    
    #endregion
    
    
    
    #region Работа с Excel
    
    /// <summary>
    /// Парсит xlsx файл.
    /// Первая строка парсится как названия для столбцов.
    /// </summary>
    /// <param name="byteArray">Структура с массивом байт</param>
    /// <param name="requiredLabels">Необходимые названия столбцов. Если название не найдено, то выкидывает ошибку</param>
    /// <returns>Массив строк из файла. В словаре по ключу - название столбца, по значению - значение ячейки. Пустые ячейки заполняются string.Empty</returns>
    [Public]
    public static List<System.Collections.Generic.Dictionary<string, string>> ParceExcel(Sungero.Docflow.Structures.Module.IByteArray byteArray, string[] requiredLabels)
    {
      var result = new List<Dictionary<string, string>>();
      
      using(var stream = new System.IO.MemoryStream(byteArray.Bytes))
      {
        using (var document = DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Open(stream, true))
        {
          var wbPart = document.WorkbookPart;
          var sheet = wbPart.Workbook.Descendants<DocumentFormat.OpenXml.Spreadsheet.Sheet>().First();
          var wsPart = (DocumentFormat.OpenXml.Packaging.WorksheetPart)(wbPart.GetPartById(sheet.Id));
          var rows = wsPart.Worksheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>();
          
          //первая строка - названия столбцов
          var labels = rows.First().Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>()
            .Select(cell => GetCellValue(cell, wbPart))
            .ToList();
          
          foreach (var requiredLabel in requiredLabels)
          {
            if (!labels.Contains(requiredLabel))
              throw new Exception(string.Format("Не верный формат excel таблицы.\nНе найден столбец - \"{0}\" в списке:\n{1}", requiredLabel, string.Join(",\n", labels)));
          }
          
          foreach (var row in rows.Skip(1))
          {
            //Заполняем пустыми значениями, т.к. пустые ячейки не обрабатываются
            var values = new Dictionary<string, string>();
            foreach (var label in labels)
              values.Add(label, string.Empty);

            
            foreach (var cell in row.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>())
            {
              var realIndex = CellReferenceToIndex(cell);
              if(realIndex < labels.Count)
                values[labels[realIndex]] = GetCellValue(cell, wbPart);
            }
            
            var isRowAdd = false;
            
            foreach (var data in values)
            {
              if (!string.IsNullOrEmpty(data.Value))
              {
                isRowAdd = true;
                break;
              }
            }
            
            if (isRowAdd)
              result.Add(values);
          }
        }
      }
     
      Logger.Debug(">>Import data: parsing excel file - complited!");
      
      return result;
    }
    
    /// <summary>
    /// Возращает индекс в строке, с учетом пустых ячеек. Elements<Cell>() не содержит пустые ячейки.
    /// </summary>
    private static int CellReferenceToIndex(DocumentFormat.OpenXml.Spreadsheet.Cell cell)
    {
      var index = 0;
      var reference = cell.CellReference.ToString().ToUpper();
      foreach (var ch in reference)
      {
        if (Char.IsLetter(ch))
        {
          int value = (int)ch - (int)'A';
          index = (index == 0) ? value : ((index + 1) * 26) + value;
        }
        else
          return index;
      }
      
      return index;
    }
    
    /// <summary>
    /// Возращает значение ячейки в виде строки
    /// </summary>
    private static string GetCellValue(DocumentFormat.OpenXml.Spreadsheet.Cell cell, DocumentFormat.OpenXml.Packaging.WorkbookPart wbPart)
    {
      if (cell == null)
        return string.Empty;

      if (cell.DataType != null)
      {
        if (cell.DataType.Value == DocumentFormat.OpenXml.Spreadsheet.CellValues.SharedString)
        {
          var stringTable = wbPart.GetPartsOfType<DocumentFormat.OpenXml.Packaging.SharedStringTablePart>().FirstOrDefault();
          if (stringTable != null)
            return stringTable.SharedStringTable.ElementAt(int.Parse(cell.InnerText)).InnerText;
        }
        
        if (cell.DataType.Value == DocumentFormat.OpenXml.Spreadsheet.CellValues.Boolean)
          return cell.InnerText == "0" ? "FALSE" : "TRUE";
      }
      
      return cell.InnerText;
    }
    
    #endregion
    
    
    
    #region Создание версий документа
    
    /// <summary>
    /// Создать версию документа из шаблона
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="template">Шаблон</param>
    /// <param name="isSave">Сохранить документ</param>
    [Remote, Public]
    public static Sungero.Docflow.IOfficialDocument CreateVersionFromTemplate(Sungero.Docflow.IOfficialDocument document,
                                                                              Sungero.Docflow.IDocumentTemplate template,
                                                                              bool isSave)
    {
      if (document == null)
        return null;
      
      if (template == null || !template.HasVersions)
        return null;
      
      using (var body = template.LastVersion.Body.Read())
      {
        var newVersion = document.CreateVersionFrom(body, template.AssociatedApplication.Extension);
        var exEntity = (Sungero.Domain.Shared.IExtendedEntity)document;
        exEntity.Params[Sungero.Content.Shared.ElectronicDocumentUtils.FromTemplateIdKey] = template.Id;
      }
      
      if (isSave)
        document.Save();
      
      return document;
    }
    
    /// <summary>
    /// Создать версию документа из массива байт.
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="byteArray">Структура с массивом байт</param>
    /// <param name="extension">Расширение файла</param>
    /// <param name="isSave">Сохранить документ</param>
    [Remote, Public]
    public static Sungero.Content.IElectronicDocument CreateVersionFromByteArray(Sungero.Content.IElectronicDocument document,
                                                                                 Sungero.Docflow.Structures.Module.IByteArray byteArray,
                                                                                 string extension,
                                                                                 bool isSave)
    {
      if (document == null)
        return null;
      
      if (string.IsNullOrEmpty(extension))
        return null;
      
      if (byteArray == null || byteArray.Bytes.Length == 0)
        return null;
      
      using (var stream = new MemoryStream(byteArray.Bytes))
      {
        document.CreateVersionFrom(stream, extension);
      }
      
      if (isSave)
        document.Save();
      
      return document;
    }
    
    #endregion
    
    
    
    #region Работа с задачами и заданиями
    
    /// <summary>
    /// Завершить конкурирующие задания (конкурентное выполнение)
    /// </summary>
    /// <param name="assignment">Задание</param>
    [Public]
    public static void AbortConcurrentAssignemnts(Sungero.Workflow.IAssignment assignment)
    {
      if (assignment == null)
        return;
      
      var anotherAssignments = Sungero.Workflow.Assignments
        .GetAll(a => Equals(a.Task, assignment.Task))
        .Where(a => a.Status == Sungero.Workflow.Assignment.Status.InProcess)
        .Where(a => a.IterationId == assignment.IterationId)
        .Where(a => a.BlockUid == assignment.BlockUid)
        .Where(a => !Equals(a, assignment));
      
      foreach (var another in anotherAssignments)
        another.Abort();
    }
    
    #endregion
    
    
    
    #region Работа с папками и файлами на сервере
    
    /// <summary>
    /// Создать папку в Temp каталоге
    /// </summary>
    /// <param name="name">Имя папки</param>
    /// <returns>Путь к созданной папке</returns>
    [Public]
    public static string CreateTempFolder(string name)
    {
      string folderPath = System.IO.Path.GetTempPath();
      
      var createFolderPatch = Path.Combine(folderPath, name);
      DeleteFolder(createFolderPatch);
      Directory.CreateDirectory(createFolderPatch);
      
      return createFolderPatch;
    }
    
    /// <summary>
    /// Удалить папку на диске
    /// </summary>
    /// <param name="folderPatch">Путь к папке</param>
    [Public]
    public static void DeleteFolder(string folderPatch)
    {
      if (Directory.Exists(folderPatch))
        Directory.Delete(folderPatch, true);
    }
    
    /// <summary>
    /// Удалить файл на диске
    /// </summary>
    /// <param name="filePatch">Путь к файлу</param>
    [Public]
    public static void DeleteFile(string filePatch)
    {
      if (File.Exists(filePatch))
        File.Delete(filePatch);
    }
    
    /// <summary>
    /// Экспорт версии документа в Zip архив
    /// </summary>
    /// <param name="zipArhive">Zip архив</param>
    /// <param name="version">Версия электронного документа</param>
    /// <param name="filePatch">Имя папки в архиве</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="isPublicBodyOnly">Экспортировать только PublicBody</param>
    /// <param name="isCertificateExport">Экспортировать сертификаты</param>
    /// <param name="signatureTypes">Тип подписи (если null, то экспортируются все типы)</param>
    /// <param name="users">Подписавшие (если null, то экспортируются подписи всех подписавших)</param>
    /// <returns>Zip архив</returns>
    [Public]
    public static IZip ExportDocumentVersionFromZip(IZip zipArhive,
                                                    Sungero.Content.IElectronicDocumentVersions version,
                                                    string[] folderName,
                                                    string fileName,
                                                    bool isPublicBodyOnly,
                                                    bool isCertificateExport,
                                                    List<Sungero.Core.SignatureType> signatureTypes,
                                                    List<Sungero.CoreEntities.IUser> users)
    {
      if (zipArhive == null)
        zipArhive = Zip.Create();
      
      var publicBodyExtension = string.Empty;
      
      #region Экспорт PublicBody

      if (version.PublicBody.Size != 0)
      {
        publicBodyExtension = version.AssociatedApplication.Extension;
        var publicBodyName = string.Format("{0}_v{1}.{2}", fileName, version.Number, publicBodyExtension);
        
        if (folderName != null && folderName.Count() > 0)
          zipArhive.Add(version.PublicBody, publicBodyName, folderName);
        else
          zipArhive.Add(version.PublicBody, publicBodyName);
      }
      else
        isPublicBodyOnly = false;
      
      #endregion
      
      #region Экспорт Body
      
      if (!isPublicBodyOnly && version.Body.Size != 0 && version.PublicBody.Size != version.Body.Size)
      {
        var bodyExtension = version.BodyAssociatedApplication.Extension;
        var bodyName = string.Empty;
        
        if (bodyExtension == publicBodyExtension)
          bodyName = string.Format("{0}_v{1}_original.{2}", fileName, version.Number, bodyExtension);
        else
          bodyName = string.Format("{0}_v{1}.{2}", fileName, version.Number, bodyExtension);

        if (folderName != null && folderName.Count() > 0)
          zipArhive.Add(version.Body, bodyName, folderName);
        else
          zipArhive.Add(version.Body, bodyName);
      }
      
      #endregion
      
      #region Экспорт сертификатов
      
      if (isCertificateExport)
      {
        var signatures = Signatures.Get(version).Where(s => s.IsExternal != true && s.SignCertificate != null);
        
        if (signatureTypes.Any())
          signatures = signatures.Where(s => signatureTypes.Contains(s.SignatureType));
        
        if (users.Any())
        {
          var usersIds = users.Select(u => u.Id);
          signatures = signatures.Where(s => usersIds.Contains(s.Signatory.Id));
        }
        
        if (signatures.Any())
        {
          var signature = signatures.LastOrDefault();
          var signName = string.Format("{0}_v{1}_{2}", fileName, version.Number, signature.Signatory.Name);
          
          if (folderName != null && folderName.Count() > 0)
            zipArhive.Add(signature, signName, folderName);
          else
            zipArhive.Add(signature, signName);
        }
      }
      
      #endregion
      
      return zipArhive;
    }
    
    /// <summary>
    /// Экспорт массива байт в Zip архив
    /// </summary>
    /// <param name="zipArhive">Zip архив</param>
    /// <param name="byteArray">Массив байт</param>
    /// <param name="filePatch">Имя папки в архиве</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="extension">Расширение файла</param>
    /// <returns>Zip архив</returns>
    [Public]
    public static IZip ExportByteArrayFromZip(IZip zipArhive,
                                              Sungero.Docflow.Structures.Module.IByteArray byteArray,
                                              string[] folderName,
                                              string fileName,
                                              string extension)
    {
      if (zipArhive == null)
        zipArhive = Zip.Create();

      if (folderName != null && folderName.Count() > 0)
        zipArhive.Add(byteArray.Bytes, fileName, extension, folderName);
      else
        zipArhive.Add(byteArray.Bytes, fileName, extension);

      return zipArhive;
    }
    
    /// <summary>
    /// Экспорт версии документа на диск
    /// </summary>
    /// <param name="version">Версия электронного документа</param>
    /// <param name="filePatch">Путь экспорта</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="isPublicBodyOnly">Экспортировать только PublicBody</param>
    /// <param name="isCertificateExport">Экспортировать сертификаты</param>
    /// <param name="signatureTypes">Тип подписи (если null, то экспортируются все тип)</param>
    /// <param name="users">Подписавшие (если null, то экспортируются подписи всех подписавших)</param>
    /// <returns></returns>
    [Public]
    public static List<string> ExportDocumentVersion(Sungero.Content.IElectronicDocumentVersions version,
                                                     string filePatch,
                                                     string fileName,
                                                     bool isPublicBodyOnly,
                                                     bool isCertificateExport,
                                                     List<Sungero.Core.SignatureType> signatureTypes,
                                                     List<Sungero.CoreEntities.IUser> users)
    {
      var patchesList = new List<string>();
      
      if (!Directory.Exists(filePatch))
        Directory.CreateDirectory(filePatch);
      
      var publicBodyExtension = string.Empty;
      
      #region Экспорт PublicBody
      
      if (version.PublicBody.Size != 0)
      {
        publicBodyExtension = version.AssociatedApplication.Extension;
        var publicBodyPatch = Path.Combine(filePatch, string.Format("{0}_v{1}.{2}", fileName, version.Number, publicBodyExtension));
        using (var memory = new System.IO.MemoryStream())
        {
          version.PublicBody.Read().CopyTo(memory);
          ExportFile(publicBodyPatch, memory.ToArray());
          patchesList.Add(publicBodyPatch);
        }
      }
      else
        isPublicBodyOnly = false;
      
      #endregion
      
      #region Экспорт Body
      
      if (!isPublicBodyOnly && version.Body.Size != 0 && version.PublicBody.Size != version.Body.Size)
      {
        var bodyExtension = version.BodyAssociatedApplication.Extension;
        var bodyPatch = string.Empty;
        
        if (bodyExtension == publicBodyExtension)
          bodyPatch = Path.Combine(filePatch, string.Format("{0}_v{1}_original.{2}", fileName, version.Number, bodyExtension));
        else
          bodyPatch = Path.Combine(filePatch, string.Format("{0}_v{1}.{2}", fileName, version.Number, bodyExtension));
        
        using (var memory = new System.IO.MemoryStream())
        {
          version.Body.Read().CopyTo(memory);
          ExportFile(bodyPatch, memory.ToArray());
          patchesList.Add(bodyPatch);
        }
      }
      
      #endregion
      
      #region Экспорт сертификатов
      
      if (isCertificateExport)
      {
        var signatures = Signatures.Get(version).Where(s => s.IsExternal != true && s.SignCertificate != null);
        
        if (signatureTypes.Any())
          signatures = signatures.Where(s => signatureTypes.Contains(s.SignatureType));
        
        if (users.Any())
        {
          var usersIds = users.Select(u => u.Id);
          signatures = signatures.Where(s => usersIds.Contains(s.Signatory.Id));
        }
        
        if (signatures.Any())
        {
          var signature = signatures.LastOrDefault();
          var signPatch = Path.Combine(filePatch, string.Format("{0}_v{1}_{2}_{3}", fileName, version.Number, signature.Signatory.Name, Constants.Module.SigFileName));
          ExportFile(signPatch, signature.GetDataSignature());
        }
      }
      
      #endregion
      
      return patchesList;
    }
    
    /// <summary>
    /// Экспорт массива байт на диск
    /// </summary>
    /// <param name="patch">Путь</param>
    /// <param name="byteArray">Массив байт</param>
    [Public]
    public static void ExportFile(string patch, Sungero.Docflow.Structures.Module.IByteArray byteArray)
    {
      ExportFile(patch, byteArray.Bytes);
    }
    
    /// <summary>
    /// Экспорт файла на диск
    /// </summary>
    /// <param name="patch">Путь</param>
    /// <param name="memoryArray">Массив байт</param>
    private static void ExportFile(string patch, byte[] memoryArray)
    {
      using (var fstream = new FileStream(patch, FileMode.OpenOrCreate))
      {
        fstream.Write(memoryArray , 0, memoryArray.Length);
        fstream.Flush();
      }
    }
    
    /// <summary>
    /// Имя документа/название папки для выгрузки.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="forFolder">Если true - название папки для выгрузки, иначе имя документа.</param>
    /// <returns>Имя документа/название папки.</returns>
    [Remote(IsPure = true), Public]
    public static string GetDocumentNameForExport(Sungero.Docflow.IOfficialDocument document, bool forFolder)
    {
      var name = string.Empty;
      var russianCulture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
      using (Sungero.Core.CultureInfoExtensions.SwitchTo(russianCulture))
      {
        if (document.RegistrationNumber != null)
          name += Sungero.Docflow.OfficialDocuments.Resources.Number + document.RegistrationNumber;
        
        if (document.RegistrationDate != null)
          name += Sungero.Docflow.OfficialDocuments.Resources.DateFrom + document.RegistrationDate.Value.ToString("d");
      }
      
      var accounting = AccountingDocumentBases.As(document);
      var type = string.Empty;
      if (accounting != null || forFolder)
        type = document.DocumentKind.DocumentType.Name;
      
      // Если тип документа не удалось определить, берем просто его имя ограниченной длины. Только для имен файлов.
      if (!string.IsNullOrWhiteSpace(type))
        name = type + name;
      else if (!forFolder)
        name = document.Name.Substring(0, Math.Min(document.Name.Length, Constants.Module.ExportNameLength));
      
      // Для формирования имени неформализованного финансового документа.
      if (!forFolder && accounting != null && accounting.IsFormalized != true)
        name = document.Name.Substring(0, Math.Min(document.Name.Length, Constants.Module.ExportNameLength));
      
      return name;
    }
    
    #endregion

    
    
    #region Работа с подписями

    /// <summary>
    /// Получить подпись версии документа в виде массива байт
    /// </summary>
    /// <param name="version">Версия документа</param>
    /// <param name="signatureType">Тип подписи</param>
    /// <param name="employee">Сотрудник</param>
    /// <returns>Массив байт подписи</returns>
    [Public]
    public static Sungero.Docflow.Structures.Module.IByteArray GetDataSignature(Sungero.Content.IElectronicDocumentVersions version,
                                                                                Sungero.Core.SignatureType signatureType,
                                                                                Sungero.Company.IEmployee employee)
    {
      var signatures = Signatures.Get(version).Where(s => s.SignatureType == signatureType && s.IsExternal != true);
      
      var signature = signatures.FirstOrDefault();
      if (employee != null)
        signature = signatures.Where(s => Equals(s.Signatory, employee)).FirstOrDefault();
      
      var structure = Sungero.Docflow.Structures.Module.ByteArray.Create();
      if (signature != null)
        structure.Bytes = ((Sungero.Domain.Shared.IInternalSignature)signature).GetDataSignature();
      
      return structure;
    }
    
    /// <summary>
    /// Получить электронную подпись для простановки отметки.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="versionId">Номер версии.</param>
    /// <param name="signatureType">Тип подписи.</param>
    /// <returns>Электронная подпись.</returns>
    [Public]
    public static Sungero.Domain.Shared.ISignature GetSignatureForMark(Sungero.Docflow.IOfficialDocument document,
                                                                       int versionId,
                                                                       Sungero.Core.SignatureType signatureType)
    {
      var version = document.Versions.FirstOrDefault(x => x.Id == versionId);
      if (version == null)
        return null;
      
      // Подписи.
      var versionSignatures = Signatures.Get(version)
        .Where(s => s.IsExternal != true && s.SignatureType == signatureType)
        .ToList();
      
      if (!versionSignatures.Any())
        return null;
      
      // В приоритете подпись сотрудника из поля "Подписал". Квалифицирофанная ЭП приоритетнее простой.
      return versionSignatures
        .OrderByDescending(s => Equals(s.Signatory, document.OurSignatory))
        .ThenBy(s => s.SignCertificate == null)
        .ThenByDescending(s => s.SigningDate)
        .FirstOrDefault();
    }
    
    /// <summary>
    /// Получить последний номер версии подписанный сертификатом
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="user">Пользователь</param>
    /// <param name="signatureTypes">Типы подписи (строковые значения, т.к. в Remote функциях нельзя передавать SignatureType)</param>
    /// <param name="isCertificate">Только сертификаты</param>
    /// <returns>Номер версии (-1, если нет совпадений)</returns>
    [Remote(IsPure = true), Public]
    public static int GetLastSignedVersionNumber(Sungero.Docflow.IOfficialDocument document,
                                                 IUser user,
                                                 List<string> signatureTypesString,
                                                 bool isCertificate)
    {
      var signatureTypes = new List<SignatureType>();
      foreach (var typesString in signatureTypesString)
      {
        if (typesString == SignatureType.Approval.ToString())
          signatureTypes.Add(SignatureType.Approval);
        
        if (typesString == SignatureType.Endorsing.ToString())
          signatureTypes.Add(SignatureType.Endorsing);
        
        if (typesString == SignatureType.NotEndorsing.ToString())
          signatureTypes.Add(SignatureType.NotEndorsing);
      }
      
      return GetLastSignedVersionNumber(document, user, signatureTypes, isCertificate);
    }

    /// <summary>
    /// Получить последний номер версии подписанный сертификатом
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="user">Пользователь</param>
    /// <param name="signatureTypes">Типы подписи</param>
    /// <param name="isCertificate">Только сертификаты</param>
    /// <returns>Номер версии (-1, если нет совпадений)</returns>
    [Public]
    public static int GetLastSignedVersionNumber(Sungero.Docflow.IOfficialDocument document,
                                                 IUser user,
                                                 List<Sungero.Core.SignatureType> signatureTypes,
                                                 bool isCertificate)
    {
      if (document == null || user == null || !signatureTypes.Any())
        return -1;
      
      var signatures = document.Versions
        .SelectMany(v => Signatures.Get(v))
        .Where(s => s.IsExternal != true)
        .Where(s => Equals(s.Signatory, user))
        .Where(s => signatureTypes.Contains(s.SignatureType));
      
      if (isCertificate)
        signatures = signatures.Where(s => s.SignCertificate != null);
      
      var version = signatures
        .Select(s => s.Entity)
        .Cast<Sungero.Content.IElectronicDocumentVersions>()
        .FirstOrDefault();
      
      return version != null ? version.Number.Value : -1;
    }
    
    /// <summary>
    /// Получить последний номер версии подписанный сертификатами
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="users">Список пользователей</param>
    /// <param name="signatureTypes">Типы подписи (строковые значения, т.к. в Remote функциях нельзя передавать SignatureType)</param>
    /// <param name="isCertificate">Только сертификаты</param>
    /// <param name="isPdf">Только PDF PublicBody версии</param>
    /// <returns>Номер версии (-1, если нет совпадений)</returns>
    [Remote(IsPure = true), Public]
    public static int GetLastSignedVersionNumber(Sungero.Docflow.IOfficialDocument document,
                                                 List<IUser> users,
                                                 List<string> signatureTypesString,
                                                 bool isCertificate,
                                                 bool isPdf)
    {
      var signatureTypes = new List<SignatureType>();
      foreach (var typesString in signatureTypesString)
      {
        if (typesString == SignatureType.Approval.ToString())
          signatureTypes.Add(SignatureType.Approval);
        
        if (typesString == SignatureType.Endorsing.ToString())
          signatureTypes.Add(SignatureType.Endorsing);
        
        if (typesString == SignatureType.NotEndorsing.ToString())
          signatureTypes.Add(SignatureType.NotEndorsing);
      }
      
      return GetLastSignedVersionNumber(document, users, signatureTypes, isCertificate, isPdf);
    }
    
    /// <summary>
    /// Получить последний номер версии подписанный сертификатами
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="users">Список пользователей</param>
    /// <param name="signatureTypes">Типы подписи</param>
    /// <param name="isCertificate">Только сертификаты</param>
    /// <param name="isPdf">Только PDF PublicBody версии</param>
    /// <returns>Номер версии (-1, если нет совпадений)</returns>
    [Public]
    public static int GetLastSignedVersionNumber(Sungero.Docflow.IOfficialDocument document,
                                                 List<IUser> users,
                                                 List<Sungero.Core.SignatureType> signatureTypes,
                                                 bool isCertificate,
                                                 bool isPdf)
    {
      if (document == null || !users.Any() || !signatureTypes.Any())
        return -1;
      
      var usersIds = users.Select(u => u.Id).ToList();
      
      var versions = document.Versions.ToList();
      if (isPdf)
      {
        var pdfExtension = Sungero.Docflow.PublicConstants.OfficialDocument.PdfExtension.ToLower();
        versions = versions.Where(v => v.AssociatedApplication.Extension.ToLower() == pdfExtension).ToList();
      }
      
      var signatures = versions
        .SelectMany(v => Signatures.Get(v))
        .Where(s => s.IsExternal != true)
        .Where(s => usersIds.Contains(s.Signatory.Id))
        .Where(s => signatureTypes.Contains(s.SignatureType));
      
      if (isCertificate)
        signatures = signatures.Where(s => s.SignCertificate != null);
      
      var version = signatures
        .Select(s => s.Entity)
        .Cast<Sungero.Content.IElectronicDocumentVersions>()
        .FirstOrDefault();
      return version != null ? version.Number.Value : -1;
    }
    
    /// <summary>
    /// Проверяет подписан ли документ пользователем
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="user">Пользователь</param>
    /// <param name="signatureTypes">Типы подписи</param>
    /// <param name="isCertificate">Только сертификаты</param>
    [Public]
    public static bool IsDocumentSigned(Sungero.Docflow.IOfficialDocument document,
                                        IUser user,
                                        List<Sungero.Core.SignatureType> signatureTypes,
                                        bool isCertificate)
    {
      return GetLastSignedVersionNumber(document, user, signatureTypes, isCertificate) != -1;
    }
    
    /// <summary>
    /// Получить последнюю подпись пользователя
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="user">Пользователь</param>
    /// <param name="signatureTypes">Типы подписи</param>
    /// <param name="isCertificate">Только подпись с сертификатом</param>
    /// <returns>Подпись</returns>
    [Public]
    public static Sungero.Domain.Shared.ISignature GetLastUserSignature(Sungero.Docflow.IOfficialDocument document,
                                                                        IUser user,
                                                                        List<Sungero.Core.SignatureType> signatureTypes,
                                                                        bool isCertificate)
    {
      if (document == null || user == null || !signatureTypes.Any())
        return null;
      
      var signatures = Signatures.Get(document).Where(s => s.IsExternal != true && Equals(s.Signatory, user) && signatureTypes.Contains(s.SignatureType));
      
      if (isCertificate)
        signatures = signatures.Where(s => s.SignCertificate != null);
      
      return signatures.OrderByDescending(s => s.SigningDate).FirstOrDefault();
    }
    #endregion
    
    
    
    #region Работа со штампами ЭП
    
    /// <summary>
    /// Получить отметку об ЭП в html формате.
    /// </summary>
    /// <param name="document">Документ для преобразования.</param>
    /// <param name="versionId">Id версии, для генерации.</param>
    /// <param name="signatureType">Тип подписи.</param>
    /// <returns>Изображение отметки об ЭП в виде html.</returns>
    [Public]
    public static string GetSignatureMarkAsHtml(Sungero.Docflow.IOfficialDocument document,
                                                int versionId,
                                                Sungero.Core.SignatureType signatureType)
    {
      var signature = GetSignatureForMark(document, versionId, signatureType);
      
      if (signature == null)
        return string.Empty;
      
      // В случае квалифицированной ЭП информацию для отметки брать из атрибутов субъекта сертификата.
      if (signature.SignCertificate != null)
        return Sungero.Docflow.PublicFunctions.Module.GetSignatureMarkForCertificateAsHtml(signature);
      
      // В случае простой ЭП информацию для отметки брать из атрибутов подписи.
      return Sungero.Docflow.PublicFunctions.Module.GetSignatureMarkForSimpleSignatureAsHtml(signature);
    }
    
    /// <summary>
    /// Установить штамп на PDF версию документа в виде массива байт
    /// </summary>
    /// <param name="version">Массив байт PDF версии документа.</param>
    /// <param name="signatureMark">Отметка об ЭП в html формате.</param>
    /// <param name="pageNums">Номера страниц штампа (если null, то проставляет штамп на всех страницах).</param>
    /// <param name="horizontalAlignment">Горизонтальное положение штампа (0 - по левому краю, 1 - по правому краю, 2 - по центру) .</param>
    /// <param name="verticalAlignment">Вертикальное положение штампа (0 - по верхнему краю, 1 - по центру, 2 - по нижнему краю) .</param>
    /// <returns>Информация о результате генерации для версии документа.</returns>
    [Public]
    public static finex.CollectionFunctions.Structures.Module.IСonversionToPdfResult SetStampOnByteArray(Sungero.Docflow.Structures.Module.IByteArray byteArray,
                                                                                                         string signatureMark,
                                                                                                         int[] pageNums,
                                                                                                         int horizontalAlignment,
                                                                                                         int verticalAlignment)
    {
      var info = Structures.Module.СonversionToPdfResult.Create();
      info.HasErrors = true;

      if (byteArray == null || byteArray.Bytes == null)
      {
        info.HasConvertionError = true;
        return info;
      }
      
      if (string.IsNullOrEmpty(signatureMark))
      {
        info.HasConvertionError = true;
        return info;
      }
      
      try
      {
        var pdfDocumentStream = new System.IO.MemoryStream();
        using (var inputStream = new System.IO.MemoryStream(byteArray.Bytes))
        {
          inputStream.CopyTo(pdfDocumentStream);
          try
          {
            var pdfConverter = new Sungero.AsposeExtensions.Converter();
            
            var htmlStampString = pdfConverter.CreateMarkFromHtml(signatureMark);
            
            htmlStampString.HorizontalAlignment = GetHorizontalAlignment(horizontalAlignment);
            htmlStampString.VerticalAlignment = GetVerticalAlignment(verticalAlignment);
            
            if (pageNums.Length > 0)
            {
              using (Aspose.Pdf.Document aspDocument = pdfConverter.AddStampToDocument(new Aspose.Pdf.Document(pdfDocumentStream), htmlStampString, pageNums))
                aspDocument.Save(pdfDocumentStream);
            }
            else
            {
              using (Aspose.Pdf.Document aspDocument = pdfConverter.AddStampToDocument(new Aspose.Pdf.Document(pdfDocumentStream), htmlStampString))
                aspDocument.Save(pdfDocumentStream);
            }

          }
          catch (Exception e)
          {
            Logger.ErrorFormat("SetStampOnByteArray: {0}", e.Message);
            info.HasConvertionError = true;
            info.HasLockError = false;
            info.ErrorMessage = string.Format("{0}\r\n{1}", Sungero.Docflow.Resources.DocumentBodyNeedsRepair, e.Message);
          }
        }
        
        if (!string.IsNullOrWhiteSpace(info.ErrorMessage))
          return info;
        
        info.Body = StreamToByteArray(pdfDocumentStream);
        pdfDocumentStream.Close();

        info.HasErrors = false;
      }
      catch (Sungero.Domain.Shared.Exceptions.RepeatedLockException e)
      {
        Logger.Error(e.Message);
        info.HasConvertionError = false;
        info.HasLockError = true;
        info.ErrorMessage = e.Message;
      }
      catch (Exception e)
      {
        Logger.Error(e.Message);
        info.HasConvertionError = true;
        info.HasLockError = false;
        info.ErrorMessage = e.Message;
      }

      return info;
    }
    
    /// <summary>
    /// Установить штрихкод на PDF версию документа в виде массива байт
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="byteArray">Массив байт PDF версии документа.</param>
    /// <param name="pageNums">Номера страниц штампа (если null, то проставляет штамп на всех страницах).</param>
    /// <param name="horizontalAlignment">Горизонтальное положение штампа (0 - по левому краю, 1 - по правому краю, 2 - по центру).</param>
    /// <param name="verticalAlignment">Вертикальное положение штампа (0 - по верхнему краю, 1 - по центру, 2 - по нижнему краю).</param>
    /// <returns>Информация о результате генерации для версии документа.</returns>
    [Public]
    public static finex.CollectionFunctions.Structures.Module.IСonversionToPdfResult SetBarCodeStampOnByteArray(Sungero.Docflow.IOfficialDocument document,
                                                                                                                Sungero.Docflow.Structures.Module.IByteArray byteArray,
                                                                                                                int[] pageNums,
                                                                                                                int horizontalAlignment,
                                                                                                                int verticalAlignment)
    {
      var info = Structures.Module.СonversionToPdfResult.Create();
      info.HasErrors = true;

      if ((byteArray == null || byteArray.Bytes == null) && document == null)
      {
        info.HasConvertionError = true;
        return info;
      }
      
      if (byteArray == null || byteArray.Bytes == null)
      {
        var convert = ConvertVersionToPdf(document, null, false, false);
        byteArray.Bytes = convert.Body;
      }
      
      var barcodeArray = Sungero.Content.PublicFunctions.ElectronicDocument.Remote.GetBarcode(document);
      try
      {
        var pdfDocumentStream = new System.IO.MemoryStream();
        using (var inputStream = new System.IO.MemoryStream(byteArray.Bytes))
        {
          inputStream.CopyTo(pdfDocumentStream);
          try
          {
            var stampWidth = 228;
            var stampHeight = 114;
            var pdfConverter = new Sungero.AsposeExtensions.Converter();
            var pdfStamp = CreateMarkFromByte(barcodeArray, "jpg", stampWidth, stampHeight);

            var pdfDocument = new Aspose.Pdf.Document(pdfDocumentStream);
            var pageInfo = pdfDocument.PageInfo;
            var margin = pdfDocument.PageInfo.Margin;
            
            switch (horizontalAlignment)
            {
              case 0:
                pdfStamp.XIndent = 0 - margin.Left;
                break;
              case 1:
                pdfStamp.XIndent = pageInfo.Width + (margin.Right / 2) - stampWidth - 10;
                break;
              default:
                pdfStamp.XIndent = (pageInfo.Width / 2) - stampWidth;
                break;
            }
            
            switch (verticalAlignment)
            {
              case 0:
                pdfStamp.YIndent = pageInfo.Height + margin.Top;
                break;
              case 1:
                pdfStamp.YIndent = ((pageInfo.PureHeight + margin.Top + margin.Bottom) / 2) - (stampHeight / 2);
                break;
              default:
                pdfStamp.YIndent = (margin.Bottom - 10) * -1;
                break;
            }
            
            if (pageNums.Length > 0)
            {
              using (Aspose.Pdf.Document aspDocument = pdfConverter.AddStampToDocument(pdfDocument, pdfStamp, pageNums))
                aspDocument.Save(pdfDocumentStream);
            }
            else
            {
              using (Aspose.Pdf.Document aspDocument = pdfConverter.AddStampToDocument(pdfDocument, pdfStamp))
                aspDocument.Save(pdfDocumentStream);
            }
          }
          catch (Exception e)
          {
            Logger.ErrorFormat("SetBarCodeStampOnByteArray: {0}", e.Message);
            info.HasConvertionError = true;
            info.HasLockError = false;
            info.ErrorMessage = string.Format("{0}\r\n{1}", Sungero.Docflow.Resources.DocumentBodyNeedsRepair, e.Message);
          }
          
          inputStream.Close();
        }
        
        if (!string.IsNullOrWhiteSpace(info.ErrorMessage))
          return info;
        
        info.Body = StreamToByteArray(pdfDocumentStream);
        pdfDocumentStream.Close();

        info.HasErrors = false;
      }
      catch (Sungero.Domain.Shared.Exceptions.RepeatedLockException e)
      {
        Logger.Error(e.Message);
        info.HasConvertionError = false;
        info.HasLockError = true;
        info.ErrorMessage = e.Message;
      }
      catch (Exception e)
      {
        Logger.Error(e.Message);
        info.HasConvertionError = true;
        info.HasLockError = false;
        info.ErrorMessage = e.Message;
      }
      
      return info;
    }
    
    /// <summary>
    /// Установить штамп по якорю на версию документа в виде массива байт
    /// </summary>
    /// <param name="version">Массив байт версии документа.</param>
    /// <param name="signatureMark">Отметка об ЭП в html формате.</param>
    /// <param name="anchorSymbol">Строка якоря.</param>
    /// <param name="extension">Расширение версии документа.</param>
    /// <returns>Информация о результате генерации для версии документа.</returns>
    [Public]
    public static finex.CollectionFunctions.Structures.Module.IСonversionToPdfResult SetStampAnchorOnByteArray(Sungero.Docflow.Structures.Module.IByteArray byteArray,
                                                                                                               string signatureMark,
                                                                                                               string anchorSymbol,
                                                                                                               string extension)
    {
      var info = finex.CollectionFunctions.Structures.Module.СonversionToPdfResult.Create();
      info.HasErrors = true;

      if (byteArray == null || byteArray.Bytes == null)
      {
        info.HasConvertionError = true;
        return info;
      }
      
      if (string.IsNullOrEmpty(signatureMark) || string.IsNullOrEmpty(anchorSymbol) || string.IsNullOrEmpty(extension))
      {
        info.HasConvertionError = true;
        info.ErrorMessage = "Не заполнены строковые параметры (штамп или якорь или расширение).";
        return info;
      }

      var bytes = byteArray.Bytes;
      try
      {
        System.IO.Stream pdfDocumentStream = null;
        using (var inputStream = new System.IO.MemoryStream(bytes))
        {
          try
          {

            var pdfConverter = new Sungero.AsposeExtensions.Converter();
            var pdfExtension = Sungero.Docflow.PublicConstants.OfficialDocument.PdfExtension;
            pdfDocumentStream = pdfConverter.GeneratePdf(inputStream, extension);
            
            pdfDocumentStream = pdfConverter.AddSignatureMark(pdfDocumentStream,
                                                              pdfExtension,
                                                              signatureMark,
                                                              anchorSymbol,
                                                              Constants.Module.SearchablePagesLimit);
          }
          catch (Exception e)
          {
            Logger.ErrorFormat("SetStampAnchorOnDocumentVersion: {0}", e.Message);
            info.HasConvertionError = true;
            info.HasLockError = false;
            info.ErrorMessage = string.Format("{0}\r\n{1}", Sungero.Docflow.Resources.DocumentBodyNeedsRepair, e.Message);
          }
        }
        
        if (!string.IsNullOrWhiteSpace(info.ErrorMessage))
          return info;
        
        info.Body = StreamToByteArray(pdfDocumentStream);
        pdfDocumentStream.Close();
        info.HasErrors = false;
      }
      catch (Sungero.Domain.Shared.Exceptions.RepeatedLockException e)
      {
        Logger.Error(e.Message);
        info.HasConvertionError = false;
        info.HasLockError = true;
        info.ErrorMessage = e.Message;
      }
      catch (Exception e)
      {
        Logger.Error(e.Message);
        info.HasConvertionError = true;
        info.HasLockError = false;
        info.ErrorMessage = e.Message;
      }
      
      return info;
    }
    
    /// <summary>
    /// Установить штамп по якорю на всех страницах версии документа
    /// </summary>
    /// <param name="version">Массив байт PDF версии документа.</param>
    /// <param name="signatureMark">Отметка об ЭП в html формате.</param>
    /// <param name="anchorSymbol">Строка якоря.</param>
    /// <param name="extension">Расширение версии документа.</param>
    /// <returns>Информация о результате генерации для версии документа.</returns>
    [Public]
    public static finex.CollectionFunctions.Structures.Module.IСonversionToPdfResult SetStampAnchorAllPagesOnByteArray(Sungero.Docflow.Structures.Module.IByteArray byteArray,
                                                                                                                       string signatureMark,
                                                                                                                       string anchorSymbol,
                                                                                                                       string extension)
    {
      var info = finex.CollectionFunctions.Structures.Module.СonversionToPdfResult.Create();
      info.HasErrors = true;
      
      if (byteArray == null || byteArray.Bytes.Length == 0)
      {
        info.ErrorMessage = "Переданый массив байт пуст.";
        return info;
      }
      
      if (string.IsNullOrEmpty(signatureMark) || string.IsNullOrEmpty(anchorSymbol) || string.IsNullOrEmpty(extension))
      {
        info.HasConvertionError = true;
        info.ErrorMessage = "Не заполнены строковые параметры (штамп или якорь или расширение).";
        return info;
      }
      
      if (Sungero.AsposeExtensions.Converter.CheckIfExtensionIsSupportedForAnchorSearch(extension))
      {
        System.IO.Stream pdfDocumentStream = null;
        try
        {
          using(var inputStream = new System.IO.MemoryStream(byteArray.Bytes))
          {
            var pdfConverter = new Sungero.AsposeExtensions.Converter();
            var positionList = new List<finex.CollectionFunctions.Structures.Module.AnchorPosition>();
            
            //Штамп
            Aspose.Pdf.PdfPageStamp xIndent = pdfConverter.CreateMarkFromHtml(signatureMark);
            
            //Поток с PDF документом
            pdfDocumentStream = pdfConverter.GeneratePdf(inputStream, extension);
            
            //PDF документ
            Aspose.Pdf.Document document = new Aspose.Pdf.Document(pdfDocumentStream);
            
            #region Найдем якоря на всех страницах документа
            int count = document.Pages.Count;
            int num = 1;
            while (count + 1 > num)
            {
              //Страница документа
              Aspose.Pdf.Page item = document.Pages[num];

              //Якорь
              TextFragment lastAnchorEntry = GetLastAnchorEntry(item, anchorSymbol);
              if (lastAnchorEntry != null)
              {
                var position = finex.CollectionFunctions.Structures.Module.AnchorPosition.Create();
                position.XIndent = lastAnchorEntry.Position.XIndent;
                position.YIndent = lastAnchorEntry.Position.YIndent;
                position.RectangleHeight = lastAnchorEntry.Rectangle.Height;
                position.PageNumber = num;
                positionList.Add(position);
              }
              
              num++;
            }
            #endregion
            
            //Установим штамп по найденым якорям
            foreach (var position in positionList)
            {
              xIndent.XIndent = position.XIndent;
              xIndent.YIndent = position.YIndent - xIndent.Height / 2 + position.RectangleHeight / 2;
              xIndent.Background = false;
              pdfDocumentStream = pdfConverter.AddSignatureMarkToDocumentPage(pdfDocumentStream, position.PageNumber, xIndent);
            }
            
            info.Body = StreamToByteArray(pdfDocumentStream);
            info.HasErrors = false;
          }

        }
        catch (Sungero.AsposeExtensions.PdfConvertException pdfConvertException)
        {
          info.ErrorMessage = pdfConvertException.Message;
        }
        catch (Exception exception)
        {
          info.ErrorMessage = string.Format("Cannot add stamp {0}", exception.Message);
        }

        pdfDocumentStream.Close();
      }
      
      return info;
    }
    
    private static TextFragment GetLastAnchorEntry(Aspose.Pdf.Page page, string anchor)
    {
      TextFragmentAbsorber textFragmentAbsorber = new Aspose.Pdf.Text.TextFragmentAbsorber(anchor);
      page.Accept(textFragmentAbsorber);
      if (textFragmentAbsorber.TextFragments.Count == 0)
      {
        return null;
      }
      TextFragment textFragment = new TextFragment();
      textFragment.Position.XIndent = 0;
      textFragment.Position.YIndent = page.Rect.Height;
      foreach (TextFragment textFragment1 in textFragmentAbsorber.TextFragments)
      {
        if (textFragment1.Position.YIndent >= textFragment.Position.YIndent && (textFragment1.Position.YIndent != textFragment.Position.YIndent || textFragment1.Position.XIndent <= textFragment.Position.XIndent))
        {
          continue;
        }
        textFragment = textFragment1;
      }
      return textFragment;
    }
    
    private static Aspose.Pdf.PdfPageStamp CreateMarkFromByte(byte[] byteArray, string extention, double? width, double? height)
    {
      int pageNum = 1;
      Aspose.Pdf.PdfPageStamp pdfPageStamp;
      using (var markStream = new System.IO.MemoryStream(byteArray))
      {
        var pdfConverter = new Sungero.AsposeExtensions.Converter();
        var pdfStream = pdfConverter.GeneratePdf(markStream, extention);
        pdfPageStamp = new Aspose.Pdf.PdfPageStamp(pdfStream, pageNum);
        
        if (width.HasValue || height.HasValue)
        {
          Aspose.Pdf.Rectangle rectangle = null;
          
          if (!width.HasValue || !height.HasValue)
          {
            Aspose.Pdf.Document document = new Aspose.Pdf.Document(pdfStream);
            rectangle = document.Pages[pageNum].CalculateContentBBox();
          }
          
          if (width.HasValue)
            pdfPageStamp.Width = width.Value;
          else
            pdfPageStamp.Width = rectangle.Width;
          
          if (height.HasValue)
            pdfPageStamp.Height = height.Value;
          else
            pdfPageStamp.Height = rectangle.Height;
        }
        
        pdfPageStamp.BottomMargin = 0;
        pdfPageStamp.LeftMargin = 0;
        pdfPageStamp.RightMargin = 0;
        pdfPageStamp.TopMargin = 0;
        
        pdfPageStamp.Background = true;
      }
      
      return pdfPageStamp;
    }
    
    private static Aspose.Pdf.HorizontalAlignment GetHorizontalAlignment(int horizontalAlignment)
    {
      Aspose.Pdf.HorizontalAlignment alignment;
      switch (horizontalAlignment)
      {
        case 0:
          alignment = Aspose.Pdf.HorizontalAlignment.Left;
          break;
        case 1:
          alignment = Aspose.Pdf.HorizontalAlignment.Right;
          break;
        case 2:
          alignment = Aspose.Pdf.HorizontalAlignment.Center;
          break;
        default:
          alignment = Aspose.Pdf.HorizontalAlignment.Justify;
          break;
      }
      
      return alignment;
    }
    
    private static Aspose.Pdf.VerticalAlignment GetVerticalAlignment(int verticalAlignment)
    {
      Aspose.Pdf.VerticalAlignment alignment;
      switch (verticalAlignment)
      {
        case 0:
          alignment = Aspose.Pdf.VerticalAlignment.Top;
          break;
        case 1:
          alignment= Aspose.Pdf.VerticalAlignment.Center;
          break;
        case 2:
          alignment = Aspose.Pdf.VerticalAlignment.Bottom;
          break;
        default:
          alignment = Aspose.Pdf.VerticalAlignment.None;
          break;
      }
      
      return alignment;
    }
    
    #endregion
    
    
    
    #region Преобразование в PDF

    /// <summary>
    /// Преобразовать версию документа в pdf с отметкой об ЭП.
    /// </summary>
    /// <param name="document">Документ для преобразования.</param>
    /// <param name="versionId">Id версии, для генерации (если null, то будет использоваться последняя версия документа).</param>
    /// <param name="signatureType">Тип подписи (не обязательно, если передается signatureMark).</param>
    /// <param name="signatureMark">Отметка об ЭП в html формате (если string.Empty, то будет использоваться базовый штамп).</param>
    /// <param name="isReplaceCurrentVersion">True, если нужно заменить версию документа на pdf, иначе вернуть массив байт.</param>
    /// <param name="isPublicBody">Использовать PublicBody версии, иначе по умолчанию Body.</param>
    /// <returns>Информация о результате генерации PublicBody для версии документа.</returns>
    [Public]
    public static finex.CollectionFunctions.Structures.Module.IСonversionToPdfResult ConvertVersionToPdfWithSignatureMark(Sungero.Docflow.IOfficialDocument document,
                                                                                                                          int? versionId,
                                                                                                                          Sungero.Core.SignatureType signatureType,
                                                                                                                          string signatureMark,
                                                                                                                          bool isReplaceCurrentVersion,
                                                                                                                          bool isPublicBody)
    {
      return ConvertVersionToPdfWithSignatureMark(document, versionId, signatureType, signatureMark, false, 2, 2, isReplaceCurrentVersion, isPublicBody);
    }
    
    /// <summary>
    /// Преобразовать версию документа в pdf с отметкой об ЭП.
    /// </summary>
    /// <param name="document">Документ для преобразования.</param>
    /// <param name="versionId">Id версии, для генерации (если null, то будет использоваться последняя версия документа).</param>
    /// <param name="signatureType">Тип подписи (не обязательно, если передается signatureMark).</param>
    /// <param name="signatureMark">Отметка об ЭП в html формате (если string.Empty, то будет использоваться базовый штамп).</param>
    /// <param name="horizontalAlignment">Горизонтальное положение штампа (0 - по левому краю, 1 - по правому краю, 2 - по центру) .</param>
    /// <param name="verticalAlignment">Вертикальное положение штампа (0 - по верхнему краю, 1 - по центру, 2 - по нижнему краю) .</param>
    /// <param name="isReplaceCurrentVersion">True, если нужно заменить версию документа на pdf, иначе вернуть массив байт.</param>
    /// <param name="isPublicBody">Использовать PublicBody версии, иначе по умолчанию Body.</param>
    /// <returns>Информация о результате генерации PublicBody для версии документа.</returns>
    [Public]
    public static finex.CollectionFunctions.Structures.Module.IСonversionToPdfResult ConvertVersionToPdfWithSignatureMark(Sungero.Docflow.IOfficialDocument document,
                                                                                                                          int? versionId,
                                                                                                                          Sungero.Core.SignatureType signatureType,
                                                                                                                          string signatureMark,
                                                                                                                          int horizontalAlignment,
                                                                                                                          int verticalAlignment,
                                                                                                                          bool isReplaceCurrentVersion,
                                                                                                                          bool isPublicBody)
    {
      return ConvertVersionToPdfWithSignatureMark(document, versionId, signatureType, signatureMark, true, horizontalAlignment, verticalAlignment, isReplaceCurrentVersion, isPublicBody);
    }
    
    /// <summary>
    /// Преобразовать версию документа в pdf с отметкой об ЭП.
    /// </summary>
    /// <param name="document">Документ для преобразования.</param>
    /// <param name="versionId">Id версии, для генерации (если null, то будет использоваться последняя версия документа).</param>
    /// <param name="signatureType">Тип подписи (не обязательно, если передается signatureMark).</param>
    /// <param name="signatureMark">Отметка об ЭП в html формате (если string.Empty, то будет использоваться базовый штамп).</param>
    /// <param name="isAllPages">Ставить штамп на каждой странице.</param>
    /// <param name="horizontalAlignment">Горизонтальное положение штампа (0 - по левому краю, 1 - по правому краю, 2 - по центру).</param>
    /// <param name="verticalAlignment">Вертикальное положение штампа (0 - по верхнему краю, 1 - по центру, 2 - по нижнему краю).</param>
    /// <param name="isReplaceCurrentVersion">True, если нужно заменить версию документа на pdf, иначе вернуть массив байт.</param>
    /// <param name="isPublicBody">Использовать PublicBody версии, иначе по умолчанию Body.</param>
    /// <returns>Информация о результате генерации PublicBody для версии документа.</returns>
    private static finex.CollectionFunctions.Structures.Module.IСonversionToPdfResult ConvertVersionToPdfWithSignatureMark(Sungero.Docflow.IOfficialDocument document,
                                                                                                                           int? versionId,
                                                                                                                           Sungero.Core.SignatureType signatureType,
                                                                                                                           string signatureMark,
                                                                                                                           bool isAllPages,
                                                                                                                           int horizontalAlignment,
                                                                                                                           int verticalAlignment,
                                                                                                                           bool isReplaceCurrentVersion,
                                                                                                                           bool isPublicBody)
    {
      var info = finex.CollectionFunctions.Structures.Module.СonversionToPdfResult.Create();
      info.HasErrors = true;

      if (!document.HasVersions)
      {
        info.HasConvertionError = true;
        info.ErrorMessage = CollectionFunctions.Resources.NotDocumentVersion;
        return info;
      }
      
      if (!versionId.HasValue)
        versionId = document.LastVersion.Id;
      
      var version = document.Versions.SingleOrDefault(v => v.Id == versionId);
      if (version == null)
      {
        info.HasConvertionError = true;
        info.ErrorMessage = Sungero.Docflow.OfficialDocuments.Resources.NoVersionWithNumberErrorFormat(versionId.Value);
        return info;
      }
      
      if (string.IsNullOrEmpty(signatureMark))
        signatureMark = GetSignatureMarkAsHtml(document, versionId.Value, signatureType);
      
      try
      {
        System.IO.Stream pdfDocumentStream = null;
        using (var inputStream = new System.IO.MemoryStream())
        {
          var extension = version.BodyAssociatedApplication.Extension;
          if (isPublicBody)
          {
            if (version.PublicBody.Size > 0)
            {
              version.PublicBody.Read().CopyTo(inputStream);
              extension = version.AssociatedApplication.Extension;
            }
            else
              version.Body.Read().CopyTo(inputStream);
          }
          else
            version.Body.Read().CopyTo(inputStream);
          
          try
          {
            var pdfConverter = new Sungero.AsposeExtensions.Converter();
            pdfDocumentStream = pdfConverter.GeneratePdf(inputStream, extension);
            
            if (!string.IsNullOrEmpty(signatureMark))
            {
              if (isAllPages)
              {
                var htmlStampString = pdfConverter.CreateMarkFromHtml(signatureMark);
                
                htmlStampString.HorizontalAlignment = GetHorizontalAlignment(horizontalAlignment);
                htmlStampString.VerticalAlignment = GetVerticalAlignment(verticalAlignment);
                
                using (Aspose.Pdf.Document aspDocument = pdfConverter.AddStampToDocument(new Aspose.Pdf.Document(pdfDocumentStream), htmlStampString))
                {
                  aspDocument.Save(pdfDocumentStream);
                }
              }
              else
              {
                pdfDocumentStream = pdfConverter.AddSignatureMark(pdfDocumentStream,
                                                                  extension,
                                                                  signatureMark,
                                                                  Sungero.Docflow.Resources.SignatureMarkAnchorSymbol,
                                                                  Constants.Module.SearchablePagesLimit);
              }
            }
          }
          catch (Exception e)
          {
            if (e is Sungero.AsposeExtensions.PdfConvertException)
              Logger.Error(Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), e.InnerException);
            else
              Logger.Error(string.Format("{0} {1}", Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), e.Message));
            
            info.HasConvertionError = true;
            info.HasLockError = false;
            info.ErrorMessage = Sungero.Docflow.Resources.DocumentBodyNeedsRepair;
          }
          
          inputStream.Close();
        }
        
        if (!string.IsNullOrWhiteSpace(info.ErrorMessage))
          return info;
        
        if (isReplaceCurrentVersion)
        {
          version.PublicBody.Write(pdfDocumentStream);
          version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension(Sungero.Docflow.PublicConstants.OfficialDocument.PdfExtension);
          pdfDocumentStream.Close();
          document.Save();
        }
        else
        {
          info.Body = StreamToByteArray(pdfDocumentStream);
          pdfDocumentStream.Close();
        }

        info.HasErrors = false;
      }
      catch (Sungero.Domain.Shared.Exceptions.RepeatedLockException e)
      {
        Logger.Error(e.Message);
        info.HasConvertionError = false;
        info.HasLockError = true;
        info.ErrorMessage = e.Message;
      }
      catch (Exception e)
      {
        Logger.Error(e.Message);
        info.HasConvertionError = true;
        info.HasLockError = false;
        info.ErrorMessage = e.Message;
      }
      
      return info;
    }
    
    /// <summary>
    /// Преобразовать версию документа в pdf (без наложения штампа)
    /// </summary>
    /// <param name="document">Документ для преобразования.</param>
    /// <param name="versionId">Id версии, для генерации (если null, то будет использоваться последняя версия документа).</param>
    /// <param name="isReplaceCurrentVersion">True, если нужно заменить версию документа на pdf, иначе вернуть массив байт.</param>
    /// <param name="isPublicBody">Использовать PublicBody версии, иначе по умолчанию Body.</param>
    /// <returns>Информация о результате генерации PublicBody для версии документа.</returns>
    [Remote, Public]
    public static finex.CollectionFunctions.Structures.Module.IСonversionToPdfResult ConvertVersionToPdf(Sungero.Docflow.IOfficialDocument document,
                                                                                                         int? versionId,
                                                                                                         bool isReplaceCurrentVersion,
                                                                                                         bool isPublicBody)
    {
      var info = finex.CollectionFunctions.Structures.Module.СonversionToPdfResult.Create();
      info.HasErrors = true;

      if (!document.HasVersions)
      {
        info.HasConvertionError = true;
        info.ErrorMessage = CollectionFunctions.Resources.NotDocumentVersion;
        return info;
      }
      
      if (!versionId.HasValue)
        versionId = document.LastVersion.Id;
      
      var version = document.Versions.SingleOrDefault(v => v.Id == versionId);
      if (version == null)
      {
        info.HasConvertionError = true;
        info.ErrorMessage = Sungero.Docflow.OfficialDocuments.Resources.NoVersionWithNumberErrorFormat(versionId.Value);
        return info;
      }
      
      try
      {
        System.IO.Stream pdfDocumentStream = null;
        using (var inputStream = new System.IO.MemoryStream())
        {
          var extension = version.BodyAssociatedApplication.Extension;
          if (isPublicBody)
          {
            if (version.PublicBody.Size > 0)
            {
              version.PublicBody.Read().CopyTo(inputStream);
              extension = version.AssociatedApplication.Extension;
            }
            else
              version.Body.Read().CopyTo(inputStream);
          }
          else
            version.Body.Read().CopyTo(inputStream);

          try
          {
            var pdfConverter = new Sungero.AsposeExtensions.Converter();
            pdfDocumentStream = pdfConverter.GeneratePdf(inputStream, extension);
          }
          catch (Exception e)
          {
            if (e is Sungero.AsposeExtensions.PdfConvertException)
              Logger.Error(Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), e.InnerException);
            else
              Logger.Error(string.Format("{0} {1}", Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), e.Message));
            
            info.HasConvertionError = true;
            info.HasLockError = false;
            info.ErrorMessage = Sungero.Docflow.Resources.DocumentBodyNeedsRepair;
          }
        }
        
        if (!string.IsNullOrWhiteSpace(info.ErrorMessage))
          return info;
        
        if (isReplaceCurrentVersion)
        {
          version.PublicBody.Write(pdfDocumentStream);
          version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension(Sungero.Docflow.PublicConstants.OfficialDocument.PdfExtension);
          pdfDocumentStream.Close();
          document.Save();
        }
        else
        {
          var byteArray = StreamToByteArray(pdfDocumentStream);
          pdfDocumentStream.Close();
          info.Body = byteArray;
        }
        
        info.HasErrors = false;
      }
      catch (Sungero.Domain.Shared.Exceptions.RepeatedLockException e)
      {
        Logger.Error(e.Message);
        info.HasConvertionError = false;
        info.HasLockError = true;
        info.ErrorMessage = e.Message;
      }
      catch (Exception e)
      {
        Logger.Error(e.Message);
        info.HasConvertionError = true;
        info.HasLockError = false;
        info.ErrorMessage = e.Message;
      }
      
      return info;
    }
    
    private static byte[] StreamToByteArray(System.IO.Stream input)
    {
      using (var ms = new MemoryStream())
      {
        input.CopyTo(ms);
        return ms.ToArray();
      }
    }
    
    #endregion
    
    
    
    #region Работа с PDF файлами
    
    /// <summary>
    /// Слить список pdf файлов в один документ
    /// </summary>
    /// <param name="filesPaths">Список путей входных pdf файлов</param>
    /// <returns>Полный путь к сформированному pdf файлу</returns>
    [Public]
    public static string MergePdfDocuments(List<string> filesPaths)
    {
      return MergePdfDocuments(filesPaths, string.Empty, string.Empty);
    }
    
    /// <summary>
    /// Слить список pdf файлов в один документ
    /// </summary>
    /// <param name="filesPaths">Список путей входных pdf файлов</param>
    /// <param name="outputPathFile">Путь для выходного pdf. При string.Empty использует temp папку </param>
    /// <param name="outputPathFile">Имя нового файла. При string.Empty использует Guid</param>
    /// <returns>Полный путь к сформированному pdf файлу</returns>
    [Public]
    public static string MergePdfDocuments(List<string> filesPaths,
                                           string outputPathFile,
                                           string fileName)
    {
      if (string.IsNullOrEmpty(fileName) || string.IsNullOrWhiteSpace(fileName))
        fileName = Guid.NewGuid().ToString();
      
      fileName = string.Format("{0}.{1}", fileName, Sungero.Docflow.PublicConstants.OfficialDocument.PdfExtension);
      
      if (string.IsNullOrEmpty(outputPathFile) || string.IsNullOrWhiteSpace(outputPathFile))
        outputPathFile = System.IO.Path.GetTempPath();
      
      outputPathFile = Path.Combine(outputPathFile, fileName);

      using (PdfDocument mergePdfDocument = new PdfDocument())
      {
        foreach (var filePath in filesPaths)
        {
          using (PdfDocument pdfDocument = PdfSharp.Pdf.IO.PdfReader.Open(filePath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import))
          {
            foreach (var page in pdfDocument.Pages)
              mergePdfDocument.AddPage(page);
            
            pdfDocument.Close();
            pdfDocument.Dispose();
          }
        }
        
        mergePdfDocument.Save(outputPathFile);
        mergePdfDocument.Close();
        mergePdfDocument.Dispose();
      }
      
      return outputPathFile;
    }
    
    #endregion
    
    
    
    #region Рассылка уведомлений в системе
    
    /// <summary>
    /// Отправить уведомление членам роли Администраторы.
    /// </summary>
    /// <param name="subject">Тема.</param>
    /// <param name="text">Текст.</param>
    [Remote, Public]
    public static void SendNotice(string subject,
                                  string text)
    {
      var administrators = Roles.Administrators;
      foreach (var administrator in administrators.RecipientLinks)
      {
        SendNotice(subject, text, administrator.Member, new List<IRecipient> (), null);
      }
    }
    
    /// <summary>
    /// Отправить уведомление членам роли Администраторы.
    /// </summary>
    /// <param name="subject">Тема.</param>
    /// <param name="text">Текст.</param>
    /// <param name="attachment">Вложение.</param>
    [Remote, Public]
    public static void SendNotice(string subject,
                                  string text,
                                  Sungero.Domain.Shared.IEntity attachment)
    {
      var administrators = Roles.Administrators;
      foreach (var administrator in administrators.RecipientLinks)
      {
        SendNotice(subject, text, administrator.Member, new List<IRecipient> (), attachment);
      }
    }
    
    /// <summary>
    /// Отправить уведомление.
    /// </summary>
    /// <param name="subject">Тема.</param>
    /// <param name="text">Текст.</param>
    /// <param name="recipient">Адресат.</param>
    [Remote, Public]
    public static void SendNotice(string subject,
                                  string text,
                                  IRecipient recipient)
    {
      SendNotice(subject, text, recipient, new List<IRecipient> (), null);
    }
    
    /// <summary>
    /// Отправить уведомление.
    /// </summary>
    /// <param name="subject">Тема.</param>
    /// <param name="text">Текст.</param>
    /// <param name="recipient">Адресат.</param>
    /// <param name="attachment">Вложение.</param>
    [Remote, Public]
    public static void SendNotice(string subject,
                                  string text,
                                  IRecipient recipient,
                                  Sungero.Domain.Shared.IEntity attachment)
    {
      SendNotice(subject, text, recipient, new List<IRecipient> (), attachment);
    }
    
    /// <summary>
    /// Отправить уведомление.
    /// </summary>
    /// <param name="subject">Тема.</param>
    /// <param name="text">Текст.</param>
    /// <param name="recipient">Адресат.</param>
    /// <param name="observers">Наблюдатели.</param>
    /// <param name="attachment">Вложение.</param>
    [Remote, Public]
    public static void SendNotice(string subject,
                                  string text,
                                  IRecipient recipient,
                                  List<IRecipient> observers,
                                  Sungero.Domain.Shared.IEntity attachment)
    {
      if (recipient == null)
        return;
      
      if (subject.Length > 250)
        subject = subject.Substring(0, 250);
      
      var task = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, recipient);
      task.ActiveText = text;
      
      if (observers != null && observers.Any())
      {
        foreach (var observer in observers)
          task.Observers.AddNew().Observer = observer;
      }
      
      if (attachment != null)
        task.Attachments.Add(attachment);
      
      try
      {
        task.Save();
        task.Start();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("SendNotice error: {0}", ex.Message);
      }
    }
    
    #endregion
    
    
    
    #region Почтовая рассылка
    
    /// <summary>
    /// Отправить Email письмо по протоколу SMPT.
    /// </summary>
    /// <param name="subject">Тема письма.</param>
    /// <param name="to">Получатель письма.</param>
    /// <param name="cc">Получатели копий письма.</param>
    /// <param name="body">Содержимое письма.</param>
    /// <param name="priority">Приоритет письма (Low, High, если string.Empty, то Normal)</param>
    [Public]
    public void SendSmtpMail(string subject,
                             string to,
                             List<string> cc,
                             string body,
                             string priority)
    {
      if (string.IsNullOrEmpty(to))
        return;
      
      try
      {
        using (var mailClient = new System.Net.Mail.SmtpClient())
        {
          #region нелегал? - получение конфигов (в RX 4+ конфиги SMTP не пробрасываются в .NET)
          var settings = Sungero.Domain.Server.AppSettings.Instance.SmtpClientSettings;
          if (settings != null)
          {
            mailClient.Host = settings.Host;
            mailClient.Port = settings.Port;
            mailClient.EnableSsl = settings.EnableSsl;
            if (!string.IsNullOrEmpty(settings.UserName))
            {
              mailClient.Credentials = new System.Net.NetworkCredential(settings.UserName, settings.Password);
            }
          }
          #endregion
            
          using (var mail = new System.Net.Mail.MailMessage
                 {
                   Body = body,
                   IsBodyHtml = true,
                   Subject = subject.Replace('\r', ' ').Replace('\n', ' '),
                   HeadersEncoding = System.Text.Encoding.UTF8,
                   SubjectEncoding = System.Text.Encoding.UTF8,
                   BodyEncoding = System.Text.Encoding.UTF8
                 })
          {
            #region нелегал? - получение конфигов (в RX 4+ конфиги SMTP не пробрасываются в .NET)
            if (settings != null)
            {
              mail.From = new MailAddress(settings.From, settings.FromDisplayName);
            }
            #endregion
              
            mail.To.Add(to);
            
            foreach (var email in cc)
              mail.CC.Add(email);
            
            if (string.IsNullOrEmpty(priority))
              mail.Priority = System.Net.Mail.MailPriority.Normal;
            else
            {
              if (priority.Trim().ToLower() == "high")
                mail.Priority = System.Net.Mail.MailPriority.High;
              else
                mail.Priority = System.Net.Mail.MailPriority.Low;
            }
            
            mailClient.Send(mail);
          }
        }
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("Error SendSmtpMail: {0}", ex.Message);
      }
    }
    
    #endregion
    
    
    
    #region Пересохранение базовых сущностей (для DrxUtil)
    
    /// <summary>
    /// Пересохранить все записи справочника "Подразделения".
    /// </summary>
    [Remote]
    public static bool ReSaveAllDepartments()
    {
      try
      {
        var departments = Sungero.Company.Departments.GetAll();
        foreach (var department in departments)
        {
          department.Name = department.Name;
          department.Save();
        }
      }
      catch (Exception e)
      {
        Logger.Error(e.Message, e);
        return false;
      }
      
      return true;
    }
    
    /// <summary>
    /// Пересохранить все записи справочника "Сотрудники".
    /// </summary>
    [Remote]
    public static bool ReSaveAllEmployees()
    {
      try
      {
        var employes = Sungero.Company.Employees.GetAll();
        foreach (var employee in employes)
        {
          employee.Name = employee.Name;
          employee.Save();
        }
      }
      catch (Exception e)
      {
        Logger.Error(e.Message, e);
        return false;
      }
      
      return true;
    }

    #endregion

  }
}
