using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aurea.Logging;
using Aurea.Maintenance.Debugger.Common;
using Aurea.Maintenance.Debugger.Common.Models;
using CIS.Framework.Data;
using CIS.Framework.Security;

namespace Aurea.Maintenance.Debugger.Common.Extensions
{
	public static class ICopyableExtension
	{
		public static bool CopyEntityFromSaes2Daes<T>(this T entity, string connectionString, string dbPrefix, ILogger logger, bool dryRun) where T :  ICopyableEntity
		{
			logger.Info($"Starting copying of type {entity.ToString()}");
			using (var ts = TransactionFactory.CreateTransactionScope())
			{
				var tableAttribute = entity.GetType().GetCustomAttributesIncludingBaseInterfaces<TableAttribute>().First();
				var relatedAttributes = entity.GetType().GetCustomAttributesIncludingBaseInterfaces<RelatedEntityAttribute>();

				if (CopyChildEntity(entity.GetType(), tableAttribute, relatedAttributes, connectionString,dbPrefix, logger, dryRun))
				{
					ts.Complete();
					return true;
				}

				return false;
			}
		}

		private static bool CopyChildEntity(Type entity, TableAttribute tableAttribute, IEnumerable<RelatedEntityAttribute> relatedAttributes, string connectionString, string dbPrefix, ILogger logger, bool dryRun)
		{
			logger.Info($"Start copying entity {tableAttribute.TableName}");
			foreach (var relatedAttribute in relatedAttributes.Where(x => x.IsRequiredBeforeCopy && !x.RelatedEntity.Equals(entity)))
			{
				var childTableAttribute = relatedAttribute.RelatedEntity.GetCustomAttributesIncludingBaseInterfaces<TableAttribute>().First();
				var childRelatedAttributes = relatedAttribute.RelatedEntity.GetCustomAttributesIncludingBaseInterfaces<RelatedEntityAttribute>();
				if (!CopyChildEntity(relatedAttribute.RelatedEntity, childTableAttribute, childRelatedAttributes, connectionString,dbPrefix, logger, dryRun))
				{
					return false;
				}
			}

			var sql = ConstructCopySql(entity, tableAttribute, relatedAttributes.First(), dbPrefix);
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
					if (!CopyChildEntity(relatedAttribute.RelatedEntity, childTableAttribute, childRelatedAttributes, connectionString, dbPrefix, logger,
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

		private static string ConstructCopySql(Type entity, TableAttribute tableAttribute, RelatedEntityAttribute relatedAttribute, string dbPrefix)
		{
			
			var fields = entity.GetProperties();
		    var fieldNames = string.Join(", ", fields.Select(x => $"[{x.Name}]").ToArray());
			var sql = new StringBuilder();
			if (tableAttribute.HasIdentity)
			{
				sql.AppendLine($"SET IDENTITY_INSERT [daes_{dbPrefix}].[{tableAttribute.TableSchema}].[{tableAttribute.TableName}] ON ");
			}

			sql.AppendLine($@"INSERT INTO [daes_{dbPrefix}].[{tableAttribute.TableSchema}].[{tableAttribute.TableName}] ({fieldNames}) ");
		    sql.AppendLine($@"SELECT {fieldNames} FROM [saes_{dbPrefix}].[{tableAttribute.TableSchema}].[{tableAttribute.TableName}] src ");
		    sql.AppendLine($@"WHERE {tableAttribute.PrimaryKey} = {relatedAttribute.EntityField}  ");
		    sql.AppendLine($@"AND NOT EXISTS(SELECT 1 FROM [daes_{dbPrefix}].[{tableAttribute.TableSchema}].[{tableAttribute.TableName}] dst WHERE dst.{tableAttribute.PrimaryKey} = src.{tableAttribute.PrimaryKey}");
		    if (tableAttribute.HasIdentity)
		    {
		        sql.AppendLine($"SET IDENTITY_INSERT [daes_{dbPrefix}].[{tableAttribute.TableSchema}].[{tableAttribute.TableName}] OFF ");
		    }

			return sql.ToString();
		}

	}
}
