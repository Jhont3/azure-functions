using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp2.Models
{
    public class TaskModel
    {
        public string Name { get; set; }
        public TaskModel(string name)
        {
            Name = name;
        }

    }

}
