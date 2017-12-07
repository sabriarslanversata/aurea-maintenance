namespace Aurea.Maintenance.Debugger.Common.Models
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class TableAttribute : System.Attribute
    {
        public string TableName;
        public string PrimaryKey;
        public string TableSchema;
        public bool HasIdentity;

        public TableAttribute(string tableName)
        {
            this.TableName = tableName;
            this.TableSchema = "dbo";
            this.PrimaryKey = $"{tableName}ID";
            this.HasIdentity = true;
        }
    }
}
