using Core.DomainModel;
using Core.Interface.Repository;
using Core.Interface.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Interface.Service
{
    public interface ISalesOrderDetailService
    {
        IQueryable<SalesOrderDetail> GetQueryable();
        IList<SalesOrderDetail> GetAll();
        ISalesOrderDetailValidator GetValidator();
        IQueryable<SalesOrderDetail> GetQueryableObjectsBySalesOrderId(int salesOrderId);
        IList<SalesOrderDetail> GetObjectsBySalesOrderId(int salesOrderId);
        IList<SalesOrderDetail> GetObjectsByItemId(int itemId);
        SalesOrderDetail GetObjectById(int Id);
        SalesOrderDetail CreateObject(SalesOrderDetail salesOrderDetail, ISalesOrderService _salesOrderService, IItemService _itemService);
        SalesOrderDetail CreateObject(int salesOrderId, int itemId, int quantity, decimal Price, ISalesOrderService _salesOrderService, IItemService _itemService);
        SalesOrderDetail UpdateObject(SalesOrderDetail salesOrderDetail, ISalesOrderService _salesOrderService, IItemService _itemService);
        SalesOrderDetail SoftDeleteObject(SalesOrderDetail salesOrderDetail);
        bool DeleteObject(int Id);
        SalesOrderDetail ConfirmObject(SalesOrderDetail salesOrderDetail, DateTime ConfirmationDate, IStockMutationService _stockMutationService,
                                      IItemService _itemService, IBarringService _barringService, IWarehouseItemService _warehouseItemService);
        SalesOrderDetail UnconfirmObject(SalesOrderDetail salesOrderDetail, ISalesOrderService _salesOrderService, IDeliveryOrderDetailService _deliveryOrderDetailService,
                                        IStockMutationService _stockMutationService, IItemService _itemService, IBarringService _barringService, IWarehouseItemService _warehouseItemService);
        SalesOrderDetail SetDeliveryComplete(SalesOrderDetail salesOrderDetail, int Quantity);
        SalesOrderDetail UnsetDeliveryComplete(SalesOrderDetail salesOrderDetail, int Quantity, ISalesOrderService _salesOrderService);
    }
}