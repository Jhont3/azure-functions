using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp2.Models
{
    public class TaskModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public TaskModel(int id, string name)
        {
            Id = id;
            Name = name;
        }

    }

}
