using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurea.Logging;
using Aurea.Maintenance.Debugger.Common;
using Aurea.Maintenance.Debugger.Common.Models;
using CIS.Framework.Data;

namespace Aurea.Maintenance.Debugger.Common.Extensions
{
	public static class ICopyableExtension
	{
		public static bool CopyEntityFromSaes2Daes<T>(this T entity, string connectionString, ILogger logger, bool dryRun) where T :  ICopyableEntity
		{
			logger.Info($"Starting copying of type {entity.ToString()}");
			using (var ts = TransactionFactory.CreateTransactionScope())
			{
				var tableAttribute = entity.GetType().GetCustomAttributesIncludingBaseInterfaces<TableAttribute>().First();
				var relatedAttributes = entity.GetType().GetCustomAttributesIncludingBaseInterfaces<RelatedEntityAttribute>();

				if (CopyChildEntity(entity.GetType(), tableAttribute, relatedAttributes, connectionString, logger, dryRun))
				{
					ts.Complete();
					return true;
				}

				return false;
			}
		}

		private static bool CopyChildEntity(Type entity, TableAttribute tableAttribute, IEnumerable<RelatedEntityAttribute> relatedAttributes, string connectionString, ILogger logger, bool dryRun)
		{
			logger.Info($"Start copying entity {tableAttribute.TableName}");
			foreach (var relatedAttribute in relatedAttributes.Where(x => x.IsRequiredBeforeCopy && !x.RelatedEntity.Equals(entity)))
			{
				var childTableAttribute = relatedAttribute.RelatedEntity.GetCustomAttributesIncludingBaseInterfaces<TableAttribute>().First();
				var childRelatedAttributes = relatedAttribute.RelatedEntity.GetCustomAttributesIncludingBaseInterfaces<RelatedEntityAttribute>();
				if (!CopyChildEntity(relatedAttribute.RelatedEntity, childTableAttribute, childRelatedAttributes, connectionString, logger, dryRun))
				{
					return false;
				}
			}

			var sql = ConstructCopySql(entity, tableAttribute);
			try
			{
				if (!dryRun)
				{
					SqlHelper.ExecuteNonQuery(SqlHelper.CreateCommand(connectionString, sql, CommandType.Text));
				}

				foreach (var relatedAttribute in relatedAttributes.Where(x => !x.IsRequiredBeforeCopy && !x.RelatedEntity.Equals(entity)))
				{
					var childTableAttribute = relatedAttribute.RelatedEntity.GetCustomAttributesIncludingBaseInterfaces<TableAttribute>().First();
					var childRelatedAttributes = relatedAttribute.RelatedEntity.GetCustomAttributesIncludingBaseInterfaces<RelatedEntityAttribute>();
					if (!CopyChildEntity(relatedAttribute.RelatedEntity, childTableAttribute, childRelatedAttributes, connectionString, logger,
						dryRun))
					{
						return false;
					}
				}

				return true;
			}
			catch (Exception e)
			{
				logger.Error(e, $"Error occurred while copying data of {tableAttribute.TableName}");
				return false;
			}
		}

		private static string ConstructCopySql(Type entity, TableAttribute tableAttribute)
		{
			var fields = entity.GetProperties();
		    var sql = new StringBuilder();
		    if (tableAttribute.HasIdentity)
		    {
		        sql.Append($"SET IDENTITY_INSERT daes_Spark.{tableAttribute.TableSchema}.{tableAttribute.TableName} ON");
		    }
			var sqls = @"PRINT 'Copy Addresses'
SET IDENTITY_INSERT daes_Spark..Address ON

INSERT INTO daes_Spark..Address
([AddrID], [ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion])
SELECT
  [AddrID], [ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion] 
FROM  saes_Spark..Address src
WHERE AddrID IN (
	SELECT SiteAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
	UNION
	SELECT MailAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
	UNION
	SELECT CorrAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
	UNION
	SELECT AddrId FROM saes_Spark..Premise WHERE CustID = @CustID
	UNION
	SELECT AddrId FROM saes_Spark..Meter WHERE PremID IN (SELECT PremID FROM saes_Spark..Premise WHERE CustId = @CustId)
)
AND NOT EXISTS (SELECT 1 FROM daes_Spark..Address dst WHERE src.AddrID = dst.AddrID)";

			return sql.ToString();
		}

	}
}
