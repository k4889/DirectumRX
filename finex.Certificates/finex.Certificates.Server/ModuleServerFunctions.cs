using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace finex.Certificates.Server
{
	public class ModuleFunctions
	{
		
		/// <summary>
		/// Закрыть сертификат
		/// </summary>
		/// <param name="certificateId">ИД сертификата</param>
		[Remote(IsPure = true), Public]
		public void CloseCertificateAsyncHandler(int certificateId)
		{
			var handler = AsyncHandlers.CloseCertificate.Create();
			handler.certificateId = certificateId;
			handler.ExecuteAsync();
		}
		
		/// <summary>
		/// Проверить действующие сертификаты текущего пользователя
		/// </summary>
		/// <returns>Список сертификатов</returns>
		[Remote(IsPure = true), Public]
		public List<Sungero.CoreEntities.ICertificate> GetActiveCertificate()
		{
			return Sungero.CoreEntities.Certificates.GetAll()
				.Where(c => Equals(c.Owner, Sungero.CoreEntities.Users.Current))
				.Where(c => c.Enabled == true)
				//Mishin можно заменить на
				//.Where(c => !c.NotAfter.HasValue || c.NotAfter >= Calendar.Today) - работает аналогично
				.Where(c => (!c.NotAfter.HasValue || (c.NotAfter.HasValue && c.NotAfter >= Calendar.Today)))
				.ToList();
		}
		
		/// <summary>
		/// Проверить действующие сертификаты текущего пользователя с учетом срока мониторинга
		/// </summary>
		/// <returns>Список сертификатов</returns>
		[Remote(IsPure = true), Public]
		public List<Sungero.CoreEntities.ICertificate> GetActiveCertificateByMonitoringPeriod()
		{
			var monitoringPeriod = finex.EditableConstants.PublicFunctions.Module.Remote.GetValueIntByName("CertificateMonitoringPeriod", false);
			
			if (!monitoringPeriod.HasValue)
				monitoringPeriod = 14;
			
			if (monitoringPeriod.Value < 1)
				monitoringPeriod = 0;
			
			var beginingDate = Calendar.Today.AddDays(monitoringPeriod.Value);
			
			return Sungero.CoreEntities.Certificates.GetAll()
				.Where(c => Equals(c.Owner, Sungero.CoreEntities.Users.Current))
				.Where(c => c.Enabled == true)
				//Mishin можно заменить на
				//.Where(c => !c.NotAfter.HasValue || c.NotAfter > beginingDate) - работает аналогично
				.Where(c => (!c.NotAfter.HasValue || (c.NotAfter.HasValue && c.NotAfter > beginingDate)))
				.ToList();
		}
		
		/// <summary>
		/// Проверить наличие копии сертификата у текущего пользователя
		/// </summary>
		/// <param name="byteArray">Структура с массивом байт</param>
		/// <returns>True, если найдена копия сертификата, иначе False</returns>
		[Remote(IsPure = true), Public]
		public bool CheckCopyCertificate(Sungero.Docflow.Structures.Module.IByteArray byteArray)
		{
			if (byteArray == null || byteArray.Bytes == null)
				return false;
			
			System.Security.Cryptography.X509Certificates.X509Certificate2 x509Certificate2;
			x509Certificate2 = new System.Security.Cryptography.X509Certificates.X509Certificate2(byteArray.Bytes);

			return Sungero.CoreEntities.Certificates.GetAll()
				.Any(c => Equals(c.Owner, Sungero.CoreEntities.Users.Current)
				     && c.Thumbprint == x509Certificate2.Thumbprint);
		}
		
		/// <summary>
		/// Создать сертификат для текущего пользователя из массива байт
		/// </summary>
		/// <param name="byteArray">Структура с массивом байт</param>
		/// <param name="password">Пароль сертификата</param>
		/// <returns>Сертификат</returns>
		[Remote(IsPure = true), Public]
		public Sungero.CoreEntities.ICertificate CreateCertificate(Sungero.Docflow.Structures.Module.IByteArray byteArray,
		                                                           string password)
		{
			if (byteArray == null || byteArray.Bytes == null)
				return Sungero.CoreEntities.Certificates.Null;
			
			var certificate = Sungero.CoreEntities.Certificates.Create();
			
			//Владелец
			certificate.Owner = Sungero.CoreEntities.Users.Current;
			
			System.Security.Cryptography.X509Certificates.X509Certificate2 x509Certificate2;
			
			//Mishin здесь бы наверное подошел тренарник
			if (string.IsNullOrEmpty(password))
				x509Certificate2 = new System.Security.Cryptography.X509Certificates.X509Certificate2(byteArray.Bytes);
			else
				x509Certificate2 = new System.Security.Cryptography.X509Certificates.X509Certificate2(byteArray.Bytes, CommonLibrary.StringUtils.ToSecureString(password));
			
			var list = (new Sungero.Cryptography.CertificateValidator(x509Certificate2)).ValidationErrors.ToList();
			if (list.Any())
				throw new Exception(string.Format("{0}", string.Join(Environment.NewLine, list)));
			
			//Массив байт сертификата
			certificate.X509Certificate = x509Certificate2.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Cert);
			
			//Отпечаток сертификата
			certificate.Thumbprint = x509Certificate2.Thumbprint;
			
			//Кем выдан
			certificate.Issuer = x509Certificate2.GetNameInfo(System.Security.Cryptography.X509Certificates.X509NameType.SimpleName, true);
			
			//Выдан по
			certificate.NotAfter = new DateTime?(Calendar.FromUtcTime(x509Certificate2.NotAfter.ToUniversalTime()));
			
			//Выдан с
			certificate.NotBefore = new DateTime?(Calendar.FromUtcTime(x509Certificate2.NotBefore.ToUniversalTime()));
			
			//Тема
			certificate.Subject = x509Certificate2.GetNameInfo(System.Security.Cryptography.X509Certificates.X509NameType.SimpleName, false);
			
			certificate.Save();
			
			return certificate;
		}
		
	}
}