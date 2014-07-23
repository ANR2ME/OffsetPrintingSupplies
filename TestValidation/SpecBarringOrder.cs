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

    public class SpecBarringOrder: nspec
    {
        DataBuilder d;
        
        void before_each()
        {
            var db = new OffsetPrintingSuppliesEntities();
            using (db)
            {
                db.DeleteAllTables();
                d = new DataBuilder();
                d.PopulateData();
            }
        }

        void data_validation()
        {
        
            it["validates_data"] = () =>
            {
                d.bargeneric.Errors.Count().should_be(0);
                d.barleft1.Errors.Count().should_be(0);
                d.barleft2.Errors.Count().should_be(0);
                d.barright1.Errors.Count().should_be(0);
                d.barright2.Errors.Count().should_be(0);
                d.blanket1.Errors.Count().should_be(0);
                d.blanket2.Errors.Count().should_be(0);
                d.blanket3.Errors.Count().should_be(0);
                d.barringOrderCustomer.Errors.Count().should_be(0);
                d.barringODCustomer1.Errors.Count().should_be(0);
                d.barringODCustomer2.Errors.Count().should_be(0);
                d.barringODCustomer3.Errors.Count().should_be(0);
                d.barringODCustomer4.Errors.Count().should_be(0);
            };

            it["deletes_barringorder"] = () =>
            {
                d.barringOrderCustomer = d._barringOrderService.SoftDeleteObject(d.barringOrderCustomer, d._barringOrderDetailService);
                d.barringOrderCustomer.Errors.Count().should_be(0);
            };

            it["confirms_barringorder"] = () =>
            {
                d.barringOrderCustomer = d._barringOrderService.ConfirmObject(d.barringOrderCustomer, d._barringOrderDetailService, d._barringService, d._itemService, d._warehouseItemService);
                d.barringOrderCustomer.IsConfirmed.should_be(true);
                d.barringOrderCustomer.Errors.Count().should_be(0);
            };

            it["unconfirms_barring_order"] = () =>
            {
                d.barringOrderCustomer = d._barringOrderService.ConfirmObject(d.barringOrderCustomer, d._barringOrderDetailService, d._barringService, d._itemService, d._warehouseItemService);
                d.barringOrderCustomer.IsConfirmed.should_be(true);
                d.barringOrderCustomer = d._barringOrderService.UnconfirmObject(d.barringOrderCustomer, d._barringOrderDetailService, d._barringService, d._itemService, d._warehouseItemService);
                d.barringOrderCustomer.IsConfirmed.should_be(false);
                d.barringOrderCustomer.Errors.Count().should_be(0);
            };

            it["deletes_barringorder_with_processed_detail"] = () =>
            {
                d.barringOrderCustomer = d._barringOrderService.ConfirmObject(d.barringOrderCustomer, d._barringOrderDetailService, d._barringService, d._itemService, d._warehouseItemService);
                d.barringODCustomer1 = d._barringOrderDetailService.CutObject(d.barringODCustomer1);
                d.barringODCustomer1 = d._barringOrderDetailService.SideSealObject(d.barringODCustomer1);

                d.barringOrderCustomer = d._barringOrderService.SoftDeleteObject(d.barringOrderCustomer, d._barringOrderDetailService);
                d.barringOrderCustomer.Errors.Count().should_not_be(0);
            };

            context["when barring order details are cut"] = () =>
            {
                before = () =>
                {
                    d.barringOrderCustomer = d._barringOrderService.ConfirmObject(d.barringOrderCustomer, d._barringOrderDetailService, d._barringService, d._itemService, d._warehouseItemService);

                    d._barringOrderDetailService.CutObject(d.barringODCustomer1);
                    d._barringOrderDetailService.CutObject(d.barringODCustomer2);
                    d._barringOrderDetailService.CutObject(d.barringODCustomer3);
                    d._barringOrderDetailService.CutObject(d.barringODCustomer4);
                };

                it["validates_barringorderdetails"] = () =>
                {
                    d.barringODCustomer1.Errors.Count().should_be(0);
                    d.barringODCustomer2.Errors.Count().should_be(0);
                    d.barringODCustomer3.Errors.Count().should_be(0);
                    d.barringODCustomer4.Errors.Count().should_be(0);
                };

                it["validates_unconfirmbarringorder"] = () =>
                {
                    d.barringOrderCustomer = d._barringOrderService.UnconfirmObject(d.barringOrderCustomer, d._barringOrderDetailService, d._barringService, d._itemService, d._warehouseItemService);
                    d.barringOrderCustomer.IsConfirmed.should_be(true);
                    d.barringOrderCustomer.Errors.Count().should_not_be(0);
                };

                context["when barring order details are all finished"] = () =>
                {
                    before = () =>
                    {
                        d._barringOrderDetailService.SideSealObject(d.barringODCustomer1);
                        d._barringOrderDetailService.PrepareObject(d.barringODCustomer1);
                        d._barringOrderDetailService.ApplyTapeAdhesiveToObject(d.barringODCustomer1);
                        d._barringOrderDetailService.MountObject(d.barringODCustomer1);
                        d._barringOrderDetailService.HeatPressObject(d.barringODCustomer1);
                        d._barringOrderDetailService.PullOffTestObject(d.barringODCustomer1);
                        d._barringOrderDetailService.QCAndMarkObject(d.barringODCustomer1);
                        d._barringOrderDetailService.PackageObject(d.barringODCustomer1);
                        d._barringOrderDetailService.AddLeftBar(d.barringODCustomer1, d._barringService);
                        d._barringOrderDetailService.AddRightBar(d.barringODCustomer1, d._barringService);

                        d._barringOrderDetailService.SideSealObject(d.barringODCustomer2);
                        d._barringOrderDetailService.PrepareObject(d.barringODCustomer2);
                        d._barringOrderDetailService.ApplyTapeAdhesiveToObject(d.barringODCustomer2);
                        d._barringOrderDetailService.MountObject(d.barringODCustomer2);
                        d._barringOrderDetailService.HeatPressObject(d.barringODCustomer2);
                        d._barringOrderDetailService.PullOffTestObject(d.barringODCustomer2);
                        d._barringOrderDetailService.QCAndMarkObject(d.barringODCustomer2);
                        d._barringOrderDetailService.PackageObject(d.barringODCustomer2);
                        d._barringOrderDetailService.AddLeftBar(d.barringODCustomer2, d._barringService);
                        d._barringOrderDetailService.AddRightBar(d.barringODCustomer2, d._barringService);

                        d._barringOrderDetailService.SideSealObject(d.barringODCustomer3);
                        d._barringOrderDetailService.PrepareObject(d.barringODCustomer3);
                        d._barringOrderDetailService.ApplyTapeAdhesiveToObject(d.barringODCustomer3);
                        d._barringOrderDetailService.MountObject(d.barringODCustomer3);
                        d._barringOrderDetailService.HeatPressObject(d.barringODCustomer3);
                        d._barringOrderDetailService.PullOffTestObject(d.barringODCustomer3);
                        d._barringOrderDetailService.QCAndMarkObject(d.barringODCustomer3);
                        d._barringOrderDetailService.PackageObject(d.barringODCustomer3);
                        d._barringOrderDetailService.AddLeftBar(d.barringODCustomer3, d._barringService);
                        d._barringOrderDetailService.AddRightBar(d.barringODCustomer3, d._barringService);

                        d._barringOrderDetailService.SideSealObject(d.barringODCustomer4);
                        d._barringOrderDetailService.PrepareObject(d.barringODCustomer4);
                        d._barringOrderDetailService.ApplyTapeAdhesiveToObject(d.barringODCustomer4);
                        d._barringOrderDetailService.MountObject(d.barringODCustomer4);
                        d._barringOrderDetailService.HeatPressObject(d.barringODCustomer4);
                        d._barringOrderDetailService.PullOffTestObject(d.barringODCustomer4);
                        d._barringOrderDetailService.QCAndMarkObject(d.barringODCustomer4);
                        d._barringOrderDetailService.AddLeftBar(d.barringODCustomer4, d._barringService);
                    };

                    it["validates_barringorderdetails"] = () =>
                    {
                        d.barringODCustomer1.Errors.Count().should_be(0);
                        d.barringODCustomer2.Errors.Count().should_be(0);
                        d.barringODCustomer3.Errors.Count().should_be(0);
                        d.barringODCustomer4.Errors.Count().should_be(0);
                    };

                    it["validates_finishbarringorder"] = () =>
                    {
                        int blanket1quantity = d.blanket1.Quantity;
                        int blanket2quantity = d.blanket2.Quantity;
                        int bargenericquantity = d.bargeneric.Quantity;
                        int barleft1quantity = d.barleft1.Quantity;
                        int barright1quantity = d.barright1.Quantity;
                        int barring1quantity = d.barring1.Quantity;
                        int barring2quantity = d.barring2.Quantity;
                        int barring1warehousequantity = d._warehouseItemService.FindOrCreateObject(d.localWarehouse.Id, d.barring1.Id).Quantity;
                        int barring2warehousequantity = d._warehouseItemService.FindOrCreateObject(d.localWarehouse.Id, d.barring2.Id).Quantity;
                        d._barringOrderDetailService.FinishObject(d.barringODCustomer1, d._barringOrderService, d._stockMutationService, d._barringService, d._itemService, d._warehouseItemService);
                        d._barringOrderDetailService.FinishObject(d.barringODCustomer2, d._barringOrderService, d._stockMutationService, d._barringService, d._itemService, d._warehouseItemService);
                        d._barringOrderDetailService.FinishObject(d.barringODCustomer3, d._barringOrderService, d._stockMutationService, d._barringService, d._itemService, d._warehouseItemService);
                        d._barringOrderDetailService.RejectObject(d.barringODCustomer4, d._barringOrderService, d._stockMutationService, d._barringService, d._itemService, d._warehouseItemService);
                        int blanket1quantityfinal = d.blanket1.Quantity;
                        int blanket2quantityfinal = d.blanket2.Quantity;
                        int bargenericquantityfinal = d.bargeneric.Quantity;
                        int barleft1quantityfinal = d.barleft1.Quantity;
                        int barright1quantityfinal = d.barright1.Quantity;
                        blanket1quantityfinal.should_be(blanket1quantity - 2);
                        blanket2quantityfinal.should_be(blanket2quantity - 2);
                        bargenericquantityfinal.should_be(bargenericquantity - 4);
                        barleft1quantityfinal.should_be(barleft1quantity - 2);
                        barright1quantityfinal.should_be(barright1quantity - 1);
                        d.barringOrderCustomer.IsCompleted.should_be(true);
                        int barring1quantityfinal = d.barring1.Quantity;
                        int barring2quantityfinal = d.barring2.Quantity;
                        int barring1warehousequantityfinal = d._warehouseItemService.FindOrCreateObject(d.localWarehouse.Id, d.barring1.Id).Quantity;
                        int barring2warehousequantityfinal = d._warehouseItemService.FindOrCreateObject(d.localWarehouse.Id, d.barring2.Id).Quantity;
                        barring1quantityfinal.should_be(barring1quantity + 2);
                        barring2quantityfinal.should_be(barring2quantity + 1);
                        barring1warehousequantityfinal.should_be(barring1warehousequantity + 2);
                        barring2warehousequantityfinal.should_be(barring2warehousequantity + 1);
                        d.barringOrderCustomer.QuantityFinal.should_be(d.barringOrderCustomer.QuantityReceived - d.barringOrderCustomer.QuantityRejected);
                    };
                };
            };
        }
    }
}