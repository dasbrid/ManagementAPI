using System;
using System.Collections.Generic;

namespace TodoApi.Models
{
    public class Token
    {
		public int Id { get; set; }
		public Guid TokenString { get; set; }
		public string TokenName { get; set; }
		public ICollection<Operation> Operations { get; set; }
    }
}