﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel
{
    /*
     * Created using Table per Type
     * http://weblogs.asp.net/manavi/inheritance-mapping-strategies-with-entity-framework-code-first-ctp5-part-2-table-per-type-tpt
     */

    public abstract partial class AbstractItem
    {
        public int Id { get; set; }
        public int ItemTypeId { get; set; }
        public string Sku { get; set; }

        public string Name { get; set; }
        public string Category { get; set; }
        public string UoM { get; set; }
        public int Quantity { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public Nullable<DateTime> UpdatedAt { get; set; }
        public Nullable<DateTime> DeletedAt { get; set; }
        public Dictionary<string, string> Errors { get; set; }
        public virtual ItemType ItemType { get; set; }
    }
}