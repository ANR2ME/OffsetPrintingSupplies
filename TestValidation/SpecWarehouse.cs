using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel;
using NSpec;
using Service.Service;
using Core.Interface.Service;
using Data.Context;
using System.Data.Entity;
using Data.Repository;
using Validation.Validation;

namespace TestValidation
{

    public class SpecWarehouse: nspec
    {
        DataBuilder d;
        
        void before_each()
        {
            var db = new OffsetPrintingSuppliesEntities();
            using (db)
            {
                db.DeleteAllTables();
                d = new DataBuilder();
                d.PopulateWarehouse();
                d.PopulateItem();
            }
        }

        void data_validation()
        {
            it["validates_data"] = () =>
            {
                d.localWarehouse.Errors.Count().should_be(0);
                d.movingWarehouse.Errors.Count().should_be(0);
                IList<WarehouseItem> localitems = d._warehouseItemService.GetObjectsByWarehouseId(d.localWarehouse.Id);
                IList<WarehouseItem> movingitems = d._warehouseItemService.GetObjectsByWarehouseId(d.movingWarehouse.Id);
                int totalitem = d._itemService.GetAll().ToList().Count();
                localitems.Count().should_be(totalitem);
                movingitems.Count().should_be(totalitem);
            };
        }
    }
}