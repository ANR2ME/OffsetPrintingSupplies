using Core.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Interface.Repository
{
    public interface ISalesInvoiceDetailRepository : IRepository<SalesInvoiceDetail>
    {
        IQueryable<SalesInvoiceDetail> GetQueryable();
        IList<SalesInvoiceDetail> GetAll();
        IList<SalesInvoiceDetail> GetAllByMonthCreated();
        IQueryable<SalesInvoiceDetail> GetQueryableObjectsBySalesInvoiceId(int salesInvoiceId);
        IList<SalesInvoiceDetail> GetObjectsBySalesInvoiceId(int salesInvoiceId);
        IQueryable<SalesInvoiceDetail> GetQueryableObjectsByDeliveryOrderDetailId(int deliveryOrderDetailId);
        IList<SalesInvoiceDetail> GetObjectsByDeliveryOrderDetailId(int deliveryOrderDetailId);
        SalesInvoiceDetail GetObjectById(int Id);
        SalesInvoiceDetail CreateObject(SalesInvoiceDetail salesInvoiceDetail);
        SalesInvoiceDetail UpdateObject(SalesInvoiceDetail salesInvoiceDetail);
        SalesInvoiceDetail SoftDeleteObject(SalesInvoiceDetail salesInvoiceDetail);
        bool DeleteObject(int Id);
        SalesInvoiceDetail ConfirmObject(SalesInvoiceDetail salesInvoiceDetail);
        SalesInvoiceDetail UnconfirmObject(SalesInvoiceDetail salesInvoiceDetail);
        string SetObjectCode(string ParentCode);
    }
}