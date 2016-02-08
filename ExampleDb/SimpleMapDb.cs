using System.Data;
using System.Windows.Forms;

namespace ProgramMain.ExampleDb
{

    public partial class SimpleMapDb
    {

        public static BindingSource CreateDataSource(DataTable table)
        {
            var bindingSource = new BindingSource {DataSource = table.DataSet, DataMember = table.TableName};

            return bindingSource;            
        }
    }
}
