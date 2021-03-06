using Core.DomainModel;
using Core.Interface.Repository;
using Core.Interface.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Interface.Service
{
    public interface ISalesOrderService
    {
        IQueryable<SalesOrder> GetQueryable();
        ISalesOrderValidator GetValidator();
        IList<SalesOrder> GetAll();
        SalesOrder GetObjectById(int Id);
        IList<SalesOrder> GetObjectsByContactId(int contactId);
        IQueryable<SalesOrder> GetQueryableConfirmedObjects();
        IList<SalesOrder> GetConfirmedObjects();
        SalesOrder CreateObject(SalesOrder salesOrder, IContactService _contactService);
        SalesOrder CreateObject(int contactId, DateTime salesDate, IContactService _contactService);
        SalesOrder UpdateObject(SalesOrder salesOrder, IContactService _contactService);
        SalesOrder SoftDeleteObject(SalesOrder salesOrder, ISalesOrderDetailService _salesOrderDetailService);
        bool DeleteObject(int Id);
        SalesOrder ConfirmObject(SalesOrder salesOrder, DateTime ConfirmationDate, ISalesOrderDetailService _salesOrderDetailService,
                                 IStockMutationService _stockMutationService, IItemService _itemService, IBarringService _barringService,
                                 IWarehouseItemService _warehouseItemService);
        SalesOrder UnconfirmObject(SalesOrder salesOrder, ISalesOrderDetailService _salesOrderDetailService,
                                   IDeliveryOrderService _deliveryOrderService, IDeliveryOrderDetailService _deliveryOrderDetailService,
                                   IStockMutationService _stockMutationService,
                                   IItemService _itemService, IBarringService _barringService, IWarehouseItemService _warehouseItemService);
        SalesOrder CheckAndSetDeliveryComplete(SalesOrder salesOrder, ISalesOrderDetailService _salesOrderDetailService);
        SalesOrder UnsetDeliveryComplete(SalesOrder salesOrder);
    }
}