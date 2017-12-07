using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurea.Logging;
using Aurea.Maintenance.Debugger.Common;
using Aurea.Maintenance.Debugger.Spark.Models;
using CIS.Framework.Data;

namespace Aurea.Maintenance.Debugger.Spark.Extensions
{
    public static class ICopyableExtension
    {
        public static bool CopyEntity<T>(this T entity, string connectionString, ILogger logger, bool dryRun) where T :  ICopyableEntity
        {
            logger.Info($"Starting copying of type {entity.ToString()}");
            using (var ts = TransactionFactory.CreateTransactionScope())
            {
                var tableAttribute = entity.GetType().GetCustomAttributesIncludingBaseInterfaces<TableAttribute>().First();
                var relatedAttributes = entity.GetType().GetCustomAttributesIncludingBaseInterfaces<RelatedEntityAttribute>();

                if (CopyChildEntity(entity, tableAttribute, relatedAttributes, connectionString, logger, dryRun))
                {
                    ts.Complete();
                    return true;
                }

                return false;
            }
        }

        private static bool CopyChildEntity<T>(T entity, TableAttribute tableAttribute, IEnumerable<RelatedEntityAttribute> relatedAttributes, string connectionString, ILogger logger, bool dryRun)
        {
            logger.Info($"Start copying table {tableAttribute.TableName}");
            foreach (var relatedAttribute in relatedAttributes)
            {
                if (relatedAttribute.IsRequiredBeforeCopy)
                {
                    var childTableAttribute = relatedAttribute.RelatedEntity.GetCustomAttributesIncludingBaseInterfaces<TableAttribute>().First();
                    var childRelatedAttributes = relatedAttribute.RelatedEntity.GetCustomAttributesIncludingBaseInterfaces<RelatedEntityAttribute>();
                    if (!CopyChildEntity(relatedAttribute.RelatedEntity, childTableAttribute, childRelatedAttributes, connectionString, logger, dryRun))
                    {
                        return false;
                    }
                }
            }

            var sql = ConstructCopySql();
            try
            {
                if (!dryRun)
                {
                    SqlHelper.ExecuteNonQuery(SqlHelper.CreateCommand(connectionString, sql, CommandType.Text));
                }
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error occurred while copying data of {tableAttribute.TableName}");
                return false;
            }
        }

        private static string ConstructCopySql()
        {
            return "";
        }

    }
}
