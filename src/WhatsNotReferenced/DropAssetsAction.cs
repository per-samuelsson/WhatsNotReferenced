
using System;

namespace WhatsNotReferenced
{
    public class DropAssetsAction
    {
        readonly UnreferencedAssets assets;

        public DropAssetsAction(UnreferencedAssets unreferenced)
        {
            assets = unreferenced;
        }

        public void Execute()
        {
            foreach (var table in assets.Tables)
            {
                DropTable(table);
            }

            foreach (var column in assets.Columns)
            {
                DropColumn(column);
            }
        }

        static void DropTable(UnreferencedTable table)
        {
            Console.WriteLine($"DROP TABLE {table.FullName}");
        }

        static void DropColumn(UnreferencedColumn column)
        {
            Console.WriteLine($"ALTER TABLE {column.TableName} DROP COLUMN {column.Name}");
        }
    }
}
