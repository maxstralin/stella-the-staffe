using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StellaTheStaffe.Models
{
    public class Paging
    {
        public Paging(Cursors cursors, string? next, string? previous)
        {
            Cursors = cursors;
            Next = next;
            Previous = previous;
        }
        public Cursors Cursors { get; init; }
        public string? Next { get; init; }
        public string? Previous { get; init; }
    }
}
