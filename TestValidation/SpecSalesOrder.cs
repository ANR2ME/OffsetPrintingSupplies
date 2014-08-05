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

    public class SpecSalesOrder : nspec
    {
        Customer customer;
        Item item_sepatubola;
        Item item_batiktulis;
        SalesOrder salesOrder;
        SalesOrderDetail salesOrderDetail1;
        SalesOrderDetail salesOrderDetail2;
        UoM Pcs;
        ItemType type;
        Warehouse warehouse;
        ICustomerService _customerService;
        IItemService _itemService;
        ISalesOrderService _salesOrderService;
        ISalesOrderDetailService _salesOrderDetailService;
        IDeliveryOrderService _deliveryOrderService;
        IDeliveryOrderDetailService _deliveryOrderDetailService;
        IStockMutationService _stockMutationService;
        IUoMService _uomService;
        IBarringService _barringService;
        IItemTypeService _itemTypeService;
        IWarehouseItemService _warehouseItemService;
        IWarehouseService _warehouseService;
        int Quantity1;
        int Quantity2;

        void before_each()
        {
            var db = new OffsetPrintingSuppliesEntities();
            using (db)
            {
                db.DeleteAllTables();
                _customerService = new CustomerService(new CustomerRepository(), new CustomerValidator());
                _itemService = new ItemService(new ItemRepository(), new ItemValidator());
                _salesOrderService = new SalesOrderService(new SalesOrderRepository(), new SalesOrderValidator());
                _salesOrderDetailService = new SalesOrderDetailService(new SalesOrderDetailRepository(), new SalesOrderDetailValidator());
                _deliveryOrderService = new DeliveryOrderService(new DeliveryOrderRepository(), new DeliveryOrderValidator());
                _deliveryOrderDetailService = new DeliveryOrderDetailService(new DeliveryOrderDetailRepository(), new DeliveryOrderDetailValidator());
                _stockMutationService = new StockMutationService(new StockMutationRepository(), new StockMutationValidator());
                _itemTypeService = new ItemTypeService(new ItemTypeRepository(), new ItemTypeValidator());
                _uomService = new UoMService(new UoMRepository(), new UoMValidator());
                _warehouseItemService = new WarehouseItemService(new WarehouseItemRepository(), new WarehouseItemValidator());
                _warehouseService = new WarehouseService(new WarehouseRepository(), new WarehouseValidator());
                _barringService = new BarringService(new BarringRepository(), new BarringValidator());

                Pcs = new UoM()
                {
                    Name = "Pcs"
                };
                _uomService.CreateObject(Pcs);

                customer = new Customer()
                {
                    Name = "President of Indonesia",
                    Address = "Istana Negara Jl. Veteran No. 16 Jakarta Pusat",
                    CustomerNo = "021 3863777",
                    PIC = "Mr. President",
                    PICCustomerNo = "021 3863777",
                    Email = "random@ri.gov.au"
                };
                customer = _customerService.CreateObject(customer);

                type = _itemTypeService.CreateObject("Item", "Item");

                warehouse = new Warehouse()
                {
                    Name = "Sentral Solusi Data",
                    Description = "Kali Besar Jakarta",
                    IsMovingWarehouse = false,
                    Code = "LCL"
                };
                warehouse = _warehouseService.CreateObject(warehouse, _warehouseItemService, _itemService);

                item_batiktulis = new Item()
                {
                    ItemTypeId = _itemTypeService.GetObjectByName("Item").Id,
                    Name = "Batik Tulis",
                    Category = "Item",
                    Sku = "bt123",
                    UoMId = Pcs.Id
                };
                _itemService.CreateObject(item_batiktulis, _uomService, _itemTypeService, _warehouseItemService, _warehouseService);
                _itemService.AdjustQuantity(item_batiktulis, 1000);
                _warehouseItemService.AdjustQuantity(_warehouseItemService.FindOrCreateObject(warehouse.Id, item_batiktulis.Id), 1000);

                item_sepatubola = new Item()
                {
                    ItemTypeId = _itemTypeService.GetObjectByName("Item").Id,
                    Name = "Sepatu Bola",
                    Category = "Item",
                    Sku = "sb123",
                    UoMId = Pcs.Id
                };
                _itemService.CreateObject(item_sepatubola, _uomService, _itemTypeService, _warehouseItemService, _warehouseService);
                _itemService.AdjustQuantity(item_sepatubola, 1000);
                _warehouseItemService.AdjustQuantity(_warehouseItemService.FindOrCreateObject(warehouse.Id, item_sepatubola.Id), 1000);
            }
        }

        void salesorder_validation()
        {
            it["validate_customer_and_items"] = () =>
            {
                customer.Errors.Count().should_be(0);
                item_batiktulis.Errors.Count().should_be(0);
                item_sepatubola.Errors.Count().should_be(0);
            };

            it["create_salesorder"] = () =>
            {
                salesOrder = _salesOrderService.CreateObject(customer.Id, new DateTime(2000, 1, 1), _customerService);
                salesOrder.Errors.Count().should_be(0);
            };

            it["create_salesorder_with_no_customerid"] = () =>
            {
                salesOrder = new SalesOrder()
                {
                    SalesDate = DateTime.Now
                };
                _salesOrderService.CreateObject(salesOrder, _customerService);
                salesOrder.Errors.Count().should_not_be(0);
            };

            it["create_salesorder_with_no_elements"] = () =>
            {
                salesOrder = new SalesOrder();
                _salesOrderService.CreateObject(salesOrder, _customerService);
                salesOrder.Errors.Count().should_not_be(0);
            };

            context["validating_salesorder"] = () =>
            {
                before = () =>
                {
                    salesOrder = new SalesOrder
                    {
                        CustomerId = customer.Id,
                        SalesDate = DateTime.Now
                    };
                    salesOrder = _salesOrderService.CreateObject(salesOrder, _customerService);
                };

                it["delete_salesorder"] = () =>
                {
                    salesOrder = _salesOrderService.SoftDeleteObject(salesOrder, _salesOrderDetailService);
                    salesOrder.Errors.Count().should_be(0);
                };

                it["delete_salesorder_and_details"] = () =>
                {
                    salesOrderDetail1 = _salesOrderDetailService.CreateObject(salesOrder.Id, item_batiktulis.Id, 5, 100000, _salesOrderService, _itemService);
                    salesOrderDetail2 = _salesOrderDetailService.CreateObject(salesOrder.Id, item_sepatubola.Id, 12, 850000, _salesOrderService, _itemService);
                    salesOrder = _salesOrderService.SoftDeleteObject(salesOrder, _salesOrderDetailService);
                    salesOrder.Errors.Count().should_be(0);
                };

                it["delete_salesorderdetail"] = () =>
                {
                    salesOrderDetail1 = _salesOrderDetailService.CreateObject(salesOrder.Id, item_batiktulis.Id, 5, 100000, _salesOrderService, _itemService);
                    salesOrderDetail2 = _salesOrderDetailService.CreateObject(salesOrder.Id, item_sepatubola.Id, 12, 850000, _salesOrderService, _itemService);
                    salesOrderDetail2 = _salesOrderDetailService.SoftDeleteObject(salesOrderDetail2);
                    salesOrderDetail2.Errors.Count().should_be(0);
                };

                it["create_salesorderdetails_with_same_item"] = () =>
                {
                    salesOrderDetail1 = _salesOrderDetailService.CreateObject(salesOrder.Id, item_batiktulis.Id, 5, 100000, _salesOrderService, _itemService);
                    salesOrderDetail2 = _salesOrderDetailService.CreateObject(salesOrder.Id, item_batiktulis.Id, 12, 850000, _salesOrderService, _itemService);
                    salesOrderDetail2.Errors.Count().should_not_be(0);
                };

                context["when confirming SO"] = () =>
                {
                    before = () =>
                    {
                        salesOrderDetail1 = _salesOrderDetailService.CreateObject(salesOrder.Id, item_batiktulis.Id, 5, 100000, _salesOrderService, _itemService);
                        salesOrderDetail2 = _salesOrderDetailService.CreateObject(salesOrder.Id, item_sepatubola.Id, 12, 850000, _salesOrderService, _itemService);
                        Quantity1 = item_batiktulis.PendingDelivery;
                        Quantity2 = item_sepatubola.PendingDelivery;
                        salesOrder = _salesOrderService.ConfirmObject(salesOrder, _salesOrderDetailService, _stockMutationService, _itemService);
                    };

                    it["confirmed_salesorder"] = () =>
                    {
                        salesOrder.Errors.Count().should_be(0);
                    };

                    it["finished_details"] = () =>
                    {
                        salesOrderDetail1 = _salesOrderDetailService.FinishObject(salesOrderDetail1, _stockMutationService, _itemService, _barringService, _warehouseItemService);
                        salesOrderDetail2 = _salesOrderDetailService.FinishObject(salesOrderDetail2, _stockMutationService, _itemService, _barringService, _warehouseItemService);
                        IList<StockMutation> stockMutationsDetail1 = _stockMutationService.GetObjectsBySourceDocumentDetailForItem(item_batiktulis.Id, Core.Constants.Constant.SourceDocumentDetailType.SalesOrderDetail, salesOrderDetail1.Id);
                        stockMutationsDetail1.Count().should_be(1);
                        IList<StockMutation> stockMutationsDetail2 = _stockMutationService.GetObjectsBySourceDocumentDetailForItem(item_sepatubola.Id, Core.Constants.Constant.SourceDocumentDetailType.SalesOrderDetail, salesOrderDetail2.Id);
                        stockMutationsDetail2.Count().should_be(1);
                        salesOrderDetail1.IsFinished.should_be(true);
                        salesOrderDetail2.IsFinished.should_be(true);
                    };

                    it["delete_confirmed_salesorder"] = () =>
                    {
                        salesOrder = _salesOrderService.SoftDeleteObject(salesOrder, _salesOrderDetailService);
                        salesOrder.Errors.Count().should_not_be(0);
                    };

                    it["unfinish_salesorderdetail"] = () =>
                    {
                        salesOrderDetail1 = _salesOrderDetailService.FinishObject(salesOrderDetail1, _stockMutationService, _itemService, _barringService, _warehouseItemService);
                        salesOrderDetail2 = _salesOrderDetailService.FinishObject(salesOrderDetail2, _stockMutationService, _itemService, _barringService, _warehouseItemService);
                        salesOrderDetail1 = _salesOrderDetailService.UnfinishObject(salesOrderDetail1, _salesOrderService, _deliveryOrderDetailService, _stockMutationService, _itemService, _barringService, _warehouseItemService);
                        salesOrderDetail2 = _salesOrderDetailService.UnfinishObject(salesOrderDetail2, _salesOrderService, _deliveryOrderDetailService, _stockMutationService, _itemService, _barringService, _warehouseItemService);
                        salesOrderDetail1.IsFinished.should_be(false);
                        salesOrderDetail2.IsFinished.should_be(false);
                        salesOrderDetail1.Errors.Count().should_be(0);
                        salesOrderDetail2.Errors.Count().should_be(0);
                        IList<StockMutation> stockMutationsDetail1 = _stockMutationService.GetObjectsBySourceDocumentDetailForItem(item_batiktulis.Id, Core.Constants.Constant.SourceDocumentDetailType.SalesOrderDetail, salesOrderDetail1.Id);
                        stockMutationsDetail1.Count().should_be(0);
                        IList<StockMutation> stockMutationsDetail2 = _stockMutationService.GetObjectsBySourceDocumentDetailForItem(item_sepatubola.Id, Core.Constants.Constant.SourceDocumentDetailType.SalesOrderDetail, salesOrderDetail2.Id);
                        stockMutationsDetail2.Count().should_be(0);
                    };

                    it["delete_unfinish_salesorderdetail"] = () =>
                    {
                        salesOrderDetail2 = _salesOrderDetailService.FinishObject(salesOrderDetail2, _stockMutationService, _itemService, _barringService, _warehouseItemService);
                        salesOrderDetail2 = _salesOrderDetailService.UnfinishObject(salesOrderDetail2, _salesOrderService, _deliveryOrderDetailService, _stockMutationService, _itemService, _barringService, _warehouseItemService);
                        salesOrderDetail2 = _salesOrderDetailService.SoftDeleteObject(salesOrderDetail2);
                        salesOrderDetail2.Errors.Count().should_be(0);
                        salesOrderDetail2.IsDeleted.should_be(true);
                    };

                    it["should increase pending delivery in item"] = () =>
                    {
                        salesOrderDetail1 = _salesOrderDetailService.FinishObject(salesOrderDetail1, _stockMutationService, _itemService, _barringService, _warehouseItemService);
                        salesOrderDetail2 = _salesOrderDetailService.FinishObject(salesOrderDetail2, _stockMutationService, _itemService, _barringService, _warehouseItemService);
                        Item NewItem1 = _itemService.GetObjectById(item_batiktulis.Id);
                        Item NewItem2 = _itemService.GetObjectById(item_sepatubola.Id);

                        int diff_1 = NewItem1.PendingDelivery - Quantity1;
                        diff_1.should_be(salesOrderDetail1.Quantity);

                        int diff_2 = NewItem2.PendingDelivery - Quantity2;
                        diff_2.should_be(salesOrderDetail2.Quantity);
                    };
                };
            };
        }
    }
}